using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Pipelines {
	/// <summary>
	/// A node in the execution tree of a pipeline that is used to
	/// execute a step in the pipeline.
	/// </summary>
	/// <typeparam name="TContext">
	/// The type of the context that is used to execute the pipeline.
	/// </typeparam>
	public sealed class PipelineExecutionNode<TContext> where TContext : PipelineExecutionContext {
		internal PipelineExecutionNode(ExecutionDelegate<TContext> callback, PipelineExecutionNode<TContext>? next) {
			Callback = callback ?? throw new ArgumentNullException(nameof(callback));
			Next = next;
		}

		/// <summary>
		/// Gets the delegate that is used to execute the step in the pipeline.
		/// </summary>
		public ExecutionDelegate<TContext> Callback { get; }

		/// <summary>
		/// Gets the reference to the next step in the pipeline.
		/// </summary>
		public PipelineExecutionNode<TContext>? Next { get; }
	}
}
