/*
	LFS_Stats, Insim Replay statistics for Live For Speed Game
	Copyright (C) 2008 Jaroslav Èerný alias JackCY, Robert B. alias Gai-Luron and Monkster.
	Jack.SC7@gmail.com, lfsgailuron@free.fr

	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either Version 3 of the License, or
	(at your option) any later Version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using DotNetExtensions;
using InSimDotNet.Packets;

namespace LFSStatistics
{

	class ExportStats
	{
		public const string htmlDotExtension = ".html";
		public static Dictionary<string, string> lang = new Dictionary<string, string>();
		public static long combinedBestLap = 0;
		public static string[] strWind = new string[3];
		public static string[] strWeather = new string[11];

		public static bool isBitSet(int var, int bit)
		{
			int bittest = (int)System.Math.Pow(2, bit);
			if ((var & bittest) == bittest)
				return true;
			return false;

		}
		public static bool isBitSet(long var, int bit)
		{
			long bittest = (long)System.Math.Pow(2, bit);
			if ((var & bittest) == bittest)
				return true;
			return false;

		}

		// TODO text colors
		// TODO InsimDotNET network, try use new packets to get more precise replay fast forwarding physics and to get layout names IS_AXI.LName

		public static string lfsStripColor(string str)
		{
			if (string.IsNullOrWhiteSpace(str)) return str;
			return InSimDotNet.Helpers.StringHelper.StripColors(str);
		}
		public static string lfsChatTextToHtml(string str)
		{
			return str.Replace("<", "&lt;")
						.Replace(">", "&gt;")
						.Replace("^^", "&circ;")
						.Replace("^E", "");
		}
		public static string lfsColorToHtml(string str)
		{
			int origLen = str.Length;

			str = str.Replace("^0", "</span><span class=\"black\">")
					.Replace("^1", "</span><span class=\"red\">")
					.Replace("^2", "</span><span class=\"green\">")
					.Replace("^3", "</span><span class=\"yellow\">")
					.Replace("^4", "</span><span class=\"blue\">")
					.Replace("^5", "</span><span class=\"pink\">")
					.Replace("^6", "</span><span class=\"turquoise\">")
					.Replace("^7", "</span><span class=\"white\">")
					.Replace("^8", "</span><span class=\"black\">")
					.Replace("^9", "</span><span class=\"black\">");
			if (str.Length > origLen)
				str = "<span>" + str + "</span>";

			str = str.Replace("^a", "*")
					.Replace("^c", ":")
					.Replace("^d", "\\")
					.Replace("^l", "&lt;")
					.Replace("^q", "?")
					.Replace("^r", "&gt;")
					.Replace("^s", "/")
					.Replace("^t", "\"")
					.Replace("^v", "|")
					.Replace("^^", "&circ;")
					.Replace("^L", "")
					.Replace("^G", "")
					.Replace("^C", "")
					.Replace("^J", "")
					.Replace("^E", "")
					.Replace("^M", "")
					.Replace("^T", "")
					.Replace("^B", "");
			return str;
		}
		public static void getLang(string theLang)
		{
			string readLine;
			int linecounter = 0;
			string val;
			string key;

			string scriptfilename = "./lang/" + theLang + ".txt";
			using (StreamReader sr = new StreamReader(scriptfilename))
			{
				while (true)
				{
					linecounter++;
					readLine = sr.ReadLine();
					if (readLine == null)
						//goto endlang;
						break;
					if (readLine.Trim() == "")
						continue;
					int index = readLine.IndexOf('='); //look for first "="
					if (index == -1)
						throw new System.Exception(string.Format("Corrupted line #{2} ('{1}') in file {0} (can not find '=' symbol)", scriptfilename, readLine, linecounter));

					key = readLine.Substring(0, index).Trim();
					val = readLine.Substring(index + 1).Trim();
					lang[key] = val;
				}
			}
			//endlang:
			strWind[0] = lang["lg_wind0"];
			strWind[1] = lang["lg_wind1"];
			strWind[2] = lang["lg_wind2"];
			strWeather[0] = lang["lg_weather0"];
			strWeather[1] = lang["lg_weather1"];
			strWeather[2] = lang["lg_weather2"];
			strWeather[3] = lang["lg_weather3"];
			strWeather[4] = lang["lg_weather4"];
			strWeather[5] = lang["lg_weather5"];
			strWeather[6] = lang["lg_weather6"];
			strWeather[7] = lang["lg_weather7"];
			strWeather[8] = lang["lg_weather8"];
			strWeather[9] = lang["lg_weather9"];
			strWeather[10] = lang["lg_weather10"];
		}

		//FIXME CsvResult
		public static void CsvResult(List<SessionStats> raceStatsList, string fileName, SessionInfo sessionInfo, Configuration config)
		{
			string formatLine, outputDir;
			int firstMaxLap = 0;
			long firstTotalTime = 0;

			switch (sessionInfo.session)
			{
				case SessionInfo.Session.None:
				default:
					return;
				case SessionInfo.Session.Practice:
					outputDir = config.PracDir;
					break;
				case SessionInfo.Session.Qualification:
					outputDir = config.QualDir;
					break;
				case SessionInfo.Session.Race:
					outputDir = config.RaceDir;
					break;
			}
			Directory.CreateDirectory(outputDir);

			using (StreamReader sr = new StreamReader("templates/csv_race.csv"))
			{
				formatLine = sr.ReadLine();
				if (formatLine == null)
					formatLine = "[RaceResults {Position},{PlayerName},{UserName},{Car},{Gap},{BestLap},{LapsDone},{PitsDone},{Penalty},{Flags}]";
			}

			raceStatsList.Sort();
			using (StreamWriter sw = new StreamWriter(outputDir + "/" + fileName + "_results_race.csv"))	// NEXT P,Q,R
			{
				int curPos = 0;
				for (int i = 0; i < raceStatsList.Count; i++)
				{
					curPos++;
					SessionStats p = raceStatsList[i];
					if (i == 0)
					{
						firstMaxLap = p.lap.Count;
						firstTotalTime = p.totalTime;
					}
					string resultLine = formatLine;
					resultLine = resultLine.Replace("[RaceResults", "");
					resultLine = resultLine.Replace("]", "");
					resultLine = resultLine.Replace("{Position}", curPos.ToString());
					//                    resultLine = resultLine.Replace("{Position}", p.resultNum.ToString() );
					resultLine = resultLine.Replace("{PlayerName}", lfsStripColor(p.NickName));
					resultLine = resultLine.Replace("{UserName}", p.userName);
					resultLine = resultLine.Replace("{Car}", p.carName);
					// if Racer do not finish
					if (p.resultNum == 999)
						resultLine = resultLine.Replace("{Gap}", "DNF");
					else
					{
						if (firstMaxLap == p.lap.Count)
						{
							if (i == 0)
								resultLine = resultLine.Replace("{Gap}", SessionStats.LfsTimeToString(p.totalTime));
							else
							{
								long tres;
								tres = p.totalTime - firstTotalTime;
								resultLine = resultLine.Replace("{Gap}", "+" + SessionStats.LfsTimeToString(tres));
							}
						}
						else
							resultLine = resultLine.Replace("{Gap}", "+" + ((int)(firstMaxLap - p.lap.Count)).ToString() + " laps");
					}
					resultLine = resultLine.Replace("{BestLap}", SessionStats.LfsTimeToString(p.bestLap));
					resultLine = resultLine.Replace("{LapsDone}", p.lap.Count.ToString());
					resultLine = resultLine.Replace("{PitsDone}", p.numStop.ToString());
					resultLine = resultLine.Replace("{Penalty}", p.penalty);
					resultLine = resultLine.Replace("{PosGrid}", p.gridPos.ToString());
					resultLine = resultLine.Replace("{Flags}", p.sFlags);
					sw.WriteLine(resultLine);
				}
			}
		}
		//FIXME TsvResult
		public static void TsvResult(List<SessionStats> raceStatsList, string fileName, SessionInfo sessionInfo, Configuration config)
		{
			string outputDir;
			switch (sessionInfo.session)
			{
				case SessionInfo.Session.None:
				default:
					return;
				case SessionInfo.Session.Practice:
					outputDir = config.PracDir;
					break;
				case SessionInfo.Session.Qualification:
					outputDir = config.QualDir;
					break;
				case SessionInfo.Session.Race:
					outputDir = config.RaceDir;
					break;
			}
			Directory.CreateDirectory(outputDir);

			raceStatsList.Sort(SessionStats.GetGridCmp());
			using (StreamWriter sw = new StreamWriter(outputDir + "/" + fileName + "_results_race_extended.tsv"))	// NEXT P,Q,R
			{
				sw.WriteLine(sessionInfo.maxSplit + 1);

				for (int i = 0; i < raceStatsList.Count; i++)
				{
					SessionStats p = raceStatsList[i];

					sw.Write(lfsStripColor(p.NickName));
					long lastCumul = 0;
					for (int j = 0; j < p.lap.Count; j++)
					{
						// case of last Lap not completed                        
						if ((p.lap[j]).LapTime == 0)
							continue;
						if (sessionInfo.maxSplit >= 1)
						{
							sw.Write("\t" + (lastCumul + (p.lap[j]).Split[0]) * 1);
							//                            sw.Write("(" + LfsTimeToString((p.lap[j]).split1) + "," + LfsTimeToString((p.lap[j]).LapTime) + ")");
						}
						if (sessionInfo.maxSplit >= 2)
						{
							sw.Write("\t" + (lastCumul + (p.lap[j]).Split[1]) * 1);
						}
						if (sessionInfo.maxSplit >= 3)
						{
							sw.Write("\t" + (lastCumul + (p.lap[j]).Split[2]) * 1);
						}
						sw.Write("\t" + (lastCumul + (p.lap[j]).LapTime) * 1);
						lastCumul = lastCumul + (p.lap[j]).LapTime;
					}
					sw.Write("\r\n");
				}
			}
		}

		public static string getAllUserName(SessionStats p)
		{
			string retValue = "";
			string br = "";
			foreach (KeyValuePair<string, UN> pair in p.allUN)
			{
				retValue = retValue + br + pair.Value.userName;
				br = "<BR>";
			}
			return retValue;

		}
		public static string getAllNickName(SessionStats p)
		{
			string retValue = "";
			string br = "";
			foreach (KeyValuePair<string, UN> pair in p.allUN)
			{
				retValue = retValue + br + pair.Value.nickName;
				br = "<BR>";
			}
			return retValue;

		}
		public static void HtmlResult(List<SessionStats> raceStatsList, string fileName, SessionInfo sessionInfo, Configuration config)
		{
			// for testing multi threading, to check if passed data is valid,
			// special care always has to be taken when working with data in parallel
			//System.Threading.Thread.Sleep(2000);
			int curPos;
			LFSWorld.WR wrInfo;
			SessionStats playerStats;
			string outputDir;

			if (!fileName.EndsWith(htmlDotExtension, StringComparison.OrdinalIgnoreCase)) fileName += htmlDotExtension;

			switch (sessionInfo.session)
			{
				case SessionInfo.Session.None:
				default:
					return;
				case SessionInfo.Session.Practice:
					outputDir = config.PracDir;
					break;
				case SessionInfo.Session.Qualification:
					outputDir = config.QualDir;
					break;
				case SessionInfo.Session.Race:
					outputDir = config.RaceDir;
					break;
			}
			Directory.CreateDirectory(outputDir);

			#region Set Lap By Lap of Leader
			// Do Stat on Leader of race
			List<SessionStats> raceLeader = new List<SessionStats>();	// race leader of each lap
			int CurLap = -1;
			long minLapTime;
			bool noRaceLeaderInThisLap = true;
			SessionStats curLapLeader = null;

			// A helper and just in case somebody used tmpTime before this zeros tmpTime before using it.
			for (int i = 0; i < raceStatsList.Count; i++)
			{
				playerStats = raceStatsList[i];
				playerStats.tmpTime = 0;
			}
			// for every lap (CurLap), can't use sessionInfo.raceLaps since LFS reports race length in hours etc. as well
			// thus it is for every driven lap
			while (true)
			{
				CurLap++;
				minLapTime = 0;
				noRaceLeaderInThisLap = true;

				for (int i = 0; i < raceStatsList.Count; i++)	// for all players
				{
					playerStats = raceStatsList[i];
					if (playerStats.lap.Count > CurLap)	// player drove in this lap, CurLap starts at 0
					{
						playerStats.tmpTime = playerStats.tmpTime + (playerStats.lap[CurLap]).LapTime;	// calculate player's total time in each lap they drove
						if (minLapTime == 0 || playerStats.tmpTime < minLapTime)	// player is faster in this lap
						{
							minLapTime = playerStats.tmpTime;
							curLapLeader = playerStats;	// update link/reference to fastest player in this lap
							noRaceLeaderInThisLap = false;
						}
					}
				}
				if (noRaceLeaderInThisLap) break;	// all race laps are processed and no race leader was found in last processed lap
				curLapLeader.lapsLead++;
				raceLeader.Add(curLapLeader);
			}
			#endregion
			#region average
			for (int i = 0; i < raceStatsList.Count; i++)
			{
				playerStats = raceStatsList[i];
				try
				{
					if (playerStats.lap.Count != 0)
						playerStats.avgTime = playerStats.cumuledTime / playerStats.lap.Count;
				}
				catch
				{
					playerStats.avgTime = 0;
				}
			}
			#endregion
			#region lap stability
			for (int i = 0; i < raceStatsList.Count; i++)
			{
				playerStats = raceStatsList[i];

				if (playerStats.avgTime == 0)
				{
					playerStats.lapStability = -1;
					continue;
				}
				for (int j = 0; j < playerStats.lap.Count; j++)
				{
					playerStats.lapStability += System.Math.Pow((double)(playerStats.avgTime - (playerStats.lap[j]).LapTime), 2);
				}
				if (playerStats.lap.Count > 1)
					playerStats.lapStability = System.Math.Sqrt(playerStats.lapStability / ((playerStats.lap.Count) - 1));
				else
					playerStats.lapStability = -1;
			}
			#endregion
			#region LBL
			int lbl_lapCount = 0;
			long lbl_bestLap = 0;
			SessionStats lbl_bestLapRacer = null;

			// Percentage/hue pairs sorted by percentage when adding them to the list
			TupleList<double, double> lbl_cellBgHue = new TupleList<double, double>() { { 100.0, 210.0 }, { 100.5, 180.0 }, { 101.5, 120.0 }, { 103.0, 60.0 }, { 105.0, 0.0 }, { 107.5, -60.0 } };	// CSS does wrap around negative hue, -60 = 300, -120 = 240
			string cellHue;
			const string lbl_cellBgLight = "50", lbl_cellBgLightWhite = "100";

			// LBL: race best lap and lap count
			for (int i = 0; i < raceStatsList.Count; i++)
			{
				playerStats = raceStatsList[i];
				if (playerStats.bestLap == 0)
					continue;
				if (lbl_bestLap == 0 || (playerStats.bestLap < lbl_bestLap))
				{
					lbl_bestLapRacer = playerStats;
					lbl_bestLap = playerStats.bestLap;
				}
				if (playerStats.lap.Count > lbl_lapCount)
					lbl_lapCount = playerStats.lap.Count;
			}
			#endregion

			using (StreamWriter sw = new StreamWriter(Path.Combine(outputDir, fileName)))
			{
				//int tagStart = 0;
				string lnickname, allUserName, readLine;

				#region Load template
				string raceHtmlName = Path.Combine("templates", "html_race.html");	// NEXT prac qual race add above to switch
				string[] raceHtml = null;
				if (!File.Exists(raceHtmlName))
				{
					LFSStats.ErrorWriteLine("Template \"" + raceHtmlName + "\" does not exist or permission to read is denied, forced shutdown.");
					LFSStats.Exit(3);
				}
				try
				{
					raceHtml = File.ReadAllLines(raceHtmlName);
				}
				catch (Exception ex)
				{
					LFSStats.ErrorWriteException("Template \"" + raceHtmlName + "\" read encountered an error, forced shutdown.", ex);
					LFSStats.Exit(3);
				}
				#endregion

				// StringBuilders for data processing
				StringBuilder inputBlock = new StringBuilder(1024);		// used to accumulate tag input lines
				StringBuilder outputBlock = new StringBuilder(1024);	// used to generate output

				combinedBestLap = 0;

				for (int lineIx = 0; lineIx < raceHtml.Length; lineIx++)
				{
					inputBlock.Clear();
					readLine = raceHtml[lineIx];

					#region RaceResult
					if (DetectTagAndBuildInputBlock("[RaceResult", "]", ref readLine, ref lineIx, raceHtml, inputBlock))
					{
						raceStatsList.Sort();
						curPos = 0;
						int firstMaxLap = 0;
						long firstTotalTime = 0;

						for (int i = 0; i < raceStatsList.Count; i++)
						{
							curPos++;
							outputBlock.Clear();
							outputBlock.Append(inputBlock);
							playerStats = raceStatsList[i];

							//lnickname = lfsColorToHtml( playerStats.NickName );
							lnickname = lfsColorToHtml(getAllNickName(playerStats));
							allUserName = getAllUserName(playerStats);
							if (i == 0)
							{
								firstMaxLap = playerStats.lap.Count;
								firstTotalTime = playerStats.totalTime;
							}
							playerStats.finalPos = curPos;
							outputBlock.Replace("{Position}", playerStats.finalPos.ToString());
							outputBlock.Replace("{PlayerNameColored}", lnickname);
							outputBlock.Replace("{UserNameLink}", htmlLinkEscape(playerStats.userName));
							//                        outputBlock.Replace("{UserName}", playerStats.userName);
							outputBlock.Replace("{UserName}", allUserName);
							outputBlock.Replace("{Car}", playerStats.carName);
							outputBlock.Replace("{Plate}", htmlEscape(lfsStripColor(playerStats.Plate)));
							wrInfo = LFSWorld.GetWR(sessionInfo.trackNameCode, playerStats.carName);
							if (wrInfo == null)
								outputBlock.Replace("{DifferenceToWR}", "");
							else
								outputBlock.Replace("{DifferenceToWR}", SessionStats.LfsTimeToString(playerStats.bestLap - wrInfo.WrTime));


							// if Racer did not finish
							if (playerStats.resultNum == 999)
								outputBlock.Replace("{Gap}", "DNF");
							else
							{
								if (firstMaxLap == playerStats.lap.Count)
								{
									if (i == 0)
										outputBlock.Replace("{Gap}", SessionStats.LfsTimeToString(playerStats.totalTime));
									else
									{
										long tres;
										tres = playerStats.totalTime - firstTotalTime;
										outputBlock.Replace("{Gap}", "+" + SessionStats.LfsTimeToString(tres));
									}
								}
								else
									outputBlock.Replace("{Gap}", "+" + ((int)(firstMaxLap - playerStats.lap.Count)).ToString() + " laps");
							}
							outputBlock.Replace("{BestLap}", SessionStats.LfsTimeToString(playerStats.bestLap));
							outputBlock.Replace("{LapsDone}", playerStats.lap.Count.ToString());
							outputBlock.Replace("{PitsDone}", playerStats.numStop.ToString());
							outputBlock.Replace("{Flags}", playerStats.sFlags);
							outputBlock.Replace("{AutoClutch}", playerStats.GetPlayerFlagAutoClutch());
							outputBlock.Replace("{AutoGearShift}", playerStats.GetPlayerFlagAutoGearShift());
							outputBlock.Replace("{AxisClutch}", playerStats.GetPlayerFlagAxisClutch());
							outputBlock.Replace("{BrakeHelp}", playerStats.GetPlayerFlagBrakeHelp());
							outputBlock.Replace("{CustomView}", playerStats.GetPlayerFlagCustomView());
							outputBlock.Replace("{DriverSide}", playerStats.GetPlayerFlagDriverSide());
							outputBlock.Replace("{ManualShifter}", playerStats.GetPlayerFlagManualShifter());
							outputBlock.Replace("{Steering}", playerStats.GetPlayerFlagSteering());
							outputBlock.Replace("{Penalty}", playerStats.penalty);

							sw.WriteLine(outputBlock.ToString());	// writes processed row
						}
						continue;
					}
					#endregion

					#region StartOrder
					if (DetectTagAndBuildInputBlock("[StartOrder", "]", ref readLine, ref lineIx, raceHtml, inputBlock))
					{
						raceStatsList.Sort(SessionStats.GetGridCmp());
						curPos = 0;
						for (int i = 0; i < raceStatsList.Count; i++)
						{
							outputBlock.Clear();
							outputBlock.Append(inputBlock);
							playerStats = raceStatsList[i];
							lnickname = lfsStripColor(getAllNickName(playerStats));
							allUserName = getAllUserName(playerStats);
							playerStats.finalPos = curPos;

							if (playerStats.gridPos == 999)
								outputBlock.Replace("{Position}", "-");
							else
								outputBlock.Replace("{Position}", playerStats.gridPos.ToString());
							outputBlock.Replace("{UserNameLink}", playerStats.userName);
							outputBlock.Replace("{PlayerName}", lnickname);
							outputBlock.Replace("{UserName}", allUserName);
							sw.WriteLine(outputBlock.ToString());
						}
						continue;
					}
					#endregion

					#region HighestClimber
					if (DetectTagAndBuildInputBlock("[HighestClimber", "]", ref readLine, ref lineIx, raceHtml, inputBlock))
					{
						raceStatsList.Sort(SessionStats.GetClimbCmp());
						curPos = 0;
						for (int i = 0; i < raceStatsList.Count; i++)
						{
							outputBlock.Clear();
							outputBlock.Append(inputBlock);
							playerStats = raceStatsList[i];
							//                        lnickname = lfsStripColor(playerStats.NickName);
							lnickname = lfsStripColor(getAllNickName(playerStats));
							allUserName = getAllUserName(playerStats);
							playerStats.finalPos = curPos;

							if (playerStats.gridPos == 999 || playerStats.resultNum >= 998)
								continue;
							curPos++;
							outputBlock.Replace("{Position}", curPos.ToString());
							//                        outputBlock.Replace("{PlayerName}", lnickname);
							//                        outputBlock.Replace("{UserName}", playerStats.userName);
							outputBlock.Replace("{PlayerName}", lnickname);
							outputBlock.Replace("{UserName}", allUserName);
							outputBlock.Replace("{StartPos}", playerStats.gridPos.ToString());
							outputBlock.Replace("{FinishPos}", (playerStats.resultNum + 1).ToString());
							outputBlock.Replace("{Difference}", (playerStats.gridPos - playerStats.resultNum - 1).ToString());

							sw.WriteLine(outputBlock.ToString());
						}
						continue;
					}
					#endregion

					#region RaceLeader
					if (DetectTagAndBuildInputBlock("[RaceLeader", "]", ref readLine, ref lineIx, raceHtml, inputBlock))
					{
						raceStatsList.Sort(SessionStats.GetLapLeadCmp());

						curPos = 0;
						//int prevPLID = -1;
						int initLap = 0;
						int endLap = 0;
						int i;
						//int curPLID = 0;
						SessionStats prevLapLeader = null;

						for (i = 0; i < raceLeader.Count; i++)
						{
							curLapLeader = raceLeader[i];
							//curPLID = curLapLeader.PLID;
							if (prevLapLeader == null || curLapLeader.PLID != prevLapLeader.PLID)	// output on every change of leader
							{
								if (prevLapLeader != null)	// won't output the first starting difference when there is no real previous leader
								{
									// outputs previous leader stats
									lnickname = lfsStripColor(getAllNickName(prevLapLeader));
									allUserName = getAllUserName(prevLapLeader);

									curPos++;
									endLap = i;
									outputBlock.Clear();
									outputBlock.Append(inputBlock);
									outputBlock.Replace("{Position}", curPos.ToString());
									//                                outputBlock.Replace("{PlayerName}", lnickname);
									//                                outputBlock.Replace("{UserName}", playerStats.userName);
									outputBlock.Replace("{PlayerName}", lnickname);
									outputBlock.Replace("{UserName}", allUserName);

									outputBlock.Replace("{LapsLead}", initLap.ToString() + "-" + endLap.ToString());
									sw.WriteLine(outputBlock.ToString());
								}
								// sets previous leader to the current and sets his initial lap
								prevLapLeader = curLapLeader;
								initLap = i + 1;
							}
						}
						// won't output the first starting difference when there is no real previous leader
						// and prevent an error in case that no race leader exists, such as nobody finished first lap
						if (prevLapLeader != null)
						{
							lnickname = lfsStripColor(getAllNickName(curLapLeader));
							allUserName = getAllUserName(curLapLeader);

							curPos++;
							endLap = i;
							outputBlock.Clear();
							outputBlock.Append(inputBlock);
							outputBlock.Replace("[RaceLeader", "");
							outputBlock.Replace("]", "");
							outputBlock.Replace("{Position}", curPos.ToString());
							//                        outputBlock.Replace("{PlayerName}", lnickname);
							//                        outputBlock.Replace("{UserName}", playerStats.userName);
							outputBlock.Replace("{PlayerName}", lnickname);
							outputBlock.Replace("{UserName}", allUserName);
							outputBlock.Replace("{LapsLead}", initLap.ToString() + "-" + endLap.ToString());
							sw.WriteLine(outputBlock.ToString());
						}
						continue;
					}
					#endregion

					#region LapsLed
					if (DetectTagAndBuildInputBlock("[LapsLed", "]", ref readLine, ref lineIx, raceHtml, inputBlock))
					{
						raceStatsList.Sort(SessionStats.GetLapLeadCmp());
						curPos = 0;
						for (int i = 0; i < raceStatsList.Count; i++)
						{
							outputBlock.Clear();
							outputBlock.Append(inputBlock);
							playerStats = raceStatsList[i];
							//                        lnickname = lfsStripColor(playerStats.NickName);
							lnickname = lfsStripColor(getAllNickName(playerStats));
							allUserName = getAllUserName(playerStats);
							playerStats.finalPos = curPos;

							if (playerStats.lapsLead == 0)
								continue;
							curPos++;
							outputBlock.Replace("{Position}", curPos.ToString());
							//                        outputBlock.Replace("{PlayerName}", lnickname);
							//                        outputBlock.Replace("{UserName}", playerStats.userName);
							outputBlock.Replace("{PlayerName}", lnickname);
							outputBlock.Replace("{UserName}", allUserName);

							outputBlock.Replace("{LapsLed}", playerStats.lapsLead.ToString());

							sw.WriteLine(outputBlock.ToString());
						}
						continue;
					}
					#endregion

					#region Relays
					if (DetectTagAndBuildInputBlock("[Relay", "]", ref readLine, ref lineIx, raceHtml, inputBlock))
					{
						if (sessionInfo.isToc) // if TOC on this race
						{
							raceStatsList.Sort();
							// string outputBlock;
							curPos = 0;
							for (int i = 0; i < raceStatsList.Count; i++)
							{
								outputBlock.Clear();
								outputBlock.Append(inputBlock);
								playerStats = raceStatsList[i];
								//                        lnickname = lfsStripColor(playerStats.NickName);
								lnickname = lfsStripColor(getAllNickName(playerStats));
								allUserName = getAllUserName(playerStats);
								playerStats.finalPos = curPos;
								//                            if (playerStats.toc.Count == 0)   // No toc for this Racer
								//                                continue;
								curPos++;
								outputBlock.Replace("{PlayerName}", lnickname);
								outputBlock.Replace("{UserName}", allUserName);
								string allRelays = "";
								string allLaps = "";
								string br = "";
								int last_lap = 1;
								int nbLaps = 0;
								for (int j = 0; j < playerStats.toc.Count; j++)
								{
									allRelays = allRelays
												+ br
												+ lfsStripColor(playerStats.toc[j].oldNickName);
									//                                        + " -> "
									//                                        + lfsStripColor((playerStats.toc[j] as Toc).newNickName);
									;
									nbLaps = playerStats.toc[j].lap - last_lap + 1;
									allLaps = allLaps
												+ br
												+ (last_lap).ToString() + "-" + playerStats.toc[j].lap + " (" + nbLaps + " " + lang["lg_laps"] + ")";
									;
									last_lap = playerStats.toc[j].lap + 1;
									br = "<br>";

								}
								allRelays = allRelays
											+ br
											+ lfsStripColor(playerStats.NickName);
								if (last_lap <= playerStats.lap.Count)
								{
									nbLaps = playerStats.lap.Count - last_lap + 1;
									allLaps = allLaps
												+ br
												+ (last_lap).ToString() + "-" + (playerStats.lap.Count).ToString() + " (" + nbLaps + " " + lang["lg_laps"] + ")";
									;
								}
								else
								{
									nbLaps = 0;
									allLaps = allLaps
												+ br
												+ (playerStats.lap.Count).ToString() + "-" + (playerStats.lap.Count).ToString() + " (" + nbLaps + " " + lang["lg_laps"] + ")";
									;
								}
								outputBlock.Replace("{Position}", (i + 1).ToString());
								outputBlock.Replace("{Relays}", allRelays);
								outputBlock.Replace("{Laps}", allLaps);
								sw.WriteLine(outputBlock.ToString());
							}

							continue;
						}
						else
							continue;
					}

					#endregion

					#region FirstLap
					if (DetectTagAndBuildInputBlock("[FirstLap", "]", ref readLine, ref lineIx, raceHtml, inputBlock))
					{
						raceStatsList.Sort(SessionStats.GetFirstLapCmp());
						curPos = 0;
						long baseTime = 0;
						for (int i = 0; i < raceStatsList.Count; i++)
						{
							playerStats = raceStatsList[i];

							outputBlock.Clear();
							outputBlock.Append(inputBlock);
							if (playerStats.firstTime == 0)
								continue;

							//                        lnickname = lfsStripColor(playerStats.NickName);
							lnickname = lfsStripColor(getAllNickName(playerStats));
							allUserName = getAllUserName(playerStats);
							curPos++;
							if (curPos == 1)
								baseTime = playerStats.firstTime;
							outputBlock.Replace("{Position}", curPos.ToString());

							//                        outputBlock.Replace("{PlayerName}", lnickname);
							//                        outputBlock.Replace("{UserName}", playerStats.userName);
							outputBlock.Replace("{PlayerName}", lnickname);
							outputBlock.Replace("{UserName}", allUserName);

							outputBlock.Replace("{LapTime}", SessionStats.LfsTimeToString(playerStats.firstTime));
							outputBlock.Replace("{Difference}", SessionStats.LfsTimeToString(playerStats.firstTime - baseTime));
							sw.WriteLine(outputBlock.ToString());
						}
						continue;
					}
					#endregion

					#region LapTimesStability
					if (DetectTagAndBuildInputBlock("[LapTimesStability", "]", ref readLine, ref lineIx, raceHtml, inputBlock))
					{
						raceStatsList.Sort(SessionStats.GetStabilityCmp());
						curPos = 0;
						double baseTime = 0;
						for (int i = 0; i < raceStatsList.Count; i++)
						{
							playerStats = raceStatsList[i];

							outputBlock.Clear();
							outputBlock.Append(inputBlock);
							if (playerStats.lapStability <= 0)
								continue;
							//                        lnickname = lfsStripColor(playerStats.NickName);
							lnickname = lfsStripColor(getAllNickName(playerStats));
							allUserName = getAllUserName(playerStats);
							curPos++;
							if (curPos == 1)
								baseTime = playerStats.lapStability;
							outputBlock.Replace("{Position}", curPos.ToString());

							//                        outputBlock.Replace("{PlayerName}", lnickname);
							//                        outputBlock.Replace("{UserName}", playerStats.userName);
							outputBlock.Replace("{PlayerName}", lnickname);
							outputBlock.Replace("{UserName}", allUserName);

							outputBlock.Replace("{Deviation}", SessionStats.LfsTimeToString((long)playerStats.lapStability));
							outputBlock.Replace("{Difference}", SessionStats.LfsTimeToString((long)(playerStats.lapStability - baseTime)));
							outputBlock.Replace("{LapsDone}", playerStats.lap.Count.ToString());

							sw.WriteLine(outputBlock.ToString());
						}
						continue;

					}
					#endregion

					#region AverageLap
					if (DetectTagAndBuildInputBlock("[AverageLap", "]", ref readLine, ref lineIx, raceHtml, inputBlock))
					{
						raceStatsList.Sort(SessionStats.GetAvgTimeCmp());
						curPos = 0;
						long baseTime = 0;
						for (int i = 0; i < raceStatsList.Count; i++)
						{
							playerStats = raceStatsList[i];

							outputBlock.Clear();
							outputBlock.Append(inputBlock);
							if (playerStats.avgTime == 0)
								continue;
							//                        lnickname = lfsStripColor(playerStats.NickName);
							lnickname = lfsStripColor(getAllNickName(playerStats));
							allUserName = getAllUserName(playerStats);
							curPos++;
							if (curPos == 1)
								baseTime = playerStats.avgTime;
							outputBlock.Replace("{Position}", curPos.ToString());

							//                        outputBlock.Replace("{PlayerName}", lnickname);
							//                        outputBlock.Replace("{UserName}", playerStats.userName);
							outputBlock.Replace("{PlayerName}", lnickname);
							outputBlock.Replace("{UserName}", allUserName);

							outputBlock.Replace("{LapTime}", SessionStats.LfsTimeToString(playerStats.avgTime));
							outputBlock.Replace("{Difference}", SessionStats.LfsTimeToString(playerStats.avgTime - baseTime));
							outputBlock.Replace("{Laps}", playerStats.lap.Count.ToString());
							sw.WriteLine(outputBlock.ToString());
						}
						continue;
					}
					#endregion

					#region BestLap
					if (DetectTagAndBuildInputBlock("[BestLap", "]", ref readLine, ref lineIx, raceHtml, inputBlock))
					{
						raceStatsList.Sort(SessionStats.GetLapCmp());
						curPos = 0;
						long baseTime = 0;
						for (int i = 0; i < raceStatsList.Count; i++)
						{
							playerStats = raceStatsList[i];

							outputBlock.Clear();
							outputBlock.Append(inputBlock);
							if (playerStats.bestLap == 0)
								continue;
							//                        lnickname = lfsStripColor(playerStats.NickName);
							lnickname = lfsStripColor(getAllNickName(playerStats));
							allUserName = getAllUserName(playerStats);
							curPos++;
							if (curPos == 1)
								baseTime = playerStats.bestLap;
							outputBlock.Replace("{Position}", curPos.ToString());

							//                        outputBlock.Replace("{PlayerName}", lnickname);
							//                        outputBlock.Replace("{UserName}", playerStats.userName);
							outputBlock.Replace("{PlayerName}", lnickname);
							outputBlock.Replace("{UserName}", allUserName);

							outputBlock.Replace("{LapTime}", SessionStats.LfsTimeToString(playerStats.bestLap));
							outputBlock.Replace("{Difference}", SessionStats.LfsTimeToString(playerStats.bestLap - baseTime));
							wrInfo = LFSWorld.GetWR(sessionInfo.trackNameCode, playerStats.carName);
							if (wrInfo == null)
								outputBlock.Replace("{DifferenceToWR}", "");
							else
								outputBlock.Replace("{DifferenceToWR}", SessionStats.LfsTimeToString(playerStats.bestLap - wrInfo.WrTime));
							outputBlock.Replace("{Lap}", playerStats.lapBestLap.ToString());
							sw.WriteLine(outputBlock.ToString());
						}
						continue;
					}
					#endregion

					#region BestSplitTable
					if (DetectTagAndBuildInputBlock("[[BestSectorTable", "]]", ref readLine, ref lineIx, raceHtml, inputBlock))
					{
						// returns non empty blocks between tags/delimiters, tags/delimiters are removed
						string[] strArray = inputBlock.ToString().Split(new string[] { "[SectorRow", "]" }, StringSplitOptions.RemoveEmptyEntries);
						// check strArray length as we do use indexed access from 0 to 2
						if (strArray.Length < 3)
						{
							sw.WriteLine(@"<span class='templateError'>Best sector table template error.</span>");
							continue;
						}

						strArray[0] = updateGlob(strArray[0], fileName, sessionInfo, "race");

						// Generates split tables
						for (int j = 0; j <= sessionInfo.maxSplit; j++)	// j = split number 0 to maxSplit
						{
							// Calculates additional split statistics
							// This is a mess and should be redone, don't code like this people
							// you could just address with 'j' the splits you need and not copy the values
							// to a temporary bestSectorSortHelper and BestSectorLapHelper,
							// get those when needed or at least remove the "cur"
							for (int i = 0; i < raceStatsList.Count; i++)
							{
								playerStats = raceStatsList[i];
								wrInfo = LFSWorld.GetWR(sessionInfo.trackNameCode, playerStats.carName);

								playerStats.bestSectorSortHelper = playerStats.BestSector[j];
								playerStats.BestSectorLapHelper = playerStats.BestSectorLap[j];
								if (wrInfo == null)
									playerStats.curWrSplit = 0;
								else
									playerStats.curWrSplit = wrInfo.Sector[j];

								//if (j == 0)	// NEXT update these classes to have the data in an array not as variables, remove all the silly ifs
								//{
								//    playerStats.bestSectorSortHelper = playerStats.bestSplit1;
								//    playerStats.BestSectorLapHelper = playerStats.lapBestSplit1;
								//    if (wrInfo == null)
								//        playerStats.curWrSplit = 0;
								//    else
								//        playerStats.curWrSplit = wrInfo.sector[j];
								//    continue;
								//}
								//if (j == 1)
								//{
								//    playerStats.bestSectorSortHelper = playerStats.bestSplit2;
								//    playerStats.BestSectorLapHelper = playerStats.lapBestSplit2;
								//    if (wrInfo == null)
								//        playerStats.curWrSplit = 0;
								//    else
								//        playerStats.curWrSplit = wrInfo.sector[j];
								//    continue;
								//}
								//if (j == 2)
								//{
								//    playerStats.bestSectorSortHelper = playerStats.bestSplit3;
								//    playerStats.BestSectorLapHelper = playerStats.lapBestSplit3;
								//    if (wrInfo == null)
								//        playerStats.curWrSplit = 0;
								//    else
								//        playerStats.curWrSplit = wrInfo.sector[j];
								//    continue;
								//}
								//if (j == sessionInfo.maxSplit)
								//{
								//    playerStats.bestSectorSortHelper = playerStats.bestLastSplit;
								//    playerStats.BestSectorLapHelper = playerStats.lapBestLastSplit;
								//    if (wrInfo == null)
								//        playerStats.curWrSplit = 0;
								//    else
								//        playerStats.curWrSplit = wrInfo.sector[j];
								//    continue;
								//}
							}
							// depends on the above code that sets curBestSplits, sorting needs to be after curbestSplits have been set
							raceStatsList.Sort(SessionStats.GetSplitCmp());

							sw.WriteLine(strArray[0].Replace("{SectorNumber}", (j + 1).ToString()));	// Writes the beginning of table

							// [SplitRow
							curPos = 0;
							long baseTime = 0;

							// Generates rows inside split tables
							for (int i = 0; i < raceStatsList.Count; i++)
							{
								playerStats = raceStatsList[i];
								outputBlock.Clear();
								outputBlock.Append(strArray[1]);

								if (playerStats.bestSectorSortHelper == 0) continue;
								//	lnickname = lfsStripColor(playerStats.NickName);
								lnickname = lfsStripColor(getAllNickName(playerStats));
								allUserName = getAllUserName(playerStats);

								//outputBlock.Replace("[BestSplit", "");
								//outputBlock.Replace("]", "");
								curPos++;
								if (curPos == 1) baseTime = playerStats.bestSectorSortHelper;
								outputBlock.Replace("{Position}", curPos.ToString());

								outputBlock.Replace("{PlayerName}", lnickname);
								outputBlock.Replace("{UserName}", allUserName);
								// outputBlock.Replace("{UserName}", playerStats.userName);
								outputBlock.Replace("{SectorTime}", SessionStats.LfsTimeToString(playerStats.bestSectorSortHelper));
								outputBlock.Replace("{Difference}", SessionStats.LfsTimeToString(playerStats.bestSectorSortHelper - baseTime));
								if (playerStats.curWrSplit == 0)
									outputBlock.Replace("{DifferenceToWR}", "");
								else
									outputBlock.Replace("{DifferenceToWR}", SessionStats.LfsTimeToString(playerStats.bestSectorSortHelper - playerStats.curWrSplit));
								outputBlock.Replace("{Lap}", playerStats.BestSectorLapHelper.ToString());
								sw.WriteLine(outputBlock.ToString());
							}
							combinedBestLap = combinedBestLap + baseTime;

							sw.WriteLine(strArray[2]);	// Writes the end of table
						}
						continue;
					}
					#endregion

					#region BestPossibleLap
					if (DetectTagAndBuildInputBlock("[BestPossibleLap", "]", ref readLine, ref lineIx, raceHtml, inputBlock))
					{
						raceStatsList.Sort(SessionStats.GetTPBCmp());
						curPos = 0;
						long baseTime = 0;
						for (int i = 0; i < raceStatsList.Count; i++)
						{
							playerStats = raceStatsList[i];

							outputBlock.Clear();
							outputBlock.Append(inputBlock);

							if (playerStats.BestSector.Sum() == 0)
								continue;
							//                        lnickname = lfsStripColor(playerStats.NickName);
							lnickname = lfsStripColor(getAllNickName(playerStats));
							allUserName = getAllUserName(playerStats);
							curPos++;
							if (curPos == 1)
								baseTime = playerStats.BestSector.Sum();
							outputBlock.Replace("{Position}", curPos.ToString());

							//                        outputBlock.Replace("{PlayerName}", lnickname);
							//                        outputBlock.Replace("{UserName}", playerStats.userName);
							outputBlock.Replace("{PlayerName}", lnickname);
							outputBlock.Replace("{UserName}", allUserName);

							outputBlock.Replace("{LapTime}", SessionStats.LfsTimeToString(playerStats.BestSector.Sum()));
							outputBlock.Replace("{Difference}", SessionStats.LfsTimeToString(playerStats.BestSector.Sum() - baseTime));
							outputBlock.Replace("{DifferenceToBestLap}", SessionStats.LfsTimeToString(playerStats.bestLap - playerStats.BestSector.Sum()));
							wrInfo = LFSWorld.GetWR(sessionInfo.trackNameCode, playerStats.carName);
							if (wrInfo == null)
								outputBlock.Replace("{DifferenceToWR}", "");
							else
								outputBlock.Replace("{DifferenceToWR}", SessionStats.LfsTimeToString(playerStats.BestSector.Sum() - wrInfo.WrTime));
							sw.WriteLine(outputBlock.ToString());
						}
						continue;
					}
					#endregion

					#region BlueFlagCausers
					if (DetectTagAndBuildInputBlock("[BlueFlagCausers", "]", ref readLine, ref lineIx, raceHtml, inputBlock))
					{
						raceStatsList.Sort(SessionStats.GetBlueFlagCmp());
						curPos = 0;
						for (int i = 0; i < raceStatsList.Count; i++)
						{
							playerStats = raceStatsList[i];

							outputBlock.Clear();
							outputBlock.Append(inputBlock);
							if (playerStats.blueFlags == 0)
								continue;
							//                        lnickname = lfsStripColor(playerStats.NickName);
							lnickname = lfsStripColor(getAllNickName(playerStats));
							allUserName = getAllUserName(playerStats);
							curPos++;
							outputBlock.Replace("{Position}", curPos.ToString());

							//                        outputBlock.Replace("{PlayerName}", lnickname);
							//                        outputBlock.Replace("{UserName}", playerStats.userName);
							outputBlock.Replace("{PlayerName}", lnickname);
							outputBlock.Replace("{UserName}", allUserName);

							outputBlock.Replace("{BlueFlagsCount}", playerStats.blueFlags.ToString());
							sw.WriteLine(outputBlock.ToString());
						}
						continue;
					}
					#endregion

					#region YellowFlagCausers
					if (DetectTagAndBuildInputBlock("[YellowFlagCausers", "]", ref readLine, ref lineIx, raceHtml, inputBlock))
					{
						raceStatsList.Sort(SessionStats.GetYellowFlagCmp());
						curPos = 0;
						for (int i = 0; i < raceStatsList.Count; i++)
						{
							playerStats = raceStatsList[i];

							outputBlock.Clear();
							outputBlock.Append(inputBlock);
							if (playerStats.yellowFlags == 0)
								continue;
							//                        lnickname = lfsStripColor(playerStats.NickName);
							lnickname = lfsStripColor(getAllNickName(playerStats));
							allUserName = getAllUserName(playerStats);
							curPos++;
							outputBlock.Replace("{Position}", curPos.ToString());

							//                        outputBlock.Replace("{PlayerName}", lnickname);
							//                        outputBlock.Replace("{UserName}", playerStats.userName);
							outputBlock.Replace("{PlayerName}", lnickname);
							outputBlock.Replace("{UserName}", allUserName);

							outputBlock.Replace("{YellowFlagsCount}", playerStats.yellowFlags.ToString());
							sw.WriteLine(outputBlock.ToString());
						}
						continue;
					}
					#endregion

					#region PitStops
					if (DetectTagAndBuildInputBlock("[PitStops", "]", ref readLine, ref lineIx, raceHtml, inputBlock))
					{
						raceStatsList.Sort(SessionStats.GetPitCmp());
						string pitinfo = "";
						curPos = 0;
						for (int i = 0; i < raceStatsList.Count; i++)
						{
							playerStats = raceStatsList[i];

							outputBlock.Clear();
							outputBlock.Append(inputBlock);

							//                        lnickname = lfsStripColor(playerStats.NickName);
							lnickname = lfsStripColor(getAllNickName(playerStats));
							allUserName = getAllUserName(playerStats);
							// If no Pit
							if (playerStats.pit.Count == 0)
								continue;
							curPos++;
							outputBlock.Replace("{Position}", curPos.ToString());

							//                        outputBlock.Replace("{PlayerName}", lnickname);
							//                        outputBlock.Replace("{UserName}", playerStats.userName);
							outputBlock.Replace("{PlayerName}", lnickname);
							outputBlock.Replace("{UserName}", allUserName);

							pitinfo = "";
							string br = "";
							string virg = "";
							string SWork = "";
							for (int j = 0; j < playerStats.pit.Count; j++)
							{
								SWork = "{lg_lap} " + ((playerStats.pit[j] as Pit).LapsDone + 1) + ":";
								var Work = playerStats.pit[j].Work;

								if ((Work & PitWorkFlags.PSE_STOP) != 0)
								{
									SWork = SWork + virg + " {lg_stop}";
									virg = ", ";
								}
								if ((Work & PitWorkFlags.PSE_SETUP) != 0)
								{
									SWork = SWork + virg + "{lg_dam_set}";
									virg = ", ";
								}
								if (
									   (Work & PitWorkFlags.PSE_FR_DAM) != 0
									|| (Work & PitWorkFlags.PSE_RE_DAM) != 0
									|| (Work & PitWorkFlags.PSE_LE_FR_DAM) != 0
									|| (Work & PitWorkFlags.PSE_RI_FR_DAM) != 0
									|| (Work & PitWorkFlags.PSE_LE_RE_DAM) != 0
									|| (Work & PitWorkFlags.PSE_RI_RE_DAM) != 0
									)
								{
									SWork = SWork + virg + "{lg_dam_mec}";
									virg = ", ";
								}
								if (
									(Work & PitWorkFlags.PSE_BODY_MINOR) != 0
									|| (Work & PitWorkFlags.PSE_BODY_MAJOR) != 0
									)
								{
									SWork = SWork + virg + "{lg_dam_body}";
									virg = ", ";
								}
								if (
									(Work & PitWorkFlags.PSE_FR_WHL) != 0
									|| (Work & PitWorkFlags.PSE_LE_FR_WHL) != 0
									|| (Work & PitWorkFlags.PSE_RI_FR_WHL) != 0
									|| (Work & PitWorkFlags.PSE_RE_WHL) != 0
									|| (Work & PitWorkFlags.PSE_LE_RE_WHL) != 0
									|| (Work & PitWorkFlags.PSE_RI_RE_WHL) != 0
									)
								{
									SWork = SWork + virg + "{lg_dam_whe}";
									virg = ", ";
								}
								if ((Work & PitWorkFlags.PSE_REFUEL) != 0)
								{
									SWork = SWork + virg + " {lg_refuel}";
									virg = ", ";
								}
								if ((playerStats.pit[j] as Pit).STime != 0)
									SWork = SWork + virg + "{lg_startpit}";
								SWork = updateGlob(SWork, fileName, sessionInfo, "race");
								pitinfo = pitinfo + br + SWork + " (" + SessionStats.LfsTimeToString((playerStats.pit[j] as Pit).STime) + ")";
								br = "<BR>";
							}
							outputBlock.Replace("{PitInfo}", pitinfo);
							outputBlock.Replace("{PitTime}", SessionStats.LfsTimeToString(playerStats.cumuledStime));
							sw.WriteLine(outputBlock.ToString());
						}
						continue;
					}
					#endregion

					#region Penalties
					if (DetectTagAndBuildInputBlock("[Penalties", "]", ref readLine, ref lineIx, raceHtml, inputBlock))
					{
						raceStatsList.Sort(SessionStats.GetPenalizationCmp());
						curPos = 0;
						for (int i = 0; i < raceStatsList.Count; i++)
						{
							playerStats = raceStatsList[i];

							outputBlock.Clear();
							outputBlock.Append(inputBlock);
							if (playerStats.numPen == 0)
								continue;
							string penaltyInfo = "";
							for (int j = 0; j < playerStats.pen.Count; j++)
							{
								string strPen = "";
								if ((playerStats.pen[j]).NewPen != (int)PenaltyValue.PENALTY_NONE)
									strPen = Enum.GetName(typeof(PenaltyValue), (playerStats.pen[j]).NewPen);
								else
									strPen = Enum.GetName(typeof(PenaltyValue), (playerStats.pen[j]).OldPen);
								strPen = "{lg_" + strPen.Remove(0, 8) + "}";
								penaltyInfo += "{lg_lap} " + (playerStats.pen[j]).Lap.ToString() + ": " + strPen.ToLower();
								penaltyInfo += "<BR>";

							}
							penaltyInfo = updateGlob(penaltyInfo, fileName, sessionInfo, "race");	// updates penalty names
							//                        lnickname = lfsStripColor(playerStats.NickName);
							lnickname = lfsStripColor(getAllNickName(playerStats));
							allUserName = getAllUserName(playerStats);
							curPos++;
							outputBlock.Replace("{Position}", curPos.ToString());

							//                        outputBlock.Replace("{PlayerName}", lnickname);
							//                        outputBlock.Replace("{UserName}", playerStats.userName);
							outputBlock.Replace("{PlayerName}", lnickname);
							outputBlock.Replace("{UserName}", allUserName);

							outputBlock.Replace("{PenaltyInfo}", penaltyInfo);
							outputBlock.Replace("{PenaltyCount}", playerStats.numPen.ToString());
							sw.WriteLine(outputBlock.ToString());
						}
						continue;
					}
					#endregion

					#region TopSpeed
					if (DetectTagAndBuildInputBlock("[TopSpeed", "]", ref readLine, ref lineIx, raceHtml, inputBlock))
					{
						raceStatsList.Sort(SessionStats.GetSpeedCmp());
						curPos = 0;
						int baseSpeed = 0;

						for (int i = 0; i < raceStatsList.Count; i++)
						{
							playerStats = raceStatsList[i];

							outputBlock.Clear();
							outputBlock.Append(inputBlock);
							if (playerStats.bestSpeed == 0)
								continue;
							//                        lnickname = lfsStripColor(playerStats.NickName);
							lnickname = lfsStripColor(getAllNickName(playerStats));
							allUserName = getAllUserName(playerStats);
							curPos++;
							if (curPos == 1)
								baseSpeed = playerStats.bestSpeed;
							outputBlock.Replace("{Position}", curPos.ToString());

							//                        outputBlock.Replace("{PlayerName}", lnickname);
							//                        outputBlock.Replace("{UserName}", playerStats.userName);
							outputBlock.Replace("{PlayerName}", lnickname);
							outputBlock.Replace("{UserName}", allUserName);

							outputBlock.Replace("{TopSpeed}", SessionStats.LfsSpeedToString(playerStats.bestSpeed));
							outputBlock.Replace("{Difference}", SessionStats.LfsSpeedToString(baseSpeed - playerStats.bestSpeed));
							outputBlock.Replace("{TopSpeedLap}", playerStats.lapBestSpeed.ToString());
							sw.WriteLine(outputBlock.ToString());
						}
						continue;
					}
					#endregion

					#region LBL

					#region Percent
					if (DetectTagAndBuildInputBlock("[LBL_Percent", "]", ref readLine, ref lineIx, raceHtml, inputBlock))
					{
						// Generate percentage colors preview
						foreach (Tuple<double, double> item in lbl_cellBgHue)
						{
							outputBlock.Clear();
							outputBlock.Append(inputBlock);
							outputBlock.Replace("{LBL_Percent}", item.Item1.ToString());
							outputBlock.Replace("{LBL_CellBgHue}", item.Item2.ToString());
							outputBlock.Replace("{LBL_CellBgLight}", lbl_cellBgLight);
							sw.WriteLine(outputBlock.ToString());
						}
						continue;
					}
					#endregion

					#region BestLap
					if (DetectTagAndBuildInputBlock("[LBL_BestLap", "]", ref readLine, ref lineIx, raceHtml, inputBlock) && lbl_bestLapRacer != null)
					{
						outputBlock.Clear();
						outputBlock.Append(inputBlock);
						lnickname = lfsStripColor(getAllNickName(lbl_bestLapRacer));
						allUserName = getAllUserName(lbl_bestLapRacer);
						outputBlock.Replace("{LBL_bestLap}", SessionStats.LfsTimeToString(lbl_bestLap));
						outputBlock.Replace("{PlayerName}", lnickname);
						outputBlock.Replace("{UserName}", allUserName);
						sw.WriteLine(outputBlock.ToString());
						continue;
					}
					#endregion

					#region Headpos
					if (DetectTagAndBuildInputBlock("[LBL_HeadPos", "]", ref readLine, ref lineIx, raceHtml, inputBlock))
					{
						raceStatsList.Sort();
						for (int i = 0; i < raceStatsList.Count; i++)
						{
							outputBlock.Clear();
							outputBlock.Append(inputBlock);
							outputBlock.Replace("{Position}", (i + 1).ToString());
							sw.WriteLine(outputBlock.ToString());
						}
						continue;
					}
					#endregion

					#region Headracer
					if (DetectTagAndBuildInputBlock("[LBL_HeadRacer", "]", ref readLine, ref lineIx, raceHtml, inputBlock))
					{
						raceStatsList.Sort();

						for (int i = 0; i < raceStatsList.Count; i++)
						{
							playerStats = raceStatsList[i];
							lnickname = lfsStripColor(getAllNickName(playerStats));
							allUserName = getAllUserName(playerStats);

							outputBlock.Clear();
							outputBlock.Append(inputBlock);
							outputBlock.Replace("{PlayerName}", lnickname);
							outputBlock.Replace("{UserName}", allUserName);
							sw.WriteLine(outputBlock.ToString());
						}
						continue;
					}
					#endregion

					#region Resultracer
					if (DetectTagAndBuildInputBlock("[[LBL_ResultsRow", "]]", ref readLine, ref lineIx, raceHtml, inputBlock))
					{
						// returns non empty blocks between tags/delimiters, tags/delimiters are removed
						string[] strArray = inputBlock.ToString().Split(new string[] { "[LBL_ResultCells", "]" }, StringSplitOptions.RemoveEmptyEntries);

						if (strArray.Length < 3)
						{
							sw.WriteLine(@"<span class='templateError'>LBL table template error.</span>");
							continue;
						}

						raceStatsList.Sort();

						// Generates rows
						for (int j = 0; j < lbl_lapCount; j++)
						{
							sw.WriteLine(strArray[0].Replace("{Lap}", (j + 1).ToString()));	// Writes the beginning of table

							// [LBL_ResultCells
							for (int i = 0; i < raceStatsList.Count; i++)
							{
								cellHue = "";
								playerStats = raceStatsList[i];
								outputBlock.Clear();
								outputBlock.Append(strArray[1]);
								lnickname = lfsStripColor(getAllNickName(playerStats));
								allUserName = getAllUserName(playerStats);

								if (playerStats.lap.Count > j)
								{
									double lapTimeLowerLimit, lapTimeUpperLimit, lapTimeDiff, lapTimeRange, hueRange;

									// find color in table
									//foreach (KeyValuePair<double, string> item in lbl_cellBgHue)
									//{
									//    lapTimeUpperLimit = (double)lbl_bestLap * item.Key / 100.0;
									//    if ((playerStats.lap[j]).LapTime <= (long)lapTimeUpperLimit)
									//    {
									//        cellHue = item.Value;
									//        break;
									//    }
									//}

									// generate colors
									for (int k = 0; k < lbl_cellBgHue.Count; k++)
									{
										lapTimeUpperLimit = (double)lbl_bestLap * lbl_cellBgHue[k].Item1 / 100.0;	// TR

										if (playerStats.lap[j].LapTime <= (long)lapTimeUpperLimit)
										{
											if (k > 0)
											{
												lapTimeLowerLimit = (double)lbl_bestLap * lbl_cellBgHue[k - 1].Item1 / 100.0;	// TL
												lapTimeDiff = playerStats.lap[j].LapTime - lapTimeLowerLimit;	// L - TL 
												lapTimeRange = lapTimeUpperLimit - lapTimeLowerLimit;			// TU - TL
												// does assume negative hues are used
												hueRange = lbl_cellBgHue[k - 1].Item2 - lbl_cellBgHue[k].Item2;	// HL - HU
												cellHue = (lbl_cellBgHue[k - 1].Item2 - hueRange * (lapTimeDiff / lapTimeRange)).ToString();
											}
											else
												cellHue = lbl_cellBgHue[k].Item2.ToString();
											break;
										}
									}
									if (cellHue.Length == 0)	// time is over defined color range
									{
										outputBlock.Replace("{LBL_CellBgHue}", "0");
										outputBlock.Replace("{LBL_CellBgLight}", lbl_cellBgLightWhite);	// white
									}
									else
									{
										outputBlock.Replace("{LBL_CellBgHue}", cellHue);
										outputBlock.Replace("{LBL_CellBgLight}", lbl_cellBgLight);	// colored
									}
									outputBlock.Replace("{LBL_Ltime}", SessionStats.LfsTimeToString((playerStats.lap[j]).LapTime));
								}
								else
								{	// did not drive that lap
									outputBlock.Replace("{LBL_CellBgHue}", "0");
									outputBlock.Replace("{LBL_CellBgLight}", lbl_cellBgLightWhite);	// white
									outputBlock.Replace("{LBL_Ltime}", "");	// no time
								}
								sw.WriteLine(outputBlock.ToString());
							}
							sw.WriteLine(strArray[2]);	// Writes the end of table
						}
						continue;
					}
					#endregion

					#region Total
					if (DetectTagAndBuildInputBlock("[LBL_Total", "]", ref readLine, ref lineIx, raceHtml, inputBlock))
					{
						raceStatsList.Sort();

						long firstTotalTime = 0;
						int firstLapCount = 0;

						for (int i = 0; i < raceStatsList.Count; i++)
						{
							playerStats = raceStatsList[i];
							if (i == 0)
							{
								firstTotalTime = playerStats.totalTime;
								firstLapCount = playerStats.lap.Count;
							}
							lnickname = lfsStripColor(getAllNickName(playerStats));
							allUserName = getAllUserName(playerStats);

							outputBlock.Clear();
							outputBlock.Append(inputBlock);

							// if Racer do not finish
							if (playerStats.resultNum == 999)
								outputBlock.Replace("{Gap}", "DNF");
							else
							{
								if (playerStats.lap.Count == firstLapCount)
								{
									if (i == 0)
										outputBlock.Replace("{Gap}", SessionStats.LfsTimeToString(firstTotalTime));
									else
										outputBlock.Replace("{Gap}", "+" + SessionStats.LfsTimeToString(playerStats.totalTime - firstTotalTime));
								}
								else
								{
									int lapDiff = firstLapCount - playerStats.lap.Count;
									if (lapDiff == 1)
										outputBlock.Replace("{Gap}", "+" + lapDiff.ToString() + " {lg_lap}");
									else
										outputBlock.Replace("{Gap}", "+" + lapDiff.ToString() + " {lg_laps}");
								}
							}
							outputBlock.Replace("{LBL_Ttime}", SessionStats.LfsTimeToString(playerStats.totalTime));
							outputBlock.Replace("{Penalty}", playerStats.penalty);

							sw.WriteLine(updateGlob(outputBlock.ToString(), fileName, sessionInfo, "race"));
						}
						continue;
					}
					#endregion

					#endregion

					#region Chat
					if (DetectTagAndBuildInputBlock("[Chat", "]", ref readLine, ref lineIx, raceHtml, inputBlock))
					{
						foreach (Tuple<string, string> item in sessionInfo.chat)
						{
							outputBlock.Clear();
							outputBlock.Append(inputBlock);
							outputBlock.Replace("{PlayerNameColored}", lfsColorToHtml(item.Item1));
							outputBlock.Replace("{MessageColored}", ExportStats.lfsColorToHtml(ExportStats.lfsChatTextToHtml(item.Item2)));
							sw.WriteLine(outputBlock.ToString());
						}
						continue;
					}
					#endregion

					#region Include
					if (DetectTagAndBuildInputBlock("[Include", "]", ref readLine, ref lineIx, raceHtml, inputBlock))
					{
						string includeName = Path.Combine("include", inputBlock.ToString().Trim());
						string includeContent = null;

						if (!File.Exists(includeName))
						{
							LFSStats.ErrorWriteLine("Include \"" + includeName + "\" does not exist or permission to read is denied.");
							continue;
						}
						try
						{
							includeContent = File.ReadAllText(includeName);
						}
						catch (Exception ex)
						{
							LFSStats.ErrorWriteException("Include \"" + includeName + "\" read encountered an error.", ex);
							continue;
						}
						sw.WriteLine(updateGlob(includeContent, fileName, sessionInfo, "race"));
						continue;
					}
					#endregion

					#region Disabled
					if (DetectTagAndBuildInputBlock("[-", "]", ref readLine, ref lineIx, raceHtml, inputBlock))
					{
						continue;
					}
					#endregion

					// all non tagged lines are updated and written to output
					readLine = updateGlob(readLine, fileName, sessionInfo, "race");
					sw.WriteLine(readLine);
				}
			}
		}

		private static string htmlLinkEscape(string link)
		{
			return htmlEscape(link.Replace(" ", "+"));
		}
		private static string htmlEscape(string link)
		{
			return link.Replace("&", "&amp;").Replace("\"", "&quot;").Replace("<", "&lt;").Replace(">", "&gt;");
		}

		/// <summary>
		/// Detects the tag and builds input block.
		/// </summary>
		/// <param name="tagStart">The tag start.</param>
		/// <param name="tagEnd">The tag end.</param>
		/// <param name="readLine">The read line.</param>
		/// <param name="lineIx">The line index.</param>
		/// <param name="raceHtml">The race HTML.</param>
		/// <param name="inputBlock">The input block.</param>
		/// <returns>True if block found, false otherwise.</returns>
		private static bool DetectTagAndBuildInputBlock(string tagStart, string tagEnd, ref string readLine, ref int lineIx, string[] raceHtml, StringBuilder inputBlock)
		{
			// Multiline [TAG
			// [Tag is supposed to be on a new line and anything before [Tag is at the beginning of inputBlock.
			// The white space before [Tag will determine the indentation of output HTML block.
			if (readLine.IndexOf(tagStart, StringComparison.OrdinalIgnoreCase) >= 0)	// Contains
			{
				// Accumulate all lines into a block to process them
				while (true)
				{
					inputBlock.Append(readLine);				// add line to block
					if (readLine.Contains(tagEnd)) break;		// contains tagEnd, block ends
					else readLine = raceHtml[++lineIx].Trim();	// move to next line and trim it from both sides
				}
				inputBlock.Replace(tagStart, "");	// remove tag start "[Tag"
				inputBlock.Replace(tagEnd, "");		// remove tag end "]"

				return true;	// contains specified [Tag and inputBlock has been built
			}
			else return false;	// does not contain specified [Tag
		}
		//FIXME qualhtmlResult
		/*public static void qualhtmlResult(Dictionary<int, SessionStats> raceStat, string fileName, string qualDir, SessionInfo sessionInfo, int maxTimeQualIgnore)
		{
			long firstTotalTime = 0;
			int curPos;
			LFSWorld.WR wi;

			List<SessionStats> raceStatsList = new List<SessionStats>();
			IDictionaryEnumerator tmpRaceStat = raceStat.GetEnumerator();
			while (tmpRaceStat.MoveNext())	//for each player
			{
				SessionStats p = (SessionStats)tmpRaceStat.Value;
				raceStatsList.Add(p);
			}
			for (int i = 0; i < raceStatsList.Count; i++)
			{
				SessionStats p = raceStatsList[i];
				p.tmpTime = 0;
			}
			// Remove Lap over then x lbl_percent of bestlaptime
			for (int i = 0; i < raceStatsList.Count; i++)
			{

				SessionStats p = raceStatsList[i];
				int idx;
				long maxTime = (int)((double)p.bestLap * ((double)1 + ((double)maxTimeQualIgnore / (double)100)));
				// Remove Lap over then x lbl_percent of bestlaptime
				for (idx = p.lap.Count - 1; idx >= 0; idx--)
				{
					if ((p.lap[idx]).LapTime > maxTime)
					{

						System.Console.WriteLine("Remove " + SessionStats.LfsTimeToString((p.lap[idx]).LapTime)
									+ " Best : " + SessionStats.LfsTimeToString(p.bestLap)
									+ " Reference : " + SessionStats.LfsTimeToString(maxTime)
						);
						p.lap.RemoveAt(idx);
					}
				}
				// Recalc Split,cumuled Time and Lap of bestlap due to remove time under bestlap + X lbl_percent of bestLap
				p.cumuledTime = 0;
				if (p.lap.Count != 0)
				{
					for (idx = 0; idx < p.lap.Count; idx++)
					{
						p.cumuledTime += (p.lap[idx]).LapTime;
						if (p.bestLap == (p.lap[idx]).LapTime)
							p.lapBestLap = idx + 1;
					}
				}
			}
			// CALC avg
			for (int i = 0; i < raceStatsList.Count; i++)
			{
				SessionStats p = raceStatsList[i];
				try
				{
					if (p.lap.Count != 0)
						p.avgTime = p.cumuledTime / p.lap.Count;
				}
				catch
				{
					p.avgTime = 0;
				}
			}
			// CALC lapStability
			for (int i = 0; i < raceStatsList.Count; i++)
			{
				SessionStats p = raceStatsList[i];

				if (p.avgTime == 0)
				{
					p.lapStability = -1;
					continue;
				}
				for (int j = 0; j < p.lap.Count; j++)
				{
					p.lapStability += System.Math.Pow((double)(p.avgTime - (p.lap[j]).LapTime), 2);
				}
				if (p.lap.Count > 1)
					p.lapStability = System.Math.Sqrt(p.lapStability / ((p.lap.Count) - 1));
				else
					p.lapStability = -1;
			}
			StreamWriter sw = new StreamWriter(qualDir + "/" + fileName + "_results_qual.html");
			StreamReader sr = new StreamReader("./templates/html_qual.html");	// FIXME
			string readLine;
			combinedBestLap = 0;

			while (true)
			{
				readLine = sr.ReadLine();
				if (readLine == null)
					break;
				if (readLine.IndexOf("[QualResults") == 0)
				{
					SessionStats.modeSort = (int)RaceStatsSort.SORT_BESTLAP;
					raceStatsList.Sort();
					// string outputBlock;
					curPos = 0;
					for (int i = 0; i < raceStatsList.Count; i++)
					{
						outputBlock.Clear();outputBlock.Append(inputBlock);
						SessionStats p = raceStatsList[i];
						if (p.bestLap == 0)
							continue;
						curPos++;
						lnickname = lfsColorToHtml(p.NickName);
						outputBlock.Replace("[QualResults", "");
						outputBlock.Replace("]", "");
						p.finalPos = curPos;
						outputBlock.Replace("{Position}", p.finalPos.ToString());
						outputBlock.Replace("{PlayerNameColored}", lnickname);
						outputBlock.Replace("{UserNameLink}", p.userName);
						outputBlock.Replace("{UserName}", p.userName);
						outputBlock.Replace("{Car}", p.carName);
						outputBlock.Replace("{NumberPlate}", lfsStripColor(p.Plate));

						outputBlock.Replace("{BestLap}", SessionStats.LfsTimeToString(p.bestLap));
						if (curPos == 1)
						{
							firstTotalTime = p.bestLap;
						}
						long tres;
						tres = p.bestLap - firstTotalTime;
						outputBlock.Replace("{Difference}", "+" + SessionStats.LfsTimeToString(tres));
						wi = LFSWorld.GetWR(currInfoRace.trackNameCode, p.carName);
						if (wi == null)
							outputBlock.Replace("{DifferenceToWR}", "");
						else
							outputBlock.Replace("{DifferenceToWR}", SessionStats.LfsTimeToString(p.bestLap - wi.WRTime));
						outputBlock.Replace("{BestLapLap}", p.lapBestLap.ToString());
						outputBlock.Replace("{LapsDone}", p.lap.Count.ToString());

						outputBlock.Replace("{Flags}", p.sFlags);
						sw.WriteLine(outputBlock.ToString());
					}
					continue;
				}
				if (readLine.IndexOf("[LapTimesStability") == 0)
				{
					SessionStats.modeSort = (int)RaceStatsSort.SORT_STABILITY;
					raceStatsList.Sort();
					// string outputBlock;
					curPos = 0;
					double baseTime = 0;
					for (int i = 0; i < raceStatsList.Count; i++)
					{
						SessionStats p = raceStatsList[i];

						outputBlock.Clear();outputBlock.Append(inputBlock);
						if (p.lapStability <= 0)
							continue;
						lnickname = lfsStripColor(p.NickName);
						outputBlock.Replace("[LapTimesStability", "");
						outputBlock.Replace("]", "");
						curPos++;
						if (curPos == 1)
							baseTime = p.lapStability;
						outputBlock.Replace("{Position}", curPos.ToString());
						outputBlock.Replace("{PlayerName}", lnickname);
						outputBlock.Replace("{UserName}", p.userName);
						outputBlock.Replace("{Deviation}", SessionStats.LfsTimeToString((long)p.lapStability));
						outputBlock.Replace("{Difference}", SessionStats.LfsTimeToString((long)(p.lapStability - baseTime)));
						outputBlock.Replace("{LapsDone}", p.lap.Count.ToString());

						sw.WriteLine(outputBlock.ToString());
					}
					continue;

				}


				if (readLine.IndexOf("[AverageLap") == 0)
				{
					SessionStats.modeSort = (int)RaceStatsSort.SORT_AVGTIME;
					raceStatsList.Sort();
					// string outputBlock;
					curPos = 0;
					long baseTime = 0;
					for (int i = 0; i < raceStatsList.Count; i++)
					{
						SessionStats p = raceStatsList[i];

						outputBlock.Clear();outputBlock.Append(inputBlock);
						if (p.avgTime == 0)
							continue;
						lnickname = lfsStripColor(p.NickName);
						outputBlock.Replace("[AverageLap", "");
						outputBlock.Replace("]", "");
						curPos++;
						if (curPos == 1)
							baseTime = p.avgTime;
						outputBlock.Replace("{Position}", curPos.ToString());
						outputBlock.Replace("{PlayerName}", lnickname);
						outputBlock.Replace("{UserName}", p.userName);
						outputBlock.Replace("{LapTime}", SessionStats.LfsTimeToString(p.avgTime));
						outputBlock.Replace("{Difference}", SessionStats.LfsTimeToString(p.avgTime - baseTime));
						outputBlock.Replace("{Laps}", p.lap.Count.ToString());
						sw.WriteLine(outputBlock.ToString());
					}
					continue;
				}

				if (readLine.IndexOf("[BestLap") == 0)
				{
					SessionStats.modeSort = (int)RaceStatsSort.SORT_BESTLAP;
					raceStatsList.Sort();
					// string outputBlock;
					curPos = 0;
					long baseTime = 0;
					for (int i = 0; i < raceStatsList.Count; i++)
					{
						SessionStats p = raceStatsList[i];

						outputBlock.Clear();outputBlock.Append(inputBlock);
						if (p.bestLap == 0)
							continue;
						lnickname = lfsStripColor(p.NickName);
						outputBlock.Replace("[BestLap", "");
						outputBlock.Replace("]", "");
						curPos++;
						if (curPos == 1)
							baseTime = p.bestLap;
						outputBlock.Replace("{Position}", curPos.ToString());
						outputBlock.Replace("{PlayerName}", lnickname);
						outputBlock.Replace("{UserName}", p.userName);
						outputBlock.Replace("{LapTime}", SessionStats.LfsTimeToString(p.bestLap));
						outputBlock.Replace("{Difference}", SessionStats.LfsTimeToString(p.bestLap - baseTime));
						wi = LFSWorld.GetWR(currInfoRace.trackNameCode, p.carName);
						if (wi == null)
							outputBlock.Replace("{DifferenceToWR}", "");
						else
							outputBlock.Replace("{DifferenceToWR}", SessionStats.LfsTimeToString(p.bestLap - wi.WRTime));
						outputBlock.Replace("{Lap}", p.lapBestLap.ToString());
						sw.WriteLine(outputBlock.ToString());
					}
					continue;
				}
				if (readLine.IndexOf("[[BestSplitTable") == 0)
				{
					ArrayList blockSplit = new ArrayList();//TODO
					blockSplit.Add(readLine);
					while (true)
					{
						readLine = sr.ReadLine();
						if (readLine == null)
							break;
						if (readLine.IndexOf("]]") != -1)
						{
							blockSplit.Add(readLine);
							break;
						}
						blockSplit.Add(readLine);
					}
					for (int j = 0; j <= currInfoRace.maxSplit; j++)
					{
						for (int i = 0; i < raceStatsList.Count; i++)
						{
							SessionStats p = raceStatsList[i];
							wi = LFSWorld.GetWR(currInfoRace.trackNameCode, p.carName);
							if (j == currInfoRace.maxSplit)
							{
								p.bestSectorSortHelper = p.bestLastSplit;
								p.BestSectorLapHelper = p.lapBestLastSplit;
								if (wi == null)
									p.curWrSplit = 0;
								else
									p.curWrSplit = wi.sectorSplitLast;
								continue;
							}
							if (j == 0)
							{
								p.bestSectorSortHelper = p.bestSplit1;
								p.BestSectorLapHelper = p.lapBestSplit1;
								if (wi == null)
									p.curWrSplit = 0;
								else
									p.curWrSplit = wi.sectorSplit[j];
								continue;
							}
							if (j == 1)
							{
								p.bestSectorSortHelper = p.bestSplit2;
								p.BestSectorLapHelper = p.lapBestSplit2;
								if (wi == null)
									p.curWrSplit = 0;
								else
									p.curWrSplit = wi.sectorSplit[j];
								continue;
							}
							if (j == 2)
							{
								p.bestSectorSortHelper = p.bestSplit3;
								p.BestSectorLapHelper = p.lapBestSplit3;
								if (wi == null)
									p.curWrSplit = 0;
								else
									p.curWrSplit = wi.sectorSplit[j];
								continue;
							}
						}
						for (int k = 0; k < blockSplit.Count; k++)
						{
							readLine = (string)blockSplit[k];
							if (readLine.IndexOf("[BestSplit") == 0)
							{
								SessionStats.modeSort = (int)RaceStatsSort.SORT_BESTSPLIT;
								raceStatsList.Sort();
								// string outputBlock;
								curPos = 0;
								long baseTime = 0;
								for (int i = 0; i < raceStatsList.Count; i++)
								{
									SessionStats p = raceStatsList[i];

									outputBlock.Clear();outputBlock.Append(inputBlock);
									if (p.bestSectorSortHelper == 0)
										continue;
									lnickname = lfsStripColor(p.NickName);
									outputBlock.Replace("[BestSplit", "");
									outputBlock.Replace("]", "");
									curPos++;
									if (curPos == 1)
										baseTime = p.bestSectorSortHelper;
									outputBlock.Replace("{Position}", curPos.ToString());
									outputBlock.Replace("{PlayerName}", lnickname);
									outputBlock.Replace("{UserName}", p.userName);
									outputBlock.Replace("{SplitTime}", SessionStats.LfsTimeToString(p.bestSectorSortHelper));
									outputBlock.Replace("{Difference}", SessionStats.LfsTimeToString(p.bestSectorSortHelper - baseTime));
									if (p.curWrSplit == 0)
										outputBlock.Replace("{DifferenceToWR}", "");
									else
										outputBlock.Replace("{DifferenceToWR}", SessionStats.LfsTimeToString(p.bestSectorSortHelper - p.curWrSplit));
									outputBlock.Replace("{Lap}", p.BestSectorLapHelper.ToString());
									sw.WriteLine(outputBlock.ToString());
								}
								combinedBestLap = combinedBestLap + baseTime;
								continue;
							}
							readLine = updateGlob(readLine, fileName, currInfoRace, "qual");
							readLine = readLine.Replace("]]", "");
							readLine = readLine.Replace("[[BestSplitTable", "");
							readLine = readLine.Replace("{SplitNumber}", (j + 1).ToString());
							sw.WriteLine(readLine);
						}
					}
				}
				if (readLine.IndexOf("[BestPossibleLap") == 0)
				{
					SessionStats.modeSort = (int)RaceStatsSort.SORT_TPB;
					raceStatsList.Sort();
					// string outputBlock;
					curPos = 0;
					long baseTime = 0;
					for (int i = 0; i < raceStatsList.Count; i++)
					{
						SessionStats p = raceStatsList[i];

						outputBlock.Clear();outputBlock.Append(inputBlock);
						if ((p.bestSplit1 + p.bestSplit2 + p.bestSplit3 + p.bestLastSplit) == 0)
							continue;
						lnickname = lfsStripColor(p.NickName);
						outputBlock.Replace("[BestPossibleLap", "");
						outputBlock.Replace("]", "");
						curPos++;
						if (curPos == 1)
							baseTime = p.bestSplit1 + p.bestSplit2 + p.bestSplit3 + p.bestLastSplit;
						outputBlock.Replace("{Position}", curPos.ToString());
						outputBlock.Replace("{PlayerName}", lnickname);
						outputBlock.Replace("{UserName}", p.userName);
						outputBlock.Replace("{LapTime}", SessionStats.LfsTimeToString(p.bestSplit1 + p.bestSplit2 + p.bestSplit3 + p.bestLastSplit));
						outputBlock.Replace("{Difference}", SessionStats.LfsTimeToString((p.bestSplit1 + p.bestSplit2 + p.bestSplit3 + p.bestLastSplit) - baseTime));
						outputBlock.Replace("{DifferenceToBestLap}", SessionStats.LfsTimeToString((p.bestLap - p.bestSplit1 - p.bestSplit2 - p.bestSplit3 - p.bestLastSplit)));
						wi = LFSWorld.GetWR(currInfoRace.trackNameCode, p.carName);
						if (wi == null)
							outputBlock.Replace("{DifferenceToWR}", "");
						else
							outputBlock.Replace("{DifferenceToWR}", SessionStats.LfsTimeToString((p.bestSplit1 + p.bestSplit2 + p.bestSplit3 + p.bestLastSplit) - wi.WRTime));
						sw.WriteLine(outputBlock.ToString());
					}
					continue;
				}
				if (readLine.IndexOf("[BlueFlagCausers") == 0)
				{
					SessionStats.modeSort = (int)RaceStatsSort.SORT_BLUEFLAG;
					raceStatsList.Sort();
					// string outputBlock;
					curPos = 0;
					for (int i = 0; i < raceStatsList.Count; i++)
					{
						SessionStats p = raceStatsList[i];

						outputBlock.Clear();outputBlock.Append(inputBlock);
						if (p.blueFlags == 0)
							continue;
						lnickname = lfsStripColor(p.NickName);
						outputBlock.Replace("[BlueFlagCausers", "");
						outputBlock.Replace("]", "");
						curPos++;
						outputBlock.Replace("{Position}", curPos.ToString());
						outputBlock.Replace("{PlayerName}", lnickname);
						outputBlock.Replace("{UserName}", p.userName);
						outputBlock.Replace("{BlueFlagsCount}", p.blueFlags.ToString());
						sw.WriteLine(outputBlock.ToString());
					}
					continue;
				}
				if (readLine.IndexOf("[YellowFlagCausers") == 0)
				{
					SessionStats.modeSort = (int)RaceStatsSort.SORT_YELLOWFLAG;
					raceStatsList.Sort();
					// string outputBlock;
					curPos = 0;
					for (int i = 0; i < raceStatsList.Count; i++)
					{
						SessionStats p = raceStatsList[i];

						outputBlock.Clear();outputBlock.Append(inputBlock);
						if (p.yellowFlags == 0)
							continue;
						lnickname = lfsStripColor(p.NickName);
						outputBlock.Replace("[YellowFlagCausers", "");
						outputBlock.Replace("]", "");
						curPos++;
						outputBlock.Replace("{Position}", curPos.ToString());
						outputBlock.Replace("{PlayerName}", lnickname);
						outputBlock.Replace("{UserName}", p.userName);
						outputBlock.Replace("{YellowFlagsCount}", p.yellowFlags.ToString());
						sw.WriteLine(outputBlock.ToString());
					}
					continue;
				}
				if (readLine.IndexOf("[PitStops") == 0)
				{
					SessionStats.modeSort = (int)RaceStatsSort.SORT_PIT;
					raceStatsList.Sort();
					// string outputBlock;
					string pitinfo = "";
					curPos = 0;
					for (int i = 0; i < raceStatsList.Count; i++)
					{
						SessionStats p = raceStatsList[i];

						outputBlock.Clear();outputBlock.Append(inputBlock);
						lnickname = lfsStripColor(p.NickName);
						outputBlock.Replace("[PitStops", "");
						outputBlock.Replace("]", "");
						curPos++;
						outputBlock.Replace("{Position}", curPos.ToString());
						outputBlock.Replace("{PlayerName}", lnickname);
						outputBlock.Replace("{UserName}", p.userName);
						pitinfo = "";
						string br = "";
						string virg = "";
						string SWork = "";
						for (int j = 0; j < p.pit.Count; j++)
						{
							SWork = "{lg_lap} " + ((p.pit[j] as Pit).LapsDone + 1) + ":";
							//                            SWork = (p.pit[j] as Pit).Work + "{lg_lap} " + ((p.pit[j] as Pit).LapsDone + 1) + ":";
							if (isBitSet((p.pit[j] as Pit).Work, 1) == true)
							{
								SWork = SWork + virg + " {lg_stop}";
								virg = ", ";
							}
							if (isBitSet((p.pit[j] as Pit).Work, 14) == true)
							{
								SWork = SWork + virg + " {lg_minor}";
								virg = ", ";
							}
							if (isBitSet((p.pit[j] as Pit).Work, 15) == true)
							{
								SWork = SWork + virg + "{lg_major}";
								virg = ", ";
							}
							if (isBitSet((p.pit[j] as Pit).Work, 17) == true)
							{
								SWork = SWork + virg + "{lg_refuel}";
								virg = ", ";
							}
							if ((p.pit[j] as Pit).STime != 0)
								SWork = SWork + virg + "{lg_start}";
							SWork = updateGlob(SWork, fileName, currInfoRace, "qual");
							pitinfo = pitinfo + br + SWork + " (" + SessionStats.LfsTimeToString((p.pit[j] as Pit).STime) + ")";
							br = "<BR>";
						}
						outputBlock.Replace("{PitInfo}", pitinfo);
						outputBlock.Replace("{PitTime}", SessionStats.LfsTimeToString(p.cumuledStime));
						sw.WriteLine(outputBlock.ToString());
					}
					continue;
				}
				if (readLine.IndexOf("[TopSpeed") == 0)
				{
					SessionStats.modeSort = (int)RaceStatsSort.SORT_BESTSPEED;
					raceStatsList.Sort();
					// string outputBlock;
					curPos = 0;
					int baseSpeed = 0;
					for (int i = 0; i < raceStatsList.Count; i++)
					{
						SessionStats p = raceStatsList[i];

						outputBlock.Clear();outputBlock.Append(inputBlock);
						if (p.bestSpeed == 0)
							continue;
						lnickname = lfsStripColor(p.NickName);
						outputBlock.Replace("[TopSpeed", "");
						outputBlock.Replace("]", "");
						curPos++;
						if (curPos == 1)
							baseSpeed = p.bestSpeed;
						outputBlock.Replace("{Position}", curPos.ToString());
						outputBlock.Replace("{PlayerName}", lnickname);
						outputBlock.Replace("{UserName}", p.userName);
						outputBlock.Replace("{TopSpeed}", SessionStats.LfsSpeedToString(p.bestSpeed));
						outputBlock.Replace("{Difference}", SessionStats.LfsSpeedToString(baseSpeed - p.bestSpeed));
						outputBlock.Replace("{TopSpeedLap}", p.lapBestSpeed.ToString());
						sw.WriteLine(outputBlock.ToString());
					}
					continue;
				}

				readLine = updateGlob(readLine, fileName, currInfoRace, "qual");
				sw.WriteLine(readLine);
			}
			sw.Close();
			sr.Close();
		}*/

		public static string updateGlob(string str, string fileName, SessionInfo sessionInfo, string chatInX)
		{
			// predefined language file translation
			foreach (KeyValuePair<string, string> item in lang)
				str = str.Replace("{" + item.Key + "}", item.Value);

			// program information substitution
			str = str.Replace("{TITLE}", LFSStats.Title);
			str = str.Replace("{TITLELONG}", LFSStats.TitleLong);
			str = str.Replace("{VERSIONSHORT}", LFSStats.VersionShort);
			str = str.Replace("{VERSIONVSHORT}", LFSStats.VersionVShort);
			str = str.Replace("{VERSION}", LFSStats.Version);
			str = str.Replace("{VERSIONV}", LFSStats.VersionV);
			str = str.Replace("{LICENSENAME}", LFSStats.LicenseName);
			str = str.Replace("{LICENSENAMESHORT}", LFSStats.LicenseNameShort);
			str = str.Replace("{LICENSE}", LFSStats.License);
			str = str.Replace("{LICENSEURL}", LFSStats.LicenseUrl);
			str = str.Replace("{COPYRIGHT}", LFSStats.Copyright);
			str = str.Replace("{PROJECTURL}", LFSStats.ProjectUrl);
			// session information substitution
			str = str.Replace("{ServerName}", lfsColorToHtml(sessionInfo.HName));
			str = str.Replace("{TrackName}", InSimDotNet.Helpers.TrackHelper.GetFullTrackName(sessionInfo.trackNameCode));
			str = str.Replace("{TrackNameShort}", sessionInfo.trackNameCode);
			str = str.Replace("{TrackImg}", sessionInfo.trackNameCode.ToLower() + ".gif");

			if (sessionInfo.raceLaps == 0)			// Practice
				str = str.Replace("{SessionLength}", sessionInfo.sraceLaps);
			else if (sessionInfo.raceLaps == 1)		// 1 lap
				str = str.Replace("{SessionLength}", sessionInfo.sraceLaps + " " + lang["lg_durationLap"]);
			else if (sessionInfo.raceLaps < 191)	// 2-1000 laps
				str = str.Replace("{SessionLength}", sessionInfo.sraceLaps + " " + lang["lg_durationLaps"]);
			else if (sessionInfo.raceLaps == 191)	// 1 hour
				str = str.Replace("{SessionLength}", sessionInfo.sraceLaps + " " + lang["lg_durationHour"]);
			else									// 2-48 hours
				str = str.Replace("{SessionLength}", sessionInfo.sraceLaps + " " + lang["lg_durationHours"]);

			if (sessionInfo.qualMins == 1)
				str = str.Replace("{SessionLength}", sessionInfo.qualMins.ToString() + " " + lang["lg_durationMin"]);
			if (sessionInfo.qualMins > 1)
				str = str.Replace("{SessionLength}", sessionInfo.qualMins.ToString() + " " + lang["lg_durationMins"]);

			str = str.Replace("{TrackConditions}", strWeather[sessionInfo.weather] + ", " + strWind[sessionInfo.wind]);
			var baseName = fileName.Replace(".html", string.Empty);
			str = str.Replace("{LapByLapGraphFileName}", baseName + "_lbl.png");
			str = str.Replace("{RaceProgressGraphFileName}", baseName + "_rpr.png");
			str = str.Replace("{CombinedBestLap}", SessionStats.LfsTimeToString(combinedBestLap));

			return str;
		}
	}
}