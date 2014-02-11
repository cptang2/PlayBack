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
    //Each step consists of an image and the corresponding events
    struct step
    {
        public string image;
        public List<string> events;


        public step(string image)
        {
            this.image = image;
            events = new List<string>();
        }


        public void setImage(string image)
        {
            this.image = image;
        }
    }


    class Replay
    {
        private List<step> sList = new List<step>();
        private Rectangle bounds = Screen.GetBounds(Point.Empty);


        public Replay() 
        {
            readTCs();
            printEvents();
        }


        // Put csv instructions file into data object
        public void readTCs()
        {
            //Read events from file and stores them in a list of eventStore objects:
            using (StreamReader instructs = new StreamReader(Program.data.file))
            {
                string line;
                while ((line = instructs.ReadLine()) != null)
                {
                    if (line.Split(',')[0] == "image")
                        sList.Add(new step(line.Split(',')[1]));
                    else if (sList.Count > 0)
                        sList[sList.Count - 1].events.Add(line);
                }
            }

            delIgnored();
            cleanDoubleClick();
        }


        //Remove ignored steps:
        private void delIgnored()
        {
            for (int i = Program.data.cfg.steps.Count - 1; i >= 0; i--)
                sList.RemoveAt(Program.data.cfg.steps[i]);
        }


        //Double click is also recorded as two click down and click up events:
        private void cleanDoubleClick()
        {
            List<string> sE;

            Func<List<step>, int, int> remStep = (s, i) =>
            {
                s[i].setImage(s[i - 1].image);
                s.RemoveAt(i - 1);

                return (i-1);
            };

            //Find doubleclick instances and delete the step before them:
            for (int i = 0; i < sList.Count; i++)
            {
                sE = sList[i].events;
                for (int j = 0; j < sE.Count; j++)
                {
                    if (sE[j].Split(',')[0].Contains("doubleclick"))
                    {
                        sE.RemoveAt(j + 1);
                        sE.RemoveAt(j - 1);

                        if (i > 0)
                            i = remStep(sList, i);

                        break;
                    }
                }
            }
        }


        //Replay events in csv file:
        public bool playSteps(int numOfThreads)
        {
            Bitmap image;

            foreach (step e in sList)
            {
                image = new Bitmap(Path.Combine(Program.data.dir, e.image));
                Program.data.rF.WriteLine("Step: {0}", e.image);

                foreach (string s in e.events)
                {
                    if (!handleStep(image, s, numOfThreads, Program.data.cfg.tol, e.image))
                        return false;
                }

                image.Dispose();
            }

            return true;
        }


        //Handles method invokes for mouse and keyboard input
        private bool handleStep(Bitmap image, string ev, int numOfThreads, float tolerance, string imageName)
        {
            string[] coms = ev.Split(',');

            //Check if the initial conditions match:
            if ((coms[0].Contains("Left") || coms[0].Contains("Right") || coms[0].Contains("Middle")) && !coms[0].Contains("up"))
            {
                //Move mouse to location recorded:
                //MouseInput.mouse("move", uint.Parse(coms[1]), uint.Parse(coms[2]));
                MouseInput.move(int.Parse(coms[1]), int.Parse(coms[2]));

                //Check if the image matches the screen:
                if (!compareToScreen(image, numOfThreads, tolerance, Program.data.cfg, imageName))
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
        private bool compareToScreen(Bitmap image, int numOfThreads, float tolerance, Config configPath, string imageName)
        {
            using (Bitmap screen = new Bitmap(bounds.Width, bounds.Height))
            {
                int index = 0;
                while (index < (int)(Program.data.cfg.timeout / 1000))
                {
                    using (Graphics g = Graphics.FromImage(screen))
                    {
                        g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                    }

                    if (CompareImages.driver(image, screen, numOfThreads, tolerance, configPath, Program.data.rF))
                        break;

                    Thread.Sleep(1000);
                    index++;
                }

                Program.data.rF.WriteLine();

                if (Program.data.cfg.record)
                    screen.Save(Path.Combine(Program.data.dir, ("_Results\\" + imageName)));

                if (index == (int)(Program.data.cfg.timeout / 1000))
                {
                    screen.Save(Path.Combine(Program.data.dir, ("_Results\\" + imageName)));

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
            foreach (step e in sList)
            {
                Console.WriteLine("Image: {0}", e.image);
                Program.data.rF.WriteLine("Image: {0}", e.image);

                foreach (string s in e.events)
                {
                    Console.WriteLine("event: {0}", s);
                    Program.data.rF.WriteLine("Image: {0}", s);
                }

                Program.data.rF.WriteLine();
                Console.Write('\n');
            }
        }
    }
}
