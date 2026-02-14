using System;
using System.Reflection;

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
        public static string License => Title + " comes with ABSOLUTELY NO WARRANTY.\n" +
            "This is free software, and you are welcome to redistribute it\n" +
            "under certain conditions; see GNU GPLv3: <http://www.gnu.org/licenses/>.";
        public static string ProjectUrl => "https://github.com/neonsp/LfsStats";
        #endregion

        #region Constants
        internal const char filePathSpaceChar = '_';
        internal const int MaxSectorCount = 4;                  // NOTE LFS Insim seems to only have splits 1,2,3 defined, that is 4 sectors at most
        internal const int MaxSplitCount = MaxSectorCount - 1;  // NOTE LFS Insim seems to only have splits 1,2,3 defined
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

            if (!options.Parse(args) || options.Help)
            {
                Console.WriteLine(options.GetUsage());
                return;
            }

            if (options.Version)
            {
                PrintVersion();
                return;
            }

            PrintHeader();

            LFSClient lfsclient = new LFSClient(options.ConfigFileName, options.RefreshInterval);
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
                LFSStats.WriteLine("\nGoodbye.");
                Environment.Exit(exitCode);
            }
        }
        private static bool hasShutdownStarted = false;
        public static bool HasShutdownStarted
        {
            get { return hasShutdownStarted; }
            private set { hasShutdownStarted = value; }
        }
    }

}
