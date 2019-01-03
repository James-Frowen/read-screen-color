using read_screen_color;
using read_screen_color_debug_window;
using System;
using System.Collections.Generic;
using System.Threading;

namespace read_screen_color_console
{
    internal class Program
    {
        private static MovingAverage movingAverage;
        private static void Main(string[] args)
        {
            movingAverage = new MovingAverage((int)ReadScreenColor.PerSecond);

            ReadScreenColor.onUpdate += readScreenColor_onUpdate;

            new Thread(() => new Form1().ShowDialog()).Start();

            Console.ReadLine();
        }

        private static void readScreenColor_onUpdate(long ElapsedMilliseconds)
        {
            movingAverage.NextSample((int)ElapsedMilliseconds);
            Console.WriteLine(movingAverage.Average);
        }
    }

    public class MovingAverage
    {
        private Queue<int> samples = new Queue<int>();
        private int queueSize;

        private int sum;

        public int Average
        {
            get { return this.samples.Count == 0 ? 0 : this.sum / this.samples.Count; }
        }

        public MovingAverage(int queueCount)
        {
            this.queueSize = queueCount;
        }

        public void NextSample(int newSample)
        {
            this.sum += newSample;
            this.samples.Enqueue(newSample);

            if (this.samples.Count > this.queueSize)
            {
                this.sum -= this.samples.Dequeue();
            }
        }
    }

}
