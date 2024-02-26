using System.Text;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Xunit.Abstractions;

namespace Deveel.Pipelines {
	public class BuildAndExecuteTests {
		private readonly ITestOutputHelper outputHelper;
		private readonly IServiceProvider services;

		public BuildAndExecuteTests(ITestOutputHelper outputHelper) {
			this.outputHelper = outputHelper;
			this.services = new ServiceCollection()
				.AddLogging(builder => builder.AddXUnit(outputHelper))
				.BuildServiceProvider();
		}

		[Fact]
		public async Task EmptyBuilder_Executes_NoError() {
			var pipeline = new TestPipelineBuilder()
				.Build(new TestBuildContext(services));

			Assert.NotNull(pipeline);
			Assert.Null(pipeline.Root);

			await pipeline.ExecuteAsync();
		}

		[Fact]
		public async Task SingleStepBuilder_Executes_NoError() {
			var pipeline = new TestPipelineBuilder()
				.Use<SimpleHandler>()
				.Build();

			Assert.NotNull(pipeline);
			Assert.NotNull(pipeline.Root);
			Assert.Null(pipeline.Root.Next);

			await pipeline.ExecuteAsync();
		}

		[Fact]
		public async Task MultipleStepBuilder_Executes_NoError() {
			var pipeline = new TestPipelineBuilder()
				.Use<SimpleHandler>()
				.Use<SimpleHandler>()
				.Build();

			Assert.NotNull(pipeline);
			Assert.NotNull(pipeline.Root);
			Assert.NotNull(pipeline.Root.Next);
			Assert.Null(pipeline.Root.Next.Next);

			await pipeline.ExecuteAsync();
		}

		[Fact]
		public async Task SingleContractStepBuilder_Executes_NoError() {
			var pipeline = new TestPipelineBuilder()
				.Use<HandlerByContract>()
				.Build();

			Assert.NotNull(pipeline);
			Assert.NotNull(pipeline.Root);

			await pipeline.ExecuteAsync();
		}

		[Fact]
		public void SingleInvalidContractStepBuilder_Build_Error() {
			var builder = new TestPipelineBuilder()
				.Use<HandlerWithInvalidContract>();

			Assert.Throws<PipelineBuildException>(() => builder.Build());
		}

		[Fact]
		public async Task SingleStepWithArgsBuilder_Executes_NoError() {
			var pipeline = new TestPipelineBuilder()
				.Use<HandlerWithArgs>("test")
				.Build();

			Assert.NotNull(pipeline);
			Assert.NotNull(pipeline.Root);

			await pipeline.ExecuteAsync();
		}

		[Fact]
		public async void SingleStepWithArgsBuilder_Execute_Error() {
			var pipeline = new TestPipelineBuilder()
				.Use<HandlerWithArgs>()
				.Build();

			await Assert.ThrowsAsync<PipelineException>(() => pipeline.ExecuteAsync());
		}

		[Fact]
		public async Task MultipleStepsInSequenceChangingValue_Executes_ValueIsChanined() {
			var pipeline = new TestPipelineBuilder()
				.Use<ChangeValueHandler>("test")
				.Use<ChangeValueHandler>("test2")
				.Build(new TestBuildContext(services));

			Assert.NotNull(pipeline);

			var context = new TestContext();
			await pipeline.ExecuteAsync(context);

			Assert.NotNull(context.Value);
			Assert.IsType<string>(context.Value);
			Assert.Equal("testtest2", context.Value);
		}

		[Fact]
		public async Task MultipleStepsWithCustomDelegateHandler_Executes_NoError() {
			var pipeline = new TestPipelineBuilder()
				.Use<CustomDelegateHandler>()
				.Use<SimpleHandler>()
				.Build(new TestBuildContext(services));

			Assert.NotNull(pipeline);

			await pipeline.ExecuteAsync();
		}

