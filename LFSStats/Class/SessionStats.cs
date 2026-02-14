using System;
using System.Collections.Generic;
using System.Linq;
using InSimDotNet.Packets;

namespace LFSStatistics
{
    class Penalty
    {
        public int Lap;
        public int OldPen;
        public int NewPen;
        public int Reason;
        public Penalty(int Lap, int OldPen, int NewPen, int Reason)
        {
            this.Lap = Lap;
            this.OldPen = OldPen;
            this.NewPen = NewPen;
            this.Reason = Reason;
        }
    }
    class Toc
    {
        public string oldNickName;
        public string oldUserName;
        public string newNickName;
        public string newUserName;
        public int lap;
        public Toc(string oldNickName, string oldUserName, string newNickName, string newUserName, int lap)
        {
            this.oldUserName = oldUserName;
            this.oldNickName = oldNickName;
            this.newUserName = newUserName;
            this.newNickName = newNickName;
            this.lap = lap;
        }
    }

    /// <summary>
    /// Timing event for position tracking. Stores lap, split, and elapsed time.
    /// </summary>
    class TimingEvent
    {
        public int Lap { get; set; }
        public int Split { get; set; }  // 0 = lap completion, 1-3 = split number
        public long ETime { get; set; }  // Elapsed time in milliseconds since race start

        public TimingEvent(int lap, int split, long eTime)
        {
            Lap = lap;
            Split = split;
            ETime = eTime;
        }
    }
    class Lap
    {
        public long[] Split { get; private set; }
        public long LapTime { get; private set; }
        public long CumuledTime { get; private set; }

        public Lap(long[] split, long lapTime, long cumuledTime)
        {
            Split = split;
            LapTime = lapTime;
            CumuledTime = cumuledTime;
        }
    }
    // Pit now wraps the InSimDotNet IS_PIT packet and exposes commonly used fields
    class Pit
    {
        public int LapsDone;
        //public PlayerFlags Flags;
        public PenaltyValue Penalty;
        public int NumStop;
        public TyreCompound RearLeft;
        public TyreCompound RearRight;
        public TyreCompound FrontLeft;
        public TyreCompound FrontRight;
        public PitWorkFlags Work;
        public long STime;

        // Keep original packet if needed for later use
        public IS_PIT Packet { get; private set; }

        public Pit(IS_PIT pit)
        {
            if (pit == null) throw new ArgumentNullException(nameof(pit));

            Packet = pit;
            LapsDone = pit.LapsDone;
            //Flags = pit.Flags;
            Penalty = pit.Penalty;
            NumStop = pit.NumStops;
            // Tyres structure from InSimDotNet
            RearLeft = pit.Tyres.RearLeft;
            RearRight = pit.Tyres.RearRight;
            FrontLeft = pit.Tyres.FrontLeft;
            FrontRight = pit.Tyres.FrontRight;
            Work = pit.Work;
            STime = 0;
        }
    }

    class SessionStats : IComparable<SessionStats>
    {
        public int UCID;                        // connection ID
        public int PLID;                        // player ID
        public string userName;
        public Dictionary<string, PlayerIdentity> allPlayers;
        private string nickName;
        public string NickName
        {
            get { return nickName; }
            set { nickName = value; }
        }
        public string Plate;

        public long[] BestSector { get; private set; }
        public int[] BestSectorLap { get; private set; }

        public long cumuledTime;
        public int bestSpeed;
        public int lapBestSpeed;
        public int numStop;
        public long cumuledStime; // accumulated Pit Time
        public int resultNum;
        public int finalPos;
        public bool finished = false;
        public int finPLID;
        public long totalTime;
        public long bestLap;
        public int lapBestLap;
        public string carName;
        public string penalty;
        public int gridPos;
        public int lapsLead;
        public long tmpTime;
        public long firstTime;
        public long avgTime;

        public long bestSectorSortHelper = 0;   // use parametrized comparer in SessionStats instead
        public long BestSectorLapHelper = 0;    // use parametrized comparer in SessionStats instead
        public long curWrSplit = 0;

        public double lapStability;

