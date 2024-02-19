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
	/// An exception that is thrown when an error occurs during the
	/// building of a pipeline.
	/// </summary>
	public class PipelineBuildException : PipelineException {
		/// <summary>
		/// Constructs the exception without a message.
		/// </summary>
		public PipelineBuildException() {
		}

		/// <summary>
		/// Constructs the exception with the specified message.
		/// </summary>
		/// <param name="message">
		/// The message that describes the error.
		/// </param>
		public PipelineBuildException(string? message) : base(message) {
		}

		/// <summary>
		/// Constructs the exception with the specified message and
		/// the inner exception that caused the error.
		/// </summary>
		/// <param name="message">
		/// The message that describes the error.
		/// </param>
		/// <param name="innerException">
		/// An exception that caused the error.
		/// </param>
		public PipelineBuildException(string? message, Exception? innerException) : base(message, innerException) {
		}
	}
}
