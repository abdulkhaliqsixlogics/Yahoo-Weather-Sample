using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace YahooWeather
{
    public class YahooWeatherSample
    {
        public static void Main()
        {
            string cURL = "https://weather-ydn-yql.media.yahoo.com/forecastrss";
            string cAppID = "test-app-id";
            string cConsumerKey = "your-consumer-key";
            string cConsumerSecret = "your-consumer-secret";
            string cOAuthVersion = "1.0";
            string cOAuthSignMethod = "HMAC-SHA1";
            string cWeatherID = Uri.EscapeDataString("Abu Dhabi, UAE"); 
            string cUnitID = "u=f";           // Metric units
            string cFormat = "json";

            string _get_timestamp()
            {
                TimeSpan lTS = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                return Convert.ToInt64(lTS.TotalSeconds).ToString();
            }  // end _get_timestamp

            string _get_nonce()
            {
                return Convert.ToBase64String(
                 new ASCIIEncoding().GetBytes(
                  DateTime.Now.Ticks.ToString()
                 )
                );
            }  // end _get_nonce

           
            string _get_auth()
            {
                string retVal;
                string lNonce = _get_nonce();
                string lTimes = _get_timestamp();
                string lCKey = string.Concat(cConsumerSecret, "&");
                string lSign = string.Format(  // note the sort order !!!
                 "format={0}&" +
                 "location={1}&" +
                 "oauth_consumer_key={2}&" +
                 "oauth_nonce={3}&" +
                 "oauth_signature_method={4}&" +
                 "oauth_timestamp={5}&" +
                 "oauth_version={6}&" +
                 "{7}",
                 cFormat,
                 cWeatherID,
                 cConsumerKey,
                 lNonce,
                 cOAuthSignMethod,
                 lTimes,
                 cOAuthVersion,
                 cUnitID
                );

                lSign = string.Concat(
                 "GET&", Uri.EscapeDataString(cURL), "&", Uri.EscapeDataString(lSign)
                );

                using (var lHasher = new HMACSHA1(Encoding.ASCII.GetBytes(lCKey)))
                {
                    lSign = Convert.ToBase64String(
                     lHasher.ComputeHash(Encoding.ASCII.GetBytes(lSign))
                    );
                }  // end using

                return "OAuth " +
                       "oauth_consumer_key=\"" + cConsumerKey + "\", " +
                       "oauth_nonce=\"" + lNonce + "\", " +
                       "oauth_timestamp=\"" + lTimes + "\", " +
                       "oauth_signature_method=\"" + cOAuthSignMethod + "\", " +
                       "oauth_signature=\"" + lSign + "\", " +
                       "oauth_version=\"" + cOAuthVersion + "\"";

            }  // end _get_auth


            string url = cURL + "?location=" + cWeatherID + "&" + cUnitID + "&format=" + cFormat;
            using (var client = new WebClient())
            {
                string responseText = string.Empty;
                try
                {
                    string headerString = _get_auth();

                    WebClient webClient = new WebClient();
                    webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                    webClient.Headers[HttpRequestHeader.Authorization] = headerString;
                    webClient.Headers.Add("X-Yahoo-App-Id", cAppID);
                    byte[] reponse = webClient.DownloadData(url);
                    string lOut = Encoding.ASCII.GetString(reponse);
                }
                catch (WebException exception)
                {
                    if (exception.Response != null)
                    {
                        var responseStream = exception.Response.GetResponseStream();
                        if (responseStream != null)
                        {
                            using (var reader = new StreamReader(responseStream))
                            {
                                responseText = reader.ReadToEnd();

                            }
                        }
                    }

                }
            }
        } 
    }
}
