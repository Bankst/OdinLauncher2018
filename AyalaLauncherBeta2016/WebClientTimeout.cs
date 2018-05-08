using System;
using System.Net;

namespace AyalaLauncherBeta2016
{
	public class WebClientTimeout : WebClient
	{
		public int Timeout { get; set; }

		/// <summary>
		/// Creates a standard WebClient
		/// </summary>
		public WebClientTimeout() { }

		/// <summary>
		/// Creates a new Webclient with settable Timeout
		/// </summary>
		/// <param name="Timeout">Timeout in ms</param>
		public WebClientTimeout(int Timeout, String HeaderData)
		{
			this.Timeout = Timeout;
		}

		protected override WebRequest GetWebRequest(Uri uri)
		{
			WebRequest w = base.GetWebRequest(uri);
			w.Timeout = Timeout;
			return w;
		}
	}
}
