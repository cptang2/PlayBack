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
        static config configList;

        //Compares the two images given. Splits the computation among the specified number of threads:
        public static bool driver(Bitmap tempImage1, Bitmap tempImage2, int numOfThreads, float tolerance, config tempConfigList, StreamWriter resultsFile)
        {
            configList = tempConfigList;

            Rectangle bounds = RFtoR(tempImage1);
            Byte[] image1 = ConvertBitmap.getBytes(tempImage1);
            Byte[] image2 = ConvertBitmap.getBytes(tempImage2);
            Thread[] compThreads = new Thread[numOfThreads];
            Differ[] diff = new Differ[numOfThreads];

            if (subRegions.Count == 0)
                getIgnBlocks(numOfThreads);

            //Split computation into several threads
            allocThreads(numOfThreads, image1, image2, compThreads, diff);

            //Wait till computation is finished:
            waitForFinish(numOfThreads, compThreads);

            uint total = 0;
            for (int i = 0; i < numOfThreads; i++)
            {
                total += diff[i].numODiffs;
            }

            //Calculate % difference:
            Console.WriteLine("{0}% different", (total / ((float)bounds.Height * bounds.Width * 4)) * 100);
            resultsFile.WriteLine("{0}% different", (total / ((float)bounds.Height * bounds.Width * 4)) * 100);

            if (((total / ((float)bounds.Height * bounds.Width * 4)) * 100) < tolerance)
                return true;
            else
                return false;
        }


        //Assign work to different threads:
        private static void allocThreads(int numOfThreads, Byte[] image1, Byte[] image2, Thread[] compThreads, Differ[] diff)
        {
            //tempBounds.Height = (int)(bounds.Height / ((float)numOfThreads));
            List<Byte[]> image1Split = new List<byte[]>();
            List<Byte[]> image2Split = new List<byte[]>(); 

            //Divide image byte arrays by number of threads:
            byte[] temp;
            int offset = 0;
            int endpoint = 0;
            for (int i = 0; i < numOfThreads; i++)
            {
                offset = (int)(image1.Length * (i / (float)numOfThreads));
                endpoint = (int)(image1.Length * ((i + 1) / (float)numOfThreads));

                temp = new Byte[endpoint - offset];
                Buffer.BlockCopy(image1, offset * sizeof(byte), temp, 0, (endpoint - offset) * sizeof(byte));
                image1Split.Add(temp);

                temp = new Byte[endpoint - offset];
                Buffer.BlockCopy(image2, offset * sizeof(byte), temp, 0, (endpoint - offset) * sizeof(byte));
                image2Split.Add(temp);
            }


            for (int i = 0; i < numOfThreads; i++)
            {
                //tempBounds.Y = bounds.Y + (int)((i / ((float)numOfThreads)) * bounds.Height);

                diff[i] = new Differ(image1Split[i], image2Split[i], subRegions[i]);
                compThreads[i] = new Thread(new ThreadStart(diff[i].compare));
                compThreads[i].Start();

                while (!compThreads[i].IsAlive) ;
            }
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

        //Ignore regions specified in config file:
        private static void getIgnBlocks(int numOfThreads)
        {
            Rectangle bounds = Screen.GetBounds(Point.Empty);

            //Make ones array:
            int[] regions = new int[bounds.Height * bounds.Width];
            for (int i = 0; i < regions.Length; i++)
                regions[i] = 1;

            //Mark rectangles in regions array:
            foreach (int[] rect in configList.regions)
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
            for (int i = 0; i < numOfThreads; i++)
            {
                offset = (int)(bounds.Height * bounds.Width * (i / ((float)numOfThreads)));
                endpoint = (int)(bounds.Height * bounds.Width * ((i+1) / ((float)numOfThreads)));

                tempArray = new int[endpoint - offset];
                Buffer.BlockCopy(regions, offset * sizeof(int), tempArray, 0, (endpoint - offset) * sizeof(int));
                subRegions.Add(tempArray);
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
