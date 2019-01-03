using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace read_screen_color
{
    public class ReadScreenColor
    {
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

        public delegate void OnUpdate(long ElapsedMilliseconds);
        public static event OnUpdate onUpdate;

        public static float PerSecond = 5f;

        private Stopwatch stopwatch;
        public static Size ScreenSize = new Size(1920, 1080);
        public static Size AverageSize = new Size(16, 9);
        //private Bitmap[,] screenPixels;
        private Bitmap screen = new Bitmap(ScreenSize.Width, ScreenSize.Height, PixelFormat.Format32bppArgb);
        private Bitmap average = new Bitmap(AverageSize.Width, AverageSize.Height, PixelFormat.Format32bppArgb);

        public Bitmap Result
        {
            get { return this.average; }
        }

        public ReadScreenColor(Size grid)
        {
            //TODO make sure not losing pixel on sides

            //this.screenPixels = new Bitmap[grid.Width, grid.Height];

            //var width = screenSize.Width / grid.Width;
            //var height = screenSize.Height / grid.Height;

            //for (int i = 0; i < grid.Width; i++)
            //{
            //    for (int j = 0; j < grid.Height; j++)
            //    {
            //        this.screenPixels[i, j] = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            //    }
            //}
        }

        public UpdateResult Update(bool withStopWatch = false)
        {
            var result = new UpdateResult();
            if (withStopWatch)
            {
                this.stopwatch = new Stopwatch();
                this.stopwatch.Start();
            }

            this.checkScreen();

            if (withStopWatch)
            {
                this.stopwatch.Stop();
                result.elapsedMilliseconds = this.stopwatch.ElapsedMilliseconds;
                Debug.WriteLine("UpdateTime:" + result.elapsedMilliseconds);
                onUpdate?.Invoke(result.elapsedMilliseconds);
            }

            return result;
        }

        private void checkScreen()
        {
            using (Graphics gdest = Graphics.FromImage(this.screen))
            {
                using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero))
                {
                    IntPtr hSrcDC = gsrc.GetHdc();
                    IntPtr hDC = gdest.GetHdc();
                    int retval = BitBlt(
                        hDC, 0, 0, ScreenSize.Width, ScreenSize.Height,
                        hSrcDC, 0, 0, (int)CopyPixelOperation.SourceCopy);
                    gdest.ReleaseHdc();
                    gsrc.ReleaseHdc();
                }
            }

            using (Graphics gavg = Graphics.FromImage(this.average))
            {
                gavg.DrawImage(this.screen, 0, 0, AverageSize.Width, this.average.Height);
            }
        }
    }
    public struct UpdateResult
    {
        public long elapsedMilliseconds;
    }
}
