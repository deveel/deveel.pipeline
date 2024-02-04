namespace Deveel.Pipelines {
	public class TestPipeline : Pipeline<TestContext> {
		internal TestPipeline(ExecutionCallback<TestContext> executionRoot) 
			: base(executionRoot) {
		}

		public ExecutionCallback<TestContext> Root => ExecutionRoot;
	}
}
