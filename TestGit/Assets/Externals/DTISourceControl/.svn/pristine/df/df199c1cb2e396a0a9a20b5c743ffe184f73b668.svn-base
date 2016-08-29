using System.IO;

namespace DTI.SourceControl.ConsoleTools
{
	internal interface IProcessLauncher
	{
		string CommandName { get; set; }

		string Arguments { get; set; }

		string BaseDirectory { get; set; }

		int Timeout { get; set; }

		StringWriter ErrorWriter { get; }

		StringWriter OutputWriter { get; }

		int Run();
	}
}
