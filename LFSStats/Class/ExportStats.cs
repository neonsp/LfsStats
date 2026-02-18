using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InSimDotNet.Packets;

namespace LFSStatistics
{

    class ExportStats
    {
        public const string htmlDotExtension = ".html";
        public static string[] strWind = new string[3];

        /// <summary>
        /// Sanitizes a filename by removing invalid characters
        /// </summary>
        private static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return fileName;

            // Get invalid characters for file names
            char[] invalidChars = Path.GetInvalidFileNameChars();

            // Replace invalid characters with underscore
            foreach (char c in invalidChars)
            {
                fileName = fileName.Replace(c, '_');
            }

            return fileName;
        }

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
            if (string.IsNullOrWhiteSpace(str)) return str;
            return InSimDotNet.Helpers.StringHelper.StripColors(str);
        }

        public static void JsonResult(List<SessionStats> raceStatsList, string fileName, SessionInfo sessionInfo, Configuration config, Dictionary<int, PlayerIdentity> connections = null)
        {
            // Sanitize filename to remove invalid characters
            fileName = SanitizeFileName(fileName);

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

            // Remove cars with 0 laps completed (e.g. relay drivers who never drove)
            raceStatsList.RemoveAll(p => p.lap.Count == 0);

            raceStatsList.Sort();

            var raceData = new RaceDataJson();

            // Session info (includes server/weather)
            DateTime sessionTime = sessionInfo.sessionStartTime != DateTime.MinValue
                ? sessionInfo.sessionStartTime
                : DateTime.Now;

            string sessionLengthStr = "";
            int effectiveQualMins = sessionInfo.rstQualMins > 0 ? sessionInfo.rstQualMins : sessionInfo.qualMins;
            int effectiveRaceLaps = sessionInfo.rstRaceLaps > 0 ? sessionInfo.rstRaceLaps : sessionInfo.raceLaps;

            if (sessionInfo.InQualification() && effectiveQualMins > 0)
                sessionLengthStr = effectiveQualMins == 1 ? "1 min" : effectiveQualMins + " mins";
            else if (effectiveRaceLaps == 0)
                sessionLengthStr = "Practice";
            else if (effectiveRaceLaps == 1)
                sessionLengthStr = "1 Lap";
            else if (effectiveRaceLaps < 191)
                sessionLengthStr = effectiveRaceLaps + " Laps";
            else if (effectiveRaceLaps == 191)
                sessionLengthStr = "1 Hour";
            else
                sessionLengthStr = (effectiveRaceLaps - 190) + " Hours";

            string sessionType;
            switch (sessionInfo.session)
            {
                case SessionInfo.Session.Qualification: sessionType = "qual"; break;
                case SessionInfo.Session.Practice: sessionType = "practice"; break;
                default: sessionType = "race"; break;
            }

            raceData.session = new SessionInfoJson
            {
                type = sessionType,
                track = sessionInfo.trackNameCode,
                trackName = InSimDotNet.Helpers.TrackHelper.GetFullTrackName(sessionInfo.trackNameCode),
                car = string.Join("+", BuildAllowedCarsList(sessionInfo)),
                laps = sessionInfo.rstRaceLaps > 0 ? sessionInfo.rstRaceLaps : sessionInfo.raceLaps,
                sessionTime = sessionInfo.rstQualMins > 0 ? sessionInfo.rstQualMins : (sessionInfo.qualMins > 0 ? sessionInfo.qualMins : (sessionInfo.raceLaps > 190 ? (sessionInfo.raceLaps - 190) * 60 : 0)),
                sessionLength = sessionLengthStr,
                date = !string.IsNullOrEmpty(sessionInfo.gameDate) ? sessionInfo.gameDate : sessionTime.ToString("yyyy-MM-dd"),
                time = !string.IsNullOrEmpty(sessionInfo.gameTime) ? sessionInfo.gameTime : sessionTime.ToString("HH:mm:ss"),
                splitsPerLap = 2,
                flags = BuildHostFlags(sessionInfo.hostFlags),
                server = sessionInfo.HName ?? "",
                utcOffset = sessionInfo.utcOffset,
                wind = sessionInfo.wind < strWind.Length ? strWind[sessionInfo.wind] : "",
                allowedCars = BuildAllowedCarsList(sessionInfo),
                carImages = ResolveCarImages(sessionInfo, raceStatsList)
            };

            // Metadata
            raceData.metadata = new MetadataJson
            {
                exportedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                mprUrl = "",
                logoUrl = config.DefaultLogoUrl
            };

            // Build players array and index lookup
            raceData.players = new List<PlayerInfo>();
            var playerIndex = new Dictionary<string, int>(); // username → index
            foreach (var p in raceStatsList)
            {
                foreach (var un in p.allPlayers)
                {
                    if (!playerIndex.ContainsKey(un.Value.userName))
                    {
                        playerIndex[un.Value.userName] = raceData.players.Count;
                        raceData.players.Add(new PlayerInfo
                        {
                            username = un.Value.userName,
                            name = lfsStripColor(un.Value.nickName),
                            nameColored = un.Value.nickName
                        });
                    }
                }
            }

            // Add non-driver connections (host, spectators) to players array
            if (connections != null)
            {
                foreach (var conn in connections)
                {
                    string username = conn.Value.userName;
                    if (string.IsNullOrEmpty(username))
                        username = "host"; // UCID 0 is the host
                    if (!playerIndex.ContainsKey(username))
                    {
                        playerIndex[username] = raceData.players.Count;
                        raceData.players.Add(new PlayerInfo
                        {
                            username = username,
                            name = lfsStripColor(conn.Value.nickName),
                            nameColored = conn.Value.nickName
                        });
                    }
                }
            }

            // Calculate positions
            Dictionary<string, List<int>> allPositions = CalculatePositions(raceStatsList);

            // Find winner for gap calculations
            SessionStats winner = raceStatsList.FirstOrDefault();
            int firstMaxLap = winner != null ? winner.lap.Count : 0;
            long firstTotalTime = winner != null ? winner.totalTime : 0;
            long winnerFinishTime = winner != null ? winner.totalTime : 0;

            // Build cars list
            raceData.cars = new List<CarEntry>();

            // Replace gridPos 999 (mid-session join) with next position after last real grid
            int maxRealGrid = 0;
            foreach (var p in raceStatsList)
                if (p.gridPos > 0 && p.gridPos < 999 && p.gridPos > maxRealGrid)
                    maxRealGrid = p.gridPos;
            foreach (var p in raceStatsList)
                if (p.gridPos >= 999)
                    p.gridPos = ++maxRealGrid;

            int curPos = 0;
            foreach (var p in raceStatsList)
            {
                curPos++;

                var carEntry = new CarEntry
                {
                    plid = p.PLID,
                    lastDriver = playerIndex[p.userName],
                    car = p.carName,
                    position = curPos,
                    gridPosition = p.gridPos,
                    lapsCompleted = p.lap.Count,
                    bestLapTime = SessionStats.LfsTimeToString(p.bestLap),
                    bestLapNumber = p.lapBestLap > 0 ? p.lapBestLap : (int?)null,
                    topSpeed = p.bestSpeed > 0 ? (double?)Math.Round(InSimDotNet.Helpers.MathHelper.SpeedToKph(p.bestSpeed), 1) : null,
                    topSpeedLap = p.lapBestSpeed > 0 ? (int?)p.lapBestSpeed : null,
                    pitStops = new List<PitStop>(),
                    penalties = new List<StatPenalty>(),
                    incidents = new IncidentCounts
                    {
                        yellowFlags = p.yellowFlags,
                        blueFlags = p.blueFlags,
                        contacts = p.contacts,
                        offTracks = p.offTracks
                    }
                };

                // Status and total time
                if (p.resultNum == 999)
                {
                    carEntry.status = "dnf";
                    carEntry.totalTime = "DNF";
                }
                else if (firstMaxLap == p.lap.Count)
                {
                    carEntry.status = "finished";
                    carEntry.totalTime = curPos == 1
                        ? SessionStats.LfsTimeToString(p.totalTime)
                        : "+" + SessionStats.LfsTimeToString(p.totalTime - firstTotalTime);
                }
                else
                {
                    long timeDiff = Math.Abs(p.totalTime - winnerFinishTime);
                    if (timeDiff < 200000)
                    {
                        carEntry.status = "lapped";
                        int lapsDown = firstMaxLap - p.lap.Count;
                        carEntry.totalTime = "+" + lapsDown + (lapsDown == 1 ? " lap" : " laps");
                    }
                    else
                    {
                        carEntry.status = "dnf";
                        carEntry.totalTime = "DNF";
                    }
                }

                // Stints (always present)
                carEntry.stints = new List<DriverStint>();
                if (p.toc.Count > 0)
                {
                    // First driver before first TOC
                    var firstToc = p.toc[0];
                    carEntry.stints.Add(new DriverStint
                    {
                        driver = playerIndex[firstToc.oldUserName],
                        fromLap = 1,
                        toLap = firstToc.lap - 1
                    });
                    for (int t = 0; t < p.toc.Count; t++)
                    {
                        var toc = p.toc[t];
                        int endLap = (t + 1 < p.toc.Count) ? p.toc[t + 1].lap - 1 : p.lap.Count;
                        carEntry.stints.Add(new DriverStint
                        {
                            driver = playerIndex[toc.newUserName],
                            fromLap = toc.lap,
                            toLap = endLap
                        });
                    }
                }
                else
                {
                    // Single driver
                    carEntry.stints.Add(new DriverStint
                    {
                        driver = playerIndex[p.userName],
                        fromLap = 1,
                        toLap = p.lap.Count
                    });
                }

                // Pit stops
                foreach (var pit in p.pit)
                {
                    var pitStop = new PitStop
                    {
                        lap = pit.LapsDone + 1,
                        duration = (pit.STime / 1000.0).ToString("F3"),
                        reason = new List<string>()
                    };
                    if ((pit.Work & PitWorkFlags.PSE_STOP) != 0) pitStop.reason.Add("stop");
                    if ((pit.Work & (PitWorkFlags.PSE_FR_DAM | PitWorkFlags.PSE_RE_DAM |
                        PitWorkFlags.PSE_LE_FR_DAM | PitWorkFlags.PSE_RI_FR_DAM |
                        PitWorkFlags.PSE_LE_RE_DAM | PitWorkFlags.PSE_RI_RE_DAM |
                        PitWorkFlags.PSE_BODY_MINOR | PitWorkFlags.PSE_BODY_MAJOR)) != 0)
                        pitStop.reason.Add("damage");
                    if ((pit.Work & (PitWorkFlags.PSE_FR_WHL | PitWorkFlags.PSE_RE_WHL |
                        PitWorkFlags.PSE_LE_FR_WHL | PitWorkFlags.PSE_RI_FR_WHL |
                        PitWorkFlags.PSE_LE_RE_WHL | PitWorkFlags.PSE_RI_RE_WHL)) != 0)
                        pitStop.reason.Add("tires");
                    if ((pit.Work & PitWorkFlags.PSE_REFUEL) != 0) pitStop.reason.Add("refuel");
                    if ((pit.Work & PitWorkFlags.PSE_SETUP) != 0) pitStop.reason.Add("setup");
                    carEntry.pitStops.Add(pitStop);
                }

                // Lap times
                carEntry.lapTimes = new List<string>();
                foreach (var lap in p.lap)
                    carEntry.lapTimes.Add(SessionStats.LfsTimeToString(lap.LapTime));

                // Lap elapsed times
                carEntry.lapETimes = new List<double>();
                foreach (var te in p.timingEvents.Where(te => te.Split == 0 && te.ETime > 0 && te.Lap > 0).OrderBy(te => te.ETime))
                    carEntry.lapETimes.Add(Math.Round(te.ETime / 1000.0, 3));

                // Positions array
                if (allPositions.ContainsKey(p.userName))
                    carEntry.positions = allPositions[p.userName].ToArray();

                // Best splits (filter placeholders)
                carEntry.bestSplits = new List<string>();
                if (p.BestSector != null)
                {
                    foreach (var sector in p.BestSector)
                    {
                        if (sector > 10 && sector < 3599000)  // Filter 0 and 1:00:00.000 placeholders
                            carEntry.bestSplits.Add(SessionStats.LfsTimeToString(sector));
                    }
                }

                raceData.cars.Add(carEntry);
            }

            // Rankings
            raceData.rankings = new RankingsJson();

            // Fastest lap
            var fastestDriver = raceStatsList.OrderBy(d => d.bestLap).FirstOrDefault(d => d.bestLap > 0);
            if (fastestDriver != null)
            {
                raceData.rankings.fastestLap = new FastestLapInfo
                {
                    driver = playerIndex[fastestDriver.userName],
                    time = SessionStats.LfsTimeToString(fastestDriver.bestLap),
                    lap = fastestDriver.lapBestLap
                };
            }

            // Pit stops ranking
            raceData.rankings.pitStops = new List<PitStopRanking>();
            var allPitStops = new List<(int driver, int lap, long duration, List<string> reason)>();
            foreach (var driver in raceStatsList)
            {
                foreach (var pit in driver.pit)
                {
                    var reasons = new List<string>();
                    if ((pit.Work & (PitWorkFlags.PSE_FR_DAM | PitWorkFlags.PSE_RE_DAM |
                        PitWorkFlags.PSE_LE_FR_DAM | PitWorkFlags.PSE_RI_FR_DAM |
                        PitWorkFlags.PSE_LE_RE_DAM | PitWorkFlags.PSE_RI_RE_DAM |
                        PitWorkFlags.PSE_BODY_MINOR | PitWorkFlags.PSE_BODY_MAJOR)) != 0)
                        reasons.Add("damage");
                    if ((pit.Work & (PitWorkFlags.PSE_FR_WHL | PitWorkFlags.PSE_RE_WHL |
                        PitWorkFlags.PSE_LE_FR_WHL | PitWorkFlags.PSE_RI_FR_WHL |
                        PitWorkFlags.PSE_LE_RE_WHL | PitWorkFlags.PSE_RI_RE_WHL)) != 0)
                        reasons.Add("tires");
                    if ((pit.Work & PitWorkFlags.PSE_REFUEL) != 0) reasons.Add("refuel");
                    allPitStops.Add((playerIndex[driver.userName], pit.LapsDone + 1, pit.STime, reasons));
                }
            }
            int pitPos = 0;
            foreach (var pit in allPitStops.OrderBy(p => p.duration))
            {
                pitPos++;
                raceData.rankings.pitStops.Add(new PitStopRanking
                {
                    position = pitPos,
                    driver = pit.driver,
                    lap = pit.lap,
                    duration = (pit.duration / 1000.0).ToString("F3"),
                    reason = pit.reason
                });
            }

            // First lap ranking
            raceData.rankings.firstLap = new List<FirstLapRanking>();
            var driversWithFirstLap = raceStatsList.Where(d => d.lap.Count > 0).OrderBy(d => d.lap[0].LapTime).ToList();
            long firstLapLeader = driversWithFirstLap.FirstOrDefault()?.lap[0].LapTime ?? 0;
            int firstLapPos = 0;
            foreach (var driver in driversWithFirstLap)
            {
                firstLapPos++;
                raceData.rankings.firstLap.Add(new FirstLapRanking
                {
                    position = firstLapPos,
                    driver = playerIndex[driver.userName],
                    time = SessionStats.LfsTimeToString(driver.lap[0].LapTime),
                    gap = SessionStats.LfsTimeToString(driver.lap[0].LapTime - firstLapLeader)
                });
            }

            // Top speed ranking
            raceData.rankings.topSpeed = new List<TopSpeedRanking>();
            var driversWithSpeed = raceStatsList.Where(d => d.bestSpeed > 0).OrderByDescending(d => d.bestSpeed).ToList();
            double topSpeedKph = driversWithSpeed.FirstOrDefault() != null
                ? Math.Round(InSimDotNet.Helpers.MathHelper.SpeedToKph(driversWithSpeed.First().bestSpeed), 1) : 0;
            int speedPos = 0;
            foreach (var driver in driversWithSpeed)
            {
                speedPos++;
                double driverKph = Math.Round(InSimDotNet.Helpers.MathHelper.SpeedToKph(driver.bestSpeed), 1);
                raceData.rankings.topSpeed.Add(new TopSpeedRanking
                {
                    position = speedPos,
                    driver = playerIndex[driver.userName],
                    speed = driverKph,
                    gap = Math.Round(topSpeedKph - driverKph, 1),
                    lap = driver.lapBestSpeed
                });
            }

            // Events
            raceData.events = new EventsJson
            {
                overtakes = new List<OvertakeInfo>(),
                incidents = new List<Incident>()
            };
            foreach (var overtake in sessionInfo.overtakes)
            {
                if (playerIndex.ContainsKey(overtake.overtaker) && playerIndex.ContainsKey(overtake.overtaken))
                {
                    raceData.events.overtakes.Add(new OvertakeInfo
                    {
                        overtaker = playerIndex[overtake.overtaker],
                        overtaken = playerIndex[overtake.overtaken],
                        lap = overtake.lap,
                        gap = (double)overtake.gapMs / 1000.0
                    });
                }
            }

            // Chat
            // Build nick → player index lookup for chat
            var nickToIndex = new Dictionary<string, int>();
            foreach (var p in raceStatsList)
            {
                foreach (var un in p.allPlayers)
                {
                    string nick = lfsStripColor(un.Value.nickName);
                    if (!nickToIndex.ContainsKey(nick) && playerIndex.ContainsKey(un.Value.userName))
                        nickToIndex[nick] = playerIndex[un.Value.userName];
                }
            }

            // Also map connection nicks (host, spectators)
            if (connections != null)
            {
                foreach (var conn in connections)
                {
                    string nick = lfsStripColor(conn.Value.nickName);
                    string username = string.IsNullOrEmpty(conn.Value.userName) ? "host" : conn.Value.userName;
                    if (!nickToIndex.ContainsKey(nick) && playerIndex.ContainsKey(username))
                        nickToIndex[nick] = playerIndex[username];
                }
            }

            raceData.chat = new List<ChatMessage>();
            foreach (var chatEntry in sessionInfo.chat)
            {
                string nick = lfsStripColor(chatEntry.nickName);
                int chatDriverIdx = nickToIndex.ContainsKey(nick) ? nickToIndex[nick] : -1;
                raceData.chat.Add(new ChatMessage
                {
                    driver = chatDriverIdx,
                    message = lfsStripColor(chatEntry.text)
                });
            }

            // World Records
            raceData.worldRecords = new Dictionary<string, WorldRecord>();
            try
            {
                if (!string.IsNullOrEmpty(sessionInfo.trackNameCode) && raceStatsList.Count > 0)
                {
                    var uniqueCars = raceStatsList.Where(d => !string.IsNullOrEmpty(d.carName)).Select(d => d.carName).Distinct();
                    foreach (var carName in uniqueCars)
                    {
                        var wr = LFSWorld.GetWR(sessionInfo.trackNameCode, carName);
                        if (wr != null)
                        {
                            var wrData = new WorldRecord
                            {
                                lapTime = SessionStats.LfsTimeToString(wr.WrTime),
                                racer = wr.RacerName,
                                track = wr.Track,
                                car = wr.CarName,
                                splits = new List<string>(),
                                sectors = new List<string>()
                            };
                            if (wr.Split != null)
                                foreach (var split in wr.Split)
                                    wrData.splits.Add(SessionStats.LfsTimeToString(split));
                            if (wr.Sector != null)
                                foreach (var sector in wr.Sector)
                                    wrData.sectors.Add(SessionStats.LfsTimeToString(sector));
                            raceData.worldRecords[carName] = wrData;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LFSStats.WriteLine("Could not retrieve World Record data: " + ex.Message);
            }

            // Serialize to JSON
            string jsonPath = Path.Combine(outputDir, fileName + ".json");
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(raceData, Newtonsoft.Json.Formatting.Indented,
                new Newtonsoft.Json.JsonSerializerSettings { NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore });
            File.WriteAllText(jsonPath, json);

            // Copy stats viewer templates
            try
            {
                string viewerHtmlSrc = Path.Combine("templates", "stats_viewer.html");
                string viewerJsSrc = Path.Combine("templates", "stats_renderer.js");
                if (File.Exists(viewerHtmlSrc))
                    File.Copy(viewerHtmlSrc, Path.Combine(outputDir, "stats_viewer.html"), overwrite: true);
                if (File.Exists(viewerJsSrc))
                    File.Copy(viewerJsSrc, Path.Combine(outputDir, "stats_renderer.js"), overwrite: true);

                string jsonFileName = Path.GetFileName(jsonPath);
                Console.WriteLine($"Stats viewer: stats_viewer.html?json={jsonFileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not copy stats viewer templates: {ex.Message}");
            }
        }


        private static Dictionary<string, List<int>> CalculatePositions(List<SessionStats> raceStatsList)
        {
            // Calculate positions using ETime from IS_SPX and IS_LAP timing events
            // Use userName (license) as key - it's unique and doesn't change during race
            Dictionary<string, List<int>> allPositions = new Dictionary<string, List<int>>();

            // Build userName -> finalPosition mapping for tiebreaker
            // raceStatsList is already sorted by CompareTo (final race order)
            Dictionary<string, int> finalPositionIndex = new Dictionary<string, int>();
            for (int i = 0; i < raceStatsList.Count; i++)
            {
                finalPositionIndex[raceStatsList[i].userName] = i;
            }

            // Initialize position lists
            foreach (var driver in raceStatsList)
            {
                allPositions[driver.userName] = new List<int>();
            }

            // Collect all unique timing points (lap/split combinations) across all drivers
            var allTimingPoints = new HashSet<(int lap, int split)>();
            foreach (var driver in raceStatsList)
            {
                foreach (var evt in driver.timingEvents)
                {
                    allTimingPoints.Add((evt.Lap, evt.Split));
                }
            }

            // Sort timing points by lap then split
            // Split ordering: S1 (split=1) comes before LAP (split=0) chronologically
            // So we need to sort split descending to get S1 before LAP, then S2, etc.
            // For splits > 0, larger numbers come later in the lap
            // But split=0 (LAP) always comes last
            var sortedTimingPoints = allTimingPoints
                .OrderBy(tp => tp.lap)
                .ThenBy(tp => tp.split == 0 ? 999 : tp.split)  // LAP (0) always last
                .ToList();

            // For each timing point, calculate positions based on ETime
            foreach (var timingPoint in sortedTimingPoints)
            {
                var driversAtPoint = new List<(string userName, long eTime, int finalPosIndex)>();

                foreach (var driver in raceStatsList)
                {
                    var evt = driver.timingEvents
                        .FirstOrDefault(e => e.Lap == timingPoint.lap && e.Split == timingPoint.split);

                    if (evt != null)
                    {
                        driversAtPoint.Add((driver.userName, evt.ETime, finalPositionIndex[driver.userName]));
                    }
                }

                // Sort by ETime, use final position as tiebreaker
                var sortedDrivers = driversAtPoint.OrderBy(d => d.eTime).ThenBy(d => d.finalPosIndex).ToList();

                for (int pos = 0; pos < sortedDrivers.Count; pos++)
                {
                    allPositions[sortedDrivers[pos].userName].Add(pos + 1);
                }
            }

            return allPositions;
        }

        /// <summary>
        /// Builds list of allowed car codes from ALC bitmask + MAL mod list.
        /// </summary>
        private static List<string> BuildAllowedCarsList(SessionInfo sessionInfo)
        {
            var cars = new List<string>();

            // Standard cars from CarFlags enum
            var flags = (CarFlags)sessionInfo.allowedCarFlags;
            if (flags != CarFlags.None)
            {
                foreach (CarFlags car in Enum.GetValues(typeof(CarFlags)))
                {
                    if (car == CarFlags.None || car == CarFlags.All) continue;
                    if (flags.HasFlag(car))
                        cars.Add(car.ToString());
                }
            }

            // Mods from IS_MAL
            cars.AddRange(sessionInfo.allowedMods);

            return cars;
        }

        /// <summary>
        /// Resolves car image URLs for all unique cars used in the session.
        /// Standard cars: direct URL. Mods: scraped from lfs.net.
        /// </summary>
        private static Dictionary<string, string> ResolveCarImages(SessionInfo sessionInfo, List<SessionStats> raceStatsList)
        {
            var images = new Dictionary<string, string>();

            // Collect unique car codes from race data
            var uniqueCars = new HashSet<string>();
            foreach (var p in raceStatsList)
                if (!string.IsNullOrEmpty(p.carName))
                    uniqueCars.Add(p.carName);

            foreach (var car in uniqueCars)
            {
                if (car.Length <= 3)
                {
                    // Standard car
                    images[car] = "https://www.lfs.net/static/showroom/cars480crp/" + car + ".png";
                }
                else
                {
                    // Mod - scrape lfs.net
                    try
                    {
                        var url = "https://www.lfs.net/files/vehmods/" + car;
                        using (var client = new System.Net.WebClient())
                        {
                            var html = client.DownloadString(url);
                            var match = System.Text.RegularExpressions.Regex.Match(html, @"<img[^>]*class=""[^""]*carModDetailImage[^""]*""[^>]*src=""([^""]+)""");
                            if (!match.Success)
                                match = System.Text.RegularExpressions.Regex.Match(html, @"<img[^>]*src=""([^""]+)""[^>]*class=""[^""]*carModDetailImage[^""]*""");
                            if (match.Success)
                            {
                                var imgUrl = match.Groups[1].Value;
                                if (imgUrl.StartsWith("/"))
                                    imgUrl = "https://www.lfs.net" + imgUrl;
                                images[car] = imgUrl;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LFSStats.WriteLine("Failed to resolve car image for " + car + ": " + ex.Message, Verbose.Session);
                    }
                }
            }

            return images;
        }

        private static List<string> BuildHostFlags(int flags)
        {
            var result = new List<string>();
            var rf = (RaceFlags)flags;
            if (rf.HasFlag(RaceFlags.HOSTF_MUST_PIT)) result.Add("mustPit");
            if (rf.HasFlag(RaceFlags.HOSTF_CAN_RESET)) result.Add("canReset");
            if (!rf.HasFlag(RaceFlags.HOSTF_CAN_REFUEL)) result.Add("noRefuel");
            if (rf.HasFlag(RaceFlags.HOSTF_NO_FLOOD)) result.Add("noFloodLights");
            if (rf.HasFlag(RaceFlags.HOSTF_SHOW_FUEL)) result.Add("showFuel");
            if (rf.HasFlag(RaceFlags.HOSTF_MID_RACE)) result.Add("midRaceJoin");
            if (rf.HasFlag(RaceFlags.HOSTF_FCV)) result.Add("forcedCockpit");
            return result;
        }
    }
}