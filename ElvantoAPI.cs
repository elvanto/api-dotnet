using System;
//using System.Collections.Generic;
using System.IO;
//using System.Linq;
using System.Net;
using System.Text;
//using System.Threading.Tasks;
using System.Web;

namespace ElvantoAPI
{
    public class ElvantoAPI
    {
        const string API_URL = "https://api.elvanto.com/v1/";
        const string OAUTH_URL = "https://api.elvanto.com/oauth/";
        const string TOKEN_URL = "https://api.elvanto.com/oauth/token";
        public string clientId { get; set; }
        public string clientSecret { get; set; }
        public string apiKey { get; set; }
        public bool oAuth { get; private set; }
        public string accessToken { get; set; }
        public string refreshToken { get; set; }
        public string expiresIn { get; set; }

        /// <summary>
        /// Constructor for OAuth authorization
        /// </summary>
        /// <param name="client_id"></param>
        /// <param name="client_secret"></param>
        public ElvantoAPI(string client_id, string client_secret)
        {
            clientId = client_id;
            clientSecret = client_secret;
            oAuth = true;
            accessToken = "";
            refreshToken = "";
            expiresIn = "";
        }

        /// <summary>
        /// Constructor with Api Key parameter for basic authorization
        /// </summary>
        /// <param name="api_key"></param>
        public ElvantoAPI(string api_key)
        {
            apiKey = api_key;
            oAuth = false;
            accessToken = "";
            refreshToken = "";
            expiresIn = "";
        }
        /// <summary>
        /// Return url for OAuth authorization.
        /// </summary>
        /// <param name="redirect_uri">Redirect uri</param>
        /// <param name="scope">Scope</param>
        /// <param name="isWebApp">Web or desktop application(true is web app)</param>
        /// <returns></returns>
        public string AuthorizeURL(string redirect_uri, string scope, bool isWebApp)
        {
            if (oAuth)
            {
                string type = "";
                if (isWebApp)
                    type = "web_server";
                else
                    type = "user_agent";

                return String.Format(OAUTH_URL + @"?type={0}&client_id={1}&redirect_uri={2}&scope={3}", type, this.clientId, redirect_uri, scope);
            }
            else
            {
                return "";
            }
        }
        /// <summary>
        /// Set tokens for current api object
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="refresh_token"></param>
        /// <param name="expires_in"></param>
        public void SetTokens(string access_token, string refresh_token, string expires_in)
        {
            accessToken = access_token;
            refreshToken = refresh_token;
            expiresIn = expires_in;
        }
        /// <summary>
        /// Return json string with access token, refresh token, expires_in parameter
        /// </summary>
        /// <param name="code">Code from authorized url</param>
        /// <param name="redirect_uri">redirect uri parameter</param>
        /// <returns></returns>
        public string GetTokens(string code, string redirect_uri)
        {
            string data = String.Format(@"grant_type=authorization_code&client_id={0}&client_secret={1}&code={2}&redirect_uri={3}",
                                                            this.clientId, this.clientSecret, code, HttpUtility.UrlEncode(redirect_uri));
            Byte[] bytes = Encoding.UTF8.GetBytes(data);

            WebRequest request = WebRequest.Create(TOKEN_URL);
            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = bytes.Length;

            Stream objStream = request.GetRequestStream();
            objStream.Write(bytes, 0, bytes.Length);
            objStream.Close();

            using (WebResponse response = request.GetResponse())
            {
                using (Stream dataStream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(dataStream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }
        /// <summary>
        /// Refresh expired access token, return json string with new access token, refresh token, expires in
        /// </summary>
        /// <returns>json string</returns>
        public string RefreshTokens()
        {
            string data = String.Format(@"grant_type=refresh_token&refresh_token={0}", this.refreshToken);
            Byte[] bytes = Encoding.UTF8.GetBytes(data);
            WebRequest request = WebRequest.Create(TOKEN_URL);
            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = bytes.Length;

            using (Stream objStream = request.GetRequestStream())
            {
                objStream.Write(bytes, 0, bytes.Length);
                objStream.Close();
            }
            using (WebResponse response = request.GetResponse())
            {
                using (Stream dataStream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(dataStream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }
        /// <summary>
        /// Call Elvanto API endpoint with parameters
        /// </summary>
        /// <param name="method">endpoint method like 'people/getAll'</param>
        /// <param name="json_parameters">json parameters for call method like '{"page": 1, "page_size": 100}' </param>
        /// <returns></returns>
        public string Call(string method, string json_parameters)
        {
            string url = API_URL + method + ".json";
            Byte[] bytes = Encoding.UTF8.GetBytes(json_parameters);
            WebRequest request = WebRequest.Create(url);
            request.Method = WebRequestMethods.Http.Post;
            request.ContentLength = bytes.Length;
            request.ContentType = "application/json";

            //API Key authentication
            if (!this.oAuth)
            {
                NetworkCredential networkCredential;
                networkCredential = new NetworkCredential(this.apiKey, "");
                CredentialCache myCredentialCache = new CredentialCache { { new Uri(url), "Basic", networkCredential } };
                request.PreAuthenticate = true;
                request.Credentials = myCredentialCache;
            }
            //OAuth authentication
            else
            {
                request.Headers.Add("Authorization", "Bearer " + this.accessToken);
            }

            using (Stream objStream = request.GetRequestStream())
            {
                objStream.Write(bytes, 0, bytes.Length);
                objStream.Close();
            }
            using (WebResponse response = request.GetResponse())
            {
                using (Stream dataStream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(dataStream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }
    }
}
