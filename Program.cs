using PoormansTPL.Threading.Tasks;
using System;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PoormansTPL
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            RunTplUsingPoormansThreadPoolScheduler();

            //RunAwaitablePoormansTaskExample();

            //RunPoormansTaskExample();

            //RunCustomThreadPoolExample();

            Console.ReadKey(true);
        }

        private static void RunTplUsingPoormansThreadPoolScheduler()
        {
            var stopWatch = new Stopwatch();

            var downloadTasks = new[]
            {
                new Task<Tuple<string, string>>(() =>
                {
                    Console.WriteLine("Downloading Microsoft.com");
                    var result = DownloadSite("http://microsoft.com");
                    Console.WriteLine("Microsoft.com - should not display confirmation if cancelled");
                    return new Tuple<string, string>("Microsoft.com", result);
                }),
                new Task<Tuple<string, string>>(() =>
                {
                    Console.WriteLine("Downloading Facebook.com");
                    var result = DownloadSite("http://facebook.com");
                    Console.WriteLine("Facebook.com - should not display confirmation if cancelled");
                    return new Tuple<string, string>("Facebook.com", result);
                }),
                new Task<Tuple<string, string>>(() =>
                {
                    Console.WriteLine("Downloading Goal.com");
                    var result = DownloadSite("http://goal.com");
                    Console.WriteLine("Goal.com - should not display confirmation if cancelled");
                    return new Tuple<string, string>("Goal.com", result);
                }),
                new Task<Tuple<string, string>>(() =>
                {
                    Console.WriteLine("Downloading Yahoo.com");
                    var result = DownloadSite("http://yahoo.com");
                    Console.WriteLine("Yahoo.com - should not display confirmation if cancelled");
                    return new Tuple<string, string>("Yahoo.com", result);
                }),
                new Task<Tuple<string, string>>(() =>
                {
                    Console.WriteLine("Downloading Google.com");
                    var result = DownloadSite("http://google.com");
                    Console.WriteLine("Google.com - should not display confirmation if cancelled");
                    return new Tuple<string, string>("Google.com", result);
                })
            };

            stopWatch.Start();

            var scheduler = new PoormansThreadPoolTaskScheduler();

            foreach (var task in downloadTasks)
            {
                task.Start(scheduler);
            }

            var taskId = Task.WaitAny(downloadTasks);
            // Observe behaviour when flag to cancel unfinished tasks is removed

            stopWatch.Stop();

            Console.WriteLine("{0} [{1}] finished first in {2} seconds", downloadTasks[taskId].Result.Item1, taskId,
                stopWatch.Elapsed.TotalSeconds);

            Console.WriteLine("Press any key to exit...");
        }

        private static async void RunAwaitablePoormansTaskExample()
        {
            await PoormansTask.Run(RunCustomThreadPoolExample);
        }

        private static void RunPoormansTaskExample()
        {
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
                }),
                new PoormansTask<Tuple<string, string>>(() =>
                {
                    Console.WriteLine("Downloading Yahoo.com");
                    var result = DownloadSite("http://yahoo.com");
                    Console.WriteLine("Yahoo.com - should not display confirmation if cancelled");
                    return new Tuple<string, string>("Yahoo.com", result);
                }),
                new PoormansTask<Tuple<string, string>>(() =>
                {
                    Console.WriteLine("Downloading Google.com");
                    var result = DownloadSite("http://google.com");
                    Console.WriteLine("Google.com - should not display confirmation if cancelled");
                    return new Tuple<string, string>("Google.com", result);
                })
            };

            stopWatch.Start();

            foreach (var task in downloadTasks)
            {
                task.Start();
            }

            var taskId = PoormansTask.WaitAny(downloadTasks, true);
            // Observe behaviour when flag to cancel unfinished tasks is removed

            stopWatch.Stop();

            Console.WriteLine("{0} [{1}] finished first in {2} seconds", downloadTasks[taskId].Result.Item1, taskId,
                stopWatch.Elapsed.TotalSeconds);

            Console.WriteLine("Press any key to exit...");
        }

        private static void RunCustomThreadPoolExample()
        {
            var threadPool = new PoormansThreadPool();

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            const int numberOfRequests = 50;

            var titlePattern = new Regex("<(title)>(.*)</\\1>");

            for (long id = 1; id <= numberOfRequests; id++)
            {
                var snapshotId = id;
                threadPool.EnqueueTask(() =>
                {
                    Console.WriteLine($"Downloading Google.com {snapshotId}");
                    var title = titlePattern.Match(DownloadSite("http://www.google.com.ng")).Groups[2];
                    Console.WriteLine($"{title} {snapshotId} done downloading.");
                });
            }

            for (long id = 1; id <= numberOfRequests; id++)
            {
                var snapshotId = id;
                threadPool.EnqueueTask(() =>
                {
                    Console.WriteLine($"Downloading Goal.com {snapshotId}");
                    var title = titlePattern.Match(DownloadSite("http://goal.com")).Groups[2];
                    Console.WriteLine($"{title} {snapshotId} done downloading.");
                });
            }

            threadPool.Shutdown();

            Console.WriteLine($"\nCompleted downloads in {stopWatch.ElapsedMilliseconds / 1000.00} seconds.");
        }

        public static string DownloadSite(string websiteAddress)
        {
            var client = new WebClient();
            return client.DownloadString(new Uri(websiteAddress));
        }
    }
}
