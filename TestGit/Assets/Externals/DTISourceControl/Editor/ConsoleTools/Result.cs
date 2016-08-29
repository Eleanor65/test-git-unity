using System;
using System.Collections.Generic;

namespace DTI.SourceControl.ConsoleTools
{
	internal class Result
	{
		public static readonly Result Ok = Success();

		public bool Succeeded { get; protected set; }

		public string ErrorMessage { get; protected set; }

		public static Result Success()
		{
			return new Result { Succeeded = true };
		}

		public static Result Failure(string message)
		{
			return new Result { ErrorMessage = message };
		}

	    public List<String> OutResult { get; set; }

	    protected Result() { }
	}
}
