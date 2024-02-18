using System.Reflection;

namespace Deveel.Pipelines {
	/// <summary>
	/// Describes a single step in a pipeline that
	/// references a specific action to be executed.
	/// </summary>
	public class PipelineStep {
		/// <summary>
		/// Constructs the pipeline step with the specified
		/// type of the handler and the optional arguments.
		/// </summary>
		/// <param name="handlerType">
		/// The type of the object that is responsible
		/// of handling the execution of the step.
		/// </param>
		/// <param name="arguments">
		/// A list of arguments that are passed to the handler
		/// when the step is executed.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// Thrown when the <paramref name="handlerType"/> is <see langword="null"/>.
		/// </exception>
		public PipelineStep(Type handlerType, params object[]? arguments) {
			HandlerType = handlerType ?? throw new ArgumentNullException(nameof(handlerType));
			Arguments = arguments;
		}

		/// <summary>
		/// Gets the type of the object that is responsible
		/// of handling the execution of the step.
		/// </summary>
		public Type HandlerType { get; }

		/// <summary>
		/// Gets the arguments that are passed to the handler
		/// for the execution of the step.
		/// </summary>
		/// <remarks>
		/// The arguments are optional and can be <see langword="null"/>:
		/// when specified, the handler must be able to handle them 
		/// in the signature of the method that is invoked.
		/// </remarks>
		public object[]? Arguments { get; }

		/// <summary>
		/// Creates a node of the execution tree of the pipeline
		/// that is used to execute the step in the pipeline.
		/// </summary>
		/// <typeparam name="TContext">
		/// The type of the context of the pipeline execution.
		/// </typeparam>
		/// <param name="buildContext">
		/// The context of the pipeline build that is used to create
		/// the executor for the step.
		/// </param>
		/// <param name="next">
		/// An optional reference to the next step in the pipeline.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="PipelineExecutionNode{TContext}"/>
		/// that can be used to execute the step in the pipeline.
		/// </returns>
		public PipelineExecutionNode<TContext> CreateExecutionNode<TContext>(PipelineBuildContext buildContext, PipelineExecutionNode<TContext>? next) where TContext : PipelineExecutionContext {
			return PipelineExecutionNode<TContext>.Create(HandlerType, buildContext, Arguments, next);
		}
	}
}