using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

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

        //Bitwise compare:
        public void compare()
        {
            uint temp = 0;
            for (int y = 0; y < bounds.Height; y++)
            {
                for (int x = 0; x < bounds.Right - bounds.Left; x++)
                {
                    //Check whether the two values are different:
                    temp = (uint)(image1.GetPixel(x, y).ToArgb() & ~image2.GetPixel(x, y).ToArgb());

                    //For any value of temp greater than 0, this will be 1:
                    numODiffs += (uint)((temp / ((temp + 1) * 1.0)) + (1.0 / 2.0));
                }
            }
        }
    }


    class CompareImages
    {
        //Compares the two images given. Splits the computation among the specified number of threads:
        public static bool driver(Bitmap image1, Bitmap image2, int numOfThreads, int tolerance)
        {
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

            Console.WriteLine("Done with compare");

            //image1.Dispose();
            //image2.Dispose();

            if (((total / (bounds.Height * bounds.Width * 1.0)) * 100) < tolerance)
                return true;
            else
                return false;
        }

        //Assign work to different threads:
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

        //Wait for the threads to finish:
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
