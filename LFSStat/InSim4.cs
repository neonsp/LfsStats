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



using System;
namespace InSim
{
    enum bfn 
{
	BFN_DEL_BTN,		//  0 - instruction     : delete one button (must set ClickID)
	BFN_CLEAR,			//  1 - instruction		: clear all buttons made by this insim instance
	BFN_USER_CLEAR,		//  2 - info            : user cleared this insim instance's buttons
	BFN_REQUEST,		//  3 - user request    : SHIFT+B or SHIFT+I - request for buttons
};

    enum pen
    {
        PENALTY_NONE,		// 0		
        PENALTY_DT,			// 1
        PENALTY_DT_VALID,	// 2
        PENALTY_SG,			// 3
        PENALTY_SG_VALID,	// 4
        PENALTY_30,			// 5
        PENALTY_45,			// 6
        PENALTY_NUM
    }
    enum penr
    {
	    PENR_UNKNOWN,		// 0 - unknown or cleared penalty
	    PENR_ADMIN,			// 1 - penalty given by admin
	    PENR_WRONG_WAY,		// 2 - wrong way driving
	    PENR_FALSE_START,	// 3 - starting before green light
	    PENR_SPEEDING,		// 4 - speeding in pit lane
	    PENR_STOP_SHORT,	// 5 - stop-go pit stop too short
	    PENR_STOP_LATE,		// 6 - compulsory stop is too late
	    PENR_NUM
    };

    enum BTN_style
    {
        ISB_C1 = 1,
        ISB_C2 = 2,
        ISB_C4 = 4,
        ISB_CLICK = 8,
        ISB_LIGHT = 16,
        ISB_DARK = 32,
        ISB_LEFT = 64,
        ISB_RIGHT = 128
    }
    enum PIT_work
    {
	    PSE_NOTHING = 0,		// bit 0 (1)
	    PSE_STOP = 2,			// bit 1 (2)
	    PSE_FR_DAM = 4,			// bit 2 (4)
	    PSE_FR_WHL = 8,			// etc...
	    PSE_LE_FR_DAM = 16,
	    PSE_LE_FR_WHL = 32,
	    PSE_RI_FR_DAM = 64,
	    PSE_RI_FR_WHL = 128,
	    PSE_RE_DAM = 256,
	    PSE_RE_WHL = 512,
	    PSE_LE_RE_DAM = 1024,
	    PSE_LE_RE_WHL = 2048,
	    PSE_RI_RE_DAM = 4096,
	    PSE_RI_RE_WHL = 8192,
	    PSE_BODY_MINOR = 16384,
	    PSE_BODY_MAJOR = 32768,
	    PSE_SETUP = 65536,
	    PSE_REFUEL = 131072,
    };

    enum confirm
    {
        CONF_MENTIONED = 1,
        CONF_CONFIRMED = 2,
        CONF_PENALTY_DT = 4,
        CONF_PENALTY_SG = 8,
        CONF_PENALTY_30 = 16,
        CONF_PENALTY_45 = 32,
        CONF_DID_NOT_PIT = 64
    }

