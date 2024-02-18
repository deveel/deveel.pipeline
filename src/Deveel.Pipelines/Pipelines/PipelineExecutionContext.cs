using System.Collections.Concurrent;

namespace Deveel.Pipelines {
	/// <summary>
	/// Represents the context of the execution of a pipeline.
	/// </summary>
	public abstract class PipelineExecutionContext {
		/// <summary>
		/// Constructs the context of the execution of a pipeline.
		/// </summary>
		/// <param name="services">
		/// The provider of services that can be used during the execution 
		/// of the pipeline.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// Thrown when the <paramref name="services"/> is <see langword="null"/>.
		/// </exception>
		protected PipelineExecutionContext(IServiceProvider services) {
			Services = services ?? throw new ArgumentNullException(nameof(services));

			Properties = new ConcurrentDictionary<string, object?>();
		}

		/// <summary>
		/// Gets the provider of services that can be used 
		/// during the execution of the pipeline.
		/// </summary>
		public virtual IServiceProvider Services { get; }

		/// <summary>
		/// Gets a token that can be used to signal the cancellation
		/// of the execution of the pipeline.
		/// </summary>
		public virtual CancellationToken ExecutionCancelled { get; } = default;

		internal bool WasNextInvoked { get; set; }

		/// <summary>
		/// Gets a dictionary of properties that can be used to store
		/// contextually relevant information during the execution of 
		/// the pipeline.
		/// </summary>
		public virtual IDictionary<string, object?> Properties { get; }
	}
}
