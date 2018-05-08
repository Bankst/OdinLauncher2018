using AyalaLauncherBeta2016.Config;
using AyalaLauncherBeta2016.Patcher;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AyalaLauncherBeta2016
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private string patcherDirectory;
		public BackgroundWorker patchUpdaterWorker;
		private AutoResetEvent patchDownloadEvent;
		private Exception patchDownloadingException;

		public BackgroundWorker launcherUpdaterWorker;
		private AutoResetEvent launcherDownloadEvent;
		private Exception launcherDownloadingException;

		public bool ServerAccessible;
		public bool ServerMaintenance;

		public MainWindow(bool ServerAccessible)
		{
			this.ServerAccessible = ServerAccessible;
			InitializeComponent();
		}

		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			SetVersions();
			SetupUpdaters();
			launcherUpdaterWorker.RunWorkerAsync();
		}

		private void SetVersions()
		{
			string newGameVersion = Versioning.FriendlyCurrentGameVersion();
			GameVersionLabel.Content = newGameVersion;
			LauncherVersionLabel.Content = Versioning.FriendlyLauncherVersion;
		}

		private void SetupUpdaters()
		{
			PlayButton.IsEnabled = false;
			patcherDirectory = Settings.LauncherDataFolder;

			launcherDownloadEvent = new AutoResetEvent(false);
			launcherUpdaterWorker = new BackgroundWorker
			{
				WorkerReportsProgress = true,
				WorkerSupportsCancellation = true
			};
			launcherUpdaterWorker.DoWork += new DoWorkEventHandler(LauncherUpdate);
			launcherUpdaterWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(LauncherUpdateComplete);

			patchDownloadEvent = new AutoResetEvent(false);
			patchUpdaterWorker = new BackgroundWorker
			{
				WorkerReportsProgress = true,
				WorkerSupportsCancellation = true
			};
			patchUpdaterWorker.DoWork += new DoWorkEventHandler(GameUpdate);
			patchUpdaterWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(GameUpdateComplete);
		}

		private void LauncherUpdate(object sender, DoWorkEventArgs e)
		{
			SetProgressBar(0);
			BackgroundWorker worker = (BackgroundWorker)sender;

			Version currentVersion = Versioning.LauncherVersion;
			Version latestVersion;
			SetTextStatus(App.resourceManager.GetString("lc_checking_new_version"));

			if (worker.CancellationPending)
			{
				e.Cancel = true;
				return;
			}
			try
			{
				String versionURI = $"{Settings.LauncherPatchesURL}{Settings.LauncherVersionFN}";
				using (WebClientAuth wc = new WebClientAuth())
				{
					latestVersion = Version.Parse(wc.DownloadString(versionURI));
				}
			}
			catch (FormatException ex)
			{
				e.Cancel = true;
				MBError(String.Format("Server has invalid response for url `{0}` . {1}", $"{Settings.LauncherPatchesURL}{Settings.LauncherVersionFN}", ex.Message));
				return;
			}
			catch (WebException ex)
			{
				if (ex.Message.Contains("timed out"))
				{
					MBError("Unable to retrieve latest version, request timed out.");
				}
				HttpWebResponse response = (HttpWebResponse)ex.Response;
				if (response == null)
				{
					MBError("Unable to retrieve latest version, no response.");
				} else
				{
					MBError(String.Format(
						"{0} \nResponse: {1} {2}",
						"Unable to retrieve latest version.",
						(int)response.StatusCode,
						response.StatusDescription
					));
				}
				
				e.Cancel = true;
				return;
			}
			catch (KeyNotFoundException ex)
			{
				e.Cancel = true;
				MBError(ex.Message);
				MBError(ex.StackTrace);
				return;
			}


			if (worker.CancellationPending)
			{
				e.Cancel = true;
				return;
			}
			int versionDifference = currentVersion.CompareTo(latestVersion);
			if (versionDifference.Equals(-1))
			{
				SetTextStatus(App.resourceManager.GetString("start_downloading"));
				WebClientAuth webClient = new WebClientAuth();
				webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(LauncherWebClient_DownloadProgressChanged);
				webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(LauncherWebClient_DownloadFileCompleted);
				string tempLauncherFilename = Path.GetTempFileName();
				Debug.WriteLine(tempLauncherFilename);
				string launcher_filename = "AyalaLauncher.exe";
				string launcher_uri = Settings.LauncherPatchesURL + launcher_filename;

				launcherDownloadEvent.Reset();
				try
				{
					webClient.DownloadFileAsync(new Uri(launcher_uri), tempLauncherFilename, launcher_filename);
				}
				catch (Exception ex)
				{
					e.Cancel = true;

					MBError(ex.Message + "\n" + ex.StackTrace);
					return;
				}
				launcherDownloadEvent.WaitOne();

				if (launcherDownloadingException != null)
				{
					WebException webex = (WebException)launcherDownloadingException;
					HttpWebResponse response = (HttpWebResponse)webex.Response;
					e.Cancel = true;
					MBError(String.Format(
						"{0}\nMessage: {1}\nRequest: {2}\nResponse: {3}",
						App.resourceManager.GetString("cant_get_patch"),
						webex.Message,
						(response != null ? response.ResponseUri.ToString() : "null"),
						(response != null ? response.StatusDescription : "null")
						));
					return;
				}

				try
				{
					LauncherUpdater.RenameRunningLauncher();
					LauncherUpdater.CopyNewLauncher(tempLauncherFilename);
				}
				catch (Exception ex)
				{
					MBError(String.Format(App.resourceManager.GetString("unknown_exception"), ex.Message, ex.StackTrace));
					e.Cancel = true;
					return;
				}
				finally
				{
					LauncherUpdater.RestartToNewLauncher();
				}
				File.Delete(tempLauncherFilename);
			}
		}

		private void LauncherUpdateComplete(object sender, RunWorkerCompletedEventArgs e)
		{
			if (!e.Cancelled)
			{
				PlayButton.IsEnabled = true;
				SetTextStatus(App.resourceManager.GetString("update_done"));
				SetProgressBar(100);
				patchUpdaterWorker.RunWorkerAsync();
				return;
			}

			SetTextStatus(App.resourceManager.GetString("update_error"));
			SetProgressBar(0);
		}

		private void LauncherWebClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
		{
			if (e.Error != null)
				launcherDownloadingException = e.Error;
			else
				launcherDownloadingException = null;

			launcherDownloadEvent.Set();
		}

		private void LauncherWebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			SetTextStatus(string.Format(App.resourceManager.GetString("downloading"),
				"New Launcher",
				Updater.HumanReadableSizeFormat(e.BytesReceived),
				Updater.HumanReadableSizeFormat(e.TotalBytesToReceive),
				e.ProgressPercentage.ToString()));

			SetProgressBar(e.ProgressPercentage);
		}

		private void GameUpdateComplete(object sender, RunWorkerCompletedEventArgs e)
		{
			if (!e.Cancelled)
			{
				PlayButton.IsEnabled = true;
				SetTextStatus(App.resourceManager.GetString("update_done"));
				SetProgressBar(100);
				return;
			}

			SetTextStatus(App.resourceManager.GetString("update_error"));
			SetProgressBar(0);
		}

		private void UpdateVersionAfterPatch(int verNumber)
		{
			Versioning.SetNewVersionAfterPatch(verNumber);
			if (!Dispatcher.CheckAccess())
			{
				Dispatcher.Invoke(() => GameVersionLabel.Content = Versioning.FriendlyCurrentGameVersion());
			} else GameVersionLabel.Content = Versioning.FriendlyCurrentGameVersion();
		}

		private void SetTextStatus(string text)
		{
			Dispatcher.Invoke(() =>
			{
				UpdaterStatusLabel.Content = text;
			});
		}

		private void SetProgressBar(int percent)
		{
			Dispatcher.BeginInvoke((Action)(() =>
			{
				UpdaterProgressBar.Value = percent;
			}));
		}



		private void MBError(string errorMsg)
		{
			Dispatcher.Invoke(() =>
			{
				MessageBox.Show(this, errorMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			});
		}

		private void MBError(string errorMsg, string title)
		{
			Dispatcher.Invoke(() =>
			{
				MessageBox.Show(this, errorMsg, title, MessageBoxButton.OK, MessageBoxImage.Error);
			});
		}

		private bool MBErrorYesNo(string errorMsg)
		{
			return Dispatcher.Invoke(() =>
			{
				var mbResult = MessageBox.Show(this, errorMsg, "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
				return mbResult.Equals(MessageBoxResult.Yes) ? true : false;
			});
			
		}

		private void GameUpdate(object sender, DoWorkEventArgs e)
        {
            SetProgressBar(0);
            SetTextStatus(App.resourceManager.GetString("checking_version"));
            BackgroundWorker worker = (BackgroundWorker)sender;

            int currentVersion = 0;
            int lastVersion = 0;

            try
            {
                currentVersion = Versioning.CurrentGameVersion;
            }
            catch
            {
                e.Cancel = true;
				MBError(App.resourceManager.GetString("cant_get_current_version"));
                return;
            }

            SetTextStatus(App.resourceManager.GetString("checking_new_version"));
            if (worker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            try
            {
				String versionURI = $"{Settings.GamePatchesURL}{Settings.GameVersionFN}";
				using (WebClientAuth wc = new WebClientAuth())
				{
					lastVersion = int.Parse(wc.DownloadString(versionURI));
				}
            } catch (FormatException)
            {
                e.Cancel = true;
                MBError(string.Format("For url `{0}` server has invalid response. Integer is expected.", $"{Settings.GamePatchesURL}{Settings.GameVersionFN}"));
                return;
			} catch (WebException ex)
			{
				e.Cancel = true;
				HttpWebResponse response = (HttpWebResponse)ex.Response;
				MBError($"{App.resourceManager.GetString("cant_get_new_version")} \nResponse: {(int)response.StatusCode} {response.StatusDescription}");
				return;
			} catch (KeyNotFoundException ex)
			{
				e.Cancel = true;
				MBError(ex.Message);
				MBError(ex.StackTrace);
				return;
			}
			

            if (worker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }
			
            for (int i = currentVersion; i < lastVersion; i++)
            {
                SetTextStatus(App.resourceManager.GetString("start_downloading"));
                WebClientAuth webClient = new WebClientAuth();
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(GameWebClient_DownloadProgressChanged);
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(GameWebClient_DownloadFileCompleted);
                string tempPatchFilename = Path.GetTempFileName();
                string patch_filename = string.Format("{0}_{1}.patch", i, i + 1);
                string patch_uri = Settings.GamePatchesURL + patch_filename;

                patchDownloadEvent.Reset();
                try
                {
                    webClient.DownloadFileAsync(new Uri(patch_uri), tempPatchFilename, patch_filename);
                }
                catch (UriFormatException)
                {
                    e.Cancel = true;
                    MBError("Configuration.patches_directory is incorrect. Nothing can be downloaded from `" + patch_uri + "`. Url expected.");
                    return;
                }
                patchDownloadEvent.WaitOne();

                if (patchDownloadingException != null)
                {
                    WebException webex = (WebException)patchDownloadingException;
                    HttpWebResponse response = (HttpWebResponse)webex.Response;
                    e.Cancel = true;
                    MBError(App.resourceManager.GetString("cant_get_patch") + "\nMessage: " + webex.Message + "\nRequest: " + (response != null ? response.ResponseUri.ToString() : "null") + "\nResponse: " + (response != null ? response.StatusDescription : "null"));
                    return;
                }

                try
                {
                    Updater.ApplyPatch(tempPatchFilename, new Action<string, int>(delegate (string text, int percent)
                    {
                        SetTextStatus(text);
                        SetProgressBar(percent);
                    }));
                }
                catch (PatcherException ex)
                {
                    MBError($"Error patching game:\n{ex.Message}", "Patcher error");
                    e.Cancel = true;
                    return;
                }
                catch (Exception ex)
                {
                    MBError(string.Format(App.resourceManager.GetString("unknown_exception"), ex.Message, ex.StackTrace));
                    e.Cancel = true;
                    return;
                }
				UpdateVersionAfterPatch(i + 1);
                File.Delete(tempPatchFilename);
            }

        }

		private void GameWebClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
                patchDownloadingException = e.Error;
            else
                patchDownloadingException = null;

            patchDownloadEvent.Set();
        }

		private void GameWebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            SetTextStatus(string.Format(App.resourceManager.GetString("downloading"),
                e.UserState.ToString(),
                Updater.HumanReadableSizeFormat(e.BytesReceived),
                Updater.HumanReadableSizeFormat(e.TotalBytesToReceive),
                e.ProgressPercentage.ToString()));

            SetProgressBar(e.ProgressPercentage);
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
			PlayButton.IsEnabled = false;
            if (StartClient(out string mbText))
            {
                Environment.Exit(1);
            }
            else
            {
                MessageBox.Show(Application.Current.MainWindow, mbText, "Error");
				PlayButton.IsEnabled = true;
            }

        }

		private bool StartClient(out string errorMessage)
        {
			var clientExe = Settings.ClientName;
			var serverIP = Settings.ServerIP;
			errorMessage = null;			
			try
			{
				Process.Start(clientExe, $"-osk_server {serverIP}");
				return true;
			}
			catch (FileNotFoundException)
			{
				errorMessage = $"Can't find game executable: {clientExe}\n Please reinstall game.";
				return false;
			}
			catch (Exception ex)
			{
				errorMessage = $"Client start error: {ex.Message}";
				return false;
			}
        }

		private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

		private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

		private void UpdaterProgressBar_Loaded(object sender, RoutedEventArgs e)
        {
            var progressBar = sender as ProgressBar;
            if (progressBar == null) return;

            progressBar.Foreground = new SolidColorBrush(Colors.Pink);
        }
	}
}