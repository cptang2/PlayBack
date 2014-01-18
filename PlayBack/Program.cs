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
            Replay repObject = new Replay();
            repObject.readInstructs(Path.Combine(Environment.CurrentDirectory, "testcases\\replay.csv"));

            Console.ReadKey();
        }
    }
}
