using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

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
        private Rectangle bounds = Screen.GetBounds(Point.Empty);

        public Replay() { }

        // Put csv instructions file into data object
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

            cleanDoubleClick();
        }

        //Double click is also recorded as two click down and click up events:
        private void cleanDoubleClick()
        {
            //Find doubleclick instances and delete the event before them:
            for (int i = 0; i < eventList.Count; i++)
            {
                for (int j = 0; j < eventList[i].events.Count; j++)
                {
                    if (eventList[i].events[j].Split(',')[0] == "doubleclick")
                    {
                        eventList[i].events.RemoveAt(j + 1);
                        eventList[i].events.RemoveAt(j - 1);
                        eventList.RemoveAt(i - 1);
                        i--;
                        break;
                    }
                }
            }
        }

        //Replay events in csv file:
        public bool playEvents(int numOfThreads, int tolerance)
        {
            Bitmap image;

            foreach (eventStore e in eventList)
            {
                image = new Bitmap(e.image);

                foreach (string s in e.events)
                {
                    if (!handleEvent(image, s, numOfThreads, tolerance))
                        return false;
                }

                image.Dispose();
            }

            return true;
        }

        //Handles method invokes for mouse and keyboard input
        private bool handleEvent(Bitmap image, string ev, int numOfThreads, int tolerance)
        {
            string[] coms = ev.Split(',');

            //Check if the initial conditions match:
            if (coms[0].Contains("Left") || coms[0].Contains("Right") || coms[0].Contains("Middle"))
            {
                //Move mouse to location recorded:
                MouseInput.mouse("move", uint.Parse(coms[1]), uint.Parse(coms[2]));

                //Check if the image matches the screen:
                if (!compareToScreen(image, numOfThreads, tolerance))
                    return false;
            }

            if ((coms[0].Contains("Left") || coms[0].Contains("Right") || coms[0].Contains("Middle")) && !coms[0].Contains("doubleclick"))
                MouseInput.mouse(coms[0], uint.Parse(coms[1]), uint.Parse(coms[2]));

            else if (coms[0].Contains("doubleclick"))
                MouseInput.doubleClick(coms[0], uint.Parse(coms[1]), uint.Parse(coms[2]));

            else if (coms[0] == "keydown")
                KeyboardInput.KeyDown(int.Parse(coms[1]));

            else if (coms[0] == "keyup")
                KeyboardInput.KeyUp(int.Parse(coms[1]));

            return true;
        }


        //Try to compare image to screen for 5 seconds:
        private bool compareToScreen(Bitmap image, int numOfThreads, int tolerance)
        {
            using (Bitmap screen = new Bitmap(bounds.Width, bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(screen))
                {
                    g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                }

                int index = 0;
                while (index < 5)
                {
                    if (CompareImages.driver(image, screen, numOfThreads, tolerance))
                        break;

                    Thread.Sleep(1000);
                    index++;
                }

                if (index == 5)
                    return false;
            }

            return true;
        }


        //Print what is stored in the eventList data structure
        public void printEvents()
        {
            foreach (eventStore e in eventList)
            {
                Console.WriteLine("Image: {0}", e.image);

                foreach (string s in e.events)
                {
                    Console.WriteLine("event: {0}", s);
                }

                Console.Write('\n');
            }
        }
    }
}
