using read_screen_color;
using System.Drawing;
using System.Windows.Forms;

namespace read_screen_color_debug_window
{
    public partial class Form1 : Form
    {
        private ReadScreenColor readScreenColor;
        private Size imageSize = new Size(16 * 30, 9 * 30);
        public Form1()
        {
            this.InitializeComponent();
            this.readScreenColor = new ReadScreenColor(new Size(1, 1));

            this.timer1.Start();
            this.timer1.Interval = (int)(1000 / ReadScreenColor.PerSecond);
            this.ClientSize = this.imageSize;
        }

        private void timer1_Tick(object sender, System.EventArgs e)
        {
            this.readScreenColor.Update(withStopWatch: true);
            var result = this.readScreenColor.Result;

            using (var gfx = this.panel1.CreateGraphics())
            {
                gfx.DrawImage(result, 0, 0, this.imageSize.Width, this.imageSize.Height);
            }
        }
    }
}
