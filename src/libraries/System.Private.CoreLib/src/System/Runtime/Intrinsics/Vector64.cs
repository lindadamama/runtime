// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Runtime.Intrinsics
{
    /// <summary>Provides a collection of static methods for creating, manipulating, and otherwise operating on 64-bit vectors.</summary>
    public static class Vector64
    {
        internal const int Size = 8;

        internal const int Alignment = 8;

        /// <summary>Gets a value that indicates whether 64-bit vector operations are subject to hardware acceleration through JIT intrinsic support.</summary>
        /// <value><see langword="true" /> if 64-bit vector operations are subject to hardware acceleration; otherwise, <see langword="false" />.</value>
        /// <remarks>64-bit vector operations are subject to hardware acceleration on systems that support Single Instruction, Multiple Data (SIMD) instructions for 64-bit vectors and the RyuJIT just-in-time compiler is used to compile managed code.</remarks>
        public static bool IsHardwareAccelerated
        {
            [Intrinsic]
            get => IsHardwareAccelerated;
        }

        /// <summary>Computes the absolute value of each element in a vector.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector that will have its absolute value computed.</param>
        /// <returns>A vector whose elements are the absolute value of the elements in <paramref name="vector" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> Abs<T>(Vector64<T> vector)
        {
            if ((typeof(T) == typeof(byte))
             || (typeof(T) == typeof(ushort))
             || (typeof(T) == typeof(uint))
             || (typeof(T) == typeof(ulong))
             || (typeof(T) == typeof(nuint)))
            {
                return vector;
            }
            else
            {
                Unsafe.SkipInit(out Vector64<T> result);

                for (int index = 0; index < Vector64<T>.Count; index++)
                {
                    T value = Scalar<T>.Abs(vector.GetElementUnsafe(index));
                    result.SetElementUnsafe(index, value);
                }

                return result;
            }
        }

        /// <inheritdoc cref="Vector128.Add{T}(Vector128{T}, Vector128{T})" />
        [Intrinsic]
        public static Vector64<T> Add<T>(Vector64<T> left, Vector64<T> right) => left + right;

        /// <inheritdoc cref="Vector128.AddSaturate{T}(Vector128{T}, Vector128{T})" />
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> AddSaturate<T>(Vector64<T> left, Vector64<T> right)
        {
            if ((typeof(T) == typeof(float)) || (typeof(T) == typeof(double)))
            {
                return left + right;
            }
            else
            {
                Unsafe.SkipInit(out Vector64<T> result);

                for (int index = 0; index < Vector64<T>.Count; index++)
                {
                    T value = Scalar<T>.AddSaturate(left.GetElementUnsafe(index), right.GetElementUnsafe(index));
                    result.SetElementUnsafe(index, value);
                }

                return result;
            }
        }

        /// <summary>Determines if all elements of a vector are equal to a given value.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector whose elements are being checked.</param>
        /// <param name="value">The value to check for in <paramref name="vector" /></param>
        /// <returns><c>true</c> if all elements of <paramref name="vector" /> are equal to <paramref name="value" />; otherwise, <c>false</c>.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> and <paramref name="value" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool All<T>(Vector64<T> vector, T value) => vector == Create(value);

        /// <summary>Determines if all elements of a vector have all their bits set.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector whose elements are being checked.</param>
        /// <returns><c>true</c> if all elements of <paramref name="vector" /> have all their bits set; otherwise, <c>false</c>.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" />(<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AllWhereAllBitsSet<T>(Vector64<T> vector)
        {
            if (typeof(T) == typeof(float))
            {
                return All(vector.AsInt32(), -1);
            }
            else if (typeof(T) == typeof(double))
            {
                return All(vector.AsInt64(), -1);
            }
            else
            {
                return All(vector, Scalar<T>.AllBitsSet);
            }
        }

        /// <summary>Computes the bitwise-and of a given vector and the ones complement of another vector.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="left">The vector to bitwise-and with <paramref name="right" />.</param>
        /// <param name="right">The vector to that is ones-complemented before being bitwise-and with <paramref name="left" />.</param>
        /// <returns>The bitwise-and of <paramref name="left" /> and the ones-complement of <paramref name="right" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="left" /> and <paramref name="right" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        public static Vector64<T> AndNot<T>(Vector64<T> left, Vector64<T> right) => left & ~right;

        /// <summary>Determines if any elements of a vector are equal to a given value.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector whose elements are being checked.</param>
        /// <param name="value">The value to check for in <paramref name="vector" /></param>
        /// <returns><c>true</c> if any elements of <paramref name="vector" /> are equal to <paramref name="value" />; otherwise, <c>false</c>.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> and <paramref name="value" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Any<T>(Vector64<T> vector, T value) => EqualsAny(vector, Create(value));

        /// <summary>Determines if any elements of a vector have all their bits set.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector whose elements are being checked.</param>
        /// <returns><c>true</c> if any elements of <paramref name="vector" /> have all their bits set; otherwise, <c>false</c>.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" />(<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AnyWhereAllBitsSet<T>(Vector64<T> vector)
        {
            if (typeof(T) == typeof(float))
            {
                return Any(vector.AsInt32(), -1);
            }
            else if (typeof(T) == typeof(double))
            {
                return Any(vector.AsInt64(), -1);
            }
            else
            {
                return Any(vector, Scalar<T>.AllBitsSet);
            }
        }

        /// <summary>Reinterprets a <see langword="Vector64&lt;TFrom&gt;" /> as a new <see langword="Vector64&lt;TTo&gt;" />.</summary>
        /// <typeparam name="TFrom">The type of the elements in the input vector.</typeparam>
        /// <typeparam name="TTo">The type of the elements in the output vector.</typeparam>
        /// <param name="vector">The vector to reinterpret.</param>
        /// <returns><paramref name="vector" /> reinterpreted as a new <see langword="Vector64&lt;TTo&gt;" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> (<typeparamref name="TFrom" />) or the type of the target (<typeparamref name="TTo" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<TTo> As<TFrom, TTo>(this Vector64<TFrom> vector)
        {
            ThrowHelper.ThrowForUnsupportedIntrinsicsVector64BaseType<TFrom>();
            ThrowHelper.ThrowForUnsupportedIntrinsicsVector64BaseType<TTo>();

            return Unsafe.BitCast<Vector64<TFrom>, Vector64<TTo>>(vector);
        }

        /// <summary>Reinterprets a <see cref="Vector64{T}" /> as a new <see langword="Vector64&lt;Byte&gt;" />.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector to reinterpret.</param>
        /// <returns><paramref name="vector" /> reinterpreted as a new <see langword="Vector64&lt;Byte&gt;" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        public static Vector64<byte> AsByte<T>(this Vector64<T> vector) => vector.As<T, byte>();

        /// <summary>Reinterprets a <see cref="Vector64{T}" /> as a new <see langword="Vector64&lt;Double&gt;" />.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector to reinterpret.</param>
        /// <returns><paramref name="vector" /> reinterpreted as a new <see langword="Vector64&lt;Double&gt;" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        public static Vector64<double> AsDouble<T>(this Vector64<T> vector) => vector.As<T, double>();

        /// <summary>Reinterprets a <see cref="Vector64{T}" /> as a new <see langword="Vector64&lt;Int16&gt;" />.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector to reinterpret.</param>
        /// <returns><paramref name="vector" /> reinterpreted as a new <see langword="Vector64&lt;Int16&gt;" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        public static Vector64<short> AsInt16<T>(this Vector64<T> vector) => vector.As<T, short>();

        /// <summary>Reinterprets a <see cref="Vector64{T}" /> as a new <see langword="Vector64&lt;Int32&gt;" />.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector to reinterpret.</param>
        /// <returns><paramref name="vector" /> reinterpreted as a new <see langword="Vector64&lt;Int32&gt;" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        public static Vector64<int> AsInt32<T>(this Vector64<T> vector) => vector.As<T, int>();

        /// <summary>Reinterprets a <see cref="Vector64{T}" /> as a new <see langword="Vector64&lt;Int64&gt;" />.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector to reinterpret.</param>
        /// <returns><paramref name="vector" /> reinterpreted as a new <see langword="Vector64&lt;Int64&gt;" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        public static Vector64<long> AsInt64<T>(this Vector64<T> vector) => vector.As<T, long>();

        /// <summary>Reinterprets a <see cref="Vector64{T}" /> as a new <see langword="Vector64&lt;IntPtr&gt;" />.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector to reinterpret.</param>
        /// <returns><paramref name="vector" /> reinterpreted as a new <see langword="Vector64&lt;IntPtr&gt;" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        public static Vector64<nint> AsNInt<T>(this Vector64<T> vector) => vector.As<T, nint>();

        /// <summary>Reinterprets a <see cref="Vector64{T}" /> as a new <see langword="Vector64&lt;UIntPtr&gt;" />.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector to reinterpret.</param>
        /// <returns><paramref name="vector" /> reinterpreted as a new <see langword="Vector64&lt;UIntPtr&gt;" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [CLSCompliant(false)]
        public static Vector64<nuint> AsNUInt<T>(this Vector64<T> vector) => vector.As<T, nuint>();

        /// <summary>Reinterprets a <see cref="Vector64{T}" /> as a new <see langword="Vector64&lt;SByte&gt;" />.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector to reinterpret.</param>
        /// <returns><paramref name="vector" /> reinterpreted as a new <see langword="Vector64&lt;SByte&gt;" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [CLSCompliant(false)]
        public static Vector64<sbyte> AsSByte<T>(this Vector64<T> vector) => vector.As<T, sbyte>();

        /// <summary>Reinterprets a <see cref="Vector64{T}" /> as a new <see langword="Vector64&lt;Single&gt;" />.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector to reinterpret.</param>
        /// <returns><paramref name="vector" /> reinterpreted as a new <see langword="Vector64&lt;Single&gt;" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        public static Vector64<float> AsSingle<T>(this Vector64<T> vector) => vector.As<T, float>();

        /// <summary>Reinterprets a <see cref="Vector64{T}" /> as a new <see langword="Vector64&lt;UInt16&gt;" />.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector to reinterpret.</param>
        /// <returns><paramref name="vector" /> reinterpreted as a new <see langword="Vector64&lt;UInt16&gt;" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [CLSCompliant(false)]
        public static Vector64<ushort> AsUInt16<T>(this Vector64<T> vector) => vector.As<T, ushort>();

        /// <summary>Reinterprets a <see cref="Vector64{T}" /> as a new <see langword="Vector64&lt;UInt32&gt;" />.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector to reinterpret.</param>
        /// <returns><paramref name="vector" /> reinterpreted as a new <see langword="Vector64&lt;UInt32&gt;" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [CLSCompliant(false)]
        public static Vector64<uint> AsUInt32<T>(this Vector64<T> vector) => vector.As<T, uint>();

        /// <summary>Reinterprets a <see cref="Vector64{T}" /> as a new <see langword="Vector64&lt;UInt64&gt;" />.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector to reinterpret.</param>
        /// <returns><paramref name="vector" /> reinterpreted as a new <see langword="Vector64&lt;UInt64&gt;" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [CLSCompliant(false)]
        public static Vector64<ulong> AsUInt64<T>(this Vector64<T> vector) => vector.As<T, ulong>();

        /// <summary>Computes the bitwise-and of two vectors.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="left">The vector to bitwise-and with <paramref name="right" />.</param>
        /// <param name="right">The vector to bitwise-and with <paramref name="left" />.</param>
        /// <returns>The bitwise-and of <paramref name="left" /> and <paramref name="right" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="left" /> and <paramref name="right" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        public static Vector64<T> BitwiseAnd<T>(Vector64<T> left, Vector64<T> right) => left & right;

        /// <summary>Computes the bitwise-or of two vectors.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="left">The vector to bitwise-or with <paramref name="right" />.</param>
        /// <param name="right">The vector to bitwise-or with <paramref name="left" />.</param>
        /// <returns>The bitwise-or of <paramref name="left" /> and <paramref name="right" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="left" /> and <paramref name="right" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        public static Vector64<T> BitwiseOr<T>(Vector64<T> left, Vector64<T> right) => left | right;

        /// <summary>Computes the ceiling of each element in a vector.</summary>
        /// <param name="vector">The vector that will have its ceiling computed.</param>
        /// <returns>A vector whose elements are the ceiling of the elements in <paramref name="vector" />.</returns>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector64<T> Ceiling<T>(Vector64<T> vector)
        {
            if ((typeof(T) == typeof(byte))
             || (typeof(T) == typeof(short))
             || (typeof(T) == typeof(int))
             || (typeof(T) == typeof(long))
             || (typeof(T) == typeof(nint))
             || (typeof(T) == typeof(nuint))
             || (typeof(T) == typeof(sbyte))
             || (typeof(T) == typeof(ushort))
             || (typeof(T) == typeof(uint))
             || (typeof(T) == typeof(ulong)))
            {
                return vector;
            }
            else
            {
                Unsafe.SkipInit(out Vector64<T> result);

                for (int index = 0; index < Vector64<T>.Count; index++)
                {
                    T value = Scalar<T>.Ceiling(vector.GetElementUnsafe(index));
                    result.SetElementUnsafe(index, value);
                }

                return result;
            }
        }

        /// <summary>Computes the ceiling of each element in a vector.</summary>
        /// <param name="vector">The vector that will have its ceiling computed.</param>
        /// <returns>A vector whose elements are the ceiling of the elements in <paramref name="vector" />.</returns>
        /// <seealso cref="MathF.Ceiling(float)" />
        [Intrinsic]
        public static Vector64<float> Ceiling(Vector64<float> vector) => Ceiling<float>(vector);

        /// <summary>Computes the ceiling of each element in a vector.</summary>
        /// <param name="vector">The vector that will have its ceiling computed.</param>
        /// <returns>A vector whose elements are the ceiling of the elements in <paramref name="vector" />.</returns>
        /// <seealso cref="Math.Ceiling(double)" />
        [Intrinsic]
        public static Vector64<double> Ceiling(Vector64<double> vector) => Ceiling<double>(vector);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.Clamp(TSelf, TSelf, TSelf)" />
        [Intrinsic]
        public static Vector64<T> Clamp<T>(Vector64<T> value, Vector64<T> min, Vector64<T> max)
        {
            // We must follow HLSL behavior in the case user specified min value is bigger than max value.
            return Min(Max(value, min), max);
        }

        /// <inheritdoc cref="ISimdVector{TSelf, T}.ClampNative(TSelf, TSelf, TSelf)" />
        [Intrinsic]
        public static Vector64<T> ClampNative<T>(Vector64<T> value, Vector64<T> min, Vector64<T> max)
        {
            // We must follow HLSL behavior in the case user specified min value is bigger than max value.
            return MinNative(MaxNative(value, min), max);
        }

        /// <summary>Conditionally selects a value from two vectors on a bitwise basis.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="condition">The mask that is used to select a value from <paramref name="left" /> or <paramref name="right" />.</param>
        /// <param name="left">The vector that is selected when the corresponding bit in <paramref name="condition" /> is one.</param>
        /// <param name="right">The vector that is selected when the corresponding bit in <paramref name="condition" /> is zero.</param>
        /// <returns>A vector whose bits come from <paramref name="left" /> or <paramref name="right" /> based on the value of <paramref name="condition" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="condition" />, <paramref name="left" />, and <paramref name="right" /> (<typeparamref name="T" />) is not supported.</exception>
        /// <remarks>The returned vector is equivalent to <paramref name="condition" /> <c>?</c> <paramref name="left" /> <c>:</c> <paramref name="right" /> on a per-bit basis.</remarks>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> ConditionalSelect<T>(Vector64<T> condition, Vector64<T> left, Vector64<T> right) => (left & condition) | AndNot(right, condition);

        /// <summary>Converts a <see langword="Vector64&lt;Int64&gt;" /> to a <see langword="Vector64&lt;Double&gt;" />.</summary>
        /// <param name="vector">The vector to convert.</param>
        /// <returns>The converted vector.</returns>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<double> ConvertToDouble(Vector64<long> vector)
        {
            Unsafe.SkipInit(out Vector64<double> result);

            for (int i = 0; i < Vector64<double>.Count; i++)
            {
                double value = vector.GetElementUnsafe(i);
                result.SetElementUnsafe(i, value);
            }

            return result;
        }

        /// <summary>Converts a <see langword="Vector64&lt;UInt64&gt;" /> to a <see langword="Vector64&lt;Double&gt;" />.</summary>
        /// <param name="vector">The vector to convert.</param>
        /// <returns>The converted vector.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<double> ConvertToDouble(Vector64<ulong> vector)
        {
            Unsafe.SkipInit(out Vector64<double> result);

            for (int i = 0; i < Vector64<double>.Count; i++)
            {
                double value = vector.GetElementUnsafe(i);
                result.SetElementUnsafe(i, value);
            }

            return result;
        }

        /// <summary>Converts a <see langword="Vector64&lt;Single&gt;" /> to a <see langword="Vector64&lt;Int32&gt;" /> using saturation on overflow.</summary>
        /// <param name="vector">The vector to convert.</param>
        /// <returns>The converted vector.</returns>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<int> ConvertToInt32(Vector64<float> vector)
        {
            Unsafe.SkipInit(out Vector64<int> result);

            for (int i = 0; i < Vector64<int>.Count; i++)
            {
                int value = float.ConvertToInteger<int>(vector.GetElementUnsafe(i));
                result.SetElementUnsafe(i, value);
            }

            return result;
        }

        /// <summary>Converts a <see langword="Vector64&lt;Single&gt;" /> to a <see langword="Vector64&lt;Int32&gt;" /> using platform specific behavior on overflow.</summary>
        /// <param name="vector">The vector to convert.</param>
        /// <returns>The converted vector.</returns>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<int> ConvertToInt32Native(Vector64<float> vector)
        {
            Unsafe.SkipInit(out Vector64<int> result);

            for (int i = 0; i < Vector64<int>.Count; i++)
            {
                int value = float.ConvertToIntegerNative<int>(vector.GetElementUnsafe(i));
                result.SetElementUnsafe(i, value);
            }

            return result;
        }

        /// <summary>Converts a <see langword="Vector64&lt;Double&gt;" /> to a <see langword="Vector64&lt;Int64&gt;" /> using saturation on overflow.</summary>
        /// <param name="vector">The vector to convert.</param>
        /// <returns>The converted vector.</returns>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<long> ConvertToInt64(Vector64<double> vector)
        {
            Unsafe.SkipInit(out Vector64<long> result);

            for (int i = 0; i < Vector64<long>.Count; i++)
            {
                long value = double.ConvertToInteger<long>(vector.GetElementUnsafe(i));
                result.SetElementUnsafe(i, value);
            }

            return result;
        }

        /// <summary>Converts a <see langword="Vector64&lt;Double&gt;" /> to a <see langword="Vector64&lt;Int64&gt;" /> using platform specific behavior on overflow.</summary>
        /// <param name="vector">The vector to convert.</param>
        /// <returns>The converted vector.</returns>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<long> ConvertToInt64Native(Vector64<double> vector)
        {
            Unsafe.SkipInit(out Vector64<long> result);

            for (int i = 0; i < Vector64<long>.Count; i++)
            {
                long value = double.ConvertToIntegerNative<long>(vector.GetElementUnsafe(i));
                result.SetElementUnsafe(i, value);
            }

            return result;
        }

        /// <summary>Converts a <see langword="Vector64&lt;Int32&gt;" /> to a <see langword="Vector64&lt;Single&gt;" />.</summary>
        /// <param name="vector">The vector to convert.</param>
        /// <returns>The converted vector.</returns>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<float> ConvertToSingle(Vector64<int> vector)
        {
            Unsafe.SkipInit(out Vector64<float> result);

            for (int i = 0; i < Vector64<float>.Count; i++)
            {
                float value = vector.GetElementUnsafe(i);
                result.SetElementUnsafe(i, value);
            }

            return result;
        }

        /// <summary>Converts a <see langword="Vector64&lt;UInt32&gt;" /> to a <see langword="Vector64&lt;Single&gt;" />.</summary>
        /// <param name="vector">The vector to convert.</param>
        /// <returns>The converted vector.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<float> ConvertToSingle(Vector64<uint> vector)
        {
            Unsafe.SkipInit(out Vector64<float> result);

            for (int i = 0; i < Vector64<float>.Count; i++)
            {
                float value = vector.GetElementUnsafe(i);
                result.SetElementUnsafe(i, value);
            }

            return result;
        }

        /// <summary>Converts a <see langword="Vector64&lt;Single&gt;" /> to a <see langword="Vector64&lt;UInt32&gt;" /> using saturation on overflow.</summary>
        /// <param name="vector">The vector to convert.</param>
        /// <returns>The converted vector.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<uint> ConvertToUInt32(Vector64<float> vector)
        {
            Unsafe.SkipInit(out Vector64<uint> result);

            for (int i = 0; i < Vector64<uint>.Count; i++)
            {
                uint value = float.ConvertToInteger<uint>(vector.GetElementUnsafe(i));
                result.SetElementUnsafe(i, value);
            }

            return result;
        }

        /// <summary>Converts a <see langword="Vector64&lt;Single&gt;" /> to a <see langword="Vector64&lt;UInt32&gt;" /> using platform specific behavior on overflow.</summary>
        /// <param name="vector">The vector to convert.</param>
        /// <returns>The converted vector.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<uint> ConvertToUInt32Native(Vector64<float> vector)
        {
            Unsafe.SkipInit(out Vector64<uint> result);

            for (int i = 0; i < Vector64<uint>.Count; i++)
            {
                uint value = float.ConvertToIntegerNative<uint>(vector.GetElementUnsafe(i));
                result.SetElementUnsafe(i, value);
            }

            return result;
        }

        /// <summary>Converts a <see langword="Vector64&lt;Double&gt;" /> to a <see langword="Vector64&lt;UInt64&gt;" /> using saturation on overflow.</summary>
        /// <param name="vector">The vector to convert.</param>
        /// <returns>The converted vector.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<ulong> ConvertToUInt64(Vector64<double> vector)
        {
            Unsafe.SkipInit(out Vector64<ulong> result);

            for (int i = 0; i < Vector64<ulong>.Count; i++)
            {
                ulong value = double.ConvertToInteger<ulong>(vector.GetElementUnsafe(i));
                result.SetElementUnsafe(i, value);
            }

            return result;
        }

        /// <summary>Converts a <see langword="Vector64&lt;Double&gt;" /> to a <see langword="Vector64&lt;UInt64&gt;" /> using platform specific behavior on overflow.</summary>
        /// <param name="vector">The vector to convert.</param>
        /// <returns>The converted vector.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<ulong> ConvertToUInt64Native(Vector64<double> vector)
        {
            Unsafe.SkipInit(out Vector64<ulong> result);

            for (int i = 0; i < Vector64<ulong>.Count; i++)
            {
                ulong value = double.ConvertToIntegerNative<ulong>(vector.GetElementUnsafe(i));
                result.SetElementUnsafe(i, value);
            }

            return result;
        }

        /// <inheritdoc cref="ISimdVector{TSelf, T}.CopySign(TSelf, TSelf)" />
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> CopySign<T>(Vector64<T> value, Vector64<T> sign)
        {
            if ((typeof(T) == typeof(byte))
             || (typeof(T) == typeof(ushort))
             || (typeof(T) == typeof(uint))
             || (typeof(T) == typeof(ulong))
             || (typeof(T) == typeof(nuint)))
            {
                return value;
            }
            else if (IsHardwareAccelerated)
            {
                return VectorMath.CopySign<Vector64<T>, T>(value, sign);
            }
            else
            {
                Unsafe.SkipInit(out Vector64<T> result);

                for (int index = 0; index < Vector64<T>.Count; index++)
                {
                    T element = Scalar<T>.CopySign(value.GetElementUnsafe(index), sign.GetElementUnsafe(index));
                    result.SetElementUnsafe(index, element);
                }

                return result;
            }
        }

        /// <summary>Copies a <see cref="Vector64{T}" /> to a given array.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector to be copied.</param>
        /// <param name="destination">The array to which <paramref name="vector" /> is copied.</param>
        /// <exception cref="ArgumentException">The length of <paramref name="destination" /> is less than <see cref="Vector64{T}.Count" />.</exception>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> and <paramref name="destination" /> (<typeparamref name="T" />) is not supported.</exception>
        /// <exception cref="NullReferenceException"><paramref name="destination" /> is <c>null</c>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this Vector64<T> vector, T[] destination)
        {
            // We explicitly don't check for `null` because historically this has thrown `NullReferenceException` for perf reasons

            if (destination.Length < Vector64<T>.Count)
            {
                ThrowHelper.ThrowArgumentException_DestinationTooShort();
            }

            Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref destination[0]), vector);
        }

        /// <summary>Copies a <see cref="Vector64{T}" /> to a given array starting at the specified index.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector to be copied.</param>
        /// <param name="destination">The array to which <paramref name="vector" /> is copied.</param>
        /// <param name="startIndex">The starting index of <paramref name="destination" /> which <paramref name="vector" /> will be copied to.</param>
        /// <exception cref="ArgumentException">The length of <paramref name="destination" /> is less than <see cref="Vector64{T}.Count" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="startIndex" /> is negative or greater than the length of <paramref name="destination" />.</exception>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> and <paramref name="destination" /> (<typeparamref name="T" />) is not supported.</exception>
        /// <exception cref="NullReferenceException"><paramref name="destination" /> is <c>null</c>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this Vector64<T> vector, T[] destination, int startIndex)
        {
            // We explicitly don't check for `null` because historically this has thrown `NullReferenceException` for perf reasons

            if ((uint)startIndex >= (uint)destination.Length)
            {
                ThrowHelper.ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_IndexMustBeLess();
            }

            if ((destination.Length - startIndex) < Vector64<T>.Count)
            {
                ThrowHelper.ThrowArgumentException_DestinationTooShort();
            }

            Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref destination[startIndex]), vector);
        }

        /// <summary>Copies a <see cref="Vector64{T}" /> to a given span.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector to be copied.</param>
        /// <param name="destination">The span to which <paramref name="vector" /> is copied.</param>
        /// <exception cref="ArgumentException">The length of <paramref name="destination" /> is less than <see cref="Vector64{T}.Count" />.</exception>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> and <paramref name="destination" /> (<typeparamref name="T" />) is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this Vector64<T> vector, Span<T> destination)
        {
            if (destination.Length < Vector64<T>.Count)
            {
                ThrowHelper.ThrowArgumentException_DestinationTooShort();
            }

            Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(destination)), vector);
        }

        internal static Vector64<T> Cos<T>(Vector64<T> vector)
            where T : ITrigonometricFunctions<T>
        {
            Unsafe.SkipInit(out Vector64<T> result);

            for (int index = 0; index < Vector64<T>.Count; index++)
            {
                T value = T.Cos(vector.GetElementUnsafe(index));
                result.SetElementUnsafe(index, value);
            }

            return result;
        }

        /// <summary>Computes the cos of each element in a vector.</summary>
        /// <param name="vector">The vector that will have its Cos computed.</param>
        /// <returns>A vector whose elements are the cos of the elements in <paramref name="vector" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<double> Cos(Vector64<double> vector)
        {
            if (IsHardwareAccelerated)
            {
                return VectorMath.CosDouble<Vector64<double>, Vector64<long>>(vector);
            }
            else
            {
                return Cos<double>(vector);
            }
        }

        /// <summary>Computes the cos of each element in a vector.</summary>
        /// <param name="vector">The vector that will have its Cos computed.</param>
        /// <returns>A vector whose elements are the cos of the elements in <paramref name="vector" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<float> Cos(Vector64<float> vector)
        {
            if (IsHardwareAccelerated)
            {
                if (Vector128.IsHardwareAccelerated)
                {
                    return VectorMath.CosSingle<Vector64<float>, Vector64<int>, Vector128<double>, Vector128<long>>(vector);
                }
                else
                {
                    return VectorMath.CosSingle<Vector64<float>, Vector64<int>, Vector64<double>, Vector64<long>>(vector);
                }
            }
            else
            {
                return Cos<float>(vector);
            }
        }

        /// <summary>Determines the number of elements in a vector that are equal to a given value.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector whose elements are being checked.</param>
        /// <param name="value">The value to check for in <paramref name="vector" /></param>
        /// <returns>The number of elements in <paramref name="vector" /> that are equal to <paramref name="value" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> and <paramref name="value" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Count<T>(Vector64<T> vector, T value) => BitOperations.PopCount(Equals(vector, Create(value)).ExtractMostSignificantBits());

        /// <summary>Determines the number of elements in a vector that have all their bits set.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector whose elements are being checked.</param>
        /// <returns>The number of elements in <paramref name="vector" /> that have all their bits set.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountWhereAllBitsSet<T>(Vector64<T> vector)
        {
            if (typeof(T) == typeof(float))
            {
                return Count(vector.AsInt32(), -1);
            }
            else if (typeof(T) == typeof(double))
            {
                return Count(vector.AsInt64(), -1);
            }
            else
            {
                return Count(vector, Scalar<T>.AllBitsSet);
            }
        }

        /// <summary>Creates a new <see cref="Vector64{T}" /> instance with all elements initialized to the specified value.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="value">The value that all elements will be initialized to.</param>
        /// <returns>A new <see cref="Vector64{T}" /> with all elements initialized to <paramref name="value" />.</returns>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> Create<T>(T value)
        {
            Unsafe.SkipInit(out Vector64<T> result);

            for (int index = 0; index < Vector64<T>.Count; index++)
            {
                result.SetElementUnsafe(index, value);
            }

            return result;
        }

        /// <summary>Creates a new <see langword="Vector64&lt;Byte&gt;" /> instance with all elements initialized to the specified value.</summary>
        /// <param name="value">The value that all elements will be initialized to.</param>
        /// <remarks>On x86, this method corresponds to __m64 _mm_set1_pi8</remarks>
        /// <returns>A new <see langword="Vector64&lt;Byte&gt;" /> with all elements initialized to <paramref name="value" />.</returns>
        [Intrinsic]
        public static Vector64<byte> Create(byte value) => Create<byte>(value);

        /// <summary>Creates a new <see langword="Vector64&lt;Double&gt;" /> instance with all elements initialized to the specified value.</summary>
        /// <param name="value">The value that all elements will be initialized to.</param>
        /// <returns>A new <see langword="Vector64&lt;Double&gt;" /> with all elements initialized to <paramref name="value" />.</returns>
        [Intrinsic]
        public static Vector64<double> Create(double value) => Create<double>(value);

        /// <summary>Creates a new <see langword="Vector64&lt;Int16&gt;" /> instance with all elements initialized to the specified value.</summary>
        /// <param name="value">The value that all elements will be initialized to.</param>
        /// <remarks>On x86, this method corresponds to __m64 _mm_set1_pi16</remarks>
        /// <returns>A new <see langword="Vector64&lt;Int16&gt;" /> with all elements initialized to <paramref name="value" />.</returns>
        [Intrinsic]
        public static Vector64<short> Create(short value) => Create<short>(value);

        /// <summary>Creates a new <see langword="Vector64&lt;Int32&gt;" /> instance with all elements initialized to the specified value.</summary>
        /// <param name="value">The value that all elements will be initialized to.</param>
        /// <remarks>On x86, this method corresponds to __m64 _mm_set1_pi32</remarks>
        /// <returns>A new <see langword="Vector64&lt;Int32&gt;" /> with all elements initialized to <paramref name="value" />.</returns>
        [Intrinsic]
        public static Vector64<int> Create(int value) => Create<int>(value);

        /// <summary>Creates a new <see langword="Vector64&lt;Int64&gt;" /> instance with all elements initialized to the specified value.</summary>
        /// <param name="value">The value that all elements will be initialized to.</param>
        /// <returns>A new <see langword="Vector64&lt;Int64&gt;" /> with all elements initialized to <paramref name="value" />.</returns>
        [Intrinsic]
        public static Vector64<long> Create(long value) => Create<long>(value);

        /// <summary>Creates a new <see langword="Vector64&lt;IntPtr&gt;" /> instance with all elements initialized to the specified value.</summary>
        /// <param name="value">The value that all elements will be initialized to.</param>
        /// <returns>A new <see langword="Vector64&lt;IntPtr&gt;" /> with all elements initialized to <paramref name="value" />.</returns>
        [Intrinsic]
        public static Vector64<nint> Create(nint value) => Create<nint>(value);

        /// <summary>Creates a new <see langword="Vector64&lt;UIntPtr&gt;" /> instance with all elements initialized to the specified value.</summary>
        /// <param name="value">The value that all elements will be initialized to.</param>
        /// <returns>A new <see langword="Vector64&lt;UIntPtr&gt;" /> with all elements initialized to <paramref name="value" />.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        public static Vector64<nuint> Create(nuint value) => Create<nuint>(value);

        /// <summary>Creates a new <see langword="Vector64&lt;SByte&gt;" /> instance with all elements initialized to the specified value.</summary>
        /// <param name="value">The value that all elements will be initialized to.</param>
        /// <remarks>On x86, this method corresponds to __m64 _mm_set1_pi8</remarks>
        /// <returns>A new <see langword="Vector64&lt;SByte&gt;" /> with all elements initialized to <paramref name="value" />.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        public static Vector64<sbyte> Create(sbyte value) => Create<sbyte>(value);

        /// <summary>Creates a new <see langword="Vector64&lt;Single&gt;" /> instance with all elements initialized to the specified value.</summary>
        /// <param name="value">The value that all elements will be initialized to.</param>
        /// <returns>A new <see langword="Vector64&lt;Single&gt;" /> with all elements initialized to <paramref name="value" />.</returns>
        [Intrinsic]
        public static Vector64<float> Create(float value) => Create<float>(value);

        /// <summary>Creates a new <see langword="Vector64&lt;UInt16&gt;" /> instance with all elements initialized to the specified value.</summary>
        /// <param name="value">The value that all elements will be initialized to.</param>
        /// <remarks>On x86, this method corresponds to __m64 _mm_set1_pi16</remarks>
        /// <returns>A new <see langword="Vector64&lt;UInt16&gt;" /> with all elements initialized to <paramref name="value" />.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        public static Vector64<ushort> Create(ushort value) => Create<ushort>(value);

        /// <summary>Creates a new <see langword="Vector64&lt;UInt32&gt;" /> instance with all elements initialized to the specified value.</summary>
        /// <param name="value">The value that all elements will be initialized to.</param>
        /// <remarks>On x86, this method corresponds to __m64 _mm_set1_pi32</remarks>
        /// <returns>A new <see langword="Vector64&lt;UInt32&gt;" /> with all elements initialized to <paramref name="value" />.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        public static Vector64<uint> Create(uint value) => Create<uint>(value);

        /// <summary>Creates a new <see langword="Vector64&lt;UInt64&gt;" /> instance with all elements initialized to the specified value.</summary>
        /// <param name="value">The value that all elements will be initialized to.</param>
        /// <returns>A new <see langword="Vector64&lt;UInt64&gt;" /> with all elements initialized to <paramref name="value" />.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        public static Vector64<ulong> Create(ulong value) => Create<ulong>(value);

        /// <summary>Creates a new <see cref="Vector64{T}" /> from a given array.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="values">The array from which the vector is created.</param>
        /// <returns>A new <see cref="Vector64{T}" /> with its elements set to the first <see cref="Vector64{T}.Count" /> elements from <paramref name="values" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The length of <paramref name="values" /> is less than <see cref="Vector64{T}.Count" />.</exception>
        /// <exception cref="NotSupportedException">The type of <paramref name="values" /> (<typeparamref name="T" />) is not supported.</exception>
        /// <exception cref="NullReferenceException"><paramref name="values" /> is <c>null</c>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> Create<T>(T[] values)
        {
            // We explicitly don't check for `null` because historically this has thrown `NullReferenceException` for perf reasons

            if (values.Length < Vector64<T>.Count)
            {
                ThrowHelper.ThrowArgumentOutOfRange_IndexMustBeLessOrEqualException();
            }

            return Unsafe.ReadUnaligned<Vector64<T>>(ref Unsafe.As<T, byte>(ref values[0]));
        }

        /// <summary>Creates a new <see cref="Vector64{T}" /> from a given array.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="values">The array from which the vector is created.</param>
        /// <param name="index">The index in <paramref name="values" /> at which to being reading elements.</param>
        /// <returns>A new <see cref="Vector64{T}" /> with its elements set to the first <see cref="Vector128{T}.Count" /> elements from <paramref name="values" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The length of <paramref name="values" />, starting from <paramref name="index" />, is less than <see cref="Vector64{T}.Count" />.</exception>
        /// <exception cref="NotSupportedException">The type of <paramref name="values" /> (<typeparamref name="T" />) is not supported.</exception>
        /// <exception cref="NullReferenceException"><paramref name="values" /> is <c>null</c>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> Create<T>(T[] values, int index)
        {
            // We explicitly don't check for `null` because historically this has thrown `NullReferenceException` for perf reasons

            if ((index < 0) || ((values.Length - index) < Vector64<T>.Count))
            {
                ThrowHelper.ThrowArgumentOutOfRange_IndexMustBeLessOrEqualException();
            }

            return Unsafe.ReadUnaligned<Vector64<T>>(ref Unsafe.As<T, byte>(ref values[index]));
        }

        /// <summary>Creates a new <see cref="Vector64{T}" /> from a given readonly span.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="values">The readonly span from which the vector is created.</param>
        /// <returns>A new <see cref="Vector64{T}" /> with its elements set to the first <see cref="Vector64{T}.Count" /> elements from <paramref name="values" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The length of <paramref name="values" /> is less than <see cref="Vector64{T}.Count" />.</exception>
        /// <exception cref="NotSupportedException">The type of <paramref name="values" /> (<typeparamref name="T" />) is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> Create<T>(ReadOnlySpan<T> values)
        {
            if (values.Length < Vector64<T>.Count)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.values);
            }

            return Unsafe.ReadUnaligned<Vector64<T>>(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(values)));
        }

        /// <summary>Creates a new <see langword="Vector64&lt;Byte&gt;" /> instance with each element initialized to the corresponding specified value.</summary>
        /// <param name="e0">The value that element 0 will be initialized to.</param>
        /// <param name="e1">The value that element 1 will be initialized to.</param>
        /// <param name="e2">The value that element 2 will be initialized to.</param>
        /// <param name="e3">The value that element 3 will be initialized to.</param>
        /// <param name="e4">The value that element 4 will be initialized to.</param>
        /// <param name="e5">The value that element 5 will be initialized to.</param>
        /// <param name="e6">The value that element 6 will be initialized to.</param>
        /// <param name="e7">The value that element 7 will be initialized to.</param>
        /// <remarks>On x86, this method corresponds to __m64 _mm_setr_pi8</remarks>
        /// <returns>A new <see langword="Vector64&lt;Byte&gt;" /> with each element initialized to corresponding specified value.</returns>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<byte> Create(byte e0, byte e1, byte e2, byte e3, byte e4, byte e5, byte e6, byte e7)
        {
            Unsafe.SkipInit(out Vector64<byte> result);
            result.SetElementUnsafe(0, e0);
            result.SetElementUnsafe(1, e1);
            result.SetElementUnsafe(2, e2);
            result.SetElementUnsafe(3, e3);
            result.SetElementUnsafe(4, e4);
            result.SetElementUnsafe(5, e5);
            result.SetElementUnsafe(6, e6);
            result.SetElementUnsafe(7, e7);
            return result;
        }

        /// <summary>Creates a new <see langword="Vector64&lt;Int16&gt;" /> instance with each element initialized to the corresponding specified value.</summary>
        /// <param name="e0">The value that element 0 will be initialized to.</param>
        /// <param name="e1">The value that element 1 will be initialized to.</param>
        /// <param name="e2">The value that element 2 will be initialized to.</param>
        /// <param name="e3">The value that element 3 will be initialized to.</param>
        /// <remarks>On x86, this method corresponds to __m64 _mm_setr_pi16</remarks>
        /// <returns>A new <see langword="Vector64&lt;Int16&gt;" /> with each element initialized to corresponding specified value.</returns>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<short> Create(short e0, short e1, short e2, short e3)
        {
            Unsafe.SkipInit(out Vector64<short> result);
            result.SetElementUnsafe(0, e0);
            result.SetElementUnsafe(1, e1);
            result.SetElementUnsafe(2, e2);
            result.SetElementUnsafe(3, e3);
            return result;
        }

        /// <summary>Creates a new <see langword="Vector64&lt;Int32&gt;" /> instance with each element initialized to the corresponding specified value.</summary>
        /// <param name="e0">The value that element 0 will be initialized to.</param>
        /// <param name="e1">The value that element 1 will be initialized to.</param>
        /// <remarks>On x86, this method corresponds to __m64 _mm_setr_pi32</remarks>
        /// <returns>A new <see langword="Vector64&lt;Int32&gt;" /> with each element initialized to corresponding specified value.</returns>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<int> Create(int e0, int e1)
        {
            Unsafe.SkipInit(out Vector64<int> result);
            result.SetElementUnsafe(0, e0);
            result.SetElementUnsafe(1, e1);
            return result;
        }

        /// <summary>Creates a new <see langword="Vector64&lt;SByte&gt;" /> instance with each element initialized to the corresponding specified value.</summary>
        /// <param name="e0">The value that element 0 will be initialized to.</param>
        /// <param name="e1">The value that element 1 will be initialized to.</param>
        /// <param name="e2">The value that element 2 will be initialized to.</param>
        /// <param name="e3">The value that element 3 will be initialized to.</param>
        /// <param name="e4">The value that element 4 will be initialized to.</param>
        /// <param name="e5">The value that element 5 will be initialized to.</param>
        /// <param name="e6">The value that element 6 will be initialized to.</param>
        /// <param name="e7">The value that element 7 will be initialized to.</param>
        /// <remarks>On x86, this method corresponds to __m64 _mm_setr_pi8</remarks>
        /// <returns>A new <see langword="Vector64&lt;SByte&gt;" /> with each element initialized to corresponding specified value.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<sbyte> Create(sbyte e0, sbyte e1, sbyte e2, sbyte e3, sbyte e4, sbyte e5, sbyte e6, sbyte e7)
        {
            Unsafe.SkipInit(out Vector64<sbyte> result);
            result.SetElementUnsafe(0, e0);
            result.SetElementUnsafe(1, e1);
            result.SetElementUnsafe(2, e2);
            result.SetElementUnsafe(3, e3);
            result.SetElementUnsafe(4, e4);
            result.SetElementUnsafe(5, e5);
            result.SetElementUnsafe(6, e6);
            result.SetElementUnsafe(7, e7);
            return result;
        }

        /// <summary>Creates a new <see langword="Vector64&lt;Single&gt;" /> instance with each element initialized to the corresponding specified value.</summary>
        /// <param name="e0">The value that element 0 will be initialized to.</param>
        /// <param name="e1">The value that element 1 will be initialized to.</param>
        /// <returns>A new <see langword="Vector64&lt;Single&gt;" /> with each element initialized to corresponding specified value.</returns>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<float> Create(float e0, float e1)
        {
            Unsafe.SkipInit(out Vector64<float> result);
            result.SetElementUnsafe(0, e0);
            result.SetElementUnsafe(1, e1);
            return result;
        }

        /// <summary>Creates a new <see langword="Vector64&lt;UInt16&gt;" /> instance with each element initialized to the corresponding specified value.</summary>
        /// <param name="e0">The value that element 0 will be initialized to.</param>
        /// <param name="e1">The value that element 1 will be initialized to.</param>
        /// <param name="e2">The value that element 2 will be initialized to.</param>
        /// <param name="e3">The value that element 3 will be initialized to.</param>
        /// <remarks>On x86, this method corresponds to __m64 _mm_setr_pi16</remarks>
        /// <returns>A new <see langword="Vector64&lt;UInt16&gt;" /> with each element initialized to corresponding specified value.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<ushort> Create(ushort e0, ushort e1, ushort e2, ushort e3)
        {
            Unsafe.SkipInit(out Vector64<ushort> result);
            result.SetElementUnsafe(0, e0);
            result.SetElementUnsafe(1, e1);
            result.SetElementUnsafe(2, e2);
            result.SetElementUnsafe(3, e3);
            return result;
        }

        /// <summary>Creates a new <see langword="Vector64&lt;UInt32&gt;" /> instance with each element initialized to the corresponding specified value.</summary>
        /// <param name="e0">The value that element 0 will be initialized to.</param>
        /// <param name="e1">The value that element 1 will be initialized to.</param>
        /// <remarks>On x86, this method corresponds to __m64 _mm_setr_pi32</remarks>
        /// <returns>A new <see langword="Vector64&lt;UInt32&gt;" /> with each element initialized to corresponding specified value.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<uint> Create(uint e0, uint e1)
        {
            Unsafe.SkipInit(out Vector64<uint> result);
            result.SetElementUnsafe(0, e0);
            result.SetElementUnsafe(1, e1);
            return result;
        }

        /// <summary>Creates a new <see cref="Vector64{T}" /> instance with the first element initialized to the specified value and the remaining elements initialized to zero.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="value">The value that element 0 will be initialized to.</param>
        /// <returns>A new <see langword="Vector64&lt;Byte&gt;" /> instance with the first element initialized to <paramref name="value" /> and the remaining elements initialized to zero.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="value" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> CreateScalar<T>(T value)
        {
            Vector64<T> result = Vector64<T>.Zero;
            result.SetElementUnsafe(0, value);
            return result;
        }

        /// <summary>Creates a new <see langword="Vector64&lt;Byte&gt;" /> instance with the first element initialized to the specified value and the remaining elements initialized to zero.</summary>
        /// <param name="value">The value that element 0 will be initialized to.</param>
        /// <returns>A new <see langword="Vector64&lt;Byte&gt;" /> instance with the first element initialized to <paramref name="value" /> and the remaining elements initialized to zero.</returns>
        [Intrinsic]
        public static Vector64<byte> CreateScalar(byte value) => CreateScalar<byte>(value);

        /// <summary>Creates a new <see langword="Vector64&lt;Double&gt;" /> instance with the first element initialized to the specified value and the remaining elements initialized to zero.</summary>
        /// <param name="value">The value that element 0 will be initialized to.</param>
        /// <returns>A new <see langword="Vector64&lt;Double&gt;" /> instance with the first element initialized to <paramref name="value" /> and the remaining elements initialized to zero.</returns>
        [Intrinsic]
        public static Vector64<double> CreateScalar(double value) => CreateScalar<double>(value);

        /// <summary>Creates a new <see langword="Vector64&lt;Int16&gt;" /> instance with the first element initialized to the specified value and the remaining elements initialized to zero.</summary>
        /// <param name="value">The value that element 0 will be initialized to.</param>
        /// <returns>A new <see langword="Vector64&lt;Int16&gt;" /> instance with the first element initialized to <paramref name="value" /> and the remaining elements initialized to zero.</returns>
        [Intrinsic]
        public static Vector64<short> CreateScalar(short value) => CreateScalar<short>(value);

        /// <summary>Creates a new <see langword="Vector64&lt;Int32&gt;" /> instance with the first element initialized to the specified value and the remaining elements initialized to zero.</summary>
        /// <param name="value">The value that element 0 will be initialized to.</param>
        /// <returns>A new <see langword="Vector64&lt;Int32&gt;" /> instance with the first element initialized to <paramref name="value" /> and the remaining elements initialized to zero.</returns>
        [Intrinsic]
        public static Vector64<int> CreateScalar(int value) => CreateScalar<int>(value);

        /// <summary>Creates a new <see langword="Vector64&lt;Int64&gt;" /> instance with the first element initialized to the specified value and the remaining elements initialized to zero.</summary>
        /// <param name="value">The value that element 0 will be initialized to.</param>
        /// <returns>A new <see langword="Vector64&lt;Int64&gt;" /> instance with the first element initialized to <paramref name="value" /> and the remaining elements initialized to zero.</returns>
        [Intrinsic]
        public static Vector64<long> CreateScalar(long value) => CreateScalar<long>(value);

        /// <summary>Creates a new <see langword="Vector64&lt;IntPtr&gt;" /> instance with the first element initialized to the specified value and the remaining elements initialized to zero.</summary>
        /// <param name="value">The value that element 0 will be initialized to.</param>
        /// <returns>A new <see langword="Vector64&lt;IntPtr&gt;" /> instance with the first element initialized to <paramref name="value" /> and the remaining elements initialized to zero.</returns>
        [Intrinsic]
        public static Vector64<nint> CreateScalar(nint value) => CreateScalar<nint>(value);

        /// <summary>Creates a new <see langword="Vector64&lt;UIntPtr&gt;" /> instance with the first element initialized to the specified value and the remaining elements initialized to zero.</summary>
        /// <param name="value">The value that element 0 will be initialized to.</param>
        /// <returns>A new <see langword="Vector64&lt;UIntPtr&gt;" /> instance with the first element initialized to <paramref name="value" /> and the remaining elements initialized to zero.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        public static Vector64<nuint> CreateScalar(nuint value) => CreateScalar<nuint>(value);

        /// <summary>Creates a new <see langword="Vector64&lt;SByte&gt;" /> instance with the first element initialized to the specified value and the remaining elements initialized to zero.</summary>
        /// <param name="value">The value that element 0 will be initialized to.</param>
        /// <returns>A new <see langword="Vector64&lt;SByte&gt;" /> instance with the first element initialized to <paramref name="value" /> and the remaining elements initialized to zero.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        public static Vector64<sbyte> CreateScalar(sbyte value) => CreateScalar<sbyte>(value);

        /// <summary>Creates a new <see langword="Vector64&lt;Single&gt;" /> instance with the first element initialized to the specified value and the remaining elements initialized to zero.</summary>
        /// <param name="value">The value that element 0 will be initialized to.</param>
        /// <returns>A new <see langword="Vector64&lt;Single&gt;" /> instance with the first element initialized to <paramref name="value" /> and the remaining elements initialized to zero.</returns>
        [Intrinsic]
        public static Vector64<float> CreateScalar(float value) => CreateScalar<float>(value);

        /// <summary>Creates a new <see langword="Vector64&lt;UInt16&gt;" /> instance with the first element initialized to the specified value and the remaining elements initialized to zero.</summary>
        /// <param name="value">The value that element 0 will be initialized to.</param>
        /// <returns>A new <see langword="Vector64&lt;UInt16&gt;" /> instance with the first element initialized to <paramref name="value" /> and the remaining elements initialized to zero.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        public static Vector64<ushort> CreateScalar(ushort value) => CreateScalar<ushort>(value);

        /// <summary>Creates a new <see langword="Vector64&lt;UInt32&gt;" /> instance with the first element initialized to the specified value and the remaining elements initialized to zero.</summary>
        /// <param name="value">The value that element 0 will be initialized to.</param>
        /// <returns>A new <see langword="Vector64&lt;UInt32&gt;" /> instance with the first element initialized to <paramref name="value" /> and the remaining elements initialized to zero.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        public static Vector64<uint> CreateScalar(uint value) => CreateScalar<uint>(value);

        /// <summary>Creates a new <see langword="Vector64&lt;UInt64&gt;" /> instance with the first element initialized to the specified value and the remaining elements initialized to zero.</summary>
        /// <param name="value">The value that element 0 will be initialized to.</param>
        /// <returns>A new <see langword="Vector64&lt;UInt64&gt;" /> instance with the first element initialized to <paramref name="value" /> and the remaining elements initialized to zero.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        public static Vector64<ulong> CreateScalar(ulong value) => CreateScalar<ulong>(value);

        /// <summary>Creates a new <see cref="Vector64{T}" /> instance with the first element initialized to the specified value and the remaining elements left uninitialized.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="value">The value that element 0 will be initialized to.</param>
        /// <returns>A new <see cref="Vector64{T}" /> instance with the first element initialized to <paramref name="value" /> and the remaining elements left uninitialized.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="value" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> CreateScalarUnsafe<T>(T value)
        {
            // This relies on us stripping the "init" flag from the ".locals"
            // declaration to let the upper bits be uninitialized.

            ThrowHelper.ThrowForUnsupportedIntrinsicsVector64BaseType<T>();
            Unsafe.SkipInit(out Vector64<T> result);

            result.SetElementUnsafe(0, value);
            return result;
        }

        /// <summary>Creates a new <see langword="Vector64&lt;Byte&gt;" /> instance with the first element initialized to the specified value and the remaining elements left uninitialized.</summary>
        /// <param name="value">The value that element 0 will be initialized to.</param>
        /// <returns>A new <see langword="Vector64&lt;Byte&gt;" /> instance with the first element initialized to <paramref name="value" /> and the remaining elements left uninitialized.</returns>
        [Intrinsic]
        public static Vector64<byte> CreateScalarUnsafe(byte value) => CreateScalarUnsafe<byte>(value);

        /// <summary>Creates a new <see langword="Vector64&lt;Double&gt;" /> instance with the first element initialized to the specified value and the remaining elements left uninitialized.</summary>
        /// <param name="value">The value that element 0 will be initialized to.</param>
        /// <returns>A new <see langword="Vector64&lt;Double&gt;" /> instance with the first element initialized to <paramref name="value" /> and the remaining elements left uninitialized.</returns>
        [Intrinsic]
        public static Vector64<double> CreateScalarUnsafe(double value) => CreateScalarUnsafe<double>(value);

        /// <summary>Creates a new <see langword="Vector64&lt;Int16&gt;" /> instance with the first element initialized to the specified value and the remaining elements left uninitialized.</summary>
        /// <param name="value">The value that element 0 will be initialized to.</param>
        /// <returns>A new <see langword="Vector64&lt;Int16&gt;" /> instance with the first element initialized to <paramref name="value" /> and the remaining elements left uninitialized.</returns>
        [Intrinsic]
        public static Vector64<short> CreateScalarUnsafe(short value) => CreateScalarUnsafe<short>(value);

        /// <summary>Creates a new <see langword="Vector64&lt;Int32&gt;" /> instance with the first element initialized to the specified value and the remaining elements left uninitialized.</summary>
        /// <param name="value">The value that element 0 will be initialized to.</param>
        /// <returns>A new <see langword="Vector64&lt;Int32&gt;" /> instance with the first element initialized to <paramref name="value" /> and the remaining elements left uninitialized.</returns>
        [Intrinsic]
        public static Vector64<int> CreateScalarUnsafe(int value) => CreateScalarUnsafe<int>(value);

        /// <summary>Creates a new <see langword="Vector64&lt;Int64&gt;" /> instance with the first element initialized to the specified value and the remaining elements left uninitialized.</summary>
        /// <param name="value">The value that element 0 will be initialized to.</param>
        /// <returns>A new <see langword="Vector64&lt;Int64&gt;" /> instance with the first element initialized to <paramref name="value" /> and the remaining elements left uninitialized.</returns>
        [Intrinsic]
        public static Vector64<long> CreateScalarUnsafe(long value) => CreateScalarUnsafe<long>(value);

        /// <summary>Creates a new <see langword="Vector64&lt;IntPtr&gt;" /> instance with the first element initialized to the specified value and the remaining elements left uninitialized.</summary>
        /// <param name="value">The value that element 0 will be initialized to.</param>
        /// <returns>A new <see langword="Vector64&lt;IntPtr&gt;" /> instance with the first element initialized to <paramref name="value" /> and the remaining elements left uninitialized.</returns>
        [Intrinsic]
        public static Vector64<nint> CreateScalarUnsafe(nint value) => CreateScalarUnsafe<nint>(value);

        /// <summary>Creates a new <see langword="Vector64&lt;UIntPtr&gt;" /> instance with the first element initialized to the specified value and the remaining elements left uninitialized.</summary>
        /// <param name="value">The value that element 0 will be initialized to.</param>
        /// <returns>A new <see langword="Vector64&lt;UIntPtr&gt;" /> instance with the first element initialized to <paramref name="value" /> and the remaining elements left uninitialized.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        public static Vector64<nuint> CreateScalarUnsafe(nuint value) => CreateScalarUnsafe<nuint>(value);

        /// <summary>Creates a new <see langword="Vector64&lt;SByte&gt;" /> instance with the first element initialized to the specified value and the remaining elements left uninitialized.</summary>
        /// <param name="value">The value that element 0 will be initialized to.</param>
        /// <returns>A new <see langword="Vector64&lt;SByte&gt;" /> instance with the first element initialized to <paramref name="value" /> and the remaining elements left uninitialized.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        public static Vector64<sbyte> CreateScalarUnsafe(sbyte value) => CreateScalarUnsafe<sbyte>(value);

        /// <summary>Creates a new <see langword="Vector64&lt;Single&gt;" /> instance with the first element initialized to the specified value and the remaining elements left uninitialized.</summary>
        /// <param name="value">The value that element 0 will be initialized to.</param>
        /// <returns>A new <see langword="Vector64&lt;Single&gt;" /> instance with the first element initialized to <paramref name="value" /> and the remaining elements left uninitialized.</returns>
        [Intrinsic]
        public static Vector64<float> CreateScalarUnsafe(float value) => CreateScalarUnsafe<float>(value);

        /// <summary>Creates a new <see langword="Vector64&lt;UInt16&gt;" /> instance with the first element initialized to the specified value and the remaining elements left uninitialized.</summary>
        /// <param name="value">The value that element 0 will be initialized to.</param>
        /// <returns>A new <see langword="Vector64&lt;UInt16&gt;" /> instance with the first element initialized to <paramref name="value" /> and the remaining elements left uninitialized.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        public static Vector64<ushort> CreateScalarUnsafe(ushort value) => CreateScalarUnsafe<ushort>(value);

        /// <summary>Creates a new <see langword="Vector64&lt;UInt32&gt;" /> instance with the first element initialized to the specified value and the remaining elements left uninitialized.</summary>
        /// <param name="value">The value that element 0 will be initialized to.</param>
        /// <returns>A new <see langword="Vector64&lt;UInt32&gt;" /> instance with the first element initialized to <paramref name="value" /> and the remaining elements left uninitialized.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        public static Vector64<uint> CreateScalarUnsafe(uint value) => CreateScalarUnsafe<uint>(value);

        /// <summary>Creates a new <see langword="Vector64&lt;UInt64&gt;" /> instance with the first element initialized to the specified value and the remaining elements left uninitialized.</summary>
        /// <param name="value">The value that element 0 will be initialized to.</param>
        /// <returns>A new <see langword="Vector64&lt;UInt64&gt;" /> instance with the first element initialized to <paramref name="value" /> and the remaining elements left uninitialized.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        public static Vector64<ulong> CreateScalarUnsafe(ulong value) => CreateScalarUnsafe<ulong>(value);

        /// <summary>Creates a new <see cref="Vector64{T}" /> instance where the elements begin at a specified value and which are spaced apart according to another specified value.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="start">The value that element 0 will be initialized to.</param>
        /// <param name="step">The value that indicates how far apart each element should be from the previous.</param>
        /// <returns>A new <see cref="Vector64{T}" /> instance with the first element initialized to <paramref name="start" /> and each subsequent element initialized to the value of the previous element plus <paramref name="step" />.</returns>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> CreateSequence<T>(T start, T step) => (Vector64<T>.Indices * step) + Create(start);

        internal static Vector64<T> DegreesToRadians<T>(Vector64<T> degrees)
            where T : ITrigonometricFunctions<T>
        {
            Unsafe.SkipInit(out Vector64<T> result);

            for (int index = 0; index < Vector64<T>.Count; index++)
            {
                T value = T.DegreesToRadians(degrees.GetElementUnsafe(index));
                result.SetElementUnsafe(index, value);
            }

            return result;
        }

        /// <summary>Converts a given vector from degrees to radians.</summary>
        /// <param name="degrees">The vector to convert to radians.</param>
        /// <returns>The vector of <paramref name="degrees" /> converted to radians.</returns>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<double> DegreesToRadians(Vector64<double> degrees)
        {
            if (IsHardwareAccelerated)
            {
                return VectorMath.DegreesToRadians<Vector64<double>, double>(degrees);
            }
            else
            {
                return DegreesToRadians<double>(degrees);
            }
        }

        /// <summary>Converts a given vector from degrees to radians.</summary>
        /// <param name="degrees">The vector to convert to radians.</param>
        /// <returns>The vector of <paramref name="degrees" /> converted to radians.</returns>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<float> DegreesToRadians(Vector64<float> degrees)
        {
            if (IsHardwareAccelerated)
            {
                return VectorMath.DegreesToRadians<Vector64<float>, float>(degrees);
            }
            else
            {
                return DegreesToRadians<float>(degrees);
            }
        }

        /// <summary>Divides two vectors to compute their quotient.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="left">The vector that will be divided by <paramref name="right" />.</param>
        /// <param name="right">The vector that will divide <paramref name="left" />.</param>
        /// <returns>The quotient of <paramref name="left" /> divided by <paramref name="right" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="left" /> and <paramref name="right" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        public static Vector64<T> Divide<T>(Vector64<T> left, Vector64<T> right) => left / right;

        /// <summary>Divides a vector by a scalar to compute the per-element quotient.</summary>
        /// <param name="left">The vector that will be divided by <paramref name="right" />.</param>
        /// <param name="right">The scalar that will divide <paramref name="left" />.</param>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <returns>The quotient of <paramref name="left" /> divided by <paramref name="right" />.</returns>
        [Intrinsic]
        public static Vector64<T> Divide<T>(Vector64<T> left, T right) => left / right;

        /// <summary>Computes the dot product of two vectors.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="left">The vector that will be dotted with <paramref name="right" />.</param>
        /// <param name="right">The vector that will be dotted with <paramref name="left" />.</param>
        /// <returns>The dot product of <paramref name="left" /> and <paramref name="right" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="left" /> and <paramref name="right" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        public static T Dot<T>(Vector64<T> left, Vector64<T> right) => Sum(left * right);

        /// <summary>Compares two vectors to determine if they are equal on a per-element basis.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="left">The vector to compare with <paramref name="right" />.</param>
        /// <param name="right">The vector to compare with <paramref name="left" />.</param>
        /// <returns>A vector whose elements are all-bits-set or zero, depending on if the corresponding elements in <paramref name="left" /> and <paramref name="right" /> were equal.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="left" /> and <paramref name="right" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> Equals<T>(Vector64<T> left, Vector64<T> right)
        {
            Unsafe.SkipInit(out Vector64<T> result);

            for (int index = 0; index < Vector64<T>.Count; index++)
            {
                T value = Scalar<T>.Equals(left.GetElementUnsafe(index), right.GetElementUnsafe(index)) ? Scalar<T>.AllBitsSet : default!;
                result.SetElementUnsafe(index, value);
            }

            return result;
        }

        /// <summary>Compares two vectors to determine if all elements are equal.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="left">The vector to compare with <paramref name="right" />.</param>
        /// <param name="right">The vector to compare with <paramref name="left" />.</param>
        /// <returns><c>true</c> if all elements in <paramref name="left" /> were equal to the corresponding element in <paramref name="right" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="left" /> and <paramref name="right" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        public static bool EqualsAll<T>(Vector64<T> left, Vector64<T> right) => left == right;

        /// <summary>Compares two vectors to determine if any elements are equal.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="left">The vector to compare with <paramref name="right" />.</param>
        /// <param name="right">The vector to compare with <paramref name="left" />.</param>
        /// <returns><c>true</c> if any elements in <paramref name="left" /> was equal to the corresponding element in <paramref name="right" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="left" /> and <paramref name="right" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EqualsAny<T>(Vector64<T> left, Vector64<T> right)
        {
            for (int index = 0; index < Vector64<T>.Count; index++)
            {
                if (Scalar<T>.Equals(left.GetElementUnsafe(index), right.GetElementUnsafe(index)))
                {
                    return true;
                }
            }

            return false;
        }

        internal static Vector64<T> Exp<T>(Vector64<T> vector)
            where T : IExponentialFunctions<T>
        {
            Unsafe.SkipInit(out Vector64<T> result);

            for (int index = 0; index < Vector64<T>.Count; index++)
            {
                T value = T.Exp(vector.GetElementUnsafe(index));
                result.SetElementUnsafe(index, value);
            }

            return result;
        }

        /// <summary>Computes the exp of each element in a vector.</summary>
        /// <param name="vector">The vector that will have its Exp computed.</param>
        /// <returns>A vector whose elements are the exp of the elements in <paramref name="vector" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<double> Exp(Vector64<double> vector)
        {
            if (IsHardwareAccelerated)
            {
                return VectorMath.ExpDouble<Vector64<double>, Vector64<ulong>>(vector);
            }
            else
            {
                return Exp<double>(vector);
            }
        }

        /// <summary>Computes the exp of each element in a vector.</summary>
        /// <param name="vector">The vector that will have its exp computed.</param>
        /// <returns>A vector whose elements are the exp of the elements in <paramref name="vector" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<float> Exp(Vector64<float> vector)
        {
            if (IsHardwareAccelerated)
            {
                if (Vector128.IsHardwareAccelerated)
                {
                    return VectorMath.ExpSingle<Vector64<float>, Vector64<uint>, Vector128<double>, Vector128<ulong>>(vector);
                }
                else
                {
                    return VectorMath.ExpSingle<Vector64<float>, Vector64<uint>, Vector64<double>, Vector64<ulong>>(vector);
                }
            }
            else
            {
                return Exp<float>(vector);
            }
        }

        /// <summary>Extracts the most significant bit from each element in a vector.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector whose elements should have their most significant bit extracted.</param>
        /// <returns>The packed most significant bits extracted from the elements in <paramref name="vector" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ExtractMostSignificantBits<T>(this Vector64<T> vector)
        {
            uint result = 0;

            for (int index = 0; index < Vector64<T>.Count; index++)
            {
                uint value = Scalar<T>.ExtractMostSignificantBit(vector.GetElementUnsafe(index));
                result |= (value << index);
            }

            return result;
        }

        /// <summary>Computes the floor of each element in a vector.</summary>
        /// <param name="vector">The vector that will have its floor computed.</param>
        /// <returns>A vector whose elements are the floor of the elements in <paramref name="vector" />.</returns>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector64<T> Floor<T>(Vector64<T> vector)
        {
            if ((typeof(T) == typeof(byte))
             || (typeof(T) == typeof(short))
             || (typeof(T) == typeof(int))
             || (typeof(T) == typeof(long))
             || (typeof(T) == typeof(nint))
             || (typeof(T) == typeof(nuint))
             || (typeof(T) == typeof(sbyte))
             || (typeof(T) == typeof(ushort))
             || (typeof(T) == typeof(uint))
             || (typeof(T) == typeof(ulong)))
            {
                return vector;
            }
            else
            {
                Unsafe.SkipInit(out Vector64<T> result);

                for (int index = 0; index < Vector64<T>.Count; index++)
                {
                    T value = Scalar<T>.Floor(vector.GetElementUnsafe(index));
                    result.SetElementUnsafe(index, value);
                }

                return result;
            }
        }

        /// <summary>Computes the floor of each element in a vector.</summary>
        /// <param name="vector">The vector that will have its floor computed.</param>
        /// <returns>A vector whose elements are the floor of the elements in <paramref name="vector" />.</returns>
        /// <seealso cref="MathF.Floor(float)" />
        [Intrinsic]
        public static Vector64<float> Floor(Vector64<float> vector) => Floor<float>(vector);

        /// <summary>Computes the floor of each element in a vector.</summary>
        /// <param name="vector">The vector that will have its floor computed.</param>
        /// <returns>A vector whose elements are the floor of the elements in <paramref name="vector" />.</returns>
        /// <seealso cref="Math.Floor(double)" />
        [Intrinsic]
        public static Vector64<double> Floor(Vector64<double> vector) => Floor<double>(vector);

        /// <summary>Computes (<paramref name="left" /> * <paramref name="right" />) + <paramref name="addend" />, rounded as one ternary operation.</summary>
        /// <param name="left">The vector to be multiplied with <paramref name="right" />.</param>
        /// <param name="right">The vector to be multiplied with <paramref name="left" />.</param>
        /// <param name="addend">The vector to be added to the result of <paramref name="left" /> multiplied by <paramref name="right" />.</param>
        /// <returns>(<paramref name="left" /> * <paramref name="right" />) + <paramref name="addend" />, rounded as one ternary operation.</returns>
        /// <remarks>
        ///   <para>This computes (<paramref name="left" /> * <paramref name="right" />) as if to infinite precision, adds <paramref name="addend" /> to that result as if to infinite precision, and finally rounds to the nearest representable value.</para>
        ///   <para>This differs from the non-fused sequence which would compute (<paramref name="left" /> * <paramref name="right" />) as if to infinite precision, round the result to the nearest representable value, add <paramref name="addend" /> to the rounded result as if to infinite precision, and finally round to the nearest representable value.</para>
        /// </remarks>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<double> FusedMultiplyAdd(Vector64<double> left, Vector64<double> right, Vector64<double> addend)
        {
            Unsafe.SkipInit(out Vector64<double> result);

            for (int index = 0; index < Vector64<double>.Count; index++)
            {
                double value = double.FusedMultiplyAdd(left.GetElementUnsafe(index), right.GetElementUnsafe(index), addend.GetElementUnsafe(index));
                result.SetElementUnsafe(index, value);
            }

            return result;
        }

        /// <summary>Computes (<paramref name="left" /> * <paramref name="right" />) + <paramref name="addend" />, rounded as one ternary operation.</summary>
        /// <param name="left">The vector to be multiplied with <paramref name="right" />.</param>
        /// <param name="right">The vector to be multiplied with <paramref name="left" />.</param>
        /// <param name="addend">The vector to be added to the result of <paramref name="left" /> multiplied by <paramref name="right" />.</param>
        /// <returns>(<paramref name="left" /> * <paramref name="right" />) + <paramref name="addend" />, rounded as one ternary operation.</returns>
        /// <remarks>
        ///   <para>This computes (<paramref name="left" /> * <paramref name="right" />) as if to infinite precision, adds <paramref name="addend" /> to that result as if to infinite precision, and finally rounds to the nearest representable value.</para>
        ///   <para>This differs from the non-fused sequence which would compute (<paramref name="left" /> * <paramref name="right" />) as if to infinite precision, round the result to the nearest representable value, add <paramref name="addend" /> to the rounded result as if to infinite precision, and finally round to the nearest representable value.</para>
        /// </remarks>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<float> FusedMultiplyAdd(Vector64<float> left, Vector64<float> right, Vector64<float> addend)
        {
            Unsafe.SkipInit(out Vector64<float> result);

            for (int index = 0; index < Vector64<float>.Count; index++)
            {
                float value = float.FusedMultiplyAdd(left.GetElementUnsafe(index), right.GetElementUnsafe(index), addend.GetElementUnsafe(index));
                result.SetElementUnsafe(index, value);
            }

            return result;
        }

        /// <summary>Gets the element at the specified index.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector to get the element from.</param>
        /// <param name="index">The index of the element to get.</param>
        /// <returns>The value of the element at <paramref name="index" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index" /> was less than zero or greater than the number of elements.</exception>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetElement<T>(this Vector64<T> vector, int index)
        {
            if ((uint)(index) >= (uint)(Vector64<T>.Count))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index);
            }

            return vector.GetElementUnsafe(index);
        }

        /// <summary>Compares two vectors to determine which is greater on a per-element basis.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="left">The vector to compare with <paramref name="left" />.</param>
        /// <param name="right">The vector to compare with <paramref name="right" />.</param>
        /// <returns>A vector whose elements are all-bits-set or zero, depending on if which of the corresponding elements in <paramref name="left" /> and <paramref name="right" /> were greater.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="left" /> and <paramref name="right" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> GreaterThan<T>(Vector64<T> left, Vector64<T> right)
        {
            Unsafe.SkipInit(out Vector64<T> result);

            for (int index = 0; index < Vector64<T>.Count; index++)
            {
                T value = Scalar<T>.GreaterThan(left.GetElementUnsafe(index), right.GetElementUnsafe(index)) ? Scalar<T>.AllBitsSet : default!;
                result.SetElementUnsafe(index, value);
            }

            return result;
        }

        /// <summary>Compares two vectors to determine if all elements are greater.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="left">The vector to compare with <paramref name="right" />.</param>
        /// <param name="right">The vector to compare with <paramref name="left" />.</param>
        /// <returns><c>true</c> if all elements in <paramref name="left" /> were greater than the corresponding element in <paramref name="right" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="left" /> and <paramref name="right" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GreaterThanAll<T>(Vector64<T> left, Vector64<T> right)
        {
            for (int index = 0; index < Vector64<T>.Count; index++)
            {
                if (!Scalar<T>.GreaterThan(left.GetElementUnsafe(index), right.GetElementUnsafe(index)))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Compares two vectors to determine if any elements are greater.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="left">The vector to compare with <paramref name="right" />.</param>
        /// <param name="right">The vector to compare with <paramref name="left" />.</param>
        /// <returns><c>true</c> if any elements in <paramref name="left" /> was greater than the corresponding element in <paramref name="right" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="left" /> and <paramref name="right" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GreaterThanAny<T>(Vector64<T> left, Vector64<T> right)
        {
            for (int index = 0; index < Vector64<T>.Count; index++)
            {
                if (Scalar<T>.GreaterThan(left.GetElementUnsafe(index), right.GetElementUnsafe(index)))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Compares two vectors to determine which is greater or equal on a per-element basis.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="left">The vector to compare with <paramref name="left" />.</param>
        /// <param name="right">The vector to compare with <paramref name="right" />.</param>
        /// <returns>A vector whose elements are all-bits-set or zero, depending on if which of the corresponding elements in <paramref name="left" /> and <paramref name="right" /> were greater or equal.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="left" /> and <paramref name="right" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> GreaterThanOrEqual<T>(Vector64<T> left, Vector64<T> right)
        {
            Unsafe.SkipInit(out Vector64<T> result);

            for (int index = 0; index < Vector64<T>.Count; index++)
            {
                T value = Scalar<T>.GreaterThanOrEqual(left.GetElementUnsafe(index), right.GetElementUnsafe(index)) ? Scalar<T>.AllBitsSet : default!;
                result.SetElementUnsafe(index, value);
            }

            return result;
        }

        /// <summary>Compares two vectors to determine if all elements are greater or equal.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="left">The vector to compare with <paramref name="right" />.</param>
        /// <param name="right">The vector to compare with <paramref name="left" />.</param>
        /// <returns><c>true</c> if all elements in <paramref name="left" /> were greater than or equal to the corresponding element in <paramref name="right" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="left" /> and <paramref name="right" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GreaterThanOrEqualAll<T>(Vector64<T> left, Vector64<T> right)
        {
            for (int index = 0; index < Vector64<T>.Count; index++)
            {
                if (!Scalar<T>.GreaterThanOrEqual(left.GetElementUnsafe(index), right.GetElementUnsafe(index)))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Compares two vectors to determine if any elements are greater or equal.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="left">The vector to compare with <paramref name="right" />.</param>
        /// <param name="right">The vector to compare with <paramref name="left" />.</param>
        /// <returns><c>true</c> if any elements in <paramref name="left" /> was greater than or equal to the corresponding element in <paramref name="right" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="left" /> and <paramref name="right" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GreaterThanOrEqualAny<T>(Vector64<T> left, Vector64<T> right)
        {
            for (int index = 0; index < Vector64<T>.Count; index++)
            {
                if (Scalar<T>.GreaterThanOrEqual(left.GetElementUnsafe(index), right.GetElementUnsafe(index)))
                {
                    return true;
                }
            }

            return false;
        }

        internal static Vector64<T> Hypot<T>(Vector64<T> x, Vector64<T> y)
            where T : IRootFunctions<T>
        {
            Unsafe.SkipInit(out Vector64<T> result);

            for (int index = 0; index < Vector64<T>.Count; index++)
            {
                T value = T.Hypot(x.GetElementUnsafe(index), y.GetElementUnsafe(index));
                result.SetElementUnsafe(index, value);
            }

            return result;
        }

        /// <summary>Computes the hypotenuse given two vectors representing the lengths of the shorter sides in a right-angled triangle.</summary>
        /// <param name="x">The vector to square and add to <paramref name="y" />.</param>
        /// <param name="y">The vector to square and add to <paramref name="x" />.</param>
        /// <returns>The square root of <paramref name="x" />-squared plus <paramref name="y" />-squared.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<double> Hypot(Vector64<double> x, Vector64<double> y)
        {
            if (IsHardwareAccelerated)
            {
                return VectorMath.HypotDouble<Vector64<double>, Vector64<ulong>>(x, y);
            }
            else
            {
                return Hypot<double>(x, y);
            }
        }

        /// <summary>Computes the hypotenuse given two vectors representing the lengths of the shorter sides in a right-angled triangle.</summary>
        /// <param name="x">The vector to square and add to <paramref name="y" />.</param>
        /// <param name="y">The vector to square and add to <paramref name="x" />.</param>
        /// <returns>The square root of <paramref name="x" />-squared plus <paramref name="y" />-squared.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<float> Hypot(Vector64<float> x, Vector64<float> y)
        {
            if (IsHardwareAccelerated)
            {
                if (Vector128.IsHardwareAccelerated)
                {
                    return VectorMath.HypotSingle<Vector64<float>, Vector128<double>>(x, y);
                }
                else
                {
                    return VectorMath.HypotSingle<Vector64<float>, Vector64<double>>(x, y);
                }
            }
            else
            {
                return Hypot<float>(x, y);
            }
        }

        /// <summary>Determines the index of the first element in a vector that is equal to a given value.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector whose elements are being checked.</param>
        /// <param name="value">The value to check for in <paramref name="vector" /></param>
        /// <returns>The index into <paramref name="vector" /> representing the first element that was equal to <paramref name="value" />; otherwise, <c>-1</c> if no such element exists.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> and <paramref name="value" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(Vector64<T> vector, T value)
        {
            int result = BitOperations.TrailingZeroCount(Equals(vector, Create(value)).ExtractMostSignificantBits());
            return (result != 32) ? result : -1;
        }

        /// <summary>Determines the index of the first element in a vector that has all bits set.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector whose elements are being checked.</param>
        /// <returns>The index into <paramref name="vector" /> representing the first element that had all bits set; otherwise, <c>-1</c> if no such element exists.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfWhereAllBitsSet<T>(Vector64<T> vector)
        {
            if (typeof(T) == typeof(float))
            {
                return IndexOf(vector.AsInt32(), -1);
            }
            else if (typeof(T) == typeof(double))
            {
                return IndexOf(vector.AsInt64(), -1);
            }
            else
            {
                return IndexOf(vector, Scalar<T>.AllBitsSet);
            }
        }

        /// <inheritdoc cref="ISimdVector{TSelf, T}.IsEvenInteger(TSelf)" />
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> IsEvenInteger<T>(Vector64<T> vector)
        {
            if (typeof(T) == typeof(float))
            {
                return VectorMath.IsEvenIntegerSingle<Vector64<float>, Vector64<uint>>(vector.AsSingle()).As<float, T>();
            }
            else if (typeof(T) == typeof(double))
            {
                return VectorMath.IsEvenIntegerDouble<Vector64<double>, Vector64<ulong>>(vector.AsDouble()).As<double, T>();
            }
            return IsZero(vector & Vector64<T>.One);
        }

        /// <inheritdoc cref="ISimdVector{TSelf, T}.IsFinite(TSelf)" />
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> IsFinite<T>(Vector64<T> vector)
        {
            if (typeof(T) == typeof(float))
            {
                return ~IsZero(AndNot(Create<uint>(float.PositiveInfinityBits), vector.AsUInt32())).As<uint, T>();
            }
            else if (typeof(T) == typeof(double))
            {
                return ~IsZero(AndNot(Create<ulong>(double.PositiveInfinityBits), vector.AsUInt64())).As<ulong, T>();
            }
            return Vector64<T>.AllBitsSet;
        }

        /// <inheritdoc cref="ISimdVector{TSelf, T}.IsInfinity(TSelf)" />
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> IsInfinity<T>(Vector64<T> vector)
        {
            if ((typeof(T) == typeof(float)) || (typeof(T) == typeof(double)))
            {
                return IsPositiveInfinity(Abs(vector));
            }
            return Vector64<T>.Zero;
        }

        /// <inheritdoc cref="ISimdVector{TSelf, T}.IsInteger(TSelf)" />
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> IsInteger<T>(Vector64<T> vector)
        {
            if ((typeof(T) == typeof(float)) || (typeof(T) == typeof(double)))
            {
                return IsFinite(vector) & Equals(vector, Truncate(vector));
            }
            return Vector64<T>.AllBitsSet;
        }

        /// <inheritdoc cref="ISimdVector{TSelf, T}.IsNaN(TSelf)" />
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> IsNaN<T>(Vector64<T> vector)
        {
            if ((typeof(T) == typeof(float)) || (typeof(T) == typeof(double)))
            {
                return ~Equals(vector, vector);
            }
            return Vector64<T>.Zero;
        }

        /// <inheritdoc cref="ISimdVector{TSelf, T}.IsNegative(TSelf)" />
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> IsNegative<T>(Vector64<T> vector)
        {
            if ((typeof(T) == typeof(byte))
             || (typeof(T) == typeof(ushort))
             || (typeof(T) == typeof(uint))
             || (typeof(T) == typeof(ulong))
             || (typeof(T) == typeof(nuint)))
            {
                return Vector64<T>.Zero;
            }
            else if (typeof(T) == typeof(float))
            {
                return LessThan(vector.AsInt32(), Vector64<int>.Zero).As<int, T>();
            }
            else if (typeof(T) == typeof(double))
            {
                return LessThan(vector.AsInt64(), Vector64<long>.Zero).As<long, T>();
            }
            else
            {
                return LessThan(vector, Vector64<T>.Zero);
            }
        }

        /// <inheritdoc cref="ISimdVector{TSelf, T}.IsNegativeInfinity(TSelf)" />
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> IsNegativeInfinity<T>(Vector64<T> vector)
        {
            if (typeof(T) == typeof(float))
            {
                return Equals(vector, Create(float.NegativeInfinity).As<float, T>());
            }
            else if (typeof(T) == typeof(double))
            {
                return Equals(vector, Create(double.NegativeInfinity).As<double, T>());
            }
            return Vector64<T>.Zero;
        }

        /// <inheritdoc cref="ISimdVector{TSelf, T}.IsNormal(TSelf)" />
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> IsNormal<T>(Vector64<T> vector)
        {
            if (typeof(T) == typeof(float))
            {
                return LessThan(Abs(vector).AsUInt32() - Create<uint>(float.SmallestNormalBits), Create<uint>(float.PositiveInfinityBits - float.SmallestNormalBits)).As<uint, T>();
            }
            else if (typeof(T) == typeof(double))
            {
                return LessThan(Abs(vector).AsUInt64() - Create<ulong>(double.SmallestNormalBits), Create<ulong>(double.PositiveInfinityBits - double.SmallestNormalBits)).As<ulong, T>();
            }
            return ~IsZero(vector);
        }

        /// <inheritdoc cref="ISimdVector{TSelf, T}.IsOddInteger(TSelf)" />
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> IsOddInteger<T>(Vector64<T> vector)
        {
            if (typeof(T) == typeof(float))
            {
                return VectorMath.IsOddIntegerSingle<Vector64<float>, Vector64<uint>>(vector.AsSingle()).As<float, T>();
            }
            else if (typeof(T) == typeof(double))
            {
                return VectorMath.IsOddIntegerDouble<Vector64<double>, Vector64<ulong>>(vector.AsDouble()).As<double, T>();
            }
            return ~IsZero(vector & Vector64<T>.One);
        }

        /// <inheritdoc cref="ISimdVector{TSelf, T}.IsPositive(TSelf)" />
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> IsPositive<T>(Vector64<T> vector)
        {
            if ((typeof(T) == typeof(byte))
             || (typeof(T) == typeof(ushort))
             || (typeof(T) == typeof(uint))
             || (typeof(T) == typeof(ulong))
             || (typeof(T) == typeof(nuint)))
            {
                return Vector64<T>.AllBitsSet;
            }
            else if (typeof(T) == typeof(float))
            {
                return GreaterThanOrEqual(vector.AsInt32(), Vector64<int>.Zero).As<int, T>();
            }
            else if (typeof(T) == typeof(double))
            {
                return GreaterThanOrEqual(vector.AsInt64(), Vector64<long>.Zero).As<long, T>();
            }
            else
            {
                return GreaterThanOrEqual(vector, Vector64<T>.Zero);
            }
        }

        /// <inheritdoc cref="ISimdVector{TSelf, T}.IsPositiveInfinity(TSelf)" />
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> IsPositiveInfinity<T>(Vector64<T> vector)
        {
            if (typeof(T) == typeof(float))
            {
                return Equals(vector, Create(float.PositiveInfinity).As<float, T>());
            }
            else if (typeof(T) == typeof(double))
            {
                return Equals(vector, Create(double.PositiveInfinity).As<double, T>());
            }
            return Vector64<T>.Zero;
        }

        /// <inheritdoc cref="ISimdVector{TSelf, T}.IsSubnormal(TSelf)" />
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> IsSubnormal<T>(Vector64<T> vector)
        {
            if (typeof(T) == typeof(float))
            {
                return LessThan(Abs(vector).AsUInt32() - Vector64<uint>.One, Create<uint>(float.MaxTrailingSignificand)).As<uint, T>();
            }
            else if (typeof(T) == typeof(double))
            {
                return LessThan(Abs(vector).AsUInt64() - Vector64<ulong>.One, Create<ulong>(double.MaxTrailingSignificand)).As<ulong, T>();
            }
            return Vector64<T>.Zero;
        }

        /// <inheritdoc cref="ISimdVector{TSelf, T}.IsZero(TSelf)" />
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> IsZero<T>(Vector64<T> vector) => Equals(vector, Vector64<T>.Zero);

        /// <summary>Determines the index of the last element in a vector that is equal to a given value.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector whose elements are being checked.</param>
        /// <param name="value">The value to check for in <paramref name="vector" /></param>
        /// <returns>The index into <paramref name="vector" /> representing the last element that was equal to <paramref name="value" />; otherwise, <c>-1</c> if no such element exists.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> and <paramref name="value" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LastIndexOf<T>(Vector64<T> vector, T value) => 31 - BitOperations.LeadingZeroCount(Equals(vector, Create(value)).ExtractMostSignificantBits());

        /// <summary>Determines the index of the last element in a vector that has all bits set.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector whose elements are being checked.</param>
        /// <returns>The index into <paramref name="vector" /> representing the last element that had all bits set; otherwise, <c>-1</c> if no such element exists.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LastIndexOfWhereAllBitsSet<T>(Vector64<T> vector)
        {
            if (typeof(T) == typeof(float))
            {
                return LastIndexOf(vector.AsInt32(), -1);
            }
            else if (typeof(T) == typeof(double))
            {
                return LastIndexOf(vector.AsInt64(), -1);
            }
            else
            {
                return LastIndexOf(vector, Scalar<T>.AllBitsSet);
            }
        }

        internal static Vector64<T> Lerp<T>(Vector64<T> x, Vector64<T> y, Vector64<T> amount)
            where T : IFloatingPointIeee754<T>
        {
            Unsafe.SkipInit(out Vector64<T> result);

            for (int index = 0; index < Vector64<T>.Count; index++)
            {
                T value = T.Lerp(x.GetElementUnsafe(index), y.GetElementUnsafe(index), amount.GetElementUnsafe(index));
                result.SetElementUnsafe(index, value);
            }

            return result;
        }

        /// <summary>Performs a linear interpolation between two vectors based on the given weighting.</summary>
        /// <param name="x">The first vector.</param>
        /// <param name="y">The second vector.</param>
        /// <param name="amount">A value between 0 and 1 that indicates the weight of <paramref name="y" />.</param>
        /// <returns>The interpolated vector.</returns>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<double> Lerp(Vector64<double> x, Vector64<double> y, Vector64<double> amount)
        {
            if (IsHardwareAccelerated)
            {
                return VectorMath.Lerp<Vector64<double>, double>(x, y, amount);
            }
            else
            {
                return Lerp<double>(x, y, amount);
            }
        }

        /// <summary>Performs a linear interpolation between two vectors based on the given weighting.</summary>
        /// <param name="x">The first vector.</param>
        /// <param name="y">The second vector.</param>
        /// <param name="amount">A value between 0 and 1 that indicates the weight of <paramref name="y" />.</param>
        /// <returns>The interpolated vector.</returns>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<float> Lerp(Vector64<float> x, Vector64<float> y, Vector64<float> amount)
        {
            if (IsHardwareAccelerated)
            {
                return VectorMath.Lerp<Vector64<float>, float>(x, y, amount);
            }
            else
            {
                return Lerp<float>(x, y, amount);
            }
        }

        /// <summary>Compares two vectors to determine which is less on a per-element basis.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="left">The vector to compare with <paramref name="left" />.</param>
        /// <param name="right">The vector to compare with <paramref name="right" />.</param>
        /// <returns>A vector whose elements are all-bits-set or zero, depending on if which of the corresponding elements in <paramref name="left" /> and <paramref name="right" /> were less.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="left" /> and <paramref name="right" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> LessThan<T>(Vector64<T> left, Vector64<T> right)
        {
            Unsafe.SkipInit(out Vector64<T> result);

            for (int index = 0; index < Vector64<T>.Count; index++)
            {
                T value = Scalar<T>.LessThan(left.GetElementUnsafe(index), right.GetElementUnsafe(index)) ? Scalar<T>.AllBitsSet : default!;
                result.SetElementUnsafe(index, value);
            }

            return result;
        }

        /// <summary>Compares two vectors to determine if all elements are less.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="left">The vector to compare with <paramref name="right" />.</param>
        /// <param name="right">The vector to compare with <paramref name="left" />.</param>
        /// <returns><c>true</c> if all elements in <paramref name="left" /> were less than the corresponding element in <paramref name="right" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="left" /> and <paramref name="right" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LessThanAll<T>(Vector64<T> left, Vector64<T> right)
        {
            for (int index = 0; index < Vector64<T>.Count; index++)
            {
                if (!Scalar<T>.LessThan(left.GetElementUnsafe(index), right.GetElementUnsafe(index)))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Compares two vectors to determine if any elements are less.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="left">The vector to compare with <paramref name="right" />.</param>
        /// <param name="right">The vector to compare with <paramref name="left" />.</param>
        /// <returns><c>true</c> if any elements in <paramref name="left" /> was less than the corresponding element in <paramref name="right" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="left" /> and <paramref name="right" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LessThanAny<T>(Vector64<T> left, Vector64<T> right)
        {
            for (int index = 0; index < Vector64<T>.Count; index++)
            {
                if (Scalar<T>.LessThan(left.GetElementUnsafe(index), right.GetElementUnsafe(index)))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Compares two vectors to determine which is less or equal on a per-element basis.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="left">The vector to compare with <paramref name="left" />.</param>
        /// <param name="right">The vector to compare with <paramref name="right" />.</param>
        /// <returns>A vector whose elements are all-bits-set or zero, depending on if which of the corresponding elements in <paramref name="left" /> and <paramref name="right" /> were less or equal.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="left" /> and <paramref name="right" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> LessThanOrEqual<T>(Vector64<T> left, Vector64<T> right)
        {
            Unsafe.SkipInit(out Vector64<T> result);

            for (int index = 0; index < Vector64<T>.Count; index++)
            {
                T value = Scalar<T>.LessThanOrEqual(left.GetElementUnsafe(index), right.GetElementUnsafe(index)) ? Scalar<T>.AllBitsSet : default!;
                result.SetElementUnsafe(index, value);
            }

            return result;
        }

        /// <summary>Compares two vectors to determine if all elements are less or equal.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="left">The vector to compare with <paramref name="right" />.</param>
        /// <param name="right">The vector to compare with <paramref name="left" />.</param>
        /// <returns><c>true</c> if all elements in <paramref name="left" /> were less than or equal to the corresponding element in <paramref name="right" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="left" /> and <paramref name="right" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LessThanOrEqualAll<T>(Vector64<T> left, Vector64<T> right)
        {
            for (int index = 0; index < Vector64<T>.Count; index++)
            {
                if (!Scalar<T>.LessThanOrEqual(left.GetElementUnsafe(index), right.GetElementUnsafe(index)))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Compares two vectors to determine if any elements are less or equal.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="left">The vector to compare with <paramref name="right" />.</param>
        /// <param name="right">The vector to compare with <paramref name="left" />.</param>
        /// <returns><c>true</c> if any elements in <paramref name="left" /> was less than or equal to the corresponding element in <paramref name="right" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="left" /> and <paramref name="right" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LessThanOrEqualAny<T>(Vector64<T> left, Vector64<T> right)
        {
            for (int index = 0; index < Vector64<T>.Count; index++)
            {
                if (Scalar<T>.LessThanOrEqual(left.GetElementUnsafe(index), right.GetElementUnsafe(index)))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Loads a vector from the given source.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="source">The source from which the vector will be loaded.</param>
        /// <returns>The vector loaded from <paramref name="source" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="source" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [CLSCompliant(false)]
        public static unsafe Vector64<T> Load<T>(T* source) => LoadUnsafe(ref *source);

        /// <summary>Loads a vector from the given aligned source.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="source">The aligned source from which the vector will be loaded.</param>
        /// <returns>The vector loaded from <paramref name="source" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="source" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Vector64<T> LoadAligned<T>(T* source)
        {
            ThrowHelper.ThrowForUnsupportedIntrinsicsVector64BaseType<T>();

            if (((nuint)(source) % Alignment) != 0)
            {
                ThrowHelper.ThrowAccessViolationException();
            }

            return *(Vector64<T>*)source;
        }

        /// <summary>Loads a vector from the given aligned source.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="source">The aligned source from which the vector will be loaded.</param>
        /// <returns>The vector loaded from <paramref name="source" />.</returns>
        /// <remarks>This method may bypass the cache on certain platforms.</remarks>
        /// <exception cref="NotSupportedException">The type of <paramref name="source" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [CLSCompliant(false)]
        public static unsafe Vector64<T> LoadAlignedNonTemporal<T>(T* source) => LoadAligned(source);

        /// <summary>Loads a vector from the given source.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="source">The source from which the vector will be loaded.</param>
        /// <returns>The vector loaded from <paramref name="source" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="source" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> LoadUnsafe<T>(ref readonly T source)
        {
            ThrowHelper.ThrowForUnsupportedIntrinsicsVector64BaseType<T>();
            ref readonly byte address = ref Unsafe.As<T, byte>(ref Unsafe.AsRef(in source));
            return Unsafe.ReadUnaligned<Vector64<T>>(in address);
        }

        /// <summary>Loads a vector from the given source and element offset.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="source">The source to which <paramref name="elementOffset" /> will be added before loading the vector.</param>
        /// <param name="elementOffset">The element offset from <paramref name="source" /> from which the vector will be loaded.</param>
        /// <returns>The vector loaded from <paramref name="source" /> plus <paramref name="elementOffset" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="source" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> LoadUnsafe<T>(ref readonly T source, nuint elementOffset)
        {
            ThrowHelper.ThrowForUnsupportedIntrinsicsVector64BaseType<T>();
            ref readonly byte address = ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef(in source), (nint)elementOffset));
            return Unsafe.ReadUnaligned<Vector64<T>>(in address);
        }

        internal static Vector64<T> Log<T>(Vector64<T> vector)
            where T : ILogarithmicFunctions<T>
        {
            Unsafe.SkipInit(out Vector64<T> result);

            for (int index = 0; index < Vector64<T>.Count; index++)
            {
                T value = T.Log(vector.GetElementUnsafe(index));
                result.SetElementUnsafe(index, value);
            }

            return result;
        }

        /// <summary>Computes the log of each element in a vector.</summary>
        /// <param name="vector">The vector that will have its log computed.</param>
        /// <returns>A vector whose elements are the log of the elements in <paramref name="vector" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<double> Log(Vector64<double> vector)
        {
            if (IsHardwareAccelerated)
            {
                return VectorMath.LogDouble<Vector64<double>, Vector64<long>, Vector64<ulong>>(vector);
            }
            else
            {
                return Log<double>(vector);
            }
        }

        /// <summary>Computes the log of each element in a vector.</summary>
        /// <param name="vector">The vector that will have its log computed.</param>
        /// <returns>A vector whose elements are the log of the elements in <paramref name="vector" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<float> Log(Vector64<float> vector)
        {
            if (IsHardwareAccelerated)
            {
                return VectorMath.LogSingle<Vector64<float>, Vector64<int>, Vector64<uint>>(vector);
            }
            else
            {
                return Log<float>(vector);
            }
        }

        internal static Vector64<T> Log2<T>(Vector64<T> vector)
            where T : ILogarithmicFunctions<T>
        {
            Unsafe.SkipInit(out Vector64<T> result);

            for (int index = 0; index < Vector64<T>.Count; index++)
            {
                T value = T.Log2(vector.GetElementUnsafe(index));
                result.SetElementUnsafe(index, value);
            }

            return result;
        }

        /// <summary>Computes the log2 of each element in a vector.</summary>
        /// <param name="vector">The vector that will have its log2 computed.</param>
        /// <returns>A vector whose elements are the log2 of the elements in <paramref name="vector" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<double> Log2(Vector64<double> vector)
        {
            if (IsHardwareAccelerated)
            {
                return VectorMath.Log2Double<Vector64<double>, Vector64<long>, Vector64<ulong>>(vector);
            }
            else
            {
                return Log2<double>(vector);
            }
        }

        /// <summary>Computes the log2 of each element in a vector.</summary>
        /// <param name="vector">The vector that will have its log2 computed.</param>
        /// <returns>A vector whose elements are the log2 of the elements in <paramref name="vector" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<float> Log2(Vector64<float> vector)
        {
            if (IsHardwareAccelerated)
            {
                return VectorMath.Log2Single<Vector64<float>, Vector64<int>, Vector64<uint>>(vector);
            }
            else
            {
                return Log2<float>(vector);
            }
        }

        /// <inheritdoc cref="ISimdVector{TSelf, T}.Max(TSelf, TSelf)" />
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> Max<T>(Vector64<T> left, Vector64<T> right)
        {
            if (IsHardwareAccelerated)
            {
                return VectorMath.Max<Vector64<T>, T>(left, right);
            }
            else
            {
                Unsafe.SkipInit(out Vector64<T> result);

                for (int index = 0; index < Vector64<T>.Count; index++)
                {
                    T value = Scalar<T>.Max(left.GetElementUnsafe(index), right.GetElementUnsafe(index));
                    result.SetElementUnsafe(index, value);
                }

                return result;
            }
        }

        /// <inheritdoc cref="ISimdVector{TSelf, T}.MaxMagnitude(TSelf, TSelf)" />
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> MaxMagnitude<T>(Vector64<T> left, Vector64<T> right)
        {
            if (IsHardwareAccelerated)
            {
                return VectorMath.MaxMagnitude<Vector64<T>, T>(left, right);
            }
            else
            {
                Unsafe.SkipInit(out Vector64<T> result);

                for (int index = 0; index < Vector64<T>.Count; index++)
                {
                    T value = Scalar<T>.MaxMagnitude(left.GetElementUnsafe(index), right.GetElementUnsafe(index));
                    result.SetElementUnsafe(index, value);
                }

                return result;
            }
        }

        /// <inheritdoc cref="ISimdVector{TSelf, T}.MaxMagnitudeNumber(TSelf, TSelf)" />
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> MaxMagnitudeNumber<T>(Vector64<T> left, Vector64<T> right)
        {
            if (IsHardwareAccelerated)
            {
                return VectorMath.MaxMagnitudeNumber<Vector64<T>, T>(left, right);
            }
            else
            {
                Unsafe.SkipInit(out Vector64<T> result);

                for (int index = 0; index < Vector64<T>.Count; index++)
                {
                    T value = Scalar<T>.MaxMagnitudeNumber(left.GetElementUnsafe(index), right.GetElementUnsafe(index));
                    result.SetElementUnsafe(index, value);
                }

                return result;
            }
        }

        /// <inheritdoc cref="ISimdVector{TSelf, T}.MaxNative(TSelf, TSelf)" />
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> MaxNative<T>(Vector64<T> left, Vector64<T> right)
        {
            if (IsHardwareAccelerated)
            {
                return ConditionalSelect(GreaterThan(left, right), left, right);
            }
            else
            {
                Unsafe.SkipInit(out Vector64<T> result);

                for (int index = 0; index < Vector64<T>.Count; index++)
                {
                    T value = Scalar<T>.GreaterThan(left.GetElementUnsafe(index), right.GetElementUnsafe(index)) ? left.GetElementUnsafe(index) : right.GetElementUnsafe(index);
                    result.SetElementUnsafe(index, value);
                }

                return result;
            }
        }

        /// <inheritdoc cref="ISimdVector{TSelf, T}.MaxNumber(TSelf, TSelf)" />
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> MaxNumber<T>(Vector64<T> left, Vector64<T> right)
        {
            if (IsHardwareAccelerated)
            {
                return VectorMath.MaxNumber<Vector64<T>, T>(left, right);
            }
            else
            {
                Unsafe.SkipInit(out Vector64<T> result);

                for (int index = 0; index < Vector64<T>.Count; index++)
                {
                    T value = Scalar<T>.MaxNumber(left.GetElementUnsafe(index), right.GetElementUnsafe(index));
                    result.SetElementUnsafe(index, value);
                }

                return result;
            }
        }

        /// <inheritdoc cref="ISimdVector{TSelf, T}.Min(TSelf, TSelf)" />
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> Min<T>(Vector64<T> left, Vector64<T> right)
        {
            if (IsHardwareAccelerated)
            {
                return VectorMath.Min<Vector64<T>, T>(left, right);
            }
            else
            {
                Unsafe.SkipInit(out Vector64<T> result);

                for (int index = 0; index < Vector64<T>.Count; index++)
                {
                    T value = Scalar<T>.Min(left.GetElementUnsafe(index), right.GetElementUnsafe(index));
                    result.SetElementUnsafe(index, value);
                }

                return result;
            }
        }

        /// <inheritdoc cref="ISimdVector{TSelf, T}.MinMagnitude(TSelf, TSelf)" />
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> MinMagnitude<T>(Vector64<T> left, Vector64<T> right)
        {
            if (IsHardwareAccelerated)
            {
                return VectorMath.MinMagnitude<Vector64<T>, T>(left, right);
            }
            else
            {
                Unsafe.SkipInit(out Vector64<T> result);

                for (int index = 0; index < Vector64<T>.Count; index++)
                {
                    T value = Scalar<T>.MinMagnitude(left.GetElementUnsafe(index), right.GetElementUnsafe(index));
                    result.SetElementUnsafe(index, value);
                }

                return result;
            }
        }

        /// <inheritdoc cref="ISimdVector{TSelf, T}.MinMagnitudeNumber(TSelf, TSelf)" />
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> MinMagnitudeNumber<T>(Vector64<T> left, Vector64<T> right)
        {
            if (IsHardwareAccelerated)
            {
                return VectorMath.MinMagnitudeNumber<Vector64<T>, T>(left, right);
            }
            else
            {
                Unsafe.SkipInit(out Vector64<T> result);

                for (int index = 0; index < Vector64<T>.Count; index++)
                {
                    T value = Scalar<T>.MinMagnitudeNumber(left.GetElementUnsafe(index), right.GetElementUnsafe(index));
                    result.SetElementUnsafe(index, value);
                }

                return result;
            }
        }

        /// <inheritdoc cref="ISimdVector{TSelf, T}.MinNative(TSelf, TSelf)" />
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> MinNative<T>(Vector64<T> left, Vector64<T> right)
        {
            if (IsHardwareAccelerated)
            {
                return ConditionalSelect(LessThan(left, right), left, right);
            }
            else
            {
                Unsafe.SkipInit(out Vector64<T> result);

                for (int index = 0; index < Vector64<T>.Count; index++)
                {
                    T value = Scalar<T>.LessThan(left.GetElementUnsafe(index), right.GetElementUnsafe(index)) ? left.GetElementUnsafe(index) : right.GetElementUnsafe(index);
                    result.SetElementUnsafe(index, value);
                }

                return result;
            }
        }

        /// <inheritdoc cref="ISimdVector{TSelf, T}.MinNumber(TSelf, TSelf)" />
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> MinNumber<T>(Vector64<T> left, Vector64<T> right)
        {
            if (IsHardwareAccelerated)
            {
                return VectorMath.MinNumber<Vector64<T>, T>(left, right);
            }
            else
            {
                Unsafe.SkipInit(out Vector64<T> result);

                for (int index = 0; index < Vector64<T>.Count; index++)
                {
                    T value = Scalar<T>.MinNumber(left.GetElementUnsafe(index), right.GetElementUnsafe(index));
                    result.SetElementUnsafe(index, value);
                }

                return result;
            }
        }

        /// <summary>Multiplies two vectors to compute their element-wise product.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="left">The vector to multiply with <paramref name="right" />.</param>
        /// <param name="right">The vector to multiply with <paramref name="left" />.</param>
        /// <returns>The element-wise product of <paramref name="left" /> and <paramref name="right" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="left" /> and <paramref name="right" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        public static Vector64<T> Multiply<T>(Vector64<T> left, Vector64<T> right) => left * right;

        /// <summary>Multiplies a vector by a scalar to compute their product.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="left">The vector to multiply with <paramref name="right" />.</param>
        /// <param name="right">The scalar to multiply with <paramref name="left" />.</param>
        /// <returns>The product of <paramref name="left" /> and <paramref name="right" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="left" /> and <paramref name="right" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        public static Vector64<T> Multiply<T>(Vector64<T> left, T right) => left * right;

        /// <summary>Multiplies a vector by a scalar to compute their product.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="left">The scalar to multiply with <paramref name="right" />.</param>
        /// <param name="right">The vector to multiply with <paramref name="left" />.</param>
        /// <returns>The product of <paramref name="left" /> and <paramref name="right" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="left" /> and <paramref name="right" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        public static Vector64<T> Multiply<T>(T left, Vector64<T> right) => right * left;

        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector64<T> MultiplyAddEstimate<T>(Vector64<T> left, Vector64<T> right, Vector64<T> addend)
        {
            Unsafe.SkipInit(out Vector64<T> result);

            for (int index = 0; index < Vector64<T>.Count; index++)
            {
                T value = Scalar<T>.MultiplyAddEstimate(left.GetElementUnsafe(index), right.GetElementUnsafe(index), addend.GetElementUnsafe(index));
                result.SetElementUnsafe(index, value);
            }

            return result;
        }

        /// <summary>Computes an estimate of (<paramref name="left" /> * <paramref name="right" />) + <paramref name="addend" />.</summary>
        /// <param name="left">The vector to be multiplied with <paramref name="right" />.</param>
        /// <param name="right">The vector to be multiplied with <paramref name="left" />.</param>
        /// <param name="addend">The vector to be added to the result of <paramref name="left" /> multiplied by <paramref name="right" />.</param>
        /// <returns>An estimate of (<paramref name="left" /> * <paramref name="right" />) + <paramref name="addend" />.</returns>
        /// <remarks>
        ///   <para>On hardware that natively supports <see cref="FusedMultiplyAdd" />, this may return a result that was rounded as one ternary operation.</para>
        ///   <para>On hardware without specialized support, this may just return (<paramref name="left" /> * <paramref name="right" />) + <paramref name="addend" />.</para>
        /// </remarks>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<double> MultiplyAddEstimate(Vector64<double> left, Vector64<double> right, Vector64<double> addend)
        {
            Unsafe.SkipInit(out Vector64<double> result);

            for (int index = 0; index < Vector64<double>.Count; index++)
            {
                double element = double.MultiplyAddEstimate(left.GetElementUnsafe(index), right.GetElementUnsafe(index), addend.GetElementUnsafe(index));
                result.SetElementUnsafe(index, element);
            }

            return result;
        }

        /// <summary>Computes an estimate of (<paramref name="left" /> * <paramref name="right" />) + <paramref name="addend" />.</summary>
        /// <param name="left">The vector to be multiplied with <paramref name="right" />.</param>
        /// <param name="right">The vector to be multiplied with <paramref name="left" />.</param>
        /// <param name="addend">The vector to be added to the result of <paramref name="left" /> multiplied by <paramref name="right" />.</param>
        /// <returns>An estimate of (<paramref name="left" /> * <paramref name="right" />) + <paramref name="addend" />.</returns>
        /// <remarks>
        ///   <para>On hardware that natively supports <see cref="FusedMultiplyAdd" />, this may return a result that was rounded as one ternary operation.</para>
        ///   <para>On hardware without specialized support, this may just return (<paramref name="left" /> * <paramref name="right" />) + <paramref name="addend" />.</para>
        /// </remarks>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<float> MultiplyAddEstimate(Vector64<float> left, Vector64<float> right, Vector64<float> addend)
        {
            Unsafe.SkipInit(out Vector64<float> result);

            for (int index = 0; index < Vector64<float>.Count; index++)
            {
                float element = float.MultiplyAddEstimate(left.GetElementUnsafe(index), right.GetElementUnsafe(index), addend.GetElementUnsafe(index));
                result.SetElementUnsafe(index, element);
            }

            return result;
        }

        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector64<TResult> Narrow<TSource, TResult>(Vector64<TSource> lower, Vector64<TSource> upper)
            where TSource : INumber<TSource>
            where TResult : INumber<TResult>
        {
            Unsafe.SkipInit(out Vector64<TResult> result);

            for (int i = 0; i < Vector64<TSource>.Count; i++)
            {
                TResult value = TResult.CreateTruncating(lower.GetElementUnsafe(i));
                result.SetElementUnsafe(i, value);
            }

            for (int i = Vector64<TSource>.Count; i < Vector64<TResult>.Count; i++)
            {
                TResult value = TResult.CreateTruncating(upper.GetElementUnsafe(i - Vector64<TSource>.Count));
                result.SetElementUnsafe(i, value);
            }

            return result;
        }

        /// <inheritdoc cref="Vector128.Narrow(Vector128{double}, Vector128{double})"/>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<float> Narrow(Vector64<double> lower, Vector64<double> upper)
            => Narrow<double, float>(lower, upper);

        /// <inheritdoc cref="Vector128.Narrow(Vector128{short}, Vector128{short})"/>
        [Intrinsic]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<sbyte> Narrow(Vector64<short> lower, Vector64<short> upper)
            => Narrow<short, sbyte>(lower, upper);

        /// <inheritdoc cref="Vector128.Narrow(Vector128{int}, Vector128{int})"/>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<short> Narrow(Vector64<int> lower, Vector64<int> upper)
            => Narrow<int, short>(lower, upper);

        /// <inheritdoc cref="Vector128.Narrow(Vector128{long}, Vector128{long})"/>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<int> Narrow(Vector64<long> lower, Vector64<long> upper)
            => Narrow<long, int>(lower, upper);

        /// <inheritdoc cref="Vector128.Narrow(Vector128{ushort}, Vector128{ushort})"/>
        [Intrinsic]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<byte> Narrow(Vector64<ushort> lower, Vector64<ushort> upper)
            => Narrow<ushort, byte>(lower, upper);

        /// <inheritdoc cref="Vector128.Narrow(Vector128{uint}, Vector128{uint})"/>
        [Intrinsic]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<ushort> Narrow(Vector64<uint> lower, Vector64<uint> upper)
            => Narrow<uint, ushort>(lower, upper);

        /// <inheritdoc cref="Vector128.Narrow(Vector128{ulong}, Vector128{ulong})"/>
        [Intrinsic]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<uint> Narrow(Vector64<ulong> lower, Vector64<ulong> upper)
            => Narrow<ulong, uint>(lower, upper);

        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector64<TResult> NarrowWithSaturation<TSource, TResult>(Vector64<TSource> lower, Vector64<TSource> upper)
            where TSource : INumber<TSource>
            where TResult : INumber<TResult>
        {
            Unsafe.SkipInit(out Vector64<TResult> result);

            for (int i = 0; i < Vector64<TSource>.Count; i++)
            {
                TResult value = TResult.CreateSaturating(lower.GetElementUnsafe(i));
                result.SetElementUnsafe(i, value);
            }

            for (int i = Vector64<TSource>.Count; i < Vector64<TResult>.Count; i++)
            {
                TResult value = TResult.CreateSaturating(upper.GetElementUnsafe(i - Vector64<TSource>.Count));
                result.SetElementUnsafe(i, value);
            }

            return result;
        }

        /// <inheritdoc cref="Vector128.NarrowWithSaturation(Vector128{double}, Vector128{double})"/>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<float> NarrowWithSaturation(Vector64<double> lower, Vector64<double> upper)
            => NarrowWithSaturation<double, float>(lower, upper);

        /// <inheritdoc cref="Vector128.NarrowWithSaturation(Vector128{short}, Vector128{short})"/>
        [Intrinsic]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<sbyte> NarrowWithSaturation(Vector64<short> lower, Vector64<short> upper)
            => NarrowWithSaturation<short, sbyte>(lower, upper);

        /// <inheritdoc cref="Vector128.NarrowWithSaturation(Vector128{int}, Vector128{int})"/>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<short> NarrowWithSaturation(Vector64<int> lower, Vector64<int> upper)
            => NarrowWithSaturation<int, short>(lower, upper);

        /// <inheritdoc cref="Vector128.NarrowWithSaturation(Vector128{long}, Vector128{long})"/>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<int> NarrowWithSaturation(Vector64<long> lower, Vector64<long> upper)
            => NarrowWithSaturation<long, int>(lower, upper);

        /// <inheritdoc cref="Vector128.NarrowWithSaturation(Vector128{ushort}, Vector128{ushort})"/>
        [Intrinsic]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<byte> NarrowWithSaturation(Vector64<ushort> lower, Vector64<ushort> upper)
            => NarrowWithSaturation<ushort, byte>(lower, upper);

        /// <inheritdoc cref="Vector128.NarrowWithSaturation(Vector128{uint}, Vector128{uint})"/>
        [Intrinsic]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<ushort> NarrowWithSaturation(Vector64<uint> lower, Vector64<uint> upper)
            => NarrowWithSaturation<uint, ushort>(lower, upper);

        /// <inheritdoc cref="Vector128.NarrowWithSaturation(Vector128{ulong}, Vector128{ulong})"/>
        [Intrinsic]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<uint> NarrowWithSaturation(Vector64<ulong> lower, Vector64<ulong> upper)
            => NarrowWithSaturation<ulong, uint>(lower, upper);

        /// <summary>Negates a vector.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector to negate.</param>
        /// <returns>A vector whose elements are the negation of the corresponding elements in <paramref name="vector" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        public static Vector64<T> Negate<T>(Vector64<T> vector) => -vector;

        /// <summary>Determines if no elements of a vector are equal to a given value.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector whose elements are being checked.</param>
        /// <param name="value">The value to check for in <paramref name="vector" /></param>
        /// <returns><c>true</c> if no elements of <paramref name="vector" /> are equal to <paramref name="value" />; otherwise, <c>false</c>.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> and <paramref name="value" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool None<T>(Vector64<T> vector, T value) => !EqualsAny(vector, Create(value));

        /// <summary>Determines if no elements of a vector have all their bits set.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector whose elements are being checked.</param>
        /// <returns><c>true</c> if no elements of <paramref name="vector" /> have all their bits set; otherwise, <c>false</c>.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" />(<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NoneWhereAllBitsSet<T>(Vector64<T> vector)
        {
            if (typeof(T) == typeof(float))
            {
                return None(vector.AsInt32(), -1);
            }
            else if (typeof(T) == typeof(double))
            {
                return None(vector.AsInt64(), -1);
            }
            else
            {
                return None(vector, Scalar<T>.AllBitsSet);
            }
        }

        /// <summary>Computes the ones-complement of a vector.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector whose ones-complement is to be computed.</param>
        /// <returns>A vector whose elements are the ones-complement of the corresponding elements in <paramref name="vector" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        public static Vector64<T> OnesComplement<T>(Vector64<T> vector) => ~vector;

        internal static Vector64<T> RadiansToDegrees<T>(Vector64<T> radians)
            where T : ITrigonometricFunctions<T>
        {
            Unsafe.SkipInit(out Vector64<T> result);

            for (int index = 0; index < Vector64<T>.Count; index++)
            {
                T value = T.RadiansToDegrees(radians.GetElementUnsafe(index));
                result.SetElementUnsafe(index, value);
            }

            return result;
        }

        /// <summary>Converts a given vector from radians to degrees.</summary>
        /// <param name="radians">The vector to convert to degrees.</param>
        /// <returns>The vector of <paramref name="radians" /> converted to degrees.</returns>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<double> RadiansToDegrees(Vector64<double> radians)
        {
            if (IsHardwareAccelerated)
            {
                return VectorMath.RadiansToDegrees<Vector64<double>, double>(radians);
            }
            else
            {
                return RadiansToDegrees<double>(radians);
            }
        }

        /// <summary>Converts a given vector from radians to degrees.</summary>
        /// <param name="radians">The vector to convert to degrees.</param>
        /// <returns>The vector of <paramref name="radians" /> converted to degrees.</returns>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<float> RadiansToDegrees(Vector64<float> radians)
        {
            if (IsHardwareAccelerated)
            {
                return VectorMath.RadiansToDegrees<Vector64<float>, float>(radians);
            }
            else
            {
                return RadiansToDegrees<float>(radians);
            }
        }

        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector64<T> Round<T>(Vector64<T> vector)
        {
            if ((typeof(T) == typeof(byte))
             || (typeof(T) == typeof(short))
             || (typeof(T) == typeof(int))
             || (typeof(T) == typeof(long))
             || (typeof(T) == typeof(nint))
             || (typeof(T) == typeof(nuint))
             || (typeof(T) == typeof(sbyte))
             || (typeof(T) == typeof(ushort))
             || (typeof(T) == typeof(uint))
             || (typeof(T) == typeof(ulong)))
            {
                return vector;
            }
            else
            {
                Unsafe.SkipInit(out Vector64<T> result);

                for (int index = 0; index < Vector64<T>.Count; index++)
                {
                    T value = Scalar<T>.Round(vector.GetElementUnsafe(index));
                    result.SetElementUnsafe(index, value);
                }

                return result;
            }
        }

        /// <inheritdoc cref="ISimdVector{TSelf, T}.Round(TSelf)" />
        [Intrinsic]
        public static Vector64<double> Round(Vector64<double> vector) => Round<double>(vector);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.Round(TSelf)" />
        [Intrinsic]
        public static Vector64<float> Round(Vector64<float> vector) => Round<float>(vector);

        /// <summary>Rounds each element in a vector to the nearest integer using the specified rounding mode.</summary>
        /// <param name="vector">The vector to round.</param>
        /// <param name="mode">The mode under which <paramref name="vector" /> should be rounded.</param>
        /// <returns>The result of rounding each element in <paramref name="vector" /> to the nearest integer using <paramref name="mode" />.</returns>
        [Intrinsic]
        public static Vector64<double> Round(Vector64<double> vector, MidpointRounding mode) => VectorMath.RoundDouble(vector, mode);

        /// <summary>Rounds each element in a vector to the nearest integer using the specified rounding mode.</summary>
        /// <param name="vector">The vector to round.</param>
        /// <param name="mode">The mode under which <paramref name="vector" /> should be rounded.</param>
        /// <returns>The result of rounding each element in <paramref name="vector" /> to the nearest integer using <paramref name="mode" />.</returns>
        [Intrinsic]
        public static Vector64<float> Round(Vector64<float> vector, MidpointRounding mode) => VectorMath.RoundSingle(vector, mode);

        /// <summary>Shifts each element of a vector left by the specified amount.</summary>
        /// <param name="vector">The vector whose elements are to be shifted.</param>
        /// <param name="shiftCount">The number of bits by which to shift each element.</param>
        /// <returns>A vector whose elements where shifted left by <paramref name="shiftCount" />.</returns>
        [Intrinsic]
        internal static Vector64<T> ShiftLeft<T>(Vector64<T> vector, int shiftCount) => vector << shiftCount;

        /// <summary>Shifts each element of a vector left by the specified amount.</summary>
        /// <param name="vector">The vector whose elements are to be shifted.</param>
        /// <param name="shiftCount">The number of bits by which to shift each element.</param>
        /// <returns>A vector whose elements where shifted left by <paramref name="shiftCount" />.</returns>
        [Intrinsic]
        public static Vector64<byte> ShiftLeft(Vector64<byte> vector, int shiftCount) => vector << shiftCount;

        /// <summary>Shifts each element of a vector left by the specified amount.</summary>
        /// <param name="vector">The vector whose elements are to be shifted.</param>
        /// <param name="shiftCount">The number of bits by which to shift each element.</param>
        /// <returns>A vector whose elements where shifted left by <paramref name="shiftCount" />.</returns>
        [Intrinsic]
        public static Vector64<short> ShiftLeft(Vector64<short> vector, int shiftCount) => vector << shiftCount;

        /// <summary>Shifts each element of a vector left by the specified amount.</summary>
        /// <param name="vector">The vector whose elements are to be shifted.</param>
        /// <param name="shiftCount">The number of bits by which to shift each element.</param>
        /// <returns>A vector whose elements where shifted left by <paramref name="shiftCount" />.</returns>
        [Intrinsic]
        public static Vector64<int> ShiftLeft(Vector64<int> vector, int shiftCount) => vector << shiftCount;

        /// <summary>Shifts each element of a vector left by the specified amount.</summary>
        /// <param name="vector">The vector whose elements are to be shifted.</param>
        /// <param name="shiftCount">The number of bits by which to shift each element.</param>
        /// <returns>A vector whose elements where shifted left by <paramref name="shiftCount" />.</returns>
        [Intrinsic]
        public static Vector64<long> ShiftLeft(Vector64<long> vector, int shiftCount) => vector << shiftCount;

        /// <summary>Shifts each element of a vector left by the specified amount.</summary>
        /// <param name="vector">The vector whose elements are to be shifted.</param>
        /// <param name="shiftCount">The number of bits by which to shift each element.</param>
        /// <returns>A vector whose elements where shifted left by <paramref name="shiftCount" />.</returns>
        [Intrinsic]
        public static Vector64<nint> ShiftLeft(Vector64<nint> vector, int shiftCount) => vector << shiftCount;

        /// <summary>Shifts each element of a vector left by the specified amount.</summary>
        /// <param name="vector">The vector whose elements are to be shifted.</param>
        /// <param name="shiftCount">The number of bits by which to shift each element.</param>
        /// <returns>A vector whose elements where shifted left by <paramref name="shiftCount" />.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        public static Vector64<nuint> ShiftLeft(Vector64<nuint> vector, int shiftCount) => vector << shiftCount;

        /// <summary>Shifts each element of a vector left by the specified amount.</summary>
        /// <param name="vector">The vector whose elements are to be shifted.</param>
        /// <param name="shiftCount">The number of bits by which to shift each element.</param>
        /// <returns>A vector whose elements where shifted left by <paramref name="shiftCount" />.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        public static Vector64<sbyte> ShiftLeft(Vector64<sbyte> vector, int shiftCount) => vector << shiftCount;

        /// <summary>Shifts each element of a vector left by the specified amount.</summary>
        /// <param name="vector">The vector whose elements are to be shifted.</param>
        /// <param name="shiftCount">The number of bits by which to shift each element.</param>
        /// <returns>A vector whose elements where shifted left by <paramref name="shiftCount" />.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        public static Vector64<ushort> ShiftLeft(Vector64<ushort> vector, int shiftCount) => vector << shiftCount;

        /// <summary>Shifts each element of a vector left by the specified amount.</summary>
        /// <param name="vector">The vector whose elements are to be shifted.</param>
        /// <param name="shiftCount">The number of bits by which to shift each element.</param>
        /// <returns>A vector whose elements where shifted left by <paramref name="shiftCount" />.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        public static Vector64<uint> ShiftLeft(Vector64<uint> vector, int shiftCount) => vector << shiftCount;

        [Intrinsic]
        internal static Vector64<uint> ShiftLeft(Vector64<uint> vector, Vector64<uint> shiftCount)
        {
            Unsafe.SkipInit(out Vector64<uint> result);

            for (int index = 0; index < Vector64<uint>.Count; index++)
            {
                uint element = vector.GetElementUnsafe(index) << (int)shiftCount.GetElementUnsafe(index);
                result.SetElementUnsafe(index, element);
            }

            return result;
        }

        /// <summary>Shifts each element of a vector left by the specified amount.</summary>
        /// <param name="vector">The vector whose elements are to be shifted.</param>
        /// <param name="shiftCount">The number of bits by which to shift each element.</param>
        /// <returns>A vector whose elements where shifted left by <paramref name="shiftCount" />.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        public static Vector64<ulong> ShiftLeft(Vector64<ulong> vector, int shiftCount) => vector << shiftCount;

        [Intrinsic]
        internal static Vector64<ulong> ShiftLeft(Vector64<ulong> vector, Vector64<ulong> shiftCount)
        {
            Unsafe.SkipInit(out Vector64<ulong> result);

            for (int index = 0; index < Vector64<ulong>.Count; index++)
            {
                ulong element = vector.GetElementUnsafe(index) << (int)shiftCount.GetElementUnsafe(index);
                result.SetElementUnsafe(index, element);
            }

            return result;
        }

        /// <summary>Shifts (signed) each element of a vector right by the specified amount.</summary>
        /// <param name="vector">The vector whose elements are to be shifted.</param>
        /// <param name="shiftCount">The number of bits by which to shift each element.</param>
        /// <returns>A vector whose elements where shifted right by <paramref name="shiftCount" />.</returns>
        [Intrinsic]
        internal static Vector64<T> ShiftRightArithmetic<T>(Vector64<T> vector, int shiftCount) => vector >> shiftCount;

        /// <summary>Shifts (signed) each element of a vector right by the specified amount.</summary>
        /// <param name="vector">The vector whose elements are to be shifted.</param>
        /// <param name="shiftCount">The number of bits by which to shift each element.</param>
        /// <returns>A vector whose elements where shifted right by <paramref name="shiftCount" />.</returns>
        [Intrinsic]
        public static Vector64<short> ShiftRightArithmetic(Vector64<short> vector, int shiftCount) => vector >> shiftCount;

        /// <summary>Shifts (signed) each element of a vector right by the specified amount.</summary>
        /// <param name="vector">The vector whose elements are to be shifted.</param>
        /// <param name="shiftCount">The number of bits by which to shift each element.</param>
        /// <returns>A vector whose elements where shifted right by <paramref name="shiftCount" />.</returns>
        [Intrinsic]
        public static Vector64<int> ShiftRightArithmetic(Vector64<int> vector, int shiftCount) => vector >> shiftCount;

        /// <summary>Shifts (signed) each element of a vector right by the specified amount.</summary>
        /// <param name="vector">The vector whose elements are to be shifted.</param>
        /// <param name="shiftCount">The number of bits by which to shift each element.</param>
        /// <returns>A vector whose elements where shifted right by <paramref name="shiftCount" />.</returns>
        [Intrinsic]
        public static Vector64<long> ShiftRightArithmetic(Vector64<long> vector, int shiftCount) => vector >> shiftCount;

        /// <summary>Shifts (signed) each element of a vector right by the specified amount.</summary>
        /// <param name="vector">The vector whose elements are to be shifted.</param>
        /// <param name="shiftCount">The number of bits by which to shift each element.</param>
        /// <returns>A vector whose elements where shifted right by <paramref name="shiftCount" />.</returns>
        [Intrinsic]
        public static Vector64<nint> ShiftRightArithmetic(Vector64<nint> vector, int shiftCount) => vector >> shiftCount;

        /// <summary>Shifts (signed) each element of a vector right by the specified amount.</summary>
        /// <param name="vector">The vector whose elements are to be shifted.</param>
        /// <param name="shiftCount">The number of bits by which to shift each element.</param>
        /// <returns>A vector whose elements where shifted right by <paramref name="shiftCount" />.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        public static Vector64<sbyte> ShiftRightArithmetic(Vector64<sbyte> vector, int shiftCount) => vector >> shiftCount;

        /// <summary>Shifts (unsigned) each element of a vector right by the specified amount.</summary>
        /// <param name="vector">The vector whose elements are to be shifted.</param>
        /// <param name="shiftCount">The number of bits by which to shift each element.</param>
        /// <returns>A vector whose elements where shifted right by <paramref name="shiftCount" />.</returns>
        [Intrinsic]
        internal static Vector64<T> ShiftRightLogical<T>(Vector64<T> vector, int shiftCount) => vector >>> shiftCount;

        /// <summary>Shifts (unsigned) each element of a vector right by the specified amount.</summary>
        /// <param name="vector">The vector whose elements are to be shifted.</param>
        /// <param name="shiftCount">The number of bits by which to shift each element.</param>
        /// <returns>A vector whose elements where shifted right by <paramref name="shiftCount" />.</returns>
        [Intrinsic]
        public static Vector64<byte> ShiftRightLogical(Vector64<byte> vector, int shiftCount) => vector >>> shiftCount;

        /// <summary>Shifts (unsigned) each element of a vector right by the specified amount.</summary>
        /// <param name="vector">The vector whose elements are to be shifted.</param>
        /// <param name="shiftCount">The number of bits by which to shift each element.</param>
        /// <returns>A vector whose elements where shifted right by <paramref name="shiftCount" />.</returns>
        [Intrinsic]
        public static Vector64<short> ShiftRightLogical(Vector64<short> vector, int shiftCount) => vector >>> shiftCount;

        /// <summary>Shifts (unsigned) each element of a vector right by the specified amount.</summary>
        /// <param name="vector">The vector whose elements are to be shifted.</param>
        /// <param name="shiftCount">The number of bits by which to shift each element.</param>
        /// <returns>A vector whose elements where shifted right by <paramref name="shiftCount" />.</returns>
        [Intrinsic]
        public static Vector64<int> ShiftRightLogical(Vector64<int> vector, int shiftCount) => vector >>> shiftCount;

        /// <summary>Shifts (unsigned) each element of a vector right by the specified amount.</summary>
        /// <param name="vector">The vector whose elements are to be shifted.</param>
        /// <param name="shiftCount">The number of bits by which to shift each element.</param>
        /// <returns>A vector whose elements where shifted right by <paramref name="shiftCount" />.</returns>
        [Intrinsic]
        public static Vector64<long> ShiftRightLogical(Vector64<long> vector, int shiftCount) => vector >>> shiftCount;

        /// <summary>Shifts (unsigned) each element of a vector right by the specified amount.</summary>
        /// <param name="vector">The vector whose elements are to be shifted.</param>
        /// <param name="shiftCount">The number of bits by which to shift each element.</param>
        /// <returns>A vector whose elements where shifted right by <paramref name="shiftCount" />.</returns>
        [Intrinsic]
        public static Vector64<nint> ShiftRightLogical(Vector64<nint> vector, int shiftCount) => vector >>> shiftCount;

        /// <summary>Shifts (unsigned) each element of a vector right by the specified amount.</summary>
        /// <param name="vector">The vector whose elements are to be shifted.</param>
        /// <param name="shiftCount">The number of bits by which to shift each element.</param>
        /// <returns>A vector whose elements where shifted right by <paramref name="shiftCount" />.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        public static Vector64<nuint> ShiftRightLogical(Vector64<nuint> vector, int shiftCount) => vector >>> shiftCount;

        /// <summary>Shifts (unsigned) each element of a vector right by the specified amount.</summary>
        /// <param name="vector">The vector whose elements are to be shifted.</param>
        /// <param name="shiftCount">The number of bits by which to shift each element.</param>
        /// <returns>A vector whose elements where shifted right by <paramref name="shiftCount" />.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        public static Vector64<sbyte> ShiftRightLogical(Vector64<sbyte> vector, int shiftCount) => vector >>> shiftCount;

        /// <summary>Shifts (unsigned) each element of a vector right by the specified amount.</summary>
        /// <param name="vector">The vector whose elements are to be shifted.</param>
        /// <param name="shiftCount">The number of bits by which to shift each element.</param>
        /// <returns>A vector whose elements where shifted right by <paramref name="shiftCount" />.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        public static Vector64<ushort> ShiftRightLogical(Vector64<ushort> vector, int shiftCount) => vector >>> shiftCount;

        /// <summary>Shifts (unsigned) each element of a vector right by the specified amount.</summary>
        /// <param name="vector">The vector whose elements are to be shifted.</param>
        /// <param name="shiftCount">The number of bits by which to shift each element.</param>
        /// <returns>A vector whose elements where shifted right by <paramref name="shiftCount" />.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        public static Vector64<uint> ShiftRightLogical(Vector64<uint> vector, int shiftCount) => vector >>> shiftCount;

        /// <summary>Shifts (unsigned) each element of a vector right by the specified amount.</summary>
        /// <param name="vector">The vector whose elements are to be shifted.</param>
        /// <param name="shiftCount">The number of bits by which to shift each element.</param>
        /// <returns>A vector whose elements where shifted right by <paramref name="shiftCount" />.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        public static Vector64<ulong> ShiftRightLogical(Vector64<ulong> vector, int shiftCount) => vector >>> shiftCount;

#if !MONO
        // These fallback methods only exist so that ShuffleNative has the same behaviour when called directly or via
        // reflection - reflecting into internal runtime methods is not supported, so we don't worry about others
        // reflecting into these. TODO: figure out if this can be solved in a nicer way.

        [Intrinsic]
        internal static Vector64<byte> ShuffleNativeFallback(Vector64<byte> vector, Vector64<byte> indices)
        {
            return Shuffle(vector, indices);
        }

        [Intrinsic]
        internal static Vector64<sbyte> ShuffleNativeFallback(Vector64<sbyte> vector, Vector64<sbyte> indices)
        {
            return Shuffle(vector, indices);
        }

        [Intrinsic]
        internal static Vector64<short> ShuffleNativeFallback(Vector64<short> vector, Vector64<short> indices)
        {
            return Shuffle(vector, indices);
        }

        [Intrinsic]
        internal static Vector64<ushort> ShuffleNativeFallback(Vector64<ushort> vector, Vector64<ushort> indices)
        {
            return Shuffle(vector, indices);
        }

        [Intrinsic]
        internal static Vector64<int> ShuffleNativeFallback(Vector64<int> vector, Vector64<int> indices)
        {
            return Shuffle(vector, indices);
        }

        [Intrinsic]
        internal static Vector64<uint> ShuffleNativeFallback(Vector64<uint> vector, Vector64<uint> indices)
        {
            return Shuffle(vector, indices);
        }

        [Intrinsic]
        internal static Vector64<float> ShuffleNativeFallback(Vector64<float> vector, Vector64<int> indices)
        {
            return Shuffle(vector, indices);
        }
#endif

        /// <summary>Creates a new vector by selecting values from an input vector using a set of indices.</summary>
        /// <param name="vector">The input vector from which values are selected.</param>
        /// <param name="indices">The per-element indices used to select a value from <paramref name="vector" />.</param>
        /// <returns>A new vector containing the values from <paramref name="vector" /> selected by the given <paramref name="indices" />.</returns>
        [Intrinsic]
        public static Vector64<byte> Shuffle(Vector64<byte> vector, Vector64<byte> indices)
        {
            Unsafe.SkipInit(out Vector64<byte> result);

            for (int index = 0; index < Vector64<byte>.Count; index++)
            {
                byte selectedIndex = indices.GetElementUnsafe(index);
                byte selectedValue = 0;

                if (selectedIndex < Vector64<byte>.Count)
                {
                    selectedValue = vector.GetElementUnsafe(selectedIndex);
                }
                result.SetElementUnsafe(index, selectedValue);
            }

            return result;
        }

        /// <summary>Creates a new vector by selecting values from an input vector using a set of indices.</summary>
        /// <param name="vector">The input vector from which values are selected.</param>
        /// <param name="indices">The per-element indices used to select a value from <paramref name="vector" />.</param>
        /// <returns>A new vector containing the values from <paramref name="vector" /> selected by the given <paramref name="indices" />.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        public static Vector64<sbyte> Shuffle(Vector64<sbyte> vector, Vector64<sbyte> indices)
        {
            Unsafe.SkipInit(out Vector64<sbyte> result);

            for (int index = 0; index < Vector64<sbyte>.Count; index++)
            {
                byte selectedIndex = (byte)indices.GetElementUnsafe(index);
                sbyte selectedValue = 0;

                if (selectedIndex < Vector64<sbyte>.Count)
                {
                    selectedValue = vector.GetElementUnsafe(selectedIndex);
                }
                result.SetElementUnsafe(index, selectedValue);
            }

            return result;
        }

        /// <summary>Creates a new vector by selecting values from an input vector using a set of indices.
        /// Behavior is platform-dependent for out-of-range indices.</summary>
        /// <param name="vector">The input vector from which values are selected.</param>
        /// <param name="indices">The per-element indices used to select a value from <paramref name="vector" />.</param>
        /// <returns>A new vector containing the values from <paramref name="vector" /> selected by the given <paramref name="indices" />.</returns>
        /// <remarks>Unlike Shuffle, this method delegates to the underlying hardware intrinsic without ensuring that <paramref name="indices"/> are normalized to [0, 7].</remarks>
#if !MONO
        [Intrinsic]
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Vector64<byte> ShuffleNative(Vector64<byte> vector, Vector64<byte> indices)
        {
#if !MONO
            return ShuffleNativeFallback(vector, indices);
#else
            return Shuffle(vector, indices);
#endif
        }

        /// <summary>Creates a new vector by selecting values from an input vector using a set of indices.
        /// Behavior is platform-dependent for out-of-range indices.</summary>
        /// <param name="vector">The input vector from which values are selected.</param>
        /// <param name="indices">The per-element indices used to select a value from <paramref name="vector" />.</param>
        /// <returns>A new vector containing the values from <paramref name="vector" /> selected by the given <paramref name="indices" />.</returns>
        /// <remarks>Unlike Shuffle, this method delegates to the underlying hardware intrinsic without ensuring that <paramref name="indices"/> are normalized to [0, 7].</remarks>
#if !MONO
        [Intrinsic]
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [CLSCompliant(false)]
        public static Vector64<sbyte> ShuffleNative(Vector64<sbyte> vector, Vector64<sbyte> indices)
        {
#if !MONO
            return ShuffleNativeFallback(vector, indices);
#else
            return Shuffle(vector, indices);
#endif
        }

        /// <summary>Creates a new vector by selecting values from an input vector using a set of indices.</summary>
        /// <param name="vector">The input vector from which values are selected.</param>
        /// <param name="indices">The per-element indices used to select a value from <paramref name="vector" />.</param>
        /// <returns>A new vector containing the values from <paramref name="vector" /> selected by the given <paramref name="indices" />.</returns>
        [Intrinsic]
        public static Vector64<short> Shuffle(Vector64<short> vector, Vector64<short> indices)
        {
            Unsafe.SkipInit(out Vector64<short> result);

            for (int index = 0; index < Vector64<short>.Count; index++)
            {
                ushort selectedIndex = (ushort)indices.GetElementUnsafe(index);
                short selectedValue = 0;

                if (selectedIndex < Vector64<short>.Count)
                {
                    selectedValue = vector.GetElementUnsafe(selectedIndex);
                }
                result.SetElementUnsafe(index, selectedValue);
            }

            return result;
        }

        /// <summary>Creates a new vector by selecting values from an input vector using a set of indices.</summary>
        /// <param name="vector">The input vector from which values are selected.</param>
        /// <param name="indices">The per-element indices used to select a value from <paramref name="vector" />.</param>
        /// <returns>A new vector containing the values from <paramref name="vector" /> selected by the given <paramref name="indices" />.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        public static Vector64<ushort> Shuffle(Vector64<ushort> vector, Vector64<ushort> indices)
        {
            Unsafe.SkipInit(out Vector64<ushort> result);

            for (int index = 0; index < Vector64<ushort>.Count; index++)
            {
                ushort selectedIndex = indices.GetElementUnsafe(index);
                ushort selectedValue = 0;

                if (selectedIndex < Vector64<ushort>.Count)
                {
                    selectedValue = vector.GetElementUnsafe(selectedIndex);
                }
                result.SetElementUnsafe(index, selectedValue);
            }

            return result;
        }

        /// <summary>Creates a new vector by selecting values from an input vector using a set of indices.</summary>
        /// <param name="vector">The input vector from which values are selected.</param>
        /// <param name="indices">The per-element indices used to select a value from <paramref name="vector" />.</param>
        /// <returns>A new vector containing the values from <paramref name="vector" /> selected by the given <paramref name="indices" />.</returns>
        /// <remarks>Unlike Shuffle, this method delegates to the underlying hardware intrinsic without ensuring that <paramref name="indices"/> are normalized to [0, 3].</remarks>
#if !MONO
        [Intrinsic]
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Vector64<short> ShuffleNative(Vector64<short> vector, Vector64<short> indices)
        {
#if !MONO
            return ShuffleNativeFallback(vector, indices);
#else
            return Shuffle(vector, indices);
#endif
        }

        /// <summary>Creates a new vector by selecting values from an input vector using a set of indices.</summary>
        /// <param name="vector">The input vector from which values are selected.</param>
        /// <param name="indices">The per-element indices used to select a value from <paramref name="vector" />.</param>
        /// <returns>A new vector containing the values from <paramref name="vector" /> selected by the given <paramref name="indices" />.</returns>
        /// <remarks>Unlike Shuffle, this method delegates to the underlying hardware intrinsic without ensuring that <paramref name="indices"/> are normalized to [0, 3].</remarks>
#if !MONO
        [Intrinsic]
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [CLSCompliant(false)]
        public static Vector64<ushort> ShuffleNative(Vector64<ushort> vector, Vector64<ushort> indices)
        {
#if !MONO
            return ShuffleNativeFallback(vector, indices);
#else
            return Shuffle(vector, indices);
#endif
        }

        /// <summary>Creates a new vector by selecting values from an input vector using a set of indices.</summary>
        /// <param name="vector">The input vector from which values are selected.</param>
        /// <param name="indices">The per-element indices used to select a value from <paramref name="vector" />.</param>
        /// <returns>A new vector containing the values from <paramref name="vector" /> selected by the given <paramref name="indices" />.</returns>
        [Intrinsic]
        public static Vector64<int> Shuffle(Vector64<int> vector, Vector64<int> indices)
        {
            Unsafe.SkipInit(out Vector64<int> result);

            for (int index = 0; index < Vector64<int>.Count; index++)
            {
                uint selectedIndex = (uint)indices.GetElementUnsafe(index);
                int selectedValue = 0;

                if (selectedIndex < Vector64<int>.Count)
                {
                    selectedValue = vector.GetElementUnsafe((int)selectedIndex);
                }
                result.SetElementUnsafe(index, selectedValue);
            }

            return result;
        }

        /// <summary>Creates a new vector by selecting values from an input vector using a set of indices.</summary>
        /// <param name="vector">The input vector from which values are selected.</param>
        /// <param name="indices">The per-element indices used to select a value from <paramref name="vector" />.</param>
        /// <returns>A new vector containing the values from <paramref name="vector" /> selected by the given <paramref name="indices" />.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        public static Vector64<uint> Shuffle(Vector64<uint> vector, Vector64<uint> indices)
        {
            Unsafe.SkipInit(out Vector64<uint> result);

            for (int index = 0; index < Vector64<uint>.Count; index++)
            {
                uint selectedIndex = indices.GetElementUnsafe(index);
                uint selectedValue = 0;

                if (selectedIndex < Vector64<uint>.Count)
                {
                    selectedValue = vector.GetElementUnsafe((int)selectedIndex);
                }
                result.SetElementUnsafe(index, selectedValue);
            }

            return result;
        }

        /// <summary>Creates a new vector by selecting values from an input vector using a set of indices.</summary>
        /// <param name="vector">The input vector from which values are selected.</param>
        /// <param name="indices">The per-element indices used to select a value from <paramref name="vector" />.</param>
        /// <returns>A new vector containing the values from <paramref name="vector" /> selected by the given <paramref name="indices" />.</returns>
        [Intrinsic]
        public static Vector64<float> Shuffle(Vector64<float> vector, Vector64<int> indices)
        {
            Unsafe.SkipInit(out Vector64<float> result);

            for (int index = 0; index < Vector64<float>.Count; index++)
            {
                uint selectedIndex = (uint)indices.GetElementUnsafe(index);
                float selectedValue = 0;

                if (selectedIndex < Vector64<float>.Count)
                {
                    selectedValue = vector.GetElementUnsafe((int)selectedIndex);
                }
                result.SetElementUnsafe(index, selectedValue);
            }

            return result;
        }

        /// <summary>Creates a new vector by selecting values from an input vector using a set of indices.</summary>
        /// <param name="vector">The input vector from which values are selected.</param>
        /// <param name="indices">The per-element indices used to select a value from <paramref name="vector" />.</param>
        /// <returns>A new vector containing the values from <paramref name="vector" /> selected by the given <paramref name="indices" />.</returns>
        /// <remarks>Unlike Shuffle, this method delegates to the underlying hardware intrinsic without ensuring that <paramref name="indices"/> are normalized to [0, 1].</remarks>
#if !MONO
        [Intrinsic]
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Vector64<int> ShuffleNative(Vector64<int> vector, Vector64<int> indices)
        {
#if !MONO
            return ShuffleNativeFallback(vector, indices);
#else
            return Shuffle(vector, indices);
#endif
        }

        /// <summary>Creates a new vector by selecting values from an input vector using a set of indices.</summary>
        /// <param name="vector">The input vector from which values are selected.</param>
        /// <param name="indices">The per-element indices used to select a value from <paramref name="vector" />.</param>
        /// <returns>A new vector containing the values from <paramref name="vector" /> selected by the given <paramref name="indices" />.</returns>
        /// <remarks>Unlike Shuffle, this method delegates to the underlying hardware intrinsic without ensuring that <paramref name="indices"/> are normalized to [0, 1].</remarks>
#if !MONO
        [Intrinsic]
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [CLSCompliant(false)]
        public static Vector64<uint> ShuffleNative(Vector64<uint> vector, Vector64<uint> indices)
        {
#if !MONO
            return ShuffleNativeFallback(vector, indices);
#else
            return Shuffle(vector, indices);
#endif
        }

        /// <summary>Creates a new vector by selecting values from an input vector using a set of indices.</summary>
        /// <param name="vector">The input vector from which values are selected.</param>
        /// <param name="indices">The per-element indices used to select a value from <paramref name="vector" />.</param>
        /// <returns>A new vector containing the values from <paramref name="vector" /> selected by the given <paramref name="indices" />.</returns>
        /// <remarks>Unlike Shuffle, this method delegates to the underlying hardware intrinsic without ensuring that <paramref name="indices"/> are normalized to [0, 1].</remarks>
#if !MONO
        [Intrinsic]
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Vector64<float> ShuffleNative(Vector64<float> vector, Vector64<int> indices)
        {
#if !MONO
            return ShuffleNativeFallback(vector, indices);
#else
            return Shuffle(vector, indices);
#endif
        }

        internal static Vector64<T> Sin<T>(Vector64<T> vector)
            where T : ITrigonometricFunctions<T>
        {
            Unsafe.SkipInit(out Vector64<T> result);

            for (int index = 0; index < Vector64<T>.Count; index++)
            {
                T value = T.Sin(vector.GetElementUnsafe(index));
                result.SetElementUnsafe(index, value);
            }

            return result;
        }

        /// <summary>Computes the sin of each element in a vector.</summary>
        /// <param name="vector">The vector that will have its Sin computed.</param>
        /// <returns>A vector whose elements are the sin of the elements in <paramref name="vector" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<double> Sin(Vector64<double> vector)
        {
            if (IsHardwareAccelerated)
            {
                return VectorMath.SinDouble<Vector64<double>, Vector64<long>>(vector);
            }
            else
            {
                return Sin<double>(vector);
            }
        }

        /// <summary>Computes the sin of each element in a vector.</summary>
        /// <param name="vector">The vector that will have its Sin computed.</param>
        /// <returns>A vector whose elements are the sin of the elements in <paramref name="vector" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<float> Sin(Vector64<float> vector)
        {
            if (IsHardwareAccelerated)
            {
                if (Vector128.IsHardwareAccelerated)
                {
                    return VectorMath.SinSingle<Vector64<float>, Vector64<int>, Vector128<double>, Vector128<long>>(vector);
                }
                else
                {
                    return VectorMath.SinSingle<Vector64<float>, Vector64<int>, Vector64<double>, Vector64<long>>(vector);
                }
            }
            else
            {
                return Sin<float>(vector);
            }
        }

        internal static (Vector64<T> Sin, Vector64<T> Cos) SinCos<T>(Vector64<T> vector)
            where T : ITrigonometricFunctions<T>
        {
            Unsafe.SkipInit(out Vector64<T> sinResult);
            Unsafe.SkipInit(out Vector64<T> cosResult);

            for (int index = 0; index < Vector64<T>.Count; index++)
            {
                (T sinValue, T cosValue) = T.SinCos(vector.GetElementUnsafe(index));
                sinResult.SetElementUnsafe(index, sinValue);
                cosResult.SetElementUnsafe(index, cosValue);
            }

            return (sinResult, cosResult);
        }

        /// <summary>Computes the sincos of each element in a vector.</summary>
        /// <param name="vector">The vector that will have its SinCos computed.</param>
        /// <returns>A vector whose elements are the sincos of the elements in <paramref name="vector" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (Vector64<double> Sin, Vector64<double> Cos) SinCos(Vector64<double> vector)
        {
            if (IsHardwareAccelerated)
            {
                return VectorMath.SinCosDouble<Vector64<double>, Vector64<long>>(vector);
            }
            else
            {
                return SinCos<double>(vector);
            }
        }

        /// <summary>Computes the sincos of each element in a vector.</summary>
        /// <param name="vector">The vector that will have its SinCos computed.</param>
        /// <returns>A vector whose elements are the sincos of the elements in <paramref name="vector" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (Vector64<float> Sin, Vector64<float> Cos) SinCos(Vector64<float> vector)
        {
            if (IsHardwareAccelerated)
            {
                if (Vector128.IsHardwareAccelerated)
                {
                    return VectorMath.SinCosSingle<Vector64<float>, Vector64<int>, Vector128<double>, Vector128<long>>(vector);
                }
                else
                {
                    return VectorMath.SinCosSingle<Vector64<float>, Vector64<int>, Vector64<double>, Vector64<long>>(vector);
                }
            }
            else
            {
                return SinCos<float>(vector);
            }
        }

        /// <summary>Computes the square root of a vector on a per-element basis.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector whose square root is to be computed.</param>
        /// <returns>A vector whose elements are the square root of the corresponding elements in <paramref name="vector" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> Sqrt<T>(Vector64<T> vector)
        {
            Unsafe.SkipInit(out Vector64<T> result);

            for (int index = 0; index < Vector64<T>.Count; index++)
            {
                T value = Scalar<T>.Sqrt(vector.GetElementUnsafe(index));
                result.SetElementUnsafe(index, value);
            }

            return result;
        }

        /// <summary>Stores a vector at the given destination.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="source">The vector that will be stored.</param>
        /// <param name="destination">The destination at which <paramref name="source" /> will be stored.</param>
        /// <exception cref="NotSupportedException">The type of <paramref name="source" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [CLSCompliant(false)]
        public static unsafe void Store<T>(this Vector64<T> source, T* destination) => source.StoreUnsafe(ref *destination);

        /// <summary>Stores a vector at the given aligned destination.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="source">The vector that will be stored.</param>
        /// <param name="destination">The aligned destination at which <paramref name="source" /> will be stored.</param>
        /// <exception cref="NotSupportedException">The type of <paramref name="source" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void StoreAligned<T>(this Vector64<T> source, T* destination)
        {
            ThrowHelper.ThrowForUnsupportedIntrinsicsVector64BaseType<T>();

            if (((nuint)destination % Alignment) != 0)
            {
                ThrowHelper.ThrowAccessViolationException();
            }

            *(Vector64<T>*)destination = source;
        }

        /// <summary>Stores a vector at the given aligned destination.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="source">The vector that will be stored.</param>
        /// <param name="destination">The aligned destination at which <paramref name="source" /> will be stored.</param>
        /// <remarks>This method may bypass the cache on certain platforms.</remarks>
        /// <exception cref="NotSupportedException">The type of <paramref name="source" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [CLSCompliant(false)]
        public static unsafe void StoreAlignedNonTemporal<T>(this Vector64<T> source, T* destination) => source.StoreAligned(destination);

        /// <summary>Stores a vector at the given destination.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="source">The vector that will be stored.</param>
        /// <param name="destination">The destination at which <paramref name="source" /> will be stored.</param>
        /// <exception cref="NotSupportedException">The type of <paramref name="source" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StoreUnsafe<T>(this Vector64<T> source, ref T destination)
        {
            ThrowHelper.ThrowForUnsupportedIntrinsicsVector64BaseType<T>();
            ref byte address = ref Unsafe.As<T, byte>(ref destination);
            Unsafe.WriteUnaligned(ref address, source);
        }

        /// <summary>Stores a vector at the given destination.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="source">The vector that will be stored.</param>
        /// <param name="destination">The destination to which <paramref name="elementOffset" /> will be added before the vector will be stored.</param>
        /// <param name="elementOffset">The element offset from <paramref name="destination" /> from which the vector will be stored.</param>
        /// <exception cref="NotSupportedException">The type of <paramref name="source" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StoreUnsafe<T>(this Vector64<T> source, ref T destination, nuint elementOffset)
        {
            ThrowHelper.ThrowForUnsupportedIntrinsicsVector64BaseType<T>();
            destination = ref Unsafe.Add(ref destination, (nint)elementOffset);
            Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref destination), source);
        }

        /// <inheritdoc cref="Vector128.Subtract{T}(Vector128{T}, Vector128{T})" />
        [Intrinsic]
        public static Vector64<T> Subtract<T>(Vector64<T> left, Vector64<T> right) => left - right;

        /// <inheritdoc cref="Vector128.SubtractSaturate{T}(Vector128{T}, Vector128{T})" />
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> SubtractSaturate<T>(Vector64<T> left, Vector64<T> right)
        {
            if ((typeof(T) == typeof(float)) || (typeof(T) == typeof(double)))
            {
                return left - right;
            }
            else
            {
                Unsafe.SkipInit(out Vector64<T> result);

                for (int index = 0; index < Vector64<T>.Count; index++)
                {
                    T value = Scalar<T>.SubtractSaturate(left.GetElementUnsafe(index), right.GetElementUnsafe(index));
                    result.SetElementUnsafe(index, value);
                }

                return result;
            }
        }

        /// <summary>Computes the sum of all elements in a vector.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector whose elements will be summed.</param>
        /// <returns>The sum of all elements in <paramref name="vector" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Sum<T>(Vector64<T> vector)
        {
            T sum = default!;

            for (int index = 0; index < Vector64<T>.Count; index++)
            {
                sum = Scalar<T>.Add(sum, vector.GetElementUnsafe(index));
            }

            return sum;
        }

        /// <summary>Converts the given vector to a scalar containing the value of the first element.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector to get the first element from.</param>
        /// <returns>A scalar <typeparamref name="T" /> containing the value of the first element.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        public static T ToScalar<T>(this Vector64<T> vector)
        {
            ThrowHelper.ThrowForUnsupportedIntrinsicsVector64BaseType<T>();
            return vector.GetElementUnsafe(0);
        }

        /// <summary>Converts the given vector to a new <see cref="Vector128{T}" /> with the lower 64-bits set to the value of the given vector and the upper 64-bits initialized to zero.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector to extend.</param>
        /// <returns>A new <see cref="Vector128{T}" /> with the lower 64-bits set to the value of <paramref name="vector" /> and the upper 64-bits initialized to zero.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<T> ToVector128<T>(this Vector64<T> vector)
        {
            ThrowHelper.ThrowForUnsupportedIntrinsicsVector64BaseType<T>();

            Vector128<T> result = default;
            result.SetLowerUnsafe(vector);
            return result;
        }

        /// <summary>Converts the given vector to a new <see cref="Vector128{T}" /> with the lower 64-bits set to the value of the given vector and the upper 64-bits left uninitialized.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector to extend.</param>
        /// <returns>A new <see cref="Vector128{T}" /> with the lower 64-bits set to the value of <paramref name="vector" /> and the upper 64-bits left uninitialized.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<T> ToVector128Unsafe<T>(this Vector64<T> vector)
        {
            ThrowHelper.ThrowForUnsupportedIntrinsicsVector64BaseType<T>();

            // This relies on us stripping the "init" flag from the ".locals"
            // declaration to let the upper bits be uninitialized.

            Unsafe.SkipInit(out Vector128<T> result);
            result.SetLowerUnsafe(vector);
            return result;
        }

        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector64<T> Truncate<T>(Vector64<T> vector)
        {
            if ((typeof(T) == typeof(byte))
             || (typeof(T) == typeof(short))
             || (typeof(T) == typeof(int))
             || (typeof(T) == typeof(long))
             || (typeof(T) == typeof(nint))
             || (typeof(T) == typeof(nuint))
             || (typeof(T) == typeof(sbyte))
             || (typeof(T) == typeof(ushort))
             || (typeof(T) == typeof(uint))
             || (typeof(T) == typeof(ulong)))
            {
                return vector;
            }
            else
            {
                Unsafe.SkipInit(out Vector64<T> result);

                for (int index = 0; index < Vector64<T>.Count; index++)
                {
                    T value = Scalar<T>.Truncate(vector.GetElementUnsafe(index));
                    result.SetElementUnsafe(index, value);
                }

                return result;
            }
        }

        /// <inheritdoc cref="ISimdVector{TSelf, T}.Truncate(TSelf)" />
        [Intrinsic]
        public static Vector64<double> Truncate(Vector64<double> vector) => Truncate<double>(vector);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.Truncate(TSelf)" />
        [Intrinsic]
        public static Vector64<float> Truncate(Vector64<float> vector) => Truncate<float>(vector);

        /// <summary>Tries to copy a <see cref="Vector{T}" /> to a given span.</summary>
        /// <typeparam name="T">The type of the input vector.</typeparam>
        /// <param name="vector">The vector to copy.</param>
        /// <param name="destination">The span to which <paramref name="destination" /> is copied.</param>
        /// <returns><c>true</c> if <paramref name="vector" /> was successfully copied to <paramref name="destination" />; otherwise, <c>false</c> if the length of <paramref name="destination" /> is less than <see cref="Vector64{T}.Count" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> and <paramref name="destination" /> (<typeparamref name="T" />) is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryCopyTo<T>(this Vector64<T> vector, Span<T> destination)
        {
            if (destination.Length < Vector64<T>.Count)
            {
                return false;
            }

            Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(destination)), vector);
            return true;
        }

        /// <summary>Widens a <see langword="Vector64&lt;Byte&gt;" /> into two <see cref="Vector64{UInt16} " />.</summary>
        /// <param name="source">The vector whose elements are to be widened.</param>
        /// <returns>A pair of vectors that contain the widened lower and upper halves of <paramref name="source" />.</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (Vector64<ushort> Lower, Vector64<ushort> Upper) Widen(Vector64<byte> source) => (WidenLower(source), WidenUpper(source));

        /// <summary>Widens a <see langword="Vector64&lt;Int16&gt;" /> into two <see cref="Vector64{Int32} " />.</summary>
        /// <param name="source">The vector whose elements are to be widened.</param>
        /// <returns>A pair of vectors that contain the widened lower and upper halves of <paramref name="source" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (Vector64<int> Lower, Vector64<int> Upper) Widen(Vector64<short> source) => (WidenLower(source), WidenUpper(source));

        /// <summary>Widens a <see langword="Vector64&lt;Int32&gt;" /> into two <see cref="Vector64{Int64} " />.</summary>
        /// <param name="source">The vector whose elements are to be widened.</param>
        /// <returns>A pair of vectors that contain the widened lower and upper halves of <paramref name="source" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (Vector64<long> Lower, Vector64<long> Upper) Widen(Vector64<int> source) => (WidenLower(source), WidenUpper(source));

        /// <summary>Widens a <see langword="Vector64&lt;SByte&gt;" /> into two <see cref="Vector64{Int16} " />.</summary>
        /// <param name="source">The vector whose elements are to be widened.</param>
        /// <returns>A pair of vectors that contain the widened lower and upper halves of <paramref name="source" />.</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (Vector64<short> Lower, Vector64<short> Upper) Widen(Vector64<sbyte> source) => (WidenLower(source), WidenUpper(source));

        /// <summary>Widens a <see langword="Vector64&lt;Single&gt;" /> into two <see cref="Vector64{Double} " />.</summary>
        /// <param name="source">The vector whose elements are to be widened.</param>
        /// <returns>A pair of vectors that contain the widened lower and upper halves of <paramref name="source" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (Vector64<double> Lower, Vector64<double> Upper) Widen(Vector64<float> source) => (WidenLower(source), WidenUpper(source));

        /// <summary>Widens a <see langword="Vector64&lt;UInt16&gt;" /> into two <see cref="Vector64{UInt32} " />.</summary>
        /// <param name="source">The vector whose elements are to be widened.</param>
        /// <returns>A pair of vectors that contain the widened lower and upper halves of <paramref name="source" />.</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (Vector64<uint> Lower, Vector64<uint> Upper) Widen(Vector64<ushort> source) => (WidenLower(source), WidenUpper(source));

        /// <summary>Widens a <see langword="Vector64&lt;UInt32&gt;" /> into two <see cref="Vector64{UInt64} " />.</summary>
        /// <param name="source">The vector whose elements are to be widened.</param>
        /// <returns>A pair of vectors that contain the widened lower and upper halves of <paramref name="source" />.</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (Vector64<ulong> Lower, Vector64<ulong> Upper) Widen(Vector64<uint> source) => (WidenLower(source), WidenUpper(source));

        /// <summary>Widens the lower half of a <see langword="Vector64&lt;Byte&gt;" /> into a <see cref="Vector64{UInt16} " />.</summary>
        /// <param name="source">The vector whose elements are to be widened.</param>
        /// <returns>A vector that contain the widened lower half of <paramref name="source" />.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<ushort> WidenLower(Vector64<byte> source)
        {
            Unsafe.SkipInit(out Vector64<ushort> lower);

            for (int i = 0; i < Vector64<ushort>.Count; i++)
            {
                ushort value = source.GetElementUnsafe(i);
                lower.SetElementUnsafe(i, value);
            }

            return lower;
        }

        /// <summary>Widens the lower half of a <see langword="Vector64&lt;Int16&gt;" /> into a <see cref="Vector64{Int32} " />.</summary>
        /// <param name="source">The vector whose elements are to be widened.</param>
        /// <returns>A vector that contain the widened lower half of <paramref name="source" />.</returns>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<int> WidenLower(Vector64<short> source)
        {
            Unsafe.SkipInit(out Vector64<int> lower);

            for (int i = 0; i < Vector64<int>.Count; i++)
            {
                int value = source.GetElementUnsafe(i);
                lower.SetElementUnsafe(i, value);
            }

            return lower;
        }

        /// <summary>Widens the lower half of a <see langword="Vector64&lt;Int32&gt;" /> into a <see cref="Vector64{Int64} " />.</summary>
        /// <param name="source">The vector whose elements are to be widened.</param>
        /// <returns>A vector that contain the widened lower half of <paramref name="source" />.</returns>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<long> WidenLower(Vector64<int> source)
        {
            Unsafe.SkipInit(out Vector64<long> lower);

            for (int i = 0; i < Vector64<long>.Count; i++)
            {
                long value = source.GetElementUnsafe(i);
                lower.SetElementUnsafe(i, value);
            }

            return lower;
        }

        /// <summary>Widens the lower half of a <see langword="Vector64&lt;SByte&gt;" /> into a <see cref="Vector64{Int16} " />.</summary>
        /// <param name="source">The vector whose elements are to be widened.</param>
        /// <returns>A vector that contain the widened lower half of <paramref name="source" />.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<short> WidenLower(Vector64<sbyte> source)
        {
            Unsafe.SkipInit(out Vector64<short> lower);

            for (int i = 0; i < Vector64<short>.Count; i++)
            {
                short value = source.GetElementUnsafe(i);
                lower.SetElementUnsafe(i, value);
            }

            return lower;
        }

        /// <summary>Widens the lower half of a <see langword="Vector64&lt;Single&gt;" /> into a <see cref="Vector64{Double} " />.</summary>
        /// <param name="source">The vector whose elements are to be widened.</param>
        /// <returns>A vector that contain the widened lower half of <paramref name="source" />.</returns>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<double> WidenLower(Vector64<float> source)
        {
            Unsafe.SkipInit(out Vector64<double> lower);

            for (int i = 0; i < Vector64<double>.Count; i++)
            {
                double value = source.GetElementUnsafe(i);
                lower.SetElementUnsafe(i, value);
            }

            return lower;
        }

        /// <summary>Widens the lower half of a <see langword="Vector64&lt;UInt16&gt;" /> into a <see cref="Vector64{UInt32} " />.</summary>
        /// <param name="source">The vector whose elements are to be widened.</param>
        /// <returns>A vector that contain the widened lower half of <paramref name="source" />.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<uint> WidenLower(Vector64<ushort> source)
        {
            Unsafe.SkipInit(out Vector64<uint> lower);

            for (int i = 0; i < Vector64<uint>.Count; i++)
            {
                uint value = source.GetElementUnsafe(i);
                lower.SetElementUnsafe(i, value);
            }

            return lower;
        }

        /// <summary>Widens the lower half of a <see langword="Vector64&lt;UInt32&gt;" /> into a <see cref="Vector64{UInt64} " />.</summary>
        /// <param name="source">The vector whose elements are to be widened.</param>
        /// <returns>A vector that contain the widened lower half of <paramref name="source" />.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<ulong> WidenLower(Vector64<uint> source)
        {
            Unsafe.SkipInit(out Vector64<ulong> lower);

            for (int i = 0; i < Vector64<ulong>.Count; i++)
            {
                ulong value = source.GetElementUnsafe(i);
                lower.SetElementUnsafe(i, value);
            }

            return lower;
        }

        /// <summary>Widens the upper half of a <see langword="Vector64&lt;Byte&gt;" /> into a <see cref="Vector64{UInt16} " />.</summary>
        /// <param name="source">The vector whose elements are to be widened.</param>
        /// <returns>A vector that contain the widened upper half of <paramref name="source" />.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<ushort> WidenUpper(Vector64<byte> source)
        {
            Unsafe.SkipInit(out Vector64<ushort> upper);

            for (int i = Vector64<ushort>.Count; i < Vector64<byte>.Count; i++)
            {
                ushort value = source.GetElementUnsafe(i);
                upper.SetElementUnsafe(i - Vector64<ushort>.Count, value);
            }

            return upper;
        }

        /// <summary>Widens the upper half of a <see langword="Vector64&lt;Int16&gt;" /> into a <see cref="Vector64{Int32} " />.</summary>
        /// <param name="source">The vector whose elements are to be widened.</param>
        /// <returns>A vector that contain the widened upper half of <paramref name="source" />.</returns>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<int> WidenUpper(Vector64<short> source)
        {
            Unsafe.SkipInit(out Vector64<int> upper);

            for (int i = Vector64<int>.Count; i < Vector64<short>.Count; i++)
            {
                int value = source.GetElementUnsafe(i);
                upper.SetElementUnsafe(i - Vector64<int>.Count, value);
            }

            return upper;
        }

        /// <summary>Widens the upper half of a <see langword="Vector64&lt;Int32&gt;" /> into a <see cref="Vector64{Int64} " />.</summary>
        /// <param name="source">The vector whose elements are to be widened.</param>
        /// <returns>A vector that contain the widened upper half of <paramref name="source" />.</returns>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<long> WidenUpper(Vector64<int> source)
        {
            Unsafe.SkipInit(out Vector64<long> upper);

            for (int i = Vector64<long>.Count; i < Vector64<int>.Count; i++)
            {
                long value = source.GetElementUnsafe(i);
                upper.SetElementUnsafe(i - Vector64<long>.Count, value);
            }

            return upper;
        }

        /// <summary>Widens the upper half of a <see langword="Vector64&lt;SByte&gt;" /> into a <see cref="Vector64{Int16} " />.</summary>
        /// <param name="source">The vector whose elements are to be widened.</param>
        /// <returns>A vector that contain the widened upper half of <paramref name="source" />.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<short> WidenUpper(Vector64<sbyte> source)
        {
            Unsafe.SkipInit(out Vector64<short> upper);

            for (int i = Vector64<short>.Count; i < Vector64<sbyte>.Count; i++)
            {
                short value = source.GetElementUnsafe(i);
                upper.SetElementUnsafe(i - Vector64<short>.Count, value);
            }

            return upper;
        }

        /// <summary>Widens the upper half of a <see langword="Vector64&lt;Single&gt;" /> into a <see cref="Vector64{Double} " />.</summary>
        /// <param name="source">The vector whose elements are to be widened.</param>
        /// <returns>A vector that contain the widened upper half of <paramref name="source" />.</returns>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<double> WidenUpper(Vector64<float> source)
        {
            Unsafe.SkipInit(out Vector64<double> upper);

            for (int i = Vector64<double>.Count; i < Vector64<float>.Count; i++)
            {
                double value = source.GetElementUnsafe(i);
                upper.SetElementUnsafe(i - Vector64<double>.Count, value);
            }

            return upper;
        }

        /// <summary>Widens the upper half of a <see langword="Vector64&lt;UInt16&gt;" /> into a <see cref="Vector64{UInt32} " />.</summary>
        /// <param name="source">The vector whose elements are to be widened.</param>
        /// <returns>A vector that contain the widened upper half of <paramref name="source" />.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<uint> WidenUpper(Vector64<ushort> source)
        {
            Unsafe.SkipInit(out Vector64<uint> upper);

            for (int i = Vector64<uint>.Count; i < Vector64<ushort>.Count; i++)
            {
                uint value = source.GetElementUnsafe(i);
                upper.SetElementUnsafe(i - Vector64<uint>.Count, value);
            }

            return upper;
        }

        /// <summary>Widens the upper half of a <see langword="Vector64&lt;UInt32&gt;" /> into a <see cref="Vector64{UInt64} " />.</summary>
        /// <param name="source">The vector whose elements are to be widened.</param>
        /// <returns>A vector that contain the widened upper half of <paramref name="source" />.</returns>
        [Intrinsic]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<ulong> WidenUpper(Vector64<uint> source)
        {
            Unsafe.SkipInit(out Vector64<ulong> upper);

            for (int i = Vector64<ulong>.Count; i < Vector64<uint>.Count; i++)
            {
                ulong value = source.GetElementUnsafe(i);
                upper.SetElementUnsafe(i - Vector64<ulong>.Count, value);
            }

            return upper;
        }

        /// <summary>Creates a new <see cref="Vector64{T}" /> with the element at the specified index set to the specified value and the remaining elements set to the same value as that in the given vector.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="vector">The vector to get the remaining elements from.</param>
        /// <param name="index">The index of the element to set.</param>
        /// <param name="value">The value to set the element to.</param>
        /// <returns>A <see cref="Vector64{T}" /> with the value of the element at <paramref name="index" /> set to <paramref name="value" /> and the remaining elements set to the same value as that in <paramref name="vector" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index" /> was less than zero or greater than the number of elements.</exception>
        /// <exception cref="NotSupportedException">The type of <paramref name="vector" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector64<T> WithElement<T>(this Vector64<T> vector, int index, T value)
        {
            if ((uint)(index) >= (uint)(Vector64<T>.Count))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index);
            }

            Vector64<T> result = vector;
            result.SetElementUnsafe(index, value);
            return result;
        }

        /// <summary>Computes the exclusive-or of two vectors.</summary>
        /// <typeparam name="T">The type of the elements in the vector.</typeparam>
        /// <param name="left">The vector to exclusive-or with <paramref name="right" />.</param>
        /// <param name="right">The vector to exclusive-or with <paramref name="left" />.</param>
        /// <returns>The exclusive-or of <paramref name="left" /> and <paramref name="right" />.</returns>
        /// <exception cref="NotSupportedException">The type of <paramref name="left" /> and <paramref name="right" /> (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        public static Vector64<T> Xor<T>(Vector64<T> left, Vector64<T> right) => left ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static T GetElementUnsafe<T>(in this Vector64<T> vector, int index)
        {
            Debug.Assert((index >= 0) && (index < Vector64<T>.Count));
            ref T address = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in vector));
            return Unsafe.Add(ref address, index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SetElementUnsafe<T>(in this Vector64<T> vector, int index, T value)
        {
            Debug.Assert((index >= 0) && (index < Vector64<T>.Count));
            ref T address = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in vector));
            Unsafe.Add(ref address, index) = value;
        }
    }
}
