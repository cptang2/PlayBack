using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

namespace PlayBack
{
    class Program
    {
        static void Main(string[] args)
        {
            // % image difference tolerance
            //float tolerance = .5F;
            //Number of threads used in image compare:
            int numOfThreads = 4;
            string result;
            string file = null;

            //Get input:
            while (!File.Exists(file))
            {
                Console.Write("File to replay: ");
                file = Console.ReadLine();
            }

            config configList = ReadConfig.read(Path.Combine(Path.GetDirectoryName(file), 
                                                                    Path.GetFileNameWithoutExtension(file) + @"\config.xml"));
            
            //Wait for the time specified in the config file
            System.Threading.Thread.Sleep(configList.startTime);

            //Run playback and record the results:
            result = createResultsFolder(file);
            result = Path.Combine(result, "results.txt");
            using (StreamWriter resultsFile = new StreamWriter(result))
            {
                Replay repObject = new Replay(file, resultsFile);
                repObject.readInstructs();
                repObject.printEvents();

                if (repObject.playEvents(numOfThreads, configList.tolerance))
                    resultsFile.WriteLine("Successful run");
                else
                    resultsFile.WriteLine("Failed run");

            }
            
            Console.WriteLine("Finished");
          
            Console.ReadKey();
        }


        static string createResultsFolder(string file)
        {
            string results = Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file) + "_Results");
            if (!Directory.Exists(results))
            {
                Directory.CreateDirectory(results);
            }
            return results;
        }
    }
}
