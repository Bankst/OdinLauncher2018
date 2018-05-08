using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Windows;
using AyalaLauncherBeta2016.Config;

namespace AyalaLauncherBeta2016
{
	/// <summary>
	/// Interaction logic for SplashWindow.xaml
	/// </summary>
	public partial class SplashWindow : Window
	{
		private BackgroundWorker splashWorker;
		private bool serverAccessible;

		public SplashWindow()
		{
			InitializeComponent();
			splashWorker = new BackgroundWorker
			{
				WorkerReportsProgress = false,
				WorkerSupportsCancellation = true
			};
			splashWorker.DoWork += new DoWorkEventHandler(Begin);
			splashWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(SplashWorker_RunWorkerCompleted);
		}

		private void SplashWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Cancelled)
			{
				Environment.Exit(0);
			}
			Hide();
			MainWindow mainWindow = new MainWindow(serverAccessible);
			mainWindow.Show();
			
		}

		private void Begin(object sender, DoWorkEventArgs e)
		{
			Thread.Sleep(500); // only so everyone can see the pretty logo :)
			// check to see if FilterLauncher is running
			serverAccessible = Settings.ServerAccessible();
			// if the first check fails, we run a fallback check just to be sure
			if (!serverAccessible)
			{
				serverAccessible = Settings.ServerAccessible(true);
				if (!serverAccessible)
				{
					var mb1 = MBErrorYesNo("Failed to connect to game server. Continue anyways?");
					if (!mb1) { e.Cancel = true; }
				}
			}

			// init settings systems
			Settings.InitLauncherDataFolder();
		}

		private bool MBErrorYesNo(string errorMsg)
		{
			return Dispatcher.Invoke(() =>
			{
				var mbResult = MessageBox.Show(this, errorMsg, "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
				if (mbResult == MessageBoxResult.Yes) return true;
				else return false;
			});
		}

		private void SplashWindow_Loaded(object sender, RoutedEventArgs e)
		{
			splashWorker.RunWorkerAsync();
		}
	}
}
