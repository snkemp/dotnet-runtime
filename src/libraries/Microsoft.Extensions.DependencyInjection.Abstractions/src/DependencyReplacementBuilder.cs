// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// A builder that allows configuring dependency replacements for a specific service registration.
    /// </summary>
    public sealed class DependencyReplacementBuilder
    {
        private readonly ServiceDescriptor _descriptor;

        internal DependencyReplacementBuilder(ServiceDescriptor descriptor)
        {
            _descriptor = descriptor;
        }

        /// <summary>
        /// Replaces the dependency <typeparamref name="TDependency"/> with <typeparamref name="TReplacement"/>
        /// when constructing the service described by this builder.
        /// </summary>
        /// <typeparam name="TDependency">The dependency type to replace.</typeparam>
        /// <typeparam name="TReplacement">The replacement implementation type.</typeparam>
        /// <returns>This builder for chaining.</returns>
        public DependencyReplacementBuilder Replace<TDependency, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TReplacement>()
            where TDependency : class
            where TReplacement : class, TDependency
        {
            _descriptor.AddDependencyReplacement(typeof(TDependency), typeof(TReplacement));
            return this;
        }
    }
}
