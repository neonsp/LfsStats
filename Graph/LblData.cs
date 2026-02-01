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

		public LblData(string fn)//, NPlot.Windows.PlotSurface2D pt)
		{
			filename = fn;
			plot = new NPlot.Windows.PlotSurface2D();// pt;

			// read file data
			FileStream file = new FileStream(filename, FileMode.Open, FileAccess.Read);
			StreamReader sr = new StreamReader(file);
			string line = "";

			players = new ArrayList();
			splits = new ArrayList();

			nsplits = Convert.ToInt32(sr.ReadLine());

			char[] splitter = { '\t' };
			int playernum = 1;
			while ((line = sr.ReadLine()) != null)
			{
				string[] result = line.Split(splitter);
				players.Add(result[0]);

				splits.Add(new ArrayList());
				/*
								splits[splits.Count-1] = new long[result.Length-1];
								for (int i = 1; i < result.Length; ++i)
								{
									string num = result[i];
									((long [])splits[splits.Count-1])[i-1] = Convert.ToInt32(num);
								}
				*/
				splits[splits.Count - 1] = new long[result.Length];
				for (int i = 0; i < result.Length; ++i)
					if (i == 0)
					{
						((long[])splits[splits.Count - 1])[i] = Convert.ToInt32(playernum++);
					}
					else
					{
						string num = result[i];
						((long[])splits[splits.Count - 1])[i] = Convert.ToInt32(num);
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
				//				|| len != -1 && ((long [])splits[p])[len] <= fastest_time && len > winner_splits)
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

			xmax = ((long[])splits[winner]).Length / nsplits;
			int tickstep = 1;
			if (xmax <= 30)
				tickstep = 1;
			else
				if (xmax > 30 && xmax <= 60)
					tickstep = 2;
				else
					if (xmax > 60)
						tickstep = 3;

			LinearAxis lx1 = new LinearAxis();

			/*			if (tickstep > 1)
							lx1.NumberOfSmallTicks = 0;
						else
							lx1.NumberOfSmallTicks = nsplits -1; 
			*/
			lx1.NumberOfSmallTicks = 0;
			lx1.LargeTickStep = tickstep;
			lx1.HideTickText = true;
			lx1.WorldMin = 0;
			lx1.WorldMax = xmax;
			lx1.TicksCrossAxis = true;
			this.plot.XAxis1 = lx1;

			LinearAxis lx2 = new LinearAxis(this.plot.XAxis1);
			lx2.Label = Settings.LapByLapGraphXAxisLabel;
			lx2.NumberOfSmallTicks = 0;
			lx2.LargeTickStep = tickstep;
			lx2.WorldMin = 0;
			lx2.WorldMax = xmax;
			lx2.HideTickText = false;
			lx2.LabelFont = Settings.commonFont;
			this.plot.XAxis2 = lx2;
			this.plot.XAxis1.LargeTickSize = 0;

			LinearAxis ly1 = new LinearAxis();
			ly1.Label = Settings.LapByLapGraphYAxisLabel;
			ly1.NumberOfSmallTicks = 0;
			ly1.Reversed = true;
			ly1.LargeTickStep = 1;
			ly1.WorldMin = 0.5f;
			ly1.WorldMax = players.Count + 0.5f;
			ly1.LabelFont = Settings.commonFont;
			ly1.TicksCrossAxis = true;
			this.plot.YAxis1 = ly1;

			LinearAxis ly2 = new LinearAxis();
			ly2.NumberOfSmallTicks = 0;
			ly2.Reversed = true;
			ly2.LargeTickStep = 1;
			ly2.WorldMin = 0.5f;
			ly2.WorldMax = players.Count + 0.5f;
			ly2.LabelFont = Settings.commonFont;
			ly2.TickTextNextToAxis = false;
			ly2.Label = "";
			ly2.TicksCrossAxis = true;
			this.plot.YAxis2 = ly2;

			this.plot.Title = Settings.LapByLapGraphTitle;
			this.plot.TitleFont = Settings.titleFont;

			Legend lg = new Legend();
			lg.Font = Settings.commonFont;
			lg.XOffset = 30;
			lg.BorderStyle = Settings.legendBorderType;
			lg.BackgroundColor = Color.Transparent;
			lg.TextColor = Color.Black;
			lg.BorderColor = Color.Black;
			this.plot.Legend = lg;

			this.plot.PlotBackColor = Color.Transparent;
			this.plot.BackColor = Color.White;

			//			double minx = 0;

			for (int i = 0; i < players.Count; ++i)
			//			for (int i=winner; i<= winner; ++i)
			{
				int[] positions = new int[((long[])splits[i]).Length];
				double[] laps = new double[((long[])splits[i]).Length];
				int splitN = 0;
				int l = 0;

				for (int split = 0; split < ((long[])splits[winner]).Length; ++split)
				{

					if (split < positions.Length)
					{
						double prop = 0;
						for (int pr = 0; pr < splitN; ++pr)
							prop += winner_avgsplitsproportions[pr + 1];
						laps[split] = (double)l + prop;
					}

					int pos = 1;

					for (int p = 0; p < players.Count; ++p)
						if (p != i && ((long[])(splits[i])).Length > split && ((long[])(splits[p])).Length > split && ((long[])(splits[p]))[split] < ((long[])(splits[i]))[split])
							++pos;

					if (split < positions.Length)
						positions[split] = pos;

					++splitN;
					if (splitN == nsplits)
					{
						splitN = 0;
						++l;
					}

				}


				LinePlot lp = new LinePlot();
				//				lp.DataSource = positions;
				lp.AbscissaData = laps;
				lp.OrdinateData = positions;

				lp.Pen = new Pen(Colors.GetColor(i), 2.0f);
				int start = i + 1;
				int end = start;
				if (positions.Length - 1 >= 0)
					end = positions[positions.Length - 1];

				if (Settings.LapByLapGraphDisplayPositions)
					lp.Label = String.Format("{0:00}-{1:00} {2:g}", start, end, players[i].ToString());
				else
					lp.Label = String.Format("{0:g}", players[i].ToString());

				this.plot.Add(lp);
				this.plot.YAxis1.WorldMin = 0;
				this.plot.YAxis2.WorldMin = 0;

				/*
								Marker mk = new Marker();
								mk.Color = System.Drawing.Color.Red;
				//                mk.Type = Marker.MarkerType.FlagUp;
								mk.Type = Marker.MarkerType.FlagUp;
								mk.Size = 15;
								int[] abs = {1,2,3,4,5,6,7,8,9,10};
								int[] ord = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
								abs.Add
								PointPlot pp = new PointPlot(mk);
								pp.ShowInLegend = false;
								pp.AbscissaData = abs;
								pp.OrdinateData = ord;
								this.plot.Add(pp);
				*/

				this.plot.XAxis1.WorldMax = xmax;
				this.plot.XAxis2.WorldMax = xmax;

			}

			this.plot.YAxis1.WorldMax = players.Count + 0.5f;

			lx1.WorldMin = 0;
			lx1.WorldMax = xmax;
			lx2.WorldMin = 0;
			lx2.WorldMax = xmax;

			this.plot.Refresh();
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
