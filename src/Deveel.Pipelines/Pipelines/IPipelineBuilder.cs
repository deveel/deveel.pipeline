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
	/// A contract for a builder that can be used to create a pipeline
	/// for a specific context.
	/// </summary>
	/// <typeparam name="TContext">
	/// The type of the context that is used to execute the pipeline.
	/// </typeparam>
	public interface IPipelineBuilder<TContext> where TContext : PipelineExecutionContext {
		/// <summary>
		/// Adds a step to the pipeline that is to be built.
		/// </summary>
		/// <param name="step">
		/// The step to be added to the pipeline.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// Thrown when the given <paramref name="step"/> is <c>null</c>.
		/// </exception>
		void AddStep(IPipelineStep step);

		/// <summary>
		/// Builds the pipeline for the given context.
		/// </summary>
		/// <param name="buidContext">
		/// A context that is used to build the pipeline.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="Pipeline{TContext}"/> that
		/// can be used to execute the pipeline.
		/// </returns>
		Pipeline<TContext> Build(PipelineBuildContext buidContext);
	}
}
