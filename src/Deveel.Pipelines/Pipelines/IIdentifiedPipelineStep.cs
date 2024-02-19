namespace Deveel.Pipelines {
	/// <summary>
	/// A step in a pipeline that is identified by a unique identifier.
	/// </summary>
	interface IIdentifiedPipelineStep : IPipelineStep {
		string Id { get; }
	}
}
