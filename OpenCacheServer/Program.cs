using Newtonsoft.Json;
using OpenCacheServer.Toolbox;
using RocksDbSharp;
using Serilog;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace OpenCacheServer
{
    class Program
    {
        static HttpListenerContext HttpContext;
        static HttpListenerRequest Request;
        static HttpListenerResponse Response;
        HttpActions actions = new HttpActions();
        static void Main(string[] args)
        {
            Log.Logger=new LoggerConfiguration()
               .MinimumLevel.Debug()
               .WriteTo.Console()
               .WriteTo.File("logs\\OpenCacheServer.txt",rollingInterval: RollingInterval.Day)
               .CreateLogger();

            HttpListener http = new HttpListener();
            //http.Prefixes.Add("http://publichost:2021/");
            http.Prefixes.Add("http://localhost:2021/");
            http.Start();
            while(true) {
                Console.WriteLine("Clients waiting");
                HttpContext=http.GetContext();
                Console.WriteLine("Client connected");
                Request=HttpContext.Request;
                Response=HttpContext.Response;
                HttpActions actions = new HttpActions() {
                    Request = Request,
                    Response = Response
                };
                switch(Request.HttpMethod) {
                    case "GET":
                        actions.Get();
                        break;
                    case "POST":
                        actions.Post();
                        break;
                    case "PUT":
                        actions.Put();
                        break;
                    case "DELETE":
                        actions.Delete();
                        break;
                }
                Response.Close();
            }
        }
    }
}
