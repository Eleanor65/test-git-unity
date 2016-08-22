using System;
using System.Collections.Generic;
using System.Linq;

namespace DTI.SourceControl.ConsoleTools
{
	internal class RegexParser : IParser
	{
		private readonly List<IPattern> _patterns = new List<IPattern>();

		public RegexParser(params string[] patterns)
		{
			foreach (var pattern in patterns)
			{
				_patterns.Add(new RegexPattern(pattern));
			}
		}

		public string[] Patterns
		{
			set
			{
				_patterns.Clear();
				foreach (var pattern in value)
				{
					_patterns.Add(new RegexPattern(pattern));
				}
			}
		}

		public Result Parse(string log)
		{
		    var errorList = String.Empty;
		    var outResult = new List<String>();
			var lines = log.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            //TODO: Убрать эти логи перед коммитом в работу!
		    UnityEngine.Debug.Log("LOG\n" + log);
			foreach (var line in lines)
			{
				Result result = null;
				foreach (var pattern in _patterns)
				{
					result = pattern.Match(line);
					if (result == null)
						continue;
				    if (!result.Succeeded)
				        errorList += result.ErrorMessage;
                    if(result.OutResult != null)
                        outResult.Add(result.OutResult.First());

				    break;
				}
				if (result == null)
					UnityEngine.Debug.LogWarning("Unparsed line: " + line);
			}
            if (!errorList.Equals(String.Empty))
		        return Result.Failure(errorList);
		    if (outResult.Count != 0)
		    {
		        var result = Result.Success();
                result.OutResult = outResult;
                return result;
		    }

			return Result.Ok;
		}
	}
}
