// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Threading;

namespace System.Reflection
{
    internal partial struct TypeNameResolver
    {
        private Func<AssemblyName, Assembly?>? _assemblyResolver;
        private Func<Assembly?, string, bool, Type?>? _typeResolver;
        private bool _throwOnError;
        private bool _ignoreCase;
        private bool _extensibleParser;
        private bool _requireAssemblyQualifiedName;
        private bool _suppressContextualReflectionContext;
        private IntPtr _unsafeAccessorMethod;
        private Assembly? _requestingAssembly;
        private Assembly? _topLevelAssembly;

        private bool SupportsUnboundGenerics { get => _unsafeAccessorMethod != IntPtr.Zero; }

        [RequiresUnreferencedCode("The type might be removed")]
        internal static Type? GetType(
            string typeName,
            Assembly requestingAssembly,
            bool throwOnError = false,
            bool ignoreCase = false)
        {
            return GetType(typeName, assemblyResolver: null, typeResolver: null, requestingAssembly: requestingAssembly,
                throwOnError: throwOnError, ignoreCase: ignoreCase, extensibleParser: false);
        }

        [RequiresUnreferencedCode("The type might be removed")]
        internal static Type? GetType(
            string typeName,
            Func<AssemblyName, Assembly?>? assemblyResolver,
            Func<Assembly?, string, bool, Type?>? typeResolver,
            Assembly? requestingAssembly,
            bool throwOnError = false,
            bool ignoreCase = false,
            bool extensibleParser = true)
        {
            ArgumentNullException.ThrowIfNull(typeName);

            // Compat: Empty name throws TypeLoadException instead of
            // the natural ArgumentException
            if (typeName.Length == 0)
            {
                if (throwOnError)
                    throw new TypeLoadException(SR.Arg_TypeLoadNullStr);
                return null;
            }

            TypeName? parsed = TypeNameParser.Parse(typeName, throwOnError);
            if (parsed is null)
            {
                return null;
            }

            return new TypeNameResolver()
            {
                _assemblyResolver = assemblyResolver,
                _typeResolver = typeResolver,
                _throwOnError = throwOnError,
                _ignoreCase = ignoreCase,
                _extensibleParser = extensibleParser,
                _requestingAssembly = requestingAssembly
            }.Resolve(parsed);
        }

        [RequiresUnreferencedCode("The type might be removed")]
        internal static Type? GetType(
            string typeName,
            bool throwOnError,
            bool ignoreCase,
            Assembly topLevelAssembly)
        {
            TypeName? parsed = TypeNameParser.Parse(typeName, throwOnError, new() { IsAssemblyGetType = true });

            if (parsed is null)
            {
                return null;
            }
            else if (parsed.AssemblyName is not null)
            {
                return throwOnError ? throw new ArgumentException(SR.Argument_AssemblyGetTypeCannotSpecifyAssembly) : null;
            }

            return new TypeNameResolver()
            {
                _throwOnError = throwOnError,
                _ignoreCase = ignoreCase,
                _topLevelAssembly = topLevelAssembly,
                _requestingAssembly = topLevelAssembly
            }.Resolve(parsed);
        }

        // Resolve type name referenced by a custom attribute metadata.
        // It uses the standard Type.GetType(typeName, throwOnError: true) algorithm with the following modifications:
        // - ContextualReflectionContext is not taken into account
        // - The dependency between the returned type and the requesting assembly is recorded for the purpose of
        // lifetime tracking of collectible types.
        internal static RuntimeType GetTypeReferencedByCustomAttribute(string typeName, RuntimeModule scope)
        {
            ArgumentException.ThrowIfNullOrEmpty(typeName);

            RuntimeAssembly requestingAssembly = scope.GetRuntimeAssembly();

            TypeName parsed = TypeName.Parse(typeName);
            RuntimeType? type = (RuntimeType?)new TypeNameResolver()
            {
                _throwOnError = true,
                _suppressContextualReflectionContext = true,
                _requestingAssembly = requestingAssembly
            }.Resolve(parsed);

            Debug.Assert(type != null);

            RuntimeTypeHandle.RegisterCollectibleTypeDependency(type, requestingAssembly);

            return type;
        }

        // Used by VM
        internal static unsafe RuntimeType? GetTypeHelper(char* pTypeName, RuntimeAssembly? requestingAssembly,
            bool throwOnError, bool requireAssemblyQualifiedName, IntPtr unsafeAccessorMethod)
        {
            ReadOnlySpan<char> typeName = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(pTypeName);
            return GetTypeHelper(typeName, requestingAssembly, throwOnError, requireAssemblyQualifiedName, unsafeAccessorMethod);
        }

