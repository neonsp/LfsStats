using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace LFSStatistics
{
	/// <summary>
	/// Command line options
	/// </summary>
	class Options
	{
		[Option('c', "config", DefaultValue = "LFSStats.cfg", HelpText = "Config file.")]
		public string ConfigFileName { get; set; }
		[Option('i', "interval", DefaultValue = 100, HelpText = "Insim refresh interval: [1-1000]ms.")]
		public int RefreshInterval { get; set; }
		[Option('v', "verbose", DefaultValue = 1, HelpText = "Verbose level: [0-4] Program, Session, Lap, Split, Info.")]
		public Verbose Verbose { get; set; }
		[Option("version", HelpText = "Display version information.")]
		public bool Version { get; set; }


		/// <summary>
		/// Gets the usage string.
		/// </summary>
		/// <returns>The usage string.</returns>
		[HelpOption(HelpText = "Display this information.")]
		public string GetUsage()
		{
			HelpText help = new HelpText();
			help.AddDashesToOption = true;
			help.AdditionalNewLineAfterOption = false;
			help.Copyright = LFSStats.CopyrightC;
			help.Heading = LFSStats.Heading;
			help.MaximumDisplayWidth = 79;	// default is 80, but last character needs to be saved for new line if that is needed
			help.AddOptions(this);	// has to be added at the end, otherwise the previous settings won't affect it
			help.AddPreOptionsLine("\nUsage:\t" + LFSStats.ProcessName + " [options]");
			//help.AddPostOptionsLine("For bug reporting instructions, please see:\n<http://---/bugs.html>.");
			help.AddPostOptionsLine("Report bugs to lfsstatistics+bugs@gmail.com.\n");

			return help;
		}
	}
}