        public long[] CurrentSplit { get; private set; }
        //public long curSplit1;
        //public long curSplit2;
        //public long curSplit3;
        private long lastSplit;

        public int yellowFlags;
        public bool inYellow;
        public int blueFlags;
        public int numPen = 0;
        public bool inBlue;
        public int contacts = 0;  // Track number of contacts/collisions
        public int offTracks = 0;  // Track number of off-track incidents (invalidated laps)

        public PlayerFlags flags { get; set; }
        public string sFlags => GetAllPlayerFlags();

        // Temporary storage for pit stop data between IS_PIT and IS_PSF
        private IS_PIT pendingPitStop;

        public List<Lap> lap = new List<Lap>();
        public List<Pit> pit = new List<Pit>();
        public List<Penalty> pen = new List<Penalty>();
        public List<Toc> toc = new List<Toc>();
        public List<TimingEvent> timingEvents = new List<TimingEvent>();  // Track split and lap times with ETime for position calculation

        // Disconnection tracking
        public bool disconnected = false;
        public DateTime disconnectTime;

        public SessionStats(int UCID, int PLID)
        {
            this.UCID = UCID;
            this.PLID = PLID;
            userName = "";
            allPlayers = new Dictionary<string, PlayerIdentity>();
            nickName = "";
            //this.sectorCount = 0;
            BestSector = new long[LFSStats.MaxSectorCount];
            BestSectorLap = new int[LFSStats.MaxSectorCount];
            //this.bestSplit1 = 0;
            //this.bestSplit2 = 0;
            //this.bestSplit3 = 0;
            //this.bestLastSplit = 0;
            bestSpeed = 0;
            numStop = 0;
            resultNum = 999;
            finished = false;
            finPLID = -1;
            totalTime = 0;
            carName = "";
            penalty = "";
            gridPos = 999;
            cumuledTime = 0;
            lapsLead = 0;
            firstTime = 0;
            avgTime = 0;
            CurrentSplit = new long[LFSStats.MaxSplitCount];
            //this.curSplit1 = 0;
            //this.curSplit2 = 0;
            //this.curSplit3 = 0;
            //this.lastSplit = 0;
            yellowFlags = 0;
            inBlue = false;
            inYellow = false;
            blueFlags = 0;
            cumuledStime = 0;
        }

        private static IComparer<SessionStats> sortResult;
        private static IComparer<SessionStats> sortGrid;
        private static IComparer<SessionStats> sortClimb;
        private static IComparer<SessionStats> sortLapLead;
        private static IComparer<SessionStats> sortFirstLap;
        private static IComparer<SessionStats> sortAvgTime;
        private static IComparer<SessionStats> sortLap;
        private static IComparer<SessionStats> sortSplit;
        private static IComparer<SessionStats> sortSpeed;
        private static IComparer<SessionStats> sortStability;
        private static IComparer<SessionStats> sortBlueFlag;
        private static IComparer<SessionStats> sortYellowFlag;
        private static IComparer<SessionStats> sortContact;
        private static IComparer<SessionStats> sortOffTrack;
        private static IComparer<SessionStats> sortPit;
        private static IComparer<SessionStats> sortTPB;
        private static IComparer<SessionStats> sortPenalization;


        /// <summary>
        /// Initializes the <see cref="SessionStats"/> class.
        /// Static constructor is invoked only once on class creation/access.
        /// Creates comparators to be used later.
        /// </summary>
        static SessionStats()
        {
            sortResult = new ResultCmp();
            sortGrid = new GridCmp();
            sortClimb = new ClimbCmp();
            sortLapLead = new LapLeadCmp();
            sortFirstLap = new FirstLapCmp();
            sortAvgTime = new AvgTimeCmp();
            sortLap = new LapCmp();
            sortSplit = new SplitCmp();
            sortSpeed = new SpeedCmp();
            sortStability = new StabilityCmp();
            sortBlueFlag = new BlueFlagCmp();
            sortYellowFlag = new YellowFlagCmp();
            sortContact = new ContactCmp();
            sortOffTrack = new OffTrackCmp();
            sortPit = new PitCmp();
            sortTPB = new TPBCmp();
            sortPenalization = new PenalizationCmp();
        }

