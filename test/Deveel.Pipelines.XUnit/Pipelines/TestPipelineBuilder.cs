namespace Deveel.Pipelines {
	public class TestPipelineBuilder : PipelineBuilder<TestContext> {
		public TestPipelineBuilder Use<THandler>(params object[] args) {
			AddStep(typeof(THandler), args);
			return this;
		}

		public TestPipelineBuilder Use(ExecutionDelegate<TestContext> func) {
			AddStep(func);
			return this;
		}

		public TestPipelineBuilder Use(Func<TestContext, ExecutionDelegate<TestContext>, Task> func) { 
			AddStep(func);
			return this;
		}

		public TestPipeline Build(TestBuildContext buildContext) {
			return new TestPipeline(BuildExecution(buildContext));
		}
	}
}
