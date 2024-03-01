# Deveel Pipelines

A simple, _low-ambition_, library for writing pipeline-driven C# applications, with a focus on simplicity and ease of use.

## Pipeline-Driven Development

Pipeline-driven development is a software development approach that focuses on the creation of a series of stages, each of which is responsible for a specific task, and that can be composed together to form a pipeline.

When dealing with complex processing and multi-stage operations, the pipeline-driven development can be a very effective approach to manage the complexity of the application, and to make it easier to understand, maintain and extend.

## Installation

The library is available as a NuGet package, and can be installed using the following command:

```bash
dotnet add package Deveel.Pipelines
```

or using the NuGet Package Manager:

```bash
Install-Package Deveel.Pipelines
```

## Usage

The library is designed to be simple and easy to use, and it's based on the concept of a pipeline, which is a series of _steps__ that are executed in sequence.

To create a pipeline, you can implement a `PipelineBuilder`, which allows you to add steps to the pipeline, and to build the pipeline itself: this library provides a base contracts to define it, but it leaves the implementation to the user, specifying the type of context that has to be used to pass data between stages.

A context that can be used in a pipeline must inherit from the `ExecutionContext` class, which provides a simple interface to pass data between stages, and to control the execution of the pipeline: pipeline builders and pipelines require a context type to be specified, and the context is used to pass data between stages.

```csharp
public class MyContext : ExecutionContext {
  public string Name { get; set; }

  public int Age { get; set; }
}

public class MyPipelineBuilder : PipelineBuilder<MyContext> {
    public MyPipeline() {
        AddSep<FirstStage>();
        AddStep<SecondStage>()
        AddStep(context => { });
   }
}
```

### Building a Pipeline

To build a pipeline, you can implement a `PipelineBuilder` that is responsible to define the steps that are part of the pipeline, and to build the pipeline itself.

The `PipelineBuilder` is a simple class that can be used to define the steps of the pipeline, and to build the pipeline itself.

```csharp
public class MyPipelineBuilder : PipelineBuilder<MyContext> {
    public MyPipeline() {
        AddStep<MyMainStage>();
    }

    public MyPipelineBuilder Use<THandler>() {
        AddStep<THandler>();
        return this;
    }
}
```

The design motivation behind not providing a default instance of the pipeline builder is to let the user to create a custom builder, that allows to chain the steps in a more fluent way, and to provide a better control over the pipeline.

```csharp
var pipeline = new MyPipelineBuilder()
	.Use<FirstStage>()
	.Use<SecondStage>()
	.Use(context => { })
	.Build();
```

If any of the steps in the pipeline depends on other services, it is possible to register them in the service container, and pass the locator to the pipeline builder, so that the steps can be resolved from the container.

```csharp
var services = new ServiceCollection();
services.AddSingleton<IAgeCalculator, AgeCalculator>();
services.AddSingleton<ICountryResolver, CountryResolver>();

var serviceProvider = services.BuildServiceProvider();

var pipeline = new MyPipelineBuilder()
	.Use<FirstStage>()
	.Use<SecondStage>()
	.Use(context => { })
	.Build(serviceProvider);
```

### Steps

To execute a step of a pipeline, you can use two different approaches, which are composabile together:

- **Service Steps**: a step that is implemented as a service, and that can be registered in the pipeline builder, and that can be resolved and executed by the pipeline itself.
- **Delegate Steps**: a step that is implemented as a delegate, and that can be added to the pipeline builder directly.

#### Service Steps

This library provides two methodologies to implement a service step, that is a step that is implemented as a service, and that can be registered in the pipeline builder, resolving its dependencies from the service container.

##### `IExecutionHandler<TContext>`

The `IExecutionHandler` interface provides a simple contract to implement a step using the `HandleAsync` method, which is called by the pipeline to execute the step.

Although it provides less flexibility than the convention-based approach, it is ultimately provides a better control over the execution of the step, since it's an explicit contract.

```csharp
public class AgeCalculationStage : IExecutionHandler<MyContext> {
    private readonly IAgeCalculator _ageCalculator;

    public FirstStage(IAgeCalculator ageCalculator) {
		_ageCalculator = ageCalculator;
	}

    public async Task HandleAsync(MyContext context, ExecutionDelegate<MyContext>? next) {
        context.Age = await _ageCalculator.CalculateAgeAsync(context.Name);
    }
}
```

It is possible to register the step in the pipeline builder, by using the `AddStep` method:

```csharp
public class MyPipelineBuilder : PipelineBuilder<MyContext> {
	public MyPipeline() {
		AddStep<AgeCalculationStage>();
	}
}
```

##### By Convention

It is also possible to implement a convention-based approach to define a step, by implementing a class that has a method that follows a specific convention:

