# Request to introduce Replacements within the .NET Dependency Injection framework as a better alternative to keyed services.

























# DependencyInjection Specifications

The goal of this document is to specify a better alternative to keyed services in Dependency Injection (DI) frameworks.
Keyed services are often used to resolve dependencies based on a key or identifier,
but they can lead to code that is hard to maintain and understand.
This specification proposes a more type-safe and maintainable approach.

## The Current Behavior of the .NET Dependency Injection Framework

The .NET Dependency Injection framework (DI) allows for the registration and resolution of services based on types.

To register a service, you typically use the `IServiceCollection` interface:
```csharp
IServiceCollection services = new ServiceCollection();
services.AddTransient<IMyService, MyServiceImplementation>();
```

You can also use singletons, or scoped lifetimes.
Additionally you can use factories or instances to create services.

```csharp
services.AddScoped<IMyService>(provider => new MyServiceImplementation());
services.AddSingleton<IMyService>(new MyServiceImplementation());
```

Later you can build an `IServiceProvider` to resolve services:
```csharp

IServiceProvider serviceProvider = services.BuildServiceProvider();
IMyService myService = serviceProvider.GetRequiredService<IMyService>();

```

Note: This can handle classes with dependencies.
```csharp
public class MyServiceImplementation : IMyService
{
    private readonly IOtherService _otherService;
    public MyServiceImplementation(IOtherService otherService)
    {
        _otherService = otherService;
    }
}

services.AddTransient<IOtherService, OtherServiceImplementation>();
services.AddTransient<IMyService, MyServiceImplementation>();
var serviceProvider = services.BuildServiceProvider();
var myService = serviceProvider.GetRequiredService<IMyService>();
// myService will have an instance of OtherServiceImplementation injected into it.
```

## The Problem Keyed Services Are Trying to Solve

Suppose you have multiple implementations of the same interface. How do you choose which one to use at runtime?

Keyed Services solve this by allowing types to be registered with a unique key.

For example:
```csharp
services.AddKeyedTransient<IMyService, MyServiceImplementationA>("ImplementationA");
services.AddKeyedTransient<IMyService, MyServiceImplementationB>("ImplementationB");
```
Then, when resolving the service, you would specify the key:
```csharp
var myServiceA = serviceProvider.GetRequiredKeyedService("ImplementationA");
var myServiceB = serviceProvider.GetRequiredKeyedService("ImplementationB");
// myServiceA is an instance of MyServiceImplementationA
// myServiceB is an instance of MyServiceImplementationB
```
Additionally, you can resolve class dependencies using the `FromKeyedServicesAttribute`:
```csharp
public class MyConsumer
{
    private readonly IMyService _myService;
    public MyConsumer([FromKeyedServices("ImplementationA")] IMyService myService)
    {
        _myService = myService;
    }
}

services.AddTransient<MyConsumer>();
services.AddKeyedTransient<IMyService, MyServiceImplementationA>("ImplementationA");
var consumer = serviceProvider.GetRequiredService<MyConsumer>();
// consumer._myService will be an instance of MyServiceImplementationA
```

It should be noted that keys can be any compile-time object such as strings, numbers, enums, or types (ie typeof(SomeType)).


## Why is this a problem?

Keyed services are a flawed approach for several reasons:

### Tight Coupling between code and DI configuration

When using keyed services, especially when using attributes like `FromKeyedServicesAttribute`, the code becomes tightly coupled to the DI configuration.
A class must know: that it is using keyed services, the specific keys being used, and the DI framework itself.
A class should only have one reason to change, and that reason should not be changes to DI configuration (ie changing keys).
Dependency orchestration is the responsibility of the startup logic, not the individual classes.

### Lack of Type Safety

Keys are often strings or other primitive types, which can lead to runtime errors if a key is misspelled or changed.
Additionally say you wanted to use types as keys:
```csharp
public class MyConsumer
{
    private readonly IMyService _myService;
    public MyConsumer([FromKeyedServices(typeof(MyServiceImplementationA))] IMyService myService)
    {
        _myService
    }
}
```
This is not much better than `MyConsumer` instantiating its own dependencies directly..

Suppose you use the conuming type as the key:
```csharp
public class MyConsumer
{
    private readonly IMyService _myService;
    public MyConsumer([FromKeyedServices(typeof(MyConsumer))] IMyService myService)
    {
        _myService
    }
}
```

But then you can't reuse 'MyConsumer' if 'MyConsumer' has additional dependencies.

Suppose you make a custom type-safe key:
```csharp
public interface IKey<TService, TImplementation>;
public class MyConsumer
{
    private readonly IMyService _myService;
    public MyConsumer([FromKeyedServices(typeof(IKey<IMyService, MyServiceImplementationA>))] IMyService myService)
    {
        _myService
    }
}

services.AddKeyedTransient<IMyService, MyServiceImplementationA, IKey<IMyService, MyServiceImplementationA>>();
```
This is marginally better (if it even compiles and works - im not sure)
but keys are now very verbose, and registration is more verbose.


And we still have the problem of tight coupling to DI configuration.

## Solution: Allow DI Replacements

Instead of using keys
we should be able to, at startup time,
replace dependencies for individual services.


Consider when we have two loggers and we want to use one logger for one consumer and another logger for another consumer.
```csharp

public interface ILogger;
public class FileLogger : ILogger;
public class DatabaseLogger : ILogger;

public class SomeUserActionService(ILogger logger); // We want this to use the database logger
public class SomeCriticalService(ILogger logger); // We want this to use the file logger
```


```csharp
var services = new ServiceCollection();
services.AddTransient<SomeUserActionService>(with => with.Replace<ILogger, DatabaseLogger>());
//also should be able to use after the fact replacement
services.For<SomeCriticalService>().Replace<ILogger, FileLogger>();

```
