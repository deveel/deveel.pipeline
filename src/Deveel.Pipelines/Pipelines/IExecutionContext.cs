namespace Deveel.Pipelines {
	/// <summary>
	/// Represents the context of the execution of a pipeline.
	/// </summary>
	public interface IExecutionContext {
		/// <summary>
		/// Gets the provider of services that can be used 
		/// during the execution of the pipeline.
		/// </summary>
		IServiceProvider Services { get; }

		/// <summary>
		/// Gets a token that can be used to signal the cancellation
		/// of the execution of the pipeline.
		/// </summary>
		CancellationToken ExecutionCancelled { get; }
	}
}
