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
        public Byte[] image1;
        public Byte[] image2;
        public int[] ignRegions;
        public uint numODiffs = 0;

        public Differ() { }

        public Differ(Byte[] tempImage1, Byte[] tempImage2, int[] subregion)
        {
            image1 = tempImage1;
            image2 = tempImage2;
            ignRegions = subregion;
        }


        //Bitwise compare:
        public void compare()
        {
            uint temp = 0;

            for (int i = 0; i < image1.Length; i++)
            {
                //Check whether the two values are different:
                temp = (uint)(image1[i] & ~image2[i]);

                //For any value of temp greater than 0, this will be 1.
                //Multiply ignRegions array to specify which regions to ignore:
                numODiffs += (uint)(((temp / (float)(temp + 1)) + (1.0 / 2.0)) * ignRegions[i/4]);
            }
        }
    }


    class CompareImages
    {
        static List<int[]> subRegions = new List<int[]>();
        readonly static int threads = Program.data.getThreads();


        //Compares the two images given. Splits the computation among the specified number of threads:
        public static bool driver(Bitmap bImage1, Bitmap bImage2)
        {
            GraphicsUnit pixel = GraphicsUnit.Pixel;
            Rectangle bounds = Rectangle.Round(bImage1.GetBounds(ref pixel));

            Byte[] image1 = ConvertBitmap.getBytes(bImage1);
            Byte[] image2 = ConvertBitmap.getBytes(bImage2);
            Thread[] compThreads = new Thread[threads];
            Differ[] diffs = new Differ[threads];

            if (subRegions.Count == 0)
                getIgnBlocks();

            //Split computation into several threads
            allocThreads(image1, image2, compThreads, diffs);

            //Wait till computation is finished:
            waitForFinish(compThreads);

            uint total = 0;
            for (int i = 0; i < threads; i++)
            {
                total += diffs[i].numODiffs;
            }

            //Calculate % difference:
            Console.WriteLine("{0}% different", (total / ((float)bounds.Height * bounds.Width * 4)) * 100);
            Program.data.rF.WriteLine("{0}% different", (total / ((float)bounds.Height * bounds.Width * 4)) * 100);

            if (((total / ((float)bounds.Height * bounds.Width * 4)) * 100) < Program.data.cfg.tol)
                return true;
            else
                return false;
        }


        //Assign work to different threads:
        private static void allocThreads(Byte[] image1, Byte[] image2, Thread[] compThreads, Differ[] diff)
        {
            List<Byte[]> img1Split = new List<byte[]>();
            List<Byte[]> img2Split = new List<byte[]>(); 

            //Divide image byte arrays by number of threads:
            byte[] temp;
            int offset = 0;
            int endpoint = 0;
            for (int i = 0; i < threads; i++)
            {
                offset = (int)(image1.Length * (i / (float)threads));
                endpoint = (int)(image1.Length * ((i + 1) / (float)threads));

                temp = new Byte[endpoint - offset];
                Buffer.BlockCopy(image1, offset * sizeof(byte), temp, 0, (endpoint - offset) * sizeof(byte));
                img1Split.Add(temp);

                temp = new Byte[endpoint - offset];
                Buffer.BlockCopy(image2, offset * sizeof(byte), temp, 0, (endpoint - offset) * sizeof(byte));
                img2Split.Add(temp);
            }

            //Start the computation on new threads:
            for (int i = 0; i < threads; i++)
            {
                diff[i] = new Differ(img1Split[i], img2Split[i], subRegions[i]);
                compThreads[i] = new Thread(new ThreadStart(diff[i].compare));
                compThreads[i].Start();

                //Wait until thread is initialize:
                while (!compThreads[i].IsAlive) ;
            }
        }


        //Wait for the threads to finish:
        private static void waitForFinish(Thread[] compThreads)
        {
            bool someAlive = true;
            while (someAlive)
            {
                someAlive = false;

                for (int i = 0; i < threads; i++)
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


        //Ignore regions specified in config file:
        private static void getIgnBlocks()
        {
            Rectangle bounds = Screen.GetBounds(Point.Empty);

            //Make ones array:
            int[] regions = new int[bounds.Height * bounds.Width];
            for (int i = 0; i < regions.Length; i++)
                regions[i] = 1;

            //Mark rectangles in regions array:
            foreach (int[] rect in Program.data.cfg.regions)
            {
                for (int i = rect[1]; i < rect[3]; i++)
                {
                    for (int j = rect[0]; j < rect[2]; j++)
                    {
                        regions[i*(bounds.Width) + j] = 0;
                    }
                }
            }

            //Divide array by number of threads:
            int offset = 0;
            int endpoint = 0;
            int[] tempArray;
            for (int i = 0; i < threads; i++)
            {
                offset = (int)(bounds.Height * bounds.Width * (i / ((float)threads)));
                endpoint = (int)(bounds.Height * bounds.Width * ((i+1) / ((float)threads)));

                tempArray = new int[endpoint - offset];
                Buffer.BlockCopy(regions, offset * sizeof(int), tempArray, 0, (endpoint - offset) * sizeof(int));
                subRegions.Add(tempArray);
            }
        }
    }
}
