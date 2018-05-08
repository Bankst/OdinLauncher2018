using System;
using System.Diagnostics;
using System.IO;

namespace AyalaLauncherBeta2016.Patcher
{
	static class LauncherUpdater
	{
		/* map
		* 
		* Check for new launcher version
		* If new version, download NewLauncher.exe
		*	rename running file to OldLauncher.exe
		*	copy NewLauncher.exe to Launcher.exe
		*	do Batch
		* Else, start GameUpdateWorker task
		* 
		*/

		/* batch map
		 *	
		 *	close RunningLauncher
		 *	delete OldLauncher.exe
		 *	start Launcher.exe
		 *
		 */

		private static string GenericLauncherPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AyalaLauncher.exe");
		private static string CurrentLauncherPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.FriendlyName);
		private static string OldLauncherPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OLD" + AppDomain.CurrentDomain.FriendlyName);

		internal static void RenameRunningLauncher()
		{
			try { if (File.Exists(OldLauncherPath)) { File.Delete(OldLauncherPath); } }
			catch { throw new IOException("Process still running"); }
			try { File.Move(CurrentLauncherPath, OldLauncherPath); }
			catch { throw; }
			
		}

		internal static void CopyNewLauncher(String NewLauncherPath)
		{
			if (File.Exists(GenericLauncherPath)) { File.Delete(GenericLauncherPath); }
			File.Copy(NewLauncherPath, GenericLauncherPath);
		}

		internal static void RestartToNewLauncher()
		{
			string strCmdText = $"/C taskkill /f /im {Path.GetFileName(CurrentLauncherPath)}&"; // close running launcher process
			strCmdText += "ping 127.0.0.1 -n 3 > nul&";
			strCmdText += $"DEL /F {Path.GetFileName(OldLauncherPath)}&"; // delete the oldlauncher.exe
			strCmdText += $"start {Path.GetFileName(GenericLauncherPath)}&"; // start launcher.exe

			ProcessStartInfo psi = new ProcessStartInfo
			{
				FileName = "CMD.EXE",
				WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
				Arguments = strCmdText,
				UseShellExecute = true,
				Verb = "runas",
				WindowStyle = ProcessWindowStyle.Hidden,
				CreateNoWindow = true
			};
			Process.Start(psi);
		}
	}
}
