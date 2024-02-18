namespace Deveel.Pipelines {
	/// <summary>
	/// A delegate that is used to execute a pipeline step
	/// in a given context.
	/// </summary>
	/// <typeparam name="TContext">
	/// The type of the context that is used to execute 
	/// the pipeline.
	/// </typeparam>
	/// <param name="context">
	/// The instance of the context that is used to execute
	/// the pipeline.
	/// </param>
	/// <returns>
	/// Returns a task that represents the asynchronous
	/// execution of the pipeline step.
	/// </returns>
	public delegate Task ExecutionDelegate<TContext>(TContext context);
}
