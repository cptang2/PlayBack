using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace PlayBack
{
    class Startup
    {
        // Read instructions from csv file and replay them:
        public static void run(int threads, string file)
        {
            string dir = Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file));

            //Wait for the time specified in the config file
            Thread.Sleep((new Config(Path.Combine(dir, @"config.xml"))).start);

            //Run playback and record the results in a results file:
            using (StreamWriter rF = new StreamWriter(rFCreate(dir + "_Results")))
            {
                Replay rObj = new Replay(file, rF);

                if (rObj.playEvents(threads))
                    rF.WriteLine("Successful run");
                else
                    rF.WriteLine("Failed run");
            }

            Console.WriteLine("Finished");
        }


        static string rFCreate(string rDir)
        {
            if (!Directory.Exists(rDir))
                Directory.CreateDirectory(rDir);

            return Path.Combine(rDir, "results.txt");
        }
    }
}
