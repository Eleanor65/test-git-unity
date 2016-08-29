using System.Threading;

namespace DTI.SourceControl.ConsoleTools
{
	internal class ThreadUtility
	{
		/// <summary>
		/// Do our best to kill a thread, passing state info
		/// </summary>
		/// <param name="thread">The thread to kill</param>
		/// <param name="stateInfo">Info for the ThreadAbortException handler</param>
		public static void KillThread(Thread thread)
		{
			try
			{
				thread.Abort();
			}
			catch (ThreadStateException)
			{
				// This is deprecated but still needed in this case
				// in order to kill the thread. The warning can't
				// be disabled because the #pragma directive is not
				// recognized by the .NET 1.1 compiler.
				thread.Resume();
			}

			if ((thread.ThreadState & ThreadState.WaitSleepJoin) != 0)
				thread.Interrupt();
		}
	}
}
