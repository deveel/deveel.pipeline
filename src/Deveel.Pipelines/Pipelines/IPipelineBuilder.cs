namespace Deveel.Pipelines {
	public interface IPipelineBuilder<TContext> where TContext : IExecutionContext {
		Pipeline<TContext> Build(IPipelineBuildContext buidContext);
	}
}
