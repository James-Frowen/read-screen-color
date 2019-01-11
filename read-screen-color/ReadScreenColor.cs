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
        public static int PartSize = 100;
        public static Size AverageSize = new Size(16, 9);
        //private Bitmap[,] screenPixels;
        private Bitmap screen = new Bitmap(ScreenSize.Width, ScreenSize.Height, PixelFormat.Format32bppArgb);
        private Bitmap screenTop = new Bitmap(ScreenSize.Width, PartSize, PixelFormat.Format32bppArgb);
        private Bitmap screenBot = new Bitmap(ScreenSize.Width, PartSize, PixelFormat.Format32bppArgb);
        private Bitmap screenLeft = new Bitmap(PartSize, ScreenSize.Height - (PartSize * 2), PixelFormat.Format32bppArgb);
        private Bitmap screenRight = new Bitmap(PartSize, ScreenSize.Height - (PartSize * 2), PixelFormat.Format32bppArgb);
        private Bitmap average = new Bitmap(AverageSize.Width, AverageSize.Height, PixelFormat.Format32bppArgb);

        public Bitmap Result
        {
            get { return this.average; }
        }

        public ReadScreenColor(/*Size grid*/)
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
            this.ReadScreen();

            using (Graphics gavg = Graphics.FromImage(this.average))
            {
                gavg.DrawImage(this.screen, 0, 0, AverageSize.Width, this.average.Height);
            }
        }

        public void ReadScreen()
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
        }
        public void CopyFromScreen()
        {
            using (Graphics gsrc = Graphics.FromImage(this.screen))
            {
                gsrc.CopyFromScreen(0, 0, 0, 0, ScreenSize, CopyPixelOperation.SourceCopy);
            }
        }
        public void CopyFromScreenParts()
        {
            using (Graphics gsrc = Graphics.FromImage(this.screen))
            {
                copyPart(gsrc, 0, 0, this.screen.Width, PartSize);
                copyPart(gsrc, 0, this.screen.Height - PartSize, this.screen.Width, PartSize);
                copyPart(gsrc, 0, PartSize, PartSize, this.screen.Height);
                copyPart(gsrc, this.screen.Width - PartSize, PartSize, PartSize, this.screen.Height);
            }
        }

        private static void copyPart(Graphics gsrc, int sourceX, int sourceY, int targetWidth, int targetHeight)
        {
            gsrc.CopyFromScreen(sourceX, sourceY, sourceX, sourceY, new Size(targetWidth, targetHeight));
        }
        public void ReadScreenParts()
        {
            using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero))
            {
                IntPtr hSrcDC = gsrc.GetHdc();

                readScreenPart(hSrcDC, this.screenTop, 0, 0, this.screen.Width, PartSize);
                readScreenPart(hSrcDC, this.screenBot, 0, this.screen.Height - PartSize, this.screen.Width, PartSize);
                readScreenPart(hSrcDC, this.screenLeft, 0, PartSize, PartSize, this.screen.Height);
                readScreenPart(hSrcDC, this.screenRight, this.screen.Width - PartSize, PartSize, PartSize, this.screen.Height);

                gsrc.ReleaseHdc();
            }
        }

        private static void readScreenPart(IntPtr hSrcDC, Bitmap target, int sourceX, int sourceY, int targetWidth, int targetHeight)
        {
            using (Graphics gdest = Graphics.FromImage(target))
            {
                IntPtr hDC = gdest.GetHdc();
                int retval = BitBlt(
                    hDC, 0, 0, targetWidth, targetHeight,
                    hSrcDC, sourceX, sourceY, (int)CopyPixelOperation.SourceCopy);

                gdest.ReleaseHdc();
            }
        }
        public void ReadScreenPartsv2()
        {
            using (Graphics gdest = Graphics.FromImage(this.screen))
            {
                using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero))
                {
                    IntPtr hSrcDC = gsrc.GetHdc();
                    IntPtr hDC = gdest.GetHdc();

                    blitPart(hSrcDC, hDC, 0, 0, this.screen.Width, PartSize);
                    blitPart(hSrcDC, hDC, 0, this.screen.Height - PartSize, this.screen.Width, PartSize);
                    blitPart(hSrcDC, hDC, 0, PartSize, PartSize, this.screen.Height);
                    blitPart(hSrcDC, hDC, this.screen.Width - PartSize, PartSize, PartSize, this.screen.Height);
                    gdest.ReleaseHdc();
                    gsrc.ReleaseHdc();
                }
            }
        }

        private static void blitPart(IntPtr hSrcDC, IntPtr hDC, int sourceX, int sourceY, int targetWidth, int targetHeight)
        {
            int retval = BitBlt(
                                    hDC, sourceX, sourceY, targetWidth, targetHeight,
                                    hSrcDC, sourceX, sourceY, (int)CopyPixelOperation.SourceCopy);
        }
    }
    public struct UpdateResult
    {
        public long elapsedMilliseconds;
    }
}
