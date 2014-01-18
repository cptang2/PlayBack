using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Drawing;

namespace PlayBack
{
    class Differ
    {
        public Bitmap image1;
        public Bitmap image2;
        public Rectangle bounds;
        public uint numODiffs = 0;

        public Differ() { }

        public Differ(Bitmap tempImage1, Bitmap tempImage2, Rectangle tempBounds)
        {
            image1 = tempImage1;
            image2 = tempImage2;
            bounds = tempBounds;
        }

        public void compare()
        {
            uint temp = 0;
            for (int y = 0; y < bounds.Height; y++)
            {
                for (int x = 0; x < bounds.Right - bounds.Left; x++)
                {
                    temp = (uint)(image1.GetPixel(x, y).ToArgb() & ~image2.GetPixel(x, y).ToArgb());
                    numODiffs += (uint)((temp / ((temp + 1) * 1.0)) + (1.0 / 3.0));
                }
            }
        }
    }


    class CompareImages
    {
        public static void driver(string tempImage1, string tempImage2, int numOfThreads)
        {
            int now = DateTime.Now.Millisecond;
            Console.WriteLine("{0},{1}", DateTime.Now.Second, DateTime.Now.Millisecond);

            Bitmap image1 = new Bitmap(tempImage1);
            Bitmap image2 = new Bitmap(tempImage2);

            System.Drawing.Imaging.PixelFormat format = image1.PixelFormat;

            Thread[] compThreads = new Thread[numOfThreads];
            Differ[] diff = new Differ[numOfThreads];

            //Split computation into several threads
            Rectangle bounds = allocThreads(numOfThreads, image1, image2, compThreads, diff);

            //Wait till computation is finished:
            waitForFinish(numOfThreads, compThreads);

            uint total = 0;
            for (int i = 0; i < numOfThreads; i++)
            {
                total += diff[i].numODiffs;
            }

            Console.WriteLine("{0}% different", (total / (bounds.Height * bounds.Width * 1.0)) * 100);

            Console.WriteLine("{0},{1}", DateTime.Now.Second, DateTime.Now.Millisecond);

            Console.WriteLine("Done with compare");

            Console.ReadKey();
        }


        private static Rectangle allocThreads(int numOfThreads, Bitmap image1, Bitmap image2, Thread[] compThreads, Differ[] diff)
        {
            System.Drawing.Imaging.PixelFormat format = image1.PixelFormat;
            Rectangle bounds = RFtoR(image1);
            Rectangle tempBounds = bounds;
            tempBounds.Height = (int)(bounds.Height / (numOfThreads * 1.0));
            for (int i = 0; i < numOfThreads; i++)
            {

                tempBounds.Y = bounds.Y + (int)((i / (numOfThreads * 1.0)) * bounds.Height);

                diff[i] = new Differ(image1.Clone(tempBounds, format), image2.Clone(tempBounds, format), tempBounds);
                compThreads[i] = new Thread(new ThreadStart(diff[i].compare));
                compThreads[i].Start();

                while (!compThreads[i].IsAlive) ;
                Console.WriteLine(i / (numOfThreads * 1.0));
            }
            return bounds;
        }


        private static void waitForFinish(int numOfThreads, Thread[] compThreads)
        {
            bool someAlive = true;
            while (someAlive)
            {
                someAlive = false;

                for (int i = 0; i < numOfThreads; i++)
                {
                    if (compThreads[i].IsAlive)
                    {
                        someAlive = true;
                    }
                }

                if (!someAlive)
                    break;
            }
        }

        static Rectangle RFtoR(Bitmap image1)
        {
            RectangleF fBounds;
            GraphicsUnit pixel = GraphicsUnit.Pixel;
            fBounds = image1.GetBounds(ref pixel);

            Rectangle bounds = new Rectangle();
            bounds.X = (int)fBounds.X;
            bounds.Y = (int)fBounds.Y;
            bounds.Width = (int)fBounds.Width;
            bounds.Height = (int)fBounds.Height;

            return bounds;
        }
    }
    
}
