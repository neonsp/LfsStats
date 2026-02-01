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
			if (Settings.ReadSettings("graph.xml"))
			{
				//string currentDir = System.IO.Directory.GetCurrentDirectory();
				string outdir = Settings.GraphOutputDirectory;
				System.IO.Directory.CreateDirectory(outdir);

				DirectoryInfo dir = new DirectoryInfo(Settings.TSVInputDirectory);

				//try
				//{
				for (int i = 0; i < dir.GetFiles("*results_race_extended.tsv").Length; i++)
				{
					string tsvfile = dir.GetFiles("*results_race_extended.tsv")[i].Name;
					string picfile = "";

					FileInfo fitsv = new FileInfo(Settings.TSVInputDirectory + tsvfile);

					if (fitsv.Length > 20)
					{
						picfile = tsvfile.Substring(0, tsvfile.Length - 26);

						string outfilename = outdir + picfile + "_lbl.png";
						FileInfo fi = new FileInfo(outfilename);

						if (!fi.Exists || fi.Length == 0)
						{
							lblData = new LblData(Settings.TSVInputDirectory + tsvfile);
							lblData.Draw();
							lblData.Save(outfilename, Settings.graphWidth, Settings.graphHeight);
						}

						//                            System.Console.WriteLine(Settings.TSVInputDirectory + tsvfile);
					}
				}
				//}
				//catch (System.Exception ex) // assume the directory path error
				//{
				//    System.Console.Error.WriteLine("Graph error.\n");
				//    System.Console.Error.WriteLine(ex.ToString());
				//    // System.Console.WriteLine("Path "+Settings.TSVInputDirectory+" not found");
				//    return;
				//}

				for (int i = 0; i < dir.GetFiles("*results_race_extended.tsv").Length; i++)
				{
					string tsvfile = dir.GetFiles("*results_race_extended.tsv")[i].Name;

					string picfile = "";

					FileInfo fitsv = new FileInfo(Settings.TSVInputDirectory + tsvfile);

					if (fitsv.Length > 20)
					{
						picfile = tsvfile.Substring(0, tsvfile.Length - 26);

						string outfilename = outdir + picfile + "_rpr.png";
						FileInfo fi = new FileInfo(outfilename);

						if (!fi.Exists || fi.Length == 0)
						{
							rprData = new RprData(Settings.TSVInputDirectory + tsvfile);
							rprData.Draw();
							rprData.Save(outfilename, Settings.graphWidth, Settings.graphHeight);
						}
					}

				}

				if (Settings.OutputLapTimes)
					for (int i = 0; i < dir.GetFiles("*results_race_extended.tsv").Length; i++)
					{
						string tsvfile = dir.GetFiles("*results_race_extended.tsv")[i].Name;

						lapData = new LapData(Settings.TSVInputDirectory + tsvfile);

						for (int player = 0; player < lapData.players.Count; ++player)
						{
							string picfile = "";

							FileInfo fitsv = new FileInfo(Settings.TSVInputDirectory + tsvfile);

							if (fitsv.Length > 20)
							{
								picfile = tsvfile.Substring(0, tsvfile.Length - 26);

								string outfilename = outdir + picfile + "_lap_" + System.Convert.ToString(player + 1) + ".png";
								FileInfo fi = new FileInfo(outfilename);

								if (!fi.Exists || fi.Length == 0)
								{
									lapData.player = player;
									lapData.Stats();
									lapData.Draw();
									lapData.Save(outfilename, Settings.graphWidth, Settings.graphHeight);
								}
							}
						}

					}

			}
		}

	}
}
