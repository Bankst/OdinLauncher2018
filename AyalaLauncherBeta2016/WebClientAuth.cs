using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AyalaLauncherBeta2016
{
	public class WebClientAuth : WebClient
	{
		public int Timeout { get; set; }

		/// <summary>
		/// Creates a new Webclient with Header auth and timeout of 1500ms
		/// </summary>
		public WebClientAuth() { this.Headers["AyalaAuthToken"] = App.AUTH_KEY; }

		protected override WebRequest GetWebRequest(Uri uri)
		{
			WebRequest w = base.GetWebRequest(uri);
			w.Headers["AyalaAuthToken"] = App.AUTH_KEY;
			return w;
		}
	}
}
