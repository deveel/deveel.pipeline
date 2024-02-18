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
