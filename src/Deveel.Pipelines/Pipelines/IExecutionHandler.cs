namespace Deveel.Pipelines {
	public interface IExecutionHandler<TContext> where TContext : IExecutionContext {
		Task HandleAsync(TContext context, ExecutionDelegate<TContext>? next);
	}
}
