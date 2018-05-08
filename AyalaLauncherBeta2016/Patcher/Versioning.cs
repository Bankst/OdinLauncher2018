using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

using AyalaLauncherBeta2016.Config;

namespace AyalaLauncherBeta2016.Patcher
{
	static class Versioning
    {
		private static string verPath = Path.Combine(Settings.LauncherDataFolder, "version.txt");
		private static string serverName = Settings.ServerName;

		public static int CurrentGameVersion
		{
			get
			{
				try
				{
					string ver = File.ReadAllText(verPath);
					if (!Regex.IsMatch(ver, @"^\d{6}$")) // not a valid version string
					{ // consider version invalid, set version back to 000000 to repatch
						string resetVer = "000000";
						return int.Parse(resetVer);
					}
					else
					{
						return int.Parse(ver);
					}
				}
				catch (FileNotFoundException)
				{
					return 0;
				}
			}
		}
		
		public static string FriendlyCurrentGameVersion()
        {
			// example 0.01.028, as int, 1028
			// example 1.91.028, as int, 191028
			try
			{
				string ver = CurrentGameVersion.ToString();
				ver = ver.PadLeft(6, '0');
				ver = ver.Insert(1, ".");
				ver = ver.Insert(4, ".");
				ver = serverName + " v" + ver;
				return ver;
			}
			catch (FileNotFoundException)
			{
				return serverName + " Unknown Version";
			}
			catch (ArgumentOutOfRangeException)
			{
				return serverName + " Unknown Version";
			}
        }

		public static Version LauncherVersion
		{
			get { return Assembly.GetEntryAssembly().GetName().Version; }
		}

		public static string FriendlyLauncherVersion
        {
            get { return "Launcher" + " v" + LauncherVersion.ToString(); }
        }

		public static void SetNewVersionAfterPatch(int newVersion)
		{
			string verToWrite = newVersion.ToString();
			verToWrite = verToWrite.PadLeft(6, '0');
			File.WriteAllText(verPath, verToWrite);
		}
	}
}
