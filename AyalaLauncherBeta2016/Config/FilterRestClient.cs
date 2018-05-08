using System;
using AyalaLauncherBeta2016.Config;
using RestSharp;
namespace AyalaLauncherBeta2016.Config
{
	class FilterRestClient
	{
		private static string host = $"{Settings.FilterLauncherHost}:{Settings.FilterLauncherPort}{Settings.APIBaseUri}";
		private static string fallbackHost = $"{Settings.FilterLauncherHostFallback}:{Settings.FilterLauncherPort}{Settings.APIBaseUri}";

		// 2016 doesnt need any launcher-side login
		//public static LoginResp RestLogin(string u, string p)
		//{			
		//	string reqUri = @"/user/login";
		//	RestClient client = new RestClient(host);
		//	RestRequest request = new RestRequest(reqUri, Method.POST);
		//	request.AddParameter("Username", u);
		//	request.AddParameter("Password", p);
		//	request.AddParameter("AuthToken", App.AUTH_KEY);
		//	IRestResponse<LoginResp> loginResp = client.Execute<LoginResp>(request);
		//	return loginResp.Data;
		//}

		public static InfoResp RestInfoReq(int SrvID, string InfoType)
		{
			string reqUri = @"/inforeq/{srvid}/{infotype}/{authtoken}";
			RestClient client = new RestClient(host);
			RestRequest request = new RestRequest(reqUri, Method.GET);
			request.AddUrlSegment("srvid", SrvID);
			request.AddUrlSegment("infotype", InfoType);
			request.AddUrlSegment("authtoken", App.AUTH_KEY);
			IRestResponse<InfoResp> infoResp = client.Execute<InfoResp>(request);
			return infoResp.Data;
		}

		public static InfoResp RestPing(bool useFallback = false) 
		{
			string reqUri = @"/ping";
			RestClient client = new RestClient(useFallback ? fallbackHost : host);
			RestRequest request = new RestRequest(reqUri, Method.GET);
			IRestResponse<InfoResp> infoResp = client.Execute<InfoResp>(request);
			return infoResp.Data;
		}
	}
	// 2016 doesnt need any launcher-side login
	//public class LoginResp
	//{
	//	public String StatusCode { get; set; }
	//	public String Response { get; set; }
	//	public String Token { get; set; }
	//}

	public class InfoResp
	{
		public string StatusCode { get; set; }
		public string Response { get; set; }
		public string Info { get; set; }
	}

	public class RestToken
	{
		public string Status { get; set; }
		public string Token { get; set; }
	}
}
