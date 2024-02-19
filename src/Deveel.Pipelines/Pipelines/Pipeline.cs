namespace Deveel.Pipelines {
	/// <summary>
	/// An execution pipeline that handles a series of steps
	/// in a given context.
	/// </summary>
	/// <typeparam name="TContext">
	/// The type of the context that is used to execute the pipeline.
	/// </typeparam>
	public class Pipeline<TContext> where TContext : PipelineExecutionContext {
		/// <summary>
		/// Constructs the pipeline with the given execution root node.
		/// </summary>
		/// <param name="executionRoot">
		/// The root node of the execution tree of the pipeline.
		/// </param>
		protected internal Pipeline(PipelineExecutionNode<TContext>? executionRoot) {
			ExecutionRoot = executionRoot;
		}

		/// <summary>
		/// Gets the root node of the execution tree of the pipeline.
		/// </summary>
		protected PipelineExecutionNode<TContext>? ExecutionRoot { get; }

		/// <summary>
		/// Executes the pipeline against the given context.
		/// </summary>
		/// <param name="context">
		/// The context that is used to execute the pipeline.
		/// </param>
		/// <remarks>
		/// <para>
		/// By default, the pipeline is not catching any exception
		/// and it is up to the caller to handle any exception that
		/// any of the steps in the pipeline may throw.
		/// </para>
		/// <para>
		/// The execution of the pipeline is sequential and it is
		/// able to determine if any of the handlers in the pipeline
		/// has invoked the <c>next</c> callback, to continue the
		/// with the next step in the pipeline, avoiding a second
		/// invocation of the same handler.
		/// </para>
		/// </remarks>
		/// <returns>
		/// Returns the task that represents the asynchronous execution
		/// of the pipeline.
		/// </returns>
		/// <exception cref="TaskCanceledException">
		/// If the execution of the pipeline was cancelled through the
		/// cancellation token of the given context.
		/// </exception>
		public virtual async Task ExecuteAsync(TContext context) {
			var executor = ExecutionRoot;
			while (executor != null) {
				context.ExecutionCancelled.ThrowIfCancellationRequested();

				await executor.Callback(context);

				// TODO: find a way to skip all the next invocations
				//       that have been done in the callback
				executor = context.WasNextInvoked ? executor.Next?.Next : executor.Next;
				context.WasNextInvoked = false;
			}
		}
	}
}
