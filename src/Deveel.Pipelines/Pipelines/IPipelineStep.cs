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
	/// Represents a step in a pipeline and that
	/// is providing a node in the execution graph.
	/// </summary>
	public interface IPipelineStep {
		/// <summary>
		/// Creates a new node in the execution graph
		/// of the pipeline.
		/// </summary>
		/// <typeparam name="TContext">
		/// The type of the context that is used to execute the pipeline.
		/// </typeparam>
		/// <param name="buildContext">
		/// The context that is used to build the pipeline.
		/// </param>
		/// <param name="next">
		/// An optional reference to the next node in the execution graph.
		/// </param>
		/// <returns>
		/// Returns a new instance of <see cref="PipelineExecutionNode{TContext}"/>
		/// that represents the node in the execution graph of the pipeline.
		/// </returns>
		PipelineExecutionNode<TContext> CreateNode<TContext>(PipelineBuildContext buildContext, PipelineExecutionNode<TContext>? next)
			where TContext : PipelineExecutionContext;
	}
}
