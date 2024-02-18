using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Pipelines {
	public class BuildTests {
		[Fact]
		public void EmptyBuilder() {
			var services = new ServiceCollection();
			var serviceProvider = services.BuildServiceProvider();

			var pipeline = new TestPipelineBuilder()
				.Build(new TestBuildContext(serviceProvider));

			Assert.NotNull(pipeline);
			Assert.Null(pipeline.Root);
		}

		[Fact]
		public void SingleStepBuilder() {
			var services = new ServiceCollection();
			var serviceProvider = services.BuildServiceProvider();

			var pipeline = new TestPipelineBuilder()
				.Use<SimpleHandler>()
				.Build(new TestBuildContext(serviceProvider));

			Assert.NotNull(pipeline);
			Assert.NotNull(pipeline.Root);
			Assert.Null(pipeline.Root.Next);
		}

		[Fact]
		public void MultipleStepBuilder() {
			var services = new ServiceCollection();
			var serviceProvider = services.BuildServiceProvider();

			var pipeline = new TestPipelineBuilder()
				.Use<SimpleHandler>()
				.Use<SimpleHandler>()
				.Build(new TestBuildContext(serviceProvider));

			Assert.NotNull(pipeline);
			Assert.NotNull(pipeline.Root);
			Assert.NotNull(pipeline.Root.Next);
			Assert.Null(pipeline.Root.Next.Next);
		}

		[Fact]
		public void SingleContractStepBuilder() {
			var services = new ServiceCollection();
			var serviceProvider = services.BuildServiceProvider();

			var pipeline = new TestPipelineBuilder()
				.Use<HandlerByContract>()
				.Build(new TestBuildContext(serviceProvider));

			Assert.NotNull(pipeline);
			Assert.NotNull(pipeline.Root);
		}

		[Fact]
		public void SingleStepWithArgsBuilder() {
			var services = new ServiceCollection();
			var serviceProvider = services.BuildServiceProvider();

			var pipeline = new TestPipelineBuilder()
				.Use<HandlerWithArgs>("test")
				.Build(new TestBuildContext(serviceProvider));

			Assert.NotNull(pipeline);
			Assert.NotNull(pipeline.Root);
		}

		[Fact]
		public void SingleStepWithServiceBuilder() {
			var services = new ServiceCollection();
			var serviceProvider = services.BuildServiceProvider();

			var pipeline = new TestPipelineBuilder()
				.Use<HandlerWithService>()
				.Build(new TestBuildContext(serviceProvider));

			Assert.NotNull(pipeline);
			Assert.NotNull(pipeline.Root);
		}

		[Fact]
		public void InvalidHandlerType() {
			var services = new ServiceCollection();
			var serviceProvider = services.BuildServiceProvider();

			Assert.Throws<PipelineBuildException>(() => {
				new TestPipelineBuilder()
					.Use<InvalidHandler>()
					.Build(new TestBuildContext(serviceProvider));
			});
		}

		[Fact]
		public void SingleHandlerWithoutNext() {
			var services = new ServiceCollection();
			var serviceProvider = services.BuildServiceProvider();
			
			var pipeline = new TestPipelineBuilder()
				.Use<HandlerWithoutNext>()
				.Build(new TestBuildContext(serviceProvider));

			Assert.NotNull(pipeline);
			Assert.NotNull(pipeline.Root);
			Assert.Null(pipeline.Root.Next);
		}

		class SimpleHandler : IExecutionHandler<TestContext> {
			public Task HandleAsync(TestContext context, ExecutionDelegate<TestContext>? next) {
				return Task.CompletedTask;
			}
		}

		class HandlerByContract {
			public Task HandleAsync(TestContext context, ExecutionDelegate<TestContext>? next) {
				return Task.CompletedTask;
			}
		}

		class HandlerWithArgs {
			public Task HandleAsync(string arg, TestContext context, ExecutionDelegate<TestContext>? next) {
				return Task.CompletedTask;
			}
		}

		class HandlerWithService {
			public HandlerWithService(IServiceProvider serviceProvider) {
				ServiceProvider = serviceProvider;
			}

			public IServiceProvider ServiceProvider { get; }

			public Task HandleAsync(TestContext context, ExecutionDelegate<TestContext>? next) {
				return Task.CompletedTask;
			}
		}

		class InvalidHandler {
			public Task HandleAsync() {
				return Task.CompletedTask;
			}
		}

		class HandlerWithoutNext {
			public Task HandleAsync(TestContext context) {
				return Task.CompletedTask;
			}
		}
	}
}
