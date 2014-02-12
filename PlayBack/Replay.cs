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


    class Replay : ParseTCs
    {
        private List<step> sList;
        private Rectangle bounds = Screen.GetBounds(Point.Empty);


        public Replay() 
        {
            sList = readTCs(Program.data.file);
            printEvents();
        }


        //Replay events in csv file:
        public bool playSteps()
        {
            foreach (step e in sList)
            {
                Program.data.rF.WriteLine("Step: {0}", e.image);

                foreach (string s in e.events)
                {
                    if (!handleStep(s, e.image))
                        return false;
                }
            }

            return true;
        }


        //Handles method invokes for mouse and keyboard input
        private bool handleStep(string ev, string image)
        {
            string[] coms = ev.Split(',');

            //Handle non-click events:
            switch (coms[0])
            {
                case ("detent"):
                    MouseInput.mouseWheel(int.Parse(coms[1]), int.Parse(coms[2]), int.Parse(coms[3]));
                    return true;

                case ("keydown"):
                    KeyboardInput.KeyDown(int.Parse(coms[1]));
                    return true;

                case ("keyup"):
                    KeyboardInput.KeyUp(int.Parse(coms[1]));
                    return true;
            }

            return checkClick(coms, image);
        }


        //Handle click event
        private bool checkClick(string[] coms, string image)
        {
            string[] dwn = { "Left", "Right", "Middle", 
                             "doubleclickLeft", "doubleclickRight", "doubleclickMiddle" };

            MouseInput.move(int.Parse(coms[1]), int.Parse(coms[2]));

            if (dwn.Contains(coms[0]))
                if (!compareToScreen(image))
                    return false;

            MouseInput.mouse(coms[0], int.Parse(coms[1]), int.Parse(coms[2]));
            return true;
        }


        //Compare the image to the screen for an allotted time:
        private bool compareToScreen(string image)
        {
            using (Bitmap png = new Bitmap(Path.Combine(Program.data.dir, image)))
            {
                using (Bitmap screen = new Bitmap(bounds.Width, bounds.Height))
                {
                    int index = 0;
                    while (index < (int)(Program.data.cfg.timeout / 1000))
                    {
                        //Get current screen bitmap:
                        using (Graphics g = Graphics.FromImage(screen))
                        {
                            g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                        }

                        if (CompareImages.driver(png, screen))
                            break;

                        Thread.Sleep(1000);
                        index++;
                    }

                    Program.data.rF.WriteLine();

                    checkSave(screen, image, index);
                }
            }

            return true;
        }


        //Check whether the image needs to be saved:
        private bool checkSave(Bitmap screen, string image, int index)
        {
            if (Program.data.cfg.record)
                screen.Save(Path.Combine(Program.data.dir, ("_Results\\" + image)));

            if (index == (int)(Program.data.cfg.timeout / 1000))
            {
                screen.Save(Path.Combine(Program.data.dir, ("_Results\\" + image)));

                //Up key everything:
                for (int i = 1; i < 150; i++)
                    KeyboardInput.KeyUp(i);

                return false;
            }

            return true;
        }


        //Print what is stored in the eventList data structure
        private void printEvents()
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
