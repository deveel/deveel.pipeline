namespace Deveel.Pipelines {
	public class Pipeline<TContext> where TContext : IExecutionContext {
		protected ExecutionCallback<TContext> ExecutionRoot { get; }

		public Pipeline(ExecutionCallback<TContext> executionRoot) {
			ExecutionRoot = executionRoot;
		}

		public async Task ExecuteAsync(TContext context) {
			var callback = ExecutionRoot;
			while (callback != null) {
				context.ExecutionCancelled.ThrowIfCancellationRequested();

				await callback.Callback(context, callback.Next?.Callback);

				callback = callback.Next;
			}

			await OnPipelineExecuted(context);
		}

		protected virtual Task OnPipelineExecuted(TContext context) {
			return Task.CompletedTask;
		}
	}
}
