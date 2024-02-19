namespace Deveel.Pipelines {
	/// <summary>
	/// Represents a step in a pipeline and that
	/// is providing a node in the execution graph.
	/// </summary>
	public interface IPipelineStep {
		/// <summary>
		/// Creates a new node in the execution graph
		/// of the pipeline.
		/// </summary>
		/// <typeparam name="TContext">
		/// The type of the context that is used to execute the pipeline.
		/// </typeparam>
		/// <param name="buildContext">
		/// The context that is used to build the pipeline.
		/// </param>
		/// <param name="next">
		/// An optional reference to the next node in the execution graph.
		/// </param>
		/// <returns>
		/// Returns a new instance of <see cref="PipelineExecutionNode{TContext}"/>
		/// that represents the node in the execution graph of the pipeline.
		/// </returns>
		PipelineExecutionNode<TContext> CreateNode<TContext>(PipelineBuildContext buildContext, PipelineExecutionNode<TContext>? next)
			where TContext : PipelineExecutionContext;
	}
}
