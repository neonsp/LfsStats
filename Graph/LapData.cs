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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace Graph
{
    /// <summary>
    /// Summary description for LapData.
    /// </summary>
    internal class LapData
    {
        private string filename;
        private NPlot.Windows.PlotSurface2D plot;
        public ArrayList players;
        public ArrayList splits;
        private int nsplits;
        private int winner;
        private long winner_avgtime;
        private long bestlap;
        public int player = 0;
        private int laps;

        public LapData(string fn)
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
        }

        public void Draw()
        {
            this.plot.PlotBackColor = Color.White;
            this.plot.BackColor = Color.White;
            ArrayList list = new ArrayList();
            ArrayList list2 = new ArrayList();
            int num = 1;
            long t = 0L;
            float num3 = 0f;
            float num4 = 1.073676E+09f;
            long num5 = 0L;
            long num6 = 0L;
            this.plot.Clear();
            if (((long[])this.splits[this.winner]).Length >= this.nsplits)
            {
                for (int i = this.nsplits; i < ((long[])this.splits[this.winner]).Length; i++)
                {
                    t += ((long[])this.splits[this.winner])[i];
                    if (i != 0)
                    {
                        t -= ((long[])this.splits[this.winner])[i - 1];
                    }
                    if (num != this.nsplits)
                    {
                        num++;
                    }
                    else
                    {
                        string str = "  " + this.timetostr(t, true) + "  :  ";
                        int num8 = this.nsplits - 1;
                        while (true)
                        {
                            if (num8 < 0)
                            {
                                list2.Add(str + "  ");
                                num6 += t;
                                num5 += 1L;
                                if (num3 < t)
                                {
                                    num3 = t;
                                }
                                if (num4 > t)
                                {
                                    num4 = t;
                                }
                                list.Add((double)t);
                                num = 1;
                                t = 0L;
                                break;
                            }
                            str = ((i != this.nsplits) || (num8 != this.nsplits)) ? (str + this.timetostr(((long[])this.splits[this.winner])[i - num8] - ((long[])this.splits[this.winner])[(i - 1) - num8], false)) : (str + this.timetostr(((long[])this.splits[this.winner])[i - num8], false));
                            if (num8 != 0)
                            {
                                str = str + " , ";
                            }
                            num8--;
                        }
                    }
                }
                if (num5 != 0L)
                {
                    this.winner_avgtime = num6 / num5;
                }
                if (num5 != 0L)
                {
                    Grid p = new Grid
                    {
                        VerticalGridType = Grid.GridType.Coarse,
                        HorizontalGridType = Grid.GridType.Coarse,
                        MajorGridPen = new Pen(Color.LightGray, 1f)
                    };
                    this.plot.Add(p);
                    for (int j = 0; j < list.Count; j++)
                    {
                        double[] numArray = new double[1];
                        double[] numArray2 = new double[1];
                        if (((double)list[j]) < this.winner_avgtime)
                        {
                            numArray[0] = j;
                            numArray2[0] = ((double)list[j]) / 10000.0;
                            HistogramPlot plot = new HistogramPlot
                            {
                                OrdinateData = numArray2,
                                AbscissaData = numArray,
                                RectangleBrush = new RectangleBrushes.Horizontal(Color.FromArgb(0x6a, 0xcd, 0x54), Color.FromArgb(0xeb, 0xff, 0xd5)),
                                Pen = { Color = Color.FromArgb(0, 150, 0) },
                                Filled = true,
                                ShowInLegend = false
                            };
                            this.plot.Add(plot);
                        }
                        if (((double)list[j]) >= this.winner_avgtime)
                        {
                            numArray[0] = j;
                            numArray2[0] = (!Settings.LimitLapTimes || ((((double)list[j]) - this.winner_avgtime) <= ((this.winner_avgtime - num4) * Settings.LimitMultiplier))) ? (((double)list[j]) / 10000.0) : ((this.winner_avgtime + ((this.winner_avgtime - num4) * Settings.LimitMultiplier)) / 10000.0);
                            HistogramPlot plot2 = new HistogramPlot
                            {
                                OrdinateData = numArray2,
                                AbscissaData = numArray,
                                RectangleBrush = (!Settings.LimitLapTimes || ((((double)list[j]) - this.winner_avgtime) <= ((this.winner_avgtime - num4) * Settings.LimitMultiplier))) ? new RectangleBrushes.Horizontal(Color.FromArgb(0xeb, 0x54, 0x89), Color.FromArgb(0xff, 230, 210)) : new RectangleBrushes.Horizontal(Color.FromArgb(190, 0x27, 0x5c), Color.FromArgb(0xeb, 0x7c, 0xb1)),
                                Pen = { Color = Color.FromArgb(150, 0, 0) },
                                Filled = true,
                                ShowInLegend = false
                            };
                            this.plot.Add(plot2);
                        }
                    }
                    LabelAxis axis = new LabelAxis(this.plot.XAxis1)
                    {
                        TicksBetweenText = false,
                        TicksCrossAxis = false,
                        LargeTickSize = 0,
                        TickTextFont = Settings.lapTimesFont
                    };
                    for (int k = 0; k < list2.Count; k++)
                    {
                        axis.AddLabel((string)list2[k], (double)k);
                    }
                    axis.TicksLabelAngle = -90f;
                    this.plot.XAxis1 = axis;
                    axis = new LabelAxis((LabelAxis)axis.Clone())
                    {
                        TicksBetweenText = false,
                        TicksCrossAxis = true,
                        LargeTickSize = 2,
                        TicksLabelAngle = -90f,
                        TickTextNextToAxis = false,
                        TickTextFont = Settings.commonFont
                    };
                    for (int m = 0; m < list2.Count; m++)
                    {
                        axis.AddLabel(Convert.ToString((int)(m + 2)), (double)m);
                    }
                    axis.LabelFont = Settings.commonFont;
                    this.plot.XAxis2 = axis;
                    this.plot.YAxis1.TicksCrossAxis = true;
                    this.plot.YAxis1.Label = (string)this.players[this.player];
                    this.plot.YAxis1.LabelFont = Settings.titleFont;
                    this.plot.YAxis1.LabelOffset = 20f;
                    this.plot.YAxis1.NumberFormat = "";
                    this.plot.YAxis1.TicksCrossAxis = false;
                    if (Settings.LimitToGlobalBestLap)
                    {
                        this.plot.YAxis1.WorldMin = ((double)this.bestlap) / 10000.0;
                    }
                    ((LinearAxis)this.plot.YAxis1).NumberOfSmallTicks = 4;
                    ((LinearAxis)this.plot.YAxis1).LargeTickStep = 1.0;
                    ((LinearAxis)this.plot.YAxis1).TicksLabelAngle = -90f;
                    HorizontalLine line = new HorizontalLine((double)(((float)this.winner_avgtime) / 10000f), Color.Gray)
                    {
                        Pen = { DashStyle = DashStyle.Dot }
                    };
                    this.plot.Add(line);
                    this.laps = (int)num5;
                    if ((this.plot.YAxis1.WorldMax - this.plot.YAxis1.WorldMin) <= 0.1)
                    {
                        Axis axis1 = this.plot.YAxis1;
                        axis1.WorldMax++;
                    }
                    this.plot.Refresh();
                }
            }
        }

        public void Save(string outfile, int width, int height)
        {
            int num = (this.laps <= 1) ? ((this.laps * 0x18) + 50) : ((this.laps * 0x13) + 50);
            Bitmap image = new Bitmap(num, Settings.graphWidth);
            this.plot.Draw(Graphics.FromImage(image), new Rectangle(0, 0, image.Width, image.Height));
            try
            {
                image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                image.Save(outfile, ImageFormat.Png);
            }
            catch
            {
                Console.WriteLine("Error! Cannot create " + outfile);
            }
        }

        public void Stats()
        {
            this.laps = 0;
            this.bestlap = 0x3fff0001L;
            for (int i = 0; i < this.players.Count; i++)
            {
                long num2 = 0L;
                int num3 = 1;
                if (((long[])this.splits[i]).Length >= this.nsplits)
                {
                    for (int j = this.nsplits; j < ((long[])this.splits[i]).Length; j++)
                    {
                        num2 += ((long[])this.splits[i])[j];
                        if (j != 0)
                        {
                            num2 -= ((long[])this.splits[i])[j - 1];
                        }
                        if (num3 != this.nsplits)
                        {
                            num3++;
                        }
                        else
                        {
                            if (this.bestlap > num2)
                            {
                                this.bestlap = num2;
                            }
                            num3 = 1;
                            num2 = 0L;
                        }
                    }
                }
            }
            this.winner = this.player;
            if (((((long[])this.splits[this.winner]).Length - 1) != 0) && ((((long[])this.splits[this.winner]).Length / this.nsplits) != 0))
            {
                this.winner_avgtime = ((long[])this.splits[this.winner])[((long[])this.splits[this.winner]).Length - 1] / ((long)(((long[])this.splits[this.winner]).Length / this.nsplits));
            }
        }

        private string timetostr(long t, bool full)
        {
            long num = (t / ((long)100)) - (((t / ((long)100)) / ((long)100)) * 100);
            long num2 = (t / 0x2710L) / ((long)60);
            long num3 = (t / 0x2710L) % ((long)60);
            if ((num2 == 0L) && !full)
            {
                return ($"{num3:D2}" + "." + $"{num:D2}");
            }
            string[] strArray = new string[] { $"{num2:D1}", ":", $"{num3:D2}", ".", $"{num:D2}" };
            return string.Concat(strArray);
        }
    }
}
