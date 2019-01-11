using read_screen_color;
using System;
using System.Diagnostics;
using System.Threading;

namespace read_screen_color_benchmarking
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            BrenchMark.Test2();
        }
    }

    public static class BrenchMark
    {
        private static double Profile(string description, int iterations, Action func)
        {
            // SOURCE : https://stackoverflow.com/a/1048708/8479976

            //Run at highest priority to minimize fluctuations caused by other processes/threads
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            // warm up 
            func();

            var watch = new Stopwatch();

            // clean up
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            watch.Start();
            for (int i = 0; i < iterations; i++)
            {
                func();
            }
            watch.Stop();
            double average = watch.Elapsed.TotalMilliseconds / iterations;
            Console.WriteLine("{0} Time Elapsed {1:0.00} ms", description, average);
            return watch.Elapsed.TotalMilliseconds;
        }
        public static void Test2()
        {
            ReadScreenColor reader = new ReadScreenColor();

            Profile("Whole", 500, reader.ReadScreen);
            Profile("Copy Whole", 500, reader.CopyFromScreen);
            //Profile("Part", 100, reader.ReadScreenParts);
            //Profile("Part v2", 100, reader.ReadScreenPartsv2);
            //Profile("Copy Parts", 100, reader.CopyFromScreenParts);

            Console.Read();
        }
        public static void Test()
        {
            Console.WriteLine(string.Format("Start"));

            ReadScreenColor reader = new ReadScreenColor();
            long wholeTotal = 0;
            long partTotal = 0;
            long part2Total = 0;
            long copyTotal = 0;
            long copyPartsTotal = 0;
            int count = 100;
            const int sleepTime = 1000 / 20;

            for (int i = 0; i < count; i++)
            {
                //
                startStopWatch();

                reader.ReadScreen();

                wholeTotal += stopStopWatch();
                sleep(sleepTime);


                //
                startStopWatch();

                reader.ReadScreenParts();

                partTotal += stopStopWatch();
                sleep(sleepTime);


                //
                startStopWatch();

                reader.ReadScreenPartsv2();

                part2Total += stopStopWatch();
                sleep(sleepTime);


                //
                startStopWatch();

                reader.CopyFromScreen();

                copyTotal += stopStopWatch();
                sleep(sleepTime);


                //
                startStopWatch();

                reader.CopyFromScreenParts();

                copyPartsTotal += stopStopWatch();
                sleep(sleepTime);
            }
            Console.WriteLine(string.Format("time scan whole screen : {0}", wholeTotal / count));
            Console.WriteLine(string.Format("time scan part screen : {0}", partTotal / count));
            Console.WriteLine(string.Format("time scan part screen : {0}", part2Total / count));
            Console.WriteLine(string.Format("time scan part screen : {0}", copyTotal / count));
            Console.WriteLine(string.Format("time scan part screen : {0}", copyPartsTotal / count));
            Console.ReadKey();
        }
        private static void startStopWatch()
        {
            sw.Reset();
            sw.Start();
        }
        private static long stopStopWatch()
        {
            sw.Stop();
            return sw.ElapsedMilliseconds;
        }
        private static void sleep(int ms)
        {
            System.Threading.Thread.Sleep(ms);
        }

        private static Stopwatch sw = new Stopwatch();
    }
}
