LFS Stats v3.0
===============

Statistics generator and interactive viewer for Live for Speed (https://www.lfs.net/).

Connects to a LFS server (or replay) via InSim, captures race data in real time,
and exports it as JSON. The included web viewer renders interactive charts, tables,
and analysis — no server-side processing required.


HOW IT WORKS
============

  LFS Server <--InSim--> LFS Stats (C#) --JSON--> Stats Viewer (HTML+JS)

  1. LFS Stats connects via InSim protocol
  2. Captures all events: laps, splits, pit stops, overtakes, chat, penalties...
  3. Exports everything as a JSON file when the session ends
  4. The Stats Viewer renders the JSON into an interactive dashboard


QUICK START
===========

  1. Open InSim port in LFS:     /insim 29999
  2. Edit LFSStats.cfg with your server details
  3. Run LFSStats.exe
  4. Open stats_viewer.html?json=your_file.json

Console keys:
  F - Fast-forward replay
  L - Toggle preserve laps on ESC pit (default: ON)
  Q - Quit safely


CONFIGURATION (LFSStats.cfg)
=============================

  host              = 127.0.0.1     LFS server IP or hostname
  port              = 29999         InSim port (/insim <port> in LFS)
  adminPassword     =               Server admin password (if needed)
  TCPmode           = true          true=TCP, false=UDP
  isLocal           = true          true=local server, false=remote
  pracDir           = results       Practice stats output directory
  qualDir           = results       Qualifying stats output directory
  raceDir           = results       Race stats output directory
  exportOnRaceSTart = yes           Export on session restart: yes/no/ask
  askForFileNameOnRST = false       Prompt for filename on export
  exportOnSTAte     = no            Export on state change: yes/no/ask
  askForFileNameOnSTA = false       Prompt for filename on state change
  pubStatIDkey      =               LFS World PubStat API key


COMMAND LINE OPTIONS
====================

  LFSStats.exe [options]

  -c, --config <file>     Config file (default: LFSStats.cfg)
  -i, --interval <ms>     InSim refresh interval: 1-1000 ms (default: 100)
  -v, --verbose <level>   Verbose level: 0-4 (default: 1)
      --version           Display version information
  -h, --help              Display this information

  Verbose levels:
    0 - Program (errors only)
    1 - Session (start/end, results)
    2 - Lap (lap completions)
    3 - Split (sector times)
    4 - Info (all events, debug)


STATS VIEWER
============

The viewer/ folder contains the web-based stats viewer:

  stats_viewer.html   - Main HTML page
  stats_renderer.js   - Rendering engine
  stats.css           - Styles
  translations.js     - 16 languages

Upload these files to a web server along with your exported JSON files:

  https://your-site.com/stats/stats_viewer.html?json=race.json
  https://your-site.com/stats/stats_viewer.html?json=season/round1.json

Or open stats_viewer.html locally and drag & drop your JSON file.


CUSTOMIZING JSON
================

The metadata section is at the top of the JSON for easy editing:

  "metadata": {
    "exportedAt": "2025-07-11T18:30:00Z",
    "mprUrl": "https://your-site.com/replays/race.mpr",
    "logoUrl": "https://your-site.com/images/league-logo.png"
  }

  mprUrl  - Link to replay file (download button in viewer header)
  logoUrl - League/team logo (top-right corner of viewer header)

Both are empty by default. Edit them in the JSON after export.


VIEWER FEATURES
===============

Tabs:
  Results         - Final standings, podium colors, grid, best laps, incidents
  Overview        - Grid order, biggest climbers, laps led, combativity
  Driver Stints   - Relay/stint info for endurance (auto-hidden if not applicable)
  Lap by Lap      - Transposed lap time table with color-coded cells
  Position Graph  - Interactive Chart.js graph with event markers
  Race Progress   - Gap-to-leader chart showing race dynamics
  Best Times      - Best lap, theoretical best, sectors, top speeds
  Compare         - Side-by-side driver comparison (up to 5 drivers)
  Analysis        - Lap time distribution, consistency, outlier filtering
  Incidents       - Yellow flags, blue flags, contacts per driver
  Chat            - In-game chat messages

Interactive graphs:
  - Zoom & pan (mouse wheel + drag)
  - Double-click to reset zoom
  - Hover tooltips with full ranking at each timing point
  - Click legend to show/hide drivers
  - Event markers: pit stops, DNF, fastest lap, personal best

Session types supported: Race, Qualifying, Practice

Languages: EN, ES, FR, PT, DE, IT, PL, RU, TR, FI, SV, LT, JA, ZH, NL, DA


LIVE DEMO
=========

  Qualifying: https://www.cesav.es/mkportal/liga/stats/stats_viewer.html?json=resultados/qual.json
  Race:       https://www.cesav.es/mkportal/liga/stats/stats_viewer.html?json=resultados/race.json
  Endurance:  https://www.cesav.es/mkportal/liga/stats/stats_viewer.html?json=resultados/endurance.json


DEPENDENCIES
============

  InSimDotNet 2.7.2.1          - LFS InSim protocol library
  Newtonsoft.Json 13.0.4       - JSON serialization
  Chart.js 4.x (CDN)          - Interactive graphs (viewer only)
  chartjs-plugin-zoom 2.x (CDN) - Zoom & pan (viewer only)


CREDITS
=======

Originally created by Robert B. (Gai-Luron), JackCY & Yamakawa (2007-2008).

Expanded by Ricardo (NeoN) with JSON export, interactive web viewer, Chart.js
graphs, overtake detection, multi-session support, driver comparator,
relay/stint support, internationalization, and modern code architecture.


LICENSE
=======

GNU General Public License v3.0 - See LICENSE file.

Project: https://github.com/neonsp/LfsStats
