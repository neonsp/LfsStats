LFS sTaTs, LFSStat, LFSStats, ... you know what i mean.

http://www.lfsforum.net/showthread.php?t=24933


GNU GPL v3, 2013, Jaroslav Cerny alias JackCY, Jack.SC7@gmail.com


Other contributors
==================
Original code is based on other versions of LFSStat published by Robert B. alias Gai-Luron (lfsgailuron@free.fr) and Monkster.

GPLv3 briefly
=============
Required:
	License and copyright notice
	State Changes
	Disclose Source

Permitted:
	Commercial Use
	Modification
	Distribution
	Patent Grant

Forbidden:
	Hold Liable
	Sublicensing

Graph.dll
=========
Based on sources from:
Graph v1.20 for LFS stats! (c) Alexander 'smith' Rudakov (piercemind@gmail.com)

Y-Graph v2.0/TODO fix location and LFSStat.cfg, add location-name to cfg to configure graph
============
YGraph.exe = Y-Graph v2.0 release by Yamakawa
https://www.lfsforum.net/showthread.php?p=1160837
Does not handle Unicode/UTF-8 and thus may crash/report error when trying to export from TSV that has Unicode characters.
Crashes when LFStat.cfg is missing, etc.

Note:
=====
* When jumping in LFS replay, do not use speeds higher than 1.0x as it will mess up the statistics when using UDP
	* This may or may not apply to TCP as well or playing the replay too fast
	* It may be that Insim does not send the data this fast because the export goes fine but the generated results
	are not information correct, or it may be that the incoming data arrive too much out of order due to UDP
	* Using replay speeds above 1.0x and as I noticed also sometimes using 0.125x will result in replay jumping faster
	when seeking to the end or any other part of it and the generated results will have malformed information
	such as race lader finishing lower laps than the rest and causing exported lap differences to be negative or '+-'
	because '+' is always inserted as negative values shouldn't occur normally, best times, race flags and other can
	also be inaccurate on occasion
	* Avoid replay jumping if possible, it can on occasion make the statistics invalid and even graph generation can then fail.
	Which can bring the application down without any exception or error report!
* Please start template "[Tag" on a new line, it can start anywhere on a line and only first "[Tag" on a line is recognized,
processing is by lines. Text before tag start "[Tag" and after tag close "]" will be repeated on each output line if there is some text.
* You can use "[" and "]" in the HTML as long as they are not equal to one of the tags "[RaceResult", ... or "[-"
* The TCP mode is not good at all. There is some obvious mistakes that need to be addressed.
	* Framing, which could be now solved with the buffer update in GetPackFromInsimServer.
	* Detecting whether the connection is half-open or not and closing it if LFS shut it down without disconnecting.
	* Other TCP communication flow realted stuff, asynchronous processing might be better here.

Changelog:
==========
v2.1
----
* Graphs:
	* Adding option to generate graph from DLL or EXE and adding Yamakawa's YGraph as an option
	* Updated NPlot library to 0.9.10.0 and due to NPlot a .NET 4.0 Client Profile is not enough and a full .NET 4.0 is needed
	* Changed default graph config to not output unused images
	* Changed default graph background to transparent
* Networking:
	* Fixed new Insim flags (AXIS_CLUTCH / THROTTLE, ...), proper names and exporting all of them, including SwapSide
		* Changed them to be displayed via CSS3 - easy to modify at any time
	* Added changes from Luron's v1.12 in Insim4.cs but TCP is still buggy
	* Added changes from Luron's LFSLapper update tcp.cs/GetPackFromInsimServer, it could now support TCP better
	* (InSim.Decoder.PIT.PIT) Pit flags text but still needs localization I think