* A `HandleAsync` or `Handle` method that takes at least one argument of the context type
* Optionally can accept an `ExecutionDelegate<TContext>` as a second argument, to call the next step in the pipeline.
* Alternatively to specifying the `ExecutionDelegate<TContext>` as a parameter, the method can accept another type of delegate, that has a single argument of the context type, and that returns a `Task`.

A first example of convention-based step without the `ExecutionDelegate<TContext>`:

```csharp
public class CountryResolver {
    private readonly ICountryResolver _countryResolver;

    public FirstStage(ICountryResolver countryResolver) {
        _countryResolver = countryResolver;
    }

    public async Task ResolveCountry(MyContext context) {
        context.Country = await _countryResolver.ResolveCountryAsync(context.PhoneNumber);
    }
}
```

A second example of convention-based step with the `ExecutionDelegate<TContext>`:

```csharp
public class AgeCalculationStage {
    private readonly IAgeCalculator _ageCalculator;

    public FirstStage(IAgeCalculator ageCalculator) {
        _ageCalculator = ageCalculator;
    }

    public async Task HandleAsync(MyContext context, ExecutionDelegate<MyContext>? next) {
        await next?.Invoke(context);

        context.Age = await _ageCalculator.CalculateAgeAsync(context.Name);
    }
}
```

An example of convention-based step with a custom delegate:

```csharp
public delegate Task MyDelegate(MyContext context);

public class AgeCalculationStage {
    private readonly IAgeCalculator _ageCalculator;

    public FirstStage(IAgeCalculator ageCalculator) {
        _ageCalculator = ageCalculator;
    }

    public async Task HandleAsync(MyContext context, MyDelegate next) {
        await next(context);
        context.Age = await _ageCalculator.CalculateAgeAsync(context.Name);
    }
}
```

##### Arguments

When registering a service step in the pipeline builder, it is possible to specify an optional set of arguments that will be passed to the contructor of the step handler (when implementing the `IExecutionHandler<TContext>` interface), or to the method (when using the convention-based approach).

```csharp
public class MyPipelineBuilder : PipelineBuilder<MyContext> {
	public MyPipeline() {
		AddStep<AgeCalculationStage>(32);
	}
}

public class AgeCalculationStage : IExecutionHandler<MyContext> {
	private readonly IAgeCalculator _ageCalculator;
    private readonly int _maxAge;

	public FirstStage(IAgeCalculator ageCalculator, int maxAge) {
		_ageCalculator = ageCalculator;
	}

	public async Task HandleAsync(MyContext context, ExecutionDelegate<MyContext>? next) {
		context.Age = await _ageCalculator.CalculateAgeAsync(context.Name);

        if (context.Age > _maxAge)
			throw new InvalidOperationException("The age is too high");
	}
}
```

Or , using the convention-based approach:

```csharp

public class AgeCalculationStage {
	private readonly IAgeCalculator _ageCalculator;

	public FirstStage(IAgeCalculator ageCalculator) {
		_ageCalculator = ageCalculator;
	}

	public async Task HandleAsync(MyContext context, int maxAge) {
		context.Age = await _ageCalculator.CalculateAgeAsync(context.Name);

		if (context.Age > maxAge)
			throw new InvalidOperationException("The age is too high");
	}
}
```

#### Delegate Steps

A delegate step is a step that is implemented as a delegate, and that can be added to the pipeline builder directly.

```csharp
public class MyPipelineBuilder : PipelineBuilder<MyContext> {
	public MyPipeline() {
		AddStep(context => {
			context.Age = 32;
		});
	}
}
```

### Pipeline Execution

### The Next Step in the Pipeline

By contract and by convention, a step in the pipeline can call the next step in the pipeline, by invoking the `ExecutionDelegate<TContext>` that is passed as an argument to the `HandleAsync` method, or by using the `ExecutionDelegate<TContext>` delegate that is passed as an argument to the method.

```csharp
public class AgeCalculationStage : IExecutionHandler<MyContext> {
	private readonly IAgeCalculator _ageCalculator;

	public FirstStage(IAgeCalculator ageCalculator) {
		_ageCalculator = ageCalculator;
	}

	public async Task HandleAsync(MyContext context, ExecutionDelegate<MyContext>? next) {
		context.Age = await _ageCalculator.CalculateAgeAsync(context.Name);

		await next?.Invoke(context);
	}
}
```

The context of a pipeline (that implements `ExecutionContext`) is used also to track if the next step was explicitly called by an handler, and if so the pipeline executor will avoid a double execution of the next step.

## Contributing

The library is open to contributions, and we welcome any kind of help, from bug reports to pull requests.

If you want to contribute to the library, please read the [CONTRIBUTING.md](CONTRIBUTING.md) file for more information.