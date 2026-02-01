/*
	LFS_Stats, Insim Replay statistics for Live For Speed Game
	Copyright (C) 2008 Jaroslav Černý alias JackCY, Robert B. alias Gai-Luron and Monkster.
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
		public int UCID;						// connection ID
		public int PLID;						// player ID
		public string userName;
		public Dictionary<string, UN> allUN;
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

		public long bestSectorSortHelper;	// use parametrized comparer in SessionStats instead
		public long BestSectorLapHelper;	// use parametrized comparer in SessionStats instead
		public long curWrSplit;

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

		private PlayerFlags flags;
		public InSimDotNet.Packets.PlayerFlags Flags
		{
			get;
			set;
		}
        public string sFlags
        {
            get
            {
                return Flags != 0 ? Flags.ToString().Replace(",", string.Empty) : string.Empty;
            }
        }
        public List<Lap> lap = new List<Lap>();
		public List<Pit> pit = new List<Pit>();
		public List<Penalty> pen = new List<Penalty>();
		public List<Toc> toc = new List<Toc>();

		public SessionStats(int UCID, int PLID)
		{
			this.UCID = UCID;
			this.PLID = PLID;
			this.userName = "";
			this.allUN = new Dictionary<string, UN>();
			this.nickName = "";
			//this.sectorCount = 0;
			this.BestSector = new long[LFSStats.MaxSectorCount];
			this.BestSectorLap = new int[LFSStats.MaxSectorCount];
			//this.bestSplit1 = 0;
			//this.bestSplit2 = 0;
			//this.bestSplit3 = 0;
			//this.bestLastSplit = 0;
			this.bestSpeed = 0;
			this.numStop = 0;
			this.resultNum = 999;
			this.finished = false;
			this.finPLID = -1;
			this.totalTime = 0;
			this.carName = "";
			this.penalty = "";
			this.gridPos = 999;
			this.cumuledTime = 0;
			this.lapsLead = 0;
			this.firstTime = 0;
			this.avgTime = 0;
			this.CurrentSplit = new long[LFSStats.MaxSplitCount];
			//this.curSplit1 = 0;
			//this.curSplit2 = 0;
			//this.curSplit3 = 0;
			//this.lastSplit = 0;
			this.yellowFlags = 0;
			this.inBlue = false;
			this.inYellow = false;
			this.blueFlags = 0;
			this.cumuledStime = 0;
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
			sortPit = new PitCmp();
			sortTPB = new TPBCmp();
			sortPenalization = new PenalizationCmp();
		}

		public string GetPlayerFlagSteering()
		{
			if (flags.HasFlag(PlayerFlags.Mouse))
				return "Mouse";
			if (flags.HasFlag(PlayerFlags.KbNoHelp))
				return "Keyboard";
			if (flags.HasFlag(PlayerFlags.KbStabilised))
				return "KeyboardStabilised";
			else
				return "Wheel";
		}
		public string GetPlayerFlagBrakeHelp()
		{
			if (flags.HasFlag(PlayerFlags.BrakeHelp))
				return "BrakeHelp";
			else
				return "";
		}
		public string GetPlayerFlagAutoGearShift()
		{
			if (flags.HasFlag(PlayerFlags.AutoGearShift))
				return "AutoGearShift";
			else
				return "";
		}
		public string GetPlayerFlagManualShifter()
		{
			if (flags.HasFlag(PlayerFlags.Shifter))
				return "Shifter";
			else
				return "";
		}
		public string GetPlayerFlagAxisClutch()
		{
			if (flags.HasFlag(PlayerFlags.AxisClutch))
				return "AxisClutch";
			else
				return "";
		}
		public string GetPlayerFlagAutoClutch()
		{
			if (flags.HasFlag(PlayerFlags.AutoClutch))
				return "AutoClutch";
			else
				return "";
		}
		public string GetPlayerFlagDriverSide()
		{
			if (flags.HasFlag(PlayerFlags.SwapSide))	// Left side steering
				return "LeftHandDrive";
			else
				return "RightHandDrive";
		}
		public string GetPlayerFlagCustomView()
		{
			if (flags.HasFlag(PlayerFlags.CustomView))
				return "CustomView";
			else
				return "";
		}

		public static string LfsTimeToString(long val)
		{
			return InSimDotNet.Helpers.StringHelper.ToLapTimeString(TimeSpan.FromMilliseconds(val));
		}
		public static string LfsSpeedToString(int val)
		{
			return Math.Round(InSimDotNet.Helpers.MathHelper.SpeedToKph(val), 2).ToString();
		}

		public void UpdateSplit(int split, long splitTime)
		{
			if (this.finished == true)
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
			this.CurrentSplit[index] = splitTime;	// save split time
			sector = splitTime - lastSplit;			// NOTE added lastSplit = 0 on lap completion
			lastSplit = splitTime;

			if ((this.BestSector[index] == 0) || (sector < this.BestSector[index]))
			{
				this.BestSector[index] = sector;
				this.BestSectorLap[index] = this.lap.Count + 1;
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
		public void UpdateLap(long lapTime, int numStop, int lapsDone, int maxSplit)
		{
			if (this.finished == true)
				return;

			if (bestLap == 0 || lapTime < bestLap)	// save best lap time and lap number
			{
				bestLap = lapTime;
				lapBestLap = lapsDone;
			}
			this.numStop = numStop;
			this.cumuledTime = this.cumuledTime + lapTime;	// accumulate total time

			// Add a new Lap when lap is done
			this.lap.Add(new Lap(this.CurrentSplit, lapTime, this.cumuledTime));

			if (this.lap.Count == 1)
				this.firstTime = lapTime;

			// best last sector
			long sector = lapTime - lastSplit;
			lastSplit = 0;	// NOTE added for best sector 1 calculation

			if (maxSplit >= LFSStats.MaxSectorCount)
				throw new Exception("Cannot update sector " + maxSplit + ", this number of splits and sectors is not supported.");

			if ((this.BestSector[maxSplit] == 0) || (sector < this.BestSector[maxSplit]))
			{
				this.BestSector[maxSplit] = sector;
				this.BestSectorLap[maxSplit] = lapsDone;
			}
		}

		public void UpdatePen(int OldPen, int NewPen, int Reason)
		{
			if (this.finished == true)
				return;
			int LapDone = this.lap.Count + 1;
			this.pen.Add(new Penalty(LapDone, OldPen, NewPen, Reason));
			if (NewPen != (int)PenaltyValue.PENALTY_NONE)
				this.numPen++;
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
			if (this.finished == true)
				return;
		this.numStop = pitDec.NumStops;
		// Use the new Pit(IS_PIT) constructor from InSimDotNet
		this.pit.Add(new Pit(pitDec));
		}
		public void updateTOC(string oldNickName, string oldUserName, string newNickName, string newUserName)
		{
			if (this.finished == true)
				return;
			this.toc.Add(new Toc(oldNickName, oldUserName, newNickName, newUserName, this.lap.Count + 1));

		}
		public void updatePSF(int PLID, TimeSpan STime)
		{
			if (this.finished == true)
				return;
			if (pit.Count > 0)
			{
				(pit[pit.Count - 1] as Pit).STime = STime.TotalMillisecondsLong();
				this.cumuledStime += STime.TotalMillisecondsLong();
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
			if (this.finished == true)
				return;

			if (bestLap == 0 || BTime < bestLap)
			{
				bestLap = BTime;
				lapBestLap = LapsDone;
			}
			this.cumuledTime = this.cumuledTime + BTime;

			// Add a new Lap when lap is done
			this.lap.Add(new Lap(this.CurrentSplit, BTime, this.cumuledTime));

			// best last sector
			long sector = BTime - lastSplit;
			lastSplit = 0;	// NOTE added for best sector 1 calculation

			if (maxSplit >= LFSStats.MaxSectorCount)
				throw new Exception("Cannot update sector " + maxSplit + ", this number of splits and sectors is not supported.");

			if ((this.BestSector[maxSplit] == 0) || (sector < this.BestSector[maxSplit]))
			{
				this.BestSector[maxSplit] = sector;
				this.BestSectorLap[maxSplit] = LapsDone;
			}
		}
		public void UpdateResult(long totalTime, int resultNum, string CName, ConfirmationFlags confirm, int numStop)
		{
			this.totalTime = totalTime;
			this.resultNum = resultNum;
			this.carName = CName;
			this.numStop = numStop;

			this.penalty = "";
			if ((confirm & ConfirmationFlags.CONF_PENALTY_DT) != 0)
			{
				this.resultNum = 998;
				this.penalty += "DT";
			}
			if ((confirm & ConfirmationFlags.CONF_PENALTY_SG) != 0)
			{
				this.resultNum = 998;
				this.penalty += "SG";
			}
			if ((confirm & ConfirmationFlags.CONF_DID_NOT_PIT) != 0)
			{
				this.resultNum = 998;
				this.penalty += "DNP";
			}
			if ((confirm & ConfirmationFlags.CONF_PENALTY_30) != 0)
				this.penalty += "30s";
			if ((confirm & ConfirmationFlags.CONF_PENALTY_45) != 0)
				this.penalty += "45s";
		}

		/// <summary>
		/// Compares to this instance. Using SORT_RESULT.
		/// </summary>
		/// <param name="other">The other instance.</param>
		/// <returns>-1 if this is lower, 0 equal, 1 higher</returns>
		public Int32 CompareTo(SessionStats other)
		{
			if (other == null) return -1;

			// SORT_RESULT
			if (other.resultNum < this.resultNum)
				return 1;
			if (other.resultNum > this.resultNum)
				return -1;
			if (other.lap.Count > this.lap.Count)
				return 1;
			if (other.lap.Count < this.lap.Count)
				return -1;
			return 0;
		}

		/// <summary>
		/// Comparator, you can also use default SessionStats IComparable.CompareTo in this case of sorting by result.
		/// </summary>
		private class ResultCmp : IComparer<SessionStats>
		{
			/// <summary>
			/// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
			/// </summary>
			/// <param name="x">The first object to compare.</param>
			/// <param name="y">The second object to compare.</param>
			/// <returns>A signed integer that indicates the relative values of x and y, as shown in the following table.
			/// Value Meaning Less than zero x is less than y. Zero x equals y. Greater than zero x is greater than y.</returns>
			public Int32 Compare(SessionStats x, SessionStats y)
			{
				if (y.resultNum < x.resultNum)
					return 1;
				if (y.resultNum > x.resultNum)
					return -1;
				if (y.lap.Count > x.lap.Count)
					return 1;
				if (y.lap.Count < x.lap.Count)
					return -1;
				return 0;
			}
		}
		public static IComparer<SessionStats> GetResultCmp()
		{
			return sortResult;
		}
		private class GridCmp : IComparer<SessionStats>
		{
			public Int32 Compare(SessionStats x, SessionStats y)
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
			public Int32 Compare(SessionStats x, SessionStats y)
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
			public Int32 Compare(SessionStats x, SessionStats y)
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
			public Int32 Compare(SessionStats x, SessionStats y)
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
			public Int32 Compare(SessionStats x, SessionStats y)
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
			public Int32 Compare(SessionStats x, SessionStats y)
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
			public Int32 Compare(SessionStats x, SessionStats y)
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
			public Int32 Compare(SessionStats x, SessionStats y)
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
			public Int32 Compare(SessionStats x, SessionStats y)
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
			public Int32 Compare(SessionStats x, SessionStats y)
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
			public Int32 Compare(SessionStats x, SessionStats y)
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
			public Int32 Compare(SessionStats x, SessionStats y)
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
		private class PitCmp : IComparer<SessionStats>
		{
			public Int32 Compare(SessionStats x, SessionStats y)
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
			public Int32 Compare(SessionStats x, SessionStats y)
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
	}
}
