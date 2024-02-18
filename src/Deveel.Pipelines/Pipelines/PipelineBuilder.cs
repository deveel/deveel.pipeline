namespace Deveel.Pipelines {
	/// <summary>
	/// The base class for a pipeline builder that can be used 
	/// to create a pipeline of steps to be executed in a specific
	/// order, against a given context.
	/// </summary>
	/// <typeparam name="TContext">
	/// The type of the context that is used to execute the pipeline.
	/// </typeparam>
	/// <seealso cref="Pipeline{TContext}"/>
	public class PipelineBuilder<TContext> : IPipelineBuilder<TContext> where TContext : PipelineExecutionContext {
		private readonly List<PipelineStep> steps = new List<PipelineStep>();

		/// <summary>
		/// Builds the execution tree of the pipeline.
		/// </summary>
		/// <param name="context">
		/// The context that is used to build the pipeline.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="PipelineExecutionNode{TContext}"/> that
		/// is the root of the execution tree of the pipeline.
		/// </returns>
		/// <exception cref="PipelineBuildException">
		/// Thrown when no steps were added to the pipeline.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Thrown when the given <paramref name="context"/> is <c>null</c>.
		/// </exception>
		protected virtual PipelineExecutionNode<TContext> BuildExecution(PipelineBuildContext context) {
			ArgumentNullException.ThrowIfNull(context, nameof(context));

			if (steps.Count == 0)
				throw new PipelineBuildException("No steps were added to the pipeline");

			PipelineExecutionNode<TContext>? next = null;

			for (var i = steps.Count - 1; i >= 0; i--) {
				var step = steps[i];
				next = step.CreateExecutionNode<TContext>(context, next);
			}

			return next!;
		}

		void IPipelineBuilder<TContext>.AddStep(PipelineStep step) {
			AddStep(step);
		}

		/// <summary>
		/// Adds a step to the pipeline that is to be built.
		/// </summary>
		/// <param name="handlerType">
		/// The type of the handler that is to be used to execute the step.
		/// </param>
		/// <param name="args">
		/// An array of arguments that are to be used to instantiate or
		/// invoke the handler.
		/// </param>
		/// <seealso cref="AddStep(PipelineStep)"/>
		/// <seealso cref="PipelineStep"/>
		protected void AddStep(Type handlerType, params object[] args) {
			AddStep(new PipelineStep(handlerType, args));
		}

		/// <summary>
		/// Adds a step to the pipeline that is to be built.
		/// </summary>
		/// <param name="step">
		/// The step to be added to the pipeline.
		/// </param>
		protected void AddStep(PipelineStep step) {
			ArgumentNullException.ThrowIfNull(step, nameof(step));
			steps.Add(step);
		}

		 Pipeline<TContext> IPipelineBuilder<TContext>.Build(PipelineBuildContext buidContext) 
			=> Build(buidContext);

		/// <summary>
		/// Builds the pipeline against the given context.
		/// </summary>
		/// <param name="buidContext">
		/// The context that is used to build the pipeline.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="Pipeline{TContext}"/> that is the
		/// result of the building of the pipeline.
		/// </returns>
		/// <seealso cref="BuildExecution(PipelineBuildContext)"/>
		protected virtual Pipeline<TContext> Build(PipelineBuildContext buidContext) 
			=> new Pipeline<TContext>(BuildExecution(buidContext));
	}
}
