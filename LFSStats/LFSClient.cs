using InSimDotNet;
using InSimDotNet.Helpers;
using InSimDotNet.Packets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LFSStatistics
{
	public class LFSClient
	{
        //bool wrLoaded = false;	// unused
        InSimDotNet.InSim insimConnection;
		Dictionary<int, SessionStats> raceStat;
		Dictionary<int, int> PLIDToUCID = new Dictionary<int, int>();
		Dictionary<int, UN> UCIDToUN = new Dictionary<int, UN>();
		// used to check for session state, thus even before a reset is performed on new session start RST
		SessionInfo sessionInfo = new SessionInfo();

        int RefreshInterval = 100;
        bool flagFirst = true; // used to get all connections on first STA packet
        bool inSession = false;
        bool inRace = false;
        bool inQual = false;

        ConsoleCtrl cc;
		Configuration config;

		/// <summary>
		/// Resets objects used to collect statistics
		/// </summary>
		private void resetStatistics()
		{
			// created new since old ones are used during parallel export
			raceStat = new Dictionary<int, SessionStats>();	//raceStat.Clear();
			PLIDToUCID.Clear();
			UCIDToUN.Clear();
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
            if (!UCIDToUN.ContainsKey(UCID)) return "???";
			return (UCIDToUN[UCID]).nickName;
		}
		public string UserNameByUCID(int UCID)
		{
			return (UCIDToUN[UCID]).userName;
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
				insimConnection.Send(new IS_TINY
                {
                    SubT = TinyType.TINY_CLOSE
                });
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
            insimConnection.Bind<IS_PEN>(OnPenalty);
            insimConnection.Bind<IS_TOC>(OnTOC);
            insimConnection.Bind<IS_SMALL>(OnSmall);


            var flags = InSimFlags.ISF_MCI | InSimFlags.ISF_NLP;
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

			insimConnection.Initialize(insimSettings);

            LoopAsync(insimConnection);
		}

        private void OnPlayerPits(InSimDotNet.InSim insim, IS_PLP packet)
        {
#if DEBUG
            Console.WriteLine("Player pits (goes to setup - stays in player list)");
#endif
        }

        private void OnPlayerLeave(InSimDotNet.InSim insim, IS_PLL pll)
        {
#if DEBUG
            Console.WriteLine("Player leaves race (spectate - leaves player list, all are shunted down)");
#endif
        }

        private void OnPlayerRename(InSimDotNet.InSim insim, IS_CPR cpr)
        {
            if (UCIDToUN.ContainsKey(cpr.UCID))
            {
                var oldNick = UCIDToUN[cpr.UCID].nickName;
                UCIDToUN[cpr.UCID].nickName = cpr.PName;

                int plid = UCIDToPLID(cpr.UCID);
                if (plid != -1)
                {
                    raceStat[plid].NickName = cpr.PName;
                }

#if DEBUG
                Console.WriteLine($"Conn Player {UCIDToUN[cpr.UCID].userName} Rename from {oldNick} to {cpr.PName}, id: {cpr.UCID}");
#endif
            }
        }

        private void OnPit(InSimDotNet.InSim insim, IS_PIT pitDec)
        {
            raceStat[pitDec.PLID].updatePIT(pitDec);
            LFSStats.WriteLine("Pit", UCIDToUN[PLIDToUCID[pitDec.PLID]].nickName, Verbose.Info);
        }

        private void OnPitFinished(InSimDotNet.InSim insim, IS_PSF pitFin)
        {
            raceStat[pitFin.PLID].updatePSF(pitFin.PLID, pitFin.STime);
        }

        private void OnLap(InSimDotNet.InSim insim, IS_LAP lapDec)
        {
            LFSStats.WriteLine("Lap " + lapDec.LapsDone, 0, raceStat[lapDec.PLID].userName, Verbose.Lap);

            string sessionName = sessionInfo.session.ToString().Substring(0, 4);

            if (lapDec.LapsDone > sessionInfo.currentLap)
            {
                sessionInfo.currentLap = lapDec.LapsDone;
                LFSStats.ConsoleTitleShow(sessionInfo.trackNameCode + ": " + sessionName + " Lap " + (lapDec.LapsDone + 1).ToString());
            }
            if (sessionInfo.InQualification())
                return;

            raceStat[lapDec.PLID].UpdateLap(lapDec.LTime.TotalMillisecondsLong(), raceStat[lapDec.PLID].numStop, lapDec.LapsDone, sessionInfo.maxSplit);
        }

        private void OnSplit(InSimDotNet.InSim insim, IS_SPX splitdec)
        {
            if (splitdec.STime.TotalMillisecondsLong() == timeConv.HMSToLong(60, 0, 0))
                return;

            raceStat[splitdec.PLID].UpdateSplit(splitdec.Split, splitdec.STime.TotalMillisecondsLong());
            LFSStats.WriteLine("SP " + splitdec.Split, splitdec.STime.TotalMillisecondsLong(), raceStat[splitdec.PLID].userName, Verbose.Split);

            if (sessionInfo.maxSplit < splitdec.Split)
                sessionInfo.maxSplit = splitdec.Split;
        }

        private void OnResult(InSimDotNet.InSim insim, IS_RES result)
        {
#if DEBUG
            Console.WriteLine("RESult (qualify or finish)");
#endif
            if (sessionInfo.InRace())
                LFSStats.WriteLine("Result", result.ResultNum + 1, ExportStats.lfsStripColor(result.PName), Verbose.Session);

            int lplid = result.PLID;
            if (result.PLID == 0)
                lplid = PLIDbyUName(result.UName);

            lplid = PLIDbyFinPLID(lplid);

            if (lplid != -1 && sessionInfo.InRace())
                raceStat[lplid].UpdateResult(result.TTime.TotalMillisecondsLong(), result.ResultNum, result.CName, result.Confirm, result.NumStops);

            if (lplid != -1 && sessionInfo.InQualification())
                raceStat[lplid].UpdateQualResult(result.TTime.TotalMillisecondsLong(), result.BTime.TotalMillisecondsLong(), result.NumStops, result.Confirm, result.LapsDone, result.ResultNum, result.NumRes, sessionInfo.maxSplit);
        }

        private void OnFinish(InSimDotNet.InSim insim, IS_FIN fin)
        {
#if DEBUG
            Console.WriteLine("FINish (qualify or finish)");
#endif
            raceStat[fin.PLID].finished = true;
            raceStat[fin.PLID].finPLID = fin.PLID;
        }

        private void OnReorder(InSimDotNet.InSim insim, IS_REO pacreo)
        {
#if DEBUG
            Console.WriteLine("REOrder (when race restarts after qualifying)");
#endif

            if (pacreo.ReqI != 0) // Ignore and get only if requested
            {
                for (int i = 0; i < pacreo.NumP; i++)
                {
                    int PLID = pacreo.PLID[i];
                    if (!raceStat.ContainsKey(PLID))
                    {
                        raceStat[PLID] = new SessionStats(0, PLID);
                    }
                    raceStat[PLID].gridPos = i + 1;
                }
            }
        }

        private void OnState(InSimDotNet.InSim insim, IS_STA state)
        {
#if DEBUG
            Console.WriteLine("STAte");
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
            Console.WriteLine("Current track: " + sessionInfo.trackNameCode);
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
            Console.WriteLine("Message received:" + msg.Msg);
#endif
            if (msg.UserType == UserType.MSO_USER) // 1 - normal visible user message
            {
                sessionInfo.chat.Add(NickNameByUCID(msg.UCID), msg.Msg);
            }
        }

        private void OnInsimMulti(InSimDotNet.InSim insim, IS_ISM multi)
        {
#if DEBUG
            Console.WriteLine("Insim Multi:" + multi.HName);
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
            raceStat[pen.PLID].UpdatePen((int)pen.OldPen, (int)pen.NewPen, (int)pen.Reason);
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

            raceStat[toc.PLID].NickName = UCIDToUN[toc.NewUCID].nickName;
            raceStat[toc.PLID].userName = UCIDToUN[toc.NewUCID].userName;
            raceStat[toc.PLID].allUN[UCIDToUN[toc.NewUCID].userName] = new UN(
                UCIDToUN[toc.NewUCID].userName,
                UCIDToUN[toc.NewUCID].nickName
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
                inSession = false; // closes data collection and prevents crashes from insim sending stuff before session start
            }
        }

        private void EventReceived(InSimDotNet.InSim insim, IS_TINY tiny)
        {
            if (tiny.SubT == TinyType.TINY_CLR)
            {
#if DEBUG
                Console.WriteLine("CLear Race - all players removed from race in one go");
#endif
                //raceStat.Clear();	// why invalidate the so far collected statistics though?
            }
            if (tiny.SubT == TinyType.TINY_REN)
            {
                sessionInfo.ExitSession();
                //TODO ASK save stats
#if DEBUG
                Console.WriteLine("Race ENd (return to entry screen)");
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

            

            int removePLID = PLIDbyUName(UCIDToUN[newPlayer.UCID].userName);
            //                            Console.WriteLine("Nick:" + newPlayer.NickName);
            //                            Console.WriteLine("Old PLID:" + removePLID + " New : " + newPlayer.PLID);
            LastPosGrid = 0;

            switch (sessionInfo.session)
            {
                case SessionInfo.Session.Practice:      // TODO new player in practice
                    break;
                case SessionInfo.Session.Qualification:
                    if (removePLID > 0)
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
                    if (removePLID > 0)
                    {
                        if (raceStat[removePLID].finished == false)
                        {
                            LastPosGrid = raceStat[removePLID].gridPos;
                            raceStat.Remove(removePLID);
                            PLIDToUCID.Remove(removePLID);
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
                raceStat[newPlayer.PLID] = new SessionStats(newPlayer.UCID, newPlayer.PLID);
                if (LastPosGrid != 0)
                    raceStat[newPlayer.PLID].gridPos = LastPosGrid;
            }
            raceStat[newPlayer.PLID].UCID = newPlayer.UCID;
            raceStat[newPlayer.PLID].userName = nplUserName;
            raceStat[newPlayer.PLID].NickName = newPlayer.PName;
            raceStat[newPlayer.PLID].allUN[nplUserName] = new UN(nplUserName, newPlayer.PName);
            raceStat[newPlayer.PLID].carName = newPlayer.CName;
            raceStat[newPlayer.PLID].Plate = newPlayer.Plate;
            raceStat[newPlayer.PLID].Flags = newPlayer.Flags;
            if (sessionInfo.InQualification())
                raceStat[newPlayer.PLID].finPLID = newPlayer.PLID;
        }

        private void NewConnection(InSimDotNet.InSim insim, IS_NCN newConnection)
        {
#if DEBUG
            Console.WriteLine(string.Format("New ConN Username: {0} Nickname: {1} ConnectionNumber: {2}", newConnection.UName, newConnection.PName, newConnection.UCID));
#endif
            // On reconnect player, RAZ infos et restart
            UCIDToUN[newConnection.UCID] = new UN(newConnection.UName, newConnection.PName);
            if (newConnection.UCID == 0 && string.IsNullOrWhiteSpace(sessionInfo.HName))
                sessionInfo.HName = newConnection.PName;
        }

        void LoopAsync(InSimDotNet.InSim insimConnection)
		{
			//bool inSession = false;
			//bool inRace = false;
			//bool inQual = false;

			#region Loop
			while (!LFSStats.HasShutdownStarted)
			{
                Thread.Sleep(1000);
                //				byte[] recvPacket = insimConnection.Receive();

                //				if (recvPacket.Length > 3)
                //				{
                //					string packetHead = insimConnection.packetHead(recvPacket);
                //					uint verifyID = insimConnection.verifyID(recvPacket);

                //					#region RST
                //					if (packetHead == "RST")
                //					{
                //						InSim.Decoder.RST rst = new InSim.Decoder.RST(recvPacket);

                //						// If already in session then generate statistics and start a new session
                //						switch (sessionInfo.session)
                //						{
                //							case SessionInfo.Session.Practice:		// End of Practice
                //								LFSStats.WriteLine("End of practice by Race STart", Verbose.Session);
                //								break;
                //							case SessionInfo.Session.Qualification:	// End of Qualification
                //								LFSStats.WriteLine("End of qualification by Race STart", Verbose.Session);
                //								ExportStatistics(true);
                //								break;
                //							case SessionInfo.Session.Race:			// End of Race
                //								LFSStats.WriteLine("End of race by Race STart", Verbose.Session);
                //								ExportStatistics(true);
                //								break;
                //						}

                //						LFSStats.ConsoleTitleRestore();
                //						sessionInfo.ExitSession();
                //						resetStatistics();	// resets the statistics before collecting new

                //						// Enter new session
                //						if (rst.RaceLaps > 0)
                //						{
                //							sessionInfo.EnterRace();
                //							LFSStats.ConsoleTitleShow(rst.TrackNameShort + ": Race Lap 1");
                //							LFSStats.WriteLine("\nRace start: " + GetSessionLengthString(rst.RaceLaps, true) + " "
                //								+ GetSessionLengthPostFix(rst.RaceLaps, true) + " of " + rst.TrackNameShort, Verbose.Session);
                //						}
                //						else if (rst.QualMins > 0)
                //						{
                //							sessionInfo.EnterQualification();
                //							LFSStats.ConsoleTitleShow(rst.TrackNameShort + ": Qual Lap 1");
                //							LFSStats.WriteLine("\nQual start: " + GetSessionLengthString(rst.QualMins, false) + " "
                //								+ GetSessionLengthPostFix(rst.QualMins, false) + " of " + rst.TrackNameShort, Verbose.Session);
                //						}
                //						else
                //						{
                //							sessionInfo.EnterPractice();
                //							LFSStats.ConsoleTitleShow(rst.TrackNameShort + ": Prac Lap 1");
                //							LFSStats.WriteLine("\nPrac start: " + rst.TrackNameShort, Verbose.Session);
                //						}


                //						// Get All Connections
                //						byte[] rstncn = InSim.Encoder.NCN();
                //						insimConnection.Send(rstncn, rstncn.Length);
                //						// Get All Players
                //						byte[] rstnpl = InSim.Encoder.NPL();
                //						insimConnection.Send(rstnpl, rstnpl.Length);
                //						// Get Reo
                //						byte[] nplreo = InSim.Encoder.REO();
                //						insimConnection.Send(nplreo, nplreo.Length);
                //						continue;
                //					}
                //					#endregion
                //					#region TINY
                //					if (packetHead == "TINY")	//confirm ack with ack
                //					{
                //						// Keep ALIVE
                //						InSim.Decoder.TINY tiny = new InSim.Decoder.TINY(recvPacket);
                //						// Keep alive connection
                //						if (tiny.SubT == "TINY_NONE")
                //						{
                //							byte[] stiny = InSim.Encoder.TINY_NONE();
                //							insimConnection.Send(stiny, stiny.Length);
                //						}
                //						continue;
                //					}
                //					#endregion
                //					if (sessionInfo.InSession())
                //					{
                //						switch (packetHead)
                //						{
                //							case "REN":
                //								sessionInfo.ExitSession();
                //#if DEBUG
                //								Console.WriteLine("Race ENd (return to entry screen)");
                //#endif
                //								break;
                //							case "NCN":
                //								InSim.Decoder.NCN newConnection = new InSim.Decoder.NCN(recvPacket);
                //#if DEBUG
                //								Console.WriteLine(string.Format("New ConN Username: {0} Nickname: {1} ConnectionNumber: {2}", newConnection.userName, newConnection.nickName, newConnection.UCID));
                //#endif
                //								// On reconnect player, RAZ infos et restart
                //								UCIDToUN[newConnection.UCID] = new UN(newConnection.userName, newConnection.nickName);
                //								if (newConnection.UCID == 0)
                //									sessionInfo.HName = newConnection.nickName;
                //								break;
                //							case "CNL":
                //#if DEBUG
                //								Console.WriteLine("ConN Leave (end connection is moved down into this slot)");
                //#endif
                //								InSim.Decoder.CNL lostConnection = new InSim.Decoder.CNL(recvPacket);
                //#if DEBUG
                //								Console.WriteLine(string.Format("Username:{0} Nickname:{1} ConnectionNumber:{2}",
                //									UCIDToUN[lostConnection.UCID].userName, UCIDToUN[lostConnection.UCID].nickName, lostConnection.UCID));
                //#endif
                //								break;
                //							#region NPL
                //							case "NPL":
                //								int LastPosGrid;
                //#if DEBUG
                //								Console.WriteLine("New PLayer joining race (if PLID already exists, then leaving pits)");
                //#endif
                //								InSim.Decoder.NPL newPlayer = new InSim.Decoder.NPL(recvPacket);
                //								int removePLID = PLIDbyNickName(newPlayer.nickName);
                //								//                            Console.WriteLine("Nick:" + newPlayer.NickName);
                //								//                            Console.WriteLine("Old PLID:" + removePLID + " New : " + newPlayer.PLID);
                //								LastPosGrid = 0;

                //								switch (sessionInfo.session)
                //								{
                //									case SessionInfo.Session.Practice:		// TODO new player in practice
                //										break;
                //									case SessionInfo.Session.Qualification:
                //										if (removePLID > 0)
                //										{
                //											if (newPlayer.PLID != removePLID)
                //											{
                //												raceStat[newPlayer.PLID] = raceStat[removePLID];
                //												raceStat[newPlayer.PLID].PLID = newPlayer.PLID;
                //												raceStat.Remove(removePLID);
                //												PLIDToUCID.Remove(removePLID);
                //											}
                //										}
                //										break;
                //									case SessionInfo.Session.Race:
                //										if (removePLID > 0)
                //										{
                //											if (raceStat[removePLID].finished == false)
                //											{
                //												LastPosGrid = raceStat[removePLID].gridPos;
                //												raceStat.Remove(removePLID);
                //												PLIDToUCID.Remove(removePLID);
                //											}
                //											else
                //											{
                //												if (newPlayer.PLID != removePLID)
                //												{
                //													raceStat[newPlayer.PLID] = raceStat[removePLID];
                //													raceStat[newPlayer.PLID].PLID = newPlayer.PLID;
                //													raceStat.Remove(removePLID);
                //													PLIDToUCID.Remove(removePLID);
                //												}
                //											}
                //										}
                //										break;
                //								}

                //								PLIDToUCID[newPlayer.PLID] = newPlayer.UCID;
                //								string nplUserName = UserNameByUCID(newPlayer.UCID);

                //								if (!raceStat.ContainsKey(newPlayer.PLID))
                //								{
                //									raceStat[newPlayer.PLID] = new SessionStats(newPlayer.UCID, newPlayer.PLID);
                //									if (LastPosGrid != 0)
                //										raceStat[newPlayer.PLID].gridPos = LastPosGrid;
                //								}
                //								raceStat[newPlayer.PLID].UCID = newPlayer.UCID;
                //								raceStat[newPlayer.PLID].userName = nplUserName;
                //								raceStat[newPlayer.PLID].NickName = newPlayer.nickName;
                //								raceStat[newPlayer.PLID].allUN[nplUserName] = new UN(nplUserName, newPlayer.nickName);
                //								raceStat[newPlayer.PLID].carName = newPlayer.CName;
                //								raceStat[newPlayer.PLID].Plate = newPlayer.Plate;
                //								raceStat[newPlayer.PLID].Flags = newPlayer.Flags;
                //								if (sessionInfo.InQualification())
                //									raceStat[newPlayer.PLID].finPLID = newPlayer.PLID;
                //								break;
                //							#endregion

                //							case "CLR":
                //#if DEBUG
                //								Console.WriteLine("CLear Race - all players removed from race in one go");
                //#endif
                //								//raceStat.Clear();	// why invalidate the so far collected statistics though?
                //								break;
                //							case "PIT": // NEW PIT : (pit stop)
                //								InSim.Decoder.PIT pitDec = new InSim.Decoder.PIT(recvPacket);
                //								raceStat[pitDec.PLID].updatePIT(pitDec);
                //								LFSStats.WriteLine("Pit", UCIDToUN[PLIDToUCID[pitDec.PLID]].nickName, Verbose.Info);
                //								break;
                //							case "PSF": // NEW PSF : (pit stop finished)
                //								InSim.Decoder.PSF pitFin = new InSim.Decoder.PSF(recvPacket);
                //								(raceStat[pitFin.PLID]).updatePSF(pitFin.PLID, pitFin.STime);
                //								break;
                //							#region LAP
                //							case "LAP":
                //								InSim.Decoder.LAP lapDec = new InSim.Decoder.LAP(recvPacket);
                //								//string lapNickName = UCIDToUN[PLIDToUCID[lapDec.PLID]].nickName;
                //								//string lapUserName = UCIDToUN[PLIDToUCID[lapDec.PLID]].userName;
                //								LFSStats.WriteLine("Lap " + lapDec.LapsDone, lapDec.lapTime, raceStat[lapDec.PLID].userName, Verbose.Lap);

                //								string sessionName = sessionInfo.session.ToString().Substring(0, 4);

                //								if (lapDec.LapsDone > sessionInfo.currentLap)	// only update to the highest lap count
                //								{
                //									sessionInfo.currentLap = lapDec.LapsDone;
                //									LFSStats.ConsoleTitleShow(sessionInfo.trackNameCode + ": " + sessionName + " Lap " + (lapDec.LapsDone + 1).ToString());
                //								}
                //								if (sessionInfo.InQualification())
                //									break;	// In qual use RES instead of Lap	WHY? Probably reports more data.
                //								raceStat[lapDec.PLID].UpdateLap(lapDec.lapTime, raceStat[lapDec.PLID].numStop, lapDec.LapsDone, sessionInfo.maxSplit);	// TODO why is number of pitstops here taken from stats and not from lap packet?
                //								break;
                //							#endregion
                //							#region SPX
                //							case "SPX":
                //								InSim.Decoder.SPX splitdec = new InSim.Decoder.SPX(recvPacket);
                //								// Assign temporary nickname to Player ID
                //								if (splitdec.splitTime == timeConv.HMSToLong(60, 0, 0))	// FIXME this could be used to remove first laps after joining that are 60:00:00 as well, if that is a issue somewhere
                //									break;
                //								raceStat[splitdec.PLID].UpdateSplit(splitdec.split, splitdec.splitTime);
                //								LFSStats.WriteLine("SP " + splitdec.split, splitdec.splitTime, raceStat[splitdec.PLID].userName, Verbose.Split);

                //								if (sessionInfo.maxSplit < splitdec.split)
                //									sessionInfo.maxSplit = splitdec.split;	// FIXME player changes car and that changes car in stats after race finished
                //								//if (splitdec.split == 1 && sessionInfo.maxSplit < 1)
                //								//    sessionInfo.maxSplit = 1;
                //								//if (splitdec.split == 2 && sessionInfo.maxSplit < 2)
                //								//    sessionInfo.maxSplit = 2;
                //								//if (splitdec.split == 3 && sessionInfo.maxSplit < 3)
                //								//    sessionInfo.maxSplit = 3;
                //								break;
                //							#endregion
                //							#region RES
                //							case "RES":
                //								InSim.Decoder.RES result = new InSim.Decoder.RES(recvPacket);
                //#if DEBUG
                //								Console.WriteLine("RESult (qualify or finish)");
                //#endif
                //								if (sessionInfo.InRace())
                //									LFSStats.WriteLine("Result", result.ResultNum + 1, ExportStats.lfsStripColor(result.nickName), Verbose.Session);
                //								int lplid = result.PLID;
                //								if (result.PLID == 0)
                //									lplid = PLIDbyNickName(result.nickName);
                //								// Retrieve current PLID with the Finish PLID, in case of player spectate and rejoin race after finish
                //								lplid = PLIDbyFinPLID(lplid);
                //								if (lplid != -1 && sessionInfo.InRace())
                //									(raceStat[lplid]).UpdateResult(result.TTime, result.ResultNum, result.CName, result.Confirm, result.NumStops);

                //								if (lplid != -1 && sessionInfo.InQualification())
                //								{
                //									//WriteLine("RESult:\t" + SessionStats.LfsTimeToString(result.BTime)
                //									//    + " " + raceStat[lplid].userName, Verbose.Lap);	// is reported by LAP
                //									raceStat[lplid].UpdateQualResult(result.TTime, result.BTime, result.NumStops, result.Confirm, result.LapsDone, result.ResultNum, result.NumRes, sessionInfo.maxSplit); // FIXME just pass the result instead
                //								}

                //								break;
                //							#endregion
                //							case "FIN":
                //								InSim.Decoder.FIN fin = new InSim.Decoder.FIN(recvPacket);
                //#if DEBUG
                //								Console.WriteLine("FINish (qualify or finish)");
                //#endif
                //								(raceStat[fin.PLID]).finished = true;
                //								(raceStat[fin.PLID]).finPLID = fin.PLID;
                //								break;
                //							#region REO
                //							case "REO":
                //								InSim.Decoder.REO pacreo = new InSim.Decoder.REO(recvPacket);
                //#if DEBUG
                //								Console.WriteLine("REOrder (when race restarts after qualifying)");
                //#endif

                //								if (pacreo.ReqI != 0) // Ignore and get Only if requested
                //								{
                //									for (int i = 0; i < pacreo.NumP; i++)
                //									{
                //										int PLID = (int)pacreo.PLID[i];
                //										if (!raceStat.ContainsKey(PLID))
                //										{
                //											raceStat[PLID] = new SessionStats(0, PLID);
                //										}
                //										(raceStat[PLID]).gridPos = i + 1;
                //									}
                //								}
                //								break;
                //							#endregion
                //							#region STA
                //							case "STA":
                //#if DEBUG
                //								Console.WriteLine("STAte");
                //#endif
                //								InSim.Decoder.STA state = new InSim.Decoder.STA(recvPacket);
                //								if (flagFirst && (state.Flags & 512) == 512)	// 512 Cruise something
                //								{
                //									flagFirst = false;
                //									// Get All Connections
                //									byte[] ncn = InSim.Encoder.NCN();
                //									insimConnection.Send(ncn, ncn.Length);
                //									// Get All Players
                //									byte[] npl = InSim.Encoder.NPL();
                //									insimConnection.Send(npl, npl.Length);
                //								}
                //								sessionInfo.trackNameCode = state.ShortTrackName;
                //								sessionInfo.weather = state.Weather;
                //								sessionInfo.wind = state.Wind;
                //								sessionInfo.raceLaps = state.RaceLaps;
                //								sessionInfo.qualMins = state.QualMins;
                //								sessionInfo.sraceLaps = GetSessionLengthString(sessionInfo.raceLaps, true);
                //#if DEBUG
                //								Console.WriteLine("Current track: " + sessionInfo.trackNameCode);
                //#endif
                //								if (state.RaceInProg == 0)
                //								{
                //									switch (sessionInfo.session)
                //									{
                //										case SessionInfo.Session.Practice:		// End of Practice
                //											LFSStats.WriteLine("End of practice by STAte", Verbose.Session);
                //											break;
                //										case SessionInfo.Session.Qualification:	// End of Qualification
                //											LFSStats.WriteLine("End of qualification by STAte", Verbose.Session);
                //											ExportQualStats(false);
                //											break;
                //										case SessionInfo.Session.Race:			// End of Race
                //											LFSStats.WriteLine("End of race by STAte", Verbose.Session);
                //											ExportStatistics(false);
                //											break;
                //									}
                //									LFSStats.ConsoleTitleRestore();
                //									sessionInfo.ExitSession();
                //								}
                //								break;
                //							#endregion
                //							#region MSO
                //							case "MSO":
                //								InSim.Decoder.MSO msg = new InSim.Decoder.MSO(recvPacket);
                //#if DEBUG
                //								Console.WriteLine("Message received:" + msg.message);
                //#endif
                //								//if (msg.message.StartsWith("!ver"))	// needs to be updated, only works on multiplayer host
                //								//    Ver(msg.PLID);
                //								if (msg.UserType == 1)	// 1 - normal visible user message, should use enumeration
                //								{
                //									// this UCID might need to be investigated if car swap causes wrong message author,
                //									// only use UCID when PLID is zero but there is no PLID to nickname conversion
                //									sessionInfo.chat.Add(NickNameByUCID(msg.UCID), msg.message);// FIXME the nicks are not always there
                //								}
                //								break;
                //							#endregion
                //							#region ISM
                //							case "ISM":
                //								InSim.Decoder.ISM multi = new InSim.Decoder.ISM(recvPacket);
                //#if DEBUG
                //								Console.WriteLine("Insim Multi:" + multi.HName);
                //#endif
                //								// Get All Connections
                //								byte[] ismncn = InSim.Encoder.NCN();
                //								insimConnection.Send(ismncn, ismncn.Length);
                //								// Get All Players
                //								byte[] ismnpl = InSim.Encoder.NPL();
                //								insimConnection.Send(ismnpl, ismnpl.Length);
                //								break;
                //							#endregion
                //							#region MCI
                //							case "MCI":
                //								InSim.Decoder.MCI mci = new InSim.Decoder.MCI(recvPacket);
                //								for (int i = 0; i < System.Math.Min(8, mci.numOfPlayers); i++)
                //								{
                //									if (raceStat.ContainsKey(mci.compCar[i].PLID))
                //									{
                //										(raceStat[mci.compCar[i].PLID]).updateMCI(mci.compCar[i].speed);	// Speed
                //									}
                //								}
                //								break;
                //							#endregion
                //							#region FLG
                //							case "FLG": // NEW FLG : (yellow or blue flags)
                //								InSim.Decoder.FLG flg = new InSim.Decoder.FLG(recvPacket);
                //								if (flg.onOff == 1 && flg.Flag == 1 && (raceStat[flg.PLID]).inBlue == false)
                //								{ // Blue flag On
                //									(raceStat[flg.PLID]).blueFlags++;
                //									(raceStat[flg.PLID]).inBlue = true;
                //								}
                //								if (flg.onOff == 1 && flg.Flag == 2 && (raceStat[flg.PLID]).inYellow == false)
                //								{ // Yellow flag On
                //									(raceStat[flg.PLID]).yellowFlags++;
                //									(raceStat[flg.PLID]).inYellow = true;
                //								}
                //								if (flg.onOff == 0 && flg.Flag == 1)
                //								{ // Blue flag Off
                //									(raceStat[flg.PLID]).inBlue = false;
                //								}
                //								if (flg.onOff == 0 && flg.Flag == 2)
                //								{ // Yellow flag Off
                //									(raceStat[flg.PLID]).inYellow = false;
                //								}
                //								break;
                //							#endregion
                //							case "PEN": // NEW PEN : (penalty)
                //								InSim.Decoder.PEN pen = new InSim.Decoder.PEN(recvPacket);
                //								(raceStat[pen.PLID]).UpdatePen(pen.OldPen, pen.NewPen, pen.Reason);
                //								break;
                //							case "NLP":
                //								break;
                //							case "VTC":
                //								break;
                //							case "VTN": // Insim Vote
                //								break;
                //							case "VTA": // Insim Vote Action
                //								break;
                //							case "III": // NEW III : InSimInfo
                //								break;
                //							case "PLA": // NEW PLA : (Pit LAne)
                //								break;
                //							#region TOC
                //							case "TOC": // NEW TOC : (take over car)
                //								InSim.Decoder.TOC toc = new InSim.Decoder.TOC(recvPacket);
                //								if (!raceStat.ContainsKey(toc.PLID))
                //									break;

                //								sessionInfo.isToc = true;

                //								int OldPLID = UCIDToPLID(toc.NewUCID);
                //								PLIDToUCID.Remove(OldPLID);
                //								raceStat.Remove(OldPLID);

                //								string oldUserName = (raceStat[toc.PLID]).userName;
                //								string oldNickName = (raceStat[toc.PLID]).NickName;
                //								raceStat[toc.PLID].NickName = UCIDToUN[toc.NewUCID].nickName;
                //								raceStat[toc.PLID].userName = UCIDToUN[toc.NewUCID].userName;
                //								raceStat[toc.PLID].allUN[UCIDToUN[toc.NewUCID].userName] = new UN(UCIDToUN[toc.NewUCID].userName, UCIDToUN[toc.NewUCID].nickName);
                //								string newUserName = (raceStat[toc.PLID]).userName;
                //								string newNickName = (raceStat[toc.PLID]).NickName;
                //								raceStat[toc.PLID].updateTOC(oldNickName, oldUserName, newNickName, newUserName);
                //								PLIDToUCID[toc.PLID] = toc.NewUCID;
                //								LFSStats.WriteLine("TOC", oldUserName + " --> " + newUserName, Verbose.Info);
                //								break;
                //							#endregion
                //							case "PFL": // NEW PFL : (player help flags)
                //								break;
                //							case "CCH": // NEW CCH : (camera changed)
                //								break;
                //							case "AXI": // send an IS_AXI
                //								break;  // autocross cleared
                //							case "AXC":
                //								break;
                //							#region SMALL
                //							case "SMALL":	// Practice end and other occasions
                //#if DEBUG
                //								Console.WriteLine("SMALL");
                //#endif
                //								// Causes a big since ther exists an invalid SMALL with size 4 that contains [4,4,4,4]
                //								//InSim.Decoder.SMALL small = new InSim.Decoder.SMALL(recvPacket);
                //								//// End of practice shows as "SMALL vote action" at least when alone on a localhost server.
                //								//// Although when going spectate, practice ends but SMALL.VTA is not reported.
                //								//// Also when ending Q or R SMALL_VTA is sent.
                //								//// Ending Q either by starting R or going to host screen from Q or R.
                //								//if (small.SubT == (int)InSim.TypeSmall.SMALL_VTA)
                //								//{
                //								//    LFSStats.ConsoleTitleRestore();
                //								//    inSession = false;	// closes collecting of data and prevent crashes due to silly insim sending stuff when session wasn't started yet
                //								//}
                //								break;
                //							#endregion

                //							default:
                //#if DEBUG
                //								Console.WriteLine("Unknown packet received: " + packetHead);
                //#endif
                //								break;
                //						}
                //					}
                //				}
            }
            if (insimConnection.IsConnected)
            {
                insimConnection.Disconnect();
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
				if (duration == 0)			// Practice
					return "Practice";
				else if (duration < 100)	// 1-99 (laps)
					return duration.ToString();
				else if (duration < 191)	// 100-1000 (laps)
					return ((duration - 100) * 10 + 100).ToString();
				else if (duration < 239)	// 1-48 (hours)
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
				if (duration == 0)			// Practice
					return "";
				else if (duration == 1)		// 1 lap
					return "lap";
				else if (duration < 191)	// 2-1000 laps
					return "laps";
				else if (duration == 191)	// 1 hour
					return "hour";
				else							// 2-48 hours
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
			Directory.CreateDirectory(config.QualDir);	// @ is used to interpret string literally, since it may have forward or backslash
#if DEBUG
			Stopwatch sw = new Stopwatch();
#endif
			TaskFactory tf = new TaskFactory();
			Task exportTask;
			List<SessionStats> tmpList = new List<SessionStats>(raceStat.Values);
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
			if (!ExportOrNot(isRST, out fileName)) return;	// decides whether to export and generates a filename or asks user for one
#if DEBUG
			Stopwatch sw = new Stopwatch();
#endif
			TaskFactory tf = new TaskFactory();
			Task exportTask;
			List<SessionStats> tmpList = new List<SessionStats>(raceStat.Values);
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
			string graph = "";
			Stopwatch sw = new Stopwatch();
			sw.Start();
			ExportStats.HtmlResult(sessionStatsList, fileName, sessionInfo, config);
			ExportStats.CsvResult(sessionStatsList, fileName, sessionInfo, config);
			ExportStats.TsvResult(sessionStatsList, fileName, sessionInfo, config);
			sw.Stop();
			LFSStats.WriteLineFromAsync("Race Stats exported (" + sw.ElapsedMilliseconds + "ms" + ")", Verbose.Program);

			sw.Restart();
			// Call graph generator
			try
			{
				switch (config.GenerateGraphs)
				{
					case Configuration.GraphOptions.no:
					default:
						graph = "No";
						break;
					case Configuration.GraphOptions.dll:
						graph = "Graph.dll";
						Graph.Graph.GenerateGraph();
						break;
					case Configuration.GraphOptions.exe:
						graph = "Graph.exe";
						Process graphProcess = Process.Start("Graph.exe");	// should be parallel, LFSStats does not wait for exit
						//graphProcess.WaitForExit();	// only to test it's processing performance
						break;
				}
			}
			catch (Exception ex)
			{
				LFSStats.ErrorWriteException("Graph exception occurred:", ex);
			}

			sw.Stop();
			LFSStats.WriteLineFromAsync(graph + " graphs generated (" + sw.ElapsedMilliseconds + "ms" + ")", Verbose.Program);
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
    }
}
