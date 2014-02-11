using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace PlayBack
{
    /*
    //Data structure used to store config file info:
    struct Config
    {
        public int start;
        public int timeout;
        public bool record;
        public float tol;
        public List<int> steps;
        public List<int[]> regions;

        public Config (int start, int timeout, float tol)
        {
            this.start = start;
            this.timeout = timeout;
            this.tol = tol;
            record = false;
            steps = new List<int>();
            regions = new List<int[]>();
        }
    }


    class ReadConfig
    {
        static Config ignList;
        static bool compiled = false;

        ReadConfig() { }

        //Didn't feel like using xPath
        public static Config read(string file)
        {
            if (compiled == false)
            {
                ignList = new Config(0,0,0);

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
                            ignList.start = int.Parse(line.Split('>')[1].Split('<')[0]);
                        }
                        else if (line.Contains("ImageTimeout"))
                        {
                            ignList.timeout = int.Parse(line.Split('>')[1].Split('<')[0]);
                        }
                        else if (line.Contains("Tolerance"))
                        {
                            ignList.tol = float.Parse(line.Split('>')[1].Split('<')[0]);
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
    }*/
}
