namespace Deveel.Pipelines {
	/// <summary>
	/// A contract for a builder that can be used to create a pipeline
	/// for a specific context.
	/// </summary>
	/// <typeparam name="TContext">
	/// The type of the context that is used to execute the pipeline.
	/// </typeparam>
	public interface IPipelineBuilder<TContext> where TContext : PipelineExecutionContext {
		/// <summary>
		/// Adds a step to the pipeline that is to be built.
		/// </summary>
		/// <param name="step">
		/// The step to be added to the pipeline.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// Thrown when the given <paramref name="step"/> is <c>null</c>.
		/// </exception>
		void AddStep(PipelineStep step);

		/// <summary>
		/// Builds the pipeline for the given context.
		/// </summary>
		/// <param name="buidContext">
		/// A context that is used to build the pipeline.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="Pipeline{TContext}"/> that
		/// can be used to execute the pipeline.
		/// </returns>
		Pipeline<TContext> Build(PipelineBuildContext buidContext);
	}
}