		[Fact]
		public async Task MultipleStepsWithoutNext_Executes_NextIsCalledByPipeline() {
			var pipeline = new TestPipelineBuilder()
				.Use<HandlerWithoutNext>()
				.Use<ChangeValueHandler>("test")
				.Build(new TestBuildContext(services));

			Assert.NotNull(pipeline);

			var context = new TestContext();

			await pipeline.ExecuteAsync(context);

			Assert.NotNull(context);
			Assert.NotNull(context.Value);
			Assert.IsType<string>(context.Value);
			Assert.Equal("test", context.Value);
		}

		[Fact]
		public async Task MultipleStepsWithNext_Executes_NextIsCalledByHandler() {
			var pipeline = new TestPipelineBuilder()
				.Use<CustomDelegateHandler>()
				.Use<ChangeValueHandler>("test")
				.Build(new TestBuildContext(services));

			Assert.NotNull(pipeline);

			var context = new TestContext();

			await pipeline.ExecuteAsync(context);

			Assert.NotNull(context);
			Assert.NotNull(context.Value);
			Assert.IsType<string>(context.Value);
			Assert.Equal("test", context.Value);
		}


		[Fact]
		public async Task SingleInlineHandler_Executes_NoError() {
			var pipeline = new TestPipelineBuilder()
				.Use(context => {
					return Task.CompletedTask;
				})
				.Build(new TestBuildContext(services));

			Assert.NotNull(pipeline);

			await pipeline.ExecuteAsync();
		}

		[Fact]
		public async Task MultipleMixedStepsInlineAndServiced_Executes_NoError() {
			var pipeline = new TestPipelineBuilder()
				.Use<SimpleHandler>()
				.Use(context => {
					return Task.CompletedTask;
				})
				.Build(new TestBuildContext(services));

			Assert.NotNull(pipeline);

			await pipeline.ExecuteAsync();
		}

		[Fact]
		public async Task SingleInlineHandlerWithNext_Executes_NoError() {
			var pipeline = new TestPipelineBuilder()
				.Use((context, next) => {
					return next?.Invoke(context) ?? Task.CompletedTask;
				})
				.Build(new TestBuildContext(services));

			Assert.NotNull(pipeline);

			await pipeline.ExecuteAsync();
		}

		[Fact]
		public async Task SingleSynchHandler_Executes_NoError() {
			var pipeline = new TestPipelineBuilder()
				.Use<SynchHandler>()
				.Build();

			Assert.NotNull(pipeline);

			await pipeline.ExecuteAsync();
		}

		[Fact]
		public async Task SingleSynchHandlerWithNext_Executes_NoError() {
			var pipeline = new TestPipelineBuilder()
				.Use<SynchHandlerWithNext>()
				.Build();

			Assert.NotNull(pipeline);

			await pipeline.ExecuteAsync();
		}

		#region Handlers

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

		private class CustomDelegateHandler {
			private readonly ILogger logger;

			public CustomDelegateHandler(ILogger<CustomDelegateHandler> logger) {
				this.logger = logger;
			}

			public async Task HandleAsync(TestContext context, CustomDelegate? next) {
				try {
					logger.LogInformation("Executing custom delegate handler");
					await (next?.Invoke(context) ?? Task.CompletedTask);
				} finally {
					logger.LogInformation("Custom delegate handler executed");
				}
			}
		}

		private delegate Task CustomDelegate(TestContext context);

		class HandlerWithoutNext {
			public Task HandleAsync(TestContext context) {
				return Task.CompletedTask;
			}
		}

		class HandlerWithInvalidContract {
			public Task Invoke(TestContext context, ExecutionDelegate<TestContext>? next) {
				return Task.CompletedTask;
			}
		}

		class SynchHandler {
			public void Handle(TestContext context) {
				// Do nothing
			}
		}

		class SynchHandlerWithNext {
			public void Handle(TestContext context, ExecutionDelegate<TestContext>? next) {
				// Do nothing
			}
		}

		#endregion
	}
}
