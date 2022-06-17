using Multimedia_ITE_HW_1.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Multimedia_ITE_HW_1
{
    public partial class Form1 : Form
    {

        Graphics graphics;

        int x = -1;
        int y = -1;

        bool isMove = false;

        Pen pen;
        List<CustomPoint> points;

        Stack<Bitmap> undoStack;
        Stack<Bitmap> redoStack;


        public Form1()
        {

            //KeyValuePair<int, int> pair = new KeyValuePair<int, int>();
            InitializeComponent();

            graphics = pic.CreateGraphics();
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            pen = new Pen(Color.Black, 5);
            pen.StartCap = pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;

            points = new List<CustomPoint>();

            undoStack = new Stack<Bitmap>();
            redoStack = new Stack<Bitmap>();

        }

        private void pic_MouseDown(object sender, MouseEventArgs e)
        {
            if (pic.Image == null)
            { OpenImageFromDrive(); }
            else
            {
                x = e.X;
                y = e.Y;

                isMove = true;
            }


        }

        private void pic_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMove)
            {
                points.Add(new CustomPoint(e.Location, pen.Color));

                graphics.DrawLine(pen, new Point(x, y), e.Location);
                x = e.X;
                y = e.Y;
            }

            //this.Text = "X :  " + e.X + "  Y : " + e.Y;
        }

        private void pic_MouseUp(object sender, MouseEventArgs e)
        {
            Bitmap bmp = new Bitmap(pic.Image);
            for (int i = 0; i < points.Count; i++)
            {
                Point pt = points[i].Point;
                Point sPt = scaledPoint(pic, points[i].Point);
                Color c0 = bmp.GetPixel(sPt.X, sPt.Y);
                Fill(bmp, sPt, c0, points[i].Color);

            }

            pic.Image = bmp;

            undoStack.Push(bmp);
            redoStack.Clear();

            points.Clear();

            isMove = false;
            x = -1;
            y = -1;
        }

        private void open_tsmi_Click(object sender, EventArgs e)
        {
            OpenImageFromDrive();
        }

        private void OpenImageFromDrive()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "*.JPG;*.PNG;*.GIF;*.JPEG|*.JPG;*.PNG;*.GIF;*.JPEG";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Bitmap bmp = new Bitmap(ofd.FileName);

                pic.Image = Image.FromFile(ofd.FileName);
                pic.Width = bmp.Width;
                pic.Height = bmp.Height;
                points.Clear();
                undoStack.Clear();
                undoStack.Push(bmp);
            }
        }

        private void grayScale_tsmi_Click(object sender, EventArgs e)
        {

            if (pic.Image != null)
            {
                Bitmap pic_img = new Bitmap(pic.Image);
                Bitmap bmp = new Bitmap(pic.Image);


                BitmapData bmpData = bmp.LockBits(
                    new Rectangle(0, 0, bmp.Width, bmp.Height),
                    ImageLockMode.ReadWrite, bmp.PixelFormat);

                int bytesPerPixel = System.Drawing.Bitmap.GetPixelFormatSize(bmp.PixelFormat) / 8;
                int heightInPixel = bmpData.Height;
                int widthInBytes = bmpData.Width * bytesPerPixel;


                int byteCount = bmpData.Stride * pic_img.Height;
                byte[] pixels = new byte[byteCount];

                IntPtr ptrFirstPixel = bmpData.Scan0;
                Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);
                int heightInPixels = bmpData.Height;


                Parallel.For(0, heightInPixel, y =>
                {

                    int currentLine = y * bmpData.Stride;


                    for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {

                        int oldBlue = pixels[currentLine + x];
                        int oldGreen = pixels[currentLine + x + 1];
                        int oldRed = pixels[currentLine + x + 2];


                        int avg = (oldBlue + oldGreen + oldRed) / 3;
                        Color color = Color.FromArgb(1, avg, avg, avg);


                        pixels[currentLine + x] = (byte)avg;
                        pixels[currentLine + x + 1] = (byte)avg;
                        pixels[currentLine + x + 2] = (byte)avg;
                    }
                });

                Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);

                bmp.UnlockBits(bmpData);

                pic.Image = bmp;
                undoStack.Push(bmp);

            }

        }

        private void red_tsmi_Click(object sender, EventArgs e)
        {
            pen.Color = Color.Red;
        }

        private void green_tsmi_Click(object sender, EventArgs e)
        {
            pen.Color = Color.Green;
        }

        private void blue_tsmi_Click(object sender, EventArgs e)
        {
            pen.Color = Color.Blue;
        }

        private void cyan_tsmi_Click(object sender, EventArgs e)
        {
            pen.Color = Color.Cyan;
        }

        private void magenta_tsmi_Click(object sender, EventArgs e)
        {
            pen.Color = Color.Magenta;
        }

        private void yellow_tsmi_Click(object sender, EventArgs e)
        {
            pen.Color = Color.Yellow;
        }

        //private void reColor_tsmi_Click(object sender, EventArgs e)
        //{
        //    Bitmap bmp = new Bitmap(pic.Image);
        //    for (int i = 0; i < points.Count; i++)
        //    {
        //        Point pt = points[i].Point;
        //        Point sPt = scaledPoint(pic, points[i].Point);
        //        Color c0 = bmp.GetPixel(sPt.X, sPt.Y);
        //        Fill(bmp, sPt, c0, points[i].Color);

        //    }

        //    pic.Image = bmp;
        //}


        static void Fill(Bitmap bmp, Point pt, Color c0, Color c1)
        {
            Color cx = bmp.GetPixel(pt.X, pt.Y);
            if (cx.GetBrightness() < 0.01f) return;  // optional, to prevent filling a black grid
            Rectangle bmpRect = new Rectangle(Point.Empty, bmp.Size);
            Stack<Point> stack = new Stack<Point>();
            int x0 = pt.X;
            int y0 = pt.Y;

            stack.Push(new Point(x0, y0));
            while (stack.Any())
            {
                Point p = stack.Pop();
                if (!bmpRect.Contains(p)) continue;
                cx = bmp.GetPixel(p.X, p.Y);
                if (

                    cx.R == cx.G && cx.G == cx.B &&
                    cx.GetBrightness() - 0.02 <= c0.GetBrightness() &&
                    c0.GetBrightness() <= cx.GetBrightness() + 0.02)  //*
                {

                    int r = (int)((c1.R * 0.8) + (cx.R * 0.2));
                    int g = (int)((c1.G * 0.8) + (cx.G * 0.2));
                    int b = (int)((c1.B * 0.8) + (cx.B * 0.2));

                    Color newColor = Color.FromArgb(r, g, b);

                    bmp.SetPixel(p.X, p.Y, c1);

                    stack.Push(new Point(p.X, p.Y + 1));
                    stack.Push(new Point(p.X, p.Y - 1));
                    stack.Push(new Point(p.X + 1, p.Y));
                    stack.Push(new Point(p.X - 1, p.Y));
                }
            }
        }


        static Point scaledPoint(PictureBox pb, Point pt)
        {
            float scaleX = 1f * pb.Image.Width / pb.ClientSize.Width;
            float scaleY = 1f * pb.Image.Height / pb.ClientSize.Height;
            return new Point((int)(pt.X * scaleX), (int)(pt.Y * scaleY));
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveImageOnDrive();
        }
        private void SaveImageOnDrive()
        {
            if (pic.Image != null)
            {
                SaveFileDialog svd = new SaveFileDialog();
                svd.Filter = "*.JPG;*.PNG;*.GIF;*.JPEG|*.JPG;*.PNG;*.GIF;*.JPEG";
                if (svd.ShowDialog() == DialogResult.OK)
                {
                    Bitmap bmp = (Bitmap)pic.Image;
                    bmp.Save(svd.FileName);
                }
            }

        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (undoStack.Count > 0)
            {
                redoStack.Push(undoStack.Pop());
            }
            if (undoStack.Count > 0)
            {
                pic.Image = undoStack.Peek();
            }

        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (redoStack.Count > 0)
            {
                undoStack.Push(redoStack.Pop());
            }
            if (undoStack.Count > 0)
            {
                pic.Image = undoStack.Peek();
            }
        }

        private void colorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog clrDialog = new ColorDialog();

            if (clrDialog.ShowDialog() == DialogResult.OK)
            {
                pen.Color = clrDialog.Color;
            }
        }
    }
}
