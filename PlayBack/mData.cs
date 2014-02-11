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

        public mData(string file)
        {
            this.file = file;
            this.dir = Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file));
            cfg = new Config(Path.Combine(dir, @"config.xml"));
            rF = new StreamWriter(rFCreate(Program.data.dir + "_Results"));
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
    }
}
