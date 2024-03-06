using Deveel.Pipelines;

using Humanizer;

using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection()
	.AddSingleton<INameNornalizer, DefaultNameNormalizer>()
	.AddMyPipeline(p => p
		.Use<AskName>()
		.Use<NormalizeName>()
		.Use<SayHello>())
		.BuildServiceProvider();

var pipeline = services.GetRequiredService<Pipeline<MyContext>>();

await pipeline.ExecuteAsync();


class MyPipelineBuilder : PipelineBuilder<MyContext> {
	public MyPipelineBuilder Use<THandler>() where THandler : class {
		AddStep<THandler>();
		return this;
	}

	public Pipeline<MyContext> Build(IServiceProvider services)
		=> BuildPipeline(new PipelineBuildContext(services));
}

class MyContext : PipelineExecutionContext {
	public string? Name { get; set; }
}

class AskName {
	public Task HandleAsync(MyContext context, ExecutionDelegate<MyContext> next) {
		Console.WriteLine("What's your name?");
		context.Name = Console.ReadLine();
		// TODO: invoke next -- currently it stops the pipeline after next
		return Task.CompletedTask;
	}
}

class SayHello {
	public void Handle(MyContext context) {
		Console.WriteLine($"Hello, {context.Name}");
	}
}

class NormalizeName {
	private readonly INameNornalizer nameNornalizer;

	public NormalizeName(INameNornalizer nameNornalizer) {
		this.nameNornalizer = nameNornalizer;
	}

	public async Task HandleAsync(MyContext context, ExecutionDelegate<MyContext> next) {
		context.Name = await nameNornalizer.NormalizeNameAsync(context.Name);
	}
}

static class ServiceCollectionExtensions {
	public static IServiceCollection AddMyPipeline(this IServiceCollection services, Action<MyPipelineBuilder> configure) {
		services.AddSingleton<Pipeline<MyContext>>(provider => {
			var builder = new MyPipelineBuilder();
			configure(builder);
			return builder.Build(provider);
		});

		return services;
	}
}

interface INameNornalizer {
	Task<string> NormalizeNameAsync(string? name);
}

class DefaultNameNormalizer : INameNornalizer {
	public Task<string> NormalizeNameAsync(string? name)
		=> Task.FromResult(name.Pascalize());
}