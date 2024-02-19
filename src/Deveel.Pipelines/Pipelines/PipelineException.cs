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
	/// An exception that is thrown when an error occurs
	/// in the pipeline context.
	/// </summary>
	public class PipelineException : Exception {
		/// <summary>
		/// Constructs the exception without a message.
		/// </summary>
		public PipelineException() {
		}

		/// <summary>
		/// Constructs the exception with the given message.
		/// </summary>
		/// <param name="message">
		/// The message that describes the error that occurred.
		/// </param>
		public PipelineException(string? message) : base(message) {
		}

		/// <summary>
		/// Constructs the exception with the given message and
		/// the inner exception that caused the error.
		/// </summary>
		/// <param name="message">
		/// The message that describes the error that occurred.
		/// </param>
		/// <param name="innerException">
		/// The exception that caused the error in the pipeline.
		/// </param>
		public PipelineException(string? message, Exception? innerException) : base(message, innerException) {
		}
	}
}