* Ohter:
	* Looked up and commented out some unused code
	* Fixed {*Conditions} "Sunny,no wind" formatting to "Sunny, no wind"
	* Added a few language file substitutions
	* LFSStat changed to .NET 4.0 Client Profile and update some outdated code for better performance and conventions
	* Cleaner version reporting and maintaining in the project
	* Removed #MONO and #WINDOWS from source files since these are supposed to be defined elsewhere, in build/compiler options,
	and not in source files, just change the #IFs back to #IF MONO and define #IF at build/compile level not in source files
	* Command line options with help text and usage
	* Customizable program ouput verbosity
	* fixed 45 sec. penalty string
	* Config errors are not reported but wrong values are set to default
	* IMPORTANT: the application is now multithreaded, uses .NET tasks for exporting the collected data,
	removes slight application hang up while exporting, thus main thread = networking runs continuously and exporting is asynchronous
	* All command line interaction removed except when config defines to ask for filename, it should not hang up anywhere and should
	quit on errors
	* Fixed some of the weird exceptions that were happening because Insim does not report well session states, thus things were being
	accessed while a session did not even start yet. It should be safe now to browse LFS without LFSStats crashing.
	* Cleaner and formatted output to command line and console title
	* Exit codes: 0 success, 1 network error, 2 connect timeout, 3 template error
	* Fixed LFS World links, HTML link encoding
	* Fixed car plate export, HTML encoding
	* Updated export of statistics to accomodate template changes = more freedom when making templates
	* Rewrote the configuration importer that now contains the loaded settings and comments starting with '#' can be anywhere on a line.
	Checks if parameters are valid and reverts to default settings for the invalid settings supplied. Invalid lines are ignored.
	* Generated statistics names should be easier to recognize "2013-09-26_23.59.59_Race_16_laps_of_FE3"
	* Fixed import from LFS World, updated API version to 1.5 and as usual updated the whole class to be .NET 4, readable,
	reusable, robust and fool proof. Reports wrong IDkeys, retries if LFSW denied the request (as can happen if more requests are made
	within 5 seconds), reports if there are errors in LFSW reply.
