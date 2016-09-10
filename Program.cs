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

            var downloadTasks = new PoormansTask<Tuple<string, string>>[4]
            {
                new PoormansTask<Tuple<string, string>>(() =>
                {
                    Console.WriteLine("Downloading Microsoft.com");
                    var result = DownloadSite("http://microsoft.com");
                    Console.WriteLine("Microsoft.com - should not display if cancelled");
                    return new Tuple<string, string>("Microsoft.com", result);
                }), 
                new PoormansTask<Tuple<string, string>>(() =>
                {
                    Console.WriteLine("Downloading Facebook.com");
                    var result = DownloadSite("http://facebook.com");
                    Console.WriteLine("Facebook.com - should not display if cancelled");
                    return new Tuple<string, string>("Facebook.com", result);
                }), 
                 new PoormansTask<Tuple<string, string>>(() =>
                {
                    Console.WriteLine("Downloading Goal.com");
                    var result = DownloadSite("http://goal.com");
                    Console.WriteLine("Goal.com - should not display if cancelled");
                    return new Tuple<string, string>("Goal.com", result);
                } ), 
                 new PoormansTask<Tuple<string, string>>(() =>
                {
                    Console.WriteLine("Downloading Yahoo.com");
                    var result = DownloadSite("http://yahoo.com");
                    Console.WriteLine("Yahoo.com - should not display if cancelled");
                    return new Tuple<string, string>("Yahoo.com", result);
                } )
            };

            stopWatch.Start();

            foreach (var task in downloadTasks)
            {
                task.Start();
            }

            int finishedTaskId = PoormansTask.WaitAny(downloadTasks, true); // alter the flag to observe behavior of the tasks

            stopWatch.Stop();

            Console.WriteLine("{0} [{1}] finished first in {2} seconds", downloadTasks[finishedTaskId].Result.Item1, finishedTaskId, stopWatch.Elapsed.TotalSeconds);

            #region My Tasks
            //            var threadCount = Environment.ProcessorCount;
            //            var myTasks = new PoormansTask<int>[threadCount];
            //            var rand = new Random();


            //            for (var i = 0; i < threadCount; i++)
            //            {
            //                var currentIndex = i;
            //                var randomExecutionTime = currentIndex > threadCount - 1 >> 1 ? rand.Next(1000, 3000) : rand.Next(4000, 5000);

            //                myTasks[i] = new PoormansTask<int>(() =>
            //                {
            //                    //if (currentIndex < 2) throw new Exception("Some custom exception");
            //                    Thread.Sleep(randomExecutionTime);
            //                    Console.WriteLine("Worker " + currentIndex + " - (" + randomExecutionTime + " ms - ID " + Thread.CurrentThread.ManagedThreadId + ")");
            //                    return randomExecutionTime;
            //                });

            //                myTasks[i].Start();
            //            }


            //            Console.WriteLine("Awaiting first task to finish...");
            //            Console.WriteLine("================================");

            //            //PoormansTask<int>.WaitAll(myTasks);

            //            //try
            //            //{
            //            //    foreach (var task in myTasks)
            //            //    {
            //            //        Console.WriteLine(task.Result + " ms.");
            //            //    }
            //            //}
            //            //catch (AggregateException aex)
            //            //{
            //            //    Console.WriteLine("The following errors occured: ");

            //            //    foreach (var exception in aex.Flatten().InnerExceptions)
            //            //        Console.WriteLine(" - " + exception.Message);
            //            //}

            //            var resultIndex = PoormansTask<int>.WaitAny(myTasks);

            //            try
            //            {
            //                Console.WriteLine("Worker " + resultIndex + " finished first at " + myTasks[resultIndex].Result + " ms");
            //            }
            //            catch (AggregateException aex)
            //            {
            //                Console.WriteLine("The following errors occured: ");

            //                foreach (var exception in aex.Flatten().InnerExceptions)
            //                    Console.WriteLine(" - " + exception.Message);
            //            }

            //            for (var i = 0; i < threadCount; i++)
            //            {
            //                var currentIndex = i;
            //                var randomExecutionTime = currentIndex > threadCount - 1 >> 1 ? rand.Next(1000, 3000) : rand.Next(4000, 5000);

            //                myTasks[i] = new PoormansTask<int>(() =>
            //                {
            //                    //if (currentIndex < 2) throw new Exception("Some custom exception");
            //                    Thread.Sleep(randomExecutionTime);
            //                    Console.WriteLine("Worker " + currentIndex + " - (" + randomExecutionTime + " ms - ID " + Thread.CurrentThread.ManagedThreadId + ")");
            //                    return randomExecutionTime;
            //                });

            //                myTasks[i].Start();
            //            }

            //            resultIndex = PoormansTask<int>.WaitAny(myTasks);

            //            try
            //            {
            //                Console.WriteLine("Worker " + resultIndex + " finished first at " + myTasks[resultIndex].Result + " ms");
            //            }
            //            catch (AggregateException aex)
            //            {
            //                Console.WriteLine("The following errors occured: ");

            //                foreach (var exception in aex.Flatten().InnerExceptions)
            //                    Console.WriteLine(" - " + exception.Message);
            //            }

            #endregion

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);

        }

        public static string DownloadSite(string websiteAddress)
        {
            var client = new WebClient();

            return client.DownloadString(new Uri(websiteAddress));
        }
    }
}

