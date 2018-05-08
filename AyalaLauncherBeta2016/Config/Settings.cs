using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

using AyalaLauncherBeta2016.API;
namespace AyalaLauncherBeta2016.Config
{
	static class Settings
	{
		private static string appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		private static string folderURI = Path.Combine(appdataPath, @"Ayala Online\Config");
		public static string LauncherDataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"reslauncher\");

		public static void InitLauncherDataFolder()
		{
			Directory.CreateDirectory(LauncherDataFolder);
		}
		
		#region WebConf
		// retrieve settings from FilterLauncher
		public const string FilterLauncherHost = "http://game.ayalaonline.net";
		public const string APIBaseUri = @"/api/v1/filter";
		public const int FilterLauncherPort = 10003;
		public const string FilterLauncherHostFallback = "http://home.meshnet0.com";

		/// <summary>
		/// Checks FilterLauncher for valid response to determine if connection issues exist
		/// </summary>
		/// <returns>
		/// Returns whether or not the server is accessible
		/// true/false
		/// </returns>
		public static bool ServerAccessible(bool checkFallback = false)
		{
			try
			{
				InfoResp pingResp = FilterRestClient.RestPing();
				return pingResp.Info.Equals("Pong");
			}
			catch
			{
				return false;
			}
		}
		

		public static bool ServerMaintenance(string Username, out Boolean MaintPatchAvailable, int serverID = 1)
		{
			try
			{
				MaintPatchAvailable = false;
				return false;
			}
			catch
			{
				MaintPatchAvailable = false;
				return false;
			}
		}

		private static string LauncherInfoRequest(string infoType, int serverID = 1)
		{
			try
			{
				InfoResp infoResp = FilterRestClient.RestInfoReq(serverID, infoType);
				return infoResp.Info;
			}
			catch
			{
				return null;
			}
		}
		
		public static string LauncherTitle { get { return LauncherInfoRequest("LauncherTitle") ; } } // Window title

		public static string LauncherPatchesURL { get { return LauncherInfoRequest("LauncherPatchesURL"); } } // Launcher self-update URL
		public static string LauncherVersionFN { get { return LauncherInfoRequest("LauncherVersionFN"); } } // Launcher version filename
		public static string LauncherPatchLogFN { get { return LauncherInfoRequest("PatchLogURL"); } } // Launcher Patch log filename

		public static string GamePatchesURL { get { return LauncherInfoRequest("GamePatchesURL"); } } // Game patches URL
		public static string GameVersionFN { get { return LauncherInfoRequest("GameVersionFN"); } } // Launcher version filename
		public static string GamePatchLogFN { get { return LauncherInfoRequest("GamePatchLogFN"); } } // Patch log URL

		public static string ServerIP // Game server IP
		{ 
			get
			{
				using (WebClientAuth wc = new WebClientAuth())
				{
					var gameHost = wc.DownloadString(LauncherInfoRequest("GameServerIP"));
					UriHostNameType hostType = Uri.CheckHostName(gameHost);
					if (hostType == UriHostNameType.Unknown) return null;
					else
					{
						IPHostEntry ipHE = Dns.GetHostEntry(gameHost);
						return ipHE.AddressList.FirstOrDefault().ToString();
					}
				}
			}
		} 
		public static string ServerName { get { return LauncherInfoRequest("GameServerName"); } } // Game name
		public static string ClientName { get { return LauncherInfoRequest("GameClientName"); } } // Client EXE name
		#endregion
	}
}