* Templates:
	* Changed template names to .html for easier processing
	* Added back to templates what Yamakawa decided to throw away but LFSStats can export and is useful, like driver swaps!
	* LBL integrated in template
	* LBL colors are HSLA, partly transparent and colors change continuously not discretely, saturation and alpha can be changed in tempplate
	in code easily, want only 2 colors sure, want crazy mixed rainbow with 100 colors and all the way to 2000% best laptime? sure!
	Just type it in there.
	* More sensible html titles (16 laps of Fern Bay Green)
	* Tag start "[-" disables output of that tag content, ends normally with tag end "]", used on [Include in the template for example
	* Chat integrated in template
	* Fixed mixed up splits that were actually sectors

---
+ Config
+ Q export
+ Q template
+ P export
+ P template
+ IS.NET

+ Created new templates
++ HTML5
++ CSS3
++ JS
++ Race


+ Updated exportstat.cs to accomodate template changes

---

+ Export options to export HTML, CSV, TSV, Graph and Graph executable location
+ Unified text color and HTML export methods
?+ Fixed colored chat messages (case "MSO":)
+ Fixed language in CSV, TSV (Graphs that don't support proper encodings, like YGraph)
+ Updated weather names
+ Making references to GNU GPL v3 and removing copyright words since it seems contradictory
+ Added open track images + fake ones unless somebody can generate proper ones

v2.0
----
* Nickname colors - fixed
* Nickname 2 HTML - fixed
* Chat 2 HTML - fixed
* Some minor renaming of templates and generated files
* Changed templates (hope old templates will still work), more space saving, added link to Chat
* When exporting on STAte changed (leave, exit) added option to CFG yes/no/ask to decide if export.
* When exporting on Race STart (race [re]started, mpr [re]starts from begining) added option to CFG yes/no/ask to decide if export.
* Question for name of stats instead of generated time, for STA and RST separatedly
* Globals in templates now work for chat to.
* Chat template and language files updated.
* StartOrder Nickname link to LFSW - fixed
* Dns.Resolve() changed to Dns.GetHostEntry()
* integrate Graph.exe (got original src files for v1.20 )
	* integrated as .DLL
	* fixed .TSV path
	* added option to LFSStats.CFG, generate automatically YES/NO
* fix some tables in templates, no white backround
* update Console.WriteLine() stuff (names, emails, ...)

TODO:
=====
* Graphs:
	* change graphs to D3, google chart, or some other free JS graph library
		* or even use HTML5 canvas and provide other older versions (img) as a backup when HTML5 canvas is not supported
	* need some new graph generator, different colors for SPLITs, yellow and blue flags, something like that without
	comparison http://www.lfsforum.net/attachment.php?attachmentid=65693&d=1220804371 LBL GRAPH - V1 good vs bad
	* add links for racer LBL graph from RaceResults StartOrder
* Networking:
	* fix Insim changes and bugs related to exporting stats when LFS quits with LFSStats still running
	* Insim 5, update to InsimDotNet
	* fix TCP mode and framing, half-open, flow in general
	* fix UDP mode, it works but the whole networking part should be redone, like the rest needed to be written again
* Other:
	* average lap is from all laps, first included, add newAverageLap without laps longer then oldAverageLap*some%
	* enable exporting of practice session
* Templates:
	* it would be nice to detect output elements not based on [OutpuElement] that has to be at the beginning of line
	and the whole output has to be shrinked into one line, but based on id="OutputElement". As I said before
	and Luron too, next time, maybe when S3 comes out, just rewrite the whole thing from scratch and properly.
	Basically it has no idea it works with HTML or XML style formatted document.
	* use HTML DOM and id="AAA" to find outputs elements

May be possible:
================
* fix topspeed when jumping in replay and other jumping or replay speed related issues may be out of the reach of LFSStats
	* see IS_RIP -> Options -> RIPOPT_FULL_PHYS

Can't be done:
==============
* LFS.exe side, insim stuff that doesn't get sent
	* no chat when jumping in replay, insim simply does not send it


---
Insim.h made browsable: http://www.brunsware.de/insim/

// REPLAY CONTROL
// ==============

// You can load a replay or set the position in a replay with an IS_RIP packet.
// Replay positions and lengths are specified in hundredths of a second.
// LFS will reply with another IS_RIP packet when the request is completed.

struct IS_RIP // Replay Information Packet
{
	byte	Size;		// 80
	byte	Type;		// ISP_RIP
	byte	ReqI;		// request : non-zero / reply : same value returned
	byte	Error;		// 0 or 1 = OK / other values are listed below

	byte	MPR;		// 0 = SPR / 1 = MPR
	byte	Paused;		// request : pause on arrival / reply : paused state
	byte	Options;	// various options - see below
	byte	Sp3;

	unsigned	CTime;	// (hundredths) request : destination / reply : position
	unsigned	TTime;	// (hundredths) request : zero / reply : replay length

	char	RName[64];	// zero or replay name - last byte must be zero
};

// NOTE about RName :
// In a request, replay RName will be loaded.  If zero then the current replay is used.
// In a reply, RName is the name of the current replay, or zero if no replay is loaded.

// You can request an IS_RIP packet at any time with this IS_TINY :

// ReqI : non-zero		(returned in the reply)
// SubT : TINY_RIP		(Replay Information Packet)

// Error codes returned in IS_RIP replies :

enum
{
	RIP_OK,				//  0 - OK : completed instruction
	RIP_ALREADY,		//  1 - OK : already at the destination
	RIP_DEDICATED,		//  2 - can't run a replay - dedicated host
	RIP_WRONG_MODE,		//  3 - can't start a replay - not in a suitable mode
	RIP_NOT_REPLAY,		//  4 - RName is zero but no replay is currently loaded
	RIP_CORRUPTED,		//  5 - IS_RIP corrupted (e.g. RName does not end with zero)
	RIP_NOT_FOUND,		//  6 - the replay file was not found
	RIP_UNLOADABLE,		//  7 - obsolete / future / corrupted
	RIP_DEST_OOB,		//  8 - destination is beyond replay length
	RIP_UNKNOWN,		//  9 - unknown error found starting replay
	RIP_USER,			// 10 - replay search was terminated by user
	RIP_OOS,			// 11 - can't reach destination - SPR is out of sync
};

// Options byte : some options

#define RIPOPT_LOOP			1		// replay will loop if this bit is set
#define RIPOPT_SKINS		2		// set this bit to download missing skins
#define RIPOPT_FULL_PHYS	4		// use full physics when searching an MPR

// NOTE : RIPOPT_FULL_PHYS makes MPR searching much slower so should not normally be used.
// This flag was added to allow high accuracy MCI packets to be output when fast forwarding.


---

HTML

Escaping a string (for the purposes of the algorithm above) consists of running the following steps:

	Replace any occurrence of the "&" character by the string "&amp;".
	Replace any occurrences of the U+00A0 NO-BREAK SPACE character by the string "&nbsp;".
	If the algorithm was invoked in the attribute mode, replace any occurrences of the """ character by the string "&quot;".
	If the algorithm was not invoked in the attribute mode, replace any occurrences of the "<" character by the string "&lt;", and any occurrences of the ">" character by the string "&gt;".

---

