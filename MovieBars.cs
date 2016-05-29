using AForge.Video.FFMPEG;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;


namespace MovieBars {

	public partial class Window : Form {

		Bitmap bars;
		int currentX;
		int frameCount;
		int barsHeight = 500;
		int frameDrops = 23;


		public Window () {
			InitializeComponent();

			ProcessVideo("test.avi");
			bars.Save("out.png");

			richTextBox.Text = "Processing complete.";
		}

		private void ProcessVideo (string file) {
			VideoFileReader reader = new VideoFileReader();
			reader.Open(file);
			frameCount = (int) reader.FrameCount;
			bars = new Bitmap(frameCount / (frameDrops + 1), barsHeight);

			for (int i = 0; i < frameCount / (frameDrops + 1); ++i) {
				for (int j = 0; j < frameDrops; ++j) {
					try {
						Bitmap drop = reader.ReadVideoFrame();
						drop.Dispose();
					} catch (ArgumentException e) {
						return;
					} 
				}
				Bitmap frame = reader.ReadVideoFrame();
				DrawAveragePixel(ref frame);
				frame.Dispose();
			}
        }

		private void DrawAveragePixel (ref Bitmap bm) {
			BitmapData srcData = bm.LockBits(
					new Rectangle(0, 0, bm.Width, bm.Height),
					ImageLockMode.ReadOnly,
					PixelFormat.Format32bppArgb);

			int stride = srcData.Stride;
			IntPtr Scan0 = srcData.Scan0;
			long[] totals = new long[] {0, 0, 0};
			int width = bm.Width;
			int height = bm.Height;

			unsafe {
				byte* p = (byte*) (void*) Scan0;

				for (int y = 0; y < height; ++y) {
					for (int x = 0; x < width; ++x) {
						for (int color = 0; color < 3; ++color) {
							int idx = (y * stride) + x*4 + color;
							totals[color] += p[idx];
						}
					}
				}
			}

			int avgR = (int) totals[2] / (width * height);
			int avgG = (int) totals[1] / (width * height);
			int avgB = (int) totals[0] / (width * height);

			using (Graphics graphics = Graphics.FromImage(bars)) {
				Pen pen = new Pen(Color.FromArgb(avgR, avgG, avgB));
				graphics.DrawLine(pen, currentX, 0, currentX, barsHeight);
				currentX++;
			}

			bm.UnlockBits(srcData);
		}
	}
}
