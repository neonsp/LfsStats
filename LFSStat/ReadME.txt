LFS sTaTs, LFSStat, LFSStats, ... you know what i mean.

http://www.lfsforum.net/showthread.php?t=24933


GNU GPL v3, 2013, Jaroslav Cerny alias JackCY, Robert B. alias Gai-Luron and Monkster.
Jack.SC7@gmail.com, lfsgailuron@free.fr

Graph.dll based on sources from:
Graph v1.20 for LFS stats! (c) Alexander 'smith' Rudakov (piercemind@gmail.com)

Graph.exe = YGraph release by Yamakawa

>>>
I would again kindly ask Yamakawa to provide sources with his release of LFSStats.
We welcome all updates and modifications as long as you provide sources and GNU GPL v3 license with your release.

XXX Your YGraph has language/encoding bugs and does not handle languages/encodings properly, please provide sources of YGraph as well so we can fix the mingled text in your graphs.
<<<


v2.01
-----
* Insim flags, proper names and exporting all of them, including SwapSide
* Added changes from Luron's v1.12 in Insim4.cs but TCP is still buggy
* Adding option to generate graph from DLL or EXE and adding Yamakawa's YGraph as an option
* Added VW Scirocco and fixed car names, just for the lulz ;) We all know it will be released in next millenium anyway
* Fixed colored chat messages (case "MSO":)
* Fixed {*Conditions} "Sunny,no wind" formatting to "Sunny, no wind"
* Added a few language file substitutions
* Updated templates

---

* Changed template names to .html for easier processing in tools that do not understand, can have TPL set equal to HTML format

* Updated exportstat.cs to accomodate template changes, old templates will work but new templates on older versions will not unless you flatten LFSstat tags to a signle line, see older templates for what I mean

---

** Race HTML
** Race JS

** Race CSS

* Unified text color and HTML export methods
* Fixed language encoding in CSV, TSV (Graphs that don't support proper encodings, like YGraph)
* Updated weather names
* Making references to GNU GPL v3 and removing copyright words since it seems contradictory
* Added open track images * fake ones unless somebody can generate proper ones



*TODO: it would be nice to detect output elements not based on [OutpuElement] that has to be at the beginning of line and the whole output has to be shrinked into one line, but based on id="OutputElement", yeah the whole LSF stats is like one big hack of mess. As I said before and Luron too, next time, maybe when S3 comes out, just rewrite the whole thing from scratch and properly. Basically it has no idea it works with HTML or XML style formatted document.



v2.00
-----
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
** integrated as .DLL
** fixed .TSV path
** added option to LFSStat.CFG, generate automatically YES/NO
* fix some tables in templates, no white backround
* update Console.WriteLine() stuff (names, emails, ...)

TODO:
-----
* Graphs:
	* maybe change graphs from graph.exe (LBL graphs), DON'T know it is real mess :/
	** need some new graph generator, different colors for SPLITs, yellow and blue flags, something like that without comparison [URL="http://www.lfsforum.net/attachment.php?attachmentid=65693&d=1220804371"]LBL GRAPH - V1 good vs bad[/URL]
	* add links for racer LBL graph from RaceResults StartOrder
* average lap is from all laps, first included, add newAverageLap without laps longer then oldAverageLap * some%

* Networking:
	* fix Insim changes and bugs related to exporting stats when LFS quits with LFSStats still running

CAN'T be DONE:
--------------
* fix topspeed when using ReplayJump
* no chat when using ReplayJump
