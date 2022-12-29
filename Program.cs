using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NitroGenerator
{
    class Program
    {
        static int _THREADS;
        static int generation;
        static int banned;
        static int nitro_codes;
        static DateTime start_time;
        static string proxyString = @"";
        static List<string> proxies = new List<string>(proxyString.Split('\n'));


        static void Main(string[] args)
        {
            Console.Title = "Firedragon's Nitro Generator";
            Console.WriteLine("Nitro Codes will be saved in Documents Folder -- nitro_codes.txt");
            Console.Write("How many threads should be used? ");
            _THREADS = int.Parse(Console.ReadLine());
            Environment.SetEnvironmentVariable("_THREADS", "0");
            generation = 0;
            banned = 0;
            nitro_codes = 0;
            start_time = DateTime.Now;

            for (int i = 0; i < _THREADS; i++)
            {
                Thread t = new Thread(new ThreadStart(masterThread));
                t.Start();
                int thr = int.Parse(Environment.GetEnvironmentVariable("_THREADS"));
                Environment.SetEnvironmentVariable("_THREADS", (thr + 1).ToString());
            }

            Thread m = new Thread(new ThreadStart(monitorThread));
            m.Start();
        }

        static string codeGenerator()
        {
            int codeLen = 16;
            string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            string code = "";
            Random rnd = new Random();
            for (int i = 0; i < codeLen; i++)
            {
                code += letters[rnd.Next(letters.Length)];
            }
            return code;
        }

        static string getProxy()
        {
            Random rnd = new Random();
            return proxies[rnd.Next(proxies.Count)];
        }

        static void banProxy(string pxy)
        {
            if (proxies.Contains(pxy))
            {
                proxies.Remove(pxy);
                banned++;
            }
        }

        static void saveCode(string code)
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filePath = Path.Combine(documentsPath, "nitro_codes.txt");

            File.AppendAllText(filePath, code + "\n");
            nitro_codes++;
        }

        static string getRuntime()
        {
            TimeSpan elapsed_time = DateTime.Now - start_time;
            return elapsed_time.ToString(@"hh\:mm\:ss");
        }

        static void masterThread()
        {
            Random rnd = new Random();
            string raw_proxy = "";
            while (true)
            {
                try
                {
                    string current_code = codeGenerator();
                    string url = "https://discordapp.com/api/v9/entitlements/gift-codes/" + current_code + "?with_application=false&with_subscription_plan=true";
                    raw_proxy = getProxy();
                    WebProxy proxy = new WebProxy(raw_proxy);
                    using (WebClient client = new WebClient())
                    {
                        client.Proxy = proxy;
                        string response = client.DownloadString(url);
                        if (response.Contains("subscription_plan"))
                        {
                            saveCode(current_code);
                        }
                        else if (response.Contains("Access denied"))
                        {
                            banProxy(raw_proxy);
                        }
                        else
                        {
                            generation++;
                            Thread.Sleep(rnd.Next(1, 10));
                        }
                    }
                }
                catch (WebException)
                {
                    banProxy(raw_proxy);
                }
            }
        }

        static void monitorThread()
        {
            while (true)
            {
                try
                {
                    Console.Clear();
                    Console.WriteLine(" <---------> GENERATOR MONITOR <--------->\n");
                    Console.WriteLine(" Gen/s: {0} | Active Threads: {1}", generation, Environment.GetEnvironmentVariable("_THREADS"));
                    Console.WriteLine(" Bad Proxies: {0} | Active Proxies: {1}", banned, proxies.Count);
                    Console.WriteLine(" Nitro Codes Generated: {0} | Runtime: {1}", nitro_codes, getRuntime());
                    Console.WriteLine("Nitro Codes will be saved in Documents Folder -- nitro_codes.txt");
                    Console.WriteLine();
                    generation = 0;
                    Thread.Sleep(1000);
                }
                catch (Exception)
                {
                    // do nothing
                }
            }
        }
    }
}
