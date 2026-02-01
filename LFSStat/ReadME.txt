LFS sTaTs, LFSStat, LFSStats, ... you know what i mean.

http://www.lfsforum.net/showthread.php?t=24933


Copyright (C) 2008 Jaroslav Cerny alias JackCY, Robert B. alias Gai-Luron and Monkster.
Jack.SC7@gmail.com, lfsgailuron@free.fr

Graph.dll based on sources from:
Graph v1.20 for LFS stats! (c) Alexander 'smith' Rudakov (piercemind@gmail.com)


v2.00 - DONE:
-------------
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
* maybe change graphs from graph.exe (LBL graphs), DON'T know it is real mess :/
** need some new graph generator, different colors for SPLITs, yellow and blue flags, something like that without comparison [URL="http://www.lfsforum.net/attachment.php?attachmentid=65693&d=1220804371"]LBL GRAPH - V1 good vs bad[/URL]
* add links for racer LBL graph from RaceResults StartOrder
* average lap is from all laps, first included, add newAverageLap without laps longer then oldAverageLap * some%

CAN'T be DONE:
--------------
* fix topspeed when using ReplayJump
* no chat when using ReplayJump