    enum ISF
    {
        RES_0 = 1,
        RES_1 = 2,	// bit 1 : spare
        LOCAL = 4,	// bit 2 : spare
        MSO_COLS = 8,	// bit 3 : spare 
        NLP = 16,	// bit 4 : set to receive NLP packets
        MCI =32	// bit 5 : set to receive MCI packets
    }
    enum TypePack
    {
	    ISP_NONE,		//  0					: not used
	    ISP_ISI,		//  1 - instruction		: insim initialise
	    ISP_VER,		//  2 - info			: version info
	    ISP_TINY,		//  3 - both ways		: multi purpose
	    ISP_SMALL,		//  4 - both ways		: multi purpose
	    ISP_STA,		//  5 - info			: state info
	    ISP_SCH,		//  6 - instruction		: single character
	    ISP_SFP,		//  7 - instruction		: state flags pack
	    ISP_SCC,		//  8 - instruction		: set car camera
	    ISP_CPP,		//  9 - both ways		: cam pos pack
	    ISP_ISM,		// 10 - info			: start multiplayer
	    ISP_MSO,		// 11 - info			: message out
	    ISP_III,		// 12 - info			: hidden /i message
	    ISP_MST,		// 13 - instruction		: type message or /command
	    ISP_MTC,		// 14 - instruction		: message to a connection
	    ISP_MOD,		// 15 - instruction		: set screen mode
	    ISP_VTN,		// 16 - info			: vote notification
	    ISP_RST,		// 17 - info			: new connection
	    ISP_NCN,		// 18 - info			: new connection
	    ISP_CNL,		// 19 - info			: connection left
	    ISP_CPR,		// 20 - info			: connection renamed
	    ISP_NPL,		// 21 - info			: new player (joined race)
	    ISP_PLP,		// 22 - info			: player pit (keeps slot in race)
	    ISP_PLL,		// 23 - info			: player leave (spectate - loses slot)
	    ISP_LAP,		// 24 - info			: lap time
	    ISP_SPX,		// 25 - info			: split x time
	    ISP_PIT,		// 26 - info			: pit stop start
	    ISP_PSF,		// 27 - info			: pit stop finish
	    ISP_PLA,		// 28 - info			: pit lane enter / leave
	    ISP_CCH,		// 29 - info			: camera changed
	    ISP_PEN,		// 30 - info			: penalty given or cleared
	    ISP_TOC,		// 31 - info			: take over car
	    ISP_FLG,		// 32 - info			: flag (yellow or blue)
	    ISP_PFL,		// 33 - info			: player flags (help flags)
	    ISP_FIN,		// 34 - info			: finished race
	    ISP_RES,		// 35 - info			: result confirmed
	    ISP_REO,		// 36 - both ways		: reorder (info or instruction)
	    ISP_NLP,		// 37 - info			: node and lap packet
	    ISP_MCI,		// 38 - info			: multi car info
        ISP_MSX,		// 39 - instruction		: type message
        ISP_MSL,		// 40 - instruction		: message to local computer
        ISP_CRS,		// 41 - info			: car reset
        ISP_BFN,		// 42 - both ways		: delete buttons / receive button requests
        ISP_43,			// 43
        ISP_44,			// 44
        ISP_BTN,		// 45 - instruction		: show a button on local or remote screen
        ISP_BTC,		// 46 - info			: sent when a user clicks a button
        ISP_BTT,		// 47 - info			: sent after typing into a button

    };
    enum TypeTiny
    {
	    TINY_NONE,		//  0					: see "maintaining the connection"
	    TINY_VER,		//  1 - info request	: get version
	    TINY_CLOSE,		//  2 - instruction		: close insim
	    TINY_PING,		//  3 - ping request	: external progam requesting a reply
	    TINY_REPLY,		//  4 - ping reply		: reply to a ping request
	    TINY_VTC,		//  5 - info			: vote cancelled
	    TINY_SCP,		//  6 - info request	: send camera pos
	    TINY_SST,		//  7 - info request	: send state info
	    TINY_GTH,		//  8 - info request	: get time in hundredths (i.e. SMALL_RTP)
	    TINY_MPE,		//  9 - info			: multi player end
	    TINY_ISM,		// 10 - info request	: get multiplayer info (i.e. ISP_ISM)
	    TINY_REN,		// 11 - info			: race end (return to game setup screen)
	    TINY_CLR,		// 12 - info			: all players cleared from race
	    TINY_NCN,		// 13 - info			: get all connections
	    TINY_NPL,		// 14 - info			: get all players
	    TINY_RES,		// 15 - info			: get all results
	    TINY_NLP,		// 16 - info request	: send an IS_NLP packet
	    TINY_MCI,		// 17 - info request	: send an IS_MCI packet
	    TINY_REO,		// 18 - info request	: send an IS_REO packet
	    TINY_RST,		// 19 - info request	: send an IS_RST packet
        TINY_AXI,		// 20 - info request	: send an IS_AXI
        TINY_AXC,		// 21 - info			: autocross cleared

    };
    enum TypeSmall // the fourth byte of IS_SMALL packets is one of these
    {
	    SMALL_NONE,		//  0					: not used
	    SMALL_SSP,		//  1 - instruction		: start sending positions
	    SMALL_SSG,		//  2 - instruction		: start sending gauges
	    SMALL_VTA,		//  3 - report			: vote action
	    SMALL_TMS,		//  4 - instruction		: time stop
	    SMALL_STP,		//  5 - instruction		: time step
	    SMALL_RTP,		//  6 - info			: race time packet (reply to GTH)
	    SMALL_NLI,		//  7 - instruction		: set node lap interval
    };


    public class Connect
    {
        public bool connected = false;
        public string Product;
        public string Version;
        public int InSimVersion;
        private System.Net.Sockets.UdpClient uc;
        private TcpConnection.Connection tc;
        private bool TCPmode;
        System.Net.IPEndPoint remoteEP = new System.Net.IPEndPoint(System.Net.IPAddress.Any, 0);