        internal static RuntimeType? GetTypeHelper(ReadOnlySpan<char> typeName, RuntimeAssembly? requestingAssembly,
            bool throwOnError, bool requireAssemblyQualifiedName, IntPtr unsafeAccessorMethod = 0)
        {
            // Compat: Empty name throws TypeLoadException instead of
            // the natural ArgumentException
            if (typeName.Length == 0)
            {
                if (throwOnError)
                    throw new TypeLoadException(SR.Arg_TypeLoadNullStr);
                return null;
            }

            TypeName? parsed = TypeNameParser.Parse(typeName, throwOnError);
            if (parsed is null)
            {
                return null;
            }

            RuntimeType? type = (RuntimeType?)new TypeNameResolver()
            {
                _requestingAssembly = requestingAssembly,
                _throwOnError = throwOnError,
                _suppressContextualReflectionContext = true,
                _requireAssemblyQualifiedName = requireAssemblyQualifiedName,
                _unsafeAccessorMethod = unsafeAccessorMethod,
            }.Resolve(parsed);

            if (type != null)
                RuntimeTypeHandle.RegisterCollectibleTypeDependency(type, requestingAssembly);

            return type;
        }

        private Assembly? ResolveAssembly(AssemblyName assemblyName)
        {
            Assembly? assembly;
            if (_assemblyResolver is not null)
            {
                assembly = _assemblyResolver(assemblyName);
                if (assembly is null && _throwOnError)
                {
                    throw new FileNotFoundException(SR.Format(SR.FileNotFound_ResolveAssembly, assemblyName));
                }
            }
            else
            {
                assembly = RuntimeAssembly.InternalLoad(assemblyName, ref Unsafe.NullRef<StackCrawlMark>(),
                    _suppressContextualReflectionContext ? null : AssemblyLoadContext.CurrentContextualReflectionContext,
                    requestingAssembly: (RuntimeAssembly?)_requestingAssembly, throwOnFileNotFound: _throwOnError);
            }
            return assembly;
        }

