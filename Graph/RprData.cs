/*
	LFSStat, Insim Replay statistics for Live For Speed Game
	Copyright (C) 2008 Jaroslav Èerný alias JackCY, Robert B. alias Gai-Luron and Monkster.
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
using NPlot;
using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Graph
{
    /// <summary>
    /// Summary description for RprData.
    /// </summary>
    internal class RprData
    {
        private string filename;
        private NPlot.Windows.PlotSurface2D plot;
        private ArrayList players;
        private ArrayList splits;
        private int nsplits;
        private int winner;
        private long winner_avgtime;
        private long[] winner_avgsplits;
        private double[] winner_avgsplitsproportions;

        public RprData(string fn)
        {
            this.filename = fn;
            this.plot = new NPlot.Windows.PlotSurface2D();
            FileStream stream = new FileStream(this.filename, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(stream);
            string str = "";
            this.players = new ArrayList();
            this.splits = new ArrayList();
            this.nsplits = Convert.ToInt32(reader.ReadLine());
            char[] separator = new char[] { '\t' };
            while ((str = reader.ReadLine()) != null)
            {
                string[] strArray = str.Split(separator);
                this.players.Add(strArray[0]);
                this.splits.Add(new ArrayList());
                this.splits[this.splits.Count - 1] = new long[strArray.Length - 1];
                for (int i = 1; i < strArray.Length; i++)
                {
                    string str2 = strArray[i];
                    ((long[])this.splits[this.splits.Count - 1])[i - 1] = Convert.ToInt32(str2);
                }
            }
            stream.Close();
            this.Stats();
        }

        public void Draw()
        {
            this.plot.Clear();
            Grid p = new Grid
            {
                VerticalGridType = Grid.GridType.Coarse,
                HorizontalGridType = Grid.GridType.Coarse,
                MajorGridPen = new Pen(Color.LightGray, 1f)
            };
            this.plot.Add(p);
            int num = 1;
            int num2 = ((long[])this.splits[this.winner]).Length / this.nsplits;
            if (num2 <= 30)
            {
                num = 1;
            }
            else if ((num2 > 30) && (num2 <= 60))
            {
                num = 2;
            }
            else if (num2 > 60)
            {
                num = 3;
            }
            LinearAxis a = new LinearAxis
            {
                NumberOfSmallTicks = 0,
                LargeTickStep = num,
                HideTickText = true,
                WorldMin = 1.0,
                WorldMax = num2
            };
            this.plot.XAxis1 = a;
            LinearAxis axis2 = new LinearAxis(a)
            {
                Label = Settings.RaceProgressGraphXAxisLabel,
                NumberOfSmallTicks = 0,
                LargeTickStep = num,
                WorldMin = 1.0,
                WorldMax = num2,
                HideTickText = false,
                LabelFont = Settings.commonFont
            };
            this.plot.XAxis2 = axis2;
            LinearAxis axis3 = new LinearAxis
            {
                NumberOfSmallTicks = 0,
                Reversed = true,
                LargeTickStep = 10.0,
                WorldMin = -5.0,
                WorldMax = 5.0,
                LabelFont = Settings.commonFont,
                HideTickText = true
            };
            this.plot.YAxis1 = axis3;
            this.plot.Title = Settings.RaceProgressGraphTitle;
            this.plot.TitleFont = Settings.titleFont;
            Legend legend = new Legend
            {
                Font = Settings.commonFont,
                BorderStyle = Settings.legendBorderType,
                XOffset = 60
            };
            this.plot.Legend = legend;
            this.plot.PlotBackColor = Color.White;
            this.plot.BackColor = Color.White;
            this.plot.Add(new HorizontalLine(0.0, Color.Black));
            for (int i = 0; i < this.players.Count; i++)
            {
                if (((long[])this.splits[i]).Length >= this.nsplits)
                {
                    double[] numArray = new double[(((long[])this.splits[i]).Length - this.nsplits) + 2];
                    double[] numArray2 = new double[] { 0.0 };
                    numArray[0] = 1.0;
                    int num4 = 1;
                    int num5 = 0;
                    int num6 = 0;
                    while (true)
                    {
                        if (num5 >= ((((long[])this.splits[i]).Length - this.nsplits) + 1))
                        {
                            LinePlot plot = new LinePlot
                            {
                                AbscissaData = numArray,
                                OrdinateData = numArray2,
                                Pen = new Pen(Colors.GetColor(i), 1f),
                                Label = $"{this.players[i].ToString()}"
                            };
                            this.plot.Add(plot);
                            LinearAxis axis4 = new LinearAxis(this.plot.YAxis1)
                            {
                                Label = ""
                            };
                            axis4.Label = Settings.RaceProgressGraphYAxisLabel;
                            axis4.NumberOfSmallTicks = axis3.NumberOfSmallTicks;
                            axis4.LargeTickStep = axis3.LargeTickStep;
                            axis4.TickTextNextToAxis = false;
                            axis4.LabelFont = Settings.commonFont;
                            axis4.HideTickText = false;
                            this.plot.YAxis2 = axis4;
                            double worldMin = this.plot.YAxis1.WorldMin;
                            this.plot.Refresh();
                            break;
                        }
                        double num8 = 0.0;
                        int num9 = 0;
                        while (true)
                        {
                            if (num9 >= num6)
                            {
                                double num7 = num4 + num8;
                                numArray[num5 + 1] = num7;
                                long num10 = 0L;
                                int index = 0;
                                while (true)
                                {
                                    if (index > num6)
                                    {
                                        long num14 = ((long[])this.splits[i])[(num5 + this.nsplits) - 1] - ((this.winner_avgtime * num4) + num10);
                                        numArray2[num5 + 1] = num14;
                                        double numPtr1 = (numArray2[num5 + 1]);
                                        numPtr1 /= 10000.0;
                                        if (numArray2[num5 + 1] > Settings.graphMinMax)
                                        {
                                            numArray2[num5 + 1] = Settings.graphMinMax;
                                        }
                                        if ((num6 + 1) == this.nsplits)
                                        {
                                            num6 = 0;
                                            num4++;
                                        }
                                        num5++;
                                        break;
                                    }
                                    num10 += this.winner_avgsplits[index];
                                    index++;
                                }
                                break;
                            }
                            num8 += this.winner_avgsplitsproportions[num9 + 1];
                            num9++;
                        }
                    }
                }
            }
        }

        public void Save(string outfile, int width, int height)
        {
            Bitmap image = new Bitmap(width, height);
            this.plot.Draw(Graphics.FromImage(image), new Rectangle(0, 0, image.Width, image.Height));
            try
            {
                image.Save(outfile, ImageFormat.Png);
            }
            catch
            {
                Console.WriteLine("Error! Cannot create " + outfile);
            }
        }

        private void Stats()
        {
            winner = 0;
            int num = 0;
            long num2 = 2147352578L;
            for (int i = 0; i < players.Count; i++)
            {
                int num3 = ((long[])splits[i]).Length - 1;
                if ((((long[])splits[winner]).Length - 1 <= 0 && ((long[])splits[i]).Length - 1 > 0) || (num3 != -1 && ((long[])splits[i])[num3] <= num2 && num3 >= num) || (num3 != -1 && ((long[])splits[i])[num3] >= num2 && num3 > num))
                {
                    winner = i;
                    num = num3;
                    num2 = ((long[])splits[i])[num3];
                }
            }

            winner_avgtime = ((long[])splits[winner])[((long[])splits[winner]).Length - 1] / (((long[])splits[winner]).Length / nsplits);
            winner_avgsplits = new long[nsplits + 1];
            winner_avgsplitsproportions = new double[nsplits + 1];
            for (int j = 0; j < nsplits; j++)
            {
                int num4 = 0;
                for (int k = j; k < ((long[])splits[winner]).Length; k += nsplits)
                {
                    if (k == 0)
                    {
                        winner_avgsplits[j + 1] += ((long[])splits[winner])[k];
                    }
                    else
                    {
                        winner_avgsplits[j + 1] += ((long[])splits[winner])[k] - ((long[])splits[winner])[k - 1];
                    }

                    num4++;
                }

                winner_avgsplits[j + 1] /= num4;
            }

            for (int l = 0; l <= nsplits; l++)
            {
                winner_avgsplitsproportions[l] = (double)winner_avgsplits[l] / (double)winner_avgtime;
            }
        }
    }
}
