using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ServerRedirecting
{
    internal class Program
    {
        private static HttpListener listener;

        readonly static string help =
            "Arguments: [port] [url]\n" +
            "port - localhost port for redirect\n" +
            "url - redirect to this link with protocol\n";

        readonly static string page =
            "<!DOCTYPE><html><head>" +
            "<meta http-equiv=\"refresh\" content=\"0; url={0}\">" +
            "</head><body></body></html>";

        readonly static string pattern =
            @"^http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$";

        public static void Start(int port)
        {
            listener = new HttpListener();
            listener.Prefixes.Add($"http://127.0.0.1:{port}/");
            listener.Start();
        }

        public static async Task Handler(string url)
        {
            while (true)
            {
                HttpListenerContext context = await listener.GetContextAsync();
                HttpListenerResponse response = context.Response;

                byte[] buffer = Encoding.UTF8.GetBytes(string.Format(page, url));
                response.ContentType = "text/html";
                response.ContentEncoding = Encoding.UTF8;
                response.ContentLength64 = buffer.LongLength;

                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                response.Close();
            }
        }

        public static void Stop(string message = "")
        {
            Console.WriteLine(help + message);
            Environment.Exit(0);
        }

        static void Main(string[] args)
        {
            if (args.Length != 2)
                Stop();

            int port = 0;
            string url = args[1];

            try
            {
                port = int.Parse(args[0]);
            }
            catch (FormatException)
            {
                Stop("Incorrect port");
            }

            if (Regex.Match(url, pattern).Success)
                Stop("Incorrect url");

            try
            {
                Start(port);
                Handler(url).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Stop(e.Message);
            }
        }
    }
}
