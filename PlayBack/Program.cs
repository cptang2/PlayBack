using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace PlayBack
{
    class Program
    {
        static void Main(string[] args)
        {
            // % image difference tolerance
            int tolerance = 5;

            //Number of threads used in image compare:
            int numOfThreads = 4;

            Console.Write("File to replay: ");
            string file = Console.ReadLine();
            Console.WriteLine();
            while (!File.Exists(file))
            {
                Console.Write("File to replay: ");
                file = Console.ReadLine();
            }

            Replay repObject = new Replay();
            repObject.readInstructs(file);

            repObject.printEvents();

            Console.WriteLine("Finished");

            if (repObject.playEvents(numOfThreads, tolerance))
                Console.ReadKey();
        }
    }
}
