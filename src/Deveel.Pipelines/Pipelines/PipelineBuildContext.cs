namespace Deveel.Pipelines {
	/// <summary>
	/// Provides a context for building a pipeline.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This default implementation of the context provides a 
	/// service provider to resolve services during the building
	/// process of the pipeline.
	/// </para>
	/// <para>
	/// When the building process of the pipeline is complex,
	/// the context can be extended to provide additional information
	/// to the pipeline builder.
	/// </para>
	/// </remarks>
	/// <seealso cref="IPipelineBuilder{TContext}.Build(PipelineBuildContext)"/>
	public class PipelineBuildContext {
		/// <summary>
		/// Constructs the context with the given service provider.
		/// </summary>
		/// <param name="services">
		/// The services that are used to resolve dependencies
		/// when building the pipeline.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// Thrown when the <paramref name="services"/> is <c>null</c>.
		/// </exception>
		public PipelineBuildContext(IServiceProvider services) {
			Services = services ?? throw new ArgumentNullException(nameof(services));
		}

		/// <summary>
		/// Gets the service provider that is used to resolve
		/// the dependencies when building the pipeline.
		/// </summary>
		public IServiceProvider Services { get; }
	}
}