        public string GetPlayerFlagSteering()
        {
            if (flags.HasFlag(PlayerFlags.PIF_MOUSE))
                return "Mouse";
            if (flags.HasFlag(PlayerFlags.PIF_KB_NO_HELP))
                return "Keyboard";
            if (flags.HasFlag(PlayerFlags.PIF_KB_STABILISED))
                return "KeyboardStabilised";
            else
                return "Wheel";
        }
        public string GetPlayerFlagBrakeHelp()
        {
            if (flags.HasFlag(PlayerFlags.PIF_HELP_B))
                return "BrakeHelp";
            else
                return "";
        }
        public string GetPlayerFlagAutoGearShift()
        {
            if (flags.HasFlag(PlayerFlags.PIF_AUTOGEARS))
                return "AutoGearShift";
            else
                return "";
        }
        public string GetPlayerFlagManualShifter()
        {
            if (flags.HasFlag(PlayerFlags.PIF_SHIFTER))
                return "Shifter";
            else
                return "";
        }
        public string GetPlayerFlagAxisClutch()
        {
            if (flags.HasFlag(PlayerFlags.PIF_AXIS_CLUTCH))
                return "AxisClutch";
            else
                return "";
        }
        public string GetPlayerFlagAutoClutch()
        {
            if (flags.HasFlag(PlayerFlags.PIF_AUTOCLUTCH))
                return "AutoClutch";
            else
                return "";
        }
        public string GetPlayerFlagDriverSide()
        {
            //TODO: Include mods check
            var singleSeaters = new HashSet<string> { "FBM", "FOX", "FO8", "BF1", "MRT" };
            if (!singleSeaters.Contains(carName) && flags.HasFlag(PlayerFlags.PIF_SWAPSIDE))
                return "SwapSide";
            else
                return "";
        }
        public string GetPlayerFlagCustomView()
        {
            if (flags.HasFlag(PlayerFlags.PIF_CUSTOM_VIEW))
                return "CustomView";
            else
                return "";
        }

        public string GetAllPlayerFlags()
        {
            var results = new List<string>
            {
                GetPlayerFlagSteering(),
                GetPlayerFlagBrakeHelp(),
                GetPlayerFlagAutoGearShift(),
                GetPlayerFlagManualShifter(),
                GetPlayerFlagAxisClutch(),
                GetPlayerFlagAutoClutch(),
                GetPlayerFlagDriverSide(),
                GetPlayerFlagCustomView()
            };
            return string.Join(" ", results.Where(flag => !string.IsNullOrWhiteSpace(flag)));
        }

        public static string LfsTimeToString(long val)
        {
            return InSimDotNet.Helpers.StringHelper.ToLapTimeString(TimeSpan.FromMilliseconds(val));
        }
        public static string LfsSpeedToString(int val)
        {
            return Math.Round(InSimDotNet.Helpers.MathHelper.SpeedToKph(val), 2).ToString();
        }

