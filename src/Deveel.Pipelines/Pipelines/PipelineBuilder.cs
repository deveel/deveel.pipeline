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
	/// The base class for a pipeline builder that can be used 
	/// to create a pipeline of steps to be executed in a specific
	/// order, against a given context.
	/// </summary>
	/// <typeparam name="TContext">
	/// The type of the context that is used to execute the pipeline.
	/// </typeparam>
	/// <seealso cref="Pipeline{TContext}"/>
	public class PipelineBuilder<TContext> where TContext : PipelineExecutionContext {
		private readonly List<IPipelineStep> steps = new List<IPipelineStep>();
		private IServiceProvider? builderServices;

		/// <summary>
		/// Builds the execution tree of the pipeline.
		/// </summary>
		/// <param name="context">
		/// The context that is used to build the pipeline.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="PipelineExecutionNode{TContext}"/> that
		/// is the root of the execution tree of the pipeline.
		/// </returns>
		/// <exception cref="PipelineBuildException">
		/// Thrown when no steps were added to the pipeline.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Thrown when the given <paramref name="context"/> is <c>null</c>.
		/// </exception>
		protected virtual PipelineExecutionNode<TContext>? BuildExecution(PipelineBuildContext context) {
			ArgumentNullException.ThrowIfNull(context, nameof(context));

			if (steps.Count == 0)
				return null;

			PipelineExecutionNode<TContext>? next = null;

			for (var i = steps.Count - 1; i >= 0; i--) {
				var step = steps[i];
				next = step.CreateNode<TContext>(context, next);
			}

			return next;
		}

		/// <summary>
		/// Adds a step to the pipeline that is to be built.
		/// </summary>
		/// <param name="handlerType">
		/// The type of the handler that is to be used to execute the step.
		/// </param>
		/// <param name="args">
		/// An array of arguments that are to be used to instantiate or
		/// invoke the handler.
		/// </param>
		/// <seealso cref="AddStep(IPipelineStep)"/>
		/// <seealso cref="ServicePipelineStep"/>
		protected void AddStep(Type handlerType, params object[] args) {
			AddStep(new ServicePipelineStep(handlerType, args));
		}

		/// <summary>
		/// Adds a step to the pipeline that delegates to a function
		/// the execution of the step.
		/// </summary>
		/// <param name="func">
		/// The delegate that is used to execute the step in the pipeline.
		/// </param>
		/// <seealso cref="AddStep(IPipelineStep)"/>
		/// <seealso cref="DelegatePipelineStep.Create{TContext}(ExecutionDelegate{TContext})"/>
		protected void AddStep(ExecutionDelegate<TContext> func)
			=> AddStep(DelegatePipelineStep.Create(func));

		/// <summary>
		/// Adds a step to the pipeline that delegates to a function
		/// the execution of the step.
		/// </summary>
		/// <param name="func"></param>
		/// <seealso cref="AddStep(IPipelineStep)"/>
		/// <seealso cref="DelegatePipelineStep.Create{TContext}(Func{TContext, ExecutionDelegate{TContext}, Task})"/>
		protected void AddStep(Func<TContext, ExecutionDelegate<TContext>, Task> func)
			=> AddStep(DelegatePipelineStep.Create(func));

		/// <summary>
		/// Adds a step to the pipeline that is to be built.
		/// </summary>
		/// <param name="step">
		/// The step to be added to the pipeline.
		/// </param>
		protected void AddStep(IPipelineStep step) {
			ArgumentNullException.ThrowIfNull(step, nameof(step));
			steps.Add(step);
		}

		/// <summary>
		/// Sets the provider instance that is used to resolve the
		/// services that are required to build the pipeline.
		/// </summary>
		/// <param name="services">
		/// The instance of <see cref="IServiceProvider"/> that is used
		/// to resolve the services required to build the pipeline.
		/// </param>
		protected void SetServiceProvider(IServiceProvider services) {
			builderServices = services;
		}

		/// <summary>
		/// Builds the pipeline against the given context.
		/// </summary>
		/// <param name="buidContext">
		/// The context that is used to build the pipeline.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="Pipeline{TContext}"/> that is the
		/// result of the building of the pipeline.
		/// </returns>
		/// <seealso cref="BuildExecution(PipelineBuildContext)"/>
		protected virtual Pipeline<TContext> BuildPipeline(PipelineBuildContext buidContext) 
			=> new Pipeline<TContext>(BuildExecution(buidContext));
	}
}
