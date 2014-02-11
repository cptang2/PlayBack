using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace PlayBack
{
    class Config
    {
        public int start;
        public int timeout;
        public float tol;
        public bool record = false;
        public List<int> steps = new List<int>();
        public List<int[]> regions = new List<int[]>();

        public Config(string file)
        {
            read(file);
        }

        //Didn't feel like using xPath
        private void read(string file)
        {
            using (StreamReader r = new StreamReader(file))
            {
                string line;
                int[] tempCoords;
                while ((line = r.ReadLine()) != null)
                {
                    if (line.Contains("step"))
                    {
                        steps.Add(int.Parse(line.Split('>')[1].Split('<')[0]));
                    }
                    else if (line.Contains("region"))
                    {
                        tempCoords = new int[4];
                        for (int i = 0; i < 4; i++)
                            tempCoords[i] = int.Parse(line.Split('(')[1].Split(')')[0].Split(',')[i]);

                        regions.Add(tempCoords);
                    }
                    else if (line.Contains("StartTime"))
                    {
                        start = int.Parse(line.Split('>')[1].Split('<')[0]);
                    }
                    else if (line.Contains("ImageTimeout"))
                    {
                        timeout = int.Parse(line.Split('>')[1].Split('<')[0]);
                    }
                    else if (line.Contains("Tolerance"))
                    {
                        tol = float.Parse(line.Split('>')[1].Split('<')[0]);
                    }
                    else if (line.Contains("Record"))
                    {
                        record = bool.Parse(line.Split('>')[1].Split('<')[0]);
                    }
                }

                steps.Sort();
            }
        }
    }
}