        public void UpdateSplit(int split, long splitTime, long eTime = 0)
        {
            if (finished == true)
                return;

            long sector;
            int index = split - 1;

            if (index >= LFSStats.MaxSplitCount)
                throw new Exception("Cannot update split " + split + ", this number of splits and sectors is not supported.");
            //if (SectorCount < split) SectorCount = split;
            //if (split == 1)
            //{
            // On first Split Create a new Lap
            //sector = splitTime;
            CurrentSplit[index] = splitTime;   // save split time
            sector = splitTime - lastSplit;         // NOTE added lastSplit = 0 on lap completion
            lastSplit = splitTime;

            // Record timing event with elapsed time for position tracking
            if (eTime > 0)
            {
                timingEvents.Add(new TimingEvent(lap.Count + 1, split, eTime));
            }
            else
            {
            }

            if ((BestSector[index] == 0) || (sector < BestSector[index]))
            {
                BestSector[index] = sector;
                BestSectorLap[index] = lap.Count + 1;
            }
            //}
            //if (split == 2)
            //{
            //    sector = splitTime - lastSplit;
            //    this.curSplit2 = splitTime;
            //    if ((this.bestSplit2 == 0) || (sector < this.bestSplit2))
            //    {
            //        this.bestSplit2 = sector;
            //        this.lapBestSplit2 = this.lap.Count;
            //    }
            //    lastSplit = splitTime;
            //}
            //if (split == 3)
            //{
            //    sector = splitTime - lastSplit;
            //    this.curSplit3 = splitTime;
            //    if ((this.bestSplit3 == 0) || (sector < this.bestSplit3))
            //    {
            //        this.bestSplit3 = sector;
            //        this.lapBestSplit3 = this.lap.Count;
            //    }
            //    lastSplit = splitTime;
            //}
        }
        public void UpdateLap(long lapTime, int numStop, int lapsDone, int maxSplit, long eTime = 0)
        {
            if (finished == true)
                return;

            if (bestLap == 0 || lapTime < bestLap)  // save best lap time and lap number
            {
                bestLap = lapTime;
                lapBestLap = lapsDone;
            }
            this.numStop = numStop;
            cumuledTime = cumuledTime + lapTime;  // accumulate total time

            // Record timing event for lap completion (split = 0 indicates lap completion)
            if (eTime > 0)
            {
                timingEvents.Add(new TimingEvent(lapsDone, 0, eTime));
            }
            else
            {
            }

            // Add a new Lap when lap is done
            lap.Add(new Lap(CurrentSplit, lapTime, cumuledTime));

            if (lap.Count == 1)
                firstTime = lapTime;

            // best last sector
            long sector = lapTime - lastSplit;
            lastSplit = 0;  // NOTE added for best sector 1 calculation

            if (maxSplit >= LFSStats.MaxSectorCount)
                throw new Exception("Cannot update sector " + maxSplit + ", this number of splits and sectors is not supported.");

            if ((BestSector[maxSplit] == 0) || (sector < BestSector[maxSplit]))
            {
                BestSector[maxSplit] = sector;
                BestSectorLap[maxSplit] = lapsDone;
            }
        }

        public void UpdatePen(int OldPen, int NewPen, int Reason)
        {
            if (finished == true)
                return;
            int LapDone = lap.Count + 1;
            pen.Add(new Penalty(LapDone, OldPen, NewPen, Reason));
            if (NewPen != (int)PenaltyValue.PENALTY_NONE)
                numPen++;
        }
        public void updateMCI(int speed)
        {
            if (bestSpeed < speed && finished == false)
            {
                bestSpeed = speed;
                lapBestSpeed = lap.Count + 1;
            }
        }
        public void updatePIT(IS_PIT pitDec)
        {
            if (finished == true)
                return;
            numStop = pitDec.NumStops;
            // Store pit data temporarily until IS_PSF confirms completion
            pendingPitStop = pitDec;
        }
        public void updateTOC(string oldNickName, string oldUserName, string newNickName, string newUserName)
        {
            if (finished == true)
                return;
            toc.Add(new Toc(oldNickName, oldUserName, newNickName, newUserName, lap.Count + 1));

        }
        public void updatePSF(int PLID, TimeSpan STime)
        {
            if (finished == true)
                return;

            // IS_PSF is received once when pit stop completes - use this as the authoritative source
            if (pendingPitStop != null)
            {
                // Create pit stop entry from pending IS_PIT data + IS_PSF duration
                Pit pitStop = new Pit(pendingPitStop);
                pitStop.STime = STime.TotalMillisecondsLong();
                pit.Add(pitStop);
                cumuledStime += STime.TotalMillisecondsLong();
                pendingPitStop = null;  // Clear pending
            }
        }
        // TODO this seems very different from race result + it's used for setting lap which normally does UpdateLap, missing penalty stuff
        public void UpdateQualResult(long TTime, long BTime, int NumStops, ConfirmationFlags Confirm, int LapsDone, int ResultNum, int NumRes, int maxSplit)
        {
            //Console.WriteLine("TTime:" + TTime
            //                    + " BTime:" + BTime
            //                    + " NumStops:" + NumStops
            //                    + " Confirm:" + Confirm
            //                    + " LapDone:" + LapDone
            //                    + " ResultNum:" + ResultNum
            //                    + " NumRes:" + NumRes);
            if (finished == true)
                return;

            if (bestLap == 0 || BTime < bestLap)
            {
                bestLap = BTime;
                lapBestLap = LapsDone;
            }
            cumuledTime = cumuledTime + BTime;

            // Add a new Lap when lap is done
            lap.Add(new Lap(CurrentSplit, BTime, cumuledTime));

            // best last sector
            long sector = BTime - lastSplit;
            lastSplit = 0;  // NOTE added for best sector 1 calculation

            if (maxSplit >= LFSStats.MaxSectorCount)
                throw new Exception("Cannot update sector " + maxSplit + ", this number of splits and sectors is not supported.");

            if ((BestSector[maxSplit] == 0) || (sector < BestSector[maxSplit]))
            {
                BestSector[maxSplit] = sector;
                BestSectorLap[maxSplit] = LapsDone;
            }
        }
        public void UpdateResult(long totalTime, int resultNum, string CName, ConfirmationFlags confirm, int numStop)
        {
            this.totalTime = totalTime;
            this.resultNum = resultNum;
            carName = CName;
            this.numStop = numStop;

            penalty = "";
            if ((confirm & ConfirmationFlags.CONF_PENALTY_DT) != 0)
            {
                this.resultNum = 998;
                penalty += "DT";
            }
            if ((confirm & ConfirmationFlags.CONF_PENALTY_SG) != 0)
            {
                this.resultNum = 998;
                penalty += "SG";
            }
            if ((confirm & ConfirmationFlags.CONF_DID_NOT_PIT) != 0)
            {
                this.resultNum = 998;
                penalty += "DNP";
            }
            if ((confirm & ConfirmationFlags.CONF_PENALTY_30) != 0)
                penalty += "30s";
            if ((confirm & ConfirmationFlags.CONF_PENALTY_45) != 0)
                penalty += "45s";
        }

