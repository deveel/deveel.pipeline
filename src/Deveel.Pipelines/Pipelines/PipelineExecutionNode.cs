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
		private PipelineExecutionNode(Type handlerType, ExecutionDelegate<TContext> callback, PipelineExecutionNode<TContext>? next) {
			HandlerType = handlerType ?? throw new ArgumentNullException(nameof(handlerType));
			Callback = callback ?? throw new ArgumentNullException(nameof(callback));
			Next = next;
		}

		/// <summary>
		/// Gets the type of the handler that defines the method
		/// to be executed in the pipeline.
		/// </summary>
		public Type HandlerType { get; }

		/// <summary>
		/// Gets the delegate that is used to execute the step in the pipeline.
		/// </summary>
		public ExecutionDelegate<TContext> Callback { get; }

		/// <summary>
		/// Gets the reference to the next step in the pipeline.
		/// </summary>
		public PipelineExecutionNode<TContext>? Next { get; }

		private const string HandleMethodName = "Handle";
		private const string HandleAsyncMethodName = "HandleAsync";

		/// <summary>
		/// Creates a new instance of the execution node of the pipeline
		/// for the given handler type and the context of the pipeline build.
		/// </summary>
		/// <param name="handlerType">
		/// The type of the handler that is used to execute the step in the pipeline.
		/// </param>
		/// <param name="buildContext">
		/// A context that is used to build the pipeline.
		/// </param>
		/// <param name="args">
		/// A set of arguments that are used to instantiate or invoke the handler.
		/// </param>
		/// <param name="next">
		/// The reference to the next step in the pipeline.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="PipelineExecutionNode{TContext}"/> that
		/// is used to execute the step in the pipeline.
		/// </returns>
		/// <exception cref="PipelineBuildException">
		/// Thrown when the handler type could not be instantiated.
		/// </exception>
		public static PipelineExecutionNode<TContext> Create(Type handlerType, PipelineBuildContext buildContext, object?[]? args = null, PipelineExecutionNode<TContext>? next = null) {
			var handler = CreateHandler(handlerType, buildContext);

			if (handler == null)
				throw new PipelineBuildException($"The handler type {handlerType} could not be instantiated.");

			ExecutionDelegate<TContext> callback;

			if (handler is IExecutionHandler<TContext> asyncHandler) {
				callback = (ctx) => {
					return asyncHandler.HandleAsync(ctx, next?.Callback);
				};
			} else {
				callback = BuildCallback(handlerType, handler, next, args);
			}

			return new PipelineExecutionNode<TContext>(handlerType, callback, next);
		}

		private static ExecutionDelegate<TContext> BuildCallback(Type handlerType, object handler, PipelineExecutionNode<TContext>? next, object?[]? args) {
			var handleMethods = handlerType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
				.Where(x => x.Name == HandleMethodName || 
							x.Name == HandleAsyncMethodName)
				.ToList();

			if (handleMethods.Count == 0)
				throw new PipelineBuildException($"The handler type {handlerType} does not have a method to handle the execution of the pipeline.");

			if (handleMethods.Count > 1)
				throw new PipelineBuildException($"The handler type {handlerType} has more than one method to handle the execution of the pipeline.");

			var handleMethod = handleMethods[0];

			if (handleMethod.ReturnType != typeof(void) && handleMethod.ReturnType != typeof(Task))
				throw new PipelineBuildException($"The handler type {handlerType} has a method to handle the execution of the pipeline that does not return void or Task.");

			var parameters = handleMethod.GetParameters();

			if (parameters.Length == 0)
				throw new PipelineBuildException($"The handler type {handlerType} has a method to handle the execution of the pipeline that does not have any parameter.");

			return (context) => {
				var callArgs = CreateHandlerArguments(handlerType, context, next?.Callback, parameters, args);
				var result = handleMethod.Invoke(handler, callArgs);
				if (result is Task task) {
					return task;
				}

				return Task.CompletedTask;
			};
		}

		private static object? CreateHandler(Type handlerType, PipelineBuildContext context) {
			return ActivatorUtilities.CreateInstance(context.Services, handlerType);
		}

		private static object?[] CreateHandlerArguments(Type handlerType, TContext context, ExecutionDelegate<TContext>? next, ParameterInfo[] parameters, object?[]? arguments = null) {
			var args = new List<object?>();

			if (arguments != null) {
				args.AddRange(arguments);
			}

			for (var i = 0; i < parameters.Length; i++) {
				var param = parameters[i];
				if (param.ParameterType == typeof(TContext)) {
					args.Insert(i, context);
				} else if (param.ParameterType == typeof(ExecutionDelegate<TContext>)) {
					args.Insert(i, WrapNext(param.ParameterType, context, next));
				} else if (param.ParameterType.IsSubclassOf(typeof(Delegate)) && 
					IsNextDelegate(param.ParameterType)) {
					args.Insert(i, WrapNext(param.ParameterType, context, next));
				}
			}

			if (args.Count != parameters.Length)
				throw new PipelineException($"The handler type {handlerType} has a method to handle the execution of the pipeline that does not have the right number of parameters.");

			return args.ToArray();
		}

		private static bool IsNextDelegate(Type delegateType) {
			var method = delegateType.GetMethod("Invoke");
			var parameters = method?.GetParameters();
			return parameters?.Length != 0 &&
				parameters?.Length <= 1 &&
				parameters[0].ParameterType == typeof(TContext);
		}

		private static Delegate WrapNext(Type delegateType, TContext context, ExecutionDelegate<TContext>? next) {
			var wrapper = new NextHandlerWrapper(context, next);
			return Delegate.CreateDelegate(delegateType, wrapper, nameof(NextHandlerWrapper.HandleAsync));
		}

		class NextHandlerWrapper {
			private readonly TContext executor;
			private readonly ExecutionDelegate<TContext>? next;

			public NextHandlerWrapper(TContext executor, ExecutionDelegate<TContext>? next) {
				this.next = next;
				this.executor = executor;
			}

			public Task HandleAsync(TContext context) {
				try {
					return next?.Invoke(context) ?? Task.CompletedTask;
				} finally {
					executor.WasNextInvoked = true;
				}
			}
		}
	}
}
