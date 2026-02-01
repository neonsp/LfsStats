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
#define MONO
#define WINDOWS

using System;
using System.Diagnostics;
using System.Threading;
using System.Reflection;


namespace LFSStat
{
    class LFSStat
    {
		static string consoleTitle = "LFSStat By JackCY";

        #region Main
        /// <summary>
        /// References GNU GPL License
        /// </summary>
        static void PrintLicense()
        {
            Console.WriteLine(System.Reflection.Assembly.GetExecutingAssembly().ToString());
            Console.WriteLine("GNU license 2008 Jaroslav CERNY ( JackCY )");
            Console.WriteLine("LFSStat comes with ABSOLUTELY NO WARRANTY");
            Console.WriteLine("This is free software, and you are welcome to redistribute it");
            Console.WriteLine("under certain conditions. Read LICENSE for details.");
            Console.WriteLine();
        }

        /// <summary>
        /// Prints command line usage with license.
        /// </summary>
        static void PrintUsage()
        {
            Console.WriteLine("Usage: LFSStat.exe scriptfilename [debug]\n");
			Console.WriteLine("Report bugs and suggestions to: http://www.lfsforum.net/showthread.php?t=24933\n");
        }

        [STAThread]
        static void Main(string[] Args)
        {
#if WINDOWS
			Console.Title = consoleTitle;
#endif
            try
            {
                PrintLicense();

                
                if (Args.Length > 2)
                {
                    PrintUsage();
                    return;
                }

                bool debugmode = false;
                if (Args.Length > 1)
                    debugmode = Args[1].ToUpper() == "DEBUG";

                if (Args.Length == 0)
                {
                    LFSClient lfsclient = new LFSClient("LFSStat.cfg", false);
//                    LFSClient lfsclient = new LFSClient("LFSStat.cfg", true);
                }
                else
                {
                    LFSClient lfsclient = new LFSClient(Args[0], debugmode);
                }
            }
            catch (Exception ex)
            {
//                System.Console.WriteLine(ex.Message);
                Console.WriteLine("The following error occurred:");
                Console.WriteLine(ex.Message);   // Print the error message.
                Console.WriteLine(ex.Source);    // Name of application or object that caused the error.
                Console.WriteLine(ex.StackTrace); //String that contains the stack trace for this exception.
                Console.WriteLine(ex.TargetSite ); //String that contains the stack trace for this exception.
                System.Threading.Thread.Sleep(25000);
            }
        }

        #endregion
    }
    class UN
    {
        public string userName;
        public string nickName;
        public UN(string userName, string nickName)
        {
            this.userName = userName;
            this.nickName = nickName;
        }
    }
    class infoRace
    {
        public string currentTrackName ="";
        public int maxSplit=0;
        public int weather=0;
        public int wind=0;
        public int raceLaps=0;
        public string sraceLaps = "";
        public int qualMins = 0;
        public string HName;
        public int currLap = 0;
        public bool isToc = false;
        public System.Collections.ArrayList chat = new System.Collections.ArrayList();

    }
    class LFSClient
    {
		string consoleTitle = "LFSStat By JackCY";

        bool wrLoaded = false;
        InSim.Connect insimConnection = new InSim.Connect();



        System.Collections.Hashtable raceStat = new System.Collections.Hashtable();

        System.Collections.Hashtable PLIDToUCID = new System.Collections.Hashtable();
        System.Collections.Hashtable UCIDToUN = new System.Collections.Hashtable();

        #region file configuration options

//        string currentTrackName = "";
//        int maxSplit = 0;
        infoRace currInfoRace = new infoRace();

        string language = "english";
        string raceDir = "";
        string qualDir = "";
        int maxTimeQualIgnore = 10;
        bool TCPmode = true;


     
        System.Collections.ArrayList shortCarNames = new System.Collections.ArrayList();
        System.Collections.ArrayList longCarNames = new System.Collections.ArrayList();
        bool debugmode = false;

		string exportOnSTAte = "yes";		// yes, no
		string exportOnRaceSTart = "yes";	// yes, no
		string generateGraphs = "dll";		// dll, exe, no
		bool askForFileNameOnRST = false;	// true, false
		bool askForFileNameOnSTA = false;	// true, false

        #endregion
        
        #region Send Message Helpers

        /// <summary>
        /// Executes action
        /// </summary>
        /// <param name="msg"></param>
        void SendMsg(string msg)
        {
            if (msg.Length > 0)
            {
                byte[] outMsg = InSim.Encoder.MST(msg);
                insimConnection.Send(outMsg, outMsg.Length);
            }
        }
        void SendMsg(byte []msg)
        {
            byte[] outMsg = InSim.Encoder.MST(msg);
            insimConnection.Send(outMsg, outMsg.Length);
        }
        void SendMsgToConnection(byte connectionNumber, string msg)
        {
            if (msg.Length > 0)
            {
                byte[] outMsg = InSim.Encoder.MTC(connectionNumber, 0, msg);
                insimConnection.Send(outMsg, outMsg.Length);
            }
        }

        void SendMsgToConnection(byte connectionNumber, byte []msg)
        {
            byte[] outMsg = InSim.Encoder.MTC(connectionNumber, 0, msg);
            insimConnection.Send(outMsg, outMsg.Length);
        }

        void SendMsgToPlayer(int PLID, string msg)
        {
            if (msg.Length > 0)
            {
                byte[] outMsg = InSim.Encoder.MTC(0, PLID, msg);
                insimConnection.Send(outMsg, outMsg.Length);
            }
        }

