using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;

namespace ksis3_server
{
    static class Server
    {

        static String root = "storage\\";
        public static void Listen()
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8080/");
            listener.Start();
            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                Log(context.Request);
                switch (context.Request.HttpMethod)
                {
                    case "GET":
                        responseGet(context);
                        break;
                    case "PUT":
                        responsePut(context);
                        break;
                    case "POST":
                        responsePost(context);
                        break;
                    case "DELETE":
                        responseDelete(context);
                        break;
                    case "MOVE":
                        responseMove(context);
                        break;
                    case "COPY":
                        responseCopy(context);
                        break;
                }
            }
        }


        private static void responseGet(HttpListenerContext context)
        {
            if (!File.Exists(getPath(context.Request.RawUrl)))
            {
                context.Response.StatusCode = 404;
                context.Response.Close();
                return;
            }
            Stream stream = context.Response.OutputStream;
            using (FileStream fileStream = File.OpenRead(getPath(context.Request.RawUrl)))
            {
                fileStream.CopyTo(stream);
            }
            context.Response.StatusCode = 200;
            context.Response.Close();
        }
        private static void responsePut(HttpListenerContext context)
        {
            Stream stream = context.Request.InputStream;

            (new FileInfo(getPath(context.Request.RawUrl))).Directory.Create();
            using (FileStream fileStream = File.OpenWrite(getPath(context.Request.RawUrl))) 
            {
                stream.CopyTo(fileStream);
            }
            context.Response.StatusCode = 200;
            context.Response.Close();

        }
        private static void responsePost(HttpListenerContext context)
        {
            Stream stream = context.Request.InputStream;

            (new FileInfo(getPath(context.Request.RawUrl))).Directory.Create();
            using (FileStream fileStream = File.Open(getPath(context.Request.RawUrl), FileMode.Append, FileAccess.Write))
            {
                stream.CopyTo(fileStream);
            }
            context.Response.StatusCode = 200;
            context.Response.Close();
        }
        private static void responseDelete(HttpListenerContext context)
        {
            if (!File.Exists(getPath(context.Request.RawUrl)))
            {
                context.Response.StatusCode = 404;
                context.Response.Close();
                return;
            }
            File.Delete(getPath(context.Request.RawUrl));
            context.Response.StatusCode = 200;
            context.Response.Close();
        }
        private static void responseCopy(HttpListenerContext context)
        {
            if (!File.Exists(getPath(context.Request.RawUrl)))
            {
                context.Response.StatusCode = 404;
                context.Response.Close();
                return;
            }

            (new FileInfo(root + context.Request.Headers["destination"])).Directory.Create();
            if (File.Exists(root + context.Request.Headers["destination"])) File.Delete(root + context.Request.Headers["destination"]);
            File.Copy(getPath(context.Request.RawUrl), root + context.Request.Headers["destination"]);

            context.Response.StatusCode = 200;
            context.Response.Close();
        }
        private static void responseMove(HttpListenerContext context)
        {
            if (!File.Exists(getPath(context.Request.RawUrl)))
            {
                context.Response.StatusCode = 404;
                context.Response.Close();
                return;
            }

            (new FileInfo(root + context.Request.Headers["destination"])).Directory.Create();
            if (File.Exists(root + context.Request.Headers["destination"])) File.Delete(root + context.Request.Headers["destination"]);
            File.Move(getPath(context.Request.RawUrl), root + context.Request.Headers["destination"]);

            context.Response.StatusCode = 200;
            context.Response.Close();
        }


        private static String getPath(String s)
        {
            return root + s.Substring(1);
        }

        private static void Log(HttpListenerRequest request)
        {
            Console.Write("received " + request.HttpMethod + " " + request.RawUrl);
            if (request.HttpMethod == "COPY" || request.HttpMethod == "MOVE")
            {
                Console.Write(" " +request.Headers["destination"]);
            }
            Console.WriteLine();
        }
    }
}
