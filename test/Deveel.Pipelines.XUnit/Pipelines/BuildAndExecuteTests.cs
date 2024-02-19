﻿using System.Text;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Xunit.Abstractions;

namespace Deveel.Pipelines {
	public class BuildAndExecuteTests {
		private readonly ITestOutputHelper outputHelper;

		public BuildAndExecuteTests(ITestOutputHelper outputHelper) {
			this.outputHelper = outputHelper;
		}

		[Fact]
		public async Task EmptyBuilder() {
			var services = new ServiceCollection();
			var serviceProvider = services.BuildServiceProvider();

			var pipeline = new TestPipelineBuilder()
				.Build(new TestBuildContext(serviceProvider));

			Assert.NotNull(pipeline);
			Assert.Null(pipeline.Root);

			await pipeline.ExecuteAsync(new TestContext(serviceProvider));
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

			Assert.NotNull(pipeline);

			var context = new TestContext(serviceProvider);
			await pipeline.ExecuteAsync(context);

			Assert.NotNull(context.Value);
			Assert.IsType<string>(context.Value);
			Assert.Equal("testtest2", context.Value);
		}

		[Fact]
		public async Task MultipleStepsWithCustomDelegateHandler() {
			var services = new ServiceCollection();
			services.AddLogging(builder => builder.AddXUnit(outputHelper));

			var serviceProvider = services.BuildServiceProvider();

			var pipeline = new TestPipelineBuilder()
				.Use<CustomDelegateHandler>()
				.Use<SimpleHandler>()
				.Build(new TestBuildContext(serviceProvider));

			Assert.NotNull(pipeline);
			var context = new TestContext(serviceProvider);

			await pipeline.ExecuteAsync(context);

			Assert.NotNull(context);
		}

		[Fact]
		public async Task MultipleStepsWithoutNext() {
			var services = new ServiceCollection();
			var serviceProvider = services.BuildServiceProvider();

			var pipeline = new TestPipelineBuilder()
				.Use<HandlerWithoutNext>()
				.Use<ChangeValueHandler>("test")
				.Build(new TestBuildContext(serviceProvider));

			Assert.NotNull(pipeline);
			var context = new TestContext(serviceProvider);

			await pipeline.ExecuteAsync(context);

			Assert.NotNull(context);
			Assert.NotNull(context.Value);
		}

		[Fact]
		public async Task SingleInlineHandler() {
			var services = new ServiceCollection();
			var serviceProvider = services.BuildServiceProvider();

			var pipeline = new TestPipelineBuilder()
				.Use(context => {
					return Task.CompletedTask;
				})
				.Build(new TestBuildContext(serviceProvider));

			Assert.NotNull(pipeline);
			var context = new TestContext(serviceProvider);

			await pipeline.ExecuteAsync(context);

			Assert.NotNull(context);
		}

		[Fact]
		public async Task MultipleMixedStepsInlineAndServiced() {
			var services = new ServiceCollection();
			var serviceProvider = services.BuildServiceProvider();

			var pipeline = new TestPipelineBuilder()
				.Use<SimpleHandler>()
				.Use(context => {
					return Task.CompletedTask;
				})
				.Build(new TestBuildContext(serviceProvider));

			Assert.NotNull(pipeline);

			var context = new TestContext(serviceProvider);
			await pipeline.ExecuteAsync(context);

			Assert.NotNull(context);
		}

		[Fact]
		public async Task SingleInlineHandlerWithNext() {
			var services = new ServiceCollection();
			var serviceProvider = services.BuildServiceProvider();

			var pipeline = new TestPipelineBuilder()
				.Use((context, next) => {
					return next?.Invoke(context) ?? Task.CompletedTask;
				})
				.Build(new TestBuildContext(serviceProvider));

			Assert.NotNull(pipeline);

			var context = new TestContext(serviceProvider);
			await pipeline.ExecuteAsync(context);

			Assert.NotNull(context);
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
	}
}