        public Connect()
        {
        }
        public void insimConnect(string host, int port, string adminPassword, string mode, string nameApp, bool isLocal, bool TCPmode )
        {
            this.TCPmode = TCPmode;
            if( this.TCPmode )
                insimConnectTCP(host, port, adminPassword, mode, nameApp, isLocal);
            else
                insimConnectUDP(host, port, adminPassword, mode, nameApp ,isLocal);
        }
        public void insimConnectTCP(string host, int port, string adminPassword, string mode, string nameApp, bool isLocal )
        {
            int nbTry = 0;
            int maxTry = 40;

            tc = new TcpConnection.Connection(host, port);
        retryConnect:
            Console.Write("Connecting");
            try
            {
                tc.Connect();
            }
            catch
            {
                if (nbTry++ > maxTry)
                {
                    Console.WriteLine("");
                    throw;
                }
                else
                {
                    System.Threading.Thread.Sleep(1000);
                    Console.Write(".");
                    goto retryConnect;
                }

            }
            Console.WriteLine("");
            Console.WriteLine("Connection OK");
            byte[] inSimInit = InSim.Encoder.ISI(adminPassword, 0, 0, nameApp,isLocal);
            try
            {
                this.Send(inSimInit, inSimInit.Length);
            }
            catch
            {
                throw;
            }
            byte[] recvPacket;
            recvPacket = this.Receive();
            InSim.Decoder.VER ver = new InSim.Decoder.VER(recvPacket);
            this.connected = true;
            this.Product = ver.Product;
            this.Version = ver.Version;
            this.InSimVersion = ver.InSimVersion;

        }
        public void insimConnectUDP(string host, int port, string adminPassword, string mode,string nameApp,bool isLocal )
        {
            int nbTry = 0;
            int maxTry = 40;

            Console.Write("Connecting");
        retryConnect:
            try
            {
                uc = new System.Net.Sockets.UdpClient(host, port);
            }
            catch
            {
                throw;
            }
#if MONO
            int localport = 0;  //for mono
#else
            int localport = ((System.Net.IPEndPoint)uc.Client.LocalEndPoint).Port;//for .NET
#endif
            byte[] inSimInit = InSim.Encoder.ISI(adminPassword, localport, 0, nameApp,isLocal );
            try
            {
                this.Send(inSimInit, inSimInit.Length);
            }
            catch
            {
                throw;
            }

//request version Not Need in insim 4, because ISI send This
//            byte[] verreq = InSim.Encoder.VER();
//            this.Send(verreq, verreq.Length);
//retr version info
            byte[] recvPacket;
            try
            {
                recvPacket = this.Receive();
            }
            catch
            {
                if (nbTry++ > maxTry)
                {
                    Console.WriteLine("");
                    throw;
                }
                else
                {
                    System.Threading.Thread.Sleep(1000);
                    Console.Write(".");
                    goto retryConnect;
                }
            }
            Console.WriteLine("");
            Console.WriteLine("Connection OK");
            InSim.Decoder.VER ver = new InSim.Decoder.VER(recvPacket);
            this.connected = true;
            this.Product = ver.Product;
            this.Version = ver.Version;
            this.InSimVersion = ver.InSimVersion;
        }
        public void Send(byte[] outMsg, int Length)
        {
            if (this.TCPmode)
                this.SendTCP(outMsg, Length);
            else
                this.SendUDP(outMsg, Length);
        }
        public void SendTCP(byte[] outMsg, int Length)
        {
            tc.SendToServer( outMsg, Length );
        }
        public void SendUDP(byte[] outMsg, int Length)
        {
            uc.Send(outMsg, Length);
        }
        public byte[] Receive()
        {
            if (this.TCPmode)
                return ReceiveTCP();
            else
                return ReceiveUDP();
        }
        public byte[] ReceiveTCP()
        {
            return tc.GetPackFromInsimServer();
        }
        public byte[] ReceiveUDP()
        {
            return uc.Receive(ref remoteEP);
        }
        public void Close()
        {
            if (this.TCPmode)
                CloseTCP();
            else
                CloseUDP();
        }
        public void CloseTCP()
        {
            uc.Close();
        }
        public void CloseUDP()
        {
            uc.Close();
        }
        public string packetHead(byte[] packet)
        {
            string packetHead;
            packetHead = Enum.GetName(typeof(TypePack), packet[1]);
            if (packetHead == null)
                packetHead = "ISP_" + packet[1].ToString();
            packetHead = packetHead.Remove(0, 4);
            return packetHead;
        }
        public uint verifyID(byte[] packet)
        {
            return (uint)0;
        }
    }
    public class Encoder
	{


/// <summary>
		/// Encoder for messages sent to LFS console
		/// </summary>
		/// <param name="msg">Message to send</param>
		/// <returns></returns>
        /// 
        static public byte[] IS_TINY(byte Type, byte ReqI, byte SubT)
        {
            byte[] packet = new byte[4];
            packet[0] = 4;
            packet[1] = Type;
            packet[2] = ReqI;
            packet[3] = SubT;
            return packet;
        }
        static public byte[] IS_SMALL(byte Type, byte ReqI, byte SubT, uint Uval )
        {
            byte[] packet = new byte[8];
            packet[0] = 8;
            packet[1] = Type;
            packet[2] = ReqI;
            packet[3] = SubT;
            packet[4] = (byte)(Uval & 0xff);
            packet[5] = (byte)((Uval >> 8) & 0xff);
            packet[6] = (byte)((Uval >> 16) & 0xff);
            packet[7] = (byte)((Uval >> 24) & 0xff);


            return packet;
        }
// MST OK in Insim 4
        static public byte[] MST(string msg)
		{
			int msgLen = msg.Length>63?63:msg.Length;

			byte[] packet = new byte[68];
            packet[0] = 68;
            packet[1] = (byte)TypePack.ISP_MST;
            packet[2] = 0;
            packet[3] = 0;
            InSim.CodePage.GetBytes(msg, 0, msgLen, packet, 4);
			return packet;
		}
        // MSX OK in Insim 4
        static public byte[] MSX(string msg)
        {
            int msgLen = msg.Length > 96 ? 96 : msg.Length;

            byte[] packet = new byte[100];
            packet[0] = 100;
            packet[1] = (byte)TypePack.ISP_MSX;
            packet[2] = 0;
            packet[3] = 0;
            InSim.CodePage.GetBytes(msg, 0, msgLen, packet, 4);
            return packet;
        }

        // MSP OK in Insim 4
        static public byte[] MSX(byte[] msg)
        {
            byte[] packet = new byte[100];
            packet[0] = 100;
            packet[1] = (byte)TypePack.ISP_MSX;
            packet[2] = 0;
            packet[3] = 0;
            System.Array.Copy(msg, 0, packet, 4, System.Math.Min(96, msg.Length));
            return packet;
        }


// MSP OK in Insim 4
        static public byte[] MST(byte[] msg)
        {
            byte[] packet = new byte[68];
            packet[0] = 68;
            packet[1] = (byte)TypePack.ISP_MST;
            packet[2] = 0;
            packet[3] = 0;
            System.Array.Copy(msg, 0, packet, 4, System.Math.Min(63,msg.Length));
            return packet;
        }
// MTC OK in Insim 4
		static public byte[] MTC(int UCID, int PLID, string msg)
		{
			int msgLen = msg.Length>63?63:msg.Length;

			byte[] packet = new byte[72];
            packet[0] = 72;
            packet[1] = (byte)TypePack.ISP_MTC;
            packet[2] = 0;
            packet[3] = 0;
			packet[4] = (byte)UCID;
			packet[5] = (byte)PLID;
            packet[6] = 0;
            packet[7] = 0;
            InSim.CodePage.GetBytes(msg, 0, msgLen, packet, 8);
            return packet;
		}
// MTC OK in Insim 4
        static public byte[] MTC(int UCID, int PLID, byte[] msg)
        {
            byte[] packet = new byte[72];
            packet[0] = 72;
            packet[1] = (byte)TypePack.ISP_MTC;
            packet[2] = 0;
            packet[3] = 0;
            packet[4] = (byte)UCID;
            packet[5] = (byte)PLID;
            packet[6] = 0;
            packet[7] = 0;
            System.Array.Copy(msg, 0, packet, 8, System.Math.Min(63, msg.Length)); 
            return packet;
        }

