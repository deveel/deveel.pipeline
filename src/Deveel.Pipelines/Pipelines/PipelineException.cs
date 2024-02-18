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
