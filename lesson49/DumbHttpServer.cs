using lesson49.Models;
using Newtonsoft.Json;
using RazorEngine;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace lesson49
{
    class DumbHttpServer
    {
        private Thread _serverThread;
        private string _siteDirectory;
        private HttpListener _listner;
        private int _port;
        public DumbHttpServer(string path, int port)
        {
            this.Initialize(path, port);
        }
        private void Initialize(string path, int port)
        {
            _siteDirectory = path;
            _port = port;
            _serverThread = new Thread(Listen);
            _serverThread.Start();
            Console.WriteLine("Сервер запущен на порту " + _port);
            Console.WriteLine("Файлы лежат в папке " + _siteDirectory);
        }
        public void Stop()
        {
            _serverThread.Abort();
            _listner.Stop();
        }
        private void Listen()
        {
            _listner = new HttpListener();
            _listner.Prefixes.Add("http://localhost:" + _port.ToString() + "/");
            _listner.Start();
            while (true)
            {
                try
                {
                    HttpListenerContext context = _listner.GetContext();
                    Process(context);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private List<Person> GetListFromJson(string filePath, int fromId, int toId)
        {
            var persons = new List<Person>();
            using (var r = new StreamReader(filePath))
            {
                string json = r.ReadToEnd();
                persons = JsonConvert.DeserializeObject<List<Person>>(json);
            }
            if (fromId == 0 && toId == 0)
            {
                return persons;
            }
            return persons.Skip(fromId-1).Take(toId - fromId + 1).ToList();
        }

        private void Process(HttpListenerContext context)
        {
            int.TryParse(context.Request.QueryString.Get("IdFrom"), out int fromId);
            int.TryParse(context.Request.QueryString.Get("IdTo"), out int toId);

            string fileName = context.Request.Url.AbsolutePath;
            string postRequest = context.Request.HttpMethod;
            if (postRequest == "POST")
            {
                Console.WriteLine(fileName);
                fileName = fileName.Substring(1);
                fileName = Path.Combine(_siteDirectory, fileName);
                string content = "";
                if (fileName.Contains("html"))
                {
                    content = BuildHtml(fileName, fromId, toId);
                }
                else
                {
                    content = File.ReadAllText(fileName);
                }
                if (File.Exists(fileName))
                {
                    try
                    {
                        byte[] htmlBytes = System.Text.Encoding.UTF8.GetBytes(content);
                        Stream fileStream = new MemoryStream(htmlBytes);
                        context.Response.ContentType = GetContentType(fileName);
                        context.Response.ContentLength64 = fileStream.Length;
                        byte[] buffer = new byte[16 * 1024];
                        int dataLength;
                        do
                        {
                            dataLength = fileStream.Read(buffer, 0, buffer.Length);
                            context.Response.OutputStream.Write(buffer, 0, dataLength);
                        } while (dataLength > 0);
                        fileStream.Close();
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        context.Response.OutputStream.Flush();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
                context.Response.OutputStream.Close();
            }
            else
            {
                Console.WriteLine(fileName);
                fileName = fileName.Substring(1);
                fileName = Path.Combine(_siteDirectory, fileName);
                string content = "";
                if (fileName.Contains("html"))
                {
                    content = BuildHtml(fileName, fromId, toId);
                }
                else
                {
                    content = File.ReadAllText(fileName);
                }
                if (File.Exists(fileName))
                {
                    try
                    {
                        byte[] htmlBytes = System.Text.Encoding.UTF8.GetBytes(content);
                        Stream fileStream = new MemoryStream(htmlBytes);
                        context.Response.ContentType = GetContentType(fileName);
                        context.Response.ContentLength64 = fileStream.Length;
                        byte[] buffer = new byte[16 * 1024];
                        int dataLength;
                        do
                        {
                            dataLength = fileStream.Read(buffer, 0, buffer.Length);
                            context.Response.OutputStream.Write(buffer, 0, dataLength);
                        } while (dataLength > 0);
                        fileStream.Close();
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        context.Response.OutputStream.Flush();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
                context.Response.OutputStream.Close();
            } 
        }

        private string GetContentType(string fileName)
        {
            var dictionary = new Dictionary<string, string>
            {
                { ".css", "text/css" },
                {".html", "text/html" },
                { ".ico", "image/x-icon"},
                { ".js", "application/x-javascript"},
                { ".json", "application/json"},
                { "png", "image/png"}
            };
            string contentType = "";
            string fileExtension = Path.GetExtension(fileName);
            dictionary.TryGetValue(fileExtension, out contentType);
            return contentType;
        }
        private string BuildHtml(string fileName, int fromId, int toId)
        {
            string html = "";
            string layoutPath = Path.Combine(_siteDirectory, "layout.html");
            string filePath = Path.Combine(_siteDirectory, fileName);
            var razorService = Engine.Razor;
            if(!razorService.IsTemplateCached("layout", null))
            {
                razorService.AddTemplate("layout", File.ReadAllText(layoutPath));
            }
            if(!razorService.IsTemplateCached(fileName, null))
            {
                razorService.AddTemplate(fileName, File.ReadAllText(filePath));
                razorService.Compile(fileName);
            }
            var persons = GetListFromJson("emploees.json", fromId, toId);
            html = razorService.Run(fileName,null,new {
                IndexTitle = "My Index Title",
                Page1 = "My page 1",
                Page2 = "My page 2",
                Page3 = "My page 3",
                X = 1,
                Persons = persons
            });
            return html;
        }
    }
}
