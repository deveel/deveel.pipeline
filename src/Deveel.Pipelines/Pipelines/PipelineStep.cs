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

		public ExecutionCallback<TContext> CreateCallback<TContext>(IPipelineBuildContext buildContext, ExecutionCallback<TContext>? next) where TContext : IExecutionContext {
			var handler = CreateHandler<TContext>(buildContext);

			if (handler == null)
				throw new InvalidOperationException($"The handler type {HandlerType} could not be instantiated.");

			if (handler is IExecutionHandler<TContext> asyncHandler) {
				return new ExecutionCallback<TContext>((ctx, n) => {
					return asyncHandler.HandleAsync(ctx, n);
				}, next);
			}

			var handleMethods = HandlerType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
				.Where(x => x.Name == "Handle" || x.Name == "HandleAsync")
				.ToList();

			if (handleMethods.Count == 0)
				throw new InvalidOperationException($"The handler type {HandlerType} does not have a method to handle the execution of the pipeline.");

			if (handleMethods.Count > 1)
				throw new InvalidOperationException($"The handler type {HandlerType} has more than one method to handle the execution of the pipeline.");

			var handleMethod = handleMethods[0];

			if (handleMethod.ReturnType != typeof(void) && handleMethod.ReturnType != typeof(Task))
				throw new InvalidOperationException($"The handler type {HandlerType} has a method to handle the execution of the pipeline that does not return void or Task.");

			var parameters = handleMethod.GetParameters();

			if (parameters.Length == 0)
				throw new InvalidOperationException($"The handler type {HandlerType} has a method to handle the execution of the pipeline that does not have any parameter.");

			return new ExecutionCallback<TContext>((context, n) => {
				var args = new List<object?>();

				if (Arguments != null) {
					args.AddRange(Arguments);
				}

				for (var i = 0; i < parameters.Length; i++) {
					var param = parameters[i];
					if (param.ParameterType == typeof(TContext)) {
						args.Insert(i, context);
					} else if (param.ParameterType == typeof(ExecutionDelegate<TContext>)) {
						args.Insert(i, n);
					}

					// TODO: should we use the service provider?
					//} else {
					//	var service = buildContext.Services.GetService(param.ParameterType);
					//	if (service == null)
					//		throw new InvalidOperationException($"The handler type {HandlerType} requires a service of type {param.ParameterType} that is not available in the context.");

					//	args.Add(service);
					//}
				}

				if (args.Count != parameters.Length)
					throw new InvalidOperationException($"The handler type {HandlerType} has a method to handle the execution of the pipeline that does not have the right number of parameters.");

				var result = handleMethod.Invoke(handler, args.ToArray());
				if (result is Task task) {
					return task;
				}

				return Task.CompletedTask;
			}, next);
		}

		private object? CreateHandler<TContext>(IPipelineBuildContext context) where TContext : IExecutionContext {
			if (HandlerType is IExecutionHandler<TContext> handler) {
				return context.Services.GetService(HandlerType);
			}

			var ctors = HandlerType.GetConstructors();
			if (ctors.Length == 0)
				return Activator.CreateInstance(HandlerType);

			if (ctors.Length > 1)
				throw new InvalidOperationException($"The handler type {HandlerType} has more than one constructor and cannot be instantiated.");

			var ctor = ctors[0];

			var args = ctor.GetParameters();
			if (args.Length == 0)
				return Activator.CreateInstance(HandlerType);

			var ctorArgs = new object[args.Length];
			for (var i = 0; i < args.Length; i++) {
				var arg = args[i];
				var service = context.Services.GetService(arg.ParameterType);
				if (service == null)
					throw new InvalidOperationException($"The handler type {HandlerType} requires a service of type {arg.ParameterType} that is not available in the context.");

				ctorArgs[i] = service;
			}

			// TODO: should we use an ActivatorUtils instead?
			return Activator.CreateInstance(HandlerType, ctorArgs);
		}
	}
}