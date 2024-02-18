namespace Deveel.Pipelines {
	public sealed class TestBuildContext : PipelineBuildContext {
		public TestBuildContext(IServiceProvider services)
			: base(services) {
		}
	}
}
