using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace DTI.SourceControl.ConsoleTools
{
	public class ProcessLauncher : IProcessLauncher
	{
		private const int DefaultTimeout = 60000;

		private readonly object _lockObject = new object();

		private StreamReader _stdError;
		private StreamReader _stdOut;

		public ProcessLauncher()
		{
			Timeout = DefaultTimeout;
			OutputWriter = new StringWriter();
			ErrorWriter = new StringWriter();
		}

		public string CommandName { get; set; }

		public string Arguments { get; set; }

		public string BaseDirectory { get; set; }

		public int Timeout { get; set; }

		public StringWriter ErrorWriter { get; private set; }

		public StringWriter OutputWriter { get; private set; }

		/// <summary>
		/// TODO CommandName не может быть null or empty
		/// </summary>
		/// <returns></returns>
		public int Run()
		{
			UnityEngine.Debug.Log("Running " + CommandName + " " + Arguments);
			{
				var threadStdOut = new Thread(ProcessStdStream);
				var threadStdErr = new Thread(ProcessErrorStream);

				try
				{
					var process = StartProcess();
					_stdOut = process.StandardOutput;
					_stdError = process.StandardError;
					threadStdOut.Start();
					threadStdErr.Start();
					process.WaitForExit(Timeout);
					threadStdOut.Join(2000);
					threadStdErr.Join(2000);
					EnsureProcessExited(process);
					return process.ExitCode;
				}
				finally
				{
					if (threadStdOut.IsAlive)
					{
						ThreadUtility.KillThread(threadStdOut);
					}

					if (threadStdErr.IsAlive)
					{
						ThreadUtility.KillThread(threadStdErr);
					}
				}
			}
		}

		private void EnsureProcessExited(Process process)
		{
			if (process.HasExited)
				return;

			try
			{
				process.Kill();
			}
			catch
			{
			}
			throw new TimeoutException("Process " + CommandName + " is exceeded time limit " +
										Timeout + " and was killed");
		}

		private void PrepareProcess(Process process)
		{
			process.StartInfo.FileName = CommandName;
			process.StartInfo.Arguments = Arguments;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.WorkingDirectory = BaseDirectory;
			process.StartInfo.ErrorDialog = false;
		}

		private Process StartProcess()
		{
			var process = new Process();
			PrepareProcess(process);
			process.Start();
			return process;
		}

		private void ProcessErrorStream()
		{
			try
			{
				string str;
				while ((str = _stdError.ReadLine()) != null)
				{
					lock (_lockObject)
					{
						ErrorWriter.WriteLine(str);
					}
				}
			}
			finally
			{
				ErrorWriter.Flush();
			}
		}

		private void ProcessStdStream()
		{
			try
			{
				string str;
				while ((str = _stdOut.ReadLine()) != null)
				{
					lock (_lockObject)
					{
						OutputWriter.WriteLine(str);
					}
				}
			}
			finally 
			{
				OutputWriter.Flush();
			}
		}
	}
}