        /// <summary>
        /// Request LFS version information
        /// </summary>
        /// <returns></returns>
// VER OK in Insim 4
        static public byte[] TINY_NONE()
        {
            return IS_TINY((byte)TypePack.ISP_TINY, 1, (byte)TypeTiny.TINY_NONE);
        }
        static public byte[] VER()
        {
            return IS_TINY((byte)TypePack.ISP_TINY, 1, (byte)TypeTiny.TINY_VER);
        }
        static public byte[] NCN()
        {
            return IS_TINY((byte)TypePack.ISP_TINY, 1, (byte)TypeTiny.TINY_NCN);
        }
        static public byte[] NPL()
        {
            return IS_TINY((byte)TypePack.ISP_TINY, 1, (byte)TypeTiny.TINY_NPL);
        }
        static public byte[] REO()
        {
            return IS_TINY((byte)TypePack.ISP_TINY, 1, (byte)TypeTiny.TINY_REO);
        }
        /// <summary>
        /// Request LFS Status
        /// </summary>
        /// <returns></returns>
// SST OK in Insim 4
        static public byte[] SST()
        {
            return IS_TINY((byte)TypePack.ISP_TINY, 1, (byte)TypeTiny.TINY_SST);
        }

		/// <summary>
		/// InSim Close Request Encoder 
		/// </summary>
		/// <returns></returns>
// OK in InSim 4
        static public byte[] ISC()
		{
            return IS_TINY((byte)TypePack.ISP_TINY, 1, (byte)TypeTiny.TINY_CLOSE );
		}

        /// <summary>
        /// Sets car state refresh interval
        /// </summary>
        /// <param name="interval">Car state refresh interval in ms. 0 = stop, minimum = 100</param>
        /// <returns></returns>
// OK in Insim 4
        static public byte[] NLI(int interval)
        {
            return IS_SMALL((byte)TypePack.ISP_SMALL, 0, (byte)TypeSmall.SMALL_NLI, (uint)interval);
        }

		/// <summary>
		/// InSim Initialization Request Encoder
		/// </summary>
		/// <param name="adminPass">Administrator password.</param>
        /// <param name="portNum">Port number. 0 for same port as output.</param>
        /// <param name="mciSeconds">Seconds between MCI packets. 0 to disable.</param>
		/// <returns></returns>
// OK in VERSION 4
        static public byte[] ISI(string adminPass, int portNum, byte mciSeconds, string nameApp, bool local)
		{
			byte[] packet = new byte[44];
            packet[0] = 44;
            packet[1] = (byte)TypePack.ISP_ISI;
            packet[2] = 1;
            packet[3] = 0;
// Port
            packet[4] = (byte)(portNum % 256);  // LSB
            packet[5] = (byte)(portNum / 256);  // MSB

// Flags
            if (local)
                packet[6] = (byte)ISF.MCI | (byte)ISF.LOCAL;    // LSB
            else
    			packet[6] = ( byte )ISF.MCI;    // LSB
            packet[7] = 0;          // MSB

            packet[8] = 0;

            packet[9] = 0;

// Number of milli seconds between NLP or MCI packets (0=none) 
            packet[10] = 100;   // LSB
            packet[11] = 0;     // MSB

			System.Text.Encoding.ASCII.GetBytes(adminPass,0,adminPass.Length,packet,12);
			System.Text.Encoding.ASCII.GetBytes(nameApp,0,nameApp.Length,packet,28);

			return packet;
		}
        static public byte[] BFN(int SubT, int UCID, int ClickID)
        {
            byte[] packet = new byte[8];
            packet[0] = 8;
            packet[1] = (byte)TypePack.ISP_BFN;
            packet[2] = 0;
            packet[3] = (byte)SubT;
            packet[4] = (byte)UCID;
            packet[5] = (byte)ClickID;
            packet[6] = 0;
            packet[7] = 0;
            return packet;
        }
        static public byte[] BTN(int L, int T, int W, int H, int UCID,int ClickID,int style, string Caption, string Text)
        {
            int poke = Caption.Length;
            string allText;
            if( poke > 0 )
                allText = " " + Caption + " " + Text;
            else
                allText = Text;

            while (allText.Length % 4 != 0)
                allText += " ";
            if (allText.Length > 240)
                allText = allText.Substring(0, 240);

            byte[] packet = new byte[12+allText.Length];
            packet[0] = (byte)(12 + allText.Length);
            packet[1] = (byte)TypePack.ISP_BTN;
            packet[2] = 1;              // ReqI
            packet[3] = (byte)UCID;     // UCID
            packet[4] = (byte)ClickID;  // ClickID Of Button
            packet[5] = 0;              // Inst
            packet[6] = (byte)style;              // BUtton Style
            packet[7] = 0;              // TypeIn
            packet[8] = (byte)L; // Left
            packet[9] = (byte)T; // Top
            packet[10] = (byte)W; // Largeur
            packet[11] = (byte)H; // Largeur
            System.Text.Encoding.ASCII.GetBytes(allText, 0, allText.Length, packet, 12);

            if (poke != 0)
            {
                packet[12] = 0;
                packet[12 + poke] = 0;
            }


            return packet;
        }

	}

