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
	/// A node in the execution tree of a pipeline that is used to
	/// execute a step in the pipeline.
	/// </summary>
	/// <typeparam name="TContext">
	/// The type of the context that is used to execute the pipeline.
	/// </typeparam>
	public sealed class PipelineExecutionNode<TContext> where TContext : PipelineExecutionContext {
		internal PipelineExecutionNode(ExecutionDelegate<TContext> callback, PipelineExecutionNode<TContext>? next) {
			Callback = callback ?? throw new ArgumentNullException(nameof(callback));
			Next = next;
		}

		/// <summary>
		/// Gets the delegate that is used to execute the step in the pipeline.
		/// </summary>
		public ExecutionDelegate<TContext> Callback { get; }

		/// <summary>
		/// Gets the reference to the next step in the pipeline.
		/// </summary>
		public PipelineExecutionNode<TContext>? Next { get; }
	}
}
