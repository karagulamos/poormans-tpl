using PoormansTPL.Threading.Tasks;
using System;
using System.Diagnostics;
using System.Net;

namespace PoormansTPL
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Performing long running operations...");

            var stopWatch = new Stopwatch();

            var downloadTasks = new[]
            {
                new PoormansTask<Tuple<string, string>>(() =>
                {
                    Console.WriteLine("Downloading Microsoft.com");
                    var result = DownloadSite("http://microsoft.com");
                    Console.WriteLine("Microsoft.com - should not display confirmation if cancelled");
                    return new Tuple<string, string>("Microsoft.com", result);
                }),
                new PoormansTask<Tuple<string, string>>(() =>
                {
                    Console.WriteLine("Downloading Facebook.com");
                    var result = DownloadSite("http://facebook.com");
                    Console.WriteLine("Facebook.com - should not display confirmation if cancelled");
                    return new Tuple<string, string>("Facebook.com", result);
                }),
                new PoormansTask<Tuple<string, string>>(() =>
                {
                    Console.WriteLine("Downloading Goal.com");
                    var result = DownloadSite("http://goal.com");
                    Console.WriteLine("Goal.com - should not display confirmation if cancelled");
                    return new Tuple<string, string>("Goal.com", result);
                } ),
                new PoormansTask<Tuple<string, string>>(() =>
                {
                    Console.WriteLine("Downloading Yahoo.com");
                    var result = DownloadSite("http://yahoo.com");
                    Console.WriteLine("Yahoo.com - should not display confirmation if cancelled");
                    return new Tuple<string, string>("Yahoo.com", result);
                } ),
                new PoormansTask<Tuple<string, string>>(() =>
                {
                    Console.WriteLine("Downloading Google.com");
                    var result = DownloadSite("http://google.com");
                    Console.WriteLine("Google.com - should not display confirmation if cancelled");
                    return new Tuple<string, string>("Google.com", result);
                } )
            };

            stopWatch.Start();

            foreach (var task in downloadTasks)
            {
                task.Start();
            }

            var taskId = PoormansTask.WaitAny(downloadTasks, true); // Observe behaviour when flag to cancel unfinished tasks is removed

            stopWatch.Stop();

            Console.WriteLine("{0} [{1}] finished first in {2} seconds", downloadTasks[taskId].Result.Item1, taskId, stopWatch.Elapsed.TotalSeconds);

            Console.WriteLine("Press any key to exit...");
            
            // RunCustomThreadPoolExample();

            Console.ReadKey(true);
        }

        public static string DownloadSite(string websiteAddress)
        {
            var client = new WebClient();
            return client.DownloadString(new Uri(websiteAddress));
        }

        private static void RunCustomThreadPoolExample()
        {
            var threadPool = new PoormansThreadPool();
            
            for (long id = 1; id <= 10; id++)
            {
                var snapshotId = id;
                threadPool.EnqueueTask(() =>
                {
                    Console.WriteLine($"Downloading Google.com {snapshotId}");
                    DownloadSite("http://google.com");
                    Console.WriteLine($"Google.com {snapshotId} done downloading.");
                });
            }

            Console.ReadLine();
        }
    }
}