	public class Decoder
	{
		/// <summary>
		/// Version packet decoder
		/// </summary>
        public static string getLongTrackName(string shortTrackName)
        {
            string retValue = "";

            switch (shortTrackName.Substring(0, 3))
            {
                case "AS1":
                    retValue = "Aston Cadet";
                    break;
                case "AS2":
                    retValue = "Aston Club";
                    break;
                case "AS3":
                    retValue = "Aston National";
                    break;
                case "AS4":
                    retValue = "Aston Historic";
                    break;
                case "AS5":
                    retValue = "Aston GP";
                    break;
                case "AS6":
                    retValue = "Aston Grand Touring";
                    break;
                case "AS7":
                    retValue = "Aston North";
                    break;
                case "BL1":
                    retValue = "Blackwood GP";
                    break;
                case "BL2":
                    retValue = "Blackwood Rallycross";
                    break;
                case "BL3":
                    retValue = "Car Park";
                    break;
                case "FE1":
                    retValue = "Fern Bay Club";
                    break;
                case "FE2":
                    retValue = "Fern Bay Green";
                    break;
                case "FE3":
                    retValue = "Fern Bay Gold";
                    break;
                case "FE4":
                    retValue = "Fern Bay Black";
                    break;
                case "FE5":
                    retValue = "Fern Bay Rallycross";
                    break;
                case "FE6":
                    retValue = "Fern Bay RallyX Green";
                    break;
                case "KY1":
                    retValue = "Kyoto Oval";
                    break;
                case "KY2":
                    retValue = "Kyoto National";
                    break;
                case "KY3":
                    retValue = "Kyoto GP";
                    break;
                case "SO1":
                    retValue = "South City Classic";
                    break;
                case "SO2":
                    retValue = "South City Sprint1";
                    break;
                case "SO3":
                    retValue = "South City Sprint3";
                    break;
                case "SO4":
                    retValue = "South City Long";
                    break;
                case "SO5":
                    retValue = "South City Town";
                    break;
                case "SO6":
                    retValue = "South City Chicane Route";
                    break;
                case "WE1":
                    retValue = "Westhill International";
                    break;
                case "AU1":
                    retValue = "Autocross";
                    break;
                case "AU2":
                    retValue = "Skid Pad";
                    break;
                case "AU3":
                    retValue = "Drag Strip";
                    break;
                case "AU4":
                    retValue = "8 Lane Drag";
                    break;
            }
            if( shortTrackName.Length > 3 && shortTrackName.Substring(3, 1) == "R")
                retValue = retValue + " Reverse";

            return (retValue);

        }

        static int pakGetByte(byte[] pak, int first)
        {
            return (int)pak[first];
        }
        static string pakGetString(byte[] pak, int first, int len)
        {
            return InSim.CodePage.GetString(pak, first, len);
        }
        static int pakGetWord(byte[] pak, int first)
        {
            return (int)System.BitConverter.ToUInt16(pak, first);
        }
        static int pakGetShort(byte[] pak, int first)
        {
            return (int)System.BitConverter.ToInt16(pak, first);
        }
        static long pakGetUnsigned(byte[] pak, int first)
        {
            return (long)System.BitConverter.ToUInt32(pak, first);
        }
        static int pakGetInt(byte[] pak, int first)
        {
            return (int)System.BitConverter.ToInt32(pak, first);
        }
        static float pakGetFloat(byte[] pak, int first)
        {
            return (float)System.BitConverter.ToSingle(pak, first);
        }
        public class TINY
        {
            public readonly int ReqI;
            public readonly string SubT;

            public TINY(byte[] packet)
            {
                ReqI = pakGetByte(packet, 2);
                SubT = Enum.GetName(typeof(TypeTiny), packet[3]);
            }
        }
        public class VER
        {
            public readonly string Version;
            public readonly string Product;
            public readonly int InSimVersion;

            public VER(byte[] packet)
            {
                Version = pakGetString(packet, 4, 8);
                Product = pakGetString(packet, 12, 6);
                InSimVersion = pakGetWord(packet, 18);
            }
        }
        public class ISM
        {
            public readonly int Host;
            public readonly string HName;

            public ISM(byte[] packet)
            {
                Host = pakGetByte(packet, 4);
                HName = pakGetString(packet, 8, 32);
            }
        }
        // Ok for Insim 4
        public class DefCompCar
        {
            public int node;
            public int lap;
            public int PLID;
            public int x;
            public int y;
            public int z;
            public int speed;
            public int direction;
            public int heading;
            public int angvel;

        }
        public class MCI
        {
            public int numOfPlayers;
            public DefCompCar[] compCar = new DefCompCar[8];

