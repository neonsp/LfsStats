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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using DotNetExtensions;


namespace LFSStatistics
{
	/// <summary>
	/// For more info see https://www.lfsforum.net/showthread.php?t=14480
	/// </summary>
	static class LFSWorld
	{
		private const int lfswTxtWRColumnCount = 10;	// 10 columns: <id_wr> <Track> <car> <split1> <split2> <split3> <laptime> <flags_hlaps> <racername> <timestamp>
		private const int lfswRequestPeriod = 5000;		// 5 seconds
		private const int lfswNumberOfWRs = 940;		// number of WRs, used for dictionary initial capacity
		private const string lfswScriptUrl = @"http://www.lfsworld.net/pubstat/get_stat2.php?idk=";	// no version specified, will use latest version by default
		private const string lfswRequestWR = @"&action=wr";	// WR
		private const string lfswScriptFormatXML = @"&s=3";	// XML

		private static Dictionary<string, TrackInfo> TrackTable { get; set; }
		private static Dictionary<string, WR> dictWR;
		private static bool initialized;

		static LFSWorld()
		{
			TrackTable = new Dictionary<string, TrackInfo>();
			dictWR = new Dictionary<string, WR>(lfswNumberOfWRs, StringComparer.OrdinalIgnoreCase);
			initialized = false;
		}

		private class TrackInfo// : System.IComparable
		{
			internal string Track { get; private set; }
			internal Dictionary<string, WR> CarTable { get; private set; }

			internal TrackInfo(string track)
			{
				this.Track = track;
				this.CarTable = new Dictionary<string, WR>();
			}

			//public int CompareTo(object x)
			//{
			//    if (string.Compare((x as TrackInfo).track, this.track) < 0)
			//        return 1;
			//    else if (string.Compare((x as TrackInfo).track, this.track) > 0)
			//        return -1;
			//    else
			//        return 0;
			//}
		}

		internal class WR
		{
			public string Track { get; private set; }
			public string CarName { get; private set; }
			public long WrTime { get; private set; }
			public long[] Split { get; private set; }
			public long[] Sector { get; private set; }
			public string RacerName { get; private set; }
			public PlayerFlags Flags { get; private set; }

			internal WR(string track, string carName, long wrTime, long[] split, long[] sector, string racerName, PlayerFlags flags)
			{
				Track = track;
				CarName = carName;
				WrTime = wrTime;
				Split = split;
				Sector = sector;
				RacerName = racerName;
				Flags = flags;
			}
		}

		internal class WRi
		{
			public int id_wr { get; private set; }
			public int track { get; private set; }
			public string car { get; private set; }
			public long split1 { get; private set; }
			public long split2 { get; private set; }
			public long split3 { get; private set; }
			public long laptime { get; private set; }
			public int flags_hlaps { get; private set; }
			public string racername { get; private set; }
			public long timestamp { get; private set; }
		}

		/// <summary>
		/// Gets the WR for given track and car combination.
		/// </summary>
		/// <param name="track">The track.</param>
		/// <param name="carName">Code of the car.</param>
		/// <returns>LFSWorld should be initialized prior, otherwise it will return null, it will also return null when given track car combination is not found. On success it returns WR.</returns>
		public static WR GetWR(string track, string carName)
		{
			if (!initialized) return null;	//throw new LFSWorldException("LFSWorld not initialized.");
			try { return TrackTable[track].CarTable[carName]; }
			catch (KeyNotFoundException) { return null; }
		}

        //private enum PubStatWR
        //{
        //    id_wr, track, car, split1, split2, split3, laptime, flags_hlaps, racername, timestamp
        //}

        internal static bool Initialize(string pubStatIDkey)
        {
            LFSStats.Write("Loading LFSWorld WRs...");

            if (pubStatIDkey == null)
                throw new ArgumentNullException(nameof(pubStatIDkey));

            if (pubStatIDkey.Length == 0)
            {
                LFSStats.WriteLine(" not loaded (no ID key)");
                return initialized;
            }

            const int maxRequests = 5;
            int requestNumber = 0;

            string url = lfswScriptUrl + pubStatIDkey + lfswRequestWR + lfswScriptFormatXML;

            while (requestNumber < maxRequests)
            {
                try
                {
                    using (WebResponse webResponse = WebRequest.Create(url).GetResponse())
                    using (Stream responseStream = webResponse.GetResponseStream())
                    {
                        if (responseStream == null)
                            throw new InvalidOperationException("Response stream is null.");

                        // Aquí iría el parsing real (actualmente comentado)
                        initialized = true;
                        return initialized;
                    }
                }
                catch (WebException ex)
                {
                    requestNumber++;

                    if (requestNumber >= maxRequests)
                    {
                        LFSStats.ErrorWriteException("LFSWorld request failed.", ex);
                        break;
                    }

                    LFSStats.Write(" waiting 5s");
                    Thread.Sleep(lfswRequestPeriod);
                }
                catch (Exception ex)
                {
                    LFSStats.ErrorWriteException("LFSWorld error.", ex);
                    break;
                }
            }

            return initialized;
        }

        private static string ConvertLFSWTrackCode(string trackCode)
		{
			string retValue = "";
			switch (trackCode[0])
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
			retValue += (int.Parse(trackCode[1].ToString()) + 1).ToString();
			if (trackCode[2] == '1')
				retValue += "R";
			return retValue;
		}
	}
}
