// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Extensions.DependencyInjection.Dev.Tests
{
    public interface IDI { string Name { get; } }
    public abstract record DI(string Name) : IDI;
    public interface IService : IDI;
    public interface IDependency : IDI;
    public interface IOtherDependency : IDI;
    public record Service(IDependency Dependency, IOtherDependency OtherDependency) : DI($"Service with {Dependency.Name} and {OtherDependency.Name}"), IService;
    public record Dependency() : DI("Dependency"), IDependency;
    public record OtherDependency() : DI("OtherDependency"), IOtherDependency;
    public record AlternateDependency() : DI("AlternateDependency"), IDependency;

    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {

        }

        [Fact]
        public void Debug_RegistratingServices()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<IService, Service>();
            serviceCollection.AddTransient<IDependency, Dependency>();
            serviceCollection.AddTransient<IOtherDependency, OtherDependency>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var service = serviceProvider.GetRequiredService<IService>();
            Assert.Equal("Service with Dependency and OtherDependency", service.Name);
        }

        [Fact]
        public void ReplaceTransient_OverridesDependencyForSpecificService()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<IService, Service>(with =>
                with.Replace<IDependency, AlternateDependency>());
            serviceCollection.AddTransient<IDependency, Dependency>();
            serviceCollection.AddTransient<IOtherDependency, OtherDependency>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            // IService should get AlternateDependency instead of Dependency
            var service = serviceProvider.GetRequiredService<IService>();
            Assert.Equal("Service with AlternateDependency and OtherDependency", service.Name);

            // Resolving IDependency directly should still return the original Dependency
            var dependency = serviceProvider.GetRequiredService<IDependency>();
            Assert.Equal("Dependency", dependency.Name);
        }

        [Fact]
        public void ForService_OverridesDependencyAfterRegistration()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<IService, Service>();
            serviceCollection.AddTransient<IDependency, Dependency>();
            serviceCollection.AddTransient<IOtherDependency, OtherDependency>();

            // After-the-fact replacement
            serviceCollection.For<IService>(with =>
                with.Replace<IDependency, AlternateDependency>());

            var serviceProvider = serviceCollection.BuildServiceProvider();

            // IService should get AlternateDependency instead of Dependency
            var service = serviceProvider.GetRequiredService<IService>();
            Assert.Equal("Service with AlternateDependency and OtherDependency", service.Name);

            // Resolving IDependency directly should still return the original Dependency
            var dependency = serviceProvider.GetRequiredService<IDependency>();
            Assert.Equal("Dependency", dependency.Name);
        }
    }
}
