using System;
using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VSDiagnostics;

namespace Microsoft.DependencyInjection.Benchmarks;

public interface IDependency;
public class DependencyA : IDependency;
public class DependencyB : IDependency;

public interface IService;
public class Service(IDependency dependency) : IService
{
    public IDependency Dependency { get; } = dependency;
}

[MemoryDiagnoser]
public class RegistrationBenchmarks
{
    [Benchmark]
    public void Registration()
    {
        IServiceCollection serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<IDependency, DependencyA>();
        serviceCollection.AddTransient<IService, Service>();
    }

    [Benchmark]
    public void Factory()
    {
        IServiceCollection serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<IDependency>(sp => new DependencyA());
        serviceCollection.AddTransient<IService>(sp => new Service(sp.GetRequiredService<IDependency>()));
    }

    [Benchmark]
    public void Replacement()
    {
        IServiceCollection serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<IDependency, DependencyA>();
        serviceCollection.AddTransient<IService, Service>(with =>
        {
            with.Replace<IDependency, DependencyB>();
        });
    }

    [Benchmark]
    public void AfterTheFactReplacement()
    {
        IServiceCollection serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<IDependency, DependencyA>();
        serviceCollection.AddTransient<IService, Service>();
        serviceCollection.For<IService>(with => with.Replace<IDependency, DependencyB>());
    }

    [Benchmark]
    public void ResolveRegistration()
    {
        IServiceCollection serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<IDependency, DependencyA>();
        serviceCollection.AddTransient<IService, Service>();
        IService service = serviceCollection.BuildServiceProvider().GetRequiredService<IService>();
    }

    [Benchmark]
    public void ResolveFactory()
    {
        IServiceCollection serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<IDependency>(sp => new DependencyA());
        serviceCollection.AddTransient<IService>(sp => new Service(sp.GetRequiredService<IDependency>()));
        IService service = serviceCollection.BuildServiceProvider().GetRequiredService<IService>();
    }

    [Benchmark]
    public void ResolveReplacement()
    {
        IServiceCollection serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<IDependency, DependencyA>();
        serviceCollection.AddTransient<IService, Service>(with =>
        {
            with.Replace<IDependency, DependencyB>();
        });
        IService service = serviceCollection.BuildServiceProvider().GetRequiredService<IService>();
    }

    [Benchmark]
    public void ResolveAfterTheFactReplacement()
    {
        IServiceCollection serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<IDependency, DependencyA>();
        serviceCollection.AddTransient<IService, Service>();
        serviceCollection.For<IService>(with => with.Replace<IDependency, DependencyB>());
        IService service = serviceCollection.BuildServiceProvider().GetRequiredService<IService>();
    }
}
