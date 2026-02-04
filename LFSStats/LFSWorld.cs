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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace LFSStatistics
{
	/// <summary>
	/// For more info see https://www.lfsforum.net/showthread.php?t=14480
	/// </summary>
	static class LFSWorld
	{
		private const int lfswRequestPeriod = 5000;		// 5 seconds
		private const int lfswNumberOfWRs = 940;		// number of WRs, used for dictionary initial capacity
		private const string lfswScriptUrl = @"https://www.lfsworld.net/pubstat/get_stat2.php?ps=0&version=1.5&idk=";	// no premium usage, version 1.5 specified
		private const string lfswRequestWR = @"&action=wr";	// WR
		private const string lfswScriptFormatXML = @"&s=1"; // JSON format

        private static Dictionary<string, TrackInfo> TrackTable { get; set; }
		private static Dictionary<string, WR> dictWR;
		private static bool initialized;

		static LFSWorld()
		{
			TrackTable = new Dictionary<string, TrackInfo>();
			dictWR = new Dictionary<string, WR>(lfswNumberOfWRs, StringComparer.OrdinalIgnoreCase);
			initialized = false;
		}

		private class TrackInfo : IComparable
		{
			internal string Track { get; private set; }
			internal Dictionary<string, WR> CarTable { get; private set; }

			internal TrackInfo(string track)
			{
				Track = track;
				CarTable = new Dictionary<string, WR>();
			}

            public int CompareTo(object obj)
            {
                if (obj == null) return 1;
                if (obj is TrackInfo other)
                {
                    return string.Compare(Track, other.Track, StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    throw new ArgumentException("Object is not a TrackInfo");
                }
            }
        }

		internal class WR
		{
			public string Track { get; private set; }
			public string CarName { get; private set; }
			public long WrTime { get; private set; }
			public long[] Split { get; private set; }
			public long[] Sector { get; private set; }
			public string RacerName { get; private set; }
			public InSimDotNet.Packets.PlayerFlags Flags { get; private set; }

			internal WR(string track, string carName, long wrTime, long[] split, long[] sector, string racerName, InSimDotNet.Packets.PlayerFlags flags)
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
            public int id_wr { get; set; }
            public string track { get; set; }
            public string car { get; set; }
            public long split1 { get; set; }
            public long split2 { get; set; }
            public long split3 { get; set; }
            public long laptime { get; set; }
            public int flags_hlaps { get; set; }
            public string racername { get; set; }
            public long timestamp { get; set; }
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

            // ---- CACHE SETUP ----
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string cacheDir = Path.Combine(basePath, "cache");
            string cacheFile = Path.Combine(cacheDir, "lfsworld_wr.json");
            TimeSpan cacheMaxAge = TimeSpan.FromDays(1);

            Directory.CreateDirectory(cacheDir);

            string json = null;

            // ---- TRY CACHE FIRST ----
            if (File.Exists(cacheFile))
            {
                DateTime lastWrite = File.GetLastWriteTimeUtc(cacheFile);
                if (DateTime.UtcNow - lastWrite < cacheMaxAge)
                {
                    try
                    {
                        json = File.ReadAllText(cacheFile);
                        LFSStats.Write(" using cache");
                    }
                    catch (IOException)
                    {
                        json = null;
                    }
                }
            }

            // ---- DOWNLOAD IF NO VALID CACHE ----
            while (json == null && requestNumber < maxRequests)
            {
                try
                {
                    using (WebResponse webResponse = WebRequest.Create(url).GetResponse())
                    using (Stream responseStream = webResponse.GetResponseStream())
                    {
                        if (responseStream == null)
                            throw new InvalidOperationException("Response stream is null.");

                        using (StreamReader reader = new StreamReader(responseStream))
                        {
                            json = reader.ReadToEnd();
                        }

                        // Save to cache (best-effort)
                        try
                        {
                            File.WriteAllText(cacheFile, json);
                        }
                        catch (IOException)
                        {
                            // cache failure is non-fatal
                        }
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

            if (json == null)
                return initialized;

            // ---- EXISTING PARSING LOGIC ----

            List<WRi> wrList = DeserializeWRiList(json);
            TrackTable = new Dictionary<string, TrackInfo>();

            foreach (WRi wri in wrList)
            {
                string trackCode = wri.track;
                string trackName = ConvertLFSWTrackCode(trackCode);

                if (!TrackTable.TryGetValue(trackName, out TrackInfo trackInfo))
                {
                    trackInfo = new TrackInfo(trackName);
                    TrackTable.Add(trackName, trackInfo);
                }

                const int maxSplits = 3;
                var splits = new long[maxSplits] { wri.split1, wri.split2, wri.split3 };
                long[] sectors = new long[maxSplits + 1];

                long previous = 0;
                int lastValidSplitIndex = -1;

                for (int i = 0; i < maxSplits; i++)
                {
                    if (splits[i] > 0)
                    {
                        sectors[i] = splits[i] - previous;
                        previous = splits[i];
                        lastValidSplitIndex = i;
                    }
                    else
                    {
                        sectors[i] = 0;
                    }
                }

                if (lastValidSplitIndex >= 0 && wri.laptime > previous)
                {
                    sectors[lastValidSplitIndex + 1] = wri.laptime - previous;
                }

                var flags = (InSimDotNet.Packets.PlayerFlags)wri.flags_hlaps;

                if (string.IsNullOrWhiteSpace(wri.car))
                {
                    LFSStats.WriteLine($"WR ignored: invalid track (track={wri.track}, id_wr={wri.id_wr})");
                    continue;
                }

                WR wr = new WR(
                    trackName,
                    wri.car,
                    wri.laptime,
                    splits,
                    sectors,
                    wri.racername,
                    flags
                );

                trackInfo.CarTable[wri.car] = wr;
            }

            initialized = true;
            LFSStats.WriteLine(" done.");
            return initialized;
        }

        private static List<WRi> DeserializeWRiList(string json)
        {
            return JsonConvert.DeserializeObject<List<WRi>>(json);
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
				case '7':
					retValue = "RO";
					break;
				case '8':
					retValue = "LA";
					break;
			}
			retValue += (int.Parse(trackCode[1].ToString()) + 1).ToString();
			if (trackCode[2] == '1')
				retValue += "R";
			return retValue;
		}
	}
}
