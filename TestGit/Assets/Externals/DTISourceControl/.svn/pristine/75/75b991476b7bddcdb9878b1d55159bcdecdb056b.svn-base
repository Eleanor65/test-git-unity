using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DTI.SourceControl.ConsoleTools
{
	internal class RegexPattern : IPattern
	{
		private const string ErrorGroup = "error";
		private const string SkipGroup = "skip";
	    private const string OutGroup = "out";
		private readonly Regex _regex;

		public RegexPattern(string regex)
		{
			_regex = new Regex(regex);
		}

		public Result Match(string line)
		{
			var match = _regex.Match(line);
			if (!match.Success)
				return null;

			if (match.Groups[ErrorGroup].Success)
				return Result.Failure(match.Groups[ErrorGroup].Value);
			if (match.Groups[SkipGroup].Success)
				return Result.Success();
		    if (match.Groups[OutGroup].Success)
		    {
		        var result = Result.Success();
                result.OutResult = new List<string> {line};
		        return result;
		    }

			return Result.Ok;
		}
	}
}
