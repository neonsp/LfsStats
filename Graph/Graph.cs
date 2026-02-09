/*
	LFSStat, Insim Replay statistics for Live For Speed Game
	Copyright (C) 2008 Jaroslav Černý alias JackCY, Robert B. alias Gai-Luron and Monkster.
	Jack.SC7@gmail.com, lfsgailuron@free.fr

	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
/*
 * Based on Graph v1.20 for LFS stats! (c) Alexander 'smith' Rudakov (piercemind@gmail.com)
 */
using System;
using System.IO;

namespace Graph
{
    public class Graph
    {
        private static LblData lblData;

        private static RprData rprData;

        private static LapData lapData;

        public static void GenerateGraph()
        {
            if (!Settings.ReadSettings("graph.xml"))
            {
                return;
            }

            string graphOutputDirectory = Settings.GraphOutputDirectory;
            Directory.CreateDirectory(graphOutputDirectory);
            DirectoryInfo directoryInfo = new DirectoryInfo(Settings.TSVInputDirectory);
            try
            {
                for (int i = 0; i < directoryInfo.GetFiles("*results_race_extended.tsv").Length; i++)
                {
                    string name = directoryInfo.GetFiles("*results_race_extended.tsv")[i].Name;
                    string text = "";
                    FileInfo fileInfo = new FileInfo(Settings.TSVInputDirectory + name);
                    if (fileInfo.Length > 20)
                    {
                        text = name.Substring(0, name.Length - 26);
                        string text2 = graphOutputDirectory + text + "_lbl.png";
                        FileInfo fileInfo2 = new FileInfo(text2);
                        if (!fileInfo2.Exists || fileInfo2.Length == 0)
                        {
                            lblData = new LblData(Settings.TSVInputDirectory + name);
                            lblData.Draw();
                            lblData.Save(text2, Settings.graphWidth, Settings.graphHeight);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            for (int j = 0; j < directoryInfo.GetFiles("*results_race_extended.tsv").Length; j++)
            {
                string name2 = directoryInfo.GetFiles("*results_race_extended.tsv")[j].Name;
                string text3 = "";
                FileInfo fileInfo3 = new FileInfo(Settings.TSVInputDirectory + name2);
                if (fileInfo3.Length > 20)
                {
                    text3 = name2.Substring(0, name2.Length - 26);
                    string text4 = graphOutputDirectory + text3 + "_rpr.png";
                    FileInfo fileInfo4 = new FileInfo(text4);
                    if (!fileInfo4.Exists || fileInfo4.Length == 0)
                    {
                        rprData = new RprData(Settings.TSVInputDirectory + name2);
                        rprData.Draw();
                        rprData.Save(text4, Settings.graphWidth, Settings.graphHeight);
                    }
                }
            }

            if (!Settings.OutputLapTimes)
            {
                return;
            }

            for (int k = 0; k < directoryInfo.GetFiles("*results_race_extended.tsv").Length; k++)
            {
                string name3 = directoryInfo.GetFiles("*results_race_extended.tsv")[k].Name;
                lapData = new LapData(Settings.TSVInputDirectory + name3);
                for (int l = 0; l < lapData.players.Count; l++)
                {
                    string text5 = "";
                    FileInfo fileInfo5 = new FileInfo(Settings.TSVInputDirectory + name3);
                    if (fileInfo5.Length > 20)
                    {
                        text5 = name3.Substring(0, name3.Length - 26);
                        string text6 = graphOutputDirectory + text5 + "_lap_" + Convert.ToString(l + 1) + ".png";
                        FileInfo fileInfo6 = new FileInfo(text6);
                        if (!fileInfo6.Exists || fileInfo6.Length == 0)
                        {
                            lapData.player = l;
                            lapData.Stats();
                            lapData.Draw();
                            lapData.Save(text6, Settings.graphWidth, Settings.graphHeight);
                        }
                    }
                }
            }
        }
    }
}
