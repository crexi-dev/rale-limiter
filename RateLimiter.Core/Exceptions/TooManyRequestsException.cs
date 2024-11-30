namespace RateLimiter.Core.Exceptions
{
	[Serializable]
	public class TooManyRequestsException : Exception
	{
		public TooManyRequestsException()
		{
		}

		public TooManyRequestsException(string message)
			: base(message)
		{
		}

		public TooManyRequestsException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
