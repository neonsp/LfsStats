/*
	LFSStats, statistics generator for Live for Speed
	Copyright © 2008-2013 Jaroslav Èerný
	
	This file is part of LFSStats.
	
	LFSStats is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.
	
	LFSStats is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
	GNU General Public License for more details.
	
	You should have received a copy of the GNU General Public License
	along with LFSStats. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Reflection;

// TODO check cargame.nl stats when somebody may have done a split but not a complete lap and it includes them in stats
// on top with weird impossible lap time of best possible lap
// TODO new tracks as background
// TODO Qualification
// TODO Practice
// TODO IS.NET
// TODO CSS, maybe make it one page and scroll down/up with fixed menu
// TODO 
// NEXT if configfile is not found, defaults are used

namespace LFSStatistics
{
	static class LFSStats
	{
		#region AssemblyInformation
		private static string aiTitle = ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyTitleAttribute), false)).Title;
		public static string Title
		{
			get { return aiTitle; }
		}
		private static string titleLong = "LFS Statistics";
		public static string TitleLong
		{
			get { return titleLong; }
		}
		private static string aiDescription = ((AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyDescriptionAttribute), false)).Description;
		public static string Description
		{
			get { return aiDescription; }
		}
		private static string aiVersionShort = Assembly.GetExecutingAssembly().GetName().Version.Major + "." + Assembly.GetExecutingAssembly().GetName().Version.Minor;
		public static string VersionShort
		{
			get { return aiVersionShort; }
		}
		public static string VersionVShort
		{
			get { return "v" + aiVersionShort; }
		}
		private static string aiVersion = Assembly.GetExecutingAssembly().GetName().Version + " (" + Assembly.GetExecutingAssembly().GetName().ProcessorArchitecture + ")";
		public static string Version
		{
			get { return aiVersion; }
		}
		public static string VersionV
		{
			get { return "v" + aiVersion; }
		}
		private static string aiCopyright = ((AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyCopyrightAttribute), false)).Copyright;
		public static string Copyright
		{
			get { return aiCopyright; }
		}
		public static string CopyrightC
		{
			get { return aiCopyright.Replace("\u00A9", "(C)"); }
		}
		private static string heading = Title + " " + VersionVShort;
		public static string Heading
		{
			get { return heading; }
		}
		//private static string consoleTitle = aiTitle + " " + aiVersion + " " + aiCopyright;
		private static string consoleTitle = aiTitle;
		public static string ConsoleTitle
		{
			get { return consoleTitle; }
		}
		//private static string processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
		private static string processName = Assembly.GetExecutingAssembly().GetName().Name;
		public static string ProcessName
		{
			get { return processName; }
		}
		private static string license = Title + " comes with ABSOLUTELY NO WARRANTY.\n" +
			"This is free software, and you are welcome to redistribute it\n" +
			"under certain conditions. Read LICENSE for details.\n" +
			"You should have received a copy of the GNU General Public License\n" +
			"along with " + Title + ". If not, see <http://www.gnu.org/licenses/>.";
		public static string License
		{
			get { return license; }
		}
		private static string licenseName = "GNU General Public License Version 3";
		public static string LicenseName
		{
			get { return licenseName; }
		}
		private static string licenseNameShort = "GNU GPLv3";
		public static string LicenseNameShort
		{
			get { return licenseNameShort; }
		}
		private static string licenseUrl = "http://www.gnu.org/licenses/";
		public static string LicenseUrl
		{
			get { return licenseUrl; }
		}
		private static string projectUrl = "http://github.com/lfs-statistics/"; // TODO create GITHUB/SourceForge/Codeplex lfsstats, lfsstatistics
		public static string ProjectUrl
		{
			get { return projectUrl; }
		}
		#endregion

		#region Constants
		internal const char filePathSpaceChar = '_';
		internal const int MaxSectorCount = 4;					// NOTE LFS Insim seems to only have splits 1,2,3 defined, that is 4 sectors at most
		internal const int MaxSplitCount = MaxSectorCount - 1;	// NOTE LFS Insim seems to only have splits 1,2,3 defined
		private const int ConsoleLeftColumnWidth = 6;
		private const int ConsoleTimeColumnWidth = 8;
		#endregion

		private static Options options = new Options();

		/// <summary>
		/// Prints header information
		/// </summary>
		static void PrintHeader()
		{
			Console.WriteLine(Heading + "\n" + CopyrightC + "\n\n" + License);
		}

		/// <summary>
		/// Prints version information
		/// </summary>
		static void PrintVersion()
		{
			Console.WriteLine(ProcessName + " (" + Title + ") " + Version + "\n" + CopyrightC + "\n\n" + License);
		}

		public static void ConsoleTitleRestore()
		{
			Console.Title = ConsoleTitle;
		}
		public static void ConsoleTitleShow(string str)
		{
			Console.Title = str + " - " + ConsoleTitle;
		}

		[STAThread]
		static void Main(string[] args)
		{
			// Handles all unhandled exceptions, run Debug version to see the line number of error
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionHandler);

			ConsoleTitleRestore();

			if (CommandLine.Parser.Default.ParseArgumentsStrict(args, options))
			{
				// version
				if (options.Version)
				{
					PrintVersion();
					return;
				}
				//TODO CSV, TSV user names
				PrintHeader();

				LFSClient lfsclient = new LFSClient(options.ConfigFileName, options.RefreshInterval);
			}
		}

		/// <summary>
		/// Unhandled Exception handler
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="eventArgs">The <see cref="System.UnhandledExceptionEventArgs"/> instance containing the event data.</param>
		static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs eventArgs)
		{
			Exception ex = (Exception)eventArgs.ExceptionObject;
			ErrorWriteException("Unhandled exception:", ex);
			//if (eventArgs.IsTerminating)
			//{
			//    Console.WriteLine("\nPress any key to exit.");
			//    Console.ReadKey();
			//}
		}

		/// <summary>
		/// Writes the specified string value to standard output based on allowed level of verbosity
		/// </summary>
		/// <param name="msg">The string.</param>
		/// <param name="vrb">The verbose level.</param>
		public static void WriteLine(string msg, Verbose vrb)
		{
			if (vrb <= options.Verbose)
				Console.WriteLine(msg);
		}
		public static void WriteLine(string header, string msg, Verbose vrb)
		{
			if (vrb <= options.Verbose)
				Console.WriteLine(header.PadLeft(ConsoleLeftColumnWidth) + ": " + msg);
		}
		public static void WriteLine(string header, int place, string name, Verbose vrb)
		{
			if (vrb <= options.Verbose)
				Console.WriteLine(header.PadLeft(ConsoleLeftColumnWidth) + ": " + (place.ToString() + ".").PadLeft(3) + " " + name);

		}
		public static void WriteLine(string header, long lfsTime, string msg, Verbose vrb)
		{
			if (vrb <= options.Verbose)
				Console.WriteLine(header.PadLeft(ConsoleLeftColumnWidth) + ": "
					+ SessionStats.LfsTimeToString(lfsTime).PadLeft(ConsoleTimeColumnWidth) + " " + msg);
		}
		/// <summary>
		/// Writes the specified string value to standard output
		/// </summary>
		/// <param name="str">The string.</param>
		/// <param name="vrb">The verbose level.</param>
		public static void WriteLine(string str)
		{
			Console.WriteLine(str);
		}
		/// <summary>
		/// Writes the specified string value to standard output
		/// </summary>
		/// <param name="str">The string.</param>
		/// <param name="vrb">The verbose level.</param>
		public static void Write(string str)
		{
			Console.Write(str);
		}
		/// <summary>
		/// Writes the specified string value to standard output based on allowed level of verbosity
		/// </summary>
		/// <param name="str">The string.</param>
		/// <param name="vrb">The verbose level.</param>
		public static void WriteLineFromAsync(string str, Verbose vrb)
		{
			if (vrb <= options.Verbose)
				Console.WriteLine("> " + str);
		}
		/// <summary>
		/// Writes the specified string value to error output
		/// </summary>
		/// <param name="str">The string.</param>
		public static void ErrorWriteLine(string str)
		{
			if (!HasShutdownStarted)
			{
				Console.BackgroundColor = ConsoleColor.Black;
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Error.WriteLine("\n! " + str);
				Console.ResetColor();
			}
		}
		/// <summary>
		/// Writes the specified string value and exception to error output
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="exception">The exception.</param>
		public static void ErrorWriteException(string message, Exception exception)
		{
			ErrorWriteLine(message + "\n! " + exception.ToString());
		}
		/// <summary>
		/// Writes the specified string lines to error output
		/// </summary>
		/// <param name="str">The string.</param>
		public static void WarningWriteLines(string line1, string line2)
		{
			if (!HasShutdownStarted)
			{
				Console.BackgroundColor = ConsoleColor.Black;
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.Error.WriteLine("\n! " + line1 + "\n! " + line2);
				Console.ResetColor();
			}
		}

		/// <summary>
		/// Terminates LFSStats and gives the underlying operating system the specified exit code.
		/// </summary>
		/// <param name="exitCode">The exit code.</param>
		internal static void Exit(int exitCode)
		{
			if (!HasShutdownStarted)
			{
				HasShutdownStarted = true;
				LFSStats.WriteLine("\nGoodbye.");	// TODO split conditions to Time of day: sunrise, ... Wind: low.
				Environment.Exit(exitCode);			// TODO HTML escape names and what ever I export to HTML
			}
		}
		private static bool hasShutdownStarted = false;
		public static bool HasShutdownStarted
		{
			get { return hasShutdownStarted; }
			private set { hasShutdownStarted = value; }
		}
	}

	enum Verbose
	{
		Program, Session, Lap, Split, Info
	}

	[Flags]
	public enum PlayerFlags : int
	{
		SwapSide = 1,
		Reserved_2 = 2,
		Reserved_4 = 4,
		AutoGearShift = 8,
		Shifter = 16,
		Reserved_32 = 32,
		BrakeHelp = 64,
		AxisClutch = 128,
		InPits = 256,
		AutoClutch = 512,
		Mouse = 1024,
		KbNoHelp = 2048,
		KbStabilised = 4096,
		CustomView = 8192
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
}
