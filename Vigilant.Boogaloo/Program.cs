using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using Vigilant.Watchman.Http;

namespace Vigilant.Boogaloo {
    class Program {
        static void Main(string[] args) {
            Console.SetWindowSize(120, 120);
            var hosts = new Dictionary<string, string>() {
                { "hq-pd-web1", "10.0.0.241" },
                { "hq-pd-web2", "10.0.0.242" },
                { "hq-pd-web3", "10.0.0.101" },
                { "hq-pd-vrn1", "10.0.0.240" },
                { "hq-pd-vrn2", "10.0.0.42" },
                { "www.spotlight-dev.com", "10.0.0.44" }
            };

            var wwwRequest = "GET / HTTP/1.1\r\nHost: www.spotlight-dev.com";
            var apiRequest = "GET / HTTP/1.1\r\nHost: api.spotlight-dev.com";
            var player = new SoundPlayer("klaxon.wav");
            var httpClient = new HttpClient();
            while (true) {
                var errors = false;
                Console.WriteLine(DateTime.Now.ToString());
                foreach (var host in hosts) {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("{0} www.spotlight-dev.com ", host.Key.PadRight(24, ' '));
                    try {
                        var wwwResponse = httpClient.Retrieve(host.Value, 80, wwwRequest, false);
                        Console.ForegroundColor = ConsoleColor.Green;
                        var status = wwwResponse.Substring(0, wwwResponse.IndexOf(Environment.NewLine));
                        if (! status.StartsWith("HTTP/1.1 200 OK")) throw (new Exception(status));
                        Console.WriteLine(status);
                    } catch (Exception ex) {
                        errors = true;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(ex.Message);
                    }
                }
                foreach (var host in hosts) {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("{0} api.spotlight-dev.com ", host.Key.PadRight(24, ' '));
                    try {
                        var apiResponse = httpClient.Retrieve(host.Value, 80, apiRequest, false);
                        Console.ForegroundColor = ConsoleColor.Green;
                        var status = apiResponse.Substring(0, apiResponse.IndexOf(Environment.NewLine));
                        if (!status.StartsWith("HTTP/1.1 200 OK")) throw (new Exception(status));
                        Console.WriteLine(status);
                    } catch (Exception ex) {
                        errors = true;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(ex.Message);
                    }

                }
                if (errors) player.Play();
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }
        }
    }
}
