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
using System.Windows.Forms;
using static NPlot.Grid;

namespace Graph
{
    /// <summary>
    /// Summary description for LblData.
    /// </summary>

    internal class LblData
    {
        private string filename;

        private NPlot.Windows.PlotSurface2D plot;

        private ArrayList players;

        private ArrayList splits;

        private int xmax = 30;

        private int nsplits;

        private int winner;

        private long winner_avgtime;

        private long[] winner_avgsplits;

        private double[] winner_avgsplitsproportions;

        public LblData(string fn)
        {
            //IL_0016: Unknown result type (might be due to invalid IL or missing references)
            //IL_0020: Expected O, but got Unknown
            filename = fn;
            plot = new NPlot.Windows.PlotSurface2D();
            FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            StreamReader streamReader = new StreamReader(fileStream);
            string text = "";
            players = new ArrayList();
            splits = new ArrayList();
            nsplits = Convert.ToInt32(streamReader.ReadLine());
            char[] separator = new char[1] { '\t' };
            int num = 1;
            while ((text = streamReader.ReadLine()) != null)
            {
                string[] array = text.Split(separator);
                players.Add(array[0]);
                splits.Add(new ArrayList());
                splits[splits.Count - 1] = new long[array.Length];
                for (int i = 0; i < array.Length; i++)
                {
                    if (i == 0)
                    {
                        ((long[])splits[splits.Count - 1])[i] = Convert.ToInt32(num++);
                        continue;
                    }

                    string value = array[i];
                    ((long[])splits[splits.Count - 1])[i] = Convert.ToInt32(value);
                }
            }

            fileStream.Close();
            Stats();
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

        public void Draw()
        {
            //IL_000b: Unknown result type (might be due to invalid IL or missing references)
            //IL_0011: Expected O, but got Unknown
            //IL_0099: Unknown result type (might be due to invalid IL or missing references)
            //IL_009f: Expected O, but got Unknown
            //IL_00ef: Unknown result type (might be due to invalid IL or missing references)
            //IL_00f5: Expected O, but got Unknown
            //IL_015a: Unknown result type (might be due to invalid IL or missing references)
            //IL_0161: Expected O, but got Unknown
            //IL_01d8: Unknown result type (might be due to invalid IL or missing references)
            //IL_01df: Expected O, but got Unknown
            //IL_027e: Unknown result type (might be due to invalid IL or missing references)
            //IL_0285: Expected O, but got Unknown
            //IL_029c: Unknown result type (might be due to invalid IL or missing references)
            //IL_0430: Unknown result type (might be due to invalid IL or missing references)
            //IL_0437: Expected O, but got Unknown
            plot.Clear();
            Grid val = new Grid();
            val.VerticalGridType = (GridType)1;
            val.HorizontalGridType = (GridType)1;
            val.MajorGridPen = new Pen(Color.LightGray, 1f);
            plot.Add((IDrawable)(object)val);
            xmax = ((long[])splits[winner]).Length / nsplits;
            int num = 1;
            if (xmax <= 30)
            {
                num = 1;
            }
            else if (xmax > 30 && xmax <= 60)
            {
                num = 2;
            }
            else if (xmax > 60)
            {
                num = 3;
            }

            LinearAxis val2 = new LinearAxis();
            val2.NumberOfSmallTicks = 0;
            val2.LargeTickStep = num;
            ((Axis)val2).HideTickText = true;
            ((Axis)val2).WorldMin = 0.0;
            ((Axis)val2).WorldMax = xmax;
            ((Axis)val2).TicksCrossAxis = true;
            plot.XAxis1 = (Axis)(object)val2;
            LinearAxis val3 = new LinearAxis(plot.XAxis1);
            ((Axis)val3).Label = Settings.LapByLapGraphXAxisLabel;
            val3.NumberOfSmallTicks = 0;
            val3.LargeTickStep = num;
            ((Axis)val3).WorldMin = 0.0;
            ((Axis)val3).WorldMax = xmax;
            ((Axis)val3).HideTickText = false;
            ((Axis)val3).LabelFont = Settings.commonFont;
            plot.XAxis2 = (Axis)(object)val3;
            plot.XAxis1.LargeTickSize = 0;
            LinearAxis val4 = new LinearAxis();
            ((Axis)val4).Label = Settings.LapByLapGraphYAxisLabel;
            val4.NumberOfSmallTicks = 0;
            ((Axis)val4).Reversed = true;
            val4.LargeTickStep = 1.0;
            ((Axis)val4).WorldMin = 0.5;
            ((Axis)val4).WorldMax = (float)players.Count + 0.5f;
            ((Axis)val4).LabelFont = Settings.commonFont;
            ((Axis)val4).TicksCrossAxis = true;
            plot.YAxis1 = (Axis)(object)val4;
            LinearAxis val5 = new LinearAxis();
            val5.NumberOfSmallTicks = 0;
            ((Axis)val5).Reversed = true;
            val5.LargeTickStep = 1.0;
            ((Axis)val5).WorldMin = 0.5;
            ((Axis)val5).WorldMax = (float)players.Count + 0.5f;
            ((Axis)val5).LabelFont = Settings.commonFont;
            ((Axis)val5).TickTextNextToAxis = false;
            ((Axis)val5).Label = "";
            ((Axis)val5).TicksCrossAxis = true;
            plot.YAxis2 = (Axis)(object)val5;
            plot.Title = Settings.LapByLapGraphTitle;
            plot.TitleFont = Settings.titleFont;
            Legend val6 = new Legend();
            ((LegendBase)val6).Font = Settings.commonFont;
            val6.XOffset = 30;
            ((LegendBase)val6).BorderStyle = Settings.legendBorderType;
            plot.Legend = val6;
            plot.PlotBackColor = Color.White;
            this.plot.BackColor = Color.White;
            for (int i = 0; i < players.Count; i++)
            {
                int[] array = new int[((long[])splits[i]).Length];
                double[] array2 = new double[((long[])splits[i]).Length];
                int num2 = 0;
                int num3 = 0;
                for (int j = 0; j < ((long[])splits[winner]).Length; j++)
                {
                    if (j < array.Length)
                    {
                        double num4 = 0.0;
                        for (int k = 0; k < num2; k++)
                        {
                            num4 += winner_avgsplitsproportions[k + 1];
                        }

                        array2[j] = (double)num3 + num4;
                    }

                    int num5 = 1;
                    for (int l = 0; l < players.Count; l++)
                    {
                        if (l != i && ((long[])splits[i]).Length > j && ((long[])splits[l]).Length > j && ((long[])splits[l])[j] < ((long[])splits[i])[j])
                        {
                            num5++;
                        }
                    }

                    if (j < array.Length)
                    {
                        array[j] = num5;
                    }

                    num2++;
                    if (num2 == nsplits)
                    {
                        num2 = 0;
                        num3++;
                    }
                }

                LinePlot val7 = new LinePlot();
                ((BaseSequencePlot)val7).AbscissaData = array2;
                ((BaseSequencePlot)val7).OrdinateData = array;
                val7.Pen = new Pen(Colors.GetColor(i), 2f);
                int num6 = i + 1;
                int num7 = num6;
                if (array.Length - 1 >= 0)
                {
                    num7 = array[array.Length - 1];
                }

                if (Settings.LapByLapGraphDisplayPositions)
                {
                    ((BasePlot)val7).Label = $"{num6:00}-{num7:00} {players[i].ToString():g}";
                }
                else
                {
                    ((BasePlot)val7).Label = $"{players[i].ToString():g}";
                }

                plot.Add((IDrawable)(object)val7);
                plot.YAxis1.WorldMin = 0.0;
                plot.YAxis2.WorldMin = 0.0;
                plot.XAxis1.WorldMax = xmax;
                plot.XAxis2.WorldMax = xmax;
            }

            plot.YAxis1.WorldMax = (float)players.Count + 0.5f;
            ((Axis)val2).WorldMin = 0.0;
            ((Axis)val2).WorldMax = xmax;
            ((Axis)val3).WorldMin = 0.0;
            ((Axis)val3).WorldMax = xmax;
            plot.Refresh();
        }

        public void Save(string outfile, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height);
            plot.Draw(Graphics.FromImage(bitmap), new Rectangle(0, 0, bitmap.Width, bitmap.Height));
            try
            {
                bitmap.Save(outfile, ImageFormat.Png);
            }
            catch
            {
                Console.WriteLine("Error! Cannot create " + outfile);
            }
        }
    }
}
