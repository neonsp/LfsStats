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
using System;
using System.Text;

namespace LFSStat
{
    
    class exportstats
    {
        public static System.Collections.Hashtable lang = new System.Collections.Hashtable();
        public static long combinedSplit = 0;
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

        public static string lfsStripColor(string str)
        {
            str = str.Replace("^0", "")
                        .Replace("^1", "")
                        .Replace("^2", "")
                        .Replace("^3", "")
                        .Replace("^4", "")
                        .Replace("^5", "")
                        .Replace("^6", "")
                        .Replace("^7", "")
                        .Replace("^8", "")
                        .Replace("^9", "")
                        .Replace("^O", "")
                        .Replace("^a", "*")
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
                        .Replace("^B", "")
                ;
            return str;
        }
		public static string lfsChatTextToHtml(string str)
		{
			str = str.Replace("<", "&lt;")
						.Replace(">", "&gt;")
						.Replace("^^", "&circ;")
						.Replace("^E", "")
			;
			return str;
		}
        public static string lfsColorToHtml(string str )
        {
            int origLen = str.Length;

            str = str.Replace("^0", "</span><span style=\"color: black;\">")
					.Replace("^1", "</span><span style=\"color: red;\">")
					.Replace("^2", "</span><span style=\"color: green;\">")
					.Replace("^3", "</span><span style=\"color: yellow;\">")
					.Replace("^4", "</span><span style=\"color: blue;\">")
					.Replace("^5", "</span><span style=\"color: pink;\">")
					.Replace("^6", "</span><span style=\"color: Turquoise;\">")
					.Replace("^7", "</span><span style=\"color: white;\">")
					.Replace("^8", "</span><span style=\"color: black;\">")
					.Replace("^9", "</span><span style=\"color: black;\">");
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
                                        .Replace("^B", "")
            ;
            return str;
        }
        public static void getLang(string theLang)
        {
            string readLine;
            int linecounter = 0;
            string val;
            string key;

            string scriptfilename = "./lang/" + theLang + ".txt";
            using (System.IO.StreamReader sr = new System.IO.StreamReader(scriptfilename))
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
                    lang[ key ] = val;
                }
            }
//endlang:
            strWind[0] = (string)lang["lg_wind0"];
            strWind[1] = (string)lang["lg_wind1"];
            strWind[2] = (string)lang["lg_wind2"];
            strWeather[0] = (string)lang["lg_weather0"];
            strWeather[1] = (string)lang["lg_weather1"];
            strWeather[2] = (string)lang["lg_weather2"];
            strWeather[3] = (string)lang["lg_weather3"];
            strWeather[4] = (string)lang["lg_weather4"];
            strWeather[5] = (string)lang["lg_weather5"];
            strWeather[6] = (string)lang["lg_weather6"];
            strWeather[7] = (string)lang["lg_weather7"];
            strWeather[8] = (string)lang["lg_weather8"];
            strWeather[9] = (string)lang["lg_weather9"];
            strWeather[10] = (string)lang["lg_weather10"];

            

        }

        public static void csvResult( System.Collections.Hashtable raceStat, string datFile, string raceDir, infoRace currInfoRace )
        {
            string formatLine;
            int firstMaxLap = 0;
            long firstTotalTime = 0;

            using (System.IO.StreamReader sr = new System.IO.StreamReader("templates/csv_race.tpl"))
            {
                formatLine = sr.ReadLine();
                if (formatLine == null)
                    formatLine = "[RaceResults {Position},{PlayerName},{UserName},{Car},{Gap},{BestLap},{LapsDone},{PitsDone},{Penalty},{Flags}]";
            }
            System.Collections.ArrayList sorted = new System.Collections.ArrayList();
            System.Collections.IDictionaryEnumerator tmpRaceStat = raceStat.GetEnumerator();
            while (tmpRaceStat.MoveNext())	//for each player
            {
                raceStats p = (raceStats)tmpRaceStat.Value;
                sorted.Add(p);
            }
            raceStats.modeSort = (int)sortRaceStats.SORT_RESULT;
            sorted.Sort();
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(raceDir + "/" + datFile + "_results_race.csv"))
            {
                int curPos = 0;
                for (int i = 0; i < sorted.Count; i++)
                {
                    curPos++;
                    raceStats p = (raceStats)sorted[i];
                    if (i == 0)
                    {
                        firstMaxLap = p.lap.Count;
                        firstTotalTime = p.totalTime;
                    }
                    string resultLine = formatLine;
                    resultLine = resultLine.Replace("[RaceResults ", "");
                    resultLine = resultLine.Replace("]", "");
                    resultLine = resultLine.Replace("{Position}", curPos.ToString());
                    //                    resultLine = resultLine.Replace("{Position}", p.resultNum.ToString() );
                    resultLine = resultLine.Replace("{PlayerName}", p.nickName.Replace("^0", "").Replace("^1", "").Replace("^2", "").Replace("^3", "").Replace("^4", "").Replace("^5", "").Replace("^6", "").Replace("^7", "").Replace("^8", ""));
                    resultLine = resultLine.Replace("{UserName}", p.userName);
                    resultLine = resultLine.Replace("{Car}", p.CName);
                    // if Racer do not finish
                    if (p.resultNum == 999)
                        resultLine = resultLine.Replace("{Gap}", "DNF");
                    else
                    {
                        if (firstMaxLap == p.lap.Count)
                        {
                            if (i == 0)
                                resultLine = resultLine.Replace("{Gap}", raceStats.LfstimeToString(p.totalTime));
                            else
                            {
                                long tres;
                                tres = p.totalTime - firstTotalTime;
                                resultLine = resultLine.Replace("{Gap}", "+" + raceStats.LfstimeToString(tres));
                            }
                        }
                        else
                            resultLine = resultLine.Replace("{Gap}", "+" + ((int)(firstMaxLap - p.lap.Count)).ToString() + " laps");
                    }
                    resultLine = resultLine.Replace("{BestLap}", raceStats.LfstimeToString(p.bestLap));
                    resultLine = resultLine.Replace("{LapsDone}", p.lap.Count.ToString());
                    resultLine = resultLine.Replace("{PitsDone}", p.numStop.ToString());
                    resultLine = resultLine.Replace("{Penalty}", p.penalty);
                    resultLine = resultLine.Replace("{PosGrid}", p.gridPos.ToString());
                    resultLine = resultLine.Replace("{Flags}", p.sFlags);
                    sw.WriteLine(resultLine);
                }
            }
        }

        public static void tsvResult(System.Collections.Hashtable raceStat, string datFile, string raceDir, infoRace currInfoRace)
        {
            System.Collections.ArrayList sorted = new System.Collections.ArrayList();
            System.Collections.IDictionaryEnumerator tmpRaceStat = raceStat.GetEnumerator();
            while (tmpRaceStat.MoveNext())	//for each player
            {
                raceStats p = (raceStats)tmpRaceStat.Value;
                sorted.Add(p);
            }
            raceStats.modeSort = (int)sortRaceStats.SORT_GRID;
            sorted.Sort();
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(raceDir + "/" + datFile + "_results_race_extended.tsv"))
            {
                sw.WriteLine(currInfoRace.maxSplit + 1);

                for (int i = 0; i < sorted.Count; i++)
                {
                    raceStats p = (raceStats)sorted[i];

                    sw.Write(p.nickName.Replace("^0", "").Replace("^1", "").Replace("^2", "").Replace("^3", "").Replace("^4", "").Replace("^5", "").Replace("^6", "").Replace("^7", "").Replace("^8", ""));
                    long lastCumul = 0;
                    for (int j = 0; j < p.lap.Count; j++)
                    {
// case of last Lap not completed                        
                        if ((p.lap[j] as Lap).lapTime == 0)
                            continue;
                        if (currInfoRace.maxSplit >= 1)
                        {
                            sw.Write("\t" + (lastCumul + (p.lap[j] as Lap).split1) * 10);
                            //                            sw.Write("(" + LfstimeToString((p.lap[j] as Lap).split1) + "," + LfstimeToString((p.lap[j] as Lap).lapTime) + ")");
                        }
                        if (currInfoRace.maxSplit >= 2)
                        {
                            sw.Write("\t" + (lastCumul + (p.lap[j] as Lap).split2) * 10);
                        }
                        if (currInfoRace.maxSplit >= 3)
                        {
                            sw.Write("\t" + (lastCumul + (p.lap[j] as Lap).split3) * 10);
                        }
                        sw.Write("\t" + (lastCumul + (p.lap[j] as Lap).lapTime) * 10);
                        lastCumul = lastCumul + (p.lap[j] as Lap).lapTime;
                    }
                    sw.Write("\r\n");
                }
            }
        }

        public static void chatResult(System.Collections.Hashtable raceStat, string datFile, string raceDir, infoRace currInfoRace, string chatInX)
        {
			// add InRace and InQual then decide!
			System.IO.StreamWriter sw = new System.IO.StreamWriter(raceDir + "/" + datFile + "_chat_" + chatInX + ".html");
            System.IO.StreamReader sr = new System.IO.StreamReader("./templates/html_chat.tpl");
            string readLine;
            combinedSplit = 0;

            while (true)
            {
                readLine = sr.ReadLine();
                if (readLine == null)
                    break;
                if (readLine.IndexOf("{IngameChatLog}") !=-1 )
                {
                    readLine = readLine.Replace("{IngameChatLog}","|" );
                    string [] str = readLine.Split('|');
                    sw.WriteLine(str[0]);
                    for (int i = 0; i < currInfoRace.chat.Count; i++)
                        sw.WriteLine(currInfoRace.chat[i] + "<BR>");
                    sw.WriteLine(str[1]);
                }
                else
                {
					readLine = updateGlob(readLine, datFile, currInfoRace, chatInX);
					sw.WriteLine(readLine);
                }
            }
            sw.Close();
            sr.Close();

        }
        public static  string getAllUserName(raceStats p)
        {
            string retValue = "";
            string br = "";
            foreach (System.Collections.DictionaryEntry un in p.allUN)
            {
                retValue = retValue + br + (un.Value as UN).userName;
                br = "<BR>";
            }
            return retValue;

        }
        public static string getAllNickName(raceStats p)
        {
            string retValue = "";
            string br = "";
            foreach (System.Collections.DictionaryEntry nn in p.allUN)
            {
                retValue = retValue + br + (nn.Value as UN).nickName;
                br = "<BR>";
            }
            return retValue;

        }
        public static void htmlResult(System.Collections.Hashtable raceStat, string datFile, string raceDir, infoRace currInfoRace)
        {
            int firstMaxLap = 0;
            long firstTotalTime = 0;
            int curPos;
            wr.wrInfo wi;



            System.Collections.ArrayList sorted = new System.Collections.ArrayList();
            System.Collections.IDictionaryEnumerator tmpRaceStat = raceStat.GetEnumerator();
            while (tmpRaceStat.MoveNext())	//for each player
            {
                raceStats p = (raceStats)tmpRaceStat.Value;
                sorted.Add(p);
            }
            // Do Stat on Leader of race
            System.Collections.ArrayList raceLeader = new System.Collections.ArrayList();
            int CurLap = -1;
            long minLapTime;
            int PLID;
            for (int i = 0; i < sorted.Count; i++)
            {
                raceStats p = (raceStats)sorted[i];
                p.tmpTime = 0;
            }
// Set Lap By Lap of Leader
            while (true)
            {
                CurLap++;
                minLapTime = 0;
                PLID = -1;
                for (int i = 0; i < sorted.Count; i++)
                {
                    raceStats p = (raceStats)sorted[i];
                    if (p.lap.Count > CurLap)
                    {
                        p.tmpTime = p.tmpTime + (p.lap[CurLap] as Lap).lapTime;
                        if (minLapTime == 0 || p.tmpTime < minLapTime)
                        {
                            PLID = p.PLID;
                            minLapTime = p.tmpTime;
                        }
                    }
                }
                if (PLID == -1)
                    goto fin;
                (raceStat[PLID] as raceStats).lapsLead++;
                raceLeader.Add(PLID);
            }
        fin:
// CALC avg
            for (int i = 0; i < sorted.Count; i++)
            {
                raceStats p = (raceStats)sorted[i];
                try
                {
                    if( p.lap.Count != 0 )
                        p.avgTime = p.cumuledTime / p.lap.Count;
                }
                catch
                {
                    p.avgTime = 0;
                }
            }
// CALC lapStability
            for (int i = 0; i < sorted.Count; i++)
            {
                raceStats p = (raceStats)sorted[i];

                if (p.avgTime == 0)
                {
                    p.lapStability = -1;
                    continue;
                }
                for (int j = 0; j < p.lap.Count; j++)
                {
                    p.lapStability += System.Math.Pow((double)(p.avgTime - (p.lap[j] as Lap).lapTime), 2);
                }
                if (p.lap.Count > 1)
                    p.lapStability = System.Math.Sqrt(p.lapStability / ((p.lap.Count) - 1));
                else
                    p.lapStability = -1;
            }


            System.IO.StreamWriter sw = new System.IO.StreamWriter(raceDir + "/" + datFile + "_results_race.html");
            System.IO.StreamReader sr = new System.IO.StreamReader("./templates/html_race.tpl");
            string readLine;
            combinedSplit = 0;

            while (true)
            {
                readLine = sr.ReadLine();
                if (readLine == null)
                    break;

                #region RaceResults
				// requires "[Tag" at the beginning of an output line, this single line is then processed
                if (readLine.IndexOf("[RaceResults") == 0)
                {
                    raceStats.modeSort = (int)sortRaceStats.SORT_RESULT;
					sorted.Sort();
					string wreadLine;
                    curPos = 0;
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        curPos++;
                        wreadLine = readLine;
                        raceStats p = (raceStats)sorted[i];

                        /*                        string lnickname = lfsColorToHtml( p.nickName ); */
                        string lnickname = lfsColorToHtml(getAllNickName(p));
                        string allUserName = getAllUserName(p);
                        if (i == 0)
                        {
                            firstMaxLap = p.lap.Count;
                            firstTotalTime = p.totalTime;
                        }
                        wreadLine = wreadLine.Replace("[RaceResults ", "");
                        wreadLine = wreadLine.Replace("]", "");
                        p.finalPos = curPos;
                        wreadLine = wreadLine.Replace("{Position}", p.finalPos.ToString());
                        wreadLine = wreadLine.Replace("{PlayerNameColoured}", lnickname);
                        wreadLine = wreadLine.Replace("{UserNameLink}", p.userName);
//                        wreadLine = wreadLine.Replace("{UserName}", p.userName);
                        wreadLine = wreadLine.Replace("{UserName}", allUserName );
                        wreadLine = wreadLine.Replace("{Car}", p.CName);
                        wreadLine = wreadLine.Replace("{Plate}", lfsStripColor( p.Plate ));
                        wi = wr.getWR(currInfoRace.currentTrackName, p.CName);
                        if( wi == null )
                            wreadLine = wreadLine.Replace("{DifferenceToWR}", "");
                        else           
                            wreadLine = wreadLine.Replace("{DifferenceToWR}", raceStats.LfstimeToString(p.bestLap - wi.WRTime));


                        // if Racer did not finish
                        if (p.resultNum == 999)
                            wreadLine = wreadLine.Replace("{Gap}", "DNF");
                        else
                        {
                            if (firstMaxLap == p.lap.Count)
                            {
                                if (i == 0)
                                    wreadLine = wreadLine.Replace("{Gap}", raceStats.LfstimeToString(p.totalTime));
                                else
                                {
                                    long tres;
                                    tres = p.totalTime - firstTotalTime;
                                    wreadLine = wreadLine.Replace("{Gap}", "+" + raceStats.LfstimeToString(tres));
                                }
                            }
                            else
                                wreadLine = wreadLine.Replace("{Gap}", "+" + ((int)(firstMaxLap - p.lap.Count)).ToString() + " laps");
                        }
                        wreadLine = wreadLine.Replace("{BestLap}", raceStats.LfstimeToString(p.bestLap));
                        wreadLine = wreadLine.Replace("{LapsDone}", p.lap.Count.ToString());
                        wreadLine = wreadLine.Replace("{PitsDone}", p.numStop.ToString());
                        wreadLine = wreadLine.Replace("{Flags}", p.sFlags);
                        wreadLine = wreadLine.Replace("{Penalty}", p.penalty);
												
                        sw.WriteLine(wreadLine);	// writes results into output as a single line
                    }
                    continue;
                }
#endregion

                #region StartOrder

                if (readLine.IndexOf("[StartOrder") == 0)
                {
                    raceStats.modeSort = (int)sortRaceStats.SORT_GRID;
                    sorted.Sort();
                    string wreadLine;
                    curPos = 0;
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        wreadLine = readLine;
                        raceStats p = (raceStats)sorted[i];
                        string lnickname = lfsStripColor(getAllNickName( p ));
                        string allUserName = getAllUserName(p);


                        wreadLine = wreadLine.Replace("[StartOrder ", "");
                        wreadLine = wreadLine.Replace("]", "");
                        p.finalPos = curPos;
                        string gridPos;
                        if (p.gridPos == 999)
                            gridPos = "-";
                        else
                            gridPos = p.gridPos.ToString();
                        wreadLine = wreadLine.Replace("{Position}", gridPos);
						wreadLine = wreadLine.Replace("{UserNameLink}", p.userName);
                        wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
                        wreadLine = wreadLine.Replace("{UserName}", allUserName);
                        sw.WriteLine(wreadLine);
                    }
                    continue;
                }
