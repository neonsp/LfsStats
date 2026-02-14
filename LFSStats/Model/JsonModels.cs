using System.Collections.Generic;

namespace LFSStatistics
{
    // Root object
    public class RaceDataJson
    {
        public MetadataJson metadata { get; set; }
        public SessionInfoJson session { get; set; }
        public List<PlayerInfo> players { get; set; }
        public List<CarEntry> cars { get; set; }
        public RankingsJson rankings { get; set; }
        public EventsJson events { get; set; }
        public List<ChatMessage> chat { get; set; }
        public Dictionary<string, WorldRecord> worldRecords { get; set; }
    }

    // Session info
    public class SessionInfoJson
    {
        public string type { get; set; }  // "race", "qual", "practice"
        public string track { get; set; }
        public string trackName { get; set; }
        public string car { get; set; }
        public int laps { get; set; }
        public int sessionTime { get; set; }
        public string sessionLength { get; set; }
        public string date { get; set; }
        public string time { get; set; }
        public int splitsPerLap { get; set; }
        public List<string> flags { get; set; }
        public string server { get; set; }
        public int utcOffset { get; set; }
        public string wind { get; set; }
        public List<string> allowedCars { get; set; }
        public Dictionary<string, string> carImages { get; set; }
    }

    // Metadata
    public class MetadataJson
    {
        public string exportedAt { get; set; }
        public string mprUrl { get; set; }
        public string logoUrl { get; set; }
    }

    // Player entry (referenced by index)
    public class PlayerInfo
    {
        public string username { get; set; }
        public string name { get; set; }
        public string nameColored { get; set; }
    }

    // Car entry (race result per car/PLID)
    public class CarEntry
    {
        public int plid { get; set; }
        public int lastDriver { get; set; }  // player index
        public string car { get; set; }
        public int position { get; set; }
        public int gridPosition { get; set; }
        public string status { get; set; }
        public int lapsCompleted { get; set; }
        public string totalTime { get; set; }
        public string bestLapTime { get; set; }
        public int? bestLapNumber { get; set; }
        public double? topSpeed { get; set; }
        public int? topSpeedLap { get; set; }
        public List<DriverStint> stints { get; set; }
        public List<string> lapTimes { get; set; }
        public List<double> lapETimes { get; set; }
        public int[] positions { get; set; }
        public List<PitStop> pitStops { get; set; }
        public List<StatPenalty> penalties { get; set; }
        public List<string> bestSplits { get; set; }
        public IncidentCounts incidents { get; set; }
    }

    // Driver stint (relay)
    public class DriverStint
    {
        public int driver { get; set; }  // player index
        public int fromLap { get; set; }
        public int toLap { get; set; }
    }

    // Incident counts per car
    public class IncidentCounts
    {
        public int yellowFlags { get; set; }
        public int blueFlags { get; set; }
        public int contacts { get; set; }
        public int offTracks { get; set; }
    }

    public class PitStop
    {
        public int lap { get; set; }
        public string duration { get; set; }
        public List<string> reason { get; set; }
    }

    public class StatPenalty
    {
        public int lap { get; set; }
        public string type { get; set; }
        public string value { get; set; }
    }

    // Rankings
    public class RankingsJson
    {
        public FastestLapInfo fastestLap { get; set; }
        public List<FirstLapRanking> firstLap { get; set; }
        public List<TopSpeedRanking> topSpeed { get; set; }
        public List<PitStopRanking> pitStops { get; set; }
    }

    public class FastestLapInfo
    {
        public int driver { get; set; }  // player index
        public string time { get; set; }
        public int lap { get; set; }
    }

    public class FirstLapRanking
    {
        public int position { get; set; }
        public int driver { get; set; }  // player index
        public string time { get; set; }
        public string gap { get; set; }
    }

    public class TopSpeedRanking
    {
        public int position { get; set; }
        public int driver { get; set; }  // player index
        public double speed { get; set; }
        public double gap { get; set; }
        public int lap { get; set; }
    }

    public class PitStopRanking
    {
        public int position { get; set; }
        public int driver { get; set; }  // player index
        public int lap { get; set; }
        public string duration { get; set; }
        public List<string> reason { get; set; }
    }

    // Events
    public class EventsJson
    {
        public List<OvertakeInfo> overtakes { get; set; }
        public List<Incident> incidents { get; set; }
    }

    public class OvertakeInfo
    {
        public int overtaker { get; set; }  // player index
        public int overtaken { get; set; }  // player index
        public int lap { get; set; }
        public double gap { get; set; }
    }

    public class Incident
    {
        public int lap { get; set; }
        public string time { get; set; }
        public string type { get; set; }
        public List<int> drivers { get; set; }  // player indices
        public string description { get; set; }
    }

    // Chat
    public class ChatMessage
    {
        public int driver { get; set; }  // player index
        public string message { get; set; }
    }

    // World Records
    public class WorldRecord
    {
        public string lapTime { get; set; }
        public string racer { get; set; }
        public List<string> splits { get; set; }
        public List<string> sectors { get; set; }
        public string track { get; set; }
        public string car { get; set; }
    }
}
