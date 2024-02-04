namespace Deveel.Pipelines {
	public sealed class ExecutionCallback<TContext> where TContext : IExecutionContext {
		public ExecutionCallback(ExecutionDelegate<TContext> callback, ExecutionCallback<TContext>? next) {
			Callback = callback ?? throw new ArgumentNullException(nameof(callback));
			Next = next;
		}

		public ExecutionDelegate<TContext> Callback { get; }

		public ExecutionCallback<TContext>? Next { get; }
	}
}