        /// <summary>
        /// Compares to this instance. Using SORT_RESULT.
        /// </summary>
        /// <param name="other">The other instance.</param>
        /// <returns>-1 if this is lower, 0 equal, 1 higher</returns>
        public int CompareTo(SessionStats other)
        {
            if (other == null) return -1;
            return CompareResult(this, other);
        }

        /// <summary>
        /// Custom sort: DNF (resultNum=999) last, then by laps desc, then totalTime asc.
        /// LFS resultNum is unreliable with disconnect/reconnect.
        /// </summary>
        private static int CompareResult(SessionStats x, SessionStats y)
        {
            // DNF (no IS_RES received) always last
            bool xDnf = x.resultNum >= 999;
            bool yDnf = y.resultNum >= 999;
            if (xDnf != yDnf)
                return xDnf ? 1 : -1;

            // Penalized drivers (998) after normal finishers
            bool xPen = x.resultNum == 998;
            bool yPen = y.resultNum == 998;
            if (xPen != yPen)
                return xPen ? 1 : -1;

            // More laps = better
            if (y.lap.Count != x.lap.Count)
                return y.lap.Count > x.lap.Count ? 1 : -1;

            // Same laps: lower totalTime = better
            if (x.totalTime != y.totalTime)
                return x.totalTime < y.totalTime ? -1 : 1;

            return 0;
        }