#endregion

                #region HighestClimber
                if (readLine.IndexOf("[HighestClimber") == 0)
                {
                    raceStats.modeSort = (int)sortRaceStats.SORT_CLIMB;
                    sorted.Sort();
                    string wreadLine;
                    curPos = 0;
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        wreadLine = readLine;
                        raceStats p = (raceStats)sorted[i];
//                        string lnickname = lfsStripColor(p.nickName);
                        string lnickname = lfsStripColor(getAllNickName(p));
                        string allUserName = getAllUserName(p);


                        wreadLine = wreadLine.Replace("[HighestClimber ", "");
                        wreadLine = wreadLine.Replace("]", "");
                        p.finalPos = curPos;
                        if (p.gridPos == 999 || p.resultNum >= 998)
                            continue;
                        curPos++;
                        wreadLine = wreadLine.Replace("{Position}", curPos.ToString());
//                        wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
//                        wreadLine = wreadLine.Replace("{UserName}", p.userName);
                        wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
                        wreadLine = wreadLine.Replace("{UserName}", allUserName);
                        wreadLine = wreadLine.Replace("{StartPos}", p.gridPos.ToString());
                        wreadLine = wreadLine.Replace("{FinishPos}", (p.resultNum + 1).ToString());
                        wreadLine = wreadLine.Replace("{Difference}", (p.gridPos - p.resultNum - 1).ToString());

                        sw.WriteLine(wreadLine);
                    }
                    continue;
                }
