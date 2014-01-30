using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace PlayBack
{
    struct config
    {
        public int startTime;
        public int timeout;
        public bool record;
        public float tolerance;
        public List<int> steps;
        public List<int[]> regions;

        public config (int tempStartTime, int tempTimeout, float tempTolerance)
        {
            startTime = tempStartTime;
            timeout = tempTimeout;
            tolerance = tempTolerance;
            record = false;
            steps = new List<int>();
            regions = new List<int[]>();
        }
    }

    class ReadConfig
    {
        static config ignList;
        static bool compiled = false;

        ReadConfig() { }

        public static config read(string file)
        {
            if (compiled == false)
            {
                ignList = new config(0,0,0);

                using (StreamReader r = new StreamReader(file))
                {
                    string line;
                    int[] tempCoords;
                    while ((line = r.ReadLine()) != null)
                    {
                        if (line.Contains("step"))
                        {
                            ignList.steps.Add(int.Parse(line.Split('>')[1].Split('<')[0]));
                        }
                        else if (line.Contains("region"))
                        {
                            tempCoords = new int[4];
                            for (int i = 0; i < 4; i++)
                                tempCoords[i] = int.Parse(line.Split('(')[1].Split(')')[0].Split(',')[i]);

                            ignList.regions.Add(tempCoords);
                        }
                        else if (line.Contains("StartTime"))
                        {
                            ignList.startTime = int.Parse(line.Split('>')[1].Split('<')[0]);
                        }
                        else if (line.Contains("ImageTimeout"))
                        {
                            ignList.timeout = int.Parse(line.Split('>')[1].Split('<')[0]);
                        }
                        else if (line.Contains("Tolerance"))
                        {
                            ignList.tolerance = float.Parse(line.Split('>')[1].Split('<')[0]);
                        }
                        else if (line.Contains("Record"))
                        {
                            ignList.record = bool.Parse(line.Split('>')[1].Split('<')[0]);
                        }
                    }

                    ignList.steps.Sort();
                }

                compiled = true;
            }

            return ignList;
        }
    }
}
