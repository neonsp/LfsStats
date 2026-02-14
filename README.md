# LFS Stats v3.0

**Statistics generator and interactive viewer for [Live for Speed](https://www.lfs.net/).**

LFS Stats connects to a Live for Speed server (or replay) via InSim, captures race data in real time, and exports it as JSON. The included web viewer renders interactive charts, tables, and analysis from the exported data — no server-side processing required.

![License](https://img.shields.io/badge/license-GPL--3.0-blue)

🇪🇸 [Leer en español](README.es.md) · 🇫🇷 [Lire en français](README.fr.md) · 🇵🇹 [Ler em português](README.pt.md)

## Table of Contents

- [How It Works](#how-it-works)
- [Quick Start](#quick-start)
- [Viewer Features](#viewer-features)
  - [Tabs](#tabs)
  - [Summary Card](#summary-card)
  - [Interactive Graphs](#interactive-graphs)
  - [Driver Comparator](#driver-comparator)
  - [Session Support](#session-support)
  - [Internationalization](#internationalization)
- [Stats Viewer Deployment](#stats-viewer-deployment)
  - [Web Server](#deploying-on-a-web-server)
  - [Local Usage](#local-usage-without-a-server)
- [Configuration Reference](#configuration-reference)
  - [LFSStats.cfg](#lfsstatscfg)
  - [Command Line Options](#command-line-options)
- [Customizing Exported JSON](#customizing-exported-json)
- [JSON Schema](#json-schema)
- [Project Structure](#project-structure)
- [Dependencies](#dependencies)
- [Credits](#credits)
- [License](#license)

## How It Works

```
┌──────────┐    InSim     ┌───────────┐    JSON     ┌─────────────────┐
│  LFS     │ ◄──────────► │ LFS Stats │ ──────────► │  Stats Viewer   │
│  Server  │   TCP/UDP    │  (C#)     │   Export    │  (HTML+JS)      │
└──────────┘              └───────────┘             └─────────────────┘
```

1. **LFS Stats** connects to a Live for Speed server via the InSim protocol
2. It captures all events: lap times, splits, pit stops, overtakes, chat messages, penalties, etc.
3. When the session ends, it exports everything as a **JSON file**
4. The **Stats Viewer** renders the JSON into an interactive dashboard

## Quick Start

### 1. Configure LFS

Open the InSim port in LFS:
```
/insim 29999
```

### 2. Configure LFS Stats

Edit `LFSStats.cfg`:
```ini
host = 127.0.0.1       # LFS server IP
port = 29999            # InSim port (must match /insim command)
adminPassword =         # Server admin password (if required)
TCPmode = true          # TCP recommended
raceDir = results       # Output directory for JSON files
```

### 3. Run

```bash
LFSStats.exe
```

The interactive console menu allows you to:
- **F** — Fast-forward replay
- **L** — Toggle preserve laps on ESC pit (default: ON)
- **Q** — Quit safely

### 4. View Results

Open the viewer with your JSON file:
```
stats_viewer.html?json=race.json
```

Or open `stats_viewer.html` directly and drag & drop your JSON file.

An example JSON file is included in `viewer/examples/endurance.json` (5-hour endurance race, 10 mod cars, 195 laps).

### Live Demo

- [Qualifying Session](https://www.cesav.es/mkportal/liga/stats/stats_viewer.html?json=resultados/qual.json) — 32 drivers, Aston
- [Race Session](https://www.cesav.es/mkportal/liga/stats/stats_viewer.html?json=resultados/race.json) — Full race with overtakes, pit stops, incidents
- [Endurance Race](https://www.cesav.es/mkportal/liga/stats/stats_viewer.html?json=resultados/endurance.json) — 5-hour endurance with driver relays



## Viewer Features

### Tabs

| Tab | Description |
|-----|-------------|
| 🏁 **Results** | Final standings with podium colors, grid positions, best laps, pit stops, incidents |
| 📋 **Overview** | Grid order, biggest climbers, laps led, combativity stats |
| 🔄 **Driver Stints** | Relay/stint information for endurance races (auto-hidden when not applicable) |
| 📊 **Lap by Lap** | Transposed lap time table with color-coded cells |
| 📈 **Position Graph** | Interactive Chart.js graph with pit stop, DNF, fastest lap, and personal best markers |
| ⏱️ **Race Progress** | Gap-to-leader chart showing race dynamics over time |
| ⚡ **Best Times** | Best lap, theoretical best lap, best sectors, top speeds |
| 🔍 **Compare** | Side-by-side driver comparison (up to 5 drivers) with lap time chart |
| 📉 **Analysis** | Lap time distribution, consistency metrics, outlier filtering |
| ⚠️ **Incidents** | Yellow flags, blue flags, contacts per driver |
| 💬 **Chat** | In-game chat messages captured during the session |

### Summary Card

An infographic bar between the header and tabs showing key stats at a glance:
- 🏆 Winner / Pole Position
- ⚡ Fastest Lap (purple highlight, F1-style)
- 💨 Top Speed
- 👥 Drivers / 🚫 DNFs
- 🔄 Total Laps / ⚔️ Overtakes / 🔧 Pit Stops / 💥 Contacts

Each card is clickable and navigates to the relevant tab.

### Interactive Graphs

All graphs feature:
- **Zoom & Pan** — Mouse wheel to zoom, drag to pan
- **Double-click** — Reset zoom
- **Hover tooltips** — Full ranking at each timing point with position changes, gaps, pit stops
- **Interactive legend** — Click driver names to show/hide, "Show All / Hide All" buttons
- **Event markers** — Pit stops (🔧), DNF (✕), fastest lap (★), personal best (●)

### Driver Comparator

Select 2-5 drivers to compare:
- Stats table: position, grid, laps, best lap, sectors, pit stops, incidents
- Lap time chart with pit stop markers
- Smart Y-axis zoom centered on average lap times
- Qualifying mode hides irrelevant stats (avg lap, consistency, chart)

### Session Support

| Session Type | Features |
|---|---|
| **Race** | Full analysis: positions, gaps, overtakes, stints, combativity |
| **Qualifying** | Temporal position graph, best lap evolution, session time limit zone |
| **Practice** | Basic lap time analysis |

### Internationalization

The viewer auto-detects browser language with 16 supported languages:

🇬🇧 English · 🇪🇸 Spanish · 🇫🇷 French · 🇵🇹 Portuguese · 🇩🇪 German · 🇮🇹 Italian · 🇵🇱 Polish · 🇷🇺 Russian · 🇹🇷 Turkish · 🇫🇮 Finnish · 🇸🇪 Swedish · 🇱🇹 Lithuanian · 🇯🇵 Japanese · 🇨🇳 Chinese · 🇳🇱 Dutch · 🇩🇰 Danish

## Stats Viewer Deployment

### Deploying on a Web Server

Upload the viewer files to your web server:
```
/your-site/lfsstats/
  ├── stats_viewer.html
  ├── stats_renderer.js
  ├── stats.css
  ├── translations.js
  └── race.json          ← your exported JSON files
```

Access via URL with the `json` parameter:
```
https://your-site.com/lfsstats/stats_viewer.html?json=race.json
https://your-site.com/lfsstats/stats_viewer.html?json=qualifying.json
https://your-site.com/lfsstats/stats_viewer.html?json=results/round1.json
```

You can organize JSON files in subdirectories:
```
/lfsstats/
  ├── stats_viewer.html
  ├── season2025/
  │   ├── round1.json
  │   ├── round2.json
  │   └── round3_quali.json
  └── endurance/
      └── 24h_race.json
```
```
stats_viewer.html?json=season2025/round1.json
stats_viewer.html?json=endurance/24h_race.json
```

### Local Usage (Without a Server)

Open `stats_viewer.html` directly in your browser. A drag & drop zone will appear — drop your JSON file to load it.

## Configuration Reference

### LFSStats.cfg

| Option | Default | Description |
|--------|---------|-------------|
| `host` | `127.0.0.1` | LFS server IP address or hostname |
| `port` | `29999` | InSim port (set in LFS with `/insim <port>`) |
| `adminPassword` | *(empty)* | Server admin password |
| `TCPmode` | `true` | Use TCP (`true`) or UDP (`false`) for InSim connection |
| `isLocal` | `true` | `true` for local server, `false` for remote host |
| `pracDir` | `results` | Output directory for practice stats |
| `qualDir` | `results` | Output directory for qualifying stats |
| `raceDir` | `results` | Output directory for race stats |
| `exportOnRaceSTart` | `yes` | Export when session restarts: `yes`, `no`, or `ask` |
| `askForFileNameOnRST` | `false` | Prompt for filename on export |
| `exportOnSTAte` | `no` | Export on state change (interruption): `yes`, `no`, or `ask` |
| `askForFileNameOnSTA` | `false` | Prompt for filename on state change |
| `pubStatIDkey` | *(empty)* | LFS World PubStat API key for world records |

### Command Line Options

```
LFSStats.exe [options]

  -c, --config <file>     Config file (default: LFSStats.cfg)
  -i, --interval <ms>     InSim refresh interval: 1-1000 ms (default: 100)
  -v, --verbose <level>   Verbose level: 0-4 (default: 1)
      --version           Display version information
  -h, --help              Display this information
```

**Verbose levels:**
- `0` — Program (errors only)
- `1` — Session (session start/end, results)
- `2` — Lap (lap completions)
- `3` — Split (sector times)
- `4` — Info (all events, connections, debug)

### Customizing Exported JSON

The `metadata` section is placed at the top of the JSON file for easy editing:

```json
{
  "metadata": {
    "exportedAt": "2025-07-11T18:30:00Z",
    "mprUrl": "https://your-site.com/replays/race.mpr",
    "logoUrl": "https://your-site.com/images/league-logo.png"
  },
  ...
}
```

- **mprUrl** — Link to the MPR replay file for download. Shown as a download button in the viewer header.
- **logoUrl** — URL of a logo image (league, team, event). Displayed in the top-right corner of the viewer header.

Both fields are exported as empty strings by default. Edit them directly in the JSON file after export.

## JSON Schema

```
├── session        → type, track, trackName, car, laps, sessionTime, sessionLength,
│                    date, time, splitsPerLap, flags[], server, wind, allowedCars[], carImages{}
├── metadata       → exportedAt, mprUrl, logoUrl
├── players[]      → [{ username, name, nameColored }, ...]
├── cars[]         → [{ plid, lastDriver, car, position, gridPosition, status,
│                       lapsCompleted, totalTime, bestLapTime, bestLapNumber,
│                       topSpeed, topSpeedLap, stints[], lapTimes[], lapETimes[],
│                       positions[], pitStops[], penalties[], bestSplits[], incidents{} }]
├── rankings       → fastestLap{}, firstLap[], topSpeed[], pitStops[]
├── events         → overtakes[], incidents[]
├── chat[]         → [{ driver, message }]
└── worldRecords{}
```

Players are stored as an indexed array. All references to drivers in `cars[]`, `chat[]`, and `events` use integer indices into `players[]`.

## Project Structure

```
LFS Stats/
├── LFSStats/
│   ├── Class/
│   │   ├── CloseIntercept.cs   — Cross-platform console close handler
│   │   ├── Configuration.cs    — Config file parser
│   │   ├── ExportStats.cs      — JSON export logic
│   │   ├── LFSWorld.cs         — LFS World API (world records)
│   │   ├── PlayerIdentity.cs   — Username/nickname association
│   │   ├── SessionInfo.cs      — Session metadata
│   │   └── SessionStats.cs     — Per-driver statistics and timing
│   ├── Extensions/
│   │   └── Extensions.cs       — TimeSpan helper extensions
│   ├── Model/
│   │   ├── ChatEntry.cs        — Chat message model
│   │   ├── JsonModels.cs       — JSON serialization models
│   │   └── Verbose.cs          — Verbosity levels enum
│   ├── LFSClient.cs            — InSim connection and event handling
│   ├── Main.cs                 — Entry point and console UI
│   ├── Options.cs              — Command line argument parser
│   └── viewer/
│       ├── stats_viewer.html   — Viewer HTML
│       ├── stats_renderer.js   — Rendering engine
│       ├── stats.css           — Styles
│       ├── translations.js     — i18n (16 languages)
│       └── examples/
│           └── endurance.json  — Example: 5-hour endurance race
├── Graph/                      — Legacy graph generation (System.Drawing)
├── LICENSE                     — GNU GPLv3
└── README.md
```

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| [InSimDotNet](https://github.com/alexmcbride/insimdotnet) | 2.7.2.1 | LFS InSim protocol library |
| [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json) | 13.0.4 | JSON serialization |
| [Chart.js](https://www.chartjs.org/) | 4.x | Interactive graphs (CDN, viewer only) |
| [chartjs-plugin-zoom](https://www.chartjs.org/chartjs-plugin-zoom/) | 2.x | Zoom & pan (CDN, viewer only) |

## Credits

Originally created by **Robert B. (Gai-Luron)**, **JackCY** & **Yamakawa** (2007-2008).

Expanded by **Ricardo (NeoN)** with JSON export, interactive web viewer, Chart.js graphs, overtake detection, multi-session support, driver comparator, relay/stint support, internationalization, and modern code architecture.

## License

[GNU General Public License v3.0](LICENSE)
