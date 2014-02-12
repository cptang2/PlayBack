using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Threading;

namespace PlayBack
{
    class Program
    {
        public static mData data;

        // Used to store testcase and database pairs
        class dbf
        {
            Dictionary<string, string> tRef = new Dictionary<string, string>();

            public dbf() { }


            public void add(string k, string s)
            {
                tRef.Add(k, s);
            }


            public bool contains(string k)
            {
                return tRef.ContainsKey(k);
            }


            public string this[string k]
            {
                get { return tRef[k]; }
                set { tRef.Add(k, value); }
            }
        }


        static void Main(string[] args)
        {
            const int threads = 4;
            const string fPath = @"C:\GDrop\Aut.txt";
            List<string> files = new List<string>();

            if (!File.Exists(fPath))
                files = getInput();

            foreach (string f in files)
            {
                data = new mData(f, threads);
                Startup.run();
            }

            Console.ReadKey();
        }


        //Get user input for file to replay
        private static List<string> getInput()
        {
            string file = null;
            List<string> files = new List<string>();

            while (!File.Exists(file))
            {
                Console.Write("File to replay: ");
                file = Console.ReadLine();
            }

            files.Add(file);

            return files;
        }


        //Parse input file for test cases
        private static dbf inParse(string file)
        {
            string ln;
            dbf tcs = new dbf();

            Func<string, bool> isInt = (k) =>
            {
                int id;
                return int.TryParse(k, out id);
            };

            using (StreamReader sr = new StreamReader(file))
            {
                while ((ln = sr.ReadLine()) != null)
                {
                    string[] tLn = ln.Split(',');

                    if (tLn.Length > 0)
                        if (tLn[0].Length > 0 && !tcs.contains(tLn[0]) && tLn[1].Length > 0 && isInt(tLn[2]))
                            tcs.add(tLn[0], tLn[1]);
                }
            }

            return tcs;
        }
    }
}
