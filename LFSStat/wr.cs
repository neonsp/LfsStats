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
using System.Collections;
using System.Text;
using System.Net;
using System.IO;


namespace LFSStat
{
    class wr
    {
        static System.Collections.Hashtable trackTable = new System.Collections.Hashtable();

        public class wrInfo
        {
            public string track;
            public string CName;
            public long WRTime;
            public long[] split = new long[3];
            public long[] sectorSplit = new long[3];
            public long sectorSplitLast;

            public wrInfo(string track, string CName, long WRTime, long split1, long split2, long split3, long sectorSplit1, long sectorSplit2, long sectorSplit3, long sectorSplitLast)
            {
                this.track = track;
                this.CName = CName;
                this.WRTime = WRTime;
                this.split[0] = split1;
                this.split[1] = split2;
                this.split[2] = split3;
                this.sectorSplit[0] = sectorSplit1;
                this.sectorSplit[1] = sectorSplit2;
                this.sectorSplit[2] = sectorSplit3;
                this.sectorSplitLast = sectorSplitLast;
            }
        }
        public class trackInfo : System.IComparable
        {
            public string track;
            public System.Collections.Hashtable carTable = new System.Collections.Hashtable();
            public trackInfo(string track)
            {
                this.track = track;
            }
            public int CompareTo(object x)
            {
                if (string.Compare((x as trackInfo).track, this.track) < 0)
                    return 1;
                else if (string.Compare((x as trackInfo).track, this.track) > 0)
                    return -1;
                else
                    return 0;
            }
        }
        static string convTrack(string track)
        {
            string retValue = "";
            switch (track[0])
            {
                case '0':
                    retValue = "BL";
                    break;
                case '1':
                    retValue = "SO";
                    break;
                case '2':
                    retValue = "FE";
                    break;
                case '3':
                    retValue = "AU";
                    break;
                case '4':
                    retValue = "KY";
                    break;
                case '5':
                    retValue = "WE";
                    break;
                case '6':
                    retValue = "AS";
                    break;
            }
            retValue += (int.Parse(track[1].ToString()) + 1).ToString();
            if (track[2] == '1')
                retValue += "R";
            return retValue;
        }
        public static wrInfo getWR(string track, string CName )
        {
            wrInfo wi;
            try
            {
                trackInfo tt = (trackTable[track] as trackInfo);
                wi = (wrInfo)tt.carTable[CName];
                return wi;
            }
            catch { return null; }

        }
        public static bool load(string user, string pass, string idk)
        {
            string readLine;

            long split1 = 0;
            long split2 = 0;
            long split3 = 0;
            long lastSplit = 0;
            long sectorSplit1 = 0;
            long sectorSplit2 = 0;
            long sectorSplit3 = 0;
            long sectorSplitLast = 0;
            long WRTime = 0;


            //            using (System.IO.StreamReader sr = new System.IO.StreamReader("./lfswr.txt"))
            string url;
            if (idk == "" && user == "" && pass == "")
            {
                return false;
            }
            if (idk != null)
                url = "http://www.lfsworld.net/pubstat/get_stat2.php?version=1.3&idk=" + idk + "&action=wr";
            else
                url = "http://www.lfsworld.net/pubstat/get_stat2.php?version=1.3&user=" + user + "&pass=" + pass + "&action=wr";
            WebRequest req = WebRequest.Create(url);
            WebResponse result = req.GetResponse();
            Stream receiveStream = result.GetResponseStream();
            using (StreamReader sr = new StreamReader(receiveStream))
            {
                while (true)
                {
                    readLine = sr.ReadLine();
                    if (readLine == null)
                        break;
                    if (readLine.IndexOf("Identification") != -1)
                        return false;
                    if (readLine.IndexOf("can't") != -1)
                        return false;
                    string[] mline = readLine.Split(' ');
                    string track = convTrack( mline[1] );
                    string car = mline[2];
                    lastSplit = 0;
                    sectorSplit1 = 0;
                    sectorSplit2 = 0;
                    sectorSplit3 = 0;
                    sectorSplitLast = 0;
                    split1 = long.Parse(mline[3]);
                    sectorSplit1 = split1 - lastSplit;
                    lastSplit = split1;

                    split2 = long.Parse(mline[4]);
                    if( split2 != 0 ){
                        sectorSplit2 = split2 - lastSplit;
                        lastSplit = split2;
                    }

                    split3 = long.Parse(mline[5]);
                    if( split3 != 0 ){
                        sectorSplit3 = split3 - lastSplit;
                        lastSplit = split3;
                    }
                    WRTime = long.Parse(mline[6]);
                    sectorSplitLast = WRTime - lastSplit;
                    if (!trackTable.ContainsKey(track))
                    {
                        trackTable[track] = new trackInfo(track);
                    }
                    trackInfo tt = (trackTable[track] as trackInfo);
                    if (!tt.carTable.ContainsKey(car))
                    {
                        tt.carTable[car] = new wrInfo(track, car, WRTime, split1, split2, split3, sectorSplit1, sectorSplit2, sectorSplit3, sectorSplitLast );
                    }
                }
            }
            return true;
        }
    }
}
