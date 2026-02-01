using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotNetExtensions;

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
		public int raceLaps = 0;
		public string sraceLaps = "";
		public int qualMins = 0;
		public string HName = "";	// what is this?
		public int currentLap = 0;
		public bool isToc = false;
		public Session session = Session.None;
		public TupleList<string, string> chat = new TupleList<string, string>();

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
			raceLaps = sessionInfo.raceLaps;
			sraceLaps = sessionInfo.sraceLaps;
			qualMins = sessionInfo.qualMins;
			HName = sessionInfo.HName;
			currentLap = sessionInfo.currentLap;
			isToc = sessionInfo.isToc;
			session = sessionInfo.session;
			chat = new TupleList<string, string>(sessionInfo.chat);
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

			if (raceLaps == 0)			// Practice
				return "";
			else if (raceLaps == 1)		// 1 lap
				ret = sraceLaps + space + "lap";
			else if (raceLaps < 191)	// 2-1000 laps
				ret = sraceLaps + space + "laps";
			else if (raceLaps == 191)	// 1 hour
				ret = sraceLaps + space + "hour";
			else						// 2-48 hours
				ret = sraceLaps + space + "hours";

			if (qualMins == 1)
				ret = qualMins + space + "min";
			if (qualMins > 1)
				ret = qualMins + space + "mins";

			return ret + space + "of" + space + trackNameCode;
		}
	}
}