        [LibraryImport(RuntimeHelpers.QCall, EntryPoint = "UnsafeAccessors_ResolveGenericParamToTypeHandle")]
        private static partial IntPtr ResolveGenericParamToTypeHandle(IntPtr unsafeAccessorMethod, [MarshalAs(UnmanagedType.Bool)] bool isMethodParam, uint paramIndex);

        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026:RequiresUnreferencedCode",
            Justification = "TypeNameResolver.GetType is marked as RequiresUnreferencedCode.")]
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2075:UnrecognizedReflectionPattern",
            Justification = "TypeNameResolver.GetType is marked as RequiresUnreferencedCode.")]
        private Type? GetType(string escapedTypeName, ReadOnlySpan<string> nestedTypeNames, TypeName parsedName)
        {
            Assembly? assembly;

            if (parsedName.AssemblyName is not null)
            {
                assembly = ResolveAssembly(parsedName.AssemblyName.ToAssemblyName());
                if (assembly is null)
                    return null;
            }
            else
            {
                assembly = _topLevelAssembly;
            }

            Type? type;

            // Resolve the top level type.
            if (_typeResolver is not null)
            {
                type = _typeResolver(assembly, escapedTypeName, _ignoreCase);

                if (type is null)
                {
                    if (_throwOnError)
                    {
                        throw new TypeLoadException(assembly is null ?
                            SR.Format(SR.TypeLoad_ResolveType, escapedTypeName) :
                            SR.Format(SR.TypeLoad_ResolveTypeFromAssembly, escapedTypeName, assembly.FullName),
                            typeName: escapedTypeName);
                    }
                    return null;
                }
            }
            else
            {
                if (assembly is null)
                {
                    if (SupportsUnboundGenerics
                        && !string.IsNullOrEmpty(escapedTypeName)
                        && escapedTypeName[0] == '!')
                    {
                        Debug.Assert(_throwOnError); // Unbound generic support currently always throws.

                        // Parse the type as an unbound generic parameter. Following the common VAR/MVAR IL syntax:
                        //  !<Number> - Represents a zero-based index into the type's generic parameters.
                        //  !!<Number> - Represents a zero-based index into the method's generic parameters.

                        // Confirm we have at least one more character
                        if (escapedTypeName.Length == 1)
                        {
                            throw new TypeLoadException(SR.Format(SR.TypeLoad_ResolveType, escapedTypeName), typeName: escapedTypeName);
                        }

                        // At this point we expect either another '!' and then a number or a number.
                        bool isMethodParam = escapedTypeName[1] == '!';
                        ReadOnlySpan<char> toParse = isMethodParam
                            ? escapedTypeName.AsSpan(2)  // Skip over "!!"
                            : escapedTypeName.AsSpan(1); // Skip over "!"
                        if (!uint.TryParse(toParse, NumberStyles.None, null, out uint paramIndex))
                        {
                            throw new TypeLoadException(SR.Format(SR.TypeLoad_ResolveType, escapedTypeName), typeName: escapedTypeName);
                        }

                        Debug.Assert(_unsafeAccessorMethod != IntPtr.Zero);
                        IntPtr typeHandle = ResolveGenericParamToTypeHandle(_unsafeAccessorMethod, isMethodParam, paramIndex);
                        if (typeHandle == IntPtr.Zero)
                        {
                            throw new TypeLoadException(SR.Format(SR.TypeLoad_ResolveType, escapedTypeName), typeName: escapedTypeName);
                        }

                        return RuntimeTypeHandle.GetRuntimeTypeFromHandle(typeHandle);
                    }

                    if (_requireAssemblyQualifiedName)
                    {
                        if (_throwOnError)
                        {
                            throw new TypeLoadException(SR.Format(SR.TypeLoad_ResolveType, escapedTypeName), typeName: escapedTypeName);
                        }
                        return null;
                    }
                    return GetTypeFromDefaultAssemblies(TypeName.Unescape(escapedTypeName), nestedTypeNames, parsedName);
                }

                if (assembly is RuntimeAssembly runtimeAssembly)
                {
                    // Compat: Non-extensible parser allows ambiguous matches with ignore case lookup
                    bool useReflectionForNestedTypes = _extensibleParser && _ignoreCase;

                    type = runtimeAssembly.GetTypeCore(TypeName.Unescape(escapedTypeName), useReflectionForNestedTypes ? default : nestedTypeNames,
                        throwOnFileNotFound: _throwOnError, ignoreCase: _ignoreCase);

                    if (type is null)
                    {
                        if (_throwOnError)
                        {
                            throw new TypeLoadException(SR.Format(SR.TypeLoad_ResolveTypeFromAssembly, parsedName.FullName, runtimeAssembly.FullName),
                                typeName: parsedName.FullName);
                        }
                        return null;
                    }

                    if (!useReflectionForNestedTypes)
                    {
                        return type;
                    }
                }
                else
                {
                    // This is a third-party Assembly object. Emulate GetTypeCore() by calling the public GetType()
                    // method. This is wasteful because it'll probably reparse a type string that we've already parsed
                    // but it can't be helped.
                    type = assembly.GetType(escapedTypeName, throwOnError: _throwOnError, ignoreCase: _ignoreCase);
                }

                if (type is null)
                    return null;
            }

            for (int i = 0; i < nestedTypeNames.Length; i++)
            {
                BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Public;
                if (_ignoreCase)
                    bindingFlags |= BindingFlags.IgnoreCase;

                type = type.GetNestedType(nestedTypeNames[i], bindingFlags);

                if (type is null)
                {
                    if (_throwOnError)
                    {
                        throw new TypeLoadException(SR.Format(SR.TypeLoad_ResolveNestedType,
                            nestedTypeNames[i], (i > 0) ? nestedTypeNames[i - 1] : TypeName.Unescape(escapedTypeName)),
                            typeName: parsedName.FullName);
                    }
                    return null;
                }
            }

            return type;
        }

        private Type? GetTypeFromDefaultAssemblies(string typeName, ReadOnlySpan<string> nestedTypeNames, TypeName parsedName)
        {
            RuntimeAssembly? requestingAssembly = (RuntimeAssembly?)_requestingAssembly;
            if (requestingAssembly is not null)
            {
                Type? type = requestingAssembly.GetTypeCore(typeName, nestedTypeNames, throwOnFileNotFound: false, ignoreCase: _ignoreCase);
                if (type is not null)
                    return type;
            }

            RuntimeAssembly coreLib = (RuntimeAssembly)typeof(object).Assembly;
            if (requestingAssembly != coreLib)
            {
                Type? type = coreLib.GetTypeCore(typeName, nestedTypeNames, throwOnFileNotFound: false, ignoreCase: _ignoreCase);
                if (type is not null)
                    return type;
            }

            RuntimeAssembly? resolvedAssembly = AssemblyLoadContext.OnTypeResolve(requestingAssembly, parsedName.FullName);
            if (resolvedAssembly is not null)
            {
                Type? type = resolvedAssembly.GetTypeCore(typeName, nestedTypeNames, throwOnFileNotFound: false, ignoreCase: _ignoreCase);
                if (type is not null)
                    return type;
            }

            if (_throwOnError)
            {
                throw new TypeLoadException(SR.Format(SR.TypeLoad_ResolveTypeFromAssembly, parsedName.FullName, (requestingAssembly ?? coreLib).FullName),
                    typeName: parsedName.FullName);
            }

            return null;
        }
    }
}