#endregion

                #region RaceLeader
                if (readLine.IndexOf("[RaceLeader") == 0)
                {
                    raceStats.modeSort = (int)sortRaceStats.SORT_LAPLEAD;
                    sorted.Sort();
                    string wreadLine;
                    curPos = 0;
                    int oldPLID = -1;
                    int initLap = 0;
                    int endLap = 0;
                    int i;
                    string lnickname;
                    int curPLID = 0;
                    for (i = 0; i < raceLeader.Count; i++)
                    {
                        curPLID = (int)raceLeader[i];
                        if (oldPLID != curPLID)
                        {
                            if (oldPLID != -1)
                            {

                                raceStats p = (raceStats)raceStat[oldPLID];

//                                lnickname = lfsStripColor(p.nickName);
                                lnickname = lfsStripColor(getAllNickName(p));
                                string allUserName = getAllUserName(p);

                                curPos++;
                                endLap = i;
                                wreadLine = readLine;
                                wreadLine = wreadLine.Replace("[RaceLeader ", "");
                                wreadLine = wreadLine.Replace("]", "");
                                wreadLine = wreadLine.Replace("{Position}", curPos.ToString());
//                                wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
//                                wreadLine = wreadLine.Replace("{UserName}", p.userName);
                                wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
                                wreadLine = wreadLine.Replace("{UserName}", allUserName);

                                wreadLine = wreadLine.Replace("{LapsLead}", initLap.ToString() + "-" + endLap.ToString());
                                sw.WriteLine(wreadLine);
                            }
                            oldPLID = curPLID;
                            initLap = i + 1;
                        }
                    }
                    if (oldPLID != -1)
                    {
                        raceStats p = (raceStats)raceStat[curPLID];
//                        lnickname = lfsStripColor(p.nickName);
                        lnickname = lfsStripColor(getAllNickName(p));
                        string allUserName = getAllUserName(p);

                        curPos++;
                        endLap = i;
                        wreadLine = readLine;
                        wreadLine = wreadLine.Replace("[RaceLeader ", "");
                        wreadLine = wreadLine.Replace("]", "");
                        wreadLine = wreadLine.Replace("{Position}", curPos.ToString());
//                        wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
//                        wreadLine = wreadLine.Replace("{UserName}", p.userName);
                        wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
                        wreadLine = wreadLine.Replace("{UserName}", allUserName);
                        wreadLine = wreadLine.Replace("{LapsLead}", initLap.ToString() + "-" + endLap.ToString());
                        sw.WriteLine(wreadLine);
                    }
                    continue;
                }
#endregion

                #region LapsLed

                if (readLine.IndexOf("[LapsLed") == 0)
                {
                    raceStats.modeSort = (int)sortRaceStats.SORT_LAPLEAD;
                    sorted.Sort();
                    string wreadLine;
                    curPos = 0;
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        wreadLine = readLine;
                        raceStats p = (raceStats)sorted[i];
//                        string lnickname = lfsStripColor(p.nickName);
                        string lnickname = lfsStripColor(getAllNickName(p));
                        string allUserName = getAllUserName(p);


                        wreadLine = wreadLine.Replace("[LapsLed ", "");
                        wreadLine = wreadLine.Replace("]", "");
                        p.finalPos = curPos;
                        if (p.lapsLead == 0)
                            continue;
                        curPos++;
                        wreadLine = wreadLine.Replace("{Position}", curPos.ToString());
//                        wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
//                        wreadLine = wreadLine.Replace("{UserName}", p.userName);
                        wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
                        wreadLine = wreadLine.Replace("{UserName}", allUserName);

                        wreadLine = wreadLine.Replace("{LapsLed}", p.lapsLead.ToString());

                        sw.WriteLine(wreadLine);
                    }
                    continue;
                }
#endregion

                #region relay

                if (readLine.IndexOf("[Relay") == 0)
                {
                    if (currInfoRace.isToc) // if TOC on this race
                    {
                        raceStats.modeSort = (int)sortRaceStats.SORT_RESULT;
                        sorted.Sort();
                        string wreadLine;
                        curPos = 0;
                        for (int i = 0; i < sorted.Count; i++)
                        {
                            wreadLine = readLine;
                            raceStats p = (raceStats)sorted[i];
                            //                        string lnickname = lfsStripColor(p.nickName);
                            string lnickname = lfsStripColor(getAllNickName(p));
                            string allUserName = getAllUserName(p);


                            wreadLine = wreadLine.Replace("[Relay ", "");
                            wreadLine = wreadLine.Replace("]", "");
                            p.finalPos = curPos;
                            //                            if (p.toc.Count == 0)   // No toc for this Racer
                            //                                continue;
                            curPos++;
                            wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
                            wreadLine = wreadLine.Replace("{UserName}", allUserName);
                            string allRelays = "";
                            string allLaps = "";
                            string br = "";
                            int last_lap = 1;
                            int nbLaps = 0;
                            for (int j = 0; j < p.toc.Count; j++)
                            {
                                allRelays = allRelays
                                            + br
                                            + lfsStripColor((p.toc[j] as Toc).oldNickName);
                                //                                        + " -> "
                                //                                        + lfsStripColor((p.toc[j] as Toc).newNickName);
                                ;
                                nbLaps = (p.toc[j] as Toc).lap - last_lap + 1;
                                allLaps = allLaps
                                            + br
                                            + (last_lap).ToString() + "-" + (p.toc[j] as Toc).lap + " (" + nbLaps + " " + lang["lg_laps"] + ")";
                                ;
                                last_lap = (p.toc[j] as Toc).lap + 1;
                                br = "<br>";

                            }
                            allRelays = allRelays
                                        + br
                                        + lfsStripColor(p.nickName);
                            if (last_lap <= p.lap.Count)
                            {
                                nbLaps = p.lap.Count - last_lap + 1;
                                allLaps = allLaps
                                            + br
                                            + (last_lap).ToString() + "-" + (p.lap.Count).ToString() + " (" + nbLaps + " " + lang["lg_laps"] + ")";
                                ;
                            }
                            else
                            {
                                nbLaps = 0;
                                allLaps = allLaps
                                            + br
                                            + (p.lap.Count).ToString() + "-" + (p.lap.Count).ToString() + " (" + nbLaps + " " + lang["lg_laps"] + ")";
                                ;
                            }
                            wreadLine = wreadLine.Replace("{Position}", (i + 1).ToString());
                            wreadLine = wreadLine.Replace("{Relays}", allRelays);
                            wreadLine = wreadLine.Replace("{Laps}", allLaps);
                            sw.WriteLine(wreadLine);
                        }

                        continue;
                    }
                    else
                        continue;
                }

                #endregion

                #region FirstLap
                if (readLine.IndexOf("[FirstLap") == 0)
                {
                    raceStats.modeSort = (int)sortRaceStats.SORT_FIRSTLAP;
                    sorted.Sort();
                    string wreadLine;
                    curPos = 0;
                    long baseTime = 0;
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        raceStats p = (raceStats)sorted[i];

                        wreadLine = readLine;
                        if (p.firstTime == 0)
                            continue;

//                        string lnickname = lfsStripColor(p.nickName);
                        string lnickname = lfsStripColor(getAllNickName(p));
                        string allUserName = getAllUserName(p);


                        wreadLine = wreadLine.Replace("[FirstLap ", "");
                        wreadLine = wreadLine.Replace("]", "");
                        curPos++;
                        if (curPos == 1)
                            baseTime = p.firstTime;
                        wreadLine = wreadLine.Replace("{Position}", curPos.ToString());

//                        wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
//                        wreadLine = wreadLine.Replace("{UserName}", p.userName);
                        wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
                        wreadLine = wreadLine.Replace("{UserName}", allUserName);

                        wreadLine = wreadLine.Replace("{LapTime}", raceStats.LfstimeToString(p.firstTime));
                        wreadLine = wreadLine.Replace("{Difference}", raceStats.LfstimeToString(p.firstTime - baseTime));
                        sw.WriteLine(wreadLine);
                    }
                    continue;
                }
#endregion

                #region LapTimesStability
                if (readLine.IndexOf("[LapTimesStability") == 0)
                {
                    raceStats.modeSort = (int)sortRaceStats.SORT_STABILITY;
                    sorted.Sort();
                    string wreadLine;
                    curPos = 0;
                    double baseTime = 0;
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        raceStats p = (raceStats)sorted[i];

                        wreadLine = readLine;
                        if (p.lapStability <= 0  )
                            continue;
//                        string lnickname = lfsStripColor(p.nickName);
                        string lnickname = lfsStripColor(getAllNickName(p));
                        string allUserName = getAllUserName(p);

                        wreadLine = wreadLine.Replace("[LapTimesStability ", "");
                        wreadLine = wreadLine.Replace("]", "");
                        curPos++;
                        if (curPos == 1)
                            baseTime = p.lapStability;
                        wreadLine = wreadLine.Replace("{Position}", curPos.ToString());

//                        wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
//                        wreadLine = wreadLine.Replace("{UserName}", p.userName);
                        wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
                        wreadLine = wreadLine.Replace("{UserName}", allUserName);

                        wreadLine = wreadLine.Replace("{Deviation}", raceStats.LfstimeToString((long)p.lapStability));
                        wreadLine = wreadLine.Replace("{Difference}", raceStats.LfstimeToString((long)(p.lapStability - baseTime)));
                        wreadLine = wreadLine.Replace("{LapsDone}", p.lap.Count.ToString());

                        sw.WriteLine(wreadLine);
                    }
                    continue;

                }
#endregion

                #region AverageLap
                if (readLine.IndexOf("[AverageLap") == 0)
                {
                    raceStats.modeSort = (int)sortRaceStats.SORT_AVGTIME;
                    sorted.Sort();
                    string wreadLine;
                    curPos = 0;
                    long baseTime = 0;
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        raceStats p = (raceStats)sorted[i];

                        wreadLine = readLine;
                        if (p.avgTime == 0)
                            continue;
//                        string lnickname = lfsStripColor(p.nickName);
                        string lnickname = lfsStripColor(getAllNickName(p));
                        string allUserName = getAllUserName(p);

                        wreadLine = wreadLine.Replace("[AverageLap ", "");
                        wreadLine = wreadLine.Replace("]", "");
                        curPos++;
                        if (curPos == 1)
                            baseTime = p.avgTime;
                        wreadLine = wreadLine.Replace("{Position}", curPos.ToString());

//                        wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
//                        wreadLine = wreadLine.Replace("{UserName}", p.userName);
                        wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
                        wreadLine = wreadLine.Replace("{UserName}", allUserName);

                        wreadLine = wreadLine.Replace("{LapTime}", raceStats.LfstimeToString(p.avgTime));
                        wreadLine = wreadLine.Replace("{Difference}", raceStats.LfstimeToString(p.avgTime - baseTime));
                        wreadLine = wreadLine.Replace("{Laps}", p.lap.Count.ToString());
                        sw.WriteLine(wreadLine);
                    }
                    continue;
                }
