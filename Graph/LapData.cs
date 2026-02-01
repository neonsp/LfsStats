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

		public int player;

		private int laps;

		private string timetostr(long t, bool full)
		{
			long Hundredths = (t / 100 - (t / 100 / 100) * 100);
			long Minutes = (t / 10000) / 60;
			long Seconds = (t / 10000) % 60;
			if (Minutes == 0 && full == false)
				return String.Format("{0:D2}", Seconds) + "." + String.Format("{0:D2}", Hundredths);
			else
				return String.Format("{0:D1}", Minutes) + ":" + String.Format("{0:D2}", Seconds) + "." + String.Format("{0:D2}", Hundredths);
		}

		public LapData(string fn)
		{
			player = 0;

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
		}

		public void Stats()
		{

			// find absolute best lap

			this.laps = 0;

			bestlap = 32767 * 32767;
			for (int p = 0; p < this.players.Count; ++p)
			{
				long lap = 0;
				int split = 1;
				if (((long[])splits[p]).Length >= nsplits)
					for (int i = nsplits; i < ((long[])splits[p]).Length; ++i)
					{
						lap += ((long[])splits[p])[i];
						if (i != 0) lap -= ((long[])splits[p])[i - 1];
						if (split == nsplits)
						{
							//                    if (lap == 929400) System.Console.WriteLine(System.Convert.ToString(lap) + " " + System.Convert.ToString(i));
							if (bestlap > lap)
							{
								bestlap = lap;
								//                                System.Console.WriteLine(System.Convert.ToString(lap) + " " + System.Convert.ToString(i) + " " + System.Convert.ToString(p));
							}
							split = 1;
							lap = 0;
						}
						else split++;
					}
			}

			//            bestlap = 929400;

			winner = player;
			// find a winner average lap and splits time
			if (((long[])splits[winner]).Length - 1 != 0 && ((long[])splits[winner]).Length / (nsplits) != 0)
				winner_avgtime = (((long[])splits[winner])[((long[])splits[winner]).Length - 1]) / (((long[])splits[winner]).Length / (nsplits));
		}

		public void Draw()
		{
			this.plot.PlotBackColor = Color.Transparent;
			this.plot.BackColor = Color.White;

			ArrayList xs1 = new ArrayList();

			ArrayList times = new ArrayList();

			int split = 1;
			long lap = 0;
			float max = 0;
			float min = 32767 * 32767;
			long nlaps = 0;
			long totaltime = 0;

			this.plot.Clear();

			if (((long[])splits[winner]).Length < nsplits)
				return;

			for (int i = nsplits; i < ((long[])splits[winner]).Length; ++i)
			{
				lap += ((long[])splits[winner])[i];
				if (i != 0) lap -= ((long[])splits[winner])[i - 1];
				if (split == nsplits)
				{
					//times.Add("     " + timetostr(lap));
					string time = "  " + timetostr(lap, true) + "  :  ";
					for (int s = nsplits - 1; s >= 0; --s)
					{
						if (i == nsplits && s == nsplits)
							time += timetostr(((long[])splits[winner])[i - s], false);
						else
							time += timetostr(((long[])splits[winner])[i - s] - ((long[])splits[winner])[i - 1 - s], false);
						if (s != 0)
							time += " , ";
					}

					time += "  ";
					times.Add(time);
					totaltime += lap;

					++nlaps;
					if (max < lap) max = lap;
					if (min > lap) min = lap;
					xs1.Add((double)lap);
					split = 1;
					lap = 0;
				}
				else split++;
			}

			if (nlaps != 0)
				this.winner_avgtime = totaltime / nlaps;

			if (nlaps == 0)
				return;

			Grid mygrid = new Grid();
			mygrid.VerticalGridType = Grid.GridType.Coarse;
			mygrid.HorizontalGridType = Grid.GridType.Coarse;
			mygrid.MajorGridPen = new Pen(Color.LightGray, 1f);

			this.plot.Add(mygrid);

			for (int i = 0; i < xs1.Count; ++i)
			{
				double[] abscissa = { 0 };
				double[] ordinate = { 0 };

				if ((double)xs1[i] < winner_avgtime)
				{
					abscissa[0] = (i);
					ordinate[0] = ((double)xs1[i]) / 10000.0;
					HistogramPlot hp = new HistogramPlot();
					hp.OrdinateData = ordinate;
					hp.AbscissaData = abscissa;

					hp.RectangleBrush = new RectangleBrushes.Horizontal(Color.FromArgb(106, 205, 84), Color.FromArgb(235, 255, 213));
					hp.Pen.Color = Color.FromArgb(0, 150, 0);
					hp.Filled = true;
					hp.ShowInLegend = false;
					this.plot.Add(hp);
				}

				if ((double)xs1[i] >= winner_avgtime)
				{
					abscissa[0] = i;
					if (Settings.LimitLapTimes && (((double)xs1[i] - winner_avgtime) > (winner_avgtime - min) * Settings.LimitMultiplier))
						ordinate[0] = (winner_avgtime + (winner_avgtime - min) * Settings.LimitMultiplier) / 10000.0;
					else
						ordinate[0] = ((double)xs1[i]) / 10000.0;

					HistogramPlot hp = new HistogramPlot();
					hp.OrdinateData = ordinate;
					hp.AbscissaData = abscissa;

					if (Settings.LimitLapTimes && (((double)xs1[i] - winner_avgtime) > (winner_avgtime - min) * Settings.LimitMultiplier))
					{
						hp.RectangleBrush = new RectangleBrushes.Horizontal(Color.FromArgb(190, 39, 92), Color.FromArgb(235, 124, 177));
					}
					else
					{
						hp.RectangleBrush = new RectangleBrushes.Horizontal(Color.FromArgb(235, 84, 137), Color.FromArgb(255, 230, 210));
					}
					hp.Pen.Color = Color.FromArgb(150, 0, 0);
					hp.Filled = true;
					hp.ShowInLegend = false;
					this.plot.Add(hp);
				}

			}

			//int xmax = ((long[])splits[winner]).Length / nsplits;

			LabelAxis la = new LabelAxis(this.plot.XAxis1);
			la.TicksBetweenText = false;
			la.TicksCrossAxis = false;
			la.LargeTickSize = 0;
			la.TickTextFont = Settings.lapTimesFont;

			for (int i = 0; i < times.Count; ++i)
				la.AddLabel((string)times[i], i);

			la.TicksLabelAngle = -90.0f;
			this.plot.XAxis1 = la;

			la = new LabelAxis((LabelAxis)la.Clone());
			la.TicksBetweenText = false;
			la.TicksCrossAxis = true;
			la.LargeTickSize = 2;
			la.TicksLabelAngle = -90.0f;
			la.TickTextNextToAxis = false;
			la.TickTextFont = Settings.commonFont;

			for (int i = 0; i < times.Count; ++i)
			{
				la.AddLabel(System.Convert.ToString(i + 2), i);
			}
			la.LabelFont = Settings.commonFont;
			this.plot.XAxis2 = la;

			this.plot.YAxis1.TicksCrossAxis = true;

			this.plot.YAxis1.Label = ((string)this.players[this.player]);
			this.plot.YAxis1.LabelFont = Settings.titleFont;
			this.plot.YAxis1.LabelOffset = 20;
			this.plot.YAxis1.NumberFormat = "";
			this.plot.YAxis1.TicksCrossAxis = false;
			if (Settings.LimitToGlobalBestLap) this.plot.YAxis1.WorldMin = (double)this.bestlap / 10000.0;
			((LinearAxis)this.plot.YAxis1).NumberOfSmallTicks = 4;
			((LinearAxis)this.plot.YAxis1).LargeTickStep = 1;
			((LinearAxis)this.plot.YAxis1).TicksLabelAngle = -90f;

			HorizontalLine hl = new HorizontalLine((float)winner_avgtime / 10000, Color.Gray);
			hl.Pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
			this.plot.Add(hl);

			this.laps = (int)nlaps;

			//            this.plot.YAxis1.WorldMax += 1;
			if (this.plot.YAxis1.WorldMax - this.plot.YAxis1.WorldMin <= 0.1)
				this.plot.YAxis1.WorldMax += 1;
			this.plot.Refresh();
			//            System.Console.WriteLine(System.Convert.ToString(this.plot.YAxis1.WorldMin) + " " + System.Convert.ToString(this.plot.YAxis1.WorldMax));
		}

		public void Save(string outfile, int width, int height)
		{
			int w;

			if (this.laps > 1)
				w = this.laps * 19 + 50;
			else
				w = this.laps * (int)(19 * 1.3) + 50;

			Bitmap b = new Bitmap(w, Settings.graphWidth);
			this.plot.Draw(Graphics.FromImage(b), new System.Drawing.Rectangle(0, 0, b.Width, b.Height));
			try
			{
				b.RotateFlip(RotateFlipType.Rotate90FlipNone);
				b.Save(outfile, System.Drawing.Imaging.ImageFormat.Png);
			}
			catch
			{
				System.Console.WriteLine("Error! Cannot create " + outfile);

			};
		}

	}
}