        private class ResultCmp : IComparer<SessionStats>
        {
            public int Compare(SessionStats x, SessionStats y)
            {
                return CompareResult(x, y);
            }
        }
        public static IComparer<SessionStats> GetResultCmp()
        {
            return sortResult;
        }
        private class GridCmp : IComparer<SessionStats>
        {
            public int Compare(SessionStats x, SessionStats y)
            {
                if (y.gridPos < x.gridPos)
                    return 1;
                if (y.gridPos > x.gridPos)
                    return -1;
                return 0;
            }
        }
        public static IComparer<SessionStats> GetGridCmp()
        {
            return sortGrid;
        }
        private class ClimbCmp : IComparer<SessionStats>
        {
            public int Compare(SessionStats x, SessionStats y)
            {
                if ((y.resultNum - y.gridPos) < (x.resultNum - x.gridPos))
                    return 1;
                if ((y.resultNum - y.gridPos) > (x.resultNum - x.gridPos))
                    return -1;
                return 0;
            }
        }
        public static IComparer<SessionStats> GetClimbCmp()
        {
            return sortClimb;
        }
        private class LapLeadCmp : IComparer<SessionStats>
        {
            public int Compare(SessionStats x, SessionStats y)
            {
                if (y.lapsLead > x.lapsLead)
                    return 1;
                if (y.lapsLead < x.lapsLead)
                    return -1;
                return 0;
            }
        }
        public static IComparer<SessionStats> GetLapLeadCmp()
        {
            return sortLapLead;
        }
        private class FirstLapCmp : IComparer<SessionStats>
        {
            public int Compare(SessionStats x, SessionStats y)
            {
                if (y.firstTime < x.firstTime)
                    return 1;
                if (y.firstTime > x.firstTime)
                    return -1;
                return 0;
            }
        }
        public static IComparer<SessionStats> GetFirstLapCmp()
        {
            return sortFirstLap;
        }
        private class AvgTimeCmp : IComparer<SessionStats>
        {
            public int Compare(SessionStats x, SessionStats y)
            {
                if (y.avgTime < x.avgTime)
                    return 1;
                if (y.avgTime > x.avgTime)
                    return -1;
                return 0;
            }
        }
        public static IComparer<SessionStats> GetAvgTimeCmp()
        {
            return sortAvgTime;
        }
        private class LapCmp : IComparer<SessionStats>
        {
            public int Compare(SessionStats x, SessionStats y)
            {
                if (y.bestLap < x.bestLap)
                    return 1;
                if (y.bestLap > x.bestLap)
                    return -1;
                return 0;
            }
        }
        public static IComparer<SessionStats> GetLapCmp()
        {
            return sortLap;
        }
        private class SplitCmp : IComparer<SessionStats>
        {
            public int Compare(SessionStats x, SessionStats y)
            {
                if (y.bestSectorSortHelper < x.bestSectorSortHelper)
                    return 1;
                if (y.bestSectorSortHelper > x.bestSectorSortHelper)
                    return -1;
                return 0;
            }
        }
        public static IComparer<SessionStats> GetSplitCmp()
        {
            return sortSplit;
        }
        private class TPBCmp : IComparer<SessionStats>
        {
            public int Compare(SessionStats x, SessionStats y)
            {
                long yBest = y.BestSector.Sum();
                long xBest = x.BestSector.Sum();
                if (yBest < xBest)
                    return 1;
                if (yBest > xBest)
                    return -1;
                return 0;
            }
        }
        public static IComparer<SessionStats> GetTPBCmp()
        {
            return sortTPB;
        }
        private class StabilityCmp : IComparer<SessionStats>
        {
            public int Compare(SessionStats x, SessionStats y)
            {
                if (y.lapStability < x.lapStability)
                    return 1;
                if (y.lapStability > x.lapStability)
                    return -1;
                return 0;
            }
        }
        public static IComparer<SessionStats> GetStabilityCmp()
        {
            return sortStability;
        }
        private class SpeedCmp : IComparer<SessionStats>
        {
            public int Compare(SessionStats x, SessionStats y)
            {
                if (y.bestSpeed > x.bestSpeed)
                    return 1;
                if (y.bestSpeed < x.bestSpeed)
                    return -1;
                return 0;
            }
        }
        public static IComparer<SessionStats> GetSpeedCmp()
        {
            return sortSpeed;
        }
        private class BlueFlagCmp : IComparer<SessionStats>
        {
            public int Compare(SessionStats x, SessionStats y)
            {
                if (y.blueFlags > x.blueFlags)
                    return 1;
                if (y.blueFlags < x.blueFlags)
                    return -1;
                return 0;
            }
        }
        public static IComparer<SessionStats> GetBlueFlagCmp()
        {
            return sortBlueFlag;
        }
        private class YellowFlagCmp : IComparer<SessionStats>
        {
            public int Compare(SessionStats x, SessionStats y)
            {
                if (y.yellowFlags > x.yellowFlags)
                    return 1;
                if (y.yellowFlags < x.yellowFlags)
                    return -1;
                return 0;
            }
        }
        public static IComparer<SessionStats> GetYellowFlagCmp()
        {
            return sortYellowFlag;
        }
        private class ContactCmp : IComparer<SessionStats>
        {
            public int Compare(SessionStats x, SessionStats y)
            {
                if (y.contacts > x.contacts)
                    return 1;
                if (y.contacts < x.contacts)
                    return -1;
                return 0;
            }
        }
        public static IComparer<SessionStats> GetContactCmp()
        {
            return sortContact;
        }
        private class OffTrackCmp : IComparer<SessionStats>
        {
            public int Compare(SessionStats x, SessionStats y)
            {
                if (y.offTracks > x.offTracks)
                    return 1;
                if (y.offTracks < x.offTracks)
                    return -1;
                return 0;
            }
        }
        public static IComparer<SessionStats> GetOffTrackCmp()
        {
            return sortOffTrack;
        }
        private class PitCmp : IComparer<SessionStats>
        {
            public int Compare(SessionStats x, SessionStats y)
            {
                if (y.pit.Count < x.pit.Count)
                    return 1;
                if (y.pit.Count > x.pit.Count)
                    return -1;
                if (y.cumuledStime < x.cumuledStime)
                    return 1;
                if (y.cumuledStime > x.cumuledStime)
                    return -1;
                return 0;
            }
        }
        public static IComparer<SessionStats> GetPitCmp()
        {
            return sortPit;
        }
        private class PenalizationCmp : IComparer<SessionStats>
        {
            public int Compare(SessionStats x, SessionStats y)
            {
                if (y.numPen > x.numPen)
                    return 1;
                if (y.numPen < x.numPen)
                    return -1;
                return 0;
            }
        }
        public static IComparer<SessionStats> GetPenalizationCmp()
        {
            return sortPenalization;
        }

