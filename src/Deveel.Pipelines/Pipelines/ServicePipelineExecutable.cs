// Copyright 2024 Antonello Provenzano
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Pipelines {
	/// <summary>
	/// Describes a single executable element in a pipeline that
	/// references a specific action to be executed.
	/// </summary>
	public sealed class ServicePipelineExecutable : IPipelineExecutable, IIdentifiedPipelineExecutable {
		private const string HandleMethodName = "Handle";
		private const string HandleAsyncMethodName = "HandleAsync";

		private static readonly Dictionary<Type, int> handlerCounters = new Dictionary<Type, int>();

		private readonly string stepId;

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
		public ServicePipelineExecutable(Type handlerType, params object[]? arguments) {
			HandlerType = handlerType ?? throw new ArgumentNullException(nameof(handlerType));
			Arguments = arguments;

			stepId = $"{handlerType.FullName}__{HandlerCounter(handlerType)}";
		}

		private static int HandlerCounter(Type handlerType) {
			if (!handlerCounters.TryGetValue(handlerType, out var counter)) {
				counter = 0;
			} else {
				counter++;
			}

			handlerCounters[handlerType] = counter;

			return counter;
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

		string IIdentifiedPipelineExecutable.Id => stepId;

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
		public PipelineExecutionNode<TContext> CreateNode<TContext>(PipelineBuildContext buildContext, PipelineExecutionNode<TContext>? next) 
			where TContext : PipelineExecutionContext {
			var handler = CreateHandler(HandlerType, buildContext);

			if (handler == null)
				throw new PipelineBuildException($"The handler type {HandlerType} could not be instantiated.");

			ExecutionDelegate<TContext> callback;

			if (handler is IExecutionHandler<TContext> asyncHandler) {
				callback = (ctx) => {
					return asyncHandler.HandleAsync(ctx, next?.Callback);
				};
			} else {
				callback = BuildCallback(HandlerType, handler, next, Arguments);
			}

			return new PipelineExecutionNode<TContext>(this, callback, next);
		}

		private ExecutionDelegate<TContext> BuildCallback<TContext>(Type handlerType, object handler, PipelineExecutionNode<TContext>? next, object?[]? args)
			where TContext : PipelineExecutionContext {
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
			if (context.Services != null)
				return ActivatorUtilities.CreateInstance(context.Services, handlerType);

			// TODO: add support for other constructor arguments
			return Activator.CreateInstance(handlerType);
		}

		private object?[] CreateHandlerArguments<TContext>(Type handlerType, TContext context, ExecutionDelegate<TContext>? next, ParameterInfo[] parameters, object?[]? arguments = null)
			where TContext : PipelineExecutionContext {
			var args = new List<object?>();

			if (arguments != null) {
				args.AddRange(arguments);
			}

			// TODO: make a better check for the parameters

			try {
				for (var i = 0; i < parameters.Length; i++) {
					var param = parameters[i];
					if (param.ParameterType == typeof(TContext)) {
						args.Insert(i, context);
					} else if (param.ParameterType == typeof(ExecutionDelegate<TContext>)) {
						args.Insert(i, WrapNext(param.ParameterType, context, next));
					} else if (param.ParameterType.IsSubclassOf(typeof(Delegate)) &&
						IsNextDelegate(param.ParameterType, typeof(TContext))) {
						args.Insert(i, WrapNext(param.ParameterType, context, next));
					}
				}
			} catch (ArgumentOutOfRangeException ex) {
				throw new PipelineException($"An argument of the method handler {HandlerType} is not in the range of allowed parameters", ex);
			} catch (PipelineException) {
				throw;
			} catch(Exception ex) {
				throw new PipelineException($"An error occurred while creating the arguments for the handler {HandlerType}", ex);
			}

			if (args.Count != parameters.Length)
				throw new PipelineException($"The handler type {handlerType} has a method to handle the execution of the pipeline that does not have the right number of parameters.");

			return args.ToArray();
		}

		private static bool IsNextDelegate(Type delegateType, Type contextType) {
			var method = delegateType.GetMethod("Invoke");
			var parameters = method?.GetParameters();
			return parameters?.Length != 0 &&
				parameters?.Length <= 1 &&
				parameters[0].ParameterType == contextType;
		}

		private Delegate WrapNext<TContext>(Type delegateType, TContext context, ExecutionDelegate<TContext>? next)
			where TContext : PipelineExecutionContext {
			var wrapper = new NextHandlerWrapper<TContext>(this, next);
			return Delegate.CreateDelegate(delegateType, wrapper, nameof(NextHandlerWrapper<TContext>.HandleAsync));
		}

		class NextHandlerWrapper<TContext> where TContext : PipelineExecutionContext {
			private readonly ServicePipelineExecutable step;
			private readonly ExecutionDelegate<TContext>? next;

			public NextHandlerWrapper(ServicePipelineExecutable step, ExecutionDelegate<TContext>? next) {
				this.step = step;
				this.next = next;
			}

			public Task HandleAsync(TContext context) {
				try {
					return next?.Invoke(context) ?? Task.CompletedTask;
				} finally {
					context.NextWasInvoked((step as IIdentifiedPipelineExecutable).Id);
				}
			}
		}
	}
}