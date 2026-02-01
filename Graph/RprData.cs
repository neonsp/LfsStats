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
using System;
using System.Collections;
using System.Drawing;
using System.IO;
using NPlot;

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
			filename = fn;
			plot = new NPlot.Windows.PlotSurface2D();

			// read file data
			FileStream file = new FileStream(filename, FileMode.Open, FileAccess.Read);
			StreamReader sr = new StreamReader(file);
			string line = "";

			players = new ArrayList();
			splits = new ArrayList();

			nsplits = Convert.ToInt32(sr.ReadLine());

			char[] splitter = { '\t' };
			while ((line = sr.ReadLine()) != null)
			{
				string[] result = line.Split(splitter);
				players.Add(result[0]);

				splits.Add(new ArrayList());
				splits[splits.Count - 1] = new long[result.Length - 1];
				for (int i = 1; i < result.Length; ++i)
				{
					string num = result[i];
					((long[])splits[splits.Count - 1])[i - 1] = Convert.ToInt32(num);
				}
			}

			file.Close();

			this.Stats();
		}

		private void Stats()
		{
			// find a winner
			winner = 0;
			int winner_splits = 0;
			long fastest_time = 32767 * 32767 * 2;
			for (int p = 0; p < players.Count; ++p)
			{
				int len = ((long[])splits[p]).Length - 1;
				if (((long[])splits[winner]).Length - 1 <= 0 && ((long[])splits[p]).Length - 1 > 0
				|| len != -1 && ((long[])splits[p])[len] <= fastest_time && len >= winner_splits
				|| len != -1 && ((long[])splits[p])[len] >= fastest_time && len > winner_splits)
				{
					winner = p;
					winner_splits = len;
					fastest_time = ((long[])splits[p])[len];
				}
			}

			// find a winner average lap and splits time
			winner_avgtime = (((long[])splits[winner])[((long[])splits[winner]).Length - 1]) / (((long[])splits[winner]).Length / (nsplits));
			winner_avgsplits = new long[nsplits + 1];
			winner_avgsplitsproportions = new double[nsplits + 1];

			for (int s = 0; s < nsplits; ++s)
			{
				int n = 0;
				for (int i = s; i < ((long[])splits[winner]).Length; i = i + nsplits)
				{
					if (i == 0)
						winner_avgsplits[s + 1] += ((long[])splits[winner])[i];
					else
						winner_avgsplits[s + 1] += ((long[])splits[winner])[i] - ((long[])splits[winner])[i - 1];
					++n;

				}
				winner_avgsplits[s + 1] /= n;
			}

			for (int s = 0; s <= nsplits; ++s)
			{
				winner_avgsplitsproportions[s] = (double)winner_avgsplits[s] / (double)winner_avgtime;
			}
		}

		public void Draw()
		{
			this.plot.Clear();

			Grid testGrid = new Grid();
			testGrid.VerticalGridType = Grid.GridType.Coarse;
			testGrid.HorizontalGridType = Grid.GridType.Coarse;
			testGrid.MajorGridPen = new Pen(Color.LightGray, 1f);

			this.plot.Add(testGrid);

			int tickstep = 1;

			int xmax = ((long[])splits[winner]).Length / nsplits;

			if (xmax <= 30)
				tickstep = 1;
			else
				if (xmax > 30 && xmax <= 60)
					tickstep = 2;
				else
					if (xmax > 60)
						tickstep = 3;

			LinearAxis lx1 = new LinearAxis();
			lx1.NumberOfSmallTicks = 0;
			lx1.LargeTickStep = tickstep;
			lx1.HideTickText = true;
			lx1.WorldMin = 1;
			lx1.WorldMax = xmax;
			this.plot.XAxis1 = lx1;

			LinearAxis lx2 = new LinearAxis(lx1);
			lx2.Label = Settings.RaceProgressGraphXAxisLabel;
			lx2.NumberOfSmallTicks = 0;
			lx2.LargeTickStep = tickstep;
			lx2.WorldMin = 1;
			lx2.WorldMax = xmax;
			lx2.HideTickText = false;
			lx2.LabelFont = Settings.commonFont;
			this.plot.XAxis2 = lx2;

			LinearAxis ly1 = new LinearAxis();
			ly1.NumberOfSmallTicks = 0;
			ly1.Reversed = true;
			ly1.LargeTickStep = 10;
			ly1.WorldMin = -5;
			ly1.WorldMax = 5;
			ly1.LabelFont = Settings.commonFont;
			ly1.HideTickText = true;
			this.plot.YAxis1 = ly1;

			this.plot.Title = Settings.RaceProgressGraphTitle;
			this.plot.TitleFont = Settings.titleFont;

			Legend lg = new Legend();
			lg.Font = Settings.commonFont;
			lg.BorderStyle = Settings.legendBorderType;
			lg.XOffset = 60;

			this.plot.Legend = lg;

			this.plot.PlotBackColor = Color.White;
			this.plot.BackColor = Color.White;

			this.plot.Add(new HorizontalLine(0.0, Color.Black));

			for (int p = 0; p < players.Count; ++p)
				if (((long[])splits[p]).Length >= nsplits)
				{
					double[] x = new double[((long[])splits[p]).Length - nsplits + 2];
					double[] y = new double[((long[])splits[p]).Length - nsplits + 2];
					x[0] = 1;
					y[0] = 0;

					int l = 1;
					int i = 0;
					int split = 0;
					double lap;

					while (i < ((long[])splits[p]).Length - nsplits + 1)
					{
						double prop = 0;
						for (int pr = 0; pr < split; ++pr)
							prop += winner_avgsplitsproportions[pr + 1];
						lap = (double)l + prop;

						x[i + 1] = lap;

						long splitsum = 0;

						for (int sps = 0; sps <= split; ++sps) splitsum += winner_avgsplits[sps];

						long real = ((long[])splits[p])[i + nsplits - 1];
						long avg = (winner_avgtime * (l) + splitsum);
						long difference = real - avg;
						y[i + 1] = difference;

						y[i + 1] /= 10000;

						if (y[i + 1] > Settings.graphMinMax)
							y[i + 1] = Settings.graphMinMax;

						++split;
						if (split == nsplits)
						{
							split = 0;
							++l;
						}
						++i;

					}

					LinePlot lp = new LinePlot();
					lp.AbscissaData = x;
					lp.OrdinateData = y;
					lp.Pen = new Pen(Colors.GetColor(p), 1f);
					lp.Label = String.Format("{0}", players[p].ToString());

					this.plot.Add(lp);

					LinearAxis ly2 = new LinearAxis(plot.YAxis1);
					ly2.Label = "";
					ly2.Label = Settings.RaceProgressGraphYAxisLabel;
					ly2.NumberOfSmallTicks = ly1.NumberOfSmallTicks;
					ly2.LargeTickStep = ly1.LargeTickStep;
					ly2.TickTextNextToAxis = false;
					ly2.LabelFont = Settings.commonFont;
					ly2.HideTickText = false;
					this.plot.YAxis2 = ly2;

					if (this.plot.YAxis1.WorldMin < 400)
					{

					}

					this.plot.Refresh();

				}

		}

		public void Save(string outfile, int width, int height)
		{
			Bitmap b = new Bitmap(width, height);
			//			System.IO.MemoryStream stream = new System.IO.MemoryStream();
			this.plot.Draw(Graphics.FromImage(b), new System.Drawing.Rectangle(0, 0, b.Width, b.Height));
			try
			{
				b.Save(outfile, System.Drawing.Imaging.ImageFormat.Png);
			}
			catch
			{
				System.Console.WriteLine("Error! Cannot create " + outfile);

			};

		}

	}
}
