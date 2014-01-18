using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;


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
            //Read events from file and stores them in a list of eventStore objects:
            using (StreamReader instructs = new StreamReader(file))
            {
                string line;
                while ((line = instructs.ReadLine()) != null)
                {
                    if (line.Split(',')[0] == "image")
                        eventList.Add(new eventStore(line.Split(',')[1]));
                    else
                        eventList[eventList.Count - 1].events.Add(line);
                }
            }
        }


        public void playEvents()
        {
            Bitmap image;

            foreach (eventStore e in eventList)
            {
                image = new Bitmap(e.image);

                foreach (string s in e.events)
                {
                    handleEvent(s);
                }
                image.Dispose();
            }
        }


        private void handleEvent(string ev)
        {
            //Need to handle doubleclick
        }


        public void printEvents()
        {
            foreach (eventStore e in eventList)
            {
                Console.WriteLine("Image: {0}", e.image);

                foreach (string s in e.events)
                {
                    Console.WriteLine("event: {0}", s);
                }
            }
        }
    }
}
