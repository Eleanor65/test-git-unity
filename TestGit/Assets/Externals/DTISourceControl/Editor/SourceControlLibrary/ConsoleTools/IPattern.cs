namespace DTI.SourceControl.ConsoleTools
{
	internal interface IPattern
	{
		Result Match(string line);
	}
}
