namespace Deveel.Pipelines {
	public class TestPipelineBuilder : PipelineBuilder<TestContext> {
		public TestPipelineBuilder Use<THandler>(params object[] args) {
			AddStep(new PipelineStep(typeof(THandler), args));
			return this;
		}

		public TestPipeline Build(TestBuildContext buildContext) {
			return new TestPipeline(BuildExecution(buildContext));
		}
	}
}
