using System;

namespace LFSStatistics
{
    /// <summary>
    /// Command line options (parsed manually, no external dependencies)
    /// </summary>
    class Options
    {
        public string ConfigFileName { get; set; } = "LFSStats.cfg";
        public int RefreshInterval { get; set; } = 100;
        public Verbose Verbose { get; set; } = (Verbose)1;
        public bool Version { get; set; } = false;
        public bool Help { get; set; } = false;

        /// <summary>
        /// Parse command line arguments
        /// </summary>
        public bool Parse(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i].ToLower();

                if (arg == "-h" || arg == "--help" || arg == "-?")
                {
                    Help = true;
                    return true;
                }
                else if (arg == "--version")
                {
                    Version = true;
                    return true;
                }
                else if ((arg == "-c" || arg == "--config") && i + 1 < args.Length)
                {
                    ConfigFileName = args[++i];
                }
                else if ((arg == "-i" || arg == "--interval") && i + 1 < args.Length)
                {
                    if (int.TryParse(args[++i], out int val) && val >= 1 && val <= 1000)
                        RefreshInterval = val;
                }
                else if ((arg == "-v" || arg == "--verbose") && i + 1 < args.Length)
                {
                    if (int.TryParse(args[++i], out int val) && val >= 0 && val <= 4)
                        Verbose = (Verbose)val;
                }
                else
                {
                    Console.WriteLine($"Unknown option: {args[i]}");
                    Help = true;
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets the usage string
        /// </summary>
        public string GetUsage()
        {
            return LFSStats.Heading + "\n"
                + LFSStats.CopyrightC + "\n\n"
                + "Usage:\t" + LFSStats.ProcessName + " [options]\n\n"
                + "Options:\n"
                + "  -c, --config <file>     Config file (default: LFSStats.cfg)\n"
                + "  -i, --interval <ms>     Insim refresh interval: 1-1000 ms (default: 100)\n"
                + "  -v, --verbose <level>   Verbose level: 0-4 (default: 1)\n"
                + "      --version           Display version information\n"
                + "  -h, --help              Display this information\n\n"
                + "Report bugs to lfsstatistics+bugs@gmail.com.\n";
        }
    }
}
