// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for configuring per-service dependency replacements.
    /// </summary>
    public static partial class ServiceCollectionServiceExtensions
    {
        /// <summary>
        /// Adds a transient service of the type specified in <typeparamref name="TService"/> with an
        /// implementation type specified in <typeparamref name="TImplementation"/>, with dependency replacements
        /// configured via <paramref name="configure"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <param name="configure">A delegate to configure dependency replacements.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddTransient<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(
            this IServiceCollection services,
            Action<DependencyReplacementBuilder> configure)
            where TService : class
            where TImplementation : class, TService
        {
            ThrowHelper.ThrowIfNull(services);
            ThrowHelper.ThrowIfNull(configure);

            var descriptor = ServiceDescriptor.Transient<TService, TImplementation>();
            configure(new DependencyReplacementBuilder(descriptor));
            services.Add(descriptor);
            return services;
        }

        /// <summary>
        /// Adds a scoped service of the type specified in <typeparamref name="TService"/> with an
        /// implementation type specified in <typeparamref name="TImplementation"/>, with dependency replacements
        /// configured via <paramref name="configure"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <param name="configure">A delegate to configure dependency replacements.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddScoped<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(
            this IServiceCollection services,
            Action<DependencyReplacementBuilder> configure)
            where TService : class
            where TImplementation : class, TService
        {
            ThrowHelper.ThrowIfNull(services);
            ThrowHelper.ThrowIfNull(configure);

            var descriptor = ServiceDescriptor.Scoped<TService, TImplementation>();
            configure(new DependencyReplacementBuilder(descriptor));
            services.Add(descriptor);
            return services;
        }

        /// <summary>
        /// Adds a singleton service of the type specified in <typeparamref name="TService"/> with an
        /// implementation type specified in <typeparamref name="TImplementation"/>, with dependency replacements
        /// configured via <paramref name="configure"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <param name="configure">A delegate to configure dependency replacements.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddSingleton<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(
            this IServiceCollection services,
            Action<DependencyReplacementBuilder> configure)
            where TService : class
            where TImplementation : class, TService
        {
            ThrowHelper.ThrowIfNull(services);
            ThrowHelper.ThrowIfNull(configure);

            var descriptor = ServiceDescriptor.Singleton<TService, TImplementation>();
            configure(new DependencyReplacementBuilder(descriptor));
            services.Add(descriptor);
            return services;
        }

        /// <summary>
        /// Finds the last registered service of type <typeparamref name="TService"/> and allows
        /// configuring dependency replacements for it.
        /// </summary>
        /// <typeparam name="TService">The service type to find.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to search.</param>
        /// <param name="configure">A delegate to configure dependency replacements.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <exception cref="InvalidOperationException">
        /// No service of type <typeparamref name="TService"/> has been registered.
        /// </exception>
        public static IServiceCollection For<TService>(
            this IServiceCollection services,
            Action<DependencyReplacementBuilder> configure)
            where TService : class
        {
            ThrowHelper.ThrowIfNull(services);
            ThrowHelper.ThrowIfNull(configure);

            ServiceDescriptor? descriptor = null;
            for (int i = services.Count - 1; i >= 0; i--)
            {
                if (services[i].ServiceType == typeof(TService))
                {
                    descriptor = services[i];
                    break;
                }
            }

            if (descriptor is null)
            {
                throw new InvalidOperationException(SR.Format(SR.NoServiceRegistered, typeof(TService)));
            }

            configure(new DependencyReplacementBuilder(descriptor));
            return services;
        }
    }
}
