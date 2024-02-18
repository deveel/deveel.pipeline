namespace Deveel.Pipelines {
	public class TestContext : PipelineExecutionContext {
		public TestContext(IServiceProvider services) : base(services) {
		}

		public object? Value { get; set; }
	}
}