#endregion

                #region BestLap
                if (readLine.IndexOf("[BestLap") == 0)
                {
                    raceStats.modeSort = (int)sortRaceStats.SORT_BESTLAP;
                    sorted.Sort();
                    string wreadLine;
                    curPos = 0;
                    long baseTime = 0;
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        raceStats p = (raceStats)sorted[i];

                        wreadLine = readLine;
                        if (p.bestLap == 0)
                            continue;

//                        string lnickname = lfsStripColor(p.nickName);
                        string lnickname = lfsStripColor(getAllNickName(p));
                        string allUserName = getAllUserName(p);


                        wreadLine = wreadLine.Replace("[BestLap ", "");
                        wreadLine = wreadLine.Replace("]", "");
                        curPos++;
                        if (curPos == 1)
                            baseTime = p.bestLap;
                        wreadLine = wreadLine.Replace("{Position}", curPos.ToString());

//                        wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
//                        wreadLine = wreadLine.Replace("{UserName}", p.userName);
                        wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
                        wreadLine = wreadLine.Replace("{UserName}", allUserName);

                        wreadLine = wreadLine.Replace("{LapTime}", raceStats.LfstimeToString(p.bestLap));
                        wreadLine = wreadLine.Replace("{Difference}", raceStats.LfstimeToString(p.bestLap - baseTime));
                        wi = wr.getWR( currInfoRace.currentTrackName,p.CName );
                        if (wi == null)
                            wreadLine = wreadLine.Replace("{DifferenceToWR}", "");
                        else
                            wreadLine = wreadLine.Replace("{DifferenceToWR}", raceStats.LfstimeToString(p.bestLap - wi.WRTime));
                        wreadLine = wreadLine.Replace("{Lap}", p.lapBestLap.ToString());
                        sw.WriteLine(wreadLine);
                    }
                    continue;
                }
#endregion

                #region BestSplitTable
                if (readLine.IndexOf("[[BestSplitTable") == 0)
                {
                    System.Collections.ArrayList blockSplit = new System.Collections.ArrayList();
                    blockSplit.Add(readLine);
                    while (true)
                    {
                        readLine = sr.ReadLine();	// end of file encountered
                        if (readLine == null)
                            break;
                        if (readLine.IndexOf("]]") > -1)	// end of block found
                        {
                            blockSplit.Add(readLine);
                            break;
                        }
                        blockSplit.Add(readLine);
                    }
                    for (int j = 0; j <= currInfoRace.maxSplit; j++)
                    {
                        for (int i = 0; i < sorted.Count; i++)
                        {
                            raceStats p = (raceStats)sorted[i];
                            wi = wr.getWR(currInfoRace.currentTrackName, p.CName);
                            if (j == currInfoRace.maxSplit)
                            {
                                p.curBestSplit = p.bestLastSplit;
                                p.curLapBestSplit = p.lapBestLastSplit; 
                                if (wi == null)
                                    p.curWrSplit = 0;
                                else
                                    p.curWrSplit = wi.sectorSplitLast;
                                continue;
                            }
                            if (j == 0)
                            {
                                p.curBestSplit = p.bestSplit1;
                                p.curLapBestSplit = p.lapBestSplit1;
                                if (wi == null)
                                    p.curWrSplit = 0;
                                else           
                                    p.curWrSplit = wi.sectorSplit[j];
                                continue;
                            }
                            if (j == 1)
                            {
                                p.curBestSplit = p.bestSplit2;
                                p.curLapBestSplit = p.lapBestSplit2;
                                if (wi == null)
                                    p.curWrSplit = 0;
                                else
                                    p.curWrSplit = wi.sectorSplit[j];
                                continue;
                            }
                            if (j == 2)
                            {
                                p.curBestSplit = p.bestSplit3;
                                p.curLapBestSplit = p.lapBestSplit3;
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
                                raceStats.modeSort = (int)sortRaceStats.SORT_BESTSPLIT;
                                sorted.Sort();
                                string wreadLine;
                                curPos = 0;
                                long baseTime = 0;
                                for (int i = 0; i < sorted.Count; i++)
                                {
                                    raceStats p = (raceStats)sorted[i];

                                    wreadLine = readLine;
                                    if (p.curBestSplit == 0)
                                        continue;
//                                  string lnickname = lfsStripColor(p.nickName);
                                    string lnickname = lfsStripColor(getAllNickName(p));
                                    string allUserName = getAllUserName(p);


                                    wreadLine = wreadLine.Replace("[BestSplit ", "");
                                    wreadLine = wreadLine.Replace("]", "");
                                    curPos++;
                                    if (curPos == 1)
                                        baseTime = p.curBestSplit;
                                    wreadLine = wreadLine.Replace("{Position}", curPos.ToString());

                                    wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
                                    wreadLine = wreadLine.Replace("{UserName}", allUserName);
//                                  wreadLine = wreadLine.Replace("{UserName}", p.userName);

                                    wreadLine = wreadLine.Replace("{SplitTime}", raceStats.LfstimeToString(p.curBestSplit));
                                    wreadLine = wreadLine.Replace("{Difference}", raceStats.LfstimeToString(p.curBestSplit - baseTime));
                                    if( p.curWrSplit == 0 )
                                        wreadLine = wreadLine.Replace("{DifferenceToWR}", "");
                                    else
                                        wreadLine = wreadLine.Replace("{DifferenceToWR}", raceStats.LfstimeToString(p.curBestSplit - p.curWrSplit));
                                    wreadLine = wreadLine.Replace("{Lap}", p.curLapBestSplit.ToString());
                                    sw.WriteLine(wreadLine);
                                }
                                combinedSplit = combinedSplit + baseTime;
                                continue;
                            }
                            readLine = updateGlob(readLine,  datFile, currInfoRace, "race");
                            readLine = readLine.Replace("]]", "");
                            readLine = readLine.Replace("[[BestSplitTable ", "");
                            readLine = readLine.Replace("{SplitNumber}", (j + 1).ToString());
                            sw.WriteLine(readLine);
                        }
                    }
                }
