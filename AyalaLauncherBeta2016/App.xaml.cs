using System;
using System.Reflection;
using System.Resources;
using System.Windows;

namespace AyalaLauncherBeta2016
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
    {
        public static ResourceManager resourceManager;
        public static string targetDirectory = "";
		public const string AUTH_KEY = @"8A8B0A619DE5B10531A64A88899C19A3D0CF7C0ECD7BCBF0E75CBA0F6E7D6DC7";

		[STAThread]
        static void Main()
        {
            resourceManager = new ResourceManager("AyalaLauncherBeta2016.Properties.Strings", Assembly.GetExecutingAssembly());
            App app = new App();
            app.InitializeComponent();
            app.Run();
        }
    }	
}
