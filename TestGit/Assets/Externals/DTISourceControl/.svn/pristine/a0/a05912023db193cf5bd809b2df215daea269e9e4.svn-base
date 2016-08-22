
namespace DTI.SourceControl.ConsoleTools
{
	internal class ConsoleTool
	{
		private readonly IProcessLauncher _launcher;
		private readonly IParser _parser;

		public ConsoleTool(IProcessLauncher launcher, IParser parser)
		{
			_launcher = launcher;
			_parser = parser;
		}

		public IProcessLauncher Launcher { get { return _launcher; } }
		public IParser Parser { get { return _parser; } }

		public Result Run()
		{
			int retCode = _launcher.Run();
			return _parser.Parse(_launcher.OutputWriter.ToString() + _launcher.ErrorWriter.ToString());
		}
	}
}
