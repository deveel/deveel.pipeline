using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deveel.Pipelines {
	public class TestPipelineBuilder : IPipelineBuilder<TestContext> {
		public TestPipelineBuilder() {
			Steps = new PipelineStepsCollection();
		}

		public PipelineStepsCollection Steps { get; }

		Pipeline<TestContext> IPipelineBuilder<TestContext>.Build(IPipelineBuildContext buidContext)
			=> Build(buidContext);

		public TestPipelineBuilder Use<THandler>(params object[] args) {
			Steps.Add(new PipelineStep(typeof(THandler), args));
			return this;
		}

		public TestPipeline Build(IPipelineBuildContext buildContext) {
			return new TestPipeline(Steps.BuildExecution<TestContext>(buildContext));
		}
	}
}
