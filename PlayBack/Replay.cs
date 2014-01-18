using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace PlayBack
{
    struct eventStore
    {
        public string image;
        public List<string> events;

        public eventStore(string tempImage)
        {
            image = tempImage;
            events = new List<string>();
        }
    }


    class Replay
    {
        List<eventStore> eventList = new List<eventStore>();

        public Replay() { }

        public void readInstructs(string file)
        {
            eventStore tempStore;

            //Read events from file:
            using (StreamReader instructs = new StreamReader(file))
            {
                string line;
                while ((line = instructs.ReadLine()) != null)
                {
                    if (line.Split(',')[0] == "image")
                        tempStore = new eventStore(line);
                }
            }
        }
    }
}
