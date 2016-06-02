
using System.IO;
using System.Net;
using System.Text;

namespace IISLogsReplay
{
    public static class WebClientHelper
    {

        public static string Get(string server, string path, string queryString, string userAgent = null, string headers = null, string cookies = null)
        {
            using (WebClient client = new WebClient())
            {
                if(userAgent != null)
                {
                    client.Headers.Add("user-agent", userAgent);
                }

                if(headers != null)
                {
                    client.Headers.Add(headers); //it only works for only one header, have to change for many headers
                }

                if(cookies != null)
                {
                    client.Headers.Add(HttpRequestHeader.Cookie, cookies); // Format : ("cookiename1=cookievalue1;cookiename2=cookievalue2");
                }

                var uri = BuildUrl(server, path, queryString);
                var data = client.OpenRead(uri);

                return data.ConvertToString();
            }
        }

        #region Private Methods
        private static string BuildUrl(string server, string path, string queryString)
        {
            StringBuilder urlBuilder = new StringBuilder();
            urlBuilder.Append(server);
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