            public MCI(byte[] packet)
            {
                numOfPlayers = pakGetByte( packet, 3 );
                int offsetStartPlayer = 4;
                int lengthPlayer = 28;

                for (int i = 0; i < System.Math.Min(8,numOfPlayers); i++)
                {
                    compCar[i] = new DefCompCar();

                    compCar[i].node =pakGetWord(packet, offsetStartPlayer + i * lengthPlayer + 0);
                    compCar[i].lap = pakGetWord(packet, offsetStartPlayer + i * lengthPlayer + 2);

                    compCar[i].PLID = pakGetByte( packet, offsetStartPlayer + i * lengthPlayer + 4 );
                    compCar[i].x = pakGetInt( packet, offsetStartPlayer + i * lengthPlayer + 8);
                    compCar[i].y = pakGetInt( packet, offsetStartPlayer + i * lengthPlayer + 12);
                    compCar[i].z = pakGetInt( packet, offsetStartPlayer + i * lengthPlayer + 16);
                    compCar[i].speed = pakGetWord( packet, offsetStartPlayer + i * lengthPlayer + 20);
                    compCar[i].direction = pakGetWord( packet, offsetStartPlayer + i * lengthPlayer + 22);
                    compCar[i].heading = pakGetWord( packet, offsetStartPlayer + i * lengthPlayer + 24);
                    compCar[i].angvel = pakGetShort(packet, offsetStartPlayer + i * lengthPlayer + 26);
//                    System.Console.WriteLine("ID:{0} x:{1} y:{2} z:{3} speed:{4} direction:{5} heading:{6} angvel:{7}", PLID, x, y, z, speed, direction, heading, angvel);
                   
//                    if(debug)System.Console.WriteLine("{0:F2}m {1:F2}m {2:F2}m {3:F2}km/h {4:F2} {5:F2} {6:F2}", xm, ym, zm, spm, dirm, headm, angm);
                }
            }
        }
// Ok for Insim 4
        public class NLP
        {
            public NLP(byte[] packet)
            {
            }
        }

        /// <summary>
        /// State pack decoder
        /// LFS will send a StatePack any time the info in the StatePack changes.
        /// </summary>
 // OK For Insim 4
        public class STA
        {
            /// <summary>
            /// Short Track Name.
            /// </summary>
            public readonly float ReplaySpeed;
            public readonly int Flags;
            public readonly int InGameCam;
            public readonly int ViewPlayer;
            public readonly int NumP;
            public readonly int NumConns;
            public readonly int NumFinished;
            public readonly int RaceInProg;
            public readonly int QualMins;
            public readonly int RaceLaps;
            public readonly string ShortTrackName;
            public readonly int Weather;
            public readonly int Wind;

            public STA(byte[] packet)
            {
                ReplaySpeed = pakGetFloat(packet, 4);
                Flags = pakGetWord(packet, 8);
                InGameCam = pakGetByte(packet, 10);
                ViewPlayer = pakGetByte(packet, 11);
                NumP = pakGetByte(packet, 12);
                NumConns = pakGetByte(packet, 13);
                NumFinished = pakGetByte( packet,14 );
                RaceInProg = pakGetByte( packet, 15 );
                QualMins = pakGetByte(packet, 16);
                RaceLaps = pakGetByte(packet, 17);
                ShortTrackName = pakGetString(packet, 20, 6);
                Weather = pakGetByte(packet, 26);
                Wind = pakGetByte(packet, 27);
            }
        }
// Ok for InSim 4
        public class REO
        {
            /// <summary>
            /// Short Track Name.
            /// </summary>
            public readonly int NumP;
            public readonly int ReqI;
            public int[] PLID = new int[36];
            public REO(byte[] packet)
            {
                ReqI = pakGetByte(packet, 2);
                NumP = pakGetByte(packet, 3);
                for( int i = 0; i < 32; i++ )
                    PLID[i] = pakGetByte(packet, i + 4);

//                Array.ConstrainedCopy(packet, 4, PLID, 0, 32);
            }

        }
        public class RES
        {
            /// <summary>
            /// Short Track Name.
            /// </summary>
            public readonly int PLID;
            public readonly string userName;
            public readonly string nickName;
            public readonly string Plate;
            public readonly string CName;
            public readonly long TTime;
            public readonly long BTime;
            public readonly int NumStops;
            public readonly int Confirm;
            public readonly int LapDone;
            public readonly int Flags;
            public readonly int ResultNum;
            public readonly int NumRes;


            public RES(byte[] packet)
            {

                PLID = pakGetByte(packet, 3 );
                userName =  pakGetString(packet, 4, 24);
                nickName = pakGetString(packet, 28, 24);
                Plate = pakGetString(packet, 52, 8);
                CName = pakGetString(packet, 60, 4);
                TTime = pakGetUnsigned(packet, 64);
                BTime = pakGetUnsigned(packet, 68);
                NumStops = pakGetByte(packet, 73);
                Confirm = pakGetByte(packet, 74);
                LapDone = pakGetWord(packet, 76);
                Flags = pakGetWord(packet, 78);
                ResultNum = pakGetByte(packet, 80);
                NumRes = pakGetByte( packet, 81);
            }
        }
        public class FIN
        {
            /// <summary>
            /// Short Track Name.
            /// </summary>
            public readonly int PLID;
            public readonly long TTime;
            public readonly long BTime;
            public readonly int NumStops;
            public readonly int Confirm;
            public readonly int LapDone;
            public readonly int Flags;


            public FIN(byte[] packet)
            {

                PLID = pakGetByte(packet, 3);
                TTime = pakGetUnsigned(packet, 4);
                BTime = pakGetUnsigned(packet, 8);
                NumStops = pakGetByte(packet, 13);
                Confirm = pakGetByte(packet, 14);
                LapDone = pakGetWord(packet, 16);
                Flags = pakGetWord(packet, 18);
            }
        }
        //NOT USED in Insim 4
        public class MSS
        {
            public readonly string UserName;
            public readonly string PlayerName;
            public readonly string Message;
            
            public MSS(byte[] packet)
            {
                UserName = "";
                PlayerName = "";
                Message = "";
            }
        }

// MSO OK FOR insim 4
		public class MSO
        {
            public readonly int UCID;
            public readonly int PLID;
            public readonly int UserType;
            public readonly int TextStart;
            public readonly string message;
            public readonly string completeMessage;

