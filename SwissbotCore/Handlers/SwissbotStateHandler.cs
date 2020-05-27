using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq.Dynamic;
using System.Linq;

namespace SwissbotCore.Handlers
{
    public class SwissbotStateHandler
    {
        public static string APIKey { get; set; }
        private static string Url = "https://api.swissdev.team/state/";
        private static void LogStateUpdate(string msg, ConsoleColor f = ConsoleColor.White, ConsoleColor b = ConsoleColor.Black)
        {
            Console.ForegroundColor = f;
            Console.BackgroundColor = b;
            Console.WriteLine("[ - State Update - ] - " + msg);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.BackgroundColor = ConsoleColor.Black;
        }

        /// <summary>
        /// Saves a new object into the state or creates one
        /// </summary>
        /// <param name="name">the name of the object</param>
        /// <param name="data">the json value of the object</param>
        public static async void SaveObject(string name, object data)
        {
            HttpClient c = new HttpClient();
            c.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(APIKey);
            Data d = new Data()
            {
                data = data
            };
            string json = JsonConvert.SerializeObject(d);
            var postdata = new StringContent(json, Encoding.UTF8, "application/json");
            var res = await c.PostAsync(Url + name, postdata);
            LogStateUpdate($"Saved {name} with {res.StatusCode}", ConsoleColor.Cyan);
        }
        private class Data
        {
            public object data { get; set; }
        }
        
        public static async Task<T> LoadObject<T>(string name)
        {
            string[] curobj = { };
            try
            {
                 curobj = await ListObjects();
            }
            catch(Exception e)
            {
                throw e;
            }
            if (curobj.Any(x => x == name))
            {
                HttpClient c = new HttpClient();
                c.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(APIKey);
                HttpResponseMessage resp = null;
                resp = await c.GetAsync(Url + name);
                if (resp.IsSuccessStatusCode)
                {
                    string cont = await resp.Content.ReadAsStringAsync();
                    LogStateUpdate($"Loaded {name}", ConsoleColor.Cyan);
                    return JsonConvert.DeserializeObject<T>(cont);
                }
                else
                {
                    LogStateUpdate($"Got a {resp.StatusCode} from the server while trying to load {name}", ConsoleColor.Red);
                    throw new Exception("Didnt get an OK!");
                }
            }
            else
                throw new Exception($"Could not find {name}");
        }
        public static async Task<string[]> ListObjects()
        {
            HttpClient c = new HttpClient();
            c.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(APIKey);
            HttpResponseMessage resp = null;
            resp = await c.GetAsync(Url);
            if (resp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string cont = await resp.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<string[]>(cont);
            }
            else
                throw new Exception("Didnt get an OK!");
        }
    }
}
