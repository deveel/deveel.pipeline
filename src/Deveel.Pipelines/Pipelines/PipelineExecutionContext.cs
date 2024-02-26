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

using System.Collections.Concurrent;

namespace Deveel.Pipelines {
	/// <summary>
	/// Represents the context of the execution of a pipeline.
	/// </summary>
	public abstract class PipelineExecutionContext {
		private readonly ConcurrentDictionary<string, bool> nextExecuted = new ConcurrentDictionary<string, bool>();
		/// <summary>
		/// Gets a token that can be used to signal the cancellation
		/// of the execution of the pipeline.
		/// </summary>
		public virtual CancellationToken ExecutionCancelled { get; } = default;

		internal bool IsNextInvoked(string? nodeId) {
			return !String.IsNullOrWhiteSpace(nodeId) &&
				nextExecuted.TryGetValue(nodeId, out var value) && value;
		}

		internal void NextWasInvoked(string nodeId) {
			nextExecuted.AddOrUpdate(nodeId, true, (_, __) => true);
		}
	}
}
