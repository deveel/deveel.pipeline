namespace Deveel.Pipelines {
	public delegate Task ExecutionDelegate<TContext>(TContext context, ExecutionDelegate<TContext>? next);
}