            public MSO(byte[] packet)
            {
                UCID = pakGetByte( packet, 4);
                PLID = pakGetByte( packet, 5);
                UserType = pakGetByte( packet, 6);
                TextStart = pakGetByte( packet, 7);
                message = pakGetString(packet, 8 + TextStart, 128 - TextStart);
                completeMessage = pakGetString(packet, 8, 128);
            }
        }

// Ok For Insim 4
		public class NPL
		{
            public readonly int PLID;
            public readonly int UCID;
            public readonly int PType;
            public readonly int Flags;
            public readonly string nickName;
            public readonly string Plate;
            public readonly string CName;
            public readonly string SName;
			public readonly string Tyres;
			public readonly int H_Mass;
			public readonly int H_TRes;
			public readonly int Pass;
			public readonly int NumP;

//			public readonly PlayerFlags flags;

			public enum PlayerFlags : ushort
			{
				SwapSide = 1,
				GCCut = 2,
				GCBlip = 4,
				Gears = 8,
				Shifter = 16,
				Reserved1 = 32,
				Brake = 64,
				Throttle = 128,
				Reserved2 = 256,
				Clutch = 512,
				Mouse = 1024,
				KbNoHelp = 2048,
				KbStabilised = 4096,
                CustomView = 8192
			}

			public NPL(byte[] pak)
			{
                PLID = pakGetByte( pak, 3);
                UCID = pakGetByte( pak, 4 );
                PType = pakGetByte( pak, 5 );
                Flags = pakGetWord(pak, 6);
                nickName = pakGetString(pak, 8, 24);
                Plate = pakGetString(pak, 32, 8);
                CName = pakGetString(pak, 40, 4);
                SName = pakGetString(pak, 44, 16);
                Tyres = pakGetString(pak, 60, 4);
                H_Mass = pakGetByte(pak, 64);
                H_TRes = pakGetByte(pak, 65);
                Pass = pakGetByte(pak, 67);
                NumP = pakGetByte(pak, 73);
//                flags = (PlayerFlags)(ushort)((pak[6]) | (pak[7] << 8));

			}
		}
        // Ok For insim 4
        public class PIT
        {
            public readonly int PLID;
            public readonly int LapsDone;
            public readonly int Flags;
            public readonly int Penalty;
            public readonly int NumStop;
            public readonly int rearL;
            public readonly int rearR;
            public readonly int frontL;
            public readonly int frontR;
            public readonly long Work;
            public readonly string sWork;

            public PIT(byte[] pak)
            {
                PLID = pakGetByte(pak, 3);
                LapsDone = pakGetWord(pak, 4);
                Flags = pakGetWord(pak, 6);
                Penalty = pakGetByte(pak, 9);
                NumStop = pakGetByte(pak, 10);
                rearL = pakGetByte(pak, 12);
                rearR = pakGetByte(pak, 13);
                frontL = pakGetByte(pak, 14);
                frontR = pakGetByte(pak, 15);
                Work = pakGetUnsigned(pak, 16);
                sWork = "";
                string virg = "";

                if(
                    (Work & (long)PIT_work.PSE_FR_DAM) != 0
                    ||(Work & (long)PIT_work.PSE_RE_DAM) != 0
                    || (Work & (long)PIT_work.PSE_LE_FR_DAM) != 0
                    || (Work & (long)PIT_work.PSE_RI_FR_DAM) != 0
                    || (Work & (long)PIT_work.PSE_LE_RE_DAM) != 0
                    || (Work & (long)PIT_work.PSE_RI_RE_DAM) != 0
                    )
                {
                    sWork = sWork + virg + "Mechanicals Damages";
                    virg = ", ";
                }
                if(
                    (Work & (long)PIT_work.PSE_BODY_MINOR) != 0
                    || (Work & (long)PIT_work.PSE_BODY_MAJOR) != 0
                    )
                {
                    sWork = sWork + virg + "Body Dammage";
                    virg = ", ";
                }
                if((Work & (long)PIT_work.PSE_REFUEL) != 0 )
                {
                    sWork = sWork + virg + "Refuel";
                    virg = ", ";
                }
                if((Work & (long)PIT_work.PSE_SETUP) != 0 )
                {
                    sWork = sWork + virg + "Setup";
                    virg = ", ";
                }
                if(
                    (Work & (long)PIT_work.PSE_FR_WHL) != 0
                    || (Work & (long)PIT_work.PSE_LE_FR_WHL) != 0
                    || (Work & (long)PIT_work.PSE_RI_FR_WHL) != 0
                    || (Work & (long)PIT_work.PSE_RE_WHL) != 0
                    || (Work & (long)PIT_work.PSE_LE_RE_WHL) != 0
                    || (Work & (long)PIT_work.PSE_RI_RE_WHL) != 0
                    )
                {
                    sWork = sWork + virg + "Wheels or Transmissions";
                    virg = ", ";
                }

            }
        }
        // Ok For insim 4
        public class PSF
        {
            public readonly int PLID;
            public readonly long STime;

            public PSF(byte[] pak)
            {
                PLID = pakGetByte(pak, 3);
                STime = pakGetUnsigned(pak, 4);
            }
        }
        // LAP OK for insim 4
        public class LAP
        {
			public readonly int PLID;
            public readonly long LTime;
            public readonly long ETime;
            public readonly int LapsDone;
            public readonly int Flags;
            public readonly int Penalty;
            public readonly int NumStop;
            

