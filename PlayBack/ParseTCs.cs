using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace PlayBack
{
    abstract class ParseTCs
    {
        // Put csv instructions file into data object
        public List<step> readTCs(string file)
        {
            List<step> sList = new List<step>();

            //Read events from file and stores them in a list of eventStore objects:
            using (StreamReader instructs = new StreamReader(file))
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

            delIgnored(sList);
            cleanDoubleClick(sList);

            return sList;
        }


        //Remove ignored steps:
        private void delIgnored(List<step> sList)
        {
            for (int i = Program.data.cfg.steps.Count - 1; i >= 0; i--)
                sList.RemoveAt(Program.data.cfg.steps[i]);
        }


        //Double click is also recorded as two click down and click up events.
        //Need to remove the unnecessary events
        private void cleanDoubleClick(List<step> sList)
        {
            List<string> sE;

            Func<List<step>, int, int> remStep = (s, i) =>
            {
                s[i].setImage(s[i - 1].image);
                s.RemoveAt(i - 1);

                return (i - 1);
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

    }
}
