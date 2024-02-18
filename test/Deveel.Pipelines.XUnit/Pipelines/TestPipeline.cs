namespace Deveel.Pipelines {
	public class TestPipeline : Pipeline<TestContext> {
		internal TestPipeline(PipelineExecutionNode<TestContext> executionRoot) 
			: base(executionRoot) {
		}

		public PipelineExecutionNode<TestContext> Root => ExecutionRoot;
	}
}