			public LAP(byte[] pak)
			{
                PLID = pakGetByte( pak, 3 );
                LTime = pakGetUnsigned(pak, 4);
                ETime = pakGetUnsigned(pak, 8);
                LapsDone = pakGetWord(pak, 12);
                Flags = pakGetWord(pak, 14);
                Penalty = pakGetByte(pak, 17);
                NumStop = pakGetByte(pak, 18);
            }
		}

		/// <summary>
		/// Split X time decoder
		/// </summary>
// LAP OK for insim 4
        public class SPX
		{
            public readonly int PLID;
            public readonly long STime;
            public readonly long ETime;
            public readonly int Split;
            public readonly int Penalty;
            public readonly int NumStop;

			public SPX(byte[] pak)
			{
                PLID = pakGetByte(pak, 3);
                STime = pakGetUnsigned(pak, 4);
                ETime = pakGetUnsigned(pak, 8);
                Split = pakGetByte( pak, 12 );
                Penalty = pakGetByte(pak, 13);
                NumStop = pakGetByte(pak, 14);
			}
		}
// RST Ok for Insim 4
        public class RST
        {
            /// <summary>
            /// Gets time for qualifications in minutes. If zero, race started.
            /// </summary>
            public readonly int RaceLaps;
            public readonly int QualMins;
            public readonly int NumP;
            public readonly string Track;
            public readonly int Weather;
            public readonly int Wind;
            public readonly int Flags;
            public readonly int NumNodes;
            public readonly int Finish;
            public readonly int Split1;
            public readonly int Split2;
            public readonly int Split3;

            public RST(byte[] pak)
            {
                RaceLaps = pakGetByte( pak ,4 );
                QualMins = pakGetByte( pak, 5 );
                NumP = pakGetByte(pak, 6);
                Track = pakGetString(pak, 8, 6);
                Weather = pakGetByte(pak, 14);
                Wind = pakGetByte(pak, 15);
                Flags = pakGetByte(pak, 16);
                NumNodes = pakGetWord(pak, 18);
                Finish = pakGetWord(pak, 20);
                Split1 = pakGetWord(pak, 22);
                Split2 = pakGetWord(pak, 24);
                Split3 = pakGetWord(pak, 26);
            }
        }
// Ok pour Insim 4
		public class NCN
		{
            public readonly int UCID;
            public readonly string userName;
            public readonly string nickName;
            public readonly int Admin;
            public readonly int Total;


			public NCN(byte[] pak)
			{
                UCID = pakGetByte( pak, 3 );
                userName = pakGetString(pak, 4, 24);
                nickName = pakGetString(pak, 28, 24);
                Admin = pakGetByte(pak, 52);
                Total = pakGetByte(pak, 53);
			}
		}
// Ok For insim 4
		public class CNL
		{
			public readonly int UCID;
            public readonly int Total;

			public CNL(byte[] pak)
			{
				UCID = pakGetByte(pak, 3);
                Total = pakGetByte(pak, 5);
            }
		}
// Ok For insim 4
        public class CPR
        {
            public readonly int UCID;
            public readonly string newNickName;
            public readonly string Plate;

            public CPR(byte[] pak)
            {
                UCID = pakGetByte( pak, 3 );
                newNickName = pakGetString(pak, 4, 24);
                Plate = pakGetString(pak, 28, 8);
            }
        }
// Ok for Insim 4
        /// <summary>
        /// PLayer Pits
        /// </summary>
        public class PLP
        {
            public readonly int PLID;

            public PLP(byte[] pak)
            {
                PLID = pakGetByte( pak , 3 );
            }
        }
// Ok for Insim 4
        /// <summary>
        /// PLayer Leave race
        /// </summary>
        public class PLL
        {
            public readonly int PLID;

            public PLL(byte[] pak)
            {
                PLID = pakGetByte( pak , 3 );
            }
        }
        // Ok For insim 4
        public class FLG
        {
            public readonly int PLID;
            public readonly int OffOn;
            public readonly int Flag;
            public readonly int CarBehind;

            public FLG(byte[] pak)
            {
                PLID = pakGetByte(pak, 3);
                OffOn = pakGetByte(pak, 4);
                Flag = pakGetByte(pak, 5);
                CarBehind = pakGetByte(pak, 6);
            }
        }
        public class PEN
        {
            public readonly int PLID;
            public readonly int OldPen;
            public readonly int NewPen;
            public readonly int Reason;

            public PEN(byte[] pak)
            {
                PLID = pakGetByte(pak, 3);
                OldPen = pakGetByte(pak, 4);
                NewPen = pakGetByte(pak, 5);
                Reason = pakGetByte(pak, 6);
            }
        }
        public class BFN
        {
            public readonly int SubT;
            public readonly int UCID;
            public readonly int ClickID;

            public BFN(byte[] pak)
            {
                SubT = pakGetByte(pak, 3);
                UCID = pakGetByte(pak, 4);
                ClickID = pakGetByte(pak, 5);
            }
        }
        public class BTC
        {
            public readonly int UCID;
            public readonly int ClickID;
            public readonly int CFlags;

            public BTC(byte[] pak)
            {
                UCID = pakGetByte(pak, 3);
                ClickID = pakGetByte(pak, 4);
                CFlags = pakGetByte(pak, 6);
            }
        }
        public class TOC
        {
            public readonly int PLID;
            public readonly int OldUCID;
            public readonly int NewUCID;


            public TOC(byte[] pak)
            {
                PLID = pakGetByte(pak, 3);
                OldUCID = pakGetByte(pak, 4);
                NewUCID = pakGetByte(pak, 5);
            }
        }



	}
}

