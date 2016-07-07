
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace IISLogsReplay
{
    public static class WebClientHelper
    {

        public static HttpStatusCode? Get(string server, string path, string queryString, string userAgent = null, string headers = null, string cookies = null)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(server);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    if (userAgent != null)
                    {
                        client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
                    }

                    if (headers != null)
                    {
                        var dotsLength = headers.IndexOf(":");
                        string name = headers.Substring(0, dotsLength);
                        string value = headers.Substring(dotsLength+1).Trim();
                        client.DefaultRequestHeaders.Add(name, value); //it only works for only one header, have to change for many headers
                    }

                    if (cookies != null)
                    {
                        client.DefaultRequestHeaders.Add("Cookie", cookies); // Format : ("cookiename1=cookievalue1;cookiename2=cookievalue2");
                    }

                    var uri = BuildUrl(path, queryString);
                    Task<HttpResponseMessage> response = client.GetAsync(uri);
                    response.Wait();
                    if (response.Result != null && response.Result.IsSuccessStatusCode)
                    {
                        return response.Result.StatusCode;
                    }
                }

            }
            catch(Exception ex)
            {

            }

            return null;
        }

        #region Private Methods
        private static string BuildUrl(string path, string queryString)
        {
            StringBuilder urlBuilder = new StringBuilder();
            urlBuilder.Append(path);
            if (!string.IsNullOrEmpty(queryString))
            {
                urlBuilder.Append("?");
                urlBuilder.Append(queryString);
            }

            return urlBuilder.ToString();
        }

        private static string ConvertToString(this Stream data)
        {
            string result = null;
            try
            {
                using (StreamReader reader = new StreamReader(data))
                {
                    result = reader.ReadToEnd();
                }
            }
            finally
            {
                data.Close();
            }

            return result;
        }
        #endregion
    }
}
