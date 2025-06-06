// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.Extensions.Options
{
    /// <summary>
    /// Implementation of <see cref="IPostConfigureOptions{TOptions}"/>.
    /// </summary>
    /// <typeparam name="TOptions">The options type being configured.</typeparam>
    public class PostConfigureOptions<TOptions> : IPostConfigureOptions<TOptions> where TOptions : class
    {
        /// <summary>
        /// Creates a new instance of <see cref="PostConfigureOptions{TOptions}"/>.
        /// </summary>
        /// <param name="name">The name of the options.</param>
        /// <param name="action">The action to register.</param>
        public PostConfigureOptions(string? name, Action<TOptions>? action)
        {
            Name = name;
            Action = action;
        }

        /// <summary>
        /// Gets the options name.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Gets the initialization action.
        /// </summary>
        public Action<TOptions>? Action { get; }

        /// <summary>
        /// Invokes the registered initialization <see cref="Action"/> if the <paramref name="name"/> matches.
        /// </summary>
        /// <param name="name">The name of the action to invoke.</param>
        /// <param name="options">The options to use in initialization.</param>
        public virtual void PostConfigure(string? name, TOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            // Null name is used to initialize all named options.
            if (Name == null || name == Name)
            {
                Action?.Invoke(options);
            }
        }
    }

    /// <summary>
    /// Implementation of <see cref="IPostConfigureOptions{TOptions}"/>.
    /// </summary>
    /// <typeparam name="TOptions">Options type being configured.</typeparam>
    /// <typeparam name="TDep">Dependency type.</typeparam>
    public class PostConfigureOptions<TOptions, TDep> : IPostConfigureOptions<TOptions>
        where TOptions : class
        where TDep : class
    {
        /// <summary>
        /// Initializes a new instance of <see cref="PostConfigureOptions{TOptions, TDep}"/>.
        /// </summary>
        /// <param name="name">The name of the options.</param>
        /// <param name="dependency">A dependency.</param>
        /// <param name="action">The action to register.</param>
        public PostConfigureOptions(string? name, TDep dependency, Action<TOptions, TDep>? action)
        {
            Name = name;
            Action = action;
            Dependency = dependency;
        }

        /// <summary>
        /// Gets the options name.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Gets the configuration action.
        /// </summary>
        public Action<TOptions, TDep>? Action { get; }

        /// <summary>
        /// The dependency.
        /// </summary>
        public TDep Dependency { get; }

        /// <summary>
        /// Invokes the registered initialization <see cref="Action"/> if the <paramref name="name"/> matches.
        /// </summary>
        /// <param name="name">The name of the options instance being configured.</param>
        /// <param name="options">The options instance to configured.</param>
        public virtual void PostConfigure(string? name, TOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            // Null name is used to configure all named options.
            if (Name == null || name == Name)
            {
                Action?.Invoke(options, Dependency);
            }
        }

        /// <summary>
        /// Configures a <typeparamref name="TOptions"/> instance using the <see cref="Options.DefaultName"/>.
        /// </summary>
        /// <param name="options">The options instance to configured.</param>
        public void PostConfigure(TOptions options) => PostConfigure(Options.DefaultName, options);
    }

    /// <summary>
    /// Implementation of <see cref="IPostConfigureOptions{TOptions}"/>.
    /// </summary>
    /// <typeparam name="TOptions">Options type being configured.</typeparam>
    /// <typeparam name="TDep1">First dependency type.</typeparam>
    /// <typeparam name="TDep2">Second dependency type.</typeparam>
    public class PostConfigureOptions<TOptions, TDep1, TDep2> : IPostConfigureOptions<TOptions>
        where TOptions : class
        where TDep1 : class
        where TDep2 : class
    {
        /// <summary>
        /// Initializes a new instance of <see cref="PostConfigureOptions{TOptions, TDep1, TDep2}"/>.
        /// </summary>
        /// <param name="name">The name of the options.</param>
        /// <param name="dependency">A dependency.</param>
        /// <param name="dependency2">A second dependency.</param>
        /// <param name="action">The action to register.</param>
        public PostConfigureOptions(string? name, TDep1 dependency, TDep2 dependency2, Action<TOptions, TDep1, TDep2>? action)
        {
            Name = name;
            Action = action;
            Dependency1 = dependency;
            Dependency2 = dependency2;
        }

        /// <summary>
        /// Gets the options name.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Gets the configuration action.
        /// </summary>
        public Action<TOptions, TDep1, TDep2>? Action { get; }

        /// <summary>
        /// Gets the first dependency.
        /// </summary>
        public TDep1 Dependency1 { get; }

        /// <summary>
        /// Gets the second dependency.
        /// </summary>
        public TDep2 Dependency2 { get; }

        /// <summary>
        /// Invokes the registered initialization <see cref="Action"/> if the <paramref name="name"/> matches.
        /// </summary>
        /// <param name="name">The name of the options instance being configured.</param>
        /// <param name="options">The options instance to configured.</param>
        public virtual void PostConfigure(string? name, TOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            // Null name is used to configure all named options.
            if (Name == null || name == Name)
            {
                Action?.Invoke(options, Dependency1, Dependency2);
            }
        }

        /// <summary>
        /// Configures a <typeparamref name="TOptions"/> instance using the <see cref="Options.DefaultName"/>.
        /// </summary>
        /// <param name="options">The options instance to configured.</param>
        public void PostConfigure(TOptions options) => PostConfigure(Options.DefaultName, options);
    }

    /// <summary>
    /// Implementation of <see cref="IPostConfigureOptions{TOptions}"/>.
    /// </summary>
    /// <typeparam name="TOptions">Options type being configured.</typeparam>
    /// <typeparam name="TDep1">First dependency type.</typeparam>
    /// <typeparam name="TDep2">Second dependency type.</typeparam>
    /// <typeparam name="TDep3">Third dependency type.</typeparam>
    public class PostConfigureOptions<TOptions, TDep1, TDep2, TDep3> : IPostConfigureOptions<TOptions>
        where TOptions : class
        where TDep1 : class
        where TDep2 : class
        where TDep3 : class
    {
        /// <summary>
        /// Initializes a new instance of <see cref="PostConfigureOptions{TOptions, TDep1, TDep2, TDep3}"/>.
        /// </summary>
        /// <param name="name">The name of the options.</param>
        /// <param name="dependency">A dependency.</param>
        /// <param name="dependency2">A second dependency.</param>
        /// <param name="dependency3">A third dependency.</param>
        /// <param name="action">The action to register.</param>
        public PostConfigureOptions(string? name, TDep1 dependency, TDep2 dependency2, TDep3 dependency3, Action<TOptions, TDep1, TDep2, TDep3>? action)
        {
            Name = name;
            Action = action;
            Dependency1 = dependency;
            Dependency2 = dependency2;
            Dependency3 = dependency3;
        }

        /// <summary>
        /// Gets the options name.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Gets the configuration action.
        /// </summary>
        public Action<TOptions, TDep1, TDep2, TDep3>? Action { get; }

        /// <summary>
        /// Gets the first dependency.
        /// </summary>
        public TDep1 Dependency1 { get; }

        /// <summary>
        /// Gets the second dependency.
        /// </summary>
        public TDep2 Dependency2 { get; }

        /// <summary>
        /// Gets the third dependency.
        /// </summary>
        public TDep3 Dependency3 { get; }

        /// <summary>
        /// Invokes the registered initialization <see cref="Action"/> if the <paramref name="name"/> matches.
        /// </summary>
        /// <param name="name">The name of the options instance being configured.</param>
        /// <param name="options">The options instance to configured.</param>
        public virtual void PostConfigure(string? name, TOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            // Null name is used to configure all named options.
            if (Name == null || name == Name)
            {
                Action?.Invoke(options, Dependency1, Dependency2, Dependency3);
            }
        }

        /// <summary>
        /// Configures a <typeparamref name="TOptions"/> instance using the <see cref="Options.DefaultName"/>.
        /// </summary>
        /// <param name="options">The options instance to configured.</param>
        public void PostConfigure(TOptions options) => PostConfigure(Options.DefaultName, options);
    }

    /// <summary>
    /// Implementation of <see cref="IPostConfigureOptions{TOptions}"/>.
    /// </summary>
    /// <typeparam name="TOptions">Options type being configured.</typeparam>
    /// <typeparam name="TDep1">First dependency type.</typeparam>
    /// <typeparam name="TDep2">Second dependency type.</typeparam>
    /// <typeparam name="TDep3">Third dependency type.</typeparam>
    /// <typeparam name="TDep4">Fourth dependency type.</typeparam>
    public class PostConfigureOptions<TOptions, TDep1, TDep2, TDep3, TDep4> : IPostConfigureOptions<TOptions>
        where TOptions : class
        where TDep1 : class
        where TDep2 : class
        where TDep3 : class
        where TDep4 : class
    {
        /// <summary>
        /// Initializes a new instance of <see cref="PostConfigureOptions{TOptions, TDep1, TDep2, TDep3, TDep4}"/>.
        /// </summary>
        /// <param name="name">The name of the options.</param>
        /// <param name="dependency1">A dependency.</param>
        /// <param name="dependency2">A second dependency.</param>
        /// <param name="dependency3">A third dependency.</param>
        /// <param name="dependency4">A fourth dependency.</param>
        /// <param name="action">The action to register.</param>
        public PostConfigureOptions(string? name, TDep1 dependency1, TDep2 dependency2, TDep3 dependency3, TDep4 dependency4, Action<TOptions, TDep1, TDep2, TDep3, TDep4>? action)
        {
            Name = name;
            Action = action;
            Dependency1 = dependency1;
            Dependency2 = dependency2;
            Dependency3 = dependency3;
            Dependency4 = dependency4;
        }

        /// <summary>
        /// Gets the options name.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Gets the configuration action.
        /// </summary>
        public Action<TOptions, TDep1, TDep2, TDep3, TDep4>? Action { get; }

        /// <summary>
        /// Gets the first dependency.
        /// </summary>
        public TDep1 Dependency1 { get; }

        /// <summary>
        /// Gets the second dependency.
        /// </summary>
        public TDep2 Dependency2 { get; }

        /// <summary>
        /// Gets the third dependency.
        /// </summary>
        public TDep3 Dependency3 { get; }

        /// <summary>
        /// Gets the fourth dependency.
        /// </summary>
        public TDep4 Dependency4 { get; }

        /// <summary>
        /// Invokes the registered initialization <see cref="Action"/> if the <paramref name="name"/> matches.
        /// </summary>
        /// <param name="name">The name of the options instance being configured.</param>
        /// <param name="options">The options instance to configured.</param>
        public virtual void PostConfigure(string? name, TOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            // Null name is used to configure all named options.
            if (Name == null || name == Name)
            {
                Action?.Invoke(options, Dependency1, Dependency2, Dependency3, Dependency4);
            }
        }

        /// <summary>
        /// Configures a <typeparamref name="TOptions"/> instance using the <see cref="Options.DefaultName"/>.
        /// </summary>
        /// <param name="options">The options instance to configured.</param>
        public void PostConfigure(TOptions options) => PostConfigure(Options.DefaultName, options);
    }

    /// <summary>
    /// Implementation of <see cref="IPostConfigureOptions{TOptions}"/>.
    /// </summary>
    /// <typeparam name="TOptions">Options type being configured.</typeparam>
    /// <typeparam name="TDep1">First dependency type.</typeparam>
    /// <typeparam name="TDep2">Second dependency type.</typeparam>
    /// <typeparam name="TDep3">Third dependency type.</typeparam>
    /// <typeparam name="TDep4">Fourth dependency type.</typeparam>
    /// <typeparam name="TDep5">Fifth dependency type.</typeparam>
    public class PostConfigureOptions<TOptions, TDep1, TDep2, TDep3, TDep4, TDep5> : IPostConfigureOptions<TOptions>
        where TOptions : class
        where TDep1 : class
        where TDep2 : class
        where TDep3 : class
        where TDep4 : class
        where TDep5 : class
    {
        /// <summary>
        /// Initializes a new instance of <see cref="PostConfigureOptions{TOptions, TDep1, TDep2, TDep3, TDep4, TDep5}"/>.
        /// </summary>
        /// <param name="name">The name of the options.</param>
        /// <param name="dependency1">A dependency.</param>
        /// <param name="dependency2">A second dependency.</param>
        /// <param name="dependency3">A third dependency.</param>
        /// <param name="dependency4">A fourth dependency.</param>
        /// <param name="dependency5">A fifth dependency.</param>
        /// <param name="action">The action to register.</param>
        public PostConfigureOptions(string? name, TDep1 dependency1, TDep2 dependency2, TDep3 dependency3, TDep4 dependency4, TDep5 dependency5, Action<TOptions, TDep1, TDep2, TDep3, TDep4, TDep5>? action)
        {
            Name = name;
            Action = action;
            Dependency1 = dependency1;
            Dependency2 = dependency2;
            Dependency3 = dependency3;
            Dependency4 = dependency4;
            Dependency5 = dependency5;
        }

        /// <summary>
        /// Gets the options name.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Gets the configuration action.
        /// </summary>
        public Action<TOptions, TDep1, TDep2, TDep3, TDep4, TDep5>? Action { get; }

        /// <summary>
        /// Gets the first dependency.
        /// </summary>
        public TDep1 Dependency1 { get; }

        /// <summary>
        /// Gets the second dependency.
        /// </summary>
        public TDep2 Dependency2 { get; }

        /// <summary>
        /// Gets the third dependency.
        /// </summary>
        public TDep3 Dependency3 { get; }

        /// <summary>
        /// Gets the fourth dependency.
        /// </summary>
        public TDep4 Dependency4 { get; }

        /// <summary>
        /// Gets the fifth dependency.
        /// </summary>
        public TDep5 Dependency5 { get; }

        /// <summary>
        /// Invokes the registered initialization <see cref="Action"/> if the <paramref name="name"/> matches.
        /// </summary>
        /// <param name="name">The name of the options instance being configured.</param>
        /// <param name="options">The options instance to configured.</param>
        public virtual void PostConfigure(string? name, TOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            // Null name is used to configure all named options.
            if (Name == null || name == Name)
            {
                Action?.Invoke(options, Dependency1, Dependency2, Dependency3, Dependency4, Dependency5);
            }
        }

        /// <summary>
        /// Configures a <typeparamref name="TOptions"/> instance using the <see cref="Options.DefaultName"/>.
        /// </summary>
        /// <param name="options">The options instance to configured.</param>
        public void PostConfigure(TOptions options) => PostConfigure(Options.DefaultName, options);
    }

}
