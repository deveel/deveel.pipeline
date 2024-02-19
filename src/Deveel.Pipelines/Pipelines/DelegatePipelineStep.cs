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

namespace Deveel.Pipelines {
	/// <summary>
	/// A pipeline step that is executed by a delegate.
	/// </summary>
	public sealed class DelegatePipelineStep : IPipelineStep {
		private DelegatePipelineStep(Delegate handler) {
			Handler = handler ?? throw new ArgumentNullException(nameof(handler));
		}

		/// <summary>
		/// Gets the delegate that is used to execute the step.
		/// </summary>
		public Delegate Handler { get; }

		/// <inheritdoc/>
		public PipelineExecutionNode<TContext> CreateNode<TContext>(PipelineBuildContext buildContext, PipelineExecutionNode<TContext>? next)
			where TContext : PipelineExecutionContext {
			// TODO: wrap the next callback to intercept the execution

			if (Handler is ExecutionDelegate<TContext> callback) {
				return new PipelineExecutionNode<TContext>(this, callback, next);
			} else {
				var parameters = Handler.Method.GetParameters();
				var withNext = parameters.Length == 2 && parameters[1].ParameterType == typeof(ExecutionDelegate<TContext>);
				var handler = new DelegateHandler<TContext>(Handler, next?.Callback, withNext);
				var wrapper = (ExecutionDelegate<TContext>) Delegate.CreateDelegate(typeof(ExecutionDelegate<TContext>), handler, nameof(DelegateHandler<TContext>.HandleAsync));
				return new PipelineExecutionNode<TContext>(this, wrapper, next);
			}
		}

		/// <summary>
		/// Creates a new instance of <see cref="DelegatePipelineStep"/> that
		/// is executed by the given delegate.
		/// </summary>
		/// <typeparam name="TContext">
		/// The type of the context that is used to execute the pipeline.
		/// </typeparam>
		/// <param name="func">
		/// The delegate that is used to execute the step.
		/// </param>
		/// <returns>
		/// Returns a new instance of <see cref="DelegatePipelineStep"/> that
		/// is executed by the given delegate.
		/// </returns>
		public static DelegatePipelineStep Create<TContext>(ExecutionDelegate<TContext> func) where TContext : PipelineExecutionContext
			=> new DelegatePipelineStep(func);

		/// <summary>
		/// Creates a new instance of <see cref="DelegatePipelineStep"/> that
		/// is executed by the given delegate.
		/// </summary>
		/// <typeparam name="TContext"></typeparam>
		/// <param name="func"></param>
		/// <returns></returns>
		public static DelegatePipelineStep Create<TContext>(Func<TContext, ExecutionDelegate<TContext>, Task> func) where TContext : PipelineExecutionContext
			=> new DelegatePipelineStep(func);

		// TODO: allow for more delegate types to be created?

		class DelegateHandler<TContext> {
			private readonly Delegate handler;
			private readonly ExecutionDelegate<TContext>? next;
			private readonly bool withNext;

			public DelegateHandler(Delegate handler, ExecutionDelegate<TContext>? next, bool withNext) {
				this.handler = handler;
				this.next = next;
				this.withNext = withNext;
			}

			public Task HandleAsync(TContext context) {
				// TODO: determine if the delegate requires the context or not
				var args = new List<object?> { context };
				if (withNext) {
					args.Add(next);
				}

				var result = handler.DynamicInvoke(args.ToArray());
				if (result is Task task) {
					return task;
				}

				return Task.CompletedTask;
			}
		}
	}
}
