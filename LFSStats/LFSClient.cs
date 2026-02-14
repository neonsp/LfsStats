using InSimDotNet;
using InSimDotNet.Packets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LFSStatistics
{
    public class LFSClient
    {
        //bool wrLoaded = false;	// unused
        InSimDotNet.InSim insimConnection;
        Dictionary<int, SessionStats> raceStat;
        List<SessionStats> disconnectedDrivers;  // Preserve stats when PLID is reused
        Dictionary<int, int> PLIDToUCID = new Dictionary<int, int>();
        Dictionary<int, PlayerIdentity> connectionPlayers = new Dictionary<int, PlayerIdentity>();
        Dictionary<int, int> pendingGridPos = new Dictionary<int, int>();  // Store gridPos until IS_NPL arrives
                                                                           // used to check for session state, thus even before a reset is performed on new session start RST
        SessionInfo sessionInfo = new SessionInfo();

        int RefreshInterval = 100;
        bool flagFirst = true; // used to get all connections on first STA packet
        bool preserveLapsOnPit = true; // Keep lap data when driver ESC-pits and rejoins
        ConsoleCtrl cc;
        Configuration config;

        private const byte RIP_REQID = 10; // Our ReqI for replay info requests

        /// <summary>
        /// Resets objects used to collect statistics
        /// </summary>
        private void resetStatistics()
        {
            // created new since old ones are used during parallel export
            raceStat = new Dictionary<int, SessionStats>(); //raceStat.Clear();
            disconnectedDrivers = new List<SessionStats>();
            PLIDToUCID.Clear();
            connectionPlayers.Clear();
            sessionInfo = new SessionInfo();

        }

        string getFormattedTime()
        {
            return (string.Format("{0:0000}", DateTime.Now.Year)
                + "-" + string.Format("{0:00}", DateTime.Now.Month)
                + "-" + string.Format("{0:00}", DateTime.Now.Day)
                + LFSStats.filePathSpaceChar + string.Format("{0:00}", DateTime.Now.Hour)
                + "." + string.Format("{0:00}", DateTime.Now.Minute)
                + "." + string.Format("{0:00}", DateTime.Now.Second));
        }
        public string NickNameByUCID(int UCID)
        {
            if (!connectionPlayers.ContainsKey(UCID)) return "???";
            return (connectionPlayers[UCID]).nickName;
        }
        public string UserNameByUCID(int UCID)
        {
            return (connectionPlayers[UCID]).userName;
        }
        public int PLIDbyNickName(string nickName)
        {
            foreach (KeyValuePair<int, SessionStats> pair in raceStat)
            {
                if (nickName == (pair.Value).NickName)
                    return (pair.Value).PLID;
            }
            return -1;
        }
        public int PLIDbyPLID(int plid)
        {
            foreach (KeyValuePair<int, SessionStats> pair in raceStat)
            {
                if (plid == (pair.Value).PLID)
                    return (pair.Value).PLID;
            }
            return -1;
        }
        public int PLIDbyUName(string uname)
        {
            foreach (KeyValuePair<int, SessionStats> pair in raceStat)
            {
                if (uname == (pair.Value).userName)
                    return (pair.Value).PLID;
            }
            return -1;
        }
        public int PLIDbyFinPLID(int finPLID)
        {
            foreach (KeyValuePair<int, SessionStats> pair in raceStat)
            {
                if (finPLID == (pair.Value).finPLID)
                    return (pair.Value).PLID;
            }
            return -1;
        }
        public int UCIDToPLID(int UCID)
        {
            foreach (KeyValuePair<int, int> pair in PLIDToUCID)
            {
                if (UCID == (int)pair.Value)
                    return (int)pair.Key;
            }
            return -1;
        }
        /// <summary>
        /// Input handler.
        /// Intercepts Ctrl-C and other break commands and Disconnect LFSStats in safe mode
        /// </summary>
        /// <param name="consoleEvent">The console event.</param>
        public void inputHandler(ConsoleCtrl.ConsoleEvent consoleEvent)
        {
            if (consoleEvent == ConsoleCtrl.ConsoleEvent.CtrlC
                || consoleEvent == ConsoleCtrl.ConsoleEvent.CtrlClose
                || consoleEvent == ConsoleCtrl.ConsoleEvent.CtrlBreak
                || consoleEvent == ConsoleCtrl.ConsoleEvent.CtrlLogoff
                || consoleEvent == ConsoleCtrl.ConsoleEvent.CtrlShutdown)
            {

                // Send Insim close
                if (insimConnection.IsConnected)
                {
                    insimConnection.Send(new IS_TINY
                    {
                        SubT = TinyType.TINY_CLOSE
                    });
                }
                // NOTE yes the exit codes should be enumerated.
                // They can be negative but usually are not and positive means an error, 0 success.
                LFSStats.Exit(0);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LFSClient"/> class.
        /// </summary>
        /// <param name="configFileName">The configFileName.</param>
        /// <param name="debugmode">if set to <c>true</c> [debugmode].</param>
        public LFSClient(string configFileName, int refreshInterval)
        {
            RefreshInterval = refreshInterval;

            resetStatistics();
            // Create keyboard events handler
            cc = new ConsoleCtrl();
            cc.ControlEvent += new ConsoleCtrl.ControlEventHandler(inputHandler);

            // Load Config
            config = new Configuration(configFileName);

            LFSStats.WriteLine("\n");
            // Load World Record
            LFSWorld.Initialize(config.PubStatIDkey);

            // Initialize InSim
            insimConnection = new InSimDotNet.InSim();
            insimConnection.Bind<IS_TINY>(EventReceived);
            insimConnection.Bind<IS_VER>(InSimVer);
            insimConnection.Bind<IS_RST>(OnSessionStart);
            insimConnection.Bind<IS_NCN>(NewConnection);
            insimConnection.Bind<IS_NPL>(NewPlayer);
            insimConnection.Bind<IS_PLP>(OnPlayerPits);
            insimConnection.Bind<IS_PLL>(OnPlayerLeave);
            insimConnection.Bind<IS_CPR>(OnPlayerRename);
            insimConnection.Bind<IS_PIT>(OnPit);
            insimConnection.Bind<IS_PSF>(OnPitFinished);
            insimConnection.Bind<IS_LAP>(OnLap);
            insimConnection.Bind<IS_SPX>(OnSplit);
            insimConnection.Bind<IS_RES>(OnResult);
            insimConnection.Bind<IS_FIN>(OnFinish);
            insimConnection.Bind<IS_REO>(OnReorder);
            insimConnection.Bind<IS_STA>(OnState);
            insimConnection.Bind<IS_MSO>(OnMessage);
            insimConnection.Bind<IS_ISM>(OnInsimMulti);
            insimConnection.Bind<IS_MCI>(OnMCI);
            insimConnection.Bind<IS_FLG>(OnFlag);
            insimConnection.Bind<IS_MAL>(OnModsAllowed);
            insimConnection.Bind<IS_PEN>(OnPenalty);
            insimConnection.Bind<IS_TOC>(OnTOC);
            insimConnection.Bind<IS_CON>(OnContact);
            insimConnection.Bind<IS_HLV>(OnHotlapValidity);
            insimConnection.Bind<IS_SMALL>(OnSmall);
            insimConnection.Bind<IS_RIP>(OnReplayInfo);

            insimConnection.Disconnected += (sender, e) =>
            {
                LFSStats.WriteLine("Disconnected from InSim: " + e.Reason);
                LFSStats.Exit(1);
            };

            var flags = InSimFlags.ISF_MCI | /*InSimFlags.ISF_NLP |*/ InSimFlags.ISF_CON;
            if (config.IsLocal)
            {
                flags |= InSimFlags.ISF_LOCAL;
            }
            var insimSettings = new InSimSettings
            {
                Host = config.IpEndPoint.Address.ToString(),
                Port = config.IpEndPoint.Port,
                Admin = config.AdminPassword,
                Flags = flags,
                Interval = RefreshInterval

            };
            if (config.TcpMode)
            {
                insimSettings.Port = config.IpEndPoint.Port;
            }
            else
            {
                insimSettings.UdpPort = config.IpEndPoint.Port;
            }

            try
            {
                insimConnection.Initialize(insimSettings);
                if (!insimConnection.IsConnected)
                {
                    throw new Exception("Could not connect to InSim. Check connection details.");
                }

                // Interactive console menu
                PrintMenu();
                while (insimConnection.IsConnected)
                {
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true);
                        switch (char.ToUpper(key.KeyChar))
                        {
                            case 'L':
                                preserveLapsOnPit = !preserveLapsOnPit;
                                Console.WriteLine($"Preserve laps on ESC pit: {(preserveLapsOnPit ? "ON" : "OFF")}");
                                break;
                            case 'F':
                                RequestReplayEnd();
                                break;
                            case 'H':
                                PrintMenu();
                                break;
                            case 'Q':
                                LFSStats.WriteLine("Menu", "Disconnecting...", Verbose.Program);
                                goto exitLoop;
                        }
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
            exitLoop:
                if (insimConnection.IsConnected)
                {
                    insimConnection.Disconnect();
                }
            }
            catch (Exception ex)
            {
                LFSStats.WriteLine("Failed to initialize InSim: " + ex.Message);
                LFSStats.Exit(1);
            }
        }

        private void OnPlayerPits(InSimDotNet.InSim insim, IS_PLP packet)
        {
#if DEBUG
            LFSStats.WriteLine("Player pits (goes to setup - stays in player list)", Verbose.Info);
#endif
        }

        private void OnPlayerLeave(InSimDotNet.InSim insim, IS_PLL pll)
        {
#if DEBUG
            LFSStats.WriteLine("Player leaves race (spectate - leaves player list)", Verbose.Info);
#endif
            // Preserve driver stats before PLID gets reused
            if (raceStat.ContainsKey(pll.PLID))
            {
                SessionStats leavingDriver = raceStat[pll.PLID];

                // Only preserve if driver has meaningful data (at least one lap or sector time)
                if (leavingDriver.lap.Count > 0 || leavingDriver.bestLap > 0)
                {
                    // Clone the driver data to preserve it
                    SessionStats preservedDriver = leavingDriver.Clone();
                    preservedDriver.disconnected = true;
                    preservedDriver.disconnectTime = DateTime.Now;

                    disconnectedDrivers.Add(preservedDriver);

                    LFSStats.WriteLine($"Preserved stats for {leavingDriver.userName} (PLID {pll.PLID}, {leavingDriver.lap.Count} laps)", Verbose.Info);
                }

                // Now safe to remove from active list
                raceStat.Remove(pll.PLID);
                PLIDToUCID.Remove(pll.PLID);
            }
        }

        private void OnPlayerRename(InSimDotNet.InSim insim, IS_CPR cpr)
        {
            if (connectionPlayers.ContainsKey(cpr.UCID))
            {
                var oldNick = connectionPlayers[cpr.UCID].nickName;
                connectionPlayers[cpr.UCID].nickName = cpr.PName;

                int plid = UCIDToPLID(cpr.UCID);
                if (plid != -1)
                {
                    raceStat[plid].NickName = cpr.PName;
                }

#if DEBUG
                LFSStats.WriteLine($"Conn Player {connectionPlayers[cpr.UCID].userName} Rename from {oldNick} to {cpr.PName}, id: {cpr.UCID}", Verbose.Info);
#endif
            }
        }

        private void OnPit(InSimDotNet.InSim insim, IS_PIT pitDec)
        {
            if (raceStat.ContainsKey(pitDec.PLID))
            {
                raceStat[pitDec.PLID].updatePIT(pitDec);
                LFSStats.WriteLine("Pit", connectionPlayers[PLIDToUCID[pitDec.PLID]].nickName, Verbose.Info);
            }
        }

        private void OnPitFinished(InSimDotNet.InSim insim, IS_PSF pitFin)
        {
            if (raceStat.ContainsKey(pitFin.PLID))
            {
                raceStat[pitFin.PLID].updatePSF(pitFin.PLID, pitFin.STime);
            }
        }

        private void OnLap(InSimDotNet.InSim insim, IS_LAP lapDec)
        {
            if (!raceStat.ContainsKey(lapDec.PLID))
            {
                // Console.WriteLine($"DEBUG: OnLap - PLID {lapDec.PLID} not in raceStat (user unknown)");
                return;
            }

            LFSStats.WriteLine("Lap " + lapDec.LapsDone, 0, raceStat[lapDec.PLID].userName, Verbose.Lap);

            string sessionName = sessionInfo.session.ToString().Substring(0, 4);

            if (lapDec.LapsDone > sessionInfo.currentLap)
            {
                sessionInfo.currentLap = lapDec.LapsDone;
                LFSStats.ConsoleTitleShow(sessionInfo.trackNameCode + ": " + sessionName + " Lap " + (lapDec.LapsDone + 1).ToString());
            }
            long eTime = lapDec.ETime.TotalMillisecondsLong();
            long lapTime = lapDec.LTime.TotalMillisecondsLong();

            if (sessionInfo.InQualification())
            {
                // In qualifying, record the lap with ETime but skip grid/overtake logic
                raceStat[lapDec.PLID].UpdateLap(lapTime, raceStat[lapDec.PLID].numStop, lapDec.LapsDone, sessionInfo.maxSplit, eTime);
                return;
            }

            // Fix corrupt LTime: if LTime matches ETime (absolute session time instead of lap duration),
            // calculate actual lap time from ETime delta. This happens after disconnect/reconnect in races.
            if (lapTime > 0 && eTime > 0)
            {
                var lastEvent = raceStat[lapDec.PLID].timingEvents
                    .Where(te => te.Split == 0 && te.ETime > 0)
                    .OrderByDescending(te => te.ETime)
                    .FirstOrDefault();
                if (lastEvent != null)
                {
                    long eTimeDelta = eTime - lastEvent.ETime;
                    // If LTime is more than double the ETime delta, it's likely corrupt (absolute ETime)
                    if (eTimeDelta > 0 && lapTime > eTimeDelta * 2 && lapTime > 600000) // > 10 min threshold
                    {
                        LFSStats.WriteLine($"Fixed corrupt LTime for {raceStat[lapDec.PLID].userName}: LTime={lapTime}ms -> ETimeDelta={eTimeDelta}ms", Verbose.Info);
                        lapTime = eTimeDelta;
                    }
                }
            }

            // Add grid timing event if this is the first timing event and gridPos is set
            bool hasGridEvent = raceStat[lapDec.PLID].timingEvents.Any(e => e.Lap == 0);
            if (!hasGridEvent && raceStat[lapDec.PLID].gridPos > 0 && raceStat[lapDec.PLID].gridPos < 999)
            {
                raceStat[lapDec.PLID].timingEvents.Add(new TimingEvent(0, 0, raceStat[lapDec.PLID].gridPos));
            }

            raceStat[lapDec.PLID].UpdateLap(lapTime, raceStat[lapDec.PLID].numStop, lapDec.LapsDone, sessionInfo.maxSplit, eTime);

            // Detect overtakes at this timing point
            DetectOvertake(lapDec.PLID, lapDec.LapsDone, eTime);
        }

        private void OnSplit(InSimDotNet.InSim insim, IS_SPX splitdec)
        {
            // 3600000ms = 1:00:00.000 → LFS placeholder for invalid split times
            if (splitdec.STime.TotalMilliseconds >= 3600000)
                return;

            if (!raceStat.ContainsKey(splitdec.PLID))
            {
                // Console.WriteLine($"DEBUG: OnSplit - PLID {splitdec.PLID} not in raceStat (user unknown)");
                return;
            }

            // Add grid timing event if this is the first timing event and gridPos is set
            bool hasGridEvent = raceStat[splitdec.PLID].timingEvents.Any(e => e.Lap == 0);
            if (!hasGridEvent && raceStat[splitdec.PLID].gridPos > 0 && raceStat[splitdec.PLID].gridPos < 999)
            {
                raceStat[splitdec.PLID].timingEvents.Add(new TimingEvent(0, 0, raceStat[splitdec.PLID].gridPos));
                // Console.WriteLine($"[GRID] Added grid timing event in OnSplit: {raceStat[splitdec.PLID].userName} at P{raceStat[splitdec.PLID].gridPos}");
            }

            long eTime = splitdec.ETime.TotalMillisecondsLong();
            // Console.WriteLine($"DEBUG: OnSplit - {raceStat[splitdec.PLID].userName} Split {splitdec.Split} ETime={eTime}ms");
            raceStat[splitdec.PLID].UpdateSplit(splitdec.Split, splitdec.STime.TotalMillisecondsLong(), eTime);
            LFSStats.WriteLine("SP " + splitdec.Split, splitdec.STime.TotalMillisecondsLong(), raceStat[splitdec.PLID].userName, Verbose.Split);

            if (sessionInfo.maxSplit < splitdec.Split)
                sessionInfo.maxSplit = splitdec.Split;

            // Detect overtakes at this timing point (only in race, not in qualifying)
            if (sessionInfo.session == SessionInfo.Session.Race)
                DetectOvertake(splitdec.PLID, raceStat[splitdec.PLID].lap.Count + 1, eTime);
        }

        private void OnResult(InSimDotNet.InSim insim, IS_RES result)
        {
#if DEBUG
            // Console.WriteLine("RESult (qualify or finish)");
#endif
            if (sessionInfo.InRace())
                LFSStats.WriteLine("Result", result.ResultNum + 1, ExportStats.lfsStripColor(result.PName), Verbose.Session);

            int lplid = result.PLID;
            if (result.PLID == 0)
                lplid = PLIDbyUName(result.UName);

            lplid = PLIDbyFinPLID(lplid);

            if (lplid != -1 && sessionInfo.InRace())
            {
                LFSStats.WriteLine($"[DEBUG-RES] UName={result.UName} PLID={result.PLID} lplid={lplid} ResultNum={result.ResultNum} TTime={result.TTime} Confirm={result.Confirm} NumStops={result.NumStops}", Verbose.Info);
                raceStat[lplid].UpdateResult(result.TTime.TotalMillisecondsLong(), result.ResultNum, result.CName, result.Confirm, result.NumStops);
            }

            if (lplid != -1 && sessionInfo.InQualification())
                raceStat[lplid].UpdateQualResult(result.TTime.TotalMillisecondsLong(), result.BTime.TotalMillisecondsLong(), result.NumStops, result.Confirm, result.LapsDone, result.ResultNum, result.NumRes, sessionInfo.maxSplit);
        }

        private void OnFinish(InSimDotNet.InSim insim, IS_FIN fin)
        {
#if DEBUG
            // Console.WriteLine("FINish (qualify or finish)");
#endif
            if (raceStat.ContainsKey(fin.PLID))
            {
                raceStat[fin.PLID].finished = true;
                raceStat[fin.PLID].finPLID = fin.PLID;
            }
        }

        private void OnReorder(InSimDotNet.InSim insim, IS_REO pacreo)
        {
            // Console.WriteLine($"OnReorder: NumP={pacreo.NumP}, ReqI={pacreo.ReqI}");

            // Store grid positions - only create SessionStats when IS_NPL arrives
            pendingGridPos.Clear();
            for (int i = 0; i < pacreo.NumP; i++)
            {
                int PLID = pacreo.PLID[i];
                int gridPos = i + 1;

                // If SessionStats already exists, assign gridPos directly
                if (raceStat.ContainsKey(PLID))
                {
                    raceStat[PLID].gridPos = gridPos;
                    // Console.WriteLine($"OnReorder: Assigned gridPos={gridPos} to existing PLID={PLID}");

                    // Add grid timing event immediately (1ms gap per position)
                    if (raceStat[PLID].timingEvents.Count == 0)
                    {
                        long gridETime = (gridPos - 1);
                        raceStat[PLID].timingEvents.Add(new TimingEvent(0, 0, gridETime));
                        // Console.WriteLine($"OnReorder: Added GRID timing event for {raceStat[PLID].userName}: GridPos {gridPos}, ETime={gridETime}ms");
                    }
                }
                else
                {
                    // Store for later assignment when IS_NPL arrives
                    pendingGridPos[PLID] = gridPos;
                    // Console.WriteLine($"OnReorder: Pending gridPos={gridPos} for PLID={PLID}");
                }
            }
        }

        private void OnState(InSimDotNet.InSim insim, IS_STA state)
        {
#if DEBUG
            // Console.WriteLine("STAte");
#endif
            if (flagFirst && ((state.Flags & StateFlags.ISS_MULTI) != 0)) // 512 Cruise something
            {
                flagFirst = false;
                // Get All Connections
                insimConnection.Send(new IS_TINY
                {
                    ReqI = 1,
                    SubT = TinyType.TINY_NCN
                });
                // Get All Players
                insim.Send(new IS_TINY
                {
                    ReqI = 1,
                    SubT = TinyType.TINY_NPL
                });
            }

            sessionInfo.trackNameCode = state.Track;
            sessionInfo.weather = state.Weather;
            sessionInfo.wind = state.Wind;
            sessionInfo.raceLaps = state.RaceLaps;
            sessionInfo.qualMins = state.QualMins;
            sessionInfo.sraceLaps = GetSessionLengthString(sessionInfo.raceLaps, true);

#if DEBUG
            // Console.WriteLine("Current track: " + sessionInfo.trackNameCode);
#endif

            if (state.RaceInProg == 0)
            {
                switch (sessionInfo.session)
                {
                    case SessionInfo.Session.Practice:
                        LFSStats.WriteLine("End of practice by STAte", Verbose.Session);
                        break;
                    case SessionInfo.Session.Qualification:
                        LFSStats.WriteLine("End of qualification by STAte", Verbose.Session);
                        ExportQualStats(false);
                        break;
                    case SessionInfo.Session.Race:
                        LFSStats.WriteLine("End of race by STAte", Verbose.Session);
                        ExportStatistics(false);
                        break;
                }
                LFSStats.ConsoleTitleRestore();
                sessionInfo.ExitSession();
            }
        }

        private void OnMessage(InSimDotNet.InSim insim, IS_MSO msg)
        {
#if DEBUG
            // Console.WriteLine("Message received:" + msg.Msg);
#endif
            // Parse /time response from system messages
            if (msg.UserType == UserType.MSO_SYSTEM && !sessionInfo.gameTimeReceived)
            {
                string text = msg.FullMsg ?? "";
                // Format: "/time set DD Mon HH:MM UTC±X"
                var match = System.Text.RegularExpressions.Regex.Match(text,
                    @"/time set (\d+)\s+(\w+)\s+(\d+):(\d+)\s+UTC([+-]?\d+)");
                if (match.Success)
                {
                    int day = int.Parse(match.Groups[1].Value);
                    string monthStr = match.Groups[2].Value;
                    int hour = int.Parse(match.Groups[3].Value);
                    int minute = int.Parse(match.Groups[4].Value);
                    int utcOff = int.Parse(match.Groups[5].Value);

                    var months = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                    {
                        {"Jan",1},{"Feb",2},{"Mar",3},{"Apr",4},{"May",5},{"Jun",6},
                        {"Jul",7},{"Aug",8},{"Sep",9},{"Oct",10},{"Nov",11},{"Dec",12}
                    };
                    int month;
                    if (months.TryGetValue(monthStr, out month))
                    {
                        int year = DateTime.Now.Year;
                        sessionInfo.gameDate = string.Format("{0:D4}-{1:D2}-{2:D2}", year, month, day);
                        sessionInfo.gameTime = string.Format("{0:D2}:{1:D2}", hour, minute);
                        sessionInfo.utcOffset = utcOff;
                        sessionInfo.gameTimeReceived = true;
                        LFSStats.WriteLine("Game time: " + sessionInfo.gameDate + " " + sessionInfo.gameTime + " UTC" + (utcOff >= 0 ? "+" : "") + utcOff, Verbose.Session);
                    }
                    return; // Don't add to chat
                }
            }

            if (msg.UserType == UserType.MSO_USER) // 1 - normal visible user message
            {
                sessionInfo.chat.Add(new ChatEntry(NickNameByUCID(msg.UCID), msg.Text));
            }
        }

        private void OnInsimMulti(InSimDotNet.InSim insim, IS_ISM multi)
        {
#if DEBUG
            // Console.WriteLine("Insim Multi:" + multi.HName);
            sessionInfo.HName = multi.HName;
#endif
            // Get All Connections
            insimConnection.Send(new IS_TINY
            {
                ReqI = 1,
                SubT = TinyType.TINY_NCN
            });
            // Get All Players
            insim.Send(new IS_TINY
            {
                ReqI = 1,
                SubT = TinyType.TINY_NPL
            });
        }

        private void OnMCI(InSimDotNet.InSim insim, IS_MCI mci)
        {
            for (int i = 0; i < mci.NumC; i++)
            {
                if (raceStat.ContainsKey(mci.Info[i].PLID))
                {
                    raceStat[mci.Info[i].PLID].updateMCI(mci.Info[i].Speed); // Speed
                }
            }
        }

        private void OnFlag(InSimDotNet.InSim insim, IS_FLG flg)
        {
            if (!raceStat.ContainsKey(flg.PLID))
                return;

            if (flg.OffOn && flg.Flag == FlagType.Blue && !raceStat[flg.PLID].inBlue)
            { // Blue flag On
                raceStat[flg.PLID].blueFlags++;
                raceStat[flg.PLID].inBlue = true;
            }

            if (flg.OffOn && flg.Flag == FlagType.Yellow && !raceStat[flg.PLID].inYellow)
            { // Yellow flag On
                raceStat[flg.PLID].yellowFlags++;
                raceStat[flg.PLID].inYellow = true;
            }

            if (!flg.OffOn && flg.Flag == FlagType.Blue)
            { // Blue flag Off
                raceStat[flg.PLID].inBlue = false;
            }

            if (!flg.OffOn && flg.Flag == FlagType.Yellow)
            { // Yellow flag Off
                raceStat[flg.PLID].inYellow = false;
            }
        }

        private void OnPenalty(InSimDotNet.InSim insim, IS_PEN pen)
        {
            if (raceStat.ContainsKey(pen.PLID))
            {
                raceStat[pen.PLID].UpdatePen((int)pen.OldPen, (int)pen.NewPen, (int)pen.Reason);
            }
        }

        private void OnContact(InSimDotNet.InSim insim, IS_CON con)
        {
            // Track contacts for both players involved
            if (raceStat.ContainsKey(con.A.PLID))
            {
                raceStat[con.A.PLID].contacts++;
            }
            if (raceStat.ContainsKey(con.B.PLID))
            {
                raceStat[con.B.PLID].contacts++;
            }

#if DEBUG
            LFSStats.WriteLine($"Contact between PLID {con.A.PLID} and {con.B.PLID}", Verbose.Info);
#endif
        }

        private void OnHotlapValidity(InSimDotNet.InSim insim, IS_HLV hlv)
        {
            // Track off-track incidents (invalidated laps)
            if (raceStat.ContainsKey(hlv.PLID))
            {
                raceStat[hlv.PLID].offTracks++;
#if DEBUG
                LFSStats.WriteLine($"Off-track incident for PLID {hlv.PLID} (reason: {hlv.HLVC})", Verbose.Info);
#endif
            }
        }

        private void OnTOC(InSimDotNet.InSim insim, IS_TOC toc)
        {
            if (!raceStat.ContainsKey(toc.PLID))
                return;

            sessionInfo.isToc = true;

            int OldPLID = UCIDToPLID(toc.NewUCID);
            PLIDToUCID.Remove(OldPLID);
            raceStat.Remove(OldPLID);

            string oldUserName = raceStat[toc.PLID].userName;
            string oldNickName = raceStat[toc.PLID].NickName;

            raceStat[toc.PLID].NickName = connectionPlayers[toc.NewUCID].nickName;
            raceStat[toc.PLID].userName = connectionPlayers[toc.NewUCID].userName;
            raceStat[toc.PLID].allPlayers[connectionPlayers[toc.NewUCID].userName] = new PlayerIdentity(
                connectionPlayers[toc.NewUCID].userName,
                connectionPlayers[toc.NewUCID].nickName
            );

            string newUserName = raceStat[toc.PLID].userName;
            string newNickName = raceStat[toc.PLID].NickName;

            raceStat[toc.PLID].updateTOC(oldNickName, oldUserName, newNickName, newUserName);
            PLIDToUCID[toc.PLID] = toc.NewUCID;

            LFSStats.WriteLine("TOC", oldUserName + " --> " + newUserName, Verbose.Info);
        }

        private void OnSmall(InSimDotNet.InSim insim, IS_SMALL small)
        {
            //#if DEBUG
            //Console.WriteLine("SMALL");
            //#endif

            // Causes a big issue since there exists an invalid SMALL with size 4 that contains [4,4,4,4]
            // SMALL packet normally indicates end of practice, vote actions, or session transitions.
            // When alone on a localhost server, practice end shows as "SMALL vote action".
            // When going spectate, practice ends but SMALL.VTA is not reported.
            // Also when ending Q or R, SMALL_VTA is sent.
            // Ending Q either by starting R or going to host screen from Q or R.

            if (small.SubT == SmallType.SMALL_VTA)
            {
                LFSStats.ConsoleTitleRestore();
            }

            if (small.SubT == SmallType.SMALL_ALC)
            {
                sessionInfo.allowedCarFlags = (long)small.UVal;
                LFSStats.WriteLine("Allowed cars bitmask: 0x" + small.UVal.ToString("X"), Verbose.Session);
            }
        }

        private void OnModsAllowed(InSimDotNet.InSim insim, IS_MAL mal)
        {
            sessionInfo.allowedMods.Clear();
            foreach (var skinId in mal.SkinIDs)
            {
                if (!string.IsNullOrEmpty(skinId))
                    sessionInfo.allowedMods.Add(skinId);
            }
            LFSStats.WriteLine("Allowed mods: " + string.Join(", ", sessionInfo.allowedMods), Verbose.Session);
        }

        private void EventReceived(InSimDotNet.InSim insim, IS_TINY tiny)
        {
            if (tiny.SubT == TinyType.TINY_CLR)
            {
#if DEBUG
                // Console.WriteLine("CLear Race - all players removed from race in one go");
#endif
                //raceStat.Clear();	// why invalidate the so far collected statistics though?
            }
            if (tiny.SubT == TinyType.TINY_REN)
            {
                sessionInfo.ExitSession();
                //TODO ASK save stats
#if DEBUG
                // Console.WriteLine("Race ENd (return to entry screen)");
#endif
            }
        }

        private void InSimVer(InSimDotNet.InSim insim, IS_VER ver)
        {
            string info = "Product: " + ver.Product + " Version: " + ver.Version + " InSim Version: " + ver.InSimVer;
            LFSStats.WriteLine(info);

            //byte[] intervalReq = InSim.Encoder.NLI(refreshInterval);
            insimConnection.Send(new IS_SMALL { ReqI = 0, SubT = SmallType.SMALL_NLI, UVal = RefreshInterval });

            LFSStats.WriteLine(LFSStats.Title + " is running...\n");
            // Request for STA info packet to be sent.
            insimConnection.Send(new IS_TINY { SubT = TinyType.TINY_SST, ReqI = 1 });
        }

        private void OnSessionStart(InSimDotNet.InSim insim, IS_RST rst)
        {
            // If already in session then generate statistics and start a new session
            switch (sessionInfo.session)
            {
                case SessionInfo.Session.Practice:      // End of Practice
                    LFSStats.WriteLine("End of practice by Race Start", Verbose.Session);
                    break;

                case SessionInfo.Session.Qualification: // End of Qualification
                    LFSStats.WriteLine("End of qualification by Race Start", Verbose.Session);
                    ExportStatistics(true);
                    break;

                case SessionInfo.Session.Race:          // End of Race
                    LFSStats.WriteLine("End of race by Race Start", Verbose.Session);
                    ExportStatistics(true);
                    break;
            }

            LFSStats.ConsoleTitleRestore();
            sessionInfo.ExitSession();
            resetStatistics(); // resets the statistics before collecting new

            var fullTrackName = InSimDotNet.Helpers.TrackHelper.GetFullTrackName(rst.Track);

            // Capture session start time and RST values (reliable, unlike IS_STA which updates post-session)
            sessionInfo.sessionStartTime = DateTime.Now;
            sessionInfo.rstQualMins = rst.QualMins;
            sessionInfo.rstRaceLaps = rst.RaceLaps;
            sessionInfo.hostFlags = (int)rst.Flags;

            // Enter new session
            if (rst.RaceLaps > 0)
            {
                sessionInfo.EnterRace();
                LFSStats.ConsoleTitleShow(fullTrackName + ": Race Lap 1");
                LFSStats.WriteLine(
                    "\nRace start: " +
                    GetSessionLengthString(rst.RaceLaps, true) + " " +
                    GetSessionLengthPostFix(rst.RaceLaps, true) +
                    " of " + rst.Track,
                    Verbose.Session
                );

                // Grid timing events will be added in OnReorder after receiving IS_REO
            }
            else if (rst.QualMins > 0)
            {
                sessionInfo.EnterQualification();
                LFSStats.ConsoleTitleShow(fullTrackName + ": Qual Lap 1");
                LFSStats.WriteLine(
                    "\nQual start: " +
                    GetSessionLengthString(rst.QualMins, false) + " " +
                    GetSessionLengthPostFix(rst.QualMins, false) +
                    " of " + rst.Track,
                    Verbose.Session
                );
            }
            else
            {
                sessionInfo.EnterPractice();
                LFSStats.ConsoleTitleShow(fullTrackName + ": Prac Lap 1");
                LFSStats.WriteLine(
                    "\nPrac start: " + rst.Track,
                    Verbose.Session
                );
            }

            byte reqI = 1;
            // Request all connections
            insimConnection.Send(new IS_TINY
            {
                ReqI = reqI,
                SubT = TinyType.TINY_NCN
            });

            // Request all players
            insim.Send(new IS_TINY
            {
                ReqI = reqI,
                SubT = TinyType.TINY_NPL
            });

            // Request allowed cars (standard)
            insim.Send(new IS_TINY
            {
                ReqI = reqI,
                SubT = TinyType.TINY_ALC
            });

            // Request allowed mods
            insim.Send(new IS_TINY
            {
                ReqI = reqI,
                SubT = TinyType.TINY_MAL
            });

            // TODO: Request game time via /time command (disabled - needs testing)
            // insim.Send(new IS_MST { Msg = "/time" });

            // Request REO (order info)
            insim.Send(new IS_TINY
            {
                ReqI = reqI,
                SubT = TinyType.TINY_REO
            });
        }
        private void NewPlayer(InSimDotNet.InSim insim, IS_NPL newPlayer)
        {
            int LastPosGrid;

            string newPlayerUserName = connectionPlayers[newPlayer.UCID].userName;

            int removePLID = PLIDbyUName(newPlayerUserName);
            //                            Console.WriteLine("Nick:" + newPlayer.NickName);
            //                            Console.WriteLine("Old PLID:" + removePLID + " New : " + newPlayer.PLID);
            LastPosGrid = 0;

            // Check if player is returning from spectator (data in disconnectedDrivers)
            SessionStats restoredDriver = null;
            for (int i = disconnectedDrivers.Count - 1; i >= 0; i--)
            {
                if (disconnectedDrivers[i].userName == newPlayerUserName)
                {
                    restoredDriver = disconnectedDrivers[i];
                    disconnectedDrivers.RemoveAt(i);
                    LFSStats.WriteLine($"Restoring stats for {newPlayerUserName} (was disconnected)", Verbose.Info);
                    break;
                }
            }

            switch (sessionInfo.session)
            {
                case SessionInfo.Session.Practice:      // TODO new player in practice
                    break;
                case SessionInfo.Session.Qualification:
                    // If player is returning from spectator, use restored data
                    if (restoredDriver != null)
                    {
                        // Remove old entry if exists
                        if (removePLID > 0)
                        {
                            raceStat.Remove(removePLID);
                            PLIDToUCID.Remove(removePLID);
                        }
                    }
                    else if (removePLID > 0)
                    {
                        if (newPlayer.PLID != removePLID)
                        {
                            raceStat[newPlayer.PLID] = raceStat[removePLID];
                            raceStat[newPlayer.PLID].PLID = newPlayer.PLID;
                            raceStat.Remove(removePLID);
                            PLIDToUCID.Remove(removePLID);
                        }
                    }
                    break;
                case SessionInfo.Session.Race:
                    // If player is returning from spectator, use restored data
                    if (restoredDriver != null)
                    {
                        // Remove old entry if exists
                        if (removePLID > 0)
                        {
                            raceStat.Remove(removePLID);
                            PLIDToUCID.Remove(removePLID);
                        }
                    }
                    else if (removePLID > 0)
                    {
                        if (raceStat[removePLID].finished == false)
                        {
                            if (preserveLapsOnPit && raceStat[removePLID].lap.Count > 0)
                            {
                                // Preserve stats: move to new PLID
                                if (newPlayer.PLID != removePLID)
                                {
                                    raceStat[newPlayer.PLID] = raceStat[removePLID];
                                    raceStat[newPlayer.PLID].PLID = newPlayer.PLID;
                                    raceStat.Remove(removePLID);
                                    PLIDToUCID.Remove(removePLID);
                                }
                                LFSStats.WriteLine($"Preserved {raceStat[newPlayer.PLID].lap.Count} laps for {newPlayerUserName} (ESC pit rejoin)", Verbose.Info);
                            }
                            else
                            {
                                // Reset stats (original behavior)
                                LastPosGrid = raceStat[removePLID].gridPos;
                                raceStat.Remove(removePLID);
                                PLIDToUCID.Remove(removePLID);
                            }
                        }
                        else
                        {
                            if (newPlayer.PLID != removePLID)
                            {
                                raceStat[newPlayer.PLID] = raceStat[removePLID];
                                raceStat[newPlayer.PLID].PLID = newPlayer.PLID;
                                raceStat.Remove(removePLID);
                                PLIDToUCID.Remove(removePLID);
                            }
                        }
                    }
                    break;
            }

            PLIDToUCID[newPlayer.PLID] = newPlayer.UCID;
            string nplUserName = UserNameByUCID(newPlayer.UCID);

            if (!raceStat.ContainsKey(newPlayer.PLID))
            {
                // If player is returning from spectator, restore their previous data
                if (restoredDriver != null)
                {
                    raceStat[newPlayer.PLID] = restoredDriver;
                    raceStat[newPlayer.PLID].PLID = newPlayer.PLID;
                    raceStat[newPlayer.PLID].UCID = newPlayer.UCID;
                    raceStat[newPlayer.PLID].disconnected = false;
                }
                else
                {
                    raceStat[newPlayer.PLID] = new SessionStats(newPlayer.UCID, newPlayer.PLID);
                    if (LastPosGrid != 0)
                        raceStat[newPlayer.PLID].gridPos = LastPosGrid;
                }
            }
            raceStat[newPlayer.PLID].UCID = newPlayer.UCID;
            raceStat[newPlayer.PLID].userName = nplUserName;
            raceStat[newPlayer.PLID].NickName = newPlayer.PName;
            raceStat[newPlayer.PLID].allPlayers[nplUserName] = new PlayerIdentity(nplUserName, newPlayer.PName);
            raceStat[newPlayer.PLID].carName = newPlayer.CName;
            raceStat[newPlayer.PLID].Plate = newPlayer.Plate;
            raceStat[newPlayer.PLID].flags = newPlayer.Flags;
            if (sessionInfo.InQualification())
                raceStat[newPlayer.PLID].finPLID = newPlayer.PLID;

            // Assign pending gridPos if available
            if (pendingGridPos.ContainsKey(newPlayer.PLID))
            {
                int gridPos = pendingGridPos[newPlayer.PLID];
                raceStat[newPlayer.PLID].gridPos = gridPos;
                // Console.WriteLine($"NewPlayer: Assigned pending gridPos={gridPos} to {nplUserName}");

                // Add grid timing event immediately (1ms gap per position)
                if (raceStat[newPlayer.PLID].timingEvents.Count == 0)
                {
                    long gridETime = (gridPos - 1);
                    raceStat[newPlayer.PLID].timingEvents.Add(new TimingEvent(0, 0, gridETime));
                    // Console.WriteLine($"NewPlayer: Added GRID timing event for {nplUserName}: GridPos {gridPos}, ETime={gridETime}ms");
                }

                pendingGridPos.Remove(newPlayer.PLID);
            }
        }

        private void PrintMenu()
        {
            Console.WriteLine();
            Console.WriteLine("=== LFSStats Menu ===");
            Console.WriteLine($"  L - Toggle preserve laps on ESC pit [{(preserveLapsOnPit ? "ON" : "OFF")}]");
            Console.WriteLine("  F - Fast forward to end of replay (Full Physics)");
            Console.WriteLine("  H - Show this menu");
            Console.WriteLine("  Q - Quit");
            Console.WriteLine("=====================");
        }

        private void RequestReplayEnd()
        {
            LFSStats.WriteLine("Menu", "Requesting replay info...", Verbose.Program);
            insimConnection.Send(new IS_TINY { ReqI = RIP_REQID, SubT = TinyType.TINY_RIP });
        }

        private void OnReplayInfo(InSimDotNet.InSim insim, IS_RIP rip)
        {
            if (rip.ReqI == RIP_REQID)
            {
                if (rip.Error > ReplayError.RIP_ALREADY)
                {
                    LFSStats.WriteLine("Menu", "Replay error: " + rip.Error, Verbose.Program);
                    return;
                }

                TimeSpan totalTime = rip.TTime;
                LFSStats.WriteLine("Menu", $"Replay length: {totalTime.TotalMilliseconds}ms ({totalTime.TotalSeconds}s) - Seeking to end with Full Physics...", Verbose.Program);

                insimConnection.Send(new IS_RIP
                {
                    ReqI = 1,
                    CTime = totalTime,
                    Options = ReplayOptions.RIPOPT_FULL_PHYS
                });
            }
        }

        private void NewConnection(InSimDotNet.InSim insim, IS_NCN newConnection)
        {
#if DEBUG
            // Console.WriteLine(string.Format("New ConN Username: {0} Nickname: {1} ConnectionNumber: {2}", newConnection.UName, newConnection.PName, newConnection.UCID));
#endif
            // On reconnect player, RAZ infos et restart
            connectionPlayers[newConnection.UCID] = new PlayerIdentity(newConnection.UName, newConnection.PName);
            if (newConnection.UCID == 0 && string.IsNullOrWhiteSpace(sessionInfo.HName))
                sessionInfo.HName = newConnection.PName;
        }

        void Loop(InSimDotNet.InSim insimConnection)
        {
            //bool inSession = false;
            //bool inRace = false;
            //bool inQual = false;

            #region Loop
            while (!LFSStats.HasShutdownStarted)
            {
                Thread.Sleep(1000);
            }

            #endregion
        }

        /// <summary>
        /// Converts LFS insim race laps to real laps and hours
        /// </summary>
        /// <param name="duration">The duration.</param>
        /// <param name="race">if set to <c>true</c> [race].</param>
        /// <returns>Number of laps or hours</returns>
        private string GetSessionLengthString(int duration, bool race)
        {
            if (race)
            {
                if (duration == 0)          // Practice
                    return "Practice";
                else if (duration < 100)    // 1-99 (laps)
                    return duration.ToString();
                else if (duration < 191)    // 100-1000 (laps)
                    return ((duration - 100) * 10 + 100).ToString();
                else if (duration < 239)    // 1-48 (hours)
                    return (duration - 190).ToString();
                else return duration.ToString();
            }
            else return duration.ToString();
        }

        /// <summary>
        /// Gets the session length post fix: lap, laps, hour, hours
        /// </summary>
        /// <param name="duration">The duration.</param>
        /// <param name="race">if set to <c>true</c> [race].</param>
        /// <returns>The post fix</returns>
        private string GetSessionLengthPostFix(int duration, bool race)
        {
            if (race)
            {
                if (duration == 0)          // Practice
                    return "";
                else if (duration == 1)     // 1 lap
                    return "lap";
                else if (duration < 191)    // 2-1000 laps
                    return "laps";
                else if (duration == 191)   // 1 hour
                    return "hour";
                else                            // 2-48 hours
                    return "hours";
            }
            else if (duration == 1)
                return "min";
            else
                return "mins";
        }

        /// <summary>
        /// Exports qualification statistics
        /// </summary>
        /// <param name="fileName">Name of statistics</param>
        private void ExportQualStats(bool isRST)
        {
            string fileName;
            if (!ExportOrNot(isRST, out fileName)) return;
            Directory.CreateDirectory(config.QualDir);  // @ is used to interpret string literally, since it may have forward or backslash
#if DEBUG
            Stopwatch sw = new Stopwatch();
#endif
            TaskFactory tf = new TaskFactory();
            Task exportTask;
            // Combine active drivers and disconnected drivers for complete results
            List<SessionStats> tmpList = new List<SessionStats>(raceStat.Values);
            tmpList.AddRange(disconnectedDrivers);
            SessionInfo tmpSessionInfo = new SessionInfo(sessionInfo);
#if DEBUG
            sw.Start();
#endif
            exportTask = tf.StartNew(obj => ExportStatsTask(tmpList, tmpSessionInfo, fileName), "HtmlResult");
            exportTask.ContinueWith(task => TaskExceptionHandler(task), TaskContinuationOptions.OnlyOnFaulted);
#if DEBUG
            sw.Stop();
            //Task.WaitAll(exportTask);
            Console.WriteLine("Qual stats export delay (" + sw.ElapsedMilliseconds + "ms" + ")");
#endif

            //ExportStats.qualhtmlResult(raceStat, fileName, qualDir, sessionInfo, maxTimeQualIgnore);
            //LFSStats.WriteLineFromAsync("Qualification Stats exported", Verbose.Program);
        }

        /// <summary>
        /// Exports race statistics
        /// </summary>
        /// <param name="isRST">if set to <c>true</c> [is RaceSTart], <c>false</c> [is STAte].</param>
        private void ExportStatistics(bool isRST)
        {
            string fileName;
            if (!ExportOrNot(isRST, out fileName)) return;  // decides whether to export and generates a filename or asks user for one
#if DEBUG
            Stopwatch sw = new Stopwatch();
#endif
            TaskFactory tf = new TaskFactory();
            Task exportTask;
            // Combine active drivers and disconnected drivers for complete results
            List<SessionStats> tmpList = new List<SessionStats>(raceStat.Values);
            tmpList.AddRange(disconnectedDrivers);
            SessionInfo tmpSessionInfo = new SessionInfo(sessionInfo);
#if DEBUG
            sw.Start();
#endif
            exportTask = tf.StartNew(obj => ExportStatsTask(tmpList, tmpSessionInfo, fileName), "HtmlResult");
            exportTask.ContinueWith(task => TaskExceptionHandler(task), TaskContinuationOptions.OnlyOnFaulted);
#if DEBUG
            sw.Stop();
            //Task.WaitAll(exportTask);
            Console.WriteLine("Stats export delay (" + sw.ElapsedMilliseconds + "ms" + ")");
#endif
        }

        /// <summary>
        /// Determines whether to export and sets output file name
        /// </summary>
        /// <param name="isRST">if set to <c>true</c> [is RaceSTart], <c>false</c> [is STAte].</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns><c>true</c> when exporting should be done, <c>false</c> otherwise</returns>
        private bool ExportOrNot(bool isRST, out string fileName)
        {
            Configuration.ExportOptions exportOptions = (isRST) ? config.ExportOnRaceSTart : config.ExportOnSTAte;
            bool askForFileName = (isRST) ? config.AskForFileNameOnRST : config.AskForFileNameOnSTA;

            switch (exportOptions)
            {
                case Configuration.ExportOptions.no:
                default:
                    fileName = "";
                    return false;
                case Configuration.ExportOptions.yes:
                    if (askForFileName)
                    {
                        LFSStats.WriteLine("Enter name of stats: ");
                        fileName = Console.ReadLine();
                    }
                    else
                        fileName = getFormattedTime() + LFSStats.filePathSpaceChar + sessionInfo.SessionName().Substring(0, 4)
                            + LFSStats.filePathSpaceChar + sessionInfo.SessionLengthAndLocation(LFSStats.filePathSpaceChar);
                    break;
                case Configuration.ExportOptions.ask:
                    LFSStats.WriteLine("Export Stats? [yes/no]: ");
                    string answer = Console.ReadLine();
                    if (answer.Equals("yes", StringComparison.OrdinalIgnoreCase)
                        || answer.Equals("y", StringComparison.OrdinalIgnoreCase))
                        goto case Configuration.ExportOptions.yes;
                    else
                        goto default;
            }
            return true;
        }
        private void ExportStatsTask(List<SessionStats> sessionStatsList, SessionInfo sessionInfo, string fileName)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            ExportStats.JsonResult(sessionStatsList, fileName, sessionInfo, config, connectionPlayers);

            sw.Stop();
            LFSStats.WriteLineFromAsync("Race Stats exported (" + sw.ElapsedMilliseconds + "ms" + ")", Verbose.Program);
        }

        /// <summary>
        /// Task exception handler.
        /// </summary>
        /// <param name="task">The task.</param>
        private void TaskExceptionHandler(Task task)
        {
            Exception ex = task.Exception;
            LFSStats.ErrorWriteException("Unhandled export task exception:", ex);
        }


        /// <summary>
        /// Detects overtakes at the current timing point by comparing positions
        /// </summary>
        private void DetectOvertake(int PLID, int lap, long eTime)
        {
            if (!raceStat.ContainsKey(PLID))
                return;

            var driver = raceStat[PLID];
            int timingIndex = driver.timingEvents.Count - 1;

            // Need at least 2 timing points to detect position changes
            if (timingIndex < 1)
                return;

            // Calculate current and previous positions
            int currentPos = CalculatePositionAtTiming(PLID, timingIndex);
            int prevPos = CalculatePositionAtTiming(PLID, timingIndex - 1);

            // No position change
            if (currentPos == prevPos)
                return;

            if (currentPos < prevPos)
            {
                // Driver improved position

                for (int targetPos = currentPos; targetPos < prevPos; targetPos++)
                {
                    foreach (var other in raceStat.Values)
                    {
                        if (other.PLID == PLID || other.timingEvents.Count <= timingIndex - 1)
                            continue;

                        int otherPrevPos = CalculatePositionAtTiming(other.PLID, timingIndex - 1);
                        int otherCurrentPos = CalculatePositionAtTiming(other.PLID, timingIndex);

                        if (otherPrevPos == targetPos && otherCurrentPos > currentPos)
                        {
                            long driverTime = driver.timingEvents[timingIndex - 1].ETime;
                            long otherTime = other.timingEvents[timingIndex - 1].ETime;
                            long gap = Math.Abs(driverTime - otherTime);

                            // Check if already registered (avoid duplicates)
                            bool alreadyRegistered = sessionInfo.overtakes.Any(ot =>
                                ot.overtaker == driver.userName &&
                                ot.overtaken == other.userName &&
                                ot.lap == lap &&
                                ot.timingPoint == timingIndex);

                            if (!alreadyRegistered && gap <= 2000)
                            {
                                sessionInfo.overtakes.Add(new OvertakeEvent(driver.userName, other.userName, lap, timingIndex, gap, eTime));
                            }
                            else if (gap > 2000)
                            {
                            }
                            break;
                        }
                    }
                }
            }
            else
            {
                // Driver lost positions

                for (int targetPos = prevPos; targetPos < currentPos; targetPos++)
                {
                    foreach (var other in raceStat.Values)
                    {
                        if (other.PLID == PLID || other.timingEvents.Count <= timingIndex - 1)
                            continue;

                        int otherPrevPos = CalculatePositionAtTiming(other.PLID, timingIndex - 1);
                        int otherCurrentPos = CalculatePositionAtTiming(other.PLID, timingIndex);

                        if (otherPrevPos > prevPos && otherCurrentPos == targetPos)
                        {
                            long driverTime = driver.timingEvents[timingIndex - 1].ETime;
                            long otherTime = other.timingEvents[timingIndex - 1].ETime;
                            long gap = Math.Abs(driverTime - otherTime);

                            // Check if already registered (avoid duplicates)
                            bool alreadyRegistered = sessionInfo.overtakes.Any(ot =>
                                ot.overtaker == other.userName &&
                                ot.overtaken == driver.userName &&
                                ot.lap == lap &&
                                ot.timingPoint == timingIndex);

                            if (!alreadyRegistered && gap <= 2000)
                            {
                                sessionInfo.overtakes.Add(new OvertakeEvent(other.userName, driver.userName, lap, timingIndex, gap, eTime));
                            }
                            else if (gap > 2000)
                            {
                            }
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the position of a driver at a specific timing point based on ETime
        /// </summary>
        private int CalculatePositionAtTiming(int PLID, int timingIndex)
        {
            if (!raceStat.ContainsKey(PLID))
                return 999;

            var driver = raceStat[PLID];

            if (timingIndex >= driver.timingEvents.Count || timingIndex < 0)
                return 999;

            long driverTime = driver.timingEvents[timingIndex].ETime;
            int position = 1;

            // Count how many drivers have better (lower) ETime at this timing point
            foreach (var other in raceStat.Values)
            {
                if (other.PLID == PLID)
                    continue;

                if (timingIndex >= other.timingEvents.Count)
                    continue;

                long otherTime = other.timingEvents[timingIndex].ETime;

                if (otherTime < driverTime)
                    position++;
            }

            return position;
        }
    }
}