#endregion

                #region BestPossibleLap
                if (readLine.IndexOf("[BestPossibleLap") == 0)
                {
                    raceStats.modeSort = (int)sortRaceStats.SORT_TPB;
                    sorted.Sort();
                    string wreadLine;
                    curPos = 0;
                    long baseTime = 0;
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        raceStats p = (raceStats)sorted[i];

                        wreadLine = readLine;
                        if ((p.bestSplit1 + p.bestSplit2 + p.bestSplit3 + p.bestLastSplit) == 0)
                            continue;

//                        string lnickname = lfsStripColor(p.nickName);
                        string lnickname = lfsStripColor(getAllNickName(p));
                        string allUserName = getAllUserName(p);

                        wreadLine = wreadLine.Replace("[BestPossibleLap ", "");
                        wreadLine = wreadLine.Replace("]", "");
                        curPos++;
                        if (curPos == 1)
                            baseTime = p.bestSplit1 + p.bestSplit2 + p.bestSplit3 + p.bestLastSplit;
                        wreadLine = wreadLine.Replace("{Position}", curPos.ToString());

//                        wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
//                        wreadLine = wreadLine.Replace("{UserName}", p.userName);
                        wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
                        wreadLine = wreadLine.Replace("{UserName}", allUserName);

                        wreadLine = wreadLine.Replace("{LapTime}", raceStats.LfstimeToString(p.bestSplit1 + p.bestSplit2 + p.bestSplit3 + p.bestLastSplit));
                        wreadLine = wreadLine.Replace("{Difference}", raceStats.LfstimeToString((p.bestSplit1 + p.bestSplit2 + p.bestSplit3 + p.bestLastSplit) - baseTime));
                        wreadLine = wreadLine.Replace("{DifferenceToBestLap}", raceStats.LfstimeToString((p.bestLap - p.bestSplit1 - p.bestSplit2 - p.bestSplit3 - p.bestLastSplit)));
                        wi = wr.getWR(currInfoRace.currentTrackName, p.CName);
                        if (wi == null)
                            wreadLine = wreadLine.Replace("{DifferenceToWR}", "");
                        else           
                            wreadLine = wreadLine.Replace("{DifferenceToWR}", raceStats.LfstimeToString((p.bestSplit1 + p.bestSplit2 + p.bestSplit3 + p.bestLastSplit) - wi.WRTime));
                        sw.WriteLine(wreadLine);
                    }
                    continue;
                }
                #endregion

                #region BlueFlagCausers
                if (readLine.IndexOf("[BlueFlagCausers") == 0)
                {
                    raceStats.modeSort = (int)sortRaceStats.SORT_BLUEFLAG;
                    sorted.Sort();
                    string wreadLine;
                    curPos = 0;
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        raceStats p = (raceStats)sorted[i];

                        wreadLine = readLine;
                        if (p.blueFlags == 0)
                            continue;

//                        string lnickname = lfsStripColor(p.nickName);
                        string lnickname = lfsStripColor(getAllNickName(p));
                        string allUserName = getAllUserName(p);

                        wreadLine = wreadLine.Replace("[BlueFlagCausers ", "");
                        wreadLine = wreadLine.Replace("]", "");
                        curPos++;
                        wreadLine = wreadLine.Replace("{Position}", curPos.ToString());

//                        wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
//                        wreadLine = wreadLine.Replace("{UserName}", p.userName);
                        wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
                        wreadLine = wreadLine.Replace("{UserName}", allUserName);

                        wreadLine = wreadLine.Replace("{BlueFlagsCount}", p.blueFlags.ToString());
                        sw.WriteLine(wreadLine);
                    }
                    continue;
                }
                #endregion

                #region YellowFlagCausers
                if (readLine.IndexOf("[YellowFlagCausers") == 0)
                {
                    raceStats.modeSort = (int)sortRaceStats.SORT_YELLOWFLAG;
                    sorted.Sort();
                    string wreadLine;
                    curPos = 0;
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        raceStats p = (raceStats)sorted[i];

                        wreadLine = readLine;
                        if (p.yellowFlags == 0)
                            continue;

//                        string lnickname = lfsStripColor(p.nickName);
                        string lnickname = lfsStripColor(getAllNickName(p));
                        string allUserName = getAllUserName(p);

                        wreadLine = wreadLine.Replace("[YellowFlagCausers ", "");
                        wreadLine = wreadLine.Replace("]", "");
                        curPos++;
                        wreadLine = wreadLine.Replace("{Position}", curPos.ToString());

//                        wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
//                        wreadLine = wreadLine.Replace("{UserName}", p.userName);
                        wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
                        wreadLine = wreadLine.Replace("{UserName}", allUserName);

                        wreadLine = wreadLine.Replace("{YellowFlagsCount}", p.yellowFlags.ToString());
                        sw.WriteLine(wreadLine);
                    }
                    continue;
                }
                #endregion

                #region PitStops
                if (readLine.IndexOf("[PitStops") == 0)
                {
                    raceStats.modeSort = (int)sortRaceStats.SORT_PIT;
                    sorted.Sort();
                    string wreadLine;
                    string pitinfo = "";
                    curPos = 0;
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        raceStats p = (raceStats)sorted[i];

                        wreadLine = readLine;

//                        string lnickname = lfsStripColor(p.nickName);
                        string lnickname = lfsStripColor(getAllNickName(p));
                        string allUserName = getAllUserName(p);

                        wreadLine = wreadLine.Replace("[PitStops ", "");
                        wreadLine = wreadLine.Replace("]", "");
// If no Pit not vue pit stop
                        if (p.pit.Count == 0)
                            continue;
                        curPos++;
                        wreadLine = wreadLine.Replace("{Position}", curPos.ToString());

//                        wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
//                        wreadLine = wreadLine.Replace("{UserName}", p.userName);
                        wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
                        wreadLine = wreadLine.Replace("{UserName}", allUserName);

                        pitinfo = "";
                        string br = "";
                        string virg = "";
                        string SWork="";
                        for( int j = 0; j < p.pit.Count;j++ ){
                            SWork = "{lg_lap} " + ((p.pit[j] as Pit).LapsDone + 1) + ":";
                            long Work = (p.pit[j] as Pit).Work;

                            if ((Work & (long)InSim.PIT_work.PSE_STOP) != 0)
                            {
                                SWork = SWork + virg + " {lg_stop}";
                                virg = ", ";
                            }
                            if ((Work & (long)InSim.PIT_work.PSE_SETUP) != 0)
                            {
                                SWork = SWork + virg + "{lg_dam_set}";
                                virg = ", ";
                            }
                            if (
                                (Work & (long)InSim.PIT_work.PSE_FR_DAM) != 0
                                || (Work & (long)InSim.PIT_work.PSE_RE_DAM) != 0
                                || (Work & (long)InSim.PIT_work.PSE_LE_FR_DAM) != 0
                                || (Work & (long)InSim.PIT_work.PSE_RI_FR_DAM) != 0
                                || (Work & (long)InSim.PIT_work.PSE_LE_RE_DAM) != 0
                                || (Work & (long)InSim.PIT_work.PSE_RI_RE_DAM) != 0
                                )
                            {
                                SWork = SWork + virg + "{lg_dam_mec}";
                                virg = ", ";
                            }
                            if (
                                (Work & (long)InSim.PIT_work.PSE_BODY_MINOR) != 0
                                || (Work & (long)InSim.PIT_work.PSE_BODY_MAJOR) != 0
                                )
                            {
                                SWork = SWork + virg + "{lg_dam_body}";
                                virg = ", ";
                            }
                            if (
                                (Work & (long)InSim.PIT_work.PSE_FR_WHL) != 0
                                || (Work & (long)InSim.PIT_work.PSE_LE_FR_WHL) != 0
                                || (Work & (long)InSim.PIT_work.PSE_RI_FR_WHL) != 0
                                || (Work & (long)InSim.PIT_work.PSE_RE_WHL) != 0
                                || (Work & (long)InSim.PIT_work.PSE_LE_RE_WHL) != 0
                                || (Work & (long)InSim.PIT_work.PSE_RI_RE_WHL) != 0
                                )
                            {
                                SWork = SWork + virg + "{lg_dam_whe}";
                                virg = ", ";
                            }
                            if ((Work & (long)InSim.PIT_work.PSE_REFUEL) != 0)
                            {
                                SWork = SWork + virg + " {lg_refuel}";
                                virg = ", ";
                            }
                            if( (p.pit[j] as Pit).STime != 0 )
                                SWork = SWork + virg + "{lg_startpit}";
                            SWork = updateGlob(SWork, datFile, currInfoRace, "race");
                            pitinfo = pitinfo + br + SWork + " (" + raceStats.LfstimeToString((p.pit[j] as Pit).STime) + ")";
                            br = "<BR>";
                        }
                        wreadLine = wreadLine.Replace("{PitInfo}", pitinfo);
                        wreadLine = wreadLine.Replace("{PitTime}", raceStats.LfstimeToString(p.cumuledStime));
                        sw.WriteLine(wreadLine);
                    }
                    continue;
                }
                #endregion

                #region Penalties
                if (readLine.IndexOf("[Penalties") == 0)
                {
                    raceStats.modeSort = (int)sortRaceStats.SORT_PEN;
                    sorted.Sort();
                    string wreadLine;
                    curPos = 0;
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        raceStats p = (raceStats)sorted[i];

                        wreadLine = readLine;
                        if (p.numPen == 0)
                            continue;
                        string penaltyInfo = "";
                        for (int j = 0; j < p.pen.Count; j++)
                        {
                            string strPen = "";
                            if ((p.pen[j] as Penalty).NewPen != (int)InSim.pen.PENALTY_NONE)
                                strPen = Enum.GetName(typeof(InSim.pen), (p.pen[j] as Penalty).NewPen);
                            else
                                strPen = Enum.GetName(typeof(InSim.pen), (p.pen[j] as Penalty).OldPen);
                            strPen = "{lg_" + strPen.Remove(0, 8) + "}";
                            penaltyInfo += "{lg_lap} " + (p.pen[j] as Penalty).Lap.ToString() + ": " + strPen.ToLower();
                            penaltyInfo += "<BR>";
                                
                        }

//                        string lnickname = lfsStripColor(p.nickName);
                        string lnickname = lfsStripColor(getAllNickName(p));
                        string allUserName = getAllUserName(p);

                        wreadLine = wreadLine.Replace("[Penalties ", "");
                        wreadLine = wreadLine.Replace("]", "");
                        curPos++;
                        wreadLine = wreadLine.Replace("{Position}", curPos.ToString());

//                        wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
//                        wreadLine = wreadLine.Replace("{UserName}", p.userName);
                        wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
                        wreadLine = wreadLine.Replace("{UserName}", allUserName);

                        wreadLine = wreadLine.Replace("{PenaltyInfo}", penaltyInfo);
                        wreadLine = wreadLine.Replace("{PenaltyCount}", p.numPen.ToString());
                        wreadLine = updateGlob(wreadLine, datFile, currInfoRace, "race");
                        sw.WriteLine(wreadLine);
                    }
                    continue;
                }
                #endregion

                #region TopSpeed
                if (readLine.IndexOf("[TopSpeed") == 0)
                {
                    raceStats.modeSort = (int)sortRaceStats.SORT_BESTSPEED;
                    sorted.Sort();
                    string wreadLine;
                    curPos = 0;
                    int baseSpeed = 0;
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        raceStats p = (raceStats)sorted[i];

                        wreadLine = readLine;
                        if (p.bestSpeed == 0)
                            continue;

//                        string lnickname = lfsStripColor(p.nickName);
                        string lnickname = lfsStripColor(getAllNickName(p));
                        string allUserName = getAllUserName(p);


                        wreadLine = wreadLine.Replace("[TopSpeed ", "");
                        wreadLine = wreadLine.Replace("]", "");
                        curPos++;
                        if (curPos == 1)
                            baseSpeed = p.bestSpeed;
                        wreadLine = wreadLine.Replace("{Position}", curPos.ToString());

//                        wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
//                        wreadLine = wreadLine.Replace("{UserName}", p.userName);
                        wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
                        wreadLine = wreadLine.Replace("{UserName}", allUserName);

                        wreadLine = wreadLine.Replace("{TopSpeed}", raceStats.LfsSpeedToString(p.bestSpeed));
                        wreadLine = wreadLine.Replace("{Difference}", raceStats.LfsSpeedToString(baseSpeed - p.bestSpeed));
                        wreadLine = wreadLine.Replace("{TopSpeedLap}", p.lapBestSpeed.ToString());
                        sw.WriteLine(wreadLine);
                    }
                    continue;
                }
                #endregion

				readLine = updateGlob(readLine, datFile, currInfoRace, "race");
                sw.WriteLine(readLine);
            }
            sw.Close();
            sr.Close();
        }
        public static void lblResult(System.Collections.Hashtable raceStat, string datFile, string raceDir, infoRace currInfoRace)
        {
            int maxLap=0;
            System.Collections.ArrayList sorted = new System.Collections.ArrayList();
            System.Collections.IDictionaryEnumerator tmpRaceStat = raceStat.GetEnumerator();
            double[] percent = new double[] { 100, 100.5, 101.75, 103, 105.25, 107 };
            string[] colPercent = new string[] { "#7070FF", "#20F0C0", "#A0F00F", "#FFFF70", "#FFA070", "#FF5090" };


            while (tmpRaceStat.MoveNext())	//for each player
            {
                raceStats p = (raceStats)tmpRaceStat.Value;
                sorted.Add(p);
            }
            raceStats.modeSort = (int)sortRaceStats.SORT_RESULT;
            sorted.Sort();
            long bestLap = 0;
            string racerBestLap = "";
            for (int i = 0; i < sorted.Count; i++)
            {
                raceStats p = (raceStats)sorted[i];
                if (p.bestLap == 0)
                    continue;
                if (bestLap == 0 || (p.bestLap < bestLap))
                {
                    bestLap = p.bestLap;
                    racerBestLap = p.nickName;
                }
                if( p.lap.Count > maxLap)
                    maxLap = p.lap.Count;
            }

            System.IO.StreamWriter sw = new System.IO.StreamWriter(raceDir + "/" + datFile + "_lbl_race.html");
            System.IO.StreamReader sr = new System.IO.StreamReader("./templates/html_lbl_race.tpl");
            string readLine;
            string wreadLine;

            while (true)
            {
                readLine = sr.ReadLine();
                if (readLine == null)
                    break;
                if (readLine.IndexOf("[Percent") == 0)
                {
                    for (int i = 0; i <= percent.GetUpperBound(0); i++)
                    {
                        wreadLine = readLine;
                        wreadLine = wreadLine.Replace("[Percent ", "");
                        wreadLine = wreadLine.Replace("]", "");
                        wreadLine = wreadLine.Replace("{Percent}", percent[i].ToString() );
                        wreadLine = wreadLine.Replace("{percbackcolor}", colPercent[i]);
                        sw.WriteLine(wreadLine);
                    }
                    continue;
                }
                if (readLine.IndexOf("[Headpos") == 0)
                {
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        wreadLine = readLine;
                        wreadLine = wreadLine.Replace("[Headpos ", "");
                        wreadLine = wreadLine.Replace("]", "");
                        wreadLine = wreadLine.Replace("{Pos}", (i + 1).ToString());
                        sw.WriteLine(wreadLine);
                    }
                    continue;
                }
                if (readLine.IndexOf("[Headracer") == 0)
                {
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        wreadLine = readLine;
                        wreadLine = wreadLine.Replace("[Headracer ", "");
                        wreadLine = wreadLine.Replace("]", "");
                        wreadLine = wreadLine.Replace("{Racer}", lfsStripColor((sorted[i] as raceStats).nickName) );
                        sw.WriteLine(wreadLine);
                    }
                    continue;
                }

                if (readLine.IndexOf("[[Resultracer") == 0)
                {
                    System.Collections.ArrayList blockSplit = new System.Collections.ArrayList();
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
                    for (int j = 0; j < maxLap; j++)
                    {
                        for (int k = 0; k < blockSplit.Count; k++)
                        {
                            readLine = (string)blockSplit[k];
                            if (readLine.IndexOf("[Resultline") == 0)
                            {
                                for (int i = 0; i < sorted.Count; i++)
                                {
                                    raceStats p = (raceStats)sorted[i];
                                    wreadLine = readLine;
                                    wreadLine = wreadLine.Replace("[Resultline ", "");
                                    wreadLine = wreadLine.Replace("]", "");
                                    if (p.lap.Count > j)
                                    {
                                        int bgcolor = -1;
                                        for (int z = 0; z <= percent.GetUpperBound(0); z++)
                                        {
                                            double comp = (double)bestLap * percent[z] / (double)100;
                                            if ((p.lap[j] as Lap).lapTime <= (int)comp)
                                            {
                                                bgcolor = z;
                                                goto exitfor;
                                            }
                                        }
                                    exitfor:
                                        if( bgcolor == -1 )
                                            wreadLine = wreadLine.Replace("{Bgcolor}", "white");
                                        else      
                                            wreadLine = wreadLine.Replace("{Bgcolor}", colPercent[bgcolor]);
                                        wreadLine = wreadLine.Replace("{Ltime}", raceStats.LfstimeToString((p.lap[j] as Lap).lapTime));
                                    }
                                    else
                                    {
                                        wreadLine = wreadLine.Replace("{Bgcolor}", "white");
                                        wreadLine = wreadLine.Replace("{Ltime}", "");
                                    }
                                    sw.WriteLine(wreadLine);
                                }
                                continue;
                            }
                            wreadLine = readLine;
							wreadLine = updateGlob(wreadLine, datFile, currInfoRace, "race");
                            wreadLine = wreadLine.Replace("[[Resultracer ", "");
                            wreadLine = wreadLine.Replace("]]", "");
                            wreadLine = wreadLine.Replace("{lap}", (j + 1).ToString());
                            sw.WriteLine(wreadLine);
                        }
                    }
                    continue;
                }
                if (readLine.IndexOf("[Total") == 0)
                {
                    long firstTotalTime=0;
                    int firstMaxLap=0;
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        wreadLine = readLine;
                        raceStats p = (raceStats)sorted[i];
                        if (i == 0)
                        {
                            firstTotalTime = p.totalTime;
                            firstMaxLap = p.lap.Count;
                        }
                        wreadLine = wreadLine.Replace("[Total ", "");
                        wreadLine = wreadLine.Replace("]", "");
                        // if Racer do not finish
                        if (p.resultNum == 999)
                            wreadLine = wreadLine.Replace("{Gap}", "DNF");
                        else
                        {
                            if (firstMaxLap == p.lap.Count)
                            {
                                if (i == 0)
                                    wreadLine = wreadLine.Replace("{Gap}", raceStats.LfstimeToString(p.totalTime));
                                else
                                {
                                    long tres;
                                    tres = p.totalTime - firstTotalTime;
                                    wreadLine = wreadLine.Replace("{Gap}", "+" + raceStats.LfstimeToString(tres));
                                }
                            }
                            else
                                wreadLine = wreadLine.Replace("{Gap}", "+" + ((int)(firstMaxLap - p.lap.Count)).ToString() + " laps");
                        }
                        wreadLine = wreadLine.Replace("{Ttime}", raceStats.LfstimeToString(p.totalTime));
                        wreadLine = wreadLine.Replace("{Penalty}", p.penalty);
                        sw.WriteLine(wreadLine);
                    }
                    continue;
                }

				readLine = updateGlob(readLine, datFile, currInfoRace, "race");
                readLine = readLine.Replace("{Bestlap}", raceStats.LfstimeToString(bestLap) );
                readLine = readLine.Replace("{Racerbestlap}", lfsStripColor( racerBestLap));

                sw.WriteLine(readLine);
            }


            sw.Close();
            sr.Close();
        }

        public static void qualhtmlResult(System.Collections.Hashtable raceStat, string datFile, string qualDir, infoRace currInfoRace, int maxTimeQualIgnore )
        {
            long firstTotalTime = 0;
            int curPos;
            wr.wrInfo wi;



            System.Collections.ArrayList sorted = new System.Collections.ArrayList();
            System.Collections.IDictionaryEnumerator tmpRaceStat = raceStat.GetEnumerator();
            while (tmpRaceStat.MoveNext())	//for each player
            {
                raceStats p = (raceStats)tmpRaceStat.Value;
                sorted.Add(p);
            }
            for (int i = 0; i < sorted.Count; i++)
            {
                raceStats p = (raceStats)sorted[i];
                p.tmpTime = 0;
            }
// Remove Lap over then x percent of bestlaptime
            for (int i = 0; i < sorted.Count; i++)
            {

                raceStats p = (raceStats)sorted[i];
                int idx;
                long maxTime = (int)((double)p.bestLap * ((double)1 + ((double)maxTimeQualIgnore / (double)100)));
// Remove Lap over then x percent of bestlaptime
                for (idx = p.lap.Count - 1; idx >= 0; idx--)
                {
                    if ((p.lap[idx] as Lap).lapTime > maxTime )
                    {

                        System.Console.WriteLine("Remove " + raceStats.LfstimeToString((p.lap[idx] as Lap).lapTime)
                                    + " Best : " + raceStats.LfstimeToString(p.bestLap)
                                    + " Reference : " + raceStats.LfstimeToString(maxTime)
                        );
                        p.lap.RemoveAt(idx);
                    }
                }
// Recalc Split,cumuled Time and Lap of bestlap due to remove time under bestlap + X percent of bestLap
                p.cumuledTime = 0;
                if (p.lap.Count != 0)
                {
                    for (idx = 0; idx < p.lap.Count; idx++)
                    {
                        p.cumuledTime += (p.lap[idx] as Lap).lapTime;
                        if (p.bestLap == (p.lap[idx] as Lap).lapTime)
                            p.lapBestLap = idx + 1;
                    }
                }
            }
            // CALC avg
            for (int i = 0; i < sorted.Count; i++)
            {
                raceStats p = (raceStats)sorted[i];
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
            for (int i = 0; i < sorted.Count; i++)
            {
                raceStats p = (raceStats)sorted[i];

                if (p.avgTime == 0)
                {
                    p.lapStability = -1;
                    continue;
                }
                for (int j = 0; j < p.lap.Count; j++)
                {
                    p.lapStability += System.Math.Pow((double)(p.avgTime - (p.lap[j] as Lap).lapTime), 2);
                }
                if (p.lap.Count > 1)
                    p.lapStability = System.Math.Sqrt(p.lapStability / ((p.lap.Count) - 1));
                else
                    p.lapStability = -1;
            }
            System.IO.StreamWriter sw = new System.IO.StreamWriter(qualDir + "/" + datFile + "_results_qual.html");
            System.IO.StreamReader sr = new System.IO.StreamReader("./templates/html_qual.tpl");
            string readLine;
            combinedSplit = 0;

            while (true)
            {
                readLine = sr.ReadLine();
                if (readLine == null)
                    break;
                if (readLine.IndexOf("[QualResults") == 0)
                {
                    raceStats.modeSort = (int)sortRaceStats.SORT_BESTLAP;
                    sorted.Sort();
                    string wreadLine;
                    curPos = 0;
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        wreadLine = readLine;
                        raceStats p = (raceStats)sorted[i];
                        if (p.bestLap == 0)
                            continue;
                        curPos++;
                        string lnickname = lfsColorToHtml(p.nickName);
                        wreadLine = wreadLine.Replace("[QualResults ", "");
                        wreadLine = wreadLine.Replace("]", "");
                        p.finalPos = curPos;
                        wreadLine = wreadLine.Replace("{Position}", p.finalPos.ToString());
                        wreadLine = wreadLine.Replace("{PlayerNameColoured}", lnickname);
                        wreadLine = wreadLine.Replace("{UserNameLink}", p.userName);
                        wreadLine = wreadLine.Replace("{UserName}", p.userName);
                        wreadLine = wreadLine.Replace("{Car}", p.CName);
                        wreadLine = wreadLine.Replace("{NumberPlate}", lfsStripColor(p.Plate));
                            
                        wreadLine = wreadLine.Replace("{BestLap}", raceStats.LfstimeToString(p.bestLap));
                        if (curPos == 1)
                        {
                            firstTotalTime = p.bestLap;
                        }
                        long tres;
                        tres = p.bestLap - firstTotalTime;
                        wreadLine = wreadLine.Replace("{Difference}", "+" + raceStats.LfstimeToString(tres));
                        wi = wr.getWR(currInfoRace.currentTrackName, p.CName);
                        if (wi == null)
                            wreadLine = wreadLine.Replace("{DifferenceToWR}", "");
                        else           
                            wreadLine = wreadLine.Replace("{DifferenceToWR}", raceStats.LfstimeToString(p.bestLap - wi.WRTime));
                        wreadLine = wreadLine.Replace("{BestLapLap}", p.lapBestLap.ToString() );
                        wreadLine = wreadLine.Replace("{LapsDone}", p.lap.Count.ToString());

                        wreadLine = wreadLine.Replace("{Flags}", p.sFlags);
                        sw.WriteLine(wreadLine);
                    }
                    continue;
                }
                if (readLine.IndexOf("[LapTimesStability") == 0)
                {
                    raceStats.modeSort = (int)sortRaceStats.SORT_STABILITY;
                    sorted.Sort();
                    string wreadLine;
                    curPos = 0;
                    double baseTime = 0;
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        raceStats p = (raceStats)sorted[i];

                        wreadLine = readLine;
                        if (p.lapStability <= 0)
                            continue;
                        string lnickname = lfsStripColor(p.nickName);
                        wreadLine = wreadLine.Replace("[LapTimesStability ", "");
                        wreadLine = wreadLine.Replace("]", "");
                        curPos++;
                        if (curPos == 1)
                            baseTime = p.lapStability;
                        wreadLine = wreadLine.Replace("{Position}", curPos.ToString());
                        wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
                        wreadLine = wreadLine.Replace("{UserName}", p.userName);
                        wreadLine = wreadLine.Replace("{Deviation}", raceStats.LfstimeToString((long)p.lapStability));
                        wreadLine = wreadLine.Replace("{Difference}", raceStats.LfstimeToString((long)(p.lapStability - baseTime)));
                        wreadLine = wreadLine.Replace("{LapsDone}", p.lap.Count.ToString());

                        sw.WriteLine(wreadLine);
                    }
                    continue;

                }


                if (readLine.IndexOf("[AverageLap") == 0)
                {
                    raceStats.modeSort = (int)sortRaceStats.SORT_AVGTIME;
                    sorted.Sort();
                    string wreadLine;
                    curPos = 0;
                    long baseTime = 0;
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        raceStats p = (raceStats)sorted[i];

                        wreadLine = readLine;
                        if (p.avgTime == 0)
                            continue;
                        string lnickname = lfsStripColor(p.nickName);
                        wreadLine = wreadLine.Replace("[AverageLap ", "");
                        wreadLine = wreadLine.Replace("]", "");
                        curPos++;
                        if (curPos == 1)
                            baseTime = p.avgTime;
                        wreadLine = wreadLine.Replace("{Position}", curPos.ToString());
                        wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
                        wreadLine = wreadLine.Replace("{UserName}", p.userName);
                        wreadLine = wreadLine.Replace("{LapTime}", raceStats.LfstimeToString(p.avgTime));
                        wreadLine = wreadLine.Replace("{Difference}", raceStats.LfstimeToString(p.avgTime - baseTime));
                        wreadLine = wreadLine.Replace("{Laps}", p.lap.Count.ToString());
                        sw.WriteLine(wreadLine);
                    }
                    continue;
                }

                if (readLine.IndexOf("[BestLap") == 0)
                {
                    raceStats.modeSort = (int)sortRaceStats.SORT_BESTLAP;
                    sorted.Sort();
                    string wreadLine;
                    curPos = 0;
                    long baseTime = 0;
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        raceStats p = (raceStats)sorted[i];

                        wreadLine = readLine;
                        if (p.bestLap == 0)
                            continue;
                        string lnickname = lfsStripColor(p.nickName);
                        wreadLine = wreadLine.Replace("[BestLap ", "");
                        wreadLine = wreadLine.Replace("]", "");
                        curPos++;
                        if (curPos == 1)
                            baseTime = p.bestLap;
                        wreadLine = wreadLine.Replace("{Position}", curPos.ToString());
                        wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
                        wreadLine = wreadLine.Replace("{UserName}", p.userName);
                        wreadLine = wreadLine.Replace("{LapTime}", raceStats.LfstimeToString(p.bestLap));
                        wreadLine = wreadLine.Replace("{Difference}", raceStats.LfstimeToString(p.bestLap - baseTime));
                        wi = wr.getWR(currInfoRace.currentTrackName, p.CName);
                        if (wi == null)
                            wreadLine = wreadLine.Replace("{DifferenceToWR}", "");
                        else           
                            wreadLine = wreadLine.Replace("{DifferenceToWR}", raceStats.LfstimeToString(p.bestLap - wi.WRTime));
                        wreadLine = wreadLine.Replace("{Lap}", p.lapBestLap.ToString());
                        sw.WriteLine(wreadLine);
                    }
                    continue;
                }
                if (readLine.IndexOf("[[BestSplitTable") == 0)
                {
                    System.Collections.ArrayList blockSplit = new System.Collections.ArrayList();
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
                        for (int i = 0; i < sorted.Count; i++)
                        {
                            raceStats p = (raceStats)sorted[i];
                            wi = wr.getWR(currInfoRace.currentTrackName, p.CName);
                            if (j == currInfoRace.maxSplit)
                            {
                                p.curBestSplit = p.bestLastSplit;
                                p.curLapBestSplit = p.lapBestLastSplit;
                                if (wi == null)
                                    p.curWrSplit = 0;
                                else
                                    p.curWrSplit = wi.sectorSplitLast;
                                continue;
                            }
                            if (j == 0)
                            {
                                p.curBestSplit = p.bestSplit1;
                                p.curLapBestSplit = p.lapBestSplit1;
                                if (wi == null)
                                    p.curWrSplit = 0;
                                else
                                    p.curWrSplit = wi.sectorSplit[j];
                                continue;
                            }
                            if (j == 1)
                            {
                                p.curBestSplit = p.bestSplit2;
                                p.curLapBestSplit = p.lapBestSplit2;
                                if (wi == null)
                                    p.curWrSplit = 0;
                                else
                                    p.curWrSplit = wi.sectorSplit[j];
                                continue;
                            }
                            if (j == 2)
                            {
                                p.curBestSplit = p.bestSplit3;
                                p.curLapBestSplit = p.lapBestSplit3;
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
                                raceStats.modeSort = (int)sortRaceStats.SORT_BESTSPLIT;
                                sorted.Sort();
                                string wreadLine;
                                curPos = 0;
                                long baseTime = 0;
                                for (int i = 0; i < sorted.Count; i++)
                                {
                                    raceStats p = (raceStats)sorted[i];

                                    wreadLine = readLine;
                                    if (p.curBestSplit == 0)
                                        continue;
                                    string lnickname = lfsStripColor(p.nickName);
                                    wreadLine = wreadLine.Replace("[BestSplit ", "");
                                    wreadLine = wreadLine.Replace("]", "");
                                    curPos++;
                                    if (curPos == 1)
                                        baseTime = p.curBestSplit;
                                    wreadLine = wreadLine.Replace("{Position}", curPos.ToString());
                                    wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
                                    wreadLine = wreadLine.Replace("{UserName}", p.userName);
                                    wreadLine = wreadLine.Replace("{SplitTime}", raceStats.LfstimeToString(p.curBestSplit));
                                    wreadLine = wreadLine.Replace("{Difference}", raceStats.LfstimeToString(p.curBestSplit - baseTime));
                                    if (p.curWrSplit == 0)
                                        wreadLine = wreadLine.Replace("{DifferenceToWR}", "");
                                    else
                                        wreadLine = wreadLine.Replace("{DifferenceToWR}", raceStats.LfstimeToString(p.curBestSplit - p.curWrSplit));
                                    wreadLine = wreadLine.Replace("{Lap}", p.curLapBestSplit.ToString());
                                    sw.WriteLine(wreadLine);
                                }
                                combinedSplit = combinedSplit + baseTime;
                                continue;
                            }
							readLine = updateGlob(readLine, datFile, currInfoRace, "qual");
                            readLine = readLine.Replace("]]", "");
                            readLine = readLine.Replace("[[BestSplitTable ", "");
                            readLine = readLine.Replace("{SplitNumber}", (j + 1).ToString());
                            sw.WriteLine(readLine);
                        }
                    }
                }
                if (readLine.IndexOf("[BestPossibleLap") == 0)
                {
                    raceStats.modeSort = (int)sortRaceStats.SORT_TPB;
                    sorted.Sort();
                    string wreadLine;
                    curPos = 0;
                    long baseTime = 0;
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        raceStats p = (raceStats)sorted[i];

                        wreadLine = readLine;
                        if ((p.bestSplit1 + p.bestSplit2 + p.bestSplit3 + p.bestLastSplit) == 0)
                            continue;
                        string lnickname = lfsStripColor(p.nickName);
                        wreadLine = wreadLine.Replace("[BestPossibleLap ", "");
                        wreadLine = wreadLine.Replace("]", "");
                        curPos++;
                        if (curPos == 1)
                            baseTime = p.bestSplit1 + p.bestSplit2 + p.bestSplit3 + p.bestLastSplit;
                        wreadLine = wreadLine.Replace("{Position}", curPos.ToString());
                        wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
                        wreadLine = wreadLine.Replace("{UserName}", p.userName);
                        wreadLine = wreadLine.Replace("{LapTime}", raceStats.LfstimeToString(p.bestSplit1 + p.bestSplit2 + p.bestSplit3 + p.bestLastSplit));
                        wreadLine = wreadLine.Replace("{Difference}", raceStats.LfstimeToString((p.bestSplit1 + p.bestSplit2 + p.bestSplit3 + p.bestLastSplit) - baseTime));
                        wreadLine = wreadLine.Replace("{DifferenceToBestLap}", raceStats.LfstimeToString((p.bestLap - p.bestSplit1 - p.bestSplit2 - p.bestSplit3 - p.bestLastSplit)));
                        wi = wr.getWR(currInfoRace.currentTrackName, p.CName);
                        if (wi == null)
                            wreadLine = wreadLine.Replace("{DifferenceToWR}", "");
                        else           
                            wreadLine = wreadLine.Replace("{DifferenceToWR}", raceStats.LfstimeToString((p.bestSplit1 + p.bestSplit2 + p.bestSplit3 + p.bestLastSplit) - wi.WRTime));
                        sw.WriteLine(wreadLine);
                    }
                    continue;
                }
                if (readLine.IndexOf("[BlueFlagCausers") == 0)
                {
                    raceStats.modeSort = (int)sortRaceStats.SORT_BLUEFLAG;
                    sorted.Sort();
                    string wreadLine;
                    curPos = 0;
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        raceStats p = (raceStats)sorted[i];

                        wreadLine = readLine;
                        if (p.blueFlags == 0)
                            continue;
                        string lnickname = lfsStripColor(p.nickName);
                        wreadLine = wreadLine.Replace("[BlueFlagCausers ", "");
                        wreadLine = wreadLine.Replace("]", "");
                        curPos++;
                        wreadLine = wreadLine.Replace("{Position}", curPos.ToString());
                        wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
                        wreadLine = wreadLine.Replace("{UserName}", p.userName);
                        wreadLine = wreadLine.Replace("{BlueFlagsCount}", p.blueFlags.ToString());
                        sw.WriteLine(wreadLine);
                    }
                    continue;
                }
                if (readLine.IndexOf("[YellowFlagCausers") == 0)
                {
                    raceStats.modeSort = (int)sortRaceStats.SORT_YELLOWFLAG;
                    sorted.Sort();
                    string wreadLine;
                    curPos = 0;
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        raceStats p = (raceStats)sorted[i];

                        wreadLine = readLine;
                        if (p.yellowFlags == 0)
                            continue;
                        string lnickname = lfsStripColor(p.nickName);
                        wreadLine = wreadLine.Replace("[YellowFlagCausers ", "");
                        wreadLine = wreadLine.Replace("]", "");
                        curPos++;
                        wreadLine = wreadLine.Replace("{Position}", curPos.ToString());
                        wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
                        wreadLine = wreadLine.Replace("{UserName}", p.userName);
                        wreadLine = wreadLine.Replace("{YellowFlagsCount}", p.yellowFlags.ToString());
                        sw.WriteLine(wreadLine);
                    }
                    continue;
                }
                if (readLine.IndexOf("[PitStops") == 0)
                {
                    raceStats.modeSort = (int)sortRaceStats.SORT_PIT;
                    sorted.Sort();
                    string wreadLine;
                    string pitinfo = "";
                    curPos = 0;
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        raceStats p = (raceStats)sorted[i];

                        wreadLine = readLine;
                        string lnickname = lfsStripColor(p.nickName);
                        wreadLine = wreadLine.Replace("[PitStops ", "");
                        wreadLine = wreadLine.Replace("]", "");
                        curPos++;
                        wreadLine = wreadLine.Replace("{Position}", curPos.ToString());
                        wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
                        wreadLine = wreadLine.Replace("{UserName}", p.userName);
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
							SWork = updateGlob(SWork, datFile, currInfoRace, "qual");
                            pitinfo = pitinfo + br + SWork + " (" + raceStats.LfstimeToString((p.pit[j] as Pit).STime) + ")";
                            br = "<BR>";
                        }
                        wreadLine = wreadLine.Replace("{PitInfo}", pitinfo);
                        wreadLine = wreadLine.Replace("{PitTime}", raceStats.LfstimeToString(p.cumuledStime));
                        sw.WriteLine(wreadLine);
                    }
                    continue;
                }
                if (readLine.IndexOf("[TopSpeed") == 0)
                {
                    raceStats.modeSort = (int)sortRaceStats.SORT_BESTSPEED;
                    sorted.Sort();
                    string wreadLine;
                    curPos = 0;
                    int baseSpeed = 0;
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        raceStats p = (raceStats)sorted[i];

                        wreadLine = readLine;
                        if (p.bestSpeed == 0)
                            continue;
                        string lnickname = lfsStripColor(p.nickName);
                        wreadLine = wreadLine.Replace("[TopSpeed ", "");
                        wreadLine = wreadLine.Replace("]", "");
                        curPos++;
                        if (curPos == 1)
                            baseSpeed = p.bestSpeed;
                        wreadLine = wreadLine.Replace("{Position}", curPos.ToString());
                        wreadLine = wreadLine.Replace("{PlayerName}", lnickname);
                        wreadLine = wreadLine.Replace("{UserName}", p.userName);
                        wreadLine = wreadLine.Replace("{TopSpeed}", raceStats.LfsSpeedToString(p.bestSpeed));
                        wreadLine = wreadLine.Replace("{Difference}", raceStats.LfsSpeedToString(baseSpeed - p.bestSpeed));
                        wreadLine = wreadLine.Replace("{TopSpeedLap}", p.lapBestSpeed.ToString());
                        sw.WriteLine(wreadLine);
                    }
                    continue;
                }

				readLine = updateGlob(readLine, datFile, currInfoRace, "qual");
                sw.WriteLine(readLine);
            }
            sw.Close();
            sr.Close();
        }
 
        public static string getStringLang( string key  )
        {
            string ret;
            try
            {
                
                ret = (string)lang[key];
//                System.Console.WriteLine(lang.Count + "-" + key + ":" + (string)lang[key]);
            }
            catch
            {
                ret = "Lang key >" + key + "< not found in language file";
            }
            return ret;
        }
		public static string updateGlob(string readLine, string datFile, infoRace currInfoRace, string chatInX)
        {

            if (readLine.IndexOf("[include") == 0)
            {
                string lreadLine = "";
                readLine = readLine.Replace("[include", "");
                readLine = readLine.Replace("]", "");
                readLine = readLine.Trim();
                System.IO.StreamReader sr;
                try
                {
                    sr = new System.IO.StreamReader(readLine);
                }
                catch { 
                    return (""); 
                }
                readLine = "";
                while (true)
                {
                    lreadLine = sr.ReadLine();
                    if (lreadLine == null)
                        break;
                    readLine = readLine + lreadLine;
                }
                sr.Close();
            }
            System.Collections.IDictionaryEnumerator tmplang = lang.GetEnumerator();
            while (tmplang.MoveNext())
            {
                readLine = readLine.Replace("{" + (string)tmplang.Key + "}", getStringLang((string)tmplang.Key));
            }
            readLine = readLine.Replace("{VERSIONSHORT}", System.Reflection.Assembly.GetExecutingAssembly().ToString());
            readLine = readLine.Replace("{ServerName}", lfsColorToHtml( currInfoRace.HName ));
            readLine = readLine.Replace("{TrackNameFull}",InSim.Decoder.getLongTrackName( currInfoRace.currentTrackName ));
            readLine = readLine.Replace("{TrackNameShort}", currInfoRace.currentTrackName);
            readLine = readLine.Replace("{TrackImg}", currInfoRace.currentTrackName.ToLower() + ".gif" );
            if( currInfoRace.raceLaps == 0 )
                readLine = readLine.Replace("{RaceLength}", currInfoRace.sraceLaps);
            else if( currInfoRace.raceLaps == 1 )
                readLine = readLine.Replace("{RaceLength}", currInfoRace.sraceLaps + " " + lang["lg_durationLap"]);
            else if( currInfoRace.raceLaps < 191 )
                readLine = readLine.Replace("{RaceLength}", currInfoRace.sraceLaps + " " + lang["lg_durationLaps"]);
            else if( currInfoRace.raceLaps == 191 )
                readLine = readLine.Replace("{RaceLength}", currInfoRace.sraceLaps + " " + lang["lg_durationHour"]);
            else
                readLine = readLine.Replace("{RaceLength}", currInfoRace.sraceLaps + " " + lang["lg_durationHours"]);


            readLine = readLine.Replace("{QualLength}", currInfoRace.qualMins.ToString() );
            readLine = readLine.Replace("{RaceConditions}", strWeather[currInfoRace.weather] + ", " + strWind[currInfoRace.wind]);
            readLine = readLine.Replace("{QualConditions}", strWeather[currInfoRace.weather] + ", " + strWind[currInfoRace.wind]);
            readLine = readLine.Replace("{LapByLapGraphFileName}", datFile + "_lbl.png");
            readLine = readLine.Replace("{linklbl}", datFile + "_lbl_race.html");
			readLine = readLine.Replace("{linkchat}", datFile + "_chat_" + chatInX + ".html");
            readLine = readLine.Replace("{RaceProgressGraphFileName}", datFile + "_rpr.png");
            readLine = readLine.Replace("{CombinedBestLap}", raceStats.LfstimeToString(combinedSplit));

            return (readLine);
        }
	}
}