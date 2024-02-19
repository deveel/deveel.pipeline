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
	/// An execution pipeline that handles a series of steps
	/// in a given context.
	/// </summary>
	/// <typeparam name="TContext">
	/// The type of the context that is used to execute the pipeline.
	/// </typeparam>
	public class Pipeline<TContext> where TContext : PipelineExecutionContext {
		/// <summary>
		/// Constructs the pipeline with the given execution root node.
		/// </summary>
		/// <param name="executionRoot">
		/// The root node of the execution tree of the pipeline.
		/// </param>
		protected internal Pipeline(PipelineExecutionNode<TContext>? executionRoot) {
			ExecutionRoot = executionRoot;
		}

		/// <summary>
		/// Gets the root node of the execution tree of the pipeline.
		/// </summary>
		protected PipelineExecutionNode<TContext>? ExecutionRoot { get; }

		/// <summary>
		/// Executes the pipeline against the given context.
		/// </summary>
		/// <param name="context">
		/// The context that is used to execute the pipeline.
		/// </param>
		/// <remarks>
		/// <para>
		/// By default, the pipeline is not catching any exception
		/// and it is up to the caller to handle any exception that
		/// any of the steps in the pipeline may throw.
		/// </para>
		/// <para>
		/// The execution of the pipeline is sequential and it is
		/// able to determine if any of the handlers in the pipeline
		/// has invoked the <c>next</c> callback, to continue the
		/// with the next step in the pipeline, avoiding a second
		/// invocation of the same handler.
		/// </para>
		/// </remarks>
		/// <returns>
		/// Returns the task that represents the asynchronous execution
		/// of the pipeline.
		/// </returns>
		/// <exception cref="TaskCanceledException">
		/// If the execution of the pipeline was cancelled through the
		/// cancellation token of the given context.
		/// </exception>
		public virtual async Task ExecuteAsync(TContext context) {
			var node = ExecutionRoot;
			while (node != null) {
				context.ExecutionCancelled.ThrowIfCancellationRequested();

				await node.Callback(context);

				node = NextNode(context, node);
			}
		}

		/// <summary>
		/// Executes the pipeline against a new instance of the context,
		/// implicitly created through the default constructor.
		/// </summary>
		/// <returns>
		/// Returns the task that represents the asynchronous execution
		/// of the pipeline.
		/// </returns>
		/// <exception cref="PipelineException">
		/// Thrown when the context type does not have a default constructor.
		/// </exception>
 		/// <exception cref="TaskCanceledException">
		/// If the execution of the pipeline was cancelled through the
		/// cancellation token of the given context.
		/// </exception>
		/// <seealso cref="ExecuteAsync(TContext)"/>
		public Task ExecuteAsync() {
			var defaultCtor = typeof(TContext).GetConstructor(Type.EmptyTypes);
			if (defaultCtor == null)
				throw new PipelineException($"The context type {typeof(TContext)} does not have a default constructor.");

			TContext context;

			try {
				context = (TContext)defaultCtor.Invoke(null);
			} catch (Exception ex) {

				throw new PipelineException($"Could not instantiate the context type {typeof(TContext)}", ex);
			}

			return ExecuteAsync(context);
		}

		private PipelineExecutionNode<TContext>? NextNode(TContext context, PipelineExecutionNode<TContext>? node) {
			while (node != null) {
				if (context.IsNextInvoked(node.Id)) {
					node = node.Next?.Next;
				} else {
					return node.Next;
				}
			}

			return null;
		}
	}
}