        /// <summary>
        /// Creates a deep copy of SessionStats to preserve data when PLID is reused
        /// </summary>
        public SessionStats Clone()
        {
            SessionStats clone = new SessionStats(UCID, PLID);

            // Copy basic fields
            clone.userName = userName;
            clone.NickName = NickName;
            clone.Plate = Plate;
            clone.carName = carName;
            clone.penalty = penalty;
            clone.gridPos = gridPos;

            // Copy timing data
            clone.cumuledTime = cumuledTime;
            clone.bestSpeed = bestSpeed;
            clone.lapBestSpeed = lapBestSpeed;
            clone.numStop = numStop;
            clone.cumuledStime = cumuledStime;
            clone.resultNum = resultNum;
            clone.finalPos = finalPos;
            clone.finished = finished;
            clone.finPLID = finPLID;
            clone.totalTime = totalTime;
            clone.bestLap = bestLap;
            clone.lapBestLap = lapBestLap;
            clone.lapsLead = lapsLead;
            clone.tmpTime = tmpTime;
            clone.firstTime = firstTime;
            clone.avgTime = avgTime;
            clone.lapStability = lapStability;
            clone.lastSplit = lastSplit;

            // Copy flags
            clone.yellowFlags = yellowFlags;
            clone.inYellow = inYellow;
            clone.blueFlags = blueFlags;
            clone.inBlue = inBlue;
            clone.numPen = numPen;
            clone.contacts = contacts;
            clone.offTracks = offTracks;
            clone.flags = flags;

            // Copy arrays
            Array.Copy(BestSector, clone.BestSector, BestSector.Length);
            Array.Copy(BestSectorLap, clone.BestSectorLap, BestSectorLap.Length);
            Array.Copy(CurrentSplit, clone.CurrentSplit, CurrentSplit.Length);

            // Deep copy collections
            clone.allPlayers = new Dictionary<string, PlayerIdentity>(allPlayers);
            clone.lap = new List<Lap>(lap);
            clone.pit = new List<Pit>(pit);
            clone.pen = new List<Penalty>(pen);
            clone.toc = new List<Toc>(toc);
            clone.timingEvents = new List<TimingEvent>(timingEvents);  // Copy timing events for position tracking

            return clone;
        }
    }
}
