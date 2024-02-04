namespace Deveel.Pipelines {
	public sealed class TestBuildContext : IPipelineBuildContext {
		public IServiceProvider Services { get; }

		public TestBuildContext(IServiceProvider services) {
			Services = services;
		}
	}
}
