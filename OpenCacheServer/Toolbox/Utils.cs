using Newtonsoft.Json;
using OpenCacheServer.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using XSerializer;

namespace OpenCacheServer.Toolbox
{
    public static class Utils
    {

        /// <summary>
        /// Parametrelerinde null nesne veya boş string içerir
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        internal static bool NullOrEmptyExist(params dynamic[] values)
        {
            for(int i = 0; i<values.Count(); i++) {
                if(values[i]?.GetType()==typeof(string)) {
                    if(string.IsNullOrEmpty(values[i])||string.IsNullOrWhiteSpace(values[i]))
                        return true;
                } else {
                    if(values[i] is null)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gönderilen string ifadede genel kabul görmeyecek bir karakter olup olmadığını kontrol eder. False ise ignored
        /// </summary>
        /// <param name="words">Kontrol bekleyen cümle</param>
        /// <returns></returns>
        internal static bool CheckIgnoredChars(this string words)
        {
            bool result = true;
            if(words?.Length<5)
                return false;
            char[] acceptedChars = "abcdefghijklmnoprstuvyzwxq_@-.1234567890".ToCharArray();
            words?.ToList<char>().ForEach(c => {
                if(!acceptedChars.Contains(c))
                    result=false;
            });
            return result;
        }

        /// <summary>
        /// Şifre karışıklığını kontrol için kullanılır
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        internal static bool CheckPasswordComplex(this string password)
        {
            return (password!=null&&
                password.Any(char.IsUpper)&&
                password.Any(char.IsLower)&&
                password.Any(char.IsDigit)&&
                password.Length>5
            );
        }

        /// <summary>
        /// Sha256 ile şifrelenmiş string döndürür
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        internal static string EncryptHash(this string rawData)
        {
            using(SHA256 sha256Hash = SHA256.Create()) {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for(int i = 0; i<bytes.Length; i++) {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// İp adresinin veya domain adresinin çözümlenmesi için kullanılır
        /// </summary>
        /// <param name="ipAddr"></param>
        /// <returns></returns>
        internal static IpResult ResolveIpAddress(string ipAddr)
        {
            var result = JsonConvert.DeserializeObject<IpResult>(GetWebJson($"http://ip-api.com/json/{ipAddr}?fields=63700991"));
            return result;
        }

        /// <summary>
        /// Nesneyi json'a çevirir
        /// </summary>
        /// <param name="source">Json'a çevirilecek nesne</param>
        /// <returns></returns>
        internal static string ToJson(this object source)
        {
            return JsonConvert.SerializeObject(source);
        }

        /// <summary>
        /// Json string'i nesneye çevirir
        /// </summary>
        /// <typeparam name="T">Çevirilecek tür</typeparam>
        /// <param name="source">Çevirilecek json string</param>
        /// <returns></returns>
        internal static T FromJson<T>(this string source)
        {
            return JsonConvert.DeserializeObject<T>(source);
        }

        /// <summary>
        /// Nesneyi XML stringe dönüştürür
        /// </summary>
        /// <param name="source">xml'e dönüşecek nesne</param>
        /// <returns></returns>
        internal static string XmlSerialize<T>(this T source)
        {
            XmlSerializer<T> serializer = new XmlSerializer<T>(typeof(T));
            string result = serializer.Serialize(source);
            return result;
        }

        /// <summary>
        /// XML stringi nesneye dönüştürür
        /// </summary>
        /// <param name="source">Nesneye dönüşecek XML</param>
        /// <returns></returns>
        internal static T XmlDeserialize<T>(this string source)
        {
            XmlSerializer<T> serializer = new XmlSerializer<T>(typeof(T));
            T result = serializer.Deserialize(source);
            return result;
        }

        /// <summary>
        /// Web adresine get isteği yapar json kontrolü yoktur
        /// </summary>
        /// <param name="url">Okunacak web adresi</param>
        /// <returns></returns>
        internal static string GetWebJson(string url)
        {
            WebClient wc = new WebClient();
            string response = wc.DownloadString(url);
            return response;
        }

        /// <summary>
        /// Projenin config.xml dosyasını okur
        /// </summary>
        /// <returns></returns>
        internal static ConfigModel ReadConfigFile()
        {
            string cmFile = File.ReadAllText("config.json");
            var jsonParse = cmFile.FromJson<ConfigModel>();
            return jsonParse;
        }
    }

    public class IpResult
    {
        public string status { get; set; }
        public string country { get; set; }
        public string countryCode { get; set; }
        public string region { get; set; }
        public string regionName { get; set; }
        public string city { get; set; }
        public string district { get; set; }
        public string zip { get; set; }
        public float lat { get; set; }
        public float lon { get; set; }
        public string timezone { get; set; }
        public int offset { get; set; }
        public string currency { get; set; }
        public string isp { get; set; }
        public string org { get; set; }
        public string _as { get; set; }
        public string asname { get; set; }
        public string reverse { get; set; }
        public bool mobile { get; set; }
        public bool proxy { get; set; }
        public bool hosting { get; set; }
        public string query { get; set; }
    }

}