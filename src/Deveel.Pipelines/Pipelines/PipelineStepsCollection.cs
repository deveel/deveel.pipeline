using System.Collections;

namespace Deveel.Pipelines {
	public class PipelineStepsCollection : ICollection<PipelineStep> {
		private readonly IList<PipelineStep> steps;

		public PipelineStepsCollection() {
			steps = new List<PipelineStep>();
		}

		public PipelineStepsCollection(IList<PipelineStep> steps) {
			this.steps = steps;
		}

		public int Count => steps.Count;

		public bool IsReadOnly => steps.IsReadOnly;

		public void Add(PipelineStep item) => steps.Add(item);

		public void Clear() => steps.Clear();

		public bool Contains(PipelineStep item) => steps.Contains(item);

		public void CopyTo(PipelineStep[] array, int arrayIndex) => steps.CopyTo(array, arrayIndex);

		public IEnumerator<PipelineStep> GetEnumerator() => steps.GetEnumerator();

		public bool Remove(PipelineStep item) => steps.Remove(item);

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public ExecutionCallback<TContext> BuildExecution<TContext>(IPipelineBuildContext context)
			where TContext : IExecutionContext {

			ExecutionCallback<TContext>? next = null;

			for (var i = steps.Count - 1; i >= 0; i--) {
				var step = steps[i];
				next = step.CreateCallback(context, next);
			}

			return next;
		}
	}
}
