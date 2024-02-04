namespace Deveel.Pipelines {
	public class TestContext : IExecutionContext {
		public TestContext(IServiceProvider services, CancellationToken executionCancelled = default) {
			Services = services;
			ExecutionCancelled = executionCancelled;
		}

		public IServiceProvider Services { get; }

		public CancellationToken ExecutionCancelled { get; }

		public object? Value { get; set; }
	}
}
