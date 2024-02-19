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
	/// A contract for a handler that is used to execute a step in a pipeline.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Implementations of this interface are used to execute a step in a pipeline
	/// through a well-defined contract.
	/// </para>
	/// <para>
	/// The pipeline framework is able to build handlers without the need of
	/// them being implementing this contract, but when this is done, the
	/// building and execution of the pipeline steps is more efficient.
	/// </para>
	/// </remarks>
	/// <typeparam name="TContext">
	/// The type of the context that is used to execute the pipeline.
	/// </typeparam>
	public interface IExecutionHandler<TContext> where TContext : PipelineExecutionContext {
		/// <summary>
		/// Handles the execution of the pipeline step against the 
		/// given context.
		/// </summary>
		/// <param name="context">
		/// The context that is used to execute the pipeline.
		/// </param>
		/// <param name="next">
		/// An optional reference to the next step in the pipeline.
		/// </param>
		/// <returns>
		/// Returns a task that represents the asynchronous execution
		/// of the pipeline step.
		/// </returns>
		Task HandleAsync(TContext context, ExecutionDelegate<TContext>? next);
	}
}
