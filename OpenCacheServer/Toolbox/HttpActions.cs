using Newtonsoft.Json;
using OpenCacheServer.Model;
using RocksDbSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace OpenCacheServer.Toolbox
{
    internal class HttpActions
    {
        internal HttpListenerRequest Request { get; set; }
        internal HttpListenerResponse Response { get; set; }
        RocksDb db;

        internal HttpActions()
        {
            string temp = Path.GetTempPath();
            string DBPath = Environment.ExpandEnvironmentVariables(Path.Combine(temp,"OpenCacheServer"));
            DbOptions options = new DbOptions()
                .SetCreateIfMissing(true)
                .IncreaseParallelism(10)
                .PrepareForBulkLoad()
                .SetAllowConcurrentMemtableWrite(true)
                .SkipStatsUpdateOnOpen(true)
                .SetUseDirectReads(true)
                .EnableStatistics();
            db = RocksDb.Open(options,DBPath);
        }
        internal void Get() {
            Console.WriteLine(JsonConvert.SerializeObject(new { Date = DateTime.Now,RequestUrl = Request.Url,HttpMethod = Request.HttpMethod },Formatting.Indented));
            string resdata = new {
                date = DateTime.Now,
                UserAgent = Request.UserAgent,
                Headers = Request.Headers,
                ProtocolVersion = Request.ProtocolVersion,
                UserLanguages = Request.UserLanguages,
                HasEntityBody = Request.HasEntityBody,
                Cookies = Request.Cookies,
                ContentEncoding = Request.ContentEncoding,
                HttpMethod = Request.HttpMethod
            }.ToJson();
            byte[] responseBytes = Encoding.UTF8.GetBytes(resdata);
            Response.AddHeader("Content-Type","application/json ; charset=utf-8");
            Response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
        }

        internal void Post()
        {
            if(Request.Headers["AuthKey"].Equals(Utils.ReadConfigFile().configs.AuthKey)) {
                if(Request.HasEntityBody) {
                    Stream body = Request.InputStream;
                    Encoding encoding = Request.ContentEncoding;
                    StreamReader reader = new StreamReader(body,encoding);
                    string postdata = reader.ReadToEnd();
                    body.Close();
                    reader.Close();
                    FilterModel fm = postdata.FromJson<FilterModel>();
                    byte[] responseBytes = Encoding.UTF8.GetBytes(PostDb(fm.IndexKey));
                    Response.AddHeader("Content-Type","application/json ; charset=utf-8");
                    Response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
                }
            } else {
                byte[] responseBytes = Encoding.UTF8.GetBytes("Error: Post data is not acceptable");
                Response.AddHeader("Content-Type","application/json ; charset=utf-8");
                Response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
            }
        }

        internal void Put()
        {
            if(Request.Headers["AuthKey"].Equals(Utils.ReadConfigFile().configs.AuthKey)) {
                if(Request.HasEntityBody) {
                    Stream body = Request.InputStream;
                    Encoding encoding = Request.ContentEncoding;
                    StreamReader reader = new StreamReader(body,encoding);

                    string putdata = reader.ReadToEnd();
                    body.Close();
                    reader.Close();
                    FilterModel fm = putdata.FromJson<FilterModel>();
                    byte[] responseBytes = Encoding.UTF8.GetBytes(PutDb(fm.IndexKey, putdata).ToString());
                    Response.AddHeader("Content-Type","application/json ; charset=utf-8");
                    Response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
                }
            } else {
                byte[] responseBytes = Encoding.UTF8.GetBytes("Error: Put data is not acceptable");
                Response.AddHeader("Content-Type","application/json ; charset=utf-8");
                Response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
            }
        }

        internal void Delete()
        {
            if(Request.Headers["AuthKey"].Equals(Utils.ReadConfigFile().configs.AuthKey)) {
                if(Request.HasEntityBody) {
                    Stream body = Request.InputStream;
                    Encoding encoding = Request.ContentEncoding;
                    StreamReader reader = new StreamReader(body,encoding);
                    string deldata = reader.ReadToEnd();
                    body.Close();
                    reader.Close();
                    FilterModel fm = deldata.FromJson<FilterModel>();
                    byte[] responseBytes = Encoding.UTF8.GetBytes(DeleteDb(fm.IndexKey).ToString());
                    Response.AddHeader("Content-Type","application/json ; charset=utf-8");
                    Response.OutputStream.Write(responseBytes,0,responseBytes.Length);
                }
            } else {
                byte[] responseBytes = Encoding.UTF8.GetBytes("Error: Delete data is not acceptable");
                Response.AddHeader("Content-Type","application/json ; charset=utf-8");
                Response.OutputStream.Write(responseBytes,0,responseBytes.Length);
            }
        }

        /// <summary>
        /// Get Key's value from Rocksdb
        /// </summary>
        /// <param name="Key">Search Key</param>
        /// <returns></returns>
        string PostDb(string Key)
        {
            try {
                string dbData = db.Get(Key);
                
                return dbData;
            } catch(Exception exc) {
                Log.Error(exc,$"PutDb(key:{Key})");
                return "Error saved to logs";
            }
        }

        /// <summary>
        /// Remove value from Rocksdb
        /// </summary>
        /// <param name="Key">Search Key</param>
        /// <returns></returns>
        bool DeleteDb(string Key)
        {
            try {
                db.Remove(Key);
                return true;
            } catch(Exception exc) {
                Log.Error(exc,$"DeleteDb(key:{Key})");
                return false;
            }
        }

        /// <summary>
        /// Update or Insert Key's value from Rocksdb
        /// </summary>
        /// <param name="Key">Search Key</param>
        /// <returns></returns>
        bool PutDb(string Key,string Value)
        {
            try {
                db.Put(Key,Value);
                return true;
            } catch(Exception exc) {
                Log.Error(exc,$"PutDb(key:{Key}, value:{Value})");
                return false;
            }
        }
    }
}
