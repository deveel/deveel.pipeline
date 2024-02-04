using System.Text;

using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Pipelines {
	public class BuildAndExecuteTests {
		[Fact]
		public async Task EmptyBuilder() {
			var services = new ServiceCollection();
			var serviceProvider = services.BuildServiceProvider();

			var pipeline = new TestPipelineBuilder()
				.Build(new TestBuildContext(serviceProvider));

			Assert.NotNull(pipeline);
			Assert.Null(pipeline.Root);

			await pipeline.ExecuteAsync(new TestContext(serviceProvider, CancellationToken.None));
		}

		[Fact]
		public async Task SingleStepBuilder() {
			var services = new ServiceCollection();
			var serviceProvider = services.BuildServiceProvider();

			var pipeline = new TestPipelineBuilder()
				.Use<SimpleHandler>()
				.Build(new TestBuildContext(serviceProvider));

			Assert.NotNull(pipeline);
			Assert.NotNull(pipeline.Root);
			Assert.Null(pipeline.Root.Next);

			await pipeline.ExecuteAsync(new TestContext(serviceProvider));
		}

		[Fact]
		public async Task MultipleStepBuilder() {
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

			await pipeline.ExecuteAsync(new TestContext(serviceProvider));
		}

		[Fact]
		public async Task SingleContractStepBuilder() {
			var services = new ServiceCollection();
			var serviceProvider = services.BuildServiceProvider();

			var pipeline = new TestPipelineBuilder()
				.Use<HandlerByContract>()
				.Build(new TestBuildContext(serviceProvider));

			Assert.NotNull(pipeline);
			Assert.NotNull(pipeline.Root);

			await pipeline.ExecuteAsync(new TestContext(serviceProvider));
		}

		[Fact]
		public async Task SingleStepWithArgsBuilder() {
			var services = new ServiceCollection();
			var serviceProvider = services.BuildServiceProvider();

			var pipeline = new TestPipelineBuilder()
				.Use<HandlerWithArgs>("test")
				.Build(new TestBuildContext(serviceProvider));

			Assert.NotNull(pipeline);
			Assert.NotNull(pipeline.Root);

			await pipeline.ExecuteAsync(new TestContext(serviceProvider));
		}

		[Fact]
		public async Task MultipleStepsInSequenceChangingValue() {
			var services = new ServiceCollection();
			var serviceProvider = services.BuildServiceProvider();

			var pipeline = new TestPipelineBuilder()
				.Use<ChangeValueHandler>("test")
				.Use<ChangeValueHandler>("test2")
				.Build(new TestBuildContext(serviceProvider));
		}

		private class SimpleHandler : IExecutionHandler<TestContext> {
			public Task HandleAsync(TestContext context, ExecutionDelegate<TestContext>? next)
				=> Task.CompletedTask;
		}

		private class HandlerByContract {
			public Task HandleAsync(TestContext context, ExecutionDelegate<TestContext>? next)
				=> Task.CompletedTask;
		}

		private class HandlerWithArgs {
			public Task HandleAsync(string arg, TestContext context, ExecutionDelegate<TestContext>? next)
				=> Task.CompletedTask;
		}

		private class ChangeValueHandler {
			public Task HandleAsync(string arg, TestContext context, ExecutionDelegate<TestContext>? next) {
				if (context.Value == null) {
					context.Value = arg;
				} else {
					var sb = new StringBuilder((string)context.Value);
					sb.Append(arg);
					context.Value = sb.ToString();
				}

				return Task.CompletedTask;
			}
		}
	}
}
