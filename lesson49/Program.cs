using System;
using System.IO;
using System.Net;
using System.Text;

namespace lesson49
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            string currentDir = Directory.GetCurrentDirectory();
            string site = currentDir + @"\site";
            DumbHttpServer server = new DumbHttpServer(site,8888);

        }
    }
}
