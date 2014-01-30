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
        config configList;
        string file;
        StreamWriter resultsFile;
        List<eventStore> eventList = new List<eventStore>();
        private Rectangle bounds = Screen.GetBounds(Point.Empty);

        public Replay(string tempFile, StreamWriter tempResultsFile) 
        {
            file = tempFile;
            configList = ReadConfig.read(Path.Combine(Path.GetDirectoryName(file),
                                                      Path.GetFileNameWithoutExtension(file) + @"\config.xml"));

            resultsFile = tempResultsFile;
        }

        // Put csv instructions file into data object
        public void readInstructs()
        {
            //Read events from file and stores them in a list of eventStore objects:
            using (StreamReader instructs = new StreamReader(file))
            {
                string line;
                while ((line = instructs.ReadLine()) != null)
                {
                    if (line.Split(',')[0] == "image")
                        eventList.Add(new eventStore(line.Split(',')[1]));
                    else if (eventList.Count > 0)
                        eventList[eventList.Count - 1].events.Add(line);
                }
            }

            removeIgnored();
            cleanDoubleClick();
        }


        //Remove ignored items:
        private void removeIgnored()
        {
            for (int i = configList.steps.Count - 1; i >= 0; i--)
            {
                eventList.RemoveAt(configList.steps[i]);
            }
        }


        //Double click is also recorded as two click down and click up events:
        private void cleanDoubleClick()
        {
            eventStore temp;

            //Find doubleclick instances and delete the event before them:
            for (int i = 0; i < eventList.Count; i++)
            {
                for (int j = 0; j < eventList[i].events.Count; j++)
                {
                    if (eventList[i].events[j].Split(',')[0].Contains("doubleclick"))
                    {
                        eventList[i].events.RemoveAt(j + 1);
                        eventList[i].events.RemoveAt(j - 1);

                        if (i > 0)
                        {
                            temp = new eventStore();
                            temp = eventList[i];
                            temp.image = eventList[i - 1].image;
                            eventList[i] = temp;

                            eventList.RemoveAt(i - 1);
                            i--;
                        }

                        break;
                    }
                }
            }
        }

        //Replay events in csv file:
        public bool playEvents(int numOfThreads, float tolerance)
        {
            Bitmap image;

            foreach (eventStore e in eventList)
            {
                image = new Bitmap(Path.Combine(Path.GetDirectoryName(file), Path.Combine(Path.GetFileNameWithoutExtension(file), e.image)));
                resultsFile.WriteLine("Step: {0}", e.image);

                foreach (string s in e.events)
                {
                    if (!handleEvent(image, s, numOfThreads, tolerance, e.image))
                        return false;
                }

                image.Dispose();
            }

            return true;
        }

        //Handles method invokes for mouse and keyboard input
        private bool handleEvent(Bitmap image, string ev, int numOfThreads, float tolerance, string imageName)
        {
            string[] coms = ev.Split(',');

            //Check if the initial conditions match:
            if ((coms[0].Contains("Left") || coms[0].Contains("Right") || coms[0].Contains("Middle")) && !coms[0].Contains("up"))
            {
                //Move mouse to location recorded:
                //MouseInput.mouse("move", uint.Parse(coms[1]), uint.Parse(coms[2]));
                MouseInput.move(int.Parse(coms[1]), int.Parse(coms[2]));

                //Check if the image matches the screen:
                if (!compareToScreen(image, numOfThreads, tolerance, configList, imageName))
                    return false;
            }

            //If mouse up, move slowly to target area:
            if (coms[0].Contains("up") && !coms[0].Contains("key"))
                MouseInput.move(int.Parse(coms[1]), int.Parse(coms[2]));

            if ((coms[0].Contains("Left") || coms[0].Contains("Right") || coms[0].Contains("Middle")) && !coms[0].Contains("doubleclick"))
                MouseInput.mouse(coms[0], int.Parse(coms[1]), int.Parse(coms[2]));

            else if (coms[0] == "detent")
                MouseInput.mouseWheel(int.Parse(coms[1]), int.Parse(coms[2]), int.Parse(coms[3]));

            else if (coms[0].Contains("doubleclick"))
                MouseInput.doubleClick(coms[0], int.Parse(coms[1]), int.Parse(coms[2]));

            else if (coms[0] == "keydown")
                KeyboardInput.KeyDown(int.Parse(coms[1]));

            else if (coms[0] == "keyup")
                KeyboardInput.KeyUp(int.Parse(coms[1]));

            return true;
        }


        //Compare the image to the screen for an allotted time:
        private bool compareToScreen(Bitmap image, int numOfThreads, float tolerance, config configPath, string imageName)
        {
            using (Bitmap screen = new Bitmap(bounds.Width, bounds.Height))
            {
                int index = 0;
                while (index < (int)(configList.timeout / 1000))
                {
                    using (Graphics g = Graphics.FromImage(screen))
                    {
                        g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                    }

                    if (CompareImages.driver(image, screen, numOfThreads, tolerance, configPath, resultsFile))
                        break;

                    Thread.Sleep(1000);
                    index++;
                }

                resultsFile.WriteLine();

                if (configList.record)
                    screen.Save(Path.Combine(Path.GetDirectoryName(file),
                                                                    (Path.GetFileNameWithoutExtension(file) + "_Results\\" + imageName)));

                if (index == (int)(configList.timeout / 1000))
                {
                    screen.Save(Path.Combine(Path.GetDirectoryName(file),
                                                                    (Path.GetFileNameWithoutExtension(file) + "_Results\\" + imageName)));

                    //Up key everything:
                    for (int i = 1; i < 150; i++)
                        KeyboardInput.KeyUp(i);

                    return false;
                }
            }

            return true;
        }


        //Print what is stored in the eventList data structure
        public void printEvents()
        {
            foreach (eventStore e in eventList)
            {
                Console.WriteLine("Image: {0}", e.image);
                resultsFile.WriteLine("Image: {0}", e.image);
                foreach (string s in e.events)
                {
                    Console.WriteLine("event: {0}", s);
                    resultsFile.WriteLine("Image: {0}", s);
                }
                resultsFile.WriteLine();
                Console.Write('\n');
            }
        }
    }
}
