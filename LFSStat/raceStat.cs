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
using System.Text;

namespace LFSStat
{
    enum sortRaceStats
    {
        SORT_RESULT,
        SORT_GRID,
        SORT_CLIMB,
        SORT_LAPLEAD,
        SORT_FIRSTLAP,
        SORT_AVGTIME,
        SORT_BESTLAP,
        SORT_BESTSPLIT,
        SORT_BESTSPEED,
        SORT_STABILITY,
        SORT_BLUEFLAG,
        SORT_YELLOWFLAG,
        SORT_PIT,
        SORT_TPB,
	    SORT_PEN
    }
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
        public long split1;
        public long split2;
        public long split3;
        public long lapTime;
        public long cumuledTime;
        public Lap(long split1, long split2, long split3, long lapTime, long cumuledTime)
        {
            this.split1 = split1;
            this.split2 = split2;
            this.split3 = split3;
            this.lapTime = lapTime;
            this.cumuledTime = cumuledTime;
        }
    }
    class Pit
    {
        public int LapsDone;
        public int Flags;
        public int Penalty;
        public int NumStop;
        public int rearL;
        public int rearR;
        public int frontL;
        public int frontR;
        public long Work;
        public long STime;
        public Pit(int LapsDone, int Flags, int Penalty, int NumStop, int rearL, int rearR, int frontL, int frontR, long Work )
        {
            this.LapsDone = LapsDone;
            this.Flags = Flags;
            this.Penalty = Penalty;
            this.NumStop = NumStop;
            this.rearL = rearL;
            this.rearR = rearR;
            this.frontL = frontL;
            this.frontR = frontR;
            this.Work = Work;
            this.STime = 0;
        }
    }
    class raceStats : System.IComparable
    {
        public static int modeSort = (int)sortRaceStats.SORT_RESULT;
        public int UCID;
        public int PLID;
        public string userName;
        public System.Collections.Hashtable allUN;
        public string nickName;
        public string Plate;
        public long bestSplit1;
        public int lapBestSplit1;
        public long bestSplit2;
        public int lapBestSplit2;
        public long bestSplit3;
        public int lapBestSplit3;
        public long bestLastSplit;
        public int lapBestLastSplit;
        public long cumuledTime;
        public int bestSpeed;
        public int lapBestSpeed;
        public int numStop;
        public long cumuledStime; // Cumuled Pit Time
        public int resultNum;
        public int finalPos;
        public bool finished = false;
        public int finPLID;
        public long totalTime;
        public long bestLap;
        public int lapBestLap;
        public string CName;
        public string penalty;
        public int gridPos;
        public int lapsLead;
        public long tmpTime;
        public long firstTime;
        public long avgTime;
        public long curBestSplit;
        public long curWrSplit;
        public long curLapBestSplit;
        public double lapStability;
        public long curSplit1;
        public long curSplit2;
        public long curSplit3;
        public int yellowFlags;
        public bool inYellow;
        public int blueFlags;
        public bool inBlue;
        public string sFlags;
        public int numPen = 0;
        public System.Collections.ArrayList lap = new System.Collections.ArrayList();
        public System.Collections.ArrayList pit = new System.Collections.ArrayList();
        public System.Collections.ArrayList pen = new System.Collections.ArrayList();
        public System.Collections.ArrayList toc = new System.Collections.ArrayList();

        
        long lastSplit;
        int CurrIdxSplit;


        public raceStats(int UCID, int PLID )
        {
            this.UCID = UCID;
            this.PLID = PLID;
            this.userName = "";
            this.allUN = new System.Collections.Hashtable();
            this.nickName = "";
            this.bestSplit1 = 0;
            this.bestSplit2 = 0;
            this.bestSplit3 = 0;
            this.bestLastSplit = 0;
            this.bestSpeed = 0;
            this.numStop = 0;
            this.resultNum = 999;
            this.finished = false;
            this.finPLID = -1;
            this.totalTime = 0;
            this.CName = "";
            this.penalty = "";
            this.gridPos = 999;
            this.cumuledTime = 0;
            this.lapsLead = 0;
            this.firstTime = 0;
            this.avgTime = 0;
            this.curSplit1 = 0;
            this.curSplit2 = 0;
            this.curSplit3 = 0;
            this.yellowFlags = 0;
            this.inBlue = false;
            this.inYellow = false;
            this.blueFlags = 0;
            this.cumuledStime = 0;
        }
        public static string LfstimeToString(long val)
        {
            long sec;
            long min;
            long hun;
            string sign = "";

            if (val < 0)
            {
                val = -val;
                sign = "-";
            }
            sec = val / 1000;
            hun = sec * 1000;
            hun = (val - hun) / 10;

            val = sec;
            min = val / 60;
            sec = min * 60;
            sec = val - sec;
            return string.Format("{3}{0}:{1,2:D2}.{2,2:D2}", min, sec, hun, sign );
        }
        public static string LfsSpeedToString(int val)
        {
            float speed;
            speed = ((float)val * 45) / 4096 ;
            return ( string.Format( "{0:F2}" , speed ) );
        }

        public int CompareTo(object x)
        {
            switch( modeSort ){
                case (int)sortRaceStats.SORT_RESULT:
                    if ((x as raceStats ).resultNum < resultNum)
                        return 1;
                    if ((x as raceStats).resultNum > resultNum)
                        return -1;
                    else
                    {
                        if ((x as raceStats).lap.Count > lap.Count)
                            return 1;
                        if ((x as raceStats).lap.Count < lap.Count)
                            return -1;
                        else
                            return 0;
                    }
                    break;
                case (int)sortRaceStats.SORT_GRID:
                    if ((x as raceStats).gridPos < gridPos)
                        return 1;
                    if ((x as raceStats).gridPos > gridPos)
                        return -1;
                    else
                        return 0;
                    break;
                case (int)sortRaceStats.SORT_CLIMB:
                    if (((x as raceStats).resultNum - (x as raceStats).gridPos) < (resultNum - gridPos))
                        return 1;
                    if (((x as raceStats).resultNum - (x as raceStats).gridPos) > (resultNum - gridPos))
                        return -1;
                    else
                        return 0;
                    break;
                case (int)sortRaceStats.SORT_LAPLEAD:
                    if ((x as raceStats).lapsLead > lapsLead)
                        return 1;
                    if ((x as raceStats).lapsLead < lapsLead)
                        return -1;
                    else
                        return 0;
                    break;
                case (int)sortRaceStats.SORT_FIRSTLAP:
                    if ((x as raceStats).firstTime < firstTime)
                        return 1;
                    if ((x as raceStats).firstTime > firstTime)
                        return -1;
                    else
                        return 0;
                    break;
                case (int)sortRaceStats.SORT_AVGTIME:
                    if ((x as raceStats).avgTime < avgTime)
                        return 1;
                    if ((x as raceStats).avgTime > avgTime)
                        return -1;
                    else
                        return 0;
                    break;
                case (int)sortRaceStats.SORT_BESTLAP:
                    if ((x as raceStats).bestLap < bestLap)
                        return 1;
                    if ((x as raceStats).bestLap > bestLap)
                        return -1;
                    else
                        return 0;
                    break;
                case (int)sortRaceStats.SORT_BESTSPLIT:
                    if ((x as raceStats).curBestSplit < curBestSplit)
                        return 1;
                    if ((x as raceStats).curBestSplit > curBestSplit)
                        return -1;
                    else
                        return 0;
                    break;
                case (int)sortRaceStats.SORT_TPB:
                    long objBest = (x as raceStats).bestSplit1 + (x as raceStats).bestSplit2 + (x as raceStats).bestSplit3 + (x as raceStats).bestLastSplit;
                    long Best = bestSplit1 + bestSplit2 + bestSplit3 + bestLastSplit;
                    if (objBest < Best)
                        return 1;
                    if (objBest > Best)
                        return -1;
                    else
                        return 0;
                    break;
                case (int)sortRaceStats.SORT_STABILITY:
                    if ((x as raceStats).lapStability < lapStability)
                        return 1;
                    if ((x as raceStats).lapStability > lapStability)
                        return -1;
                    else
                        return 0;
                    break;
                case (int)sortRaceStats.SORT_BESTSPEED:
                    if ((x as raceStats).bestSpeed > bestSpeed)
                        return 1;
                    if ((x as raceStats).bestSpeed < bestSpeed)
                        return -1;
                    else
                        return 0;
                    break;
                case (int)sortRaceStats.SORT_BLUEFLAG:
                    if ((x as raceStats).blueFlags > blueFlags)
                        return 1;
                    if ((x as raceStats).blueFlags < blueFlags)
                        return -1;
                    else
                        return 0;
                    break;
                case (int)sortRaceStats.SORT_YELLOWFLAG:
                    if ((x as raceStats).yellowFlags > yellowFlags)
                        return 1;
                    if ((x as raceStats).yellowFlags < yellowFlags)
                        return -1;
                    else
                        return 0;
                    break;
                case (int)sortRaceStats.SORT_PIT:
                    if ((x as raceStats).pit.Count < pit.Count)
                        return 1;
                    if ((x as raceStats).pit.Count > pit.Count)
                        return -1;
                    else
                    {
                        if ((x as raceStats).cumuledStime < cumuledStime)
                            return 1;
                        if ((x as raceStats).cumuledStime > cumuledStime)
                            return -1;
                        else
                            return 0;
                    }
                    break;
                case (int)sortRaceStats.SORT_PEN:
                    if ((x as raceStats).numPen > numPen)
                        return 1;
                    if ((x as raceStats).numPen < numPen)
                        return -1;
                    else
                        return 0;
                    break;


                default:
                    return 0;
                }
                
        }

        public void UpdateLap(long LTime, int numStop, int lapsDone )
        {
            long diffSplit;

            if (this.finished == true)
                return;

            if (bestLap == 0 || LTime < bestLap)
            {
                bestLap = LTime;
                lapBestLap = lapsDone;
            }
            this.numStop = numStop;
            this.cumuledTime = this.cumuledTime + LTime;
            diffSplit = LTime - lastSplit;
            if ((this.bestLastSplit == 0) || (diffSplit < this.bestLastSplit))
            {
                this.bestLastSplit = diffSplit;
                this.lapBestLastSplit = lapsDone;
            }
// Add a new Lap when lap is done
            CurrIdxSplit = this.lap.Add(new Lap( curSplit1 , curSplit2, curSplit3, LTime, this.cumuledTime  ));
            if (CurrIdxSplit == 0)
                this.firstTime = LTime;
            

        }
        public void UpdatePen( int OldPen, int NewPen, int Reason)
        {

            if (this.finished == true)
                return;
            int LapDone = this.lap.Count + 1 ;
            int CurrIdxSplit = this.pen.Add(new Penalty(LapDone , OldPen,NewPen,Reason));
            if (NewPen != (int)InSim.pen.PENALTY_NONE)
                this.numPen++;
/*
            Console.WriteLine( this.nickName + ":"
                                + "New pen : " + Enum.GetName(typeof(InSim.pen), NewPen)
                                + "Old pen : " + Enum.GetName(typeof(InSim.pen), OldPen)
            );
*/
        }
        public void UpdateSplit(int split, long STime )
        {

            long diffSplit;

            if (this.finished == true)
                return;

            if (split == 1){
// On first Split Create a new Lap
//                CurrIdxSplit = this.lap.Add(new Lap(0, 0, 0, 0));
//                (this.lap[CurrIdxSplit] as Lap).split1 = STime;
                this.curSplit1 = STime;
                lastSplit = 0;
                diffSplit = STime;
                if ((this.bestSplit1 == 0) || (diffSplit < this.bestSplit1))
                {
                    this.bestSplit1 = diffSplit;
                    this.lapBestSplit1 = this.lap.Count;
                }
                lastSplit = STime;
            }
            if (split == 2)
            {
//                (this.lap[CurrIdxSplit] as Lap).split2 = STime;
                diffSplit = STime - lastSplit;
                this.curSplit2 = STime;
                if ((this.bestSplit2 == 0) || (diffSplit < this.bestSplit2))
                {
                    this.bestSplit2 = diffSplit;
                    this.lapBestSplit2 = this.lap.Count;
                }
                lastSplit = STime;
            }
            if (split == 3)
            {
//                (this.lap[CurrIdxSplit] as Lap).split3 = STime;
                diffSplit = STime - lastSplit;
                this.curSplit3 = STime ;
                if ((this.bestSplit3 == 0) || (diffSplit < this.bestSplit3))
                {
                    this.bestSplit3 = diffSplit;
                    this.lapBestSplit3 = this.lap.Count;
                }
                lastSplit = STime;
            }
        }
        public void updateMCI(int speed)
        {
            if (bestSpeed < speed && finished == false)
            {
                bestSpeed = speed;
                lapBestSpeed = lap.Count + 1;
                //                Console.WriteLine(nickName + " : " + speed);
            }
        }
        public void updatePIT(InSim.Decoder.PIT pitDec)
        {
            int CurrIdxSplit;

            if (this.finished == true)
                return;
            this.numStop = pitDec.NumStop;
            CurrIdxSplit = this.pit.Add(new Pit(pitDec.LapsDone, pitDec.Flags, pitDec.Penalty, pitDec.NumStop, pitDec.rearL, pitDec.rearR, pitDec.frontL, pitDec.frontR, pitDec.Work));


        }
        public void updateTOC(string oldNickName, string oldUserName, string newNickName, string newUserName)
        {
            int CurrIdxSplit;

            if (this.finished == true)
                return;

            CurrIdxSplit = this.toc.Add(new Toc( oldNickName,oldUserName,newNickName,newUserName,this.lap.Count+1));


        }
        public void updatePSF(int PLID, long STime)
        {
            if (this.finished == true)
                return;
            if (pit.Count > 0)
            {
                (pit[pit.Count - 1] as Pit).STime = STime;
                this.cumuledStime += STime;
            }

        }
        public void UpdateQualResult( long TTime,long BTime,int NumStops, int Confirm, int LapDone, int ResultNum, int NumRes)
        {
/*
            Console.WriteLine("TTime:" + TTime
                                + " BTime:" + BTime
                                + " NumStops:" + NumStops
                                + " Confirm:" + Confirm
                                + " LapDone:" + LapDone
                                + " ResultNum:" + ResultNum
                                + " NumRes:" + NumRes);
 */
            long diffSplit;
            if (this.finished == true)
                return;

            if (bestLap == 0 || BTime < bestLap)
            {
                bestLap = BTime;
                lapBestLap = this.lap.Count + 1;
            }
            this.cumuledTime = this.cumuledTime + BTime;
            diffSplit = BTime - lastSplit;
            if ((this.bestLastSplit == 0) || (diffSplit < this.bestLastSplit))
            {
                this.bestLastSplit = diffSplit;
                this.lapBestLastSplit = this.lap.Count + 1;
            }

            // Add a new Lap when lap is done
            CurrIdxSplit = this.lap.Add(new Lap(curSplit1, curSplit2, curSplit3, BTime, this.cumuledTime));

        }
        public void UpdateResult(long totalTime, int resultNum, string CName, int confirm, int numStop)
        {
                this.totalTime = totalTime;
                this.resultNum = resultNum;
                this.CName = CName;
                this.numStop = numStop;

                this.penalty = "";
                if ((confirm & (int)InSim.confirm.CONF_PENALTY_DT) != 0)
                {
                    this.resultNum = 998;
                    this.penalty += "DT ";
                }
                if ((confirm & (int)InSim.confirm.CONF_PENALTY_SG) != 0)
                {
                    this.resultNum = 998;
                    this.penalty += "S&G";
                }
                if ((confirm & (int)InSim.confirm.CONF_DID_NOT_PIT) != 0)
                {
                    this.resultNum = 998;
                    this.penalty += "DNP";
                }
                if ((confirm & (int)InSim.confirm.CONF_PENALTY_30) != 0)
                {
                    this.penalty += "30 Sec.";
                }
                if ((confirm & (int)InSim.confirm.CONF_PENALTY_45) != 0)
                {
                    this.penalty += "30 Sec.";
                }
                
        }


    }
}
