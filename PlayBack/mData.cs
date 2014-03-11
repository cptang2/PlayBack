using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace PlayBack
{
    //Intended to hold metaData for the application:
    class mData
    {
        public Config cfg;
        public string dir, file;
        public StreamWriter rF;
        int threads;


        public mData(string file, int threads)
        {
            this.file = file;
            this.dir = Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file));
            cfg = new Config(Path.Combine(dir, @"config.xml"));
            rF = new StreamWriter(rFCreate(Program.data.dir + "_Results"));
            this.threads = threads;
        }


        public void dispose()
        {
            rF.Dispose();
        }


        static string rFCreate(string rDir)
        {
            if (!Directory.Exists(rDir))
                Directory.CreateDirectory(rDir);

            return Path.Combine(rDir, "results.txt");
        }


        public int getThreads()
        {
            return threads;
        }
    }
}