        void SendMsgToPlayer(int PLID, byte []msg)
        {
            byte[] outMsg = InSim.Encoder.MTC(0, PLID, msg);
            insimConnection.Send(outMsg, outMsg.Length);
        }

        #endregion


        void Ver( int PLID )
        {
            try
            {
                SendMsgToPlayer(PLID, System.Reflection.Assembly.GetExecutingAssembly().FullName);
                SendMsgToPlayer(PLID, "^3http://monkster.hopto.org/~janez/LFSStat/LFSStat.html");
            }
            catch (System.Exception)
            {
                return;
            }
        }
        string generateFileName()
        {
			return (string.Format("{0:0000}", System.DateTime.Now.Year)
				+ "-" + string.Format("{0:00}", System.DateTime.Now.Month)
				+ "-" + string.Format("{0:00}", System.DateTime.Now.Day)
				+ "_" + string.Format("{0:00}", System.DateTime.Now.Hour)
				+ "." + string.Format("{0:00}", System.DateTime.Now.Minute)
				+ "." + string.Format("{0:00}", System.DateTime.Now.Second));
        }
        public string NickNameByUCID(int UCID)
        {
            return (UCIDToUN[UCID] as UN).nickName;
        }
        public string UserNameByUCID(int UCID)
        {
            return (UCIDToUN[UCID] as UN).userName;
        }
        public int PLIDbyNickName(string nickName)
        {
            foreach (System.Collections.DictionaryEntry de in raceStat)
            {
                if (nickName == (de.Value as raceStats).nickName)
                    return (de.Value as raceStats).PLID;
            }
            return -1;
        }
        public int PLIDbyFinPLID(int finPLID)
        {
            foreach (System.Collections.DictionaryEntry de in raceStat)
            {
                if (finPLID == (de.Value as raceStats).finPLID)
                    return (de.Value as raceStats).PLID;
            }
            return -1;
        }
        public int UCIDToPLID(int UCID)
        {
            foreach (System.Collections.DictionaryEntry de in PLIDToUCID )
            {
                if (UCID == (int)de.Value)
                    return (int)de.Key;
            }
            return -1;
        }
        // To intercept Ctrl-C, ..., and other break and Close Lapper in safe mode
        public void inputHandler(ConsoleCtrl.ConsoleEvent consoleEvent)
        {
            if (consoleEvent == ConsoleCtrl.ConsoleEvent.CtrlC
                || consoleEvent == ConsoleCtrl.ConsoleEvent.CtrlClose
                || consoleEvent == ConsoleCtrl.ConsoleEvent.CtrlBreak
                || consoleEvent == ConsoleCtrl.ConsoleEvent.CtrlLogoff
                || consoleEvent == ConsoleCtrl.ConsoleEvent.CtrlShutdown)
            {
                byte[] cl = InSim.Encoder.IS_TINY((byte)InSim.TypePack.ISP_TINY, 0, (byte)InSim.TypeTiny.TINY_CLOSE);
                insimConnection.Send(cl, cl.Length);
                System.Environment.Exit(-1);
            }
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="LFSClient"/> class.
		/// </summary>
		/// <param name="scriptfilename">The scriptfilename.</param>
		/// <param name="debugmode">if set to <c>true</c> [debugmode].</param>
        public LFSClient(string scriptfilename, bool debugmode)
        {

            this.debugmode = debugmode;

            ConsoleCtrl cc = new ConsoleCtrl();
            cc.ControlEvent += new ConsoleCtrl.ControlEventHandler(inputHandler);


            #region Car Names Tables
            shortCarNames.Add("UF1"); longCarNames.Add("UF 1000");
            shortCarNames.Add("XFG"); longCarNames.Add("XF GTI");
            shortCarNames.Add("XRG"); longCarNames.Add("XR GT");
            shortCarNames.Add("XRT"); longCarNames.Add("XR GT TURBO");
            shortCarNames.Add("RB4"); longCarNames.Add("RB4 GT");
            shortCarNames.Add("FXO"); longCarNames.Add("FXO TURBO");
            shortCarNames.Add("LX4"); longCarNames.Add("LX4");
            shortCarNames.Add("LX6"); longCarNames.Add("LX6");
            shortCarNames.Add("RAC"); longCarNames.Add("RACEABOUT");
            shortCarNames.Add("FZ5"); longCarNames.Add("FZ50");
            shortCarNames.Add("MRT"); longCarNames.Add("MRT5");
            shortCarNames.Add("XFR"); longCarNames.Add("XF GTR");
            shortCarNames.Add("UFR"); longCarNames.Add("UF GTR");
            shortCarNames.Add("FOX"); longCarNames.Add("FORMULA XR");
            shortCarNames.Add("FO8"); longCarNames.Add("FORMULA V8");
            shortCarNames.Add("FXR"); longCarNames.Add("FXO GTR");
            shortCarNames.Add("XRR"); longCarNames.Add("XR GTR");
            shortCarNames.Add("FZR"); longCarNames.Add("FZ50 GTR");
            shortCarNames.Add("BF1"); longCarNames.Add("BMW SAUBER F1.06");
			shortCarNames.Add("FBM"); longCarNames.Add("FORMULA BMW FB02");
			shortCarNames.Add("VWS"); longCarNames.Add("VW SCIROCCO");
            #endregion

            #region Configuring LFSStat

			// Load Config
            LFS.Configurator cfg = new LFS.Configurator();
            cfg.Load(scriptfilename);
			// Load World Record
            string PubStatIdk = cfg.Get("PubStatIdk");
            string PubStatUser = cfg.Get("PubStatUser");
            string PubStatPass = cfg.Get("PubStatPass");
            Console.Write("Loading WR...");
            if (wr.load(PubStatUser, PubStatPass, PubStatIdk))
            {
                wrLoaded = true;
                Console.WriteLine("Ok");
            }
            else
            {
                wrLoaded = false;
                Console.WriteLine("Not Loaded");
            }
			
            string adminPassword = cfg.Get("Password");

            string host = cfg.Get("Host");
			if (host == "")
				host = "127.0.0.1";

			int port = 29999;
			try { port = int.Parse(cfg.Get("Port")); }
			catch (Exception) { }

            language = cfg.Get("lang");
            if (language == "")
                language = "english";
            exportstats.getLang(language);

            raceDir = cfg.Get("raceDir");
            if (raceDir == "")
                raceDir = ".";
            qualDir = cfg.Get("qualDir");
            if (qualDir == "")
                qualDir = ".";

            try { maxTimeQualIgnore = int.Parse(cfg.Get("maxTimeQualIgnore")); }
            catch (Exception) { }

            try { TCPmode = bool.Parse(cfg.Get("TCPmode")); }
            catch (Exception) { }

			try { askForFileNameOnRST = bool.Parse(cfg.Get("askForFileNameOnRST")); }
			catch (Exception) { }

			try { askForFileNameOnSTA = bool.Parse(cfg.Get("askForFileNameOnSTA")); }
			catch (Exception) { }

			// Export graphs?
			generateGraphs = cfg.Get("generateGraphs");
			//switch (generateGraphs)
			//{
			//    case "dll":
			//    case "exe":
			//    case "no":
			//        break;

			//    default:
			//        exportOnRaceSTart = "dll";
			//        break;
			//}

			// Export Statistics when STAte changed?
			exportOnSTAte = cfg.Get("exportOnSTAte");
			//switch (exportOnSTAte)
			//{
			//    case "yes":
			//    case "no":
			//    case "ask":
			//        break;

			//    default:
			//        exportOnSTAte = "yes";
			//        break;
			//}

			// Export Statistics when Race STart?
			exportOnRaceSTart = cfg.Get("exportOnRaceSTart");
			//switch (exportOnRaceSTart)
			//{
			//    case "yes":
			//    case "no":
			//    case "ask":
			//        break;

			//    default:
			//        exportOnRaceSTart = "yes";
			//        break;
			//}

            #endregion

            insimConnection.insimConnect(host, port, adminPassword , "U", "LFSStat",false,TCPmode );
            string info = "Product:" + insimConnection.Product + " Version:" + insimConnection.Version + " InSim Version:" + insimConnection.InSimVersion;
            Console.WriteLine(info);
/*
            uc = new System.Net.Sockets.UdpClient(host, port);

#if MONO
            int localport = 0;  //for mono
#else
            int localport = ((System.Net.IPEndPoint)uc.Client.LocalEndPoint).Port;//for .NET
#endif

            // init
            byte[] inSimInit = InSim.Encoder.ISI(adminPassword,localport,0);
            uc.Send(inSimInit, inSimInit.Length);

            //request version
            byte[] verreq = InSim.Encoder.VER();
            uc.Send(verreq, verreq.Length);

            //retr version info
            System.Net.IPEndPoint remoteEP = new System.Net.IPEndPoint(System.Net.IPAddress.Any, 0);
            byte[] recvPacket = uc.Receive(ref remoteEP);
            InSim.Decoder.VER ver = new InSim.Decoder.VER(recvPacket);
            string info = "Product:" + ver.Product + " Version:" + ver.Version + " InSim Version:" + ver.InSimVersion;
            Console.WriteLine(info);

            // send ver info to console
            SendMsg("/msg " + info);
*/
            byte[] intervalReq = InSim.Encoder.NLI(100);    //receive status every 100 ms
            insimConnection.Send(intervalReq,intervalReq.Length);

            Console.WriteLine("LFSStat is running...");
            byte[] sstreq = InSim.Encoder.SST();
            insimConnection.Send(sstreq, sstreq.Length);

            Loop(insimConnection);
//            Loop(uc);

            // terminate
            byte[] terreq = InSim.Encoder.ISC();
            insimConnection.Send(terreq, terreq.Length);

            insimConnection.Close();
//            uc.Close();
        }




//        void Loop(System.Net.Sockets.UdpClient uc)
        void Loop(InSim.Connect insimConnection )
        {

            bool InRace = false;
            bool InQual = false;
            bool flagFirst = true;
            bool flagShowInfo = true;


//            System.Net.IPEndPoint remoteEP = new System.Net.IPEndPoint(System.Net.IPAddress.Any, 0);

            while (true)
            {
//                byte[] recvPacket = uc.Receive(ref remoteEP);
                byte[] recvPacket = insimConnection.Receive();
#if MONO
#else
                if (remoteEP.Port != ((System.Net.IPEndPoint)uc.Client.RemoteEndPoint).Port)
                {
                    Console.WriteLine("Received UDP packet from unknown port {0}", ((System.Net.IPEndPoint)uc.Client.RemoteEndPoint).Port);
                    continue;
                }
#endif

                if (recvPacket.Length > 3)
                {
                    string packetHead = insimConnection.packetHead(recvPacket);
                    uint verifyID = insimConnection.verifyID(recvPacket);
//                    if (packetHead != "MCI")
//                        Console.WriteLine(packetHead);
                    switch (packetHead)
                    {
                        case "TINY"://confirm ack with ack
// Keep ALIVE
                            InSim.Decoder.TINY tiny = new InSim.Decoder.TINY(recvPacket);
// Keep alive connection
                            if (tiny.SubT == "TINY_NONE")
                            {
                                byte[] stiny = InSim.Encoder.TINY_NONE();
                                insimConnection.Send(stiny, stiny.Length);
                            }
                            break;

                        case "RST":

                            InSim.Decoder.RST rst = new InSim.Decoder.RST(recvPacket);
                            if (debugmode)
                                Console.WriteLine("Race STart : Lap " + rst.RaceLaps + " Qual :" + rst.QualMins);

/* Décalé sur REO à cause de posGrid */
                        // If in Race and new restart, Generate Stat ( For live lfsStat )
                        // On End of Race, générate Statistics
                            if (InRace == true)
							{
								InRace = false;
                                Console.WriteLine("End OF RACE by Race STart");
#if WINDOWS
								Console.Title = consoleTitle;
#endif
								switch (exportOnRaceSTart)
								{
									case "yes":
										if (askForFileNameOnRST)
										{
											Console.Write("> Enter name of stats: ");
											exportRaceStats(Console.ReadLine());
										}
										else
										{
											exportRaceStats(generateFileName());
										}
										break;

									case "ask":
										Console.Write("> Export Stats? [yes/no]: ");
										string answer = Console.ReadLine();

										if (answer.Equals("yes", StringComparison.OrdinalIgnoreCase)
											|| answer.Equals("y", StringComparison.OrdinalIgnoreCase))
										{
											if (askForFileNameOnRST)
											{
												Console.Write("> Enter name of stats: ");
												exportRaceStats(Console.ReadLine());
											}
											else
											{
												exportRaceStats(generateFileName());
											}
										}
										break;

									case "no":
										break;
								}
                            }
/**/
                            // On End of Race, générate Statistics
                            if (InQual == true)
							{
								InQual = false;
                                Console.WriteLine("End OF Qual by Race STart");
#if WINDOWS
                                Console.Title = consoleTitle;
#endif
								switch (exportOnRaceSTart)
								{
									case "yes":
										if (askForFileNameOnRST)
										{
											Console.Write("> Enter name of stats: ");
											exportQualStats(Console.ReadLine());
										}
										else
										{
											exportQualStats(generateFileName());
										}
										break;

									case "ask":
										Console.Write("> Export Stats? [yes/no]: ");
										string answer = Console.ReadLine();

										if (answer.Equals("yes", StringComparison.OrdinalIgnoreCase)
											|| answer.Equals("y", StringComparison.OrdinalIgnoreCase))
										{
											if (askForFileNameOnRST)
											{
												Console.Write("> Enter name of stats: ");
												exportQualStats(Console.ReadLine());
											}
											else
											{
												exportQualStats(generateFileName());
											}
										}
										break;

									case "no":
										break;
								}
                            }

                            Console.WriteLine("Race STart");
#if WINDOWS
                            Console.Title = "Lap : 1";
#endif
                            if (rst.RaceLaps != 0)
                            {
                                InRace = true;
                                InQual = false;
                            }
                            if (rst.QualMins != 0)
                            {
                                InQual = true;
                                InRace = false;
                            }
                            raceStat.Clear();
                            UCIDToUN.Clear();
                            PLIDToUCID.Clear();
// Get All Connections
                            byte[] rstncn = InSim.Encoder.NCN();
                            insimConnection.Send(rstncn, rstncn.Length);
                            // Get All Players
                            byte[] rstnpl = InSim.Encoder.NPL();
                            insimConnection.Send(rstnpl, rstnpl.Length);
// Get Reo
                            byte[] nplreo = InSim.Encoder.REO();
                            insimConnection.Send(nplreo, nplreo.Length);

                            break;

                        case "REN":
                            if(debugmode)Console.WriteLine("Race ENd (return to entry screen)");
                            break;

                        case "NCN":
                            InSim.Decoder.NCN newConnection = new InSim.Decoder.NCN(recvPacket);
                            
                            if (debugmode) Console.WriteLine(string.Format("New ConN Username:{0} Nickname:{1} ConnectionNumber:{2}",newConnection.userName,newConnection.nickName,newConnection.UCID));

//                            Console.WriteLine( newConnection.UCID.ToString() + " ConN Username <" + newConnection.userName + ">  nickName <" + newConnection.nickName  + ">" );
// On reconnect player, RAZ infos et restart
                            UCIDToUN[newConnection.UCID] = new UN(newConnection.userName, newConnection.nickName);
                            if (newConnection.UCID == 0)
                                currInfoRace.HName = newConnection.nickName;
                            break;

                        case "CNL":
                            if(debugmode)Console.WriteLine("ConN Leave (end connection is moved down into this slot)");
                            InSim.Decoder.CNL lostConnection = new InSim.Decoder.CNL(recvPacket);
                            if (debugmode) Console.WriteLine(string.Format("Username:{0} Nickname:{1} ConnectionNumber:{2}",
                                    (UCIDToUN[lostConnection.UCID] as UN ).userName,
                                    (UCIDToUN[lostConnection.UCID] as UN).nickName, 
                                    lostConnection.UCID)
                            );
                            break;

                        case "NPL":
                            int LastPosGrid;
                            if(debugmode)Console.WriteLine("New PLayer joining race (if number already exists, then leaving pits)");
                            InSim.Decoder.NPL newPlayer = new InSim.Decoder.NPL(recvPacket);
                            int removePLID = PLIDbyNickName(newPlayer.nickName);
//                            Console.WriteLine("Nick:" + newPlayer.nickName);
//                            Console.WriteLine("Old PLID:" + removePLID + " New : " + newPlayer.PLID);
                            LastPosGrid = 0;
                            if (InRace == true)
                            {
                                if (removePLID > 0)
                                {
                                    if ((raceStat[removePLID] as raceStats).finished == false)
                                    {
                                        LastPosGrid = (raceStat[removePLID] as raceStats).gridPos;
                                        raceStat.Remove(removePLID);
                                        PLIDToUCID.Remove(removePLID);
                                    }
                                    else
                                    {
                                        if (newPlayer.PLID != removePLID)
                                        {
                                            raceStat[newPlayer.PLID] = raceStat[removePLID];
                                            (raceStat[newPlayer.PLID] as raceStats).PLID = newPlayer.PLID;
                                            raceStat.Remove(removePLID);
                                            PLIDToUCID.Remove(removePLID);
                                        }
                                    }
                                }
                            }
                            if (InQual == true)
                            {
                                if (removePLID > 0)
                                {
                                    if (newPlayer.PLID != removePLID)
                                    {
                                        raceStat[newPlayer.PLID] = raceStat[removePLID];
                                        (raceStat[newPlayer.PLID] as raceStats).PLID = newPlayer.PLID;
                                        raceStat.Remove(removePLID);
                                        PLIDToUCID.Remove(removePLID);
                                    }
                                }
                            }

                            PLIDToUCID[newPlayer.PLID] = newPlayer.UCID;
                            string nplUserName = UserNameByUCID( newPlayer.UCID);

                            if (!raceStat.ContainsKey(newPlayer.PLID))
                            {
                                raceStat[newPlayer.PLID] = new raceStats ( newPlayer.UCID , newPlayer.PLID  );
                                if( LastPosGrid != 0 )
                                    (raceStat[newPlayer.PLID] as raceStats).gridPos = LastPosGrid;
                            }
                            (raceStat[newPlayer.PLID] as raceStats).UCID = newPlayer.UCID;
                            (raceStat[newPlayer.PLID] as raceStats).userName = nplUserName;
                            (raceStat[newPlayer.PLID] as raceStats).nickName = newPlayer.nickName;
                            (raceStat[newPlayer.PLID] as raceStats).allUN[nplUserName] = new UN(nplUserName, newPlayer.nickName);
                            (raceStat[newPlayer.PLID] as raceStats).CName = newPlayer.CName;
                            (raceStat[newPlayer.PLID] as raceStats).Plate = newPlayer.Plate;
                            if( InQual )
                                (raceStat[newPlayer.PLID] as raceStats).finPLID = newPlayer.PLID;

                            string addInfo = "";
                            for (int i = 0; i < 14; i++)
                            {
                                if (((uint)(newPlayer.Flags) & (uint)(1 << i)) != 0)
                                    addInfo += ((InSim.Decoder.NPL.PlayerFlags)(1 << i)).ToString() + " ";
                            }
                            (raceStat[newPlayer.PLID] as raceStats).sFlags = addInfo;
                            break;
							
                        case "PLP":
                            if(debugmode)Console.WriteLine("PLayer Pits (go to settings - stays in player list)");

                            InSim.Decoder.PLP plp = new InSim.Decoder.PLP(recvPacket);
                            break;

                        case "PLL":
                            if(debugmode)Console.WriteLine("PLayer Leave race (spectate - leaves player list, all are shunted down)");

                            InSim.Decoder.PLL pll = new InSim.Decoder.PLL(recvPacket);
                            break;

                        case "CPR":
                            InSim.Decoder.CPR cpr = new InSim.Decoder.CPR(recvPacket);

                            (UCIDToUN[cpr.UCID] as UN).nickName = cpr.newNickName;
                            if (UCIDToPLID(cpr.UCID) != -1)
                                (raceStat[UCIDToPLID(cpr.UCID)] as raceStats).nickName = cpr.newNickName;
                            if (debugmode) Console.WriteLine(string.Format("Conn Player {0} Rename from {1} to {2}, id:{3}", (UCIDToUN[cpr.UCID] as UN).userName, (UCIDToUN[cpr.UCID] as UN).nickName, cpr.newNickName, cpr.UCID));
                            break;

                        case "CLR":
                            if(debugmode)Console.WriteLine("CLear Race - all players removed from race in one go");

                            raceStat.Clear();

                            break;
                        case "PIT": // NEW PIT : (pit stop)
                            InSim.Decoder.PIT pitDec = new InSim.Decoder.PIT(recvPacket);
                            string pitNickName = (UCIDToUN[(int)PLIDToUCID[pitDec.PLID]] as UN).nickName;
                            string pitUserName = (UCIDToUN[(int)PLIDToUCID[pitDec.PLID]] as UN).userName;
                            (raceStat[pitDec.PLID] as raceStats).updatePIT(pitDec);
                            if (debugmode) Console.WriteLine("PIT OF " + (UCIDToUN[pitDec.PLID] as UN).nickName);
//                            Console.WriteLine("PIT OF >" + pitNickName + "< >" + pitUserName + "<");

                            break;
                        case "PSF": // NEW PSF : (pit stop finished)
                            InSim.Decoder.PSF pitFin = new InSim.Decoder.PSF(recvPacket);
                            (raceStat[pitFin.PLID] as raceStats).updatePSF(pitFin.PLID, pitFin.STime );
                            break;

                        case "LAP":

                            InSim.Decoder.LAP lapDec = new InSim.Decoder.LAP(recvPacket);
                            string lapNickName = (UCIDToUN[(int)PLIDToUCID[lapDec.PLID]] as UN).nickName;
                            string lapUserName = (UCIDToUN[(int)PLIDToUCID[lapDec.PLID]] as UN).userName;
                            if (debugmode) Console.WriteLine("LAP time " + lapUserName);
// In qual use RES instead of Lap
                            if (InQual == true)
                            {
                                break;
                            }
                           (raceStat[lapDec.PLID] as raceStats).UpdateLap((lapDec.LTime), (raceStat[lapDec.PLID] as raceStats).numStop, lapDec.LapsDone );
                           if (flagShowInfo)
                               Console.WriteLine("Lap" + lapDec.LapsDone + " " + (raceStat[lapDec.PLID] as raceStats).userName + " -> " + raceStats.LfstimeToString(lapDec.LTime));
                            if (lapDec.LapsDone > currInfoRace.currLap)
                            {
                                currInfoRace.currLap = lapDec.LapsDone;
#if WINDOWS
                                Console.Title = "Lap : " + (lapDec.LapsDone +1).ToString();
#endif
                            }
                            break;

//                        case "SP1":
//                        case "SP2":
//                        case "SP3":
                        case "SPX":
                            InSim.Decoder.SPX splitdec = new InSim.Decoder.SPX(recvPacket);
// Assign temporary nickname to Player ID
                            if (splitdec.STime == timeConv.HMSToLong(60, 0, 0))
                                break;
                            (raceStat[splitdec.PLID] as raceStats).UpdateSplit(splitdec.Split, splitdec.STime);
                            if( flagShowInfo )
                                Console.WriteLine("SP" + splitdec.Split + " " + (raceStat[splitdec.PLID] as raceStats).userName + " = " +  raceStats.LfstimeToString(splitdec.STime ));
                            if (splitdec.Split == 1)
                            {
                                if( currInfoRace.maxSplit < 1 )
                                    currInfoRace.maxSplit = 1;
                            }
                            if (splitdec.Split == 2)
                            {
                                if (currInfoRace.maxSplit < 2)
                                    currInfoRace.maxSplit = 2;
                            }
                            if (splitdec.Split == 3)
                            {
                                if (currInfoRace.maxSplit < 3)
                                    currInfoRace.maxSplit = 3;
                            }
                            break;

                        case "RES":
                            InSim.Decoder.RES result = new InSim.Decoder.RES(recvPacket);
                            if (debugmode) Console.WriteLine("RESult (qualify or finish)");
                            if (InRace == true)
                            {
                                if (flagShowInfo)
                                    Console.WriteLine("RES " + result.ResultNum.ToString() + ":" + exportstats.lfsStripColor(result.nickName));
                            }
                            int lplid = result.PLID;
                            if (result.PLID == 0)
                                lplid = PLIDbyNickName(result.nickName);
// Retreive current PLID with the Finish PLID, in case of player spectate and rejoin race after finish
                            lplid = PLIDbyFinPLID(lplid);
                            if (lplid != -1 && InRace == true)
                                (raceStat[lplid] as raceStats).UpdateResult(result.TTime, result.ResultNum, result.CName, result.Confirm, result.NumStops);
                            if (lplid != -1 && InQual == true)
                            {
                                if (flagShowInfo)
                                    Console.WriteLine("Lap " + (raceStat[lplid] as raceStats).userName + " -> " + raceStats.LfstimeToString(result.BTime));
                                (raceStat[lplid] as raceStats).UpdateQualResult(result.TTime,
                                                                                result.BTime,
                                                                                result.NumStops,
                                                                                result.Confirm,
                                                                                result.LapDone,
                                                                                result.ResultNum,
                                                                                result.NumRes
                                );
                            }

                            break;
                        case "FIN": // New Insim Packet
                            InSim.Decoder.FIN fin = new InSim.Decoder.FIN(recvPacket);
//                            if (debugmode) 
                                Console.WriteLine("FINISH (qualify or finish)");
                            (raceStat[fin.PLID] as raceStats).finished = true;
                            (raceStat[fin.PLID] as raceStats).finPLID = fin.PLID;
                            break;
                        case "REO":
                            InSim.Decoder.REO pacreo = new InSim.Decoder.REO(recvPacket);
                            if (debugmode) 
                                 Console.WriteLine("REOrder (when race restarts after qualifying)");

                            if (pacreo.ReqI != 0) // Ignore and get Only if requested
                            {
                                for (int i = 0; i < pacreo.NumP; i++)
                                {
                                    int PLID = (int)pacreo.PLID[i];
                                    if (!raceStat.ContainsKey(PLID))
                                    {
                                        raceStat[PLID] = new raceStats(0, PLID);
                                    }
                                    (raceStat[PLID] as raceStats).gridPos = i + 1;
                                }
                            }
                            break;

                        case "STA":
//                            Console.WriteLine("STATE");
                            if (debugmode) Console.WriteLine("STAte");
                            InSim.Decoder.STA state = new InSim.Decoder.STA(recvPacket);
                            if (flagFirst == true && (state.Flags & 512) == 512)
                            {
                                flagFirst = false;
                                // Get All Connections
                                byte[] ncn = InSim.Encoder.NCN();
                                insimConnection.Send(ncn, ncn.Length);
                                // Get All Players
                                byte[] npl = InSim.Encoder.NPL();
                                insimConnection.Send(npl, npl.Length);
                            }
                            currInfoRace.currentTrackName = state.ShortTrackName;
                            currInfoRace.weather = state.Weather;
                            currInfoRace.wind = state.Wind;
                            currInfoRace.raceLaps = state.RaceLaps;
                            currInfoRace.qualMins = state.QualMins;
                            if (currInfoRace.raceLaps == 0)
                                currInfoRace.sraceLaps = "Practice";
                            else if (currInfoRace.raceLaps < 100)
                                currInfoRace.sraceLaps = currInfoRace.raceLaps.ToString();
                            else if (currInfoRace.raceLaps < 191)
                                currInfoRace.sraceLaps = ((currInfoRace.raceLaps - 100) * 10 + 100).ToString();
                            else if (currInfoRace.raceLaps < 239)
                                currInfoRace.sraceLaps = (currInfoRace.raceLaps - 190).ToString();

                            if (debugmode) Console.WriteLine("Current track:" + currInfoRace.currentTrackName);
                            // On End of Race, générate Statistics
                            if (state.RaceInProg == 0 && InRace == true)
                            {
                                InRace = false;
                                Console.WriteLine("End OF RACE by STAte");
#if WINDOWS
                                Console.Title = consoleTitle;
#endif
								switch (exportOnSTAte)
								{
									case "yes":
										if (askForFileNameOnSTA)
										{
											Console.Write("> Enter name of stats: ");
											exportRaceStats(Console.ReadLine());
										}
										else
										{
											exportRaceStats(generateFileName());
										}
										break;

									case "ask":
										Console.Write("> Export Stats? [yes/no]: ");
										string answer = Console.ReadLine();

										if (answer.Equals("yes", StringComparison.OrdinalIgnoreCase)
											|| answer.Equals("y", StringComparison.OrdinalIgnoreCase))
										{
											if (askForFileNameOnSTA)
											{
												Console.Write("> Enter name of stats: ");
												exportRaceStats(Console.ReadLine());
											}
											else
											{
												exportRaceStats(generateFileName());
											}
										}
										break;

									case "no":
										break;
								}
                            }
                            // On End of Race, générate Statistics
                            if (state.RaceInProg == 0 && InQual == true)
                            {
                                InQual = false;
                                Console.WriteLine("End of Qual by STAte");
#if WINDOWS
                                Console.Title = consoleTitle;
#endif
								switch (exportOnSTAte)
								{
									case "yes":
										if (askForFileNameOnSTA)
										{
											Console.Write("> Enter name of stats: ");
											exportQualStats(Console.ReadLine());
										}
										else
										{
											exportQualStats(generateFileName());
										}
										break;

									case "ask":
										Console.Write("> Export Stats? [yes/no]: ");
										string answer = Console.ReadLine();

										if (answer.Equals("yes", StringComparison.OrdinalIgnoreCase)
											|| answer.Equals("y", StringComparison.OrdinalIgnoreCase))
										{
											if (askForFileNameOnSTA)
											{
												Console.Write("> Enter name of stats: ");
												exportQualStats(Console.ReadLine());
											}
											else
											{
												exportQualStats(generateFileName());
											}
										}
										break;

									case "no":
										break;
								}
                            }
                            break;
/*
                        case "MSS":
                            InSim.Decoder.MSS mss = new InSim.Decoder.MSS(recvPacket);
                            if (debugmode) Console.WriteLine("Message received Username:{0} Player name:{1} Message:{2}",mss.UserName,mss.PlayerName,mss.Message);
                            string nicknamewithoutcolors = mss.PlayerName;

                            ackReq = false;
                            break;
*/

                        case "MSO":
                            InSim.Decoder.MSO msg = new InSim.Decoder.MSO(recvPacket);
                            if (debugmode) Console.WriteLine("Message received:" + msg.message);

                            if (msg.message.StartsWith("!ver"))
                                Ver(msg.PLID);
                            if (msg.UserType == 1)
                            {
								string schat = exportstats.lfsColorToHtml(NickNameByUCID(msg.UCID)) + ":" + exportstats.lfsColorToHtml(exportstats.lfsChatTextToHtml(msg.message));
                                currInfoRace.chat.Add(schat);
                            }
                            break;
                        case "ISM":
                            InSim.Decoder.ISM multi = new InSim.Decoder.ISM(recvPacket);
                            if (debugmode) Console.WriteLine("Insim Multi:" + multi.HName);
                            // Get All Connections
                            byte[] ismncn = InSim.Encoder.NCN();
                            insimConnection.Send(ismncn, ismncn.Length);
                            // Get All Players
                            byte[] ismnpl = InSim.Encoder.NPL();
                            insimConnection.Send(ismnpl, ismnpl.Length);
                            break;

                        case "MCI":

                            InSim.Decoder.MCI mci = new InSim.Decoder.MCI(recvPacket );
                            for (int i = 0; i < System.Math.Min(8, mci.numOfPlayers); i++)
                            {
                                if (raceStat.ContainsKey(mci.compCar[i].PLID))
                                {
                                    (raceStat[mci.compCar[i].PLID] as raceStats).updateMCI(mci.compCar[i].speed);
                                }
                            }
                            break;
                        case "FLG": // NEW FLG : (yellow or blue flags)
                            InSim.Decoder.FLG flg = new InSim.Decoder.FLG(recvPacket);
                            if (flg.OffOn == 1 && flg.Flag == 1 && (raceStat[flg.PLID] as raceStats).inBlue == false)
                            { // Blue flag On
                                (raceStat[flg.PLID] as raceStats).blueFlags++;
                                (raceStat[flg.PLID] as raceStats).inBlue = true;
                            }
                            if (flg.OffOn == 1 && flg.Flag == 2 && (raceStat[flg.PLID] as raceStats).inYellow == false)
                            { // Yellow flag On
                                (raceStat[flg.PLID] as raceStats).yellowFlags++;
                                (raceStat[flg.PLID] as raceStats).inYellow = true;
                            }
                            if (flg.OffOn == 0 && flg.Flag == 1)
                            { // Blue flag Off
                                (raceStat[flg.PLID] as raceStats).inBlue = false;
                            }
                            if (flg.OffOn == 0 && flg.Flag == 2)
                            { // Yellow flag Off
                                (raceStat[flg.PLID] as raceStats).inYellow = false;
                            }
                            break;
                        case "PEN": // NEW PEN : (penalty)
                            InSim.Decoder.PEN pen = new InSim.Decoder.PEN(recvPacket);
                            (raceStat[pen.PLID] as raceStats).UpdatePen(pen.OldPen,pen.NewPen,pen.Reason);
                            break;
                        case "NLP":
                            break;
                        case "VTC":
                            break;
                        case "VTN": // Insim Vote
                            break;
                        case "VTA": // Insim Vote Action
                            break;
                        case "III": // NEW III : InSimInfo
                            break;
                        case "PLA": // NEW PLA : (Pit LAne)
                            break;
                        case "TOC": // NEW TOC : (take over car)
                            #region TOC
                            InSim.Decoder.TOC toc = new InSim.Decoder.TOC(recvPacket);
                            if ( !raceStat.ContainsKey(toc.PLID) )
                            {
                                break;
                            }
                            currInfoRace.isToc = true;


                            int OldPLID = UCIDToPLID(toc.NewUCID);
                            PLIDToUCID.Remove(OldPLID);
                            raceStat.Remove(OldPLID);

                            string oldUserName = (raceStat[toc.PLID] as raceStats).userName;
                            string oldNickName = (raceStat[toc.PLID] as raceStats).nickName;
                            (raceStat[toc.PLID] as raceStats).nickName = (UCIDToUN[toc.NewUCID] as UN).nickName;
                            (raceStat[toc.PLID] as raceStats).userName = (UCIDToUN[toc.NewUCID] as UN).userName;
                            (raceStat[toc.PLID] as raceStats).allUN[(UCIDToUN[toc.NewUCID] as UN).userName] = new UN( (UCIDToUN[toc.NewUCID] as UN).userName, (UCIDToUN[toc.NewUCID] as UN).nickName );
                            string newUserName = (raceStat[toc.PLID] as raceStats).userName;
                            string newNickName = (raceStat[toc.PLID] as raceStats).nickName;
                            (raceStat[toc.PLID] as raceStats).updateTOC(oldNickName, oldUserName, newNickName, newUserName);
                            Console.WriteLine("<<<--------------------- TOC: " + oldUserName + "-->" + newUserName + "--------------------->>>");

                            PLIDToUCID[toc.PLID] = toc.NewUCID;
                            #endregion
                            break;
/*
                        case "TOC": // NEW TOC : (take over car)
                            if (debugmode) Console.WriteLine("Take over CVar");
                            break;
*/
                        case "PFL": // NEW PFL : (player help flags)
                            break;
                        case "CCH": // NEW CCH : (camera changed)
                            break;
                        case "AXI": // send an IS_AXI
                            break;  // autocross cleared
                        case "AXC":
                            break;

                        default:
                            if (debugmode) Console.WriteLine("Unknown packet received:" + packetHead);
//                            Console.WriteLine("Unknown packet received:" + packetHead);
                            break;
                    }
// Not Used in insim 4
/*
                    if (ackReq)
                    {
                        byte[] ackPack = InSim.Encoder.ACK(verifyID);
                        insimConnection.Send(ackPack, ackPack.Length);
                    }
 */
                }
            }

        }

		/// <summary>
		/// Exports qualification statistics
		/// </summary>
		/// <param name="datFile">Name of statistics</param>
		private void exportQualStats(string datFile)
		{
			System.IO.Directory.CreateDirectory(@qualDir);
			exportstats.qualhtmlResult(raceStat, datFile, qualDir, currInfoRace, maxTimeQualIgnore);
			exportstats.chatResult(raceStat, datFile, qualDir, currInfoRace, "qual");
			currInfoRace.maxSplit = 0;
			currInfoRace.currLap = 0;
			currInfoRace.chat.Clear();
			Console.WriteLine("> Qualification Stats exported <");
		}
		/// <summary>
		/// Exports race statistics
		/// </summary>
		/// <param name="datFile">Name of statistics</param>
		private void exportRaceStats(string datFile)
		{
			System.IO.Directory.CreateDirectory(@raceDir);
			exportstats.csvResult(raceStat, datFile, raceDir, currInfoRace);
			exportstats.tsvResult(raceStat, datFile, raceDir, currInfoRace);
			exportstats.htmlResult(raceStat, datFile, raceDir, currInfoRace);
			exportstats.chatResult(raceStat, datFile, raceDir, currInfoRace, "race");
			exportstats.lblResult(raceStat, datFile, raceDir, currInfoRace);
			currInfoRace.maxSplit = 0;
			currInfoRace.currLap = 0;
			currInfoRace.chat.Clear();
			Console.WriteLine("> Race Stats exported <");

			// Call graph generator
			switch (generateGraphs)
			{
				case "dll":
					Graph.Graph.GenerateGraph();
					Console.WriteLine("> Graph.dll graphs generated <");
					break;
				case "exe":
					Process p = Process.Start("Graph.exe");
					Console.WriteLine("> Graph.exe graphs generated <");
					break;

				default:
					break;
			}
		}
    }
}
