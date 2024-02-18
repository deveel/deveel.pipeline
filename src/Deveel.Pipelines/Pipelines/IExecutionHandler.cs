namespace Deveel.Pipelines {
	/// <summary>
	/// A contract for a handler that is used to execute a step in a pipeline.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Implementations of this interface are used to execute a step in a pipeline
	/// through a well-defined contract.
	/// </para>
	/// <para>
	/// The pipeline framework is able to build handlers without the need of
	/// them being implementing this contract, but when this is done, the
	/// building and execution of the pipeline steps is more efficient.
	/// </para>
	/// </remarks>
	/// <typeparam name="TContext">
	/// The type of the context that is used to execute the pipeline.
	/// </typeparam>
	public interface IExecutionHandler<TContext> where TContext : PipelineExecutionContext {
		/// <summary>
		/// Handles the execution of the pipeline step against the 
		/// given context.
		/// </summary>
		/// <param name="context">
		/// The context that is used to execute the pipeline.
		/// </param>
		/// <param name="next">
		/// An optional reference to the next step in the pipeline.
		/// </param>
		/// <returns>
		/// Returns a task that represents the asynchronous execution
		/// of the pipeline step.
		/// </returns>
		Task HandleAsync(TContext context, ExecutionDelegate<TContext>? next);
	}
}
