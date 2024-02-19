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
	/// Provides a context for building a pipeline.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This default implementation of the context provides a 
	/// service provider to resolve services during the building
	/// process of the pipeline.
	/// </para>
	/// <para>
	/// When the building process of the pipeline is complex,
	/// the context can be extended to provide additional information
	/// to the pipeline builder.
	/// </para>
	/// </remarks>
	/// <seealso cref="IPipelineBuilder{TContext}.Build(PipelineBuildContext)"/>
	public class PipelineBuildContext {
		/// <summary>
		/// Constructs the context with the given service provider.
		/// </summary>
		/// <param name="services">
		/// The services that are used to resolve dependencies
		/// when building the pipeline.
		/// </param>
		public PipelineBuildContext(IServiceProvider? services) {
			Services = services;
		}

		/// <summary>
		/// Constructs the context with no service provider.
		/// </summary>
		public PipelineBuildContext() : this(null) {
		}

		/// <summary>
		/// Gets the service provider that is used to resolve
		/// the dependencies when building the pipeline.
		/// </summary>
		public IServiceProvider? Services { get; }
	}
}
