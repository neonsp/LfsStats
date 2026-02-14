using System;
using System.Collections.Generic;

namespace LFSStatistics
{
    /// <summary>
    /// Race information
    /// </summary>
    class SessionInfo
    {
        public string trackNameCode = "";
        public int maxSplit = 0;
        public int weather = 0;
        public int wind = 0;
        public string gameDate = "";      // Date from /time command (e.g. "2026-08-01")
        public string gameTime = "";      // Time from /time command (e.g. "09:00")
        public int utcOffset = 0;         // UTC offset from /time command (e.g. -5)
        public bool gameTimeReceived = false;  // Whether /time response was parsed
        public int raceLaps = 0;
        public string sraceLaps = "";
        public int qualMins = 0;
        public string HName = "";   // what is this?
        public int currentLap = 0;
        public bool isToc = false;
        public Session session = Session.None;
        public List<ChatEntry> chat = new List<ChatEntry>();
        public DateTime sessionStartTime = DateTime.MinValue;  // Real session start time
        public long allowedCarFlags = 0;     // Bitmask from SMALL_ALC
        public List<string> allowedMods = new List<string>();  // SkinIDs from IS_MAL
        public List<OvertakeEvent> overtakes = new List<OvertakeEvent>();  // Overtake events detected during race
        public int rstQualMins = 0;   // QualMins from IS_RST (reliable, unlike IS_STA which updates after session ends)
        public int rstRaceLaps = 0;   // RaceLaps from IS_RST (reliable)
        public int hostFlags = 0;     // Host flags from IS_RST (RaceFlags enum)

        internal enum Session
        {
            None, Practice, Qualification, Race
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionInfo"/> class.
        /// </summary>
        public SessionInfo() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="SessionInfo"/> class.
        /// Deep copy constructor.
        /// </summary>
        /// <param name="sessionInfo">The SessionInfo isntance to copy.</param>
        public SessionInfo(SessionInfo sessionInfo)
        {
            trackNameCode = sessionInfo.trackNameCode;
            maxSplit = sessionInfo.maxSplit;
            weather = sessionInfo.weather;
            wind = sessionInfo.wind;
            gameDate = sessionInfo.gameDate;
            gameTime = sessionInfo.gameTime;
            utcOffset = sessionInfo.utcOffset;
            gameTimeReceived = sessionInfo.gameTimeReceived;
            raceLaps = sessionInfo.raceLaps;
            sraceLaps = sessionInfo.sraceLaps;
            qualMins = sessionInfo.qualMins;
            HName = sessionInfo.HName;
            currentLap = sessionInfo.currentLap;
            isToc = sessionInfo.isToc;
            session = sessionInfo.session;
            chat = new List<ChatEntry>(sessionInfo.chat);
            sessionStartTime = sessionInfo.sessionStartTime;
            overtakes = new List<OvertakeEvent>(sessionInfo.overtakes);
            rstQualMins = sessionInfo.rstQualMins;
            rstRaceLaps = sessionInfo.rstRaceLaps;
            hostFlags = sessionInfo.hostFlags;
        }

        internal bool InSession()
        {
            return (session > Session.None) ? true : false;
        }
        internal bool InPractice()
        {
            return (session == Session.Practice) ? true : false;
        }
        internal bool InQualification()
        {
            return (session == Session.Qualification) ? true : false;
        }
        internal bool InRace()
        {
            return (session == Session.Race) ? true : false;
        }
        internal void ExitSession()
        {
            session = Session.None;
        }
        internal void EnterPractice()
        {
            session = Session.Practice;
        }
        internal void EnterQualification()
        {
            session = Session.Qualification;
        }
        internal void EnterRace()
        {
            session = Session.Race;
        }
        internal string SessionName()
        {
            return session.ToString();
        }
        internal string SessionLengthAndLocation(char space)
        {
            string ret;

            if (raceLaps == 0)          // Practice
                return "";
            else if (raceLaps == 1)     // 1 lap
                ret = sraceLaps + space + "lap";
            else if (raceLaps < 191)    // 2-1000 laps
                ret = sraceLaps + space + "laps";
            else if (raceLaps == 191)   // 1 hour
                ret = sraceLaps + space + "hour";
            else                        // 2-48 hours
                ret = sraceLaps + space + "hours";

            if (qualMins == 1)
                ret = qualMins + space + "min";
            if (qualMins > 1)
                ret = qualMins + space + "mins";

            return ret + space + "of" + space + trackNameCode;
        }
    }

    /// <summary>
    /// Represents an overtake event detected during the race
    /// </summary>
    class OvertakeEvent
    {
        public string overtaker;      // userName of driver who overtook
        public string overtaken;      // userName of driver who was overtaken
        public int lap;               // Lap number when overtake occurred
        public int timingPoint;       // Index of timing point where detected
        public long gapMs;            // Gap in milliseconds at previous timing point
        public long eTime;            // ETime when overtake was detected

        public OvertakeEvent(string overtaker, string overtaken, int lap, int timingPoint, long gapMs, long eTime)
        {
            this.overtaker = overtaker;
            this.overtaken = overtaken;
            this.lap = lap;
            this.timingPoint = timingPoint;
            this.gapMs = gapMs;
            this.eTime = eTime;
        }
    }
}
