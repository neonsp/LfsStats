// LFS Stats Viewer - Complete JavaScript Renderer
// Reads JSON and renders all statistics

const LFS_STATS_VERSION = '3.0.2';

let raceData = null;

// LFS Track names
// Parse LFS color codes to HTML
function parseLFSColors(text) {
    if (!text) return '';
    
    const colorMap = {
        '^0': '#000000', '^1': '#FF0000', '^2': '#00FF00', '^3': '#FFFF00',
        '^4': '#0000FF', '^5': '#00FFFF', '^6': '#FF00FF', '^7': '#FFFFFF',
        '^8': '#666666', '^9': '#888888'
    };
    
    let html = '';
    let currentColor = '#FFFFFF';
    let i = 0;
    
    while (i < text.length) {
        if (text[i] === '^' && i + 1 < text.length && /[0-9]/.test(text[i + 1])) {
            const code = text.substring(i, i + 2);
            if (colorMap[code]) {
                currentColor = colorMap[code];
            }
            i += 2;
        } else {
            html += `<span style="color: ${currentColor};">${escapeHtml(text[i])}</span>`;
            i++;
        }
    }
    
    return html;
}

// Load JSON on page load
window.addEventListener('DOMContentLoaded', () => {
    const params = new URLSearchParams(window.location.search);
    const jsonFile = params.get('json');
    
    const verEl = document.getElementById('app-version');
    if (verEl) verEl.textContent = 'LFS Stats v' + LFS_STATS_VERSION;
    const exportedEl = document.getElementById('exported-at');
    if (exportedEl) exportedEl.textContent = '';

    if (jsonFile) {
        loadRaceData(jsonFile);
    } else {
        showFilePicker();
    }
    setupTabs();
    setupGraphLegend();
    setupProgressLegend();
    setupGraphControls();
});

// Load race data from JSON
async function loadRaceData(jsonFile) {
    try {
        const response = await fetch(jsonFile);
        if (!response.ok) throw new Error(`Failed to load ${jsonFile}`);
        
        const data = await response.json();
        processRaceData(data);
    } catch (error) {
        showFilePickerWithError(jsonFile);
    }
}

// Process loaded JSON data and render
function processRaceData(data) {
    // Restore UI if file picker was shown
    const header = document.querySelector('.header');
    if (header && !document.getElementById('race-title')) {
        header.innerHTML = '<div id="race-title"></div><div id="race-info"></div>';
    }
    const tabs = document.querySelector('.tabs');
    const summary = document.getElementById('summary-card');
    if (tabs) tabs.style.display = '';
    if (summary) summary.style.display = '';
    raceData = data;
    initializeData();
    if (raceData.session && raceData.session.type === 'qual') {
        normalizeQualifyingData();
    }
    const exportedEl = document.getElementById('exported-at');
    if (exportedEl && raceData.metadata && raceData.metadata.exportedAt) {
        exportedEl.textContent = t('exportedAt') + ': ' + new Date(raceData.metadata.exportedAt).toLocaleString();
    }
    renderAll();
}

// Show file picker / drag & drop zone for local file access
function showFilePicker() {
    const tabs = document.querySelector('.tabs');
    const summary = document.getElementById('summary-card');
    if (tabs) tabs.style.display = 'none';
    if (summary) summary.style.display = 'none';
    const header = document.querySelector('.header');
    header.innerHTML = `
        <div id="file-drop-zone">
            <div class="drop-icon">📂</div>
            <h2>LFS Stats Viewer</h2>
            <p>${t('dropJsonHere') || 'Drag & drop a JSON file here'}</p>
            <p style="color:#888;font-size:0.85em;">${t('orClickToSelect') || 'or click to select a file'}</p>
            <input type="file" id="file-input" accept=".json" style="display:none;">
        </div>
    `;

    const dropZone = document.getElementById('file-drop-zone');
    const fileInput = document.getElementById('file-input');

    dropZone.addEventListener('click', () => fileInput.click());

    dropZone.addEventListener('dragover', (e) => {
        e.preventDefault();
        dropZone.classList.add('drag-over');
    });

    dropZone.addEventListener('dragleave', () => {
        dropZone.classList.remove('drag-over');
    });

    dropZone.addEventListener('drop', (e) => {
        e.preventDefault();
        dropZone.classList.remove('drag-over');
        const file = e.dataTransfer.files[0];
        if (file) loadLocalFile(file);
    });

    fileInput.addEventListener('change', (e) => {
        const file = e.target.files[0];
        if (file) loadLocalFile(file);
    });
}

function showFilePickerWithError(fileName) {
    showFilePicker();
    const dropZone = document.getElementById('file-drop-zone');
    if (dropZone) {
        const errorMsg = document.createElement('p');
        errorMsg.style.cssText = 'color: #ff6b6b; margin-top: 16px; font-size: 0.9em;';
        errorMsg.textContent = (t('fileNotFound') || 'File not found') + ': ' + fileName;
        dropZone.appendChild(errorMsg);
    }
}

function loadLocalFile(file) {
    if (!file.name.endsWith('.json')) {
        showError('Please select a valid JSON file');
        return;
    }
    const reader = new FileReader();
    reader.onload = (e) => {
        try {
            const data = JSON.parse(e.target.result);
            processRaceData(data);
        } catch (err) {
            showError('Invalid JSON file: ' + err.message);
        }
    };
    reader.readAsText(file);
}

// Normalize JSON schema to internal (drivers-based) format
function initializeData() {
    if (!raceData) return;
    
    const players = raceData.players; // array of { username, name, nameColored }
    
    // Helper: resolve player index to player object
    const getPlayer = (idx) => players[idx] || { username: '?', name: '?', nameColored: '?' };
    raceData.getPlayer = getPlayer;
    
    // Enrich cars with display names from players array
    raceData.cars.forEach(car => {
        // Collect unique driver indices from stints
        const uniqueIndices = [];
        const seen = new Set();
        (car.stints || []).forEach(s => {
            if (!seen.has(s.driver)) {
                seen.add(s.driver);
                uniqueIndices.push(s.driver);
            }
        });
        const lastPlayer = getPlayer(car.lastDriver);
        car.username = lastPlayer.username;
        car.name = uniqueIndices.map(i => getPlayer(i).name).join(' / ');
        car.nameColored = uniqueIndices.map(i => getPlayer(i).nameColored).join(' ^7/ ');
        
        // Enrich stints with player info
        (car.stints || []).forEach(s => {
            const p = getPlayer(s.driver);
            s.name = p.name;
            s.nameColored = p.nameColored;
            s.username = p.username;
        });
        
        // Ensure incidents object exists
        if (!car.incidents) car.incidents = {};
    });
    
    // Store getPlayer globally for use in renderers
    raceData.getPlayer = getPlayer;
}

// Normalize qualifying data: remove bogus lap times
function normalizeQualifyingData() {
    if (!raceData || !raceData.cars) return;
    
    raceData.cars.forEach(driver => {
        if (!driver.lapTimes) return;
        
        // Filter out 1:00:00.000 placeholders and deduplicate consecutive identical times
        const cleaned = [];
        driver.lapTimes.forEach(t => {
            const parsed = parseLapTime(t);
            if (parsed >= 3599 || parsed === Infinity || parsed <= 0) return;
            // Skip consecutive duplicates
            if (cleaned.length > 0 && cleaned[cleaned.length - 1] === t) return;
            cleaned.push(t);
        });
        driver.lapTimes = cleaned;
        
        // No firstLapTime field needed - use lapTimes[0] directly
        
        // Fix lapsCompleted to match actual laps
        driver.lapsCompleted = cleaned.length;
        
        // Fix bestLapNumber if null: find which cleaned lap matches bestLapTime
        if (driver.bestLapNumber == null && driver.bestLapTime && cleaned.length > 0) {
            const bestParsed = parseLapTime(driver.bestLapTime);
            for (let i = 0; i < cleaned.length; i++) {
                if (Math.abs(parseLapTime(cleaned[i]) - bestParsed) < 0.002) {
                    driver.bestLapNumber = i + 1;
                    break;
                }
            }
        }
    });
    
    // Rebuild firstLapRanking from cleaned driver data
    const firstLaps = raceData.cars
        .filter(d => d.lapTimes && d.lapTimes.length > 0)
        .map(d => ({ username: d.username, time: d.lapTimes[0], parsed: parseLapTime(d.lapTimes[0]) }))
        .filter(d => d.parsed > 0 && d.parsed < 3599)
        .sort((a, b) => a.parsed - b.parsed);
    
    const leaderTime = firstLaps.length > 0 ? firstLaps[0].parsed : 0;
    raceData.rankings.firstLap = firstLaps.map((d, i) => ({
        position: i + 1,
        driver: raceData.players.findIndex(p => p.username === d.username),
        time: d.time,
        gap: i === 0 ? '0:00.000' : formatTimeDiff(d.parsed - leaderTime)
    }));
}

// Render all sections
// Hide chart tooltips on wheel (zoom) or click events
document.addEventListener('wheel', function(e) {
    if (e.target && e.target.tagName === 'CANVAS') {
        document.querySelectorAll('[id^="chartjs-tooltip"]').forEach(el => el.style.opacity = '0');
    }
}, { passive: true });
document.addEventListener('click', function(e) {
    if (e.target && e.target.tagName === 'CANVAS') {
        document.querySelectorAll('[id^="chartjs-tooltip"]').forEach(el => el.style.opacity = '0');
    }
});

function renderAll() {
    if (!raceData) return;
    
    const isQual = raceData.session && raceData.session.type === 'qual';
    const isPractice = raceData.session && raceData.session.type === 'practice';
    
    renderHeader();
    renderSummaryCard();
    renderResults();
    
    // Overview only for races (not qualifying/practice)
    if (!isQual && !isPractice) {
        renderOverview();
    } else {
        // Hide overview tab and content
        const overviewTab = document.querySelector('.tab[data-tab="overview"]');
        if (overviewTab) overviewTab.style.display = 'none';
        document.getElementById('overview').innerHTML = '';
        
        // Rename progress tab for qualifying/practice
        const progressTab = document.querySelector('.tab[data-tab="progress"]');
        if (progressTab) {
            const tabText = progressTab.querySelector('.tab-text');
            if (tabText) tabText.textContent = isQual ? t('qualProgress') : t('practiceProgress');
        }
    }
    
    renderStints();
    renderLapByLap();
    renderGraph();
    renderProgressGraph();
    renderBestTimes();
    renderCompare();
    renderIncidents();
    renderAnalysis();
    renderChat();
}

// Render header
function renderHeader() {
    const race = raceData.session;
    const metadata = raceData.metadata;
    const isQual = race && race.type === 'qual';
    const isPractice = race && race.type === 'practice';
    
    // Session length display: use sessionTime for qual/practice, sessionLength for race
    const sessionDisplay = (isQual || isPractice) && race.sessionTime > 0
        ? `${race.sessionTime} min`
        : translateSessionLength(race.sessionLength);
    
    // LFS Logo
    const lfsLogoUrl = 'https://www.lfs.net/static/header/lfslogo.webp';
    
    // Weather/wind removed - will be calculated from solar position in the future
    
    // MPR download link
    let mprLink = '';
    if (raceData.metadata.mprUrl && raceData.metadata.mprUrl.trim() !== '') {
        mprLink = ` | <a href="${raceData.metadata.mprUrl}" class="mpr-download" target="_blank" rel="noopener">📥 ${t('downloadReplay')}</a>`;
    }
    
    // Host flags badges
    let flagsBadges = '';
    const flags = (race.flags || []).filter(f => !(isQual && f === 'mustPit'));
    if (flags.length > 0) {
        const flagLabels = {
            mustPit: { icon: '🔧', label: t('flagMustPit') || 'Mandatory Pit' },
            canReset: { icon: '🔄', label: t('flagCanReset') || 'Reset Allowed' },
            noRefuel: { icon: '⛽', label: t('flagNoRefuel') || 'No Refuel' },
            noFloodLights: { icon: '🌙', label: t('flagNoFlood') || 'No Flood Lights' },
            showFuel: { icon: '📊', label: t('flagShowFuel') || 'Fuel Info' },
            forcedCockpit: { icon: '👁️', label: t('flagForcedCockpit') || 'Cockpit View' },
            midRaceJoin: { icon: '🚪', label: t('flagMidRaceJoin') || 'Mid-Race Join' }
        };
        const badges = flags.map(f => {
            const info = flagLabels[f] || { icon: '🏴', label: f };
            return `<span class="flag-badge" title="${info.label}">${info.icon} ${info.label}</span>`;
        }).join('');
        flagsBadges = `<div class="flags-row">${badges}</div>`;
    }
    
    // Session type label
    const sessionTypeLabel = isQual ? t('qualResults') : isPractice ? t('practiceResults') : t('raceResults');
    
    // Server name
    const serverName = race.server && race.server.trim() ? race.server : '';
    const serverHtml = serverName ? `<div style="font-size:11px;color:rgba(255,255,255,0.4);letter-spacing:1px;text-transform:uppercase;margin-top:2px;">${serverName}</div>` : '';
    
    // Custom logo from metadata
    const customLogoUrl = raceData.metadata.logoUrl || '';
    const customLogoHtml = customLogoUrl
        ? `<div class="header-logo-right"><img src="${customLogoUrl}" alt="Logo" class="custom-logo" onerror="this.style.display='none'"></div>`
        : `<div class="header-logo-right"></div>`;
    
    document.getElementById('race-title').innerHTML = 
        `<div class="header-content">
            <div class="header-logo-left">
                <img src="${lfsLogoUrl}" alt="LFS Logo" class="lfs-logo" onerror="this.style.display='none'" title="Live For Speed">
            </div>
            <div class="header-center">
                <h1>${race.trackName}</h1>
                <div class="track-subtitle">${race.track} · ${sessionDisplay}</div>
                ${serverHtml}
            </div>
            ${customLogoHtml}
        </div>`;
    
    // Info cards
    let infoCards = '';
    const cards = [];
    // Date/time cards disabled until /time command is working
    // cards.push(`<span class="info-card">📅 ${race.date}</span>`);
    // if (race.time) cards.push(`<span class="info-card">🕐 ${race.time}</span>`);
    cards.push(`<span class="info-card">🏎️ ${raceData.cars.length} ${t('drivers')}</span>`);
    if (race.wind) cards.push(`<span class="info-card">💨 ${translateWeather(race.wind)}</span>`);
    if (raceData.metadata.mprUrl && raceData.metadata.mprUrl.trim() !== '') {
        cards.push(`<span class="info-card"><a href="${raceData.metadata.mprUrl}" class="mpr-download" target="_blank" rel="noopener">📥 ${t('downloadReplay')}</a></span>`);
    }
    infoCards = `<div class="info-cards">${cards.join('')}</div>`;
    
    // Allowed cars images
    let allowedCarsHtml = '';
    const carImages = race.carImages || {};
    const allowedCars = race.allowedCars || [];
    let carsToShow = allowedCars.length > 0 ? allowedCars : [...new Set(raceData.cars.map(c => c.car))];
    if (carsToShow.length > 0) {
        const carItems = carsToShow.map(car => {
            let imgUrl, linkUrl;
            if (car.length <= 3) {
                imgUrl = `https://www.lfs.net/static/showroom/cars160/${car}.png`;
                linkUrl = `https://www.lfs.net/cars/${car}`;
            } else {
                imgUrl = carImages[car];
                linkUrl = `https://www.lfs.net/files/vehmods/${car}`;
            }
            if (imgUrl) {
                return `<a href="${linkUrl}" target="_blank" title="${car}" class="allowed-car-img"><img src="${imgUrl}" alt="${car}" onerror="this.parentElement.style.display='none'"></a>`;
            }
            return `<a href="${linkUrl}" target="_blank" class="allowed-car-text">${car}</a>`;
        }).join('');
        allowedCarsHtml = `<div class="allowed-cars-row">${carItems}</div>`;
    }

    document.getElementById('race-info').innerHTML = `${infoCards}${flagsBadges}${allowedCarsHtml}`;
    
    // Inject info into header center column
    const centerEl = document.querySelector('.header-center');
    if (centerEl) {
        centerEl.insertAdjacentHTML('beforeend', `${infoCards}${flagsBadges}${allowedCarsHtml}`);
        document.getElementById('race-info').innerHTML = '';
    }
}

// Render summary card (infographic highlights)
function renderSummaryCard() {
    const container = document.getElementById('summary-card');
    if (!container) return;

    const cars = raceData.cars;
    const isQual = raceData.session && raceData.session.type === 'qual';
    const isPractice = raceData.session && raceData.session.type === 'practice';

    // Total drivers
    const totalDrivers = cars.length;

    // Winner / Pole
    let winnerCar;
    if (isQual) {
        winnerCar = [...cars].sort((a, b) => parseLapTime(a.bestLapTime) - parseLapTime(b.bestLapTime))[0];
    } else {
        winnerCar = cars.find(c => c.position === 1) || cars[0];
    }
    // Fastest lap
    const fastestEntry = raceData.rankings?.fastestLap;
    let fastestTime = '-';
    if (fastestEntry && fastestEntry.time) {
        fastestTime = fastestEntry.time;
    }

    // Top speed
    const topSpeedEntry = raceData.rankings?.topSpeed?.[0];
    let topSpeedVal = '-';
    if (topSpeedEntry) {
        topSpeedVal = topSpeedEntry.speed + ' km/h';
    }

    // Most incidents (contacts)
    const mostContacts = [...cars].sort((a, b) => (b.incidents?.contacts || 0) - (a.incidents?.contacts || 0))[0];
    const mostContactsVal = mostContacts ? (mostContacts.incidents?.contacts || 0) : 0;

    // DNFs
    const dnfCount = cars.filter(c => c.status === 'dnf').length;

    // Total overtakes
    const totalOvertakes = raceData.events?.overtakes?.length || 0;

    // Total pit stops
    const totalPits = cars.reduce((sum, c) => sum + (c.pitStops?.length || 0), 0);

    // Closest finish (race only): gap between P1 and P2
    let closestFinish = null;
    if (!isQual && !isPractice && cars.length >= 2) {
        const p1 = cars.find(c => c.position === 1);
        const p2 = cars.find(c => c.position === 2);
        if (p1 && p2 && p1.totalTime !== 'DNF' && p2.totalTime !== 'DNF') {
            closestFinish = p2.totalTime;
        }
    }

    // Most laps completed
    const maxLaps = Math.max(...cars.map(c => c.lapsCompleted));

    // Build cards
    const driverLink = (car) => {
        if (!car) return '-';
        const colored = car.nameColored ? parseLFSColors(car.nameColored) : escapeHtml(car.name);
        return `<a href="https://www.lfsworld.net/?win=stats&player=${encodeURIComponent(car.username)}" target="_blank" class="link-driver" title="${escapeHtml(car.username)}">${colored}</a>`;
    };
    const playerLink = (playerIdx) => {
        const p = raceData.getPlayer(playerIdx);
        const colored = p.nameColored ? parseLFSColors(p.nameColored) : escapeHtml(p.name);
        return `<a href="https://www.lfsworld.net/?win=stats&player=${encodeURIComponent(p.username)}" target="_blank" class="link-driver" title="${escapeHtml(p.username)}">${colored}</a>`;
    };

    let cards = '';
    const sc = (tab) => `data-goto-tab="${tab}"`;

    // 1. Winner/Pole
    cards += `<div class="summary-stat" ${sc('results')}>
        <span class="stat-icon">${isQual ? '🏎️' : '🏆'}</span>
        <span class="stat-value">${driverLink(winnerCar)}</span>
        <span class="stat-label">${isQual ? t('pole') || 'Pole' : t('winner') || 'Winner'}</span>
    </div>`;

    // 2. Fastest lap
    cards += `<div class="summary-stat" ${sc('best-times')}>
        <span class="stat-icon">⚡</span>
        <span class="stat-value purple">${fastestTime}</span>
        <span class="stat-label">${t('fastestLap')}</span>
        <span class="stat-detail">${playerLink(fastestEntry.driver)}</span>
    </div>`;

    // 3. Top speed (hide if no meaningful data)
    if (topSpeedEntry && topSpeedEntry.speed > 10) {
        cards += `<div class="summary-stat" ${sc('best-times')}>
            <span class="stat-icon">💨</span>
            <span class="stat-value">${topSpeedVal}</span>
            <span class="stat-label">${t('topSpeed')}</span>
            <span class="stat-detail">${playerLink(topSpeedEntry.driver)}</span>
        </div>`;
    }

    // 4. Drivers
    cards += `<div class="summary-stat" ${sc('results')}>
        <span class="stat-icon">👥</span>
        <span class="stat-value">${totalDrivers}</span>
        <span class="stat-label">${t('drivers')}</span>
    </div>`;

    // 5. DNFs (right after drivers - related data)
    if (!isQual && !isPractice && dnfCount > 0) {
        cards += `<div class="summary-stat" ${sc('results')}>
            <span class="stat-icon">🚫</span>
            <span class="stat-value">${dnfCount}</span>
            <span class="stat-label">DNF</span>
        </div>`;
    }

    // 6. Total laps
    cards += `<div class="summary-stat" ${sc('lbl')}>
        <span class="stat-icon">🔄</span>
        <span class="stat-value">${maxLaps}</span>
        <span class="stat-label">${t('laps')}</span>
    </div>`;

    // 7. Overtakes (race only, if any)
    if (!isQual && !isPractice && totalOvertakes > 0) {
        cards += `<div class="summary-stat" ${sc('overview')}>
            <span class="stat-icon">⚔️</span>
            <span class="stat-value">${totalOvertakes}</span>
            <span class="stat-label">${t('overtakes') || 'Overtakes'}</span>
        </div>`;
    }

    // 8. Pit stops (if any)
    if (totalPits > 0) {
        cards += `<div class="summary-stat" ${sc('best-times')}>
            <span class="stat-icon">🔧</span>
            <span class="stat-value">${totalPits}</span>
            <span class="stat-label">${t('pitStops')}</span>
        </div>`;
    }

    // 9. Most contacts
    if (mostContactsVal > 0) {
        cards += `<div class="summary-stat" ${sc('incidents')}>
            <span class="stat-icon">💥</span>
            <span class="stat-value">${mostContactsVal}</span>
            <span class="stat-label">${t('contacts')}</span>
            <span class="stat-detail">${driverLink(mostContacts)}</span>
        </div>`;
    }

    container.innerHTML = `<div class="summary-card">${cards}</div>`;

    // Click handler: activate tab and scroll to section
    container.querySelectorAll('[data-goto-tab]').forEach(el => {
        el.addEventListener('click', (e) => {
            // Don't navigate if clicking a link inside the card
            if (e.target.closest('a')) return;
            const tabId = el.dataset.gotoTab;
            const tab = document.querySelector(`.tab[data-tab="${tabId}"]`);
            if (tab) {
                tab.click();
                const section = document.getElementById(tabId);
                if (section) section.scrollIntoView({ behavior: 'smooth', block: 'start' });
            }
        });
    });
}

// Render results table
function renderResults() {
    const isQual = raceData.session && raceData.session.type === 'qual';
    const isPractice = raceData.session && raceData.session.type === 'practice';
    const resultsTitle = isQual ? t('qualResults') : isPractice ? t('practiceResults') : t('raceResults');
    
    // Find fastest lap time across all drivers
    const fastestLapTime = Math.min(...raceData.cars
        .map(d => parseLapTime(d.bestLapTime))
        .filter(t => t > 0 && t < 3599 && t !== Infinity));
    
    const html = `
        <h2>🏁 ${resultsTitle}</h2>
        <table>
            <thead>
                <tr>
                    <th>${t('pos')}</th>
                    ${!isQual ? `<th>${t('grid')}</th>` : ''}
                    <th>${t('driver')}</th>
                    <th>${t('car')}</th>
                    ${!isQual ? `<th>${t('status')}</th>` : ''}
                    <th>${t('laps')}</th>
                    ${!isQual ? `<th>${t('totalTime')}</th>` : ''}
                    <th>${t('bestLap')}</th>
                    <th>${t('pitStops')}</th>
                    <th>${t('incidents')}</th>
                </tr>
            </thead>
            <tbody>
                ${(isQual 
                    ? [...raceData.cars].sort((a, b) => parseLapTime(a.bestLapTime) - parseLapTime(b.bestLapTime))
                    : raceData.cars
                ).map((d, idx) => {
                    const displayPos = isQual ? idx + 1 : d.position;
                    // Build incidents string - only show non-zero counts
                    const incidents = [];
                    if (d.incidents.yellowFlags > 0) incidents.push(`<span title="${t('yellowFlags')}">⚠️ ${d.incidents.yellowFlags}</span>`);
                    if (d.incidents.blueFlags > 0) incidents.push(`<span title="${t('blueFlags')}">🔵 ${d.incidents.blueFlags}</span>`);
                    if (d.incidents.contacts > 0) incidents.push(`<span title="${t('contacts')}">💥 ${d.incidents.contacts}</span>`);
                    const incidentsStr = incidents.length > 0 ? incidents.join(' | ') : '';
                    
                    // Build car link with image
                    const carHtml = getCarHtml(d.car);
                    
                    // Build driver link to LFSWorld
                    const driverLink = getDriverLink(d);
                    
                    return `
                        <tr>
                            <td class="position pos-${displayPos}">${displayPos}</td>
                            ${!isQual ? `<td>${d.gridPosition}</td>` : ''}
                            <td>${driverLink}</td>
                            <td>${carHtml}</td>
                            ${!isQual ? `<td class="status-${d.status}">${translateStatus(d.status)}</td>` : ''}
                            <td>${d.lapsCompleted}</td>
                            ${!isQual ? `<td>${d.totalTime}</td>` : ''}
                            <td${parseLapTime(d.bestLapTime) < 3599 && Math.abs(parseLapTime(d.bestLapTime) - fastestLapTime) < 0.001 ? ' style="color: #A855F7; font-weight: bold;"' : ''}>${parseLapTime(d.bestLapTime) < 3599 ? d.bestLapTime : '-'}${parseLapTime(d.bestLapTime) < 3599 && d.bestLapNumber != null ? ` <small>(${t('lapLabel')}${d.bestLapNumber})</small>` : ''}${parseLapTime(d.bestLapTime) < 3599 && Math.abs(parseLapTime(d.bestLapTime) - fastestLapTime) < 0.001 ? ' ★' : ''}</td>
                            <td>${d.pitStops.length}</td>
                            <td>${incidentsStr}</td>
                        </tr>
                    `;
                }).join('')}
            </tbody>
        </table>
    `;
    
    document.getElementById('results').innerHTML = html;
}

// Render overview (grid, leaders, climbers)
function renderOverview() {
    const gridOrder = [...raceData.cars].sort((a, b) => a.gridPosition - b.gridPosition);
    const biggestClimber = calculateBiggestClimber();
    const lapsLed = calculateLapsLed();
    
    const html = `
        <h2>📋 ${t('overview')}</h2>
        <div class="section-grid">
            <div class="section-box">
                <h3>🏁 ${t("gridOrder")}</h3>
                <table class="compact-table">
                    <thead>
                        <tr>
                            <th>Grid</th>
                            <th>${t("driver")}</th>
                            <th>Final Pos</th>
                            <th>Change</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${gridOrder.map(d => {
                            const change = d.gridPosition - d.position;
                            const changeStr = change > 0 ? `+${change}` : change;
                            const changeColor = change > 0 ? '#4CAF50' : (change < 0 ? '#F44336' : '#888');
                            return `
                                <tr>
                                    <td class="position pos-${d.gridPosition}">${d.gridPosition}</td>
                                    <td>${getDriverLink(d)}</td>
                                    <td>${d.position}</td>
                                    <td style="color: ${changeColor}; font-weight: bold;">${changeStr}</td>
                                </tr>
                            `;
                        }).join('')}
                    </tbody>
                </table>
            </div>
            
            <div class="section-box">
                <h3>👑 ${t("biggestClimber")}</h3>
                <table class="compact-table">
                    <thead>
                        <tr>
                            <th>${t("rank")}</th>
                            <th>${t("driver")}</th>
                            <th>${t("start")}</th>
                            <th>${t("finish")}</th>
                            <th>${t("gain")}</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${biggestClimber.map((d, i) => `
                            <tr>
                                <td class="position pos-${i + 1}">${i + 1}</td>
                                <td>${getDriverLink(d)}</td>
                                <td class="position pos-${d.gridPosition}">${d.gridPosition}</td>
                                <td class="position pos-${d.position}">${d.position}</td>
                                <td class="text-success">+${d.gain}</td>
                            </tr>
                        `).join('')}
                    </tbody>
                </table>
            </div>
            
            <div class="section-box">
                <h3>⚔️ ${t("combativity")}</h3>
                <table class="compact-table">
                    <thead>
                        <tr>
                            <th>${t('rank')}</th>
                            <th>${t('driver')}</th>
                            <th>${t('overtakesMade')}</th>
                            <th>${t('overtakesReceived')}</th>
                            <th>${t('total')}</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${calculateCombativity().map((d, i) => {
                            const totalClass = d.total > 0 ? 'text-success' : (d.total < 0 ? 'text-danger' : '');
                            const totalPrefix = d.total > 0 ? '+' : '';
                            return `
                            <tr>
                                <td class="position pos-${i + 1}">${i + 1}</td>
                                <td>${getDriverLink(d)}</td>
                                <td class="text-success">${d.overtakesMade}</td>
                                <td class="text-danger">${d.overtakesReceived}</td>
                                <td class="${totalClass}">${totalPrefix}${d.total}</td>
                            </tr>
                        `}).join('')}
                    </tbody>
                </table>
            </div>
            
            <div class="section-box">
                <h3>🏆 ${t("lapsLed")}</h3>
                <table class="compact-table">
                    <thead>
                        <tr>
                            <th>${t("driver")}</th>
                            <th>${t('lapsLed')}</th>
                            <th>${t("percentage")}</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${lapsLed.map(l => `
                            <tr>
                                <td>${getDriverLinkFromLapLed(l)}</td>
                                <td>${l.laps}</td>
                                <td>${l.percentage.toFixed(1)}%</td>
                            </tr>
                        `).join('')}
                    </tbody>
                </table>
            </div>
        </div>
    `;
    
    document.getElementById('overview').innerHTML = html;
}

// Render driver stints/relays table (only for endurance races with takeovers)
function renderStints() {
    // Check if any driver has stints data
    const driversWithStints = raceData.cars.filter(d => d.stints && d.stints.length > 1);
    
    if (driversWithStints.length === 0) {
        document.getElementById('stints-tab').style.display = 'none';
        return;
    }
    
    // Show the tab
    document.getElementById('stints-tab').style.display = '';
    
    let html = `<h2>🔄 ${t('driverStints')}</h2>`;
    html += `<div class="stints-grid">`;
    
    driversWithStints.forEach(d => {
        // Group stints by unique driver and sum laps
        const driverSummary = {};
        const driverOrder = [];
        d.stints.forEach(stint => {
            const laps = stint.toLap - stint.fromLap + 1;
            if (!driverSummary[stint.username]) {
                driverSummary[stint.username] = { ...stint, totalLaps: 0, stintDetails: [] };
                driverOrder.push(stint.username);
            }
            driverSummary[stint.username].totalLaps += laps;
            driverSummary[stint.username].stintDetails.push({ from: stint.fromLap, to: stint.toLap, laps });
        });
        
        const teamName = d.nameColored ? parseLFSColors(d.nameColored) : escapeHtml(d.name);
        
        html += `<div class="section-box">`;
        html += `<h3>P${d.position} — ${teamName}</h3>`;
        
        // Summary bar showing stint proportions
        const totalLaps = d.lapsCompleted;
        if (totalLaps > 0) {
            const stintColors = ['#9333ea', '#f59e0b', '#22c55e', '#06b6d4', '#ec4899', '#84cc16', '#14b8a6', '#f472b6'];
            html += `<div class="stint-bar" style="display:flex;height:24px;border-radius:4px;overflow:hidden;margin-bottom:12px;">`;
            let colorIdx = 0;
            const colorMap = {};
            d.stints.forEach(stint => {
                if (!colorMap[stint.username]) {
                    colorMap[stint.username] = stintColors[colorIdx % stintColors.length];
                    colorIdx++;
                }
                const laps = stint.toLap - stint.fromLap + 1;
                const pct = (laps / totalLaps * 100).toFixed(1);
                const bg = colorMap[stint.username];
                html += `<div style="width:${pct}%;background:${bg};display:flex;align-items:center;justify-content:center;font-size:10px;color:white;min-width:20px;" title="${stint.name}: L${stint.fromLap}-L${stint.toLap} (${laps})">${laps > 5 ? stint.name : ''}</div>`;
            });
            html += `</div>`;
        }
        
        // Detailed stints table
        html += `<table><thead><tr>`;
        html += `<th>#</th><th>${t('stintDriver')}</th><th>${t('stintFrom')}</th><th>${t('stintTo')}</th><th>${t('stintLaps')}</th>`;
        html += `</tr></thead><tbody>`;
        d.stints.forEach((stint, idx) => {
            const laps = stint.toLap - stint.fromLap + 1;
            const driverLink = `<a href="https://www.lfsworld.net/?win=stats&racer=${encodeURIComponent(stint.username)}" target="_blank" class="link-driver">${stint.nameColored ? parseLFSColors(stint.nameColored) : escapeHtml(stint.name)}</a>`;
            html += `<tr><td>${idx + 1}</td><td>${driverLink}</td><td>${t('lapLabel')}${stint.fromLap}</td><td>${t('lapLabel')}${stint.toLap}</td><td>${laps}</td></tr>`;
        });
        html += `</tbody></table>`;
        
        // Driver summary
        html += `<h4 style="margin-top:12px;">${t('totalLaps')}</h4>`;
        html += `<table><thead><tr><th>${t('stintDriver')}</th><th>${t('stintLaps')}</th><th>%</th></tr></thead><tbody>`;
        driverOrder.forEach(username => {
            const info = driverSummary[username];
            const pct = totalLaps > 0 ? (info.totalLaps / totalLaps * 100).toFixed(1) : '0.0';
            const driverLink = `<a href="https://www.lfsworld.net/?win=stats&racer=${encodeURIComponent(info.username)}" target="_blank" class="link-driver">${info.nameColored ? parseLFSColors(info.nameColored) : escapeHtml(info.name)}</a>`;
            html += `<tr><td>${driverLink}</td><td>${info.totalLaps}</td><td>${pct}%</td></tr>`;
        });
        html += `</tbody></table>`;
        html += `</div>`;
    });
    html += `</div>`;
    
    document.getElementById('stints').innerHTML = html;
}

// Render lap by lap table
function renderLapByLap() {
    const race = raceData.session;
    const drivers = raceData.cars;
    const maxLaps = Math.max(...drivers.map(d => d.lapTimes ? d.lapTimes.length : 0));
    
    if (maxLaps === 0) {
        document.getElementById('lbl').innerHTML = '<p>No lap time data available</p>';
        return;
    }
    
    // Sort drivers by final position
    const sortedDrivers = [...drivers].sort((a, b) => a.position - b.position);
    
    // Find best lap time for color scaling (excluding first laps)
    const allLapTimes = [];
    drivers.forEach(d => {
        if (d.lapTimes) {
            d.lapTimes.forEach((t, idx) => {
                // Skip first lap for best lap calculation
                if (idx === 0) return;
                const parsed = parseLapTime(t);
                if (parsed !== Infinity) allLapTimes.push(parsed);
            });
        }
    });
    const bestLapTime = Math.min(...allLapTimes);
    
    // Find best first lap time for first lap color scaling
    const firstLapTimes = [];
    drivers.forEach(d => {
        if (d.lapTimes && d.lapTimes[0]) {
            const parsed = parseLapTime(d.lapTimes[0]);
            if (parsed !== Infinity && parsed < 3599) firstLapTimes.push(parsed);
        }
    });
    const bestFirstLap = firstLapTimes.length > 0 
        ? Math.min(...firstLapTimes)
        : bestLapTime;
    
    // Color scale (exact colors from CESAV)
    const colorScale = [
        { percent: 100.0, color: '#7070FF' },   // Blue (best)
        { percent: 100.5, color: '#20F0C0' },   // Cyan
        { percent: 101.75, color: '#A0F00F' },  // Green
        { percent: 103.0, color: '#FFFF70' },   // Yellow
        { percent: 105.25, color: '#FFA070' },  // Orange
        { percent: 107.0, color: '#FF5090' }    // Pink/Red (worst)
    ];
    
    function getLapColor(lapTime, lapNumber) {
        if (!lapTime || lapTime === '-') return '#333';
        
        const parsed = parseLapTime(lapTime);
        if (parsed === Infinity) return '#333';
        
        // First lap uses best first lap as reference
        const referenceTime = lapNumber === 1 ? bestFirstLap : bestLapTime;
        const percent = (parsed / referenceTime) * 100;
        
        // Find the appropriate color based on percentage
        let color = '#FF5090'; // Default to worst
        for (let i = 0; i < colorScale.length; i++) {
            if (percent <= colorScale[i].percent) {
                color = colorScale[i].color;
                break;
            }
        }
        
        return color;
    }
    
    // Create map for pit stops
    const pitStopMap = {};
    drivers.forEach(d => {
        pitStopMap[d.username] = new Set(d.pitStops.map(p => p.lap));
    });
    
    // Build table HTML - TRANSPOSED (rows = laps, columns = drivers)
    let html = `
        <h2>📊 ${t('lapByLapTable')}</h2>
        <div class="scrollable-container">
            <table class="lbl-table">
                <thead>
                    <tr>
                        <th class="lbl-sticky-header-pos">${t('lapHeader')}</th>
                        ${sortedDrivers.map(d => `<th title="${escapeHtml(d.name)}">P${d.position}</th>`).join('')}
                    </tr>
                    <tr>
                        <th class="lbl-sticky-header-pos"></th>
                        ${sortedDrivers.map(d => {
                            const names = (d.stints || []).reduce((acc, s) => {
                                const seen = acc.map(a => a.driver);
                                if (!seen.includes(s.driver)) acc.push(s);
                                return acc;
                            }, []);
                            const displayName = names.length > 1
                                ? names.map(s => { const p = raceData.getPlayer(s.driver); return p.nameColored ? parseLFSColors(p.nameColored) : escapeHtml(p.name); }).join('<br>')
                                : (d.nameColored ? parseLFSColors(d.nameColored) : escapeHtml(d.name));
                            return `<th class="driver-name-header"><small>${displayName}</small></th>`;
                        }).join('')}
                    </tr>
                </thead>
                <tbody>
    `;
    
    // Each row is a lap
    for (let lap = 0; lap < maxLaps; lap++) {
        html += `
            <tr>
                <td class="lbl-sticky-pos"><strong>${t('lapLabel')}${lap + 1}</strong></td>
        `;
        
        // Each column is a driver (sorted by position)
        sortedDrivers.forEach(driver => {
            const lapTime = driver.lapTimes && driver.lapTimes[lap] ? driver.lapTimes[lap] : '-';
            const bgColor = getLapColor(lapTime, lap + 1);
            const hasPit = pitStopMap[driver.username] && pitStopMap[driver.username].has(lap + 1);
            const pitIndicator = hasPit ? ' <span title="Pit Stop">🔧</span>' : '';
            
            html += `<td class="lap-cell" style="background: ${bgColor};">${lapTime}${pitIndicator}</td>`;
        });
        
        html += `</tr>`;
    }
    
    html += `
                </tbody>
            </table>
        </div>
        <div class="legend">
            <h4>${t('legend')}:</h4>
            <div class="legend-items">
                <div class="legend-item">
                    <div class="legend-color-box color-box-100"></div>
                    <span>100.0%</span>
                </div>
                <div class="legend-item">
                    <div class="legend-color-box color-box-101"></div>
                    <span>100.5%</span>
                </div>
                <div class="legend-item">
                    <div class="legend-color-box color-box-102"></div>
                    <span>101.75%</span>
                </div>
                <div class="legend-item">
                    <div class="legend-color-box color-box-103"></div>
                    <span>103.0%</span>
                </div>
                <div class="legend-item">
                    <div class="legend-color-box color-box-105"></div>
                    <span>105.25%</span>
                </div>
                <div class="legend-item">
                    <div class="legend-color-box color-box-107"></div>
                    <span>107.0%+</span>
                </div>
                <div class="legend-item">
                    <span>🔧</span>
                    <span>${t('pitStop')}</span>
                </div>
            </div>
        </div>
    `;
    
    document.getElementById('lbl').innerHTML = html;
}

// Render position graph
// Global variable for position chart instance
let positionChartInstance = null;

// Global state for marker visibility (Position Graph)
const markerVisibility = {
    pitStops: true,
    dnf: true,
    lapped: true,
    personalBest: true,
    fastestLap: true
};

// Global state for marker visibility (Race Progress)
const progressMarkerVisibility = {
    pitStops: true,
    dnf: true
};

// Qualifying position graph: time-based, position from best lap ranking
function renderQualifyingGraph(canvas, sortedDrivers, baseColors, race) {
    const drivers = raceData.cars;
    
    // 1. Build per-driver lap events from lapETimes.
    //    In qualifying: lapETimes[0] = first line crossing (out lap),
    //    lapETimes[i] for i>=1 = lap i completion.
    //    Lap time = lapETimes[i] - lapETimes[i-1].
    //    Each completed lap (i>=1) generates a point on the graph.
    const driverEvents = {};  // username → [{ time, lapTime }]
    drivers.forEach(driver => {
        if (!driver.lapETimes || driver.lapETimes.length < 2) return;
        
        driverEvents[driver.username] = [];
        for (let i = 1; i < driver.lapETimes.length; i++) {
            const lapStartTime = driver.lapETimes[i - 1];
            const lapTime = driver.lapETimes[i] - lapStartTime;
            // Skip absurd lap times (pit stops, resets, etc. > 5 min)
            if (lapTime > 300) continue;
            // Skip laps that started after the session time limit
            const sessionLimit = race.sessionTime > 0 ? race.sessionTime * 60 : 0;
            if (sessionLimit > 0 && lapStartTime >= sessionLimit) continue;
            driverEvents[driver.username].push({
                time: driver.lapETimes[i],    // instant the lap was completed
                lapTime: lapTime,             // duration of this lap in seconds
                lapNumber: i
            });
        }
        if (driverEvents[driver.username].length === 0) {
            delete driverEvents[driver.username];
        }
    });
    
    // 2. Create a global timeline of ALL events (any driver completing a lap)
    const allEvents = [];
    Object.entries(driverEvents).forEach(([username, evts]) => {
        evts.forEach(e => {
            allEvents.push({ time: e.time, username, lapTime: e.lapTime });
        });
    });
    allEvents.sort((a, b) => a.time - b.time);
    
    if (allEvents.length === 0) {
        canvas.style.display = 'none';
        return;
    }
    
    // 3. Walk through events chronologically, maintaining best lap per driver
    //    and recalculating the full ranking at each event.
    const bestTimes = {};  // username → best lap time so far
    
    // For each driver, store their data points: [{ x: time, y: position }]
    const driverDataPoints = {};
    drivers.forEach(d => { driverDataPoints[d.username] = []; });
    
    // Track current position per driver so we can detect when a non-involved driver
    // needs a new point (their position shifted because someone else improved)
    const currentPositions = {};  // username → current position
    
    allEvents.forEach(event => {
        // Update best time for the driver who just completed a lap
        if (!bestTimes[event.username] || event.lapTime < bestTimes[event.username]) {
            bestTimes[event.username] = event.lapTime;
        }
        
        // Recalculate full ranking from best times
        const ranked = Object.entries(bestTimes).sort((a, b) => a[1] - b[1]);
        const newPositions = {};
        ranked.forEach(([username, _], idx) => { newPositions[username] = idx + 1; });
        
        // The driver who triggered this event ALWAYS gets a point
        if (newPositions[event.username] !== undefined) {
            driverDataPoints[event.username].push({
                x: event.time,
                y: newPositions[event.username]
            });
        }
        
        // Any OTHER driver whose position changed also gets a point at this instant
        Object.keys(newPositions).forEach(username => {
            if (username === event.username) return;  // already added
            if (currentPositions[username] !== newPositions[username]) {
                driverDataPoints[username].push({
                    x: event.time,
                    y: newPositions[username]
                });
            }
        });
        
        // Update current positions
        Object.assign(currentPositions, newPositions);
    });
    
    // 4. Build Chart.js datasets
    const maxTime = Math.max(...allEvents.map(e => e.time));
    const maxDrivers = Object.keys(bestTimes).length;
    
    // Extend each driver's line to maxTime with their final position
    Object.keys(driverDataPoints).forEach(username => {
        const pts = driverDataPoints[username];
        if (pts.length > 0) {
            const lastPt = pts[pts.length - 1];
            if (lastPt.x < maxTime) {
                pts.push({ x: maxTime, y: lastPt.y });
            }
        }
    });
    
    const datasets = sortedDrivers.map((driver, index) => {
        const color = baseColors[index % baseColors.length];
        const points = driverDataPoints[driver.username] || [];
        
        return {
            label: driver.name,
            data: points,
            borderColor: color,
            backgroundColor: color,
            borderWidth: 2.5,
            pointRadius: 4,
            pointHoverRadius: 7,
            pointHoverBackgroundColor: color,
            pointHoverBorderColor: '#fff',
            pointHoverBorderWidth: 2,
            tension: 0,
            stepped: 'before',
            spanGaps: true,
            showLine: true,
            driverData: driver
        };
    }).filter(ds => ds.data.length > 0);  // Only show drivers with data
    
    // 5. Build pit stop markers: find the ETime closest to each pit stop
    //    Pit stop on lap N means it happened after completing that lap
    const pitStopMarkers = [];  // [{ x: time, y: position, driver, duration }]
    drivers.forEach(driver => {
        if (!driver.pitStops || driver.pitStops.length === 0) return;
        if (!driver.lapETimes || driver.lapETimes.length === 0) return;
        
        driver.pitStops.forEach(pit => {
            // Pit on lap N → happened around lapETimes[N] (after Nth lap completion)
            // lapETimes[0] = out lap, so pit lap N maps to lapETimes[N]
            const pitLap = pit.lap;
            if (pitLap >= 0 && pitLap < driver.lapETimes.length) {
                const pitTime = driver.lapETimes[pitLap];
                // Find position at this time from the driver's data points
                const driverPts = driverDataPoints[driver.username] || [];
                let pos = null;
                for (let i = driverPts.length - 1; i >= 0; i--) {
                    if (driverPts[i].x <= pitTime + 1) {
                        pos = driverPts[i].y;
                        break;
                    }
                }
                if (pos !== null) {
                    // Find color for this driver
                    const driverIdx = sortedDrivers.findIndex(d => d.username === driver.username);
                    const color = driverIdx >= 0 ? baseColors[driverIdx % baseColors.length] : '#888';
                    pitStopMarkers.push({
                        x: pitTime,
                        y: pos,
                        driver: driver.name,
                        duration: pit.duration,
                        color: color
                    });
                }
            }
        });
    });
    
    // 6. Session time limit zone
    const sessionTimeSec = race.sessionTime > 0 ? race.sessionTime * 60 : 0;  // e.g. 600s for 10 min
    
    // Find the last lap started before the limit for each driver,
    // and the latest finish time among those laps
    let lastValidFinish = sessionTimeSec;
    if (sessionTimeSec > 0) {
        Object.entries(driverEvents).forEach(([username, evts]) => {
            // Each event's .time is when the lap FINISHED.
            // The lap STARTED at the previous ETime.
            // evts are ordered chronologically.
            const driverETimes = drivers.find(d => d.username === username)?.lapETimes || [];
            
            evts.forEach((evt, i) => {
                // Lap i started at driverETimes[i] (the previous crossing), finished at evt.time = driverETimes[i+1]
                const lapStartTime = driverETimes[i];  // start of this lap
                const lapEndTime = evt.time;            // end of this lap
                
                // If lap started before the limit and finished after, it's valid extra time
                if (lapStartTime < sessionTimeSec && lapEndTime > lastValidFinish) {
                    lastValidFinish = lapEndTime;
                }
            });
        });
    }
    
    const sessionLimitPlugin = {
        id: 'qualSessionLimit',
        beforeDatasetsDraw(chart) {
            if (sessionTimeSec <= 0) return;
            const ctx = chart.ctx;
            const xScale = chart.scales.x;
            const yScale = chart.scales.y;
            const chartArea = chart.chartArea;
            
            const limitPixel = xScale.getPixelForValue(sessionTimeSec);
            
            // Only draw if the limit is visible in the chart area
            if (limitPixel < chartArea.left || limitPixel > chartArea.right) return;
            
            // Draw shaded zone from session limit to end of chart
            const rightEdge = chartArea.right;
            if (limitPixel < rightEdge) {
                ctx.save();
                ctx.fillStyle = 'rgba(255, 100, 100, 0.08)';
                ctx.fillRect(limitPixel, chartArea.top, rightEdge - limitPixel, chartArea.bottom - chartArea.top);
                ctx.restore();
            }
            
            // Draw vertical dashed line at session limit
            ctx.save();
            ctx.strokeStyle = 'rgba(255, 200, 50, 0.6)';
            ctx.lineWidth = 2;
            ctx.setLineDash([6, 4]);
            ctx.beginPath();
            ctx.moveTo(limitPixel, chartArea.top);
            ctx.lineTo(limitPixel, chartArea.bottom);
            ctx.stroke();
            
            // Label
            ctx.fillStyle = 'rgba(255, 200, 50, 0.8)';
            ctx.font = 'bold 11px Arial';
            ctx.textAlign = 'left';
            ctx.textBaseline = 'top';
            ctx.fillText(`⏱ ${race.sessionTime} min`, limitPixel + 4, chartArea.top + 4);
            ctx.restore();
        }
    };
    
    // Pit stop drawing plugin
    const pitStopPlugin = {
        id: 'qualPitStops',
        afterDatasetsDraw(chart) {
            if (pitStopMarkers.length === 0) return;
            const ctx = chart.ctx;
            const xScale = chart.scales.x;
            const yScale = chart.scales.y;
            
            if (!markerVisibility.pitStops) return;
            // Build set of hidden driver names
            const hiddenDrivers = new Set();
            chart.data.datasets.forEach((ds, i) => {
                if (chart.getDatasetMeta(i).hidden) hiddenDrivers.add(ds.driverData.name);
            });
            pitStopMarkers.filter(m => !hiddenDrivers.has(m.driver)).forEach(marker => {
                const xPixel = xScale.getPixelForValue(marker.x);
                const yPixel = yScale.getPixelForValue(marker.y);
                
                // Draw white circle with "P" (same style as race graph)
                ctx.save();
                ctx.fillStyle = '#FFFFFF';
                ctx.strokeStyle = marker.color || '#888';
                ctx.lineWidth = 2;
                ctx.beginPath();
                ctx.arc(xPixel, yPixel, 8, 0, Math.PI * 2);
                ctx.fill();
                ctx.stroke();
                ctx.fillStyle = '#000000';
                ctx.font = 'bold 11px Arial';
                ctx.textAlign = 'center';
                ctx.textBaseline = 'middle';
                ctx.fillText('P', xPixel, yPixel);
                ctx.restore();
            });
        }
    };
    
    // Format time for X-axis: mm:ss
    function formatSessionTime(seconds) {
        const mins = Math.floor(seconds / 60);
        const secs = Math.floor(seconds % 60);
        return `${mins}:${secs.toString().padStart(2, '0')}`;
    }
    
    positionChartInstance = new Chart(canvas, {
        type: 'line',
        data: { datasets },
        plugins: [sessionLimitPlugin, pitStopPlugin],
        options: {
            responsive: true,
            maintainAspectRatio: false,
            animation: false,
            interaction: {
                mode: 'nearest',
                intersect: false
            },
            plugins: {
                title: {
                    display: true,
                    text: [t('qualPositionGraph'), `${race.track} - ${race.sessionTime > 0 ? race.sessionTime + ' min' : race.sessionLength}`],
                    color: '#FFD700',
                    font: { size: 20, weight: 'bold' }
                },
                legend: {
                    display: true,
                    position: 'right',
                    align: 'start',
                    maxHeight: 600,
                    labels: {
                        color: '#fff',
                        usePointStyle: true,
                        padding: 12,
                        font: { size: 9 },
                        boxWidth: 12,
                        generateLabels: function(chart) {
                            const spacers = [
                                { text: '', hidden: true, fillStyle: 'transparent', strokeStyle: 'transparent', datasetIndex: -1 },
                                { text: '', hidden: true, fillStyle: 'transparent', strokeStyle: 'transparent', datasetIndex: -1 }
                            ];
                            const driverLabels = chart.data.datasets.map((dataset, i) => {
                                const driver = dataset.driverData;
                                const meta = chart.getDatasetMeta(i);
                                return {
                                    text: `P${driver.position} ${driver.name}`,
                                    fillStyle: dataset.borderColor,
                                    strokeStyle: dataset.borderColor,
                                    lineWidth: dataset.borderWidth,
                                    hidden: meta.hidden,
                                    datasetIndex: i,
                                    fontColor: '#FFFFFF'
                                };
                            });
                            return [...spacers, ...driverLabels];
                        }
                    },
                    onClick: function(e, legendItem, legend) {
                        const index = legendItem.datasetIndex;
                        if (index < 0) return;
                        const chart = legend.chart;
                        const meta = chart.getDatasetMeta(index);
                        meta.hidden = !meta.hidden;
                        chart.update();
                    }
                },
                tooltip: {
                    enabled: false,
                    external: function(context) {
                        let tooltipEl = document.getElementById('chartjs-tooltip-qual');
                        if (!tooltipEl) {
                            tooltipEl = document.createElement('div');
                            tooltipEl.id = 'chartjs-tooltip-qual';
                            tooltipEl.style.background = 'rgba(0, 0, 0, 0.9)';
                            tooltipEl.style.borderRadius = '4px';
                            tooltipEl.style.color = 'white';
                            tooltipEl.style.pointerEvents = 'none';
                            tooltipEl.style.position = 'absolute';
                            tooltipEl.style.transform = 'translate(-50%, 0)';
                            tooltipEl.style.transition = 'all .1s ease';
                            tooltipEl.style.padding = '12px';
                            tooltipEl.style.fontFamily = 'Arial, sans-serif';
                            tooltipEl.style.fontSize = '11px';
                            tooltipEl.style.zIndex = '1000';
                            tooltipEl.style.minWidth = '380px';
                            document.body.appendChild(tooltipEl);
                        }
                        
                        const tooltipModel = context.tooltip;
                        if (tooltipModel.opacity === 0) {
                            tooltipEl.style.opacity = 0;
                            return;
                        }
                        
                        if (tooltipModel.body) {
                            const hoveredTime = tooltipModel.dataPoints[0].parsed.x;
                            
                            // Find the position snapshot closest to (but not after) this time
                            // Rebuild ranking at this time: best lap per driver considering only events <= hoveredTime
                            const bestAtTime = {};
                            allEvents.forEach(evt => {
                                if (evt.time > hoveredTime) return;
                                if (!bestAtTime[evt.username] || evt.lapTime < bestAtTime[evt.username]) {
                                    bestAtTime[evt.username] = evt.lapTime;
                                }
                            });
                            
                            const ranked = Object.entries(bestAtTime)
                                .sort((a, b) => a[1] - b[1])
                                .map(([username, best], idx) => ({ username, best, position: idx + 1 }));
                            
                            // Find previous snapshot to detect position changes
                            const prevBest = {};
                            let prevTime = 0;
                            // Find the last event BEFORE hoveredTime
                            for (let i = allEvents.length - 1; i >= 0; i--) {
                                if (allEvents[i].time < hoveredTime - 0.01) {
                                    prevTime = allEvents[i].time;
                                    break;
                                }
                            }
                            allEvents.forEach(evt => {
                                if (evt.time > prevTime) return;
                                if (!prevBest[evt.username] || evt.lapTime < prevBest[evt.username]) {
                                    prevBest[evt.username] = evt.lapTime;
                                }
                            });
                            const prevRanked = Object.entries(prevBest)
                                .sort((a, b) => a[1] - b[1]);
                            const prevPositions = {};
                            prevRanked.forEach(([username, _], idx) => { prevPositions[username] = idx + 1; });
                            
                            // Build HTML
                            let html = `<div style="font-weight:bold;font-size:13px;margin-bottom:8px;color:#FFD700;">⏱ ${formatSessionTime(hoveredTime)}</div>`;
                            
                            ranked.forEach(entry => {
                                const driver = drivers.find(d => d.username === entry.username);
                                if (!driver) return;
                                const dsIndex = datasets.findIndex(ds => ds.driverData && ds.driverData.username === entry.username);
                                const color = dsIndex >= 0 ? datasets[dsIndex].borderColor : '#888';
                                const coloredName = driver.nameColored ? parseLFSColors(driver.nameColored) : driver.name;
                                const bestStr = SessionStats_LfsTimeToString(entry.best);
                                
                                // Position change arrow
                                const prev = prevPositions[entry.username];
                                let arrow = '<span style="min-width:16px;display:inline-block;"></span>';
                                if (prev !== undefined && prev !== entry.position) {
                                    if (entry.position < prev) {
                                        arrow = `<span style="color:#00FF00;min-width:16px;display:inline-block;">▲</span>`;
                                    } else {
                                        arrow = `<span style="color:#FF4444;min-width:16px;display:inline-block;">▼</span>`;
                                    }
                                }
                                
                                html += `<div style="display:flex;gap:6px;align-items:center;margin:3px 0;">`;
                                // Check if driver is in pit around this time
                                let isInPit = false;
                                if (driver.pitStops && driver.lapETimes) {
                                    isInPit = driver.pitStops.some(ps => {
                                        const pitTime = ps.lap < driver.lapETimes.length ? driver.lapETimes[ps.lap] : null;
                                        return pitTime !== null && Math.abs(pitTime - hoveredTime) < 30;
                                    });
                                }
                                const pitIcon = isInPit ? '<span style="min-width:18px;display:inline-block;text-align:center;">🔧</span>' : '<span style="min-width:18px;display:inline-block;"></span>';
                                
                                html += `<span style="display:inline-block;width:12px;height:12px;background:${color};border-radius:2px;flex-shrink:0;"></span>`;
                                html += `<span style="color:#888;min-width:28px;">P${entry.position}</span>`;
                                html += arrow;
                                html += pitIcon;
                                html += `<span style="flex:1;">${coloredName}</span>`;
                                html += `<span style="color:#AAA;font-size:10px;min-width:60px;text-align:right;">${bestStr}</span>`;
                                html += `</div>`;
                            });
                            
                            tooltipEl.innerHTML = html;
                        }
                        
                        const position = context.chart.canvas.getBoundingClientRect();
                        tooltipEl.style.opacity = 1;
                        const leftPos = position.left + window.pageXOffset + tooltipModel.caretX;
                        const tw = tooltipEl.offsetWidth || 380;
                        if (leftPos - tw / 2 < 10) {
                            tooltipEl.style.transform = 'translate(0, 0)';
                            tooltipEl.style.left = '10px';
                        } else if (leftPos + tw / 2 > window.innerWidth - 10) {
                            tooltipEl.style.transform = 'translate(-100%, 0)';
                            tooltipEl.style.left = (window.innerWidth - 10) + 'px';
                        } else {
                            tooltipEl.style.transform = 'translate(-50%, 0)';
                            tooltipEl.style.left = leftPos + 'px';
                        }
                        // Position tooltip within viewport
                        const th = tooltipEl.offsetHeight || 200;
                        const idealTop = position.top + window.pageYOffset + tooltipModel.caretY;
                        const viewTop = window.pageYOffset;
                        const viewBottom = viewTop + window.innerHeight;
                        let finalTop = idealTop;
                        if (idealTop + th > viewBottom - 10) finalTop = viewBottom - th - 10;
                        if (finalTop < viewTop + 10) finalTop = viewTop + 10;
                        tooltipEl.style.top = finalTop + 'px';
                        tooltipEl.style.maxHeight = (window.innerHeight - 20) + 'px';
                        tooltipEl.style.overflowY = 'auto';
                    }
                },
                zoom: {
                    zoom: {
                        wheel: { enabled: true },
                        pinch: { enabled: true },
                        mode: 'xy',
                        onZoomStart: function() { document.querySelectorAll('[id^="chartjs-tooltip"]').forEach(el => el.style.opacity = '0'); }
                    },
                    pan: {
                        enabled: true,
                        mode: 'xy',
                        modifierKey: null,
                        onPanStart: function() { document.querySelectorAll('[id^="chartjs-tooltip"]').forEach(el => el.style.opacity = '0'); }
                    }
                }
            },
            scales: {
                x: {
                    type: 'linear',
                    min: 0,
                    max: maxTime * 1.02,  // Small padding
                    title: {
                        display: true,
                        text: t('time'),
                        color: '#FFD700',
                        font: { size: 14, weight: 'bold' }
                    },
                    ticks: {
                        color: '#888',
                        font: { size: 11 },
                        callback: function(value) {
                            return formatSessionTime(value);
                        },
                        stepSize: 60
                    },
                    grid: { color: '#2a2a2a', lineWidth: 0.5 }
                },
                y: {
                    reverse: true,
                    min: 1,
                    max: maxDrivers,
                    title: {
                        display: true,
                        text: t('positionAxis'),
                        color: '#FFD700',
                        font: { size: 14, weight: 'bold' }
                    },
                    ticks: {
                        stepSize: 1,
                        color: '#888',
                        font: { size: 11 }
                    },
                    grid: { color: '#2a2a2a', lineWidth: 0.5 }
                }
            }
        }
    });
    
    // Double-click to reset zoom
    canvas.addEventListener('dblclick', function() {
        if (positionChartInstance) {
            positionChartInstance.resetZoom();
        }
    });
}

// Helper to format lap time from seconds (for tooltip)
function SessionStats_LfsTimeToString(totalSeconds) {
    const mins = Math.floor(totalSeconds / 60);
    const secs = (totalSeconds % 60).toFixed(3);
    return `${mins}:${parseFloat(secs) < 10 ? '0' : ''}${secs}`;
}

// Render lap time heatmap
function renderHeatmap() {
    const container = document.getElementById('heatmap');
    if (!container) return;

    const isQual = raceData.session && raceData.session.type === 'qual';
    const cars = isQual
        ? [...raceData.cars].sort((a, b) => parseLapTime(a.bestLapTime) - parseLapTime(b.bestLapTime))
        : [...raceData.cars];

    // Find max laps
    const maxLaps = Math.max(...cars.map(c => c.lapTimes ? c.lapTimes.length : 0));
    if (maxLaps === 0) {
        container.innerHTML = '';
        return;
    }

    // Collect all valid lap times for color scale (exclude first lap and pit laps)
    const allTimes = [];
    cars.forEach(c => {
        const pitLaps = new Set((c.pitStops || []).map(p => p.lap));
        c.lapTimes.forEach((t, i) => {
            if (i === 0) return; // skip first lap
            if (pitLaps.has(i)) return;
            const p = parseLapTime(t);
            if (p > 0 && p < 3599) allTimes.push(p);
        });
    });

    if (allTimes.length === 0) {
        container.innerHTML = '';
        return;
    }

    allTimes.sort((a, b) => a - b);
    // Use percentiles for better color distribution
    const p5 = allTimes[Math.floor(allTimes.length * 0.05)] || allTimes[0];
    const p50 = allTimes[Math.floor(allTimes.length * 0.5)];
    const p95 = allTimes[Math.floor(allTimes.length * 0.95)] || allTimes[allTimes.length - 1];
    const fastest = allTimes[0];

    // Color function: green (fast) → yellow (mid) → red (slow)
    function heatColor(time) {
        if (time <= 0 || time >= 3599) return 'transparent';
        // Normalize between p5 and p95
        let ratio = (time - p5) / (p95 - p5);
        ratio = Math.max(0, Math.min(1, ratio));

        let r, g, b;
        if (ratio < 0.5) {
            // Green to Yellow
            const t = ratio * 2;
            r = Math.round(40 + t * 215);
            g = Math.round(180 - t * 30);
            b = Math.round(40 - t * 20);
        } else {
            // Yellow to Red
            const t = (ratio - 0.5) * 2;
            r = Math.round(255);
            g = Math.round(150 - t * 150);
            b = Math.round(20);
        }
        return `rgb(${r},${g},${b})`;
    }

    // Fastest lap overall
    const globalFastest = Math.min(...allTimes);

    // Build table
    let html = `<h2>🌡️ ${t('heatmap') || 'Heatmap'}</h2>`;
    html += `<div class="heatmap-wrap"><table class="heatmap-table">`;
    html += `<thead><tr><th class="heatmap-driver">${t('driver')}</th>`;
    for (let i = 0; i < maxLaps; i++) {
        html += `<th class="heatmap-lap">${i + 1}</th>`;
    }
    html += `</tr></thead><tbody>`;

    cars.forEach((c, idx) => {
        const pos = isQual ? idx + 1 : c.position;
        const pitLaps = new Set((c.pitStops || []).map(p => p.lap));
        const bestTime = parseLapTime(c.bestLapTime);

        html += `<tr><td class="heatmap-driver"><span class="heatmap-pos pos-${pos}">P${pos}</span> ${getDriverLink(c)}</td>`;

        for (let i = 0; i < maxLaps; i++) {
            const lapTime = c.lapTimes && c.lapTimes[i] ? c.lapTimes[i] : null;
            const parsed = lapTime ? parseLapTime(lapTime) : 0;
            const valid = parsed > 0 && parsed < 3599;

            let cellClass = '';
            let style = '';
            let content = '';

            if (!valid) {
                style = 'background: rgba(255,255,255,0.02);';
                content = i < (c.lapTimes ? c.lapTimes.length : 0) ? '' : '';
            } else {
                const isPit = pitLaps.has(i);
                const isFastest = Math.abs(parsed - globalFastest) < 0.001;
                const isPersonalBest = Math.abs(parsed - bestTime) < 0.001;
                const bg = heatColor(parsed);
                style = `background: ${bg};`;

                if (isFastest) {
                    cellClass = 'heatmap-fastest';
                } else if (isPersonalBest) {
                    cellClass = 'heatmap-pb';
                }
                if (isPit) {
                    cellClass += ' heatmap-pit';
                }
            }

            html += `<td class="heatmap-cell ${cellClass}" style="${style}" title="${valid ? formatLapTime(parsed) : '-'}"></td>`;
        }

        html += `</tr>`;
    });

    html += `</tbody></table></div>`;

    // Legend
    html += `<div class="heatmap-legend">
        <span class="heatmap-legend-fast">■</span> ${t('fast') || 'Fast'}
        <span class="heatmap-legend-mid">■</span> ${t('average') || 'Average'}
        <span class="heatmap-legend-slow">■</span> ${t('slow') || 'Slow'}
        <span class="heatmap-fastest-mark">■</span> ★ ${t('fastestLap')}
        <span class="heatmap-pb-mark">■</span> ${t('personalBest')}
        <span class="heatmap-pit-mark">P</span> ${t('pitStop')}
    </div>`;

    container.innerHTML = html;
}

function renderGraph() {
    const canvas = document.getElementById('position-graph');
    const race = raceData.session;
    const drivers = raceData.cars;
    const isQualSession = race && race.type === 'qual';
    
    // Check if any car has positions data
    if (!drivers || !drivers.some(d => d.positions && d.positions.length > 0)) {
        canvas.style.display = 'none';
        return;
    }
    
    canvas.style.display = 'block';
    
    // Destroy previous chart instance
    if (positionChartInstance) {
        positionChartInstance.destroy();
        positionChartInstance = null;
    }
    
    // Define colors for all drivers (30 distinct colors)
    const baseColors = [
        '#FF0000', '#0000FF', '#00FF00', '#FFA500', '#800080', '#00FFFF',
        '#FF00FF', '#FFFF00', '#FF1493', '#008080', '#D2691E', '#00FF7F',
        '#8B0000', '#4169E1', '#FFD700', '#DC143C', '#32CD32', '#4B0082',
        '#FF4500', '#9ACD32', '#FF69B4', '#1E90FF', '#ADFF2F', '#FF6347',
        '#BA55D3', '#20B2AA', '#F08080', '#FFDAB9', '#87CEEB', '#98FB98'
    ];
    
    // Sort drivers by final position for consistent color assignment
    const sortedDrivers = [...drivers].sort((a, b) => a.position - b.position);

    // ==========================================
    // QUALIFYING: Temporal position graph
    // ==========================================
    if (isQualSession) {
        // Hide non-applicable legend items (keep only pit stops)
        const legendItems = document.querySelectorAll('#graph .graph-legend .legend-item');
        legendItems.forEach((item, i) => {
            if (i >= 1 && i <= 4) item.style.display = 'none';  // personal best, fastest, DNF, lapped
        });
        renderQualifyingGraph(canvas, sortedDrivers, baseColors, race);
        return;
    }

    // ==========================================
    // RACE: Lap-based position graph
    // ==========================================
    
    // Get total timing points from first driver
    const totalTimingPoints = drivers[0]?.positions?.length || 0;
    const totalLaps = race.laps;
    
    // Calculate points per lap from actual data (more reliable than splitsPerLap+1)
    const finisher = drivers.find(d => d.lapsCompleted === totalLaps);
    const pointsPerLap = finisher ? Math.round((finisher.positions.length - 1) / finisher.lapsCompleted) : ((raceData.session.splitsPerLap || 2) + 1);
    
    // Create X-axis labels with proper lap mapping
    const xLabels = [];
    
    for (let i = 0; i < totalTimingPoints; i++) {
        if (i === 0) {
            xLabels.push(t('startLabel'));
        } else if (i % pointsPerLap === 0) {
            const lap = i / pointsPerLap;
            if (lap <= totalLaps) {
                xLabels.push(t('lapLabel') + lap);
            } else {
                xLabels.push('');
            }
        } else if (i === totalTimingPoints - 1) {
            xLabels.push(t('finishLabel'));
        } else {
            xLabels.push('');
        }
    }
    
    // Create datasets for each driver
    const datasets = sortedDrivers.map((driver, index) => {
        let driverPositions = driver.positions || [];
        const color = baseColors[index % baseColors.length];
        const isDNF = driver.status === 'dnf';
        
        // Truncate positions after DNF (replace with null after last completed lap)
        if (isDNF && driver.lapsCompleted > 0) {
            const lastValidIndex = driver.lapsCompleted * pointsPerLap;
            driverPositions = driverPositions.map((v, i) => i <= lastValidIndex ? v : null);
        }
        
        return {
            label: driver.name,
            data: driverPositions,
            borderColor: color,
            backgroundColor: color,
            borderWidth: 2.5,
            borderDash: [],
            pointRadius: 0,
            pointHoverRadius: 5,
            pointHoverBackgroundColor: color,
            pointHoverBorderColor: '#fff',
            pointHoverBorderWidth: 2,
            tension: 0,
            spanGaps: false,
            driverData: driver
        };
    });
    
    // Create plugin for pit stop markers
    const pitStopPlugin = {
        id: 'pitStopMarkers',
        afterDatasetsDraw: function(chart) {
            const ctx = chart.ctx;
            const xScale = chart.scales.x;
            const yScale = chart.scales.y;
            
            chart.data.datasets.forEach((dataset, datasetIndex) => {
                const driver = sortedDrivers[datasetIndex];
                const color = dataset.borderColor;
                
                // Check if this dataset is hidden (legend toggle)
                const meta = chart.getDatasetMeta(datasetIndex);
                if (meta.hidden) {
                    return; // Skip all markers for hidden driver
                }
                
                // Draw pit stop markers
                if (markerVisibility.pitStops && driver.pitStops && driver.pitStops.length > 0) {
                    driver.pitStops.forEach(pit => {
                        const pitLap = pit.lap;
                        
                        // Calculate timing point index for this lap (proportional)
                        const lapProgress = pitLap / totalLaps;
                        const timingPointIndex = Math.round(lapProgress * (totalTimingPoints - 1));
                        
                        // Get position at this timing point
                        const position = dataset.data[timingPointIndex];
                        
                        if (position && position > 0) {
                            const x = xScale.getPixelForValue(timingPointIndex);
                            const y = yScale.getPixelForValue(position);
                            
                            // Draw white circle with P
                            ctx.save();
                            ctx.fillStyle = '#FFFFFF';
                            ctx.strokeStyle = color;
                            ctx.lineWidth = 2;
                            ctx.beginPath();
                            ctx.arc(x, y, 8, 0, Math.PI * 2);
                            ctx.fill();
                            ctx.stroke();
                            
                            ctx.fillStyle = '#000000';
                            ctx.font = 'bold 11px Arial';
                            ctx.textAlign = 'center';
                            ctx.textBaseline = 'middle';
                            ctx.fillText('P', x, y);
                            ctx.restore();
                        }
                    });
                }
                
                // Draw DNF marker (X) at last valid position
                if (markerVisibility.dnf && driver.status === 'dnf') {
                    const driverPositions = dataset.data;
                    
                    // Find last valid position (> 0)
                    let lastValidIndex = -1;
                    let lastValidPosition = 0;
                    
                    for (let i = driverPositions.length - 1; i >= 0; i--) {
                        if (driverPositions[i] && driverPositions[i] > 0) {
                            lastValidIndex = i;
                            lastValidPosition = driverPositions[i];
                            break;
                        }
                    }
                    
                    if (lastValidIndex >= 0) {
                        const x = xScale.getPixelForValue(lastValidIndex);
                        const y = yScale.getPixelForValue(lastValidPosition);
                        
                        // Draw red X
                        ctx.save();
                        ctx.strokeStyle = '#FF0000';
                        ctx.lineWidth = 3;
                        ctx.lineCap = 'round';
                        
                        const size = 8;
                        ctx.beginPath();
                        ctx.moveTo(x - size, y - size);
                        ctx.lineTo(x + size, y + size);
                        ctx.moveTo(x + size, y - size);
                        ctx.lineTo(x - size, y + size);
                        ctx.stroke();
                        ctx.restore();
                    }
                }
                
                // Check if personal best and fastest lap are the same lap
                const hasFastestLap = raceData.rankings.fastestLap && driver.username === raceData.getPlayer(raceData.rankings.fastestLap.driver).username;
                const fastestLapNumber = hasFastestLap ? raceData.rankings.fastestLap.lap : -1;
                const personalBestLapNumber = driver.bestLapNumber || -1;
                const bothMarkersSameLap = hasFastestLap && fastestLapNumber === personalBestLapNumber;
                const bothMarkersVisible = markerVisibility.personalBest && markerVisibility.fastestLap;
                
                // Draw personal best lap marker (triangle)
                // Skip if both markers are on same lap and both are visible (fastest lap star takes priority)
                if (markerVisibility.personalBest && personalBestLapNumber > 0 && 
                    !(bothMarkersSameLap && bothMarkersVisible)) {
                    
                    const bestLapNumber = personalBestLapNumber;
                    const lapProgress = bestLapNumber / totalLaps;
                    const timingPointIndex = Math.round(lapProgress * (totalTimingPoints - 1));
                    const position = dataset.data[timingPointIndex];
                    
                    if (position && position > 0) {
                        const x = xScale.getPixelForValue(timingPointIndex);
                        const y = yScale.getPixelForValue(position);
                        
                        // Draw triangle (pointing right)
                        ctx.save();
                        ctx.fillStyle = color;
                        ctx.strokeStyle = '#FFFFFF';
                        ctx.lineWidth = 1.5;
                        ctx.beginPath();
                        ctx.moveTo(x - 6, y - 7);
                        ctx.lineTo(x + 6, y);
                        ctx.lineTo(x - 6, y + 7);
                        ctx.closePath();
                        ctx.fill();
                        ctx.stroke();
                        ctx.restore();
                    }
                }
                
                // Draw fastest lap marker (star) - only for the driver with overall fastest lap
                if (markerVisibility.fastestLap && hasFastestLap) {
                    const lapProgress = fastestLapNumber / totalLaps;
                    const timingPointIndex = Math.round(lapProgress * (totalTimingPoints - 1));
                    const position = dataset.data[timingPointIndex];
                    
                    if (position && position > 0) {
                        const x = xScale.getPixelForValue(timingPointIndex);
                        const y = yScale.getPixelForValue(position);
                        
                        // Draw star
                        ctx.save();
                        ctx.fillStyle = '#FFD700';
                        ctx.strokeStyle = color;
                        ctx.lineWidth = 2;
                        ctx.beginPath();
                        for (let i = 0; i < 5; i++) {
                            const angle = (i * 4 * Math.PI) / 5 - Math.PI / 2;
                            const xPos = x + 9 * Math.cos(angle);
                            const yPos = y + 9 * Math.sin(angle);
                            if (i === 0) ctx.moveTo(xPos, yPos);
                            else ctx.lineTo(xPos, yPos);
                        }
                        ctx.closePath();
                        ctx.fill();
                        ctx.stroke();
                        ctx.restore();
                    }
                }
                
                // Draw dashed line for lapped drivers to finish
                if (markerVisibility.lapped && driver.status === 'lapped') {
                    const driverPositions = dataset.data;
                    
                    // Find last valid position
                    let lastValidIndex = -1;
                    let lastValidPosition = 0;
                    
                    for (let i = driverPositions.length - 1; i >= 0; i--) {
                        if (driverPositions[i] && driverPositions[i] > 0) {
                            lastValidIndex = i;
                            lastValidPosition = driverPositions[i];
                            break;
                        }
                    }
                    
                    // If last valid point is before the end, draw dashed line to finish
                    if (lastValidIndex >= 0 && lastValidIndex < totalTimingPoints - 1) {
                        const startX = xScale.getPixelForValue(lastValidIndex);
                        const startY = yScale.getPixelForValue(lastValidPosition);
                        const endX = xScale.getPixelForValue(totalTimingPoints - 1);
                        const endY = yScale.getPixelForValue(driver.position); // Final position from results
                        
                        ctx.save();
                        ctx.strokeStyle = color;
                        ctx.lineWidth = 2;
                        ctx.setLineDash([5, 5]); // Dashed pattern
                        ctx.beginPath();
                        ctx.moveTo(startX, startY);
                        ctx.lineTo(endX, endY);
                        ctx.stroke();
                        ctx.setLineDash([]); // Reset dash
                        ctx.restore();
                    }
                }
            });
        }
    };
    
    // Create Chart.js chart
    positionChartInstance = new Chart(canvas, {
        type: 'line',
        data: {
            labels: xLabels,
            datasets: datasets
        },
        plugins: [pitStopPlugin],
        options: {
            responsive: true,
            maintainAspectRatio: false,
            animation: false, // Disable animation so dashed lines don't look out of sync
            interaction: {
                mode: 'index',
                axis: 'x',
                intersect: false
            },
            plugins: {
                title: {
                    display: true,
                    text: [isQualSession ? t('qualPositionGraph') : t('racePositionGraph'), `${race.track} - ${race.laps} ${t('laps')}`],
                    color: '#FFD700',
                    font: {
                        size: 20,
                        weight: 'bold'
                    }
                },
                legend: {
                    display: true,
                    position: 'right',
                    align: 'start',
                    maxHeight: 600,
                    labels: {
                        color: '#fff',
                        usePointStyle: true,
                        padding: 12,
                        font: {
                            size: 9
                        },
                        boxWidth: 12,
                        generateLabels: function(chart) {
                            const datasets = chart.data.datasets;
                            // Add spacers at top to push legend down
                            const spacers = [
                                { text: '', hidden: true, fillStyle: 'transparent', strokeStyle: 'transparent', datasetIndex: -1 },
                                { text: '', hidden: true, fillStyle: 'transparent', strokeStyle: 'transparent', datasetIndex: -1 }
                            ];
                            
                            // Generate labels with position prefix or DNF
                            const driverLabels = datasets.map((dataset, i) => {
                                const driver = sortedDrivers[i];
                                const positionText = (!isQualSession && driver.status === 'dnf') ? 'DNF' : `P${driver.position}`;
                                const meta = chart.getDatasetMeta(i);
                                return {
                                    text: `${positionText} ${driver.name}`,
                                    fillStyle: dataset.borderColor,
                                    strokeStyle: dataset.borderColor,
                                    lineWidth: dataset.borderWidth,
                                    lineDash: dataset.borderDash,
                                    hidden: meta.hidden, // Use actual hidden state
                                    datasetIndex: i,
                                    fontColor: '#FFFFFF' // White text for better contrast
                                };
                            });
                            
                            return [...spacers, ...driverLabels];
                        }
                    },
                    onClick: function(e, legendItem, legend) {
                        const index = legendItem.datasetIndex;
                        if (index < 0) return; // Ignore spacer clicks
                        const chart = legend.chart;
                        const meta = chart.getDatasetMeta(index);
                        meta.hidden = !meta.hidden;
                        chart.update();
                    }
                },
                tooltip: {
                    enabled: false, // Disable default tooltip
                    external: function(context) {
                        // Custom HTML tooltip for Position Graph
                        let tooltipEl = document.getElementById('chartjs-tooltip-position');
                        
                        if (!tooltipEl) {
                            tooltipEl = document.createElement('div');
                            tooltipEl.id = 'chartjs-tooltip-position';
                            tooltipEl.style.background = 'rgba(0, 0, 0, 0.9)';
                            tooltipEl.style.borderRadius = '4px';
                            tooltipEl.style.color = 'white';
                            tooltipEl.style.opacity = 1;
                            tooltipEl.style.pointerEvents = 'none';
                            tooltipEl.style.position = 'absolute';
                            tooltipEl.style.transform = 'translate(-50%, 0)';
                            tooltipEl.style.transition = 'all .1s ease';
                            tooltipEl.style.padding = '12px';
                            tooltipEl.style.fontFamily = 'Arial, sans-serif';
                            tooltipEl.style.fontSize = '11px';
                            tooltipEl.style.zIndex = '1000';
                            tooltipEl.style.minWidth = '350px';
                            document.body.appendChild(tooltipEl);
                        }
                        
                        const tooltipModel = context.tooltip;
                        
                        if (tooltipModel.opacity === 0) {
                            tooltipEl.style.opacity = 0;
                            return;
                        }
                        
                        if (tooltipModel.body) {
                            const timingPointIndex = tooltipModel.dataPoints[0].dataIndex;
                            const label = context.chart.data.labels[timingPointIndex];
                            
                            // Extract current lap from timing point index
                            // Structure: GRID(0), S1(1), LAP(2), S1(3), LAP(4), ...
                            // i=0: grid (before lap 1)
                            // i=1: split 1 of lap 1
                            // i=2: completion of lap 1 (starting lap 2)
                            // i=3: split 1 of lap 2
                            // i=4: completion of lap 2 (starting lap 3)
                            let currentLap = 0;
                            if (timingPointIndex === 0) {
                                currentLap = 0; // Grid/start
                            } else {
                                // Calculate which lap we're in based on timing point index
                                currentLap = Math.ceil(timingPointIndex / pointsPerLap);
                            }
                            
                            // Collect all drivers (including DNF and lapped finishers)
                            // Iterate over ALL datasets, not just dataPoints
                            const driversAtPoint = [];
                            context.chart.data.datasets.forEach((dataset, index) => {
                                const driverData = dataset.driverData;
                                if (!driverData) return;
                                
                                // Get position at this timing point (may be null for lapped/DNF)
                                let position = dataset.data[timingPointIndex];
                                
                                // Check if driver has DNF'd by this point
                                let hasDNFd = false;
                                if (driverData.status === 'dnf') {
                                    const lapsCompleted = driverData.lapsCompleted || 0;
                                    hasDNFd = currentLap > lapsCompleted;
                                }
                                
                                // Check if driver is a lapped finisher (lapped/finished but no position data at this point)
                                let isLappedFinisher = false;
                                if (!position && (driverData.status === 'lapped' || driverData.status === 'finished')) {
                                    const lapsCompleted = driverData.lapsCompleted || 0;
                                    // If they finished/lapped but we're past their last lap
                                    if (currentLap > lapsCompleted) {
                                        isLappedFinisher = true;
                                        position = driverData.position; // Use final position
                                    }
                                }
                                
                                // Include if: has valid position OR has DNF'd OR is lapped finisher
                                if ((position && position > 0) || hasDNFd || isLappedFinisher) {
                                    // Check if driver is currently in pit at this timing point
                                    let isInPit = false;
                                    if (driverData.pitStops) {
                                        isInPit = driverData.pitStops.some(ps => ps.lap === currentLap);
                                    }
                                    
                                    // Count pit stops up to current lap
                                    let pitStopsCount = 0;
                                    if (driverData.pitStops) {
                                        pitStopsCount = driverData.pitStops.filter(ps => ps.lap <= currentLap).length;
                                    }
                                    
                                    // Get position at previous timing point for arrow
                                    let prevPosition = null;
                                    if (timingPointIndex > 0) {
                                        const prevPos = dataset.data[timingPointIndex - 1];
                                        if (prevPos && prevPos > 0) prevPosition = prevPos;
                                    }
                                    
                                    driversAtPoint.push({
                                        name: dataset.label,
                                        nameColored: driverData.nameColored,
                                        driverData: driverData,
                                        position: position > 0 ? position : null,
                                        prevPosition: prevPosition,
                                        isDNF: hasDNFd,
                                        isInPit: isInPit,
                                        pitStops: pitStopsCount,
                                        color: dataset.borderColor
                                    });
                                }
                            });
                            
                            // Sort: active drivers by position first, then DNF at the end
                            driversAtPoint.sort((a, b) => {
                                if (a.isDNF && !b.isDNF) return 1;  // DNF goes after active
                                if (!a.isDNF && b.isDNF) return -1; // Active goes before DNF
                                if (a.isDNF && b.isDNF) return 0;   // DNF order doesn't matter
                                return a.position - b.position;     // Active sorted by position
                            });
                            
                            // Build HTML
                            let innerHtml = '<div style="font-weight:bold;font-size:13px;margin-bottom:8px;color:#FFFFFF;">';
                            innerHtml += `${t('timingPoint')}: ${label}`;
                            innerHtml += '</div>';
                            
                            let dnfSeparatorAdded = false;
                            driversAtPoint.forEach((driver) => {
                                // Add separator line before first DNF
                                if (driver.isDNF && !dnfSeparatorAdded) {
                                    innerHtml += `<div style="border-top:1px solid #444;margin:8px 0 4px 0;"></div>`;
                                    dnfSeparatorAdded = true;
                                }
                                
                                // Parse LFS colors in driver name
                                const coloredName = driver.nameColored ? parseLFSColors(driver.nameColored) : driver.name;
                                
                                // Position change arrow
                                let arrow = '<span style="min-width:16px;display:inline-block;"></span>';
                                if (!driver.isDNF && driver.prevPosition !== null && driver.prevPosition !== driver.position) {
                                    if (driver.position < driver.prevPosition) {
                                        arrow = `<span style="color:#00FF00;min-width:16px;display:inline-block;">▲</span>`;
                                    } else {
                                        arrow = `<span style="color:#FF4444;min-width:16px;display:inline-block;">▼</span>`;
                                    }
                                }
                                
                                // Pit stop icon if currently in pit
                                const pitIcon = driver.isInPit ? '<span style="min-width:18px;display:inline-block;text-align:center;">🔧</span>' : '<span style="min-width:18px;display:inline-block;"></span>';
                                
                                // Pit stops count text
                                const pitText = driver.pitStops > 0 ? ` (${driver.pitStops} ${driver.pitStops > 1 ? t('pits') : t('pit')})` : '';
                                
                                // Position text: "DNF" or "P{number}"
                                const posText = driver.isDNF ? 'DNF' : `P${driver.position}`;
                                const posColor = driver.isDNF ? '#FF6666' : '#888';
                                
                                // Current stint driver (for relay races)
                                const stintDriver = (driver.driverData.stints && driver.driverData.stints.length > 1) ? getStintDriverAtLap(driver.driverData, currentLap) : null;
                                const stintHtml = stintDriver ? `<span style="color:#AAA;font-size:10px;"> [${stintDriver.name}]</span>` : '';
                                
                                innerHtml += `<div style="display:flex;gap:6px;align-items:center;margin:4px 0;">`;
                                innerHtml += `<span style="display:inline-block;width:12px;height:12px;background:${driver.color};border-radius:2px;flex-shrink:0;"></span>`;
                                innerHtml += `<span style="color:${posColor};min-width:30px;">${posText}</span>`;
                                innerHtml += arrow;
                                innerHtml += pitIcon;
                                innerHtml += `<span style="flex:1;">${coloredName}${stintHtml}</span>`;
                                if (pitText) innerHtml += `<span style="color:#CCC;font-size:10px;">${pitText}</span>`;
                                innerHtml += `</div>`;
                            });
                            
                            tooltipEl.innerHTML = innerHtml;
                        }
                        
                        const position = context.chart.canvas.getBoundingClientRect();
                        tooltipEl.style.opacity = 1;
                        const leftPos = position.left + window.pageXOffset + tooltipModel.caretX;
                        const tw = tooltipEl.offsetWidth || 350;
                        if (leftPos - tw / 2 < 10) {
                            tooltipEl.style.transform = 'translate(0, 0)';
                            tooltipEl.style.left = '10px';
                        } else if (leftPos + tw / 2 > window.innerWidth - 10) {
                            tooltipEl.style.transform = 'translate(-100%, 0)';
                            tooltipEl.style.left = (window.innerWidth - 10) + 'px';
                        } else {
                            tooltipEl.style.transform = 'translate(-50%, 0)';
                            tooltipEl.style.left = leftPos + 'px';
                        }
                        // Position tooltip within viewport
                        const th = tooltipEl.offsetHeight || 200;
                        const idealTop = position.top + window.pageYOffset + tooltipModel.caretY;
                        const viewTop = window.pageYOffset;
                        const viewBottom = viewTop + window.innerHeight;
                        let finalTop = idealTop;
                        if (idealTop + th > viewBottom - 10) finalTop = viewBottom - th - 10;
                        if (finalTop < viewTop + 10) finalTop = viewTop + 10;
                        tooltipEl.style.top = finalTop + 'px';
                        tooltipEl.style.maxHeight = (window.innerHeight - 20) + 'px';
                        tooltipEl.style.overflowY = 'auto';
                    }
                },
                zoom: {
                    limits: {
                        y: {min: 1, max: drivers.length},
                        x: {min: 0, max: totalTimingPoints - 1}
                    },
                    zoom: {
                        wheel: {
                            enabled: true,
                        },
                        pinch: {
                            enabled: true
                        },
                        mode: 'xy',
                        onZoomStart: function() {
                            document.querySelectorAll('[id^="chartjs-tooltip"]').forEach(el => el.style.opacity = '0');
                        },
                    },
                    pan: {
                        enabled: true,
                        mode: 'xy',
                        modifierKey: null, // No key required - direct drag to pan
                        onPanStart: function() {
                            document.querySelectorAll('[id^="chartjs-tooltip"]').forEach(el => el.style.opacity = '0');
                        },
                    }
                }
            },
            scales: {
                x: {
                    display: true,
                    title: {
                        display: true,
                        text: t('lapsAxis'),
                        color: '#FFD700',
                        font: {
                            size: 14,
                            weight: 'bold'
                        }
                    },
                    ticks: {
                        color: '#888',
                        font: {
                            size: 11
                        },
                        autoSkip: true,
                        maxRotation: 0,
                        minRotation: 0
                    },
                    grid: {
                        color: '#2a2a2a',
                        lineWidth: 0.5
                    }
                },
                y: {
                    display: true,
                    reverse: true,
                    min: 1,
                    max: drivers.length,
                    title: {
                        display: true,
                        text: t('positionAxis'),
                        color: '#FFD700',
                        font: {
                            size: 14,
                            weight: 'bold'
                        }
                    },
                    ticks: {
                        stepSize: 1,
                        color: '#888',
                        font: {
                            size: 11
                        }
                    },
                    grid: {
                        color: function(context) {
                            if (!context || !context.tick) return '#2a2a2a';
                            const value = context.tick.value;
                            return value % 5 === 0 ? '#444' : '#2a2a2a';
                        },
                        lineWidth: function(context) {
                            if (!context || !context.tick) return 0.5;
                            const value = context.tick.value;
                            return value % 5 === 0 ? 1.5 : 0.5;
                        }
                    }
                }
            }
        }
    });
    
    // Add double-click to reset zoom for Position Graph
    canvas.addEventListener('dblclick', () => {
        if (positionChartInstance) {
            positionChartInstance.resetZoom();
        }
    });
}
// Render race time progress graph
// Global variable to store chart instance
let progressChartInstance = null;

// Render race time progress graph with Chart.js
function renderQualifyingProgressGraph(canvas, drivers, race) {
    // In qualifying, "progress" = gap to the best lap time at each moment
    // X-axis = session time, Y-axis = gap to best overall lap time
    const sessionTimeSec = race.sessionTime > 0 ? race.sessionTime * 60 : 0;
    
    // Build events from lapETimes (same as position graph)
    const driverEvents = {};  // username → [{ time, lapTime, cumBest }]
    drivers.forEach(driver => {
        if (!driver.lapETimes || driver.lapETimes.length < 2) return;
        driverEvents[driver.username] = [];
        for (let i = 1; i < driver.lapETimes.length; i++) {
            const lapStartTime = driver.lapETimes[i - 1];
            const lapTime = driver.lapETimes[i] - lapStartTime;
            if (lapTime > 300) continue;
            if (sessionTimeSec > 0 && lapStartTime >= sessionTimeSec) continue;
            driverEvents[driver.username].push({
                time: driver.lapETimes[i],
                lapTime: lapTime
            });
        }
        if (driverEvents[driver.username].length === 0) delete driverEvents[driver.username];
    });
    
    // Build global timeline
    const allEvents = [];
    Object.entries(driverEvents).forEach(([username, evts]) => {
        evts.forEach(evt => {
            allEvents.push({ ...evt, username });
        });
    });
    allEvents.sort((a, b) => a.time - b.time);
    
    if (allEvents.length === 0) return;
    
    const maxTime = Math.max(...allEvents.map(e => e.time));
    
    // Calculate gaps: at each event, recalculate best lap for everyone,
    // gap = driver's best lap - overall best lap
    const bestTimes = {};      // username → best lap time so far
    const driverDataPoints = {}; // username → [{ x: time, y: gap }]
    
    // Initialize
    Object.keys(driverEvents).forEach(u => { driverDataPoints[u] = []; });
    
    allEvents.forEach(event => {
        // Update best time for this driver
        if (!bestTimes[event.username] || event.lapTime < bestTimes[event.username]) {
            bestTimes[event.username] = event.lapTime;
        }
        
        // Find overall best
        const overallBest = Math.min(...Object.values(bestTimes));
        
        // Add point for the driver who just completed a lap
        const gap = bestTimes[event.username] - overallBest;
        driverDataPoints[event.username].push({ x: event.time, y: gap });
        
        // Update any other driver whose gap changed (because overall best changed)
        Object.keys(bestTimes).forEach(username => {
            if (username === event.username) return;
            const prevPoints = driverDataPoints[username];
            const newGap = bestTimes[username] - overallBest;
            if (prevPoints.length > 0 && Math.abs(prevPoints[prevPoints.length - 1].y - newGap) > 0.001) {
                prevPoints.push({ x: event.time, y: newGap });
            }
        });
    });
    
    // Extend lines to maxTime
    Object.keys(driverDataPoints).forEach(username => {
        const pts = driverDataPoints[username];
        if (pts.length > 0 && pts[pts.length - 1].x < maxTime) {
            pts.push({ x: maxTime, y: pts[pts.length - 1].y });
        }
    });
    
    // Sort drivers by final gap (best first)
    const sortedDrivers = drivers
        .filter(d => driverDataPoints[d.username] && driverDataPoints[d.username].length > 0)
        .sort((a, b) => {
            const gapA = driverDataPoints[a.username].slice(-1)[0]?.y ?? Infinity;
            const gapB = driverDataPoints[b.username].slice(-1)[0]?.y ?? Infinity;
            return gapA - gapB;
        });
    
    const baseColors = [
        '#FF0000', '#0000FF', '#00FF00', '#FFA500', '#800080', '#00FFFF',
        '#FF00FF', '#FFFF00', '#FF1493', '#008080', '#D2691E', '#00FF7F',
        '#8B0000', '#4169E1', '#FFD700', '#DC143C', '#32CD32', '#4B0082',
        '#FF4500', '#9ACD32', '#FF69B4', '#1E90FF', '#ADFF2F', '#FF6347',
        '#BA55D3', '#20B2AA', '#F08080', '#FFDAB9', '#87CEEB', '#98FB98'
    ];
    
    // Build pit stop markers
    const pitStopMarkers = [];
    drivers.forEach(driver => {
        if (!driver.pitStops || driver.pitStops.length === 0) return;
        if (!driver.lapETimes || driver.lapETimes.length === 0) return;
        driver.pitStops.forEach(pit => {
            const pitLap = pit.lap;
            if (pitLap >= 0 && pitLap < driver.lapETimes.length) {
                const pitTime = driver.lapETimes[pitLap];
                // Find gap at this time
                const pts = driverDataPoints[driver.username] || [];
                let gap = null;
                for (let i = pts.length - 1; i >= 0; i--) {
                    if (pts[i].x <= pitTime + 1) { gap = pts[i].y; break; }
                }
                if (gap !== null) {
                    const driverIdx = sortedDrivers.findIndex(d => d.username === driver.username);
                    const color = driverIdx >= 0 ? baseColors[driverIdx % baseColors.length] : '#888';
                    pitStopMarkers.push({ x: pitTime, y: gap, driver: driver.name, color, duration: pit.duration });
                }
            }
        });
    });
    
    // Max gap for Y-axis
    let maxGap = 0;
    Object.values(driverDataPoints).forEach(pts => {
        pts.forEach(p => { if (p.y > maxGap) maxGap = p.y; });
    });
    maxGap = Math.ceil(maxGap * 1.1) || 5;
    
    // Session limit plugin
    const sessionLimitPlugin = {
        id: 'progressSessionLimit',
        beforeDatasetsDraw(chart) {
            if (sessionTimeSec <= 0) return;
            const ctx = chart.ctx;
            const xScale = chart.scales.x;
            const chartArea = chart.chartArea;
            const limitPixel = xScale.getPixelForValue(sessionTimeSec);
            if (limitPixel < chartArea.left || limitPixel > chartArea.right) return;
            if (limitPixel < chartArea.right) {
                ctx.save();
                ctx.fillStyle = 'rgba(255, 100, 100, 0.08)';
                ctx.fillRect(limitPixel, chartArea.top, chartArea.right - limitPixel, chartArea.bottom - chartArea.top);
                ctx.restore();
            }
            ctx.save();
            ctx.strokeStyle = 'rgba(255, 200, 50, 0.6)';
            ctx.lineWidth = 2;
            ctx.setLineDash([6, 4]);
            ctx.beginPath();
            ctx.moveTo(limitPixel, chartArea.top);
            ctx.lineTo(limitPixel, chartArea.bottom);
            ctx.stroke();
            ctx.fillStyle = 'rgba(255, 200, 50, 0.8)';
            ctx.font = 'bold 11px Arial';
            ctx.textAlign = 'left';
            ctx.textBaseline = 'top';
            ctx.fillText(`⏱ ${race.sessionTime} min`, limitPixel + 4, chartArea.top + 4);
            ctx.restore();
        }
    };
    
    // Pit stop plugin
    const pitStopPlugin = {
        id: 'progressQualPitStops',
        afterDatasetsDraw(chart) {
            if (!progressMarkerVisibility.pitStops || pitStopMarkers.length === 0) return;
            const ctx = chart.ctx;
            const xScale = chart.scales.x;
            const yScale = chart.scales.y;
            const hiddenDrivers = new Set();
            chart.data.datasets.forEach((ds, i) => {
                if (chart.getDatasetMeta(i).hidden) hiddenDrivers.add(ds.driverData.name);
            });
            pitStopMarkers.filter(m => !hiddenDrivers.has(m.driver)).forEach(marker => {
                const xPixel = xScale.getPixelForValue(marker.x);
                const yPixel = yScale.getPixelForValue(marker.y);
                ctx.save();
                ctx.fillStyle = '#FFFFFF';
                ctx.strokeStyle = marker.color || '#888';
                ctx.lineWidth = 2;
                ctx.beginPath();
                ctx.arc(xPixel, yPixel, 8, 0, Math.PI * 2);
                ctx.fill();
                ctx.stroke();
                ctx.fillStyle = '#000000';
                ctx.font = 'bold 11px Arial';
                ctx.textAlign = 'center';
                ctx.textBaseline = 'middle';
                ctx.fillText('P', xPixel, yPixel);
                ctx.restore();
            });
        }
    };
    
    function formatSessionTime(seconds) {
        const mins = Math.floor(seconds / 60);
        const secs = Math.floor(seconds % 60);
        return `${mins}:${secs.toString().padStart(2, '0')}`;
    }
    
    const datasets = sortedDrivers.map((driver, index) => {
        const color = baseColors[index % baseColors.length];
        const points = driverDataPoints[driver.username] || [];
        return {
            label: driver.name,
            data: points,
            borderColor: color,
            backgroundColor: color,
            pointBackgroundColor: color,
            borderWidth: 2,
            pointRadius: 3,
            pointHoverRadius: 6,
            tension: 0,
            stepped: 'before',
            spanGaps: true,
            showLine: true,
            fill: false,
            driverData: driver
        };
    }).filter(ds => ds.data.length > 0);
    
    if (progressChartInstance) progressChartInstance.destroy();
    
    progressChartInstance = new Chart(canvas, {
        type: 'line',
        data: { datasets },
        plugins: [sessionLimitPlugin, pitStopPlugin],
        options: {
            responsive: true,
            maintainAspectRatio: false,
            animation: false,
            interaction: { mode: 'nearest', intersect: false },
            plugins: {
                title: {
                    display: true,
                    text: [t('qualTimeProgress') || 'Qualifying Time Progress', t('gapToBestLap') || 'Gap to Best Lap'],
                    color: '#FFD700',
                    font: { size: 20, weight: 'bold' }
                },
                legend: {
                    display: true,
                    position: 'right',
                    labels: {
                        color: '#fff',
                        font: { size: 11 },
                        usePointStyle: true,
                        pointStyle: 'circle',
                        padding: 8
                    }
                },
                tooltip: {
                    enabled: false,
                    external: function(context) {
                        let tooltipEl = document.getElementById('chartjs-tooltip-progress');
                        if (!tooltipEl) {
                            tooltipEl = document.createElement('div');
                            tooltipEl.id = 'chartjs-tooltip-progress';
                            tooltipEl.style.background = 'rgba(0, 0, 0, 0.9)';
                            tooltipEl.style.borderRadius = '4px';
                            tooltipEl.style.color = 'white';
                            tooltipEl.style.pointerEvents = 'none';
                            tooltipEl.style.position = 'absolute';
                            tooltipEl.style.transform = 'translate(-50%, 0)';
                            tooltipEl.style.transition = 'all .1s ease';
                            tooltipEl.style.padding = '12px';
                            tooltipEl.style.fontFamily = 'Arial, sans-serif';
                            tooltipEl.style.fontSize = '11px';
                            tooltipEl.style.zIndex = '1000';
                            tooltipEl.style.minWidth = '380px';
                            document.body.appendChild(tooltipEl);
                        }
                        const tooltipModel = context.tooltip;
                        if (tooltipModel.opacity === 0) { tooltipEl.style.opacity = 0; return; }
                        
                        if (tooltipModel.body) {
                            const hoveredTime = tooltipModel.dataPoints[0].parsed.x;
                            
                            // Rebuild ranking at this time
                            const bestAtTime = {};
                            allEvents.forEach(evt => {
                                if (evt.time > hoveredTime) return;
                                if (!bestAtTime[evt.username] || evt.lapTime < bestAtTime[evt.username]) {
                                    bestAtTime[evt.username] = evt.lapTime;
                                }
                            });
                            const overallBest = Math.min(...Object.values(bestAtTime));
                            const ranked = Object.entries(bestAtTime)
                                .sort((a, b) => a[1] - b[1])
                                .map(([username, best], idx) => ({ username, best, gap: best - overallBest, position: idx + 1 }));
                            
                            // Previous ranking for arrows
                            const prevBest = {};
                            let prevTime = 0;
                            for (let i = allEvents.length - 1; i >= 0; i--) {
                                if (allEvents[i].time < hoveredTime - 0.01) { prevTime = allEvents[i].time; break; }
                            }
                            allEvents.forEach(evt => {
                                if (evt.time > prevTime) return;
                                if (!prevBest[evt.username] || evt.lapTime < prevBest[evt.username]) {
                                    prevBest[evt.username] = evt.lapTime;
                                }
                            });
                            const prevRanked = Object.entries(prevBest).sort((a, b) => a[1] - b[1]);
                            const prevPositions = {};
                            prevRanked.forEach(([username, _], idx) => { prevPositions[username] = idx + 1; });
                            
                            let html = `<div style="font-weight:bold;font-size:13px;margin-bottom:8px;color:#FFD700;">⏱ ${formatSessionTime(hoveredTime)}</div>`;
                            
                            ranked.forEach(entry => {
                                const driver = drivers.find(d => d.username === entry.username);
                                if (!driver) return;
                                const dsIndex = datasets.findIndex(ds => ds.driverData && ds.driverData.username === entry.username);
                                const color = dsIndex >= 0 ? datasets[dsIndex].borderColor : '#888';
                                const coloredName = driver.nameColored ? parseLFSColors(driver.nameColored) : driver.name;
                                const bestStr = SessionStats_LfsTimeToString(entry.best);
                                
                                // Arrow
                                const prev = prevPositions[entry.username];
                                let arrow = '<span style="min-width:16px;display:inline-block;"></span>';
                                if (prev !== undefined && prev !== entry.position) {
                                    arrow = entry.position < prev
                                        ? `<span style="color:#00FF00;min-width:16px;display:inline-block;">▲</span>`
                                        : `<span style="color:#FF4444;min-width:16px;display:inline-block;">▼</span>`;
                                }
                                
                                // Pit icon
                                let isInPit = false;
                                if (driver.pitStops && driver.lapETimes) {
                                    isInPit = driver.pitStops.some(ps => {
                                        const pitTime = ps.lap < driver.lapETimes.length ? driver.lapETimes[ps.lap] : null;
                                        return pitTime !== null && Math.abs(pitTime - hoveredTime) < 30;
                                    });
                                }
                                const pitIcon = isInPit ? '<span style="min-width:18px;display:inline-block;text-align:center;">🔧</span>' : '<span style="min-width:18px;display:inline-block;"></span>';
                                
                                // Gap text
                                const gapText = entry.gap === 0 ? t('leader') : (entry.gap >= 60 ? `+${formatTimeDiff(entry.gap)}` : `+${entry.gap.toFixed(3)}s`);
                                
                                html += `<div style="display:flex;gap:6px;align-items:center;margin:3px 0;">`;
                                html += `<span style="display:inline-block;width:12px;height:12px;background:${color};border-radius:2px;flex-shrink:0;"></span>`;
                                html += `<span style="color:#888;min-width:28px;">P${entry.position}</span>`;
                                html += arrow;
                                html += pitIcon;
                                html += `<span style="flex:1;">${coloredName}</span>`;
                                html += `<span style="color:#AAA;font-size:10px;min-width:60px;text-align:right;">${gapText}</span>`;
                                html += `</div>`;
                            });
                            
                            tooltipEl.innerHTML = html;
                        }
                        
                        const position = context.chart.canvas.getBoundingClientRect();
                        tooltipEl.style.opacity = 1;
                        const leftPos = position.left + window.pageXOffset + tooltipModel.caretX;
                        const tw = tooltipEl.offsetWidth || 400;
                        if (leftPos - tw / 2 < 10) {
                            tooltipEl.style.transform = 'translate(0, 0)';
                            tooltipEl.style.left = '10px';
                        } else if (leftPos + tw / 2 > window.innerWidth - 10) {
                            tooltipEl.style.transform = 'translate(-100%, 0)';
                            tooltipEl.style.left = (window.innerWidth - 10) + 'px';
                        } else {
                            tooltipEl.style.transform = 'translate(-50%, 0)';
                            tooltipEl.style.left = leftPos + 'px';
                        }
                        // Position tooltip within viewport
                        const th = tooltipEl.offsetHeight || 200;
                        const idealTop = position.top + window.pageYOffset + tooltipModel.caretY;
                        const viewTop = window.pageYOffset;
                        const viewBottom = viewTop + window.innerHeight;
                        let finalTop = idealTop;
                        if (idealTop + th > viewBottom - 10) finalTop = viewBottom - th - 10;
                        if (finalTop < viewTop + 10) finalTop = viewTop + 10;
                        tooltipEl.style.top = finalTop + 'px';
                        tooltipEl.style.maxHeight = (window.innerHeight - 20) + 'px';
                        tooltipEl.style.overflowY = 'auto';
                    }
                },
                zoom: {
                    zoom: { wheel: { enabled: true }, pinch: { enabled: true }, mode: 'y', onZoomStart: function() { document.querySelectorAll('[id^="chartjs-tooltip"]').forEach(el => el.style.opacity = '0'); } },
                    pan: { enabled: true, mode: 'y', modifierKey: null, onPanStart: function() { document.querySelectorAll('[id^="chartjs-tooltip"]').forEach(el => el.style.opacity = '0'); } }
                }
            },
            scales: {
                x: {
                    type: 'linear',
                    min: 0,
                    max: maxTime * 1.02,
                    title: { display: true, text: t('sessionTime') || 'Session Time', color: '#fff' },
                    ticks: {
                        color: '#888',
                        maxRotation: 0,
                        callback: value => formatSessionTime(value)
                    },
                    grid: { color: '#333' }
                },
                y: {
                    reverse: true,
                    min: 0,
                    title: { display: true, text: t('gapToBestLap') || 'Gap to Best Lap', color: '#fff' },
                    ticks: {
                        color: '#888',
                        callback: value => formatGapValue(value)
                    },
                    grid: {
                        color: ctx => ctx.tick && ctx.tick.value === 0 ? '#666' : '#333',
                        lineWidth: ctx => ctx.tick && ctx.tick.value === 0 ? 2 : 1
                    }
                }
            }
        }
    });
    
    // Double-click to reset zoom
    canvas.addEventListener('dblclick', () => {
        if (progressChartInstance) progressChartInstance.resetZoom();
    });
}

function renderProgressGraph() {
    const canvas = document.getElementById('progress-graph');
    const race = raceData.session;
    const drivers = raceData.cars;
    const isQualSession = race && race.type === 'qual';
    
    if (isQualSession) {
        // Hide non-applicable legend items for qualifying (DNF only, keep pit stops)
        const legendItems = document.querySelectorAll('#progress .graph-legend .legend-item');
        legendItems.forEach((item, i) => {
            if (i === 1) item.style.display = 'none';  // DNF not applicable
        });
        renderQualifyingProgressGraph(canvas, drivers, race);
        return;
    }
    
    // Filter drivers with lap times
    const driversWithLaps = drivers.filter(d => d.lapTimes && d.lapTimes.length > 0);
    if (driversWithLaps.length === 0) {
        return;
    }
    
    // Step 1: Calculate cumulative times for all drivers at each lap completion
    // Start with grid (S) where everyone is at 0
    const maxLaps = Math.max(...driversWithLaps.map(d => d.lapTimes.length));
    const cumulativeTimes = {};
    
    driversWithLaps.forEach(driver => {
        // Grid/Start - staggered start based on grid position
        // Assume 350ms between each grid position
        const gridGap = (driver.gridPosition - 1) * 0.350; // P1 = 0s, P2 = 0.350s, P3 = 0.700s, etc.
        const cumulative = [gridGap];
        
        // Use lapETimes (absolute elapsed times) when available - more accurate than parsing lapTimes strings
        if (driver.lapETimes && driver.lapETimes.length > 0) {
            for (let lap = 0; lap < maxLaps; lap++) {
                if (lap < driver.lapETimes.length && driver.lapETimes[lap] > 0) {
                    cumulative.push(driver.lapETimes[lap]);
                } else {
                    cumulative.push(null);
                }
            }
        } else {
            // Fallback: sum parsed lapTimes
            let totalTime = gridGap;
            for (let lap = 0; lap < maxLaps; lap++) {
                if (lap < driver.lapTimes.length) {
                    const lapTime = parseLapTime(driver.lapTimes[lap]);
                    if (lapTime > 0 && lapTime !== Infinity) {
                        totalTime += lapTime;
                        cumulative.push(totalTime);
                    } else {
                        cumulative.push(null);
                    }
                } else {
                    cumulative.push(null);
                }
            }
        }
        
        cumulativeTimes[driver.username] = cumulative;
    });
    
    const totalTimingPoints = maxLaps + 1; // +1 for grid/start
    
    // Step 2: Calculate gaps relative to CURRENT LEADER at each timing point
    const driverGaps = [];
    
    driversWithLaps.forEach(driver => {
        const gaps = [];
        const driverTimes = cumulativeTimes[driver.username];
        
        for (let i = 0; i < totalTimingPoints; i++) {
            // Find leader (minimum cumulative time) at this timing point
            let leaderTime = Infinity;
            driversWithLaps.forEach(d => {
                const time = cumulativeTimes[d.username][i];
                if (time !== null && time < leaderTime) {
                    leaderTime = time;
                }
            });
            
            const driverTime = driverTimes[i];
            
            if (driverTime !== null && leaderTime !== Infinity && leaderTime >= 0) {
                // Calculate gap: always >= 0 (leader is at 0)
                const gap = driverTime - leaderTime;
                gaps.push(gap);
            } else {
                gaps.push(null);
            }
        }
        
        driverGaps.push({
            driver: driver,
            gaps: gaps
        });
    });
    
    // Calculate maximum gap for Y-axis scaling
    let maxGap = 0;
    driverGaps.forEach(dg => {
        dg.gaps.forEach(gap => {
            if (gap !== null && gap > maxGap) {
                maxGap = gap;
            }
        });
    });
    // Add 10% padding to max gap
    maxGap = Math.ceil(maxGap * 1.1);
    
    // Colors for drivers
    const colors = [
        '#FF0000', '#0000FF', '#00FF00', '#FFA500', '#800080', '#00FFFF',
        '#FF00FF', '#FFFF00', '#FF1493', '#008080', '#D2691E', '#00FF7F',
        '#8B0000', '#4169E1', '#FFD700', '#DC143C', '#32CD32', '#4B0082',
        '#FF4500', '#9ACD32', '#FF69B4', '#1E90FF', '#ADFF2F', '#FF6347',
        '#BA55D3', '#20B2AA', '#F08080', '#FFDAB9', '#87CEEB', '#98FB98'
    ];
    
    // Prepare Chart.js datasets
    const datasets = driverGaps.map((dg, index) => {
        return {
            label: dg.driver.name,
            data: dg.gaps.map((gap, i) => ({
                x: i,
                y: gap
            })),
            borderColor: colors[index % colors.length],
            backgroundColor: colors[index % colors.length] + '20',
            borderWidth: 2,
            pointRadius: 0,
            pointHoverRadius: 5,
            tension: 0.1,
            spanGaps: true,
            // Store driver object for tooltip access
            driverData: dg.driver
        };
    });
    
    // Create X-axis labels: S (start), then L1, L2, L3...
    const xLabels = [];
    for (let i = 0; i < totalTimingPoints; i++) {
        if (i === 0) {
            xLabels.push(t('startLabel')); // Start/Grid
        } else {
            xLabels.push(t('lapLabel') + i); // L1, L2, L3... / V1, V2, V3...
        }
    }
    
    // Create plugin for markers on Race Progress
    const progressMarkersPlugin = {
        id: 'progressMarkers',
        afterDatasetsDraw: function(chart) {
            const ctx = chart.ctx;
            const xScale = chart.scales.x;
            const yScale = chart.scales.y;
            
            chart.data.datasets.forEach((dataset, datasetIndex) => {
                const driverData = dataset.driverData;
                if (!driverData) return;
                
                const color = dataset.borderColor;
                
                // Check if this dataset is hidden (legend toggle)
                const meta = chart.getDatasetMeta(datasetIndex);
                if (meta.hidden) {
                    return; // Skip all markers for hidden driver
                }
                
                // Draw pit stop markers
                if (progressMarkerVisibility.pitStops && driverData.pitStops && driverData.pitStops.length > 0) {
                    driverData.pitStops.forEach(pit => {
                        const pitLap = pit.lap;
                        
                        // Get gap at this lap (timingPoint index = lap number)
                        const gapData = dataset.data[pitLap];
                        
                        if (gapData && gapData.y !== null && gapData.y !== undefined) {
                            const x = xScale.getPixelForValue(gapData.x);
                            const y = yScale.getPixelForValue(gapData.y);
                            
                            // Draw white circle with red border and P
                            ctx.save();
                            ctx.fillStyle = '#FFFFFF';
                            ctx.strokeStyle = '#FF0000';
                            ctx.lineWidth = 2;
                            ctx.beginPath();
                            ctx.arc(x, y, 8, 0, Math.PI * 2);
                            ctx.fill();
                            ctx.stroke();
                            
                            ctx.fillStyle = '#000000';
                            ctx.font = 'bold 11px Arial';
                            ctx.textAlign = 'center';
                            ctx.textBaseline = 'middle';
                            ctx.fillText('P', x, y);
                            ctx.restore();
                        }
                    });
                }
                
                // Draw DNF marker (X) at last valid position
                if (progressMarkerVisibility.dnf && driverData.status === 'dnf') {
                    const lapsCompleted = driverData.lapsCompleted || 0;
                    
                    // Get gap at last completed lap
                    const lastGapData = dataset.data[lapsCompleted];
                    
                    if (lastGapData && lastGapData.y !== null && lastGapData.y !== undefined) {
                        const x = xScale.getPixelForValue(lastGapData.x);
                        const y = yScale.getPixelForValue(lastGapData.y);
                        
                        // Draw red X
                        ctx.save();
                        ctx.strokeStyle = '#FF0000';
                        ctx.lineWidth = 3;
                        ctx.lineCap = 'round';
                        
                        const size = 8;
                        ctx.beginPath();
                        ctx.moveTo(x - size, y - size);
                        ctx.lineTo(x + size, y + size);
                        ctx.moveTo(x + size, y - size);
                        ctx.lineTo(x - size, y + size);
                        ctx.stroke();
                        ctx.restore();
                    }
                }
            });
        }
    };
    
    // Destroy previous chart instance if it exists
    if (progressChartInstance) {
        progressChartInstance.destroy();
    }
    
    // Create Chart.js chart
    progressChartInstance = new Chart(canvas, {
        type: 'line',
        data: {
            labels: xLabels,
            datasets: datasets
        },
        plugins: [progressMarkersPlugin],
        options: {
            responsive: true,
            maintainAspectRatio: false,
            animation: false, // Disable animation for instant rendering
            interaction: {
                mode: 'nearest',
                axis: 'x',
                intersect: false
            },
            plugins: {
                title: {
                    display: true,
                    text: [t('raceTimeProgress'), t('gapToCurrentLeader')],
                    color: '#FFD700',
                    font: {
                        size: 20,
                        weight: 'bold'
                    }
                },
                legend: {
                    display: true,
                    position: 'right',
                    labels: {
                        color: '#fff',
                        font: {
                            size: 11
                        },
                        usePointStyle: true,
                        pointStyle: 'circle',
                        padding: 8
                    }
                },
                tooltip: {
                    enabled: false, // Disable default tooltip
                    external: function(context) {
                        // Custom HTML tooltip with colored driver names
                        let tooltipEl = document.getElementById('chartjs-tooltip-progress');
                        
                        if (!tooltipEl) {
                            tooltipEl = document.createElement('div');
                            tooltipEl.id = 'chartjs-tooltip-progress';
                            tooltipEl.style.background = 'rgba(0, 0, 0, 0.9)';
                            tooltipEl.style.borderRadius = '4px';
                            tooltipEl.style.color = 'white';
                            tooltipEl.style.opacity = 1;
                            tooltipEl.style.pointerEvents = 'none';
                            tooltipEl.style.position = 'absolute';
                            tooltipEl.style.transform = 'translate(-50%, 0)';
                            tooltipEl.style.transition = 'all .1s ease';
                            tooltipEl.style.padding = '12px';
                            tooltipEl.style.fontFamily = 'Arial, sans-serif';
                            tooltipEl.style.fontSize = '11px';
                            tooltipEl.style.zIndex = '1000';
                            tooltipEl.style.minWidth = '350px';
                            document.body.appendChild(tooltipEl);
                        }
                        
                        const tooltipModel = context.tooltip;
                        
                        if (tooltipModel.opacity === 0) {
                            tooltipEl.style.opacity = 0;
                            return;
                        }
                        
                        if (tooltipModel.body) {
                            const timingPoint = tooltipModel.dataPoints[0].dataIndex;
                            const isStart = timingPoint === 0;
                            
                            // Calculate current lap (same logic as Position Graph)
                            const currentLap = timingPoint; // In Race Progress, timingPoint = lap number (0=grid, 1=L1, etc)
                            
                            // Collect all drivers (including DNF and lapped)
                            const driversAtPoint = [];
                            context.chart.data.datasets.forEach((dataset, index) => {
                                const driverData = dataset.driverData;
                                if (!driverData) return;
                                
                                // Get gap at this timing point
                                const gap = dataset.data[timingPoint] ? dataset.data[timingPoint].y : null;
                                
                                // Check if driver has DNF'd by this point
                                let hasDNFd = false;
                                if (driverData.status === 'dnf') {
                                    const lapsCompleted = driverData.lapsCompleted || 0;
                                    hasDNFd = currentLap > lapsCompleted;
                                }
                                
                                // Check if driver is lapped (no gap data but still racing/finished)
                                let isLapped = false;
                                let lapsBehind = 0;
                                if (!gap && !hasDNFd && (driverData.status === 'lapped' || driverData.status === 'finished')) {
                                    const lapsCompleted = driverData.lapsCompleted || 0;
                                    if (currentLap > lapsCompleted) {
                                        isLapped = true;
                                        lapsBehind = currentLap - lapsCompleted;
                                    }
                                }
                                
                                // Include if: has valid gap OR has DNF'd OR is lapped
                                if ((gap !== null && gap !== undefined) || hasDNFd || isLapped) {
                                    // Check if in pit
                                    let isInPit = false;
                                    if (driverData.pitStops) {
                                        isInPit = driverData.pitStops.some(ps => ps.lap === currentLap);
                                    }
                                    // Pit stops count
                                    let pitStopsCount = 0;
                                    if (driverData.pitStops) {
                                        pitStopsCount = driverData.pitStops.filter(ps => ps.lap <= currentLap).length;
                                    }
                                    driversAtPoint.push({
                                        name: dataset.label,
                                        nameColored: driverData.nameColored,
                                        driverData: driverData,
                                        gap: gap !== null && gap !== undefined ? gap : null,
                                        isDNF: hasDNFd,
                                        isLapped: isLapped,
                                        lapsBehind: lapsBehind,
                                        isInPit: isInPit,
                                        pitStops: pitStopsCount,
                                        color: dataset.borderColor
                                    });
                                }
                            });
                            
                            // Sort: active drivers by gap, then lapped, then DNF at the end
                            driversAtPoint.sort((a, b) => {
                                // DNF goes last
                                if (a.isDNF && !b.isDNF) return 1;
                                if (!a.isDNF && b.isDNF) return -1;
                                if (a.isDNF && b.isDNF) return 0;
                                
                                // Lapped goes after active
                                if (a.isLapped && !b.isLapped) return 1;
                                if (!a.isLapped && b.isLapped) return -1;
                                
                                // Both lapped: sort by laps behind
                                if (a.isLapped && b.isLapped) return a.lapsBehind - b.lapsBehind;
                                
                                // Both active: sort by gap
                                return a.gap - b.gap;
                            });
                            
                            // Build previous lap ranking for arrows
                            const prevDriversAtPoint = [];
                            if (timingPoint > 0) {
                                context.chart.data.datasets.forEach((dataset) => {
                                    const dd = dataset.driverData;
                                    if (!dd) return;
                                    const prevGap = dataset.data[timingPoint - 1] ? dataset.data[timingPoint - 1].y : null;
                                    if (prevGap !== null && prevGap !== undefined) {
                                        prevDriversAtPoint.push({ name: dataset.label, gap: prevGap });
                                    }
                                });
                                prevDriversAtPoint.sort((a, b) => a.gap - b.gap);
                            }
                            const prevPositionMap = {};
                            prevDriversAtPoint.forEach((d, i) => { prevPositionMap[d.name] = i + 1; });
                            
                            // Build HTML
                            let innerHtml = '<div style="font-weight:bold;font-size:13px;margin-bottom:8px;color:#FFFFFF;">';
                            innerHtml += isStart ? t('startGrid') : `${t('lapHeader')} ${timingPoint}`;
                            innerHtml += '</div>';
                            
                            let lappedSeparatorAdded = false;
                            let dnfSeparatorAdded = false;
                            let activePos = 1;
                            
                            driversAtPoint.forEach((driver) => {
                                // Add separator line before first lapped driver
                                if (driver.isLapped && !lappedSeparatorAdded) {
                                    innerHtml += `<div style="border-top:1px solid #444;margin:8px 0 4px 0;"></div>`;
                                    lappedSeparatorAdded = true;
                                }
                                
                                // Add separator line before first DNF
                                if (driver.isDNF && !dnfSeparatorAdded) {
                                    innerHtml += `<div style="border-top:1px solid #444;margin:8px 0 4px 0;"></div>`;
                                    dnfSeparatorAdded = true;
                                }
                                
                                // Position or status label
                                let posText;
                                let posColor;
                                let currentPos = null;
                                if (driver.isDNF) {
                                    posText = 'DNF';
                                    posColor = '#FF6666';
                                } else if (driver.isLapped) {
                                    posText = '-' + driver.lapsBehind + 'L';
                                    posColor = '#FFA500';
                                } else {
                                    currentPos = activePos;
                                    posText = 'P' + activePos;
                                    posColor = '#888';
                                    activePos++;
                                }
                                
                                // Arrow
                                const prevPos = prevPositionMap[driver.name];
                                let arrow = '<span style="min-width:16px;display:inline-block;"></span>';
                                if (currentPos !== null && prevPos !== undefined && prevPos !== currentPos) {
                                    arrow = currentPos < prevPos
                                        ? `<span style="color:#00FF00;min-width:16px;display:inline-block;">▲</span>`
                                        : `<span style="color:#FF4444;min-width:16px;display:inline-block;">▼</span>`;
                                }
                                
                                // Pit icon
                                const pitIcon = driver.isInPit ? '<span style="min-width:18px;display:inline-block;text-align:center;">🔧</span>' : '<span style="min-width:18px;display:inline-block;"></span>';
                                
                                // Pit count
                                const pitText = driver.pitStops > 0 ? ` (${driver.pitStops} ${driver.pitStops > 1 ? t('pits') : t('pit')})` : '';
                                
                                // Gap text
                                let gapText;
                                if (driver.isDNF) {
                                    gapText = '-';
                                } else if (driver.isLapped) {
                                    gapText = driver.lapsBehind === 1 ? '+1 lap' : `+${driver.lapsBehind} laps`;
                                } else if (driver.gap === 0) {
                                    gapText = t('leader');
                                } else {
                                    if (driver.gap >= 60) {
                                        const minutes = Math.floor(driver.gap / 60);
                                        const seconds = driver.gap % 60;
                                        gapText = `+${minutes}:${seconds.toFixed(3).padStart(6, '0')}`;
                                    } else {
                                        gapText = `+${driver.gap.toFixed(3)}s`;
                                    }
                                }
                                
                                const coloredName = driver.nameColored ? parseLFSColors(driver.nameColored) : driver.name;
                                
                                // Current stint driver (for relay races)
                                const stintDriver = (driver.driverData.stints && driver.driverData.stints.length > 1) ? getStintDriverAtLap(driver.driverData, currentLap) : null;
                                const stintHtml = stintDriver ? `<span style="color:#AAA;font-size:10px;"> [${stintDriver.name}]</span>` : '';
                                
                                innerHtml += `<div style="display:flex;gap:6px;align-items:center;margin:4px 0;">`;
                                innerHtml += `<span style="display:inline-block;width:12px;height:12px;background:${driver.color};border-radius:2px;flex-shrink:0;"></span>`;
                                innerHtml += `<span style="color:${posColor};min-width:30px;">${posText}</span>`;
                                innerHtml += arrow;
                                innerHtml += pitIcon;
                                innerHtml += `<span style="flex:1;min-width:150px;">${coloredName}${stintHtml}</span>`;
                                innerHtml += `<span style="color:#CCC;text-align:right;min-width:80px;">${gapText}</span>`;
                                if (pitText) innerHtml += `<span style="color:#CCC;font-size:10px;">${pitText}</span>`;
                                innerHtml += `</div>`;
                            });
                            
                            tooltipEl.innerHTML = innerHtml;
                        }
                        
                        const position = context.chart.canvas.getBoundingClientRect();
                        tooltipEl.style.opacity = 1;
                        const leftPos = position.left + window.pageXOffset + tooltipModel.caretX;
                        const tw = tooltipEl.offsetWidth || 400;
                        if (leftPos - tw / 2 < 10) {
                            tooltipEl.style.transform = 'translate(0, 0)';
                            tooltipEl.style.left = '10px';
                        } else if (leftPos + tw / 2 > window.innerWidth - 10) {
                            tooltipEl.style.transform = 'translate(-100%, 0)';
                            tooltipEl.style.left = (window.innerWidth - 10) + 'px';
                        } else {
                            tooltipEl.style.transform = 'translate(-50%, 0)';
                            tooltipEl.style.left = leftPos + 'px';
                        }
                        // Position tooltip within viewport
                        const th = tooltipEl.offsetHeight || 200;
                        const idealTop = position.top + window.pageYOffset + tooltipModel.caretY;
                        const viewTop = window.pageYOffset;
                        const viewBottom = viewTop + window.innerHeight;
                        let finalTop = idealTop;
                        if (idealTop + th > viewBottom - 10) finalTop = viewBottom - th - 10;
                        if (finalTop < viewTop + 10) finalTop = viewTop + 10;
                        tooltipEl.style.top = finalTop + 'px';
                        tooltipEl.style.maxHeight = (window.innerHeight - 20) + 'px';
                        tooltipEl.style.overflowY = 'auto';
                    }
                },
                zoom: {
                    limits: {
                        y: {min: 0, max: maxGap}, // Allow zoom out to full gap range
                        x: {min: 0, max: maxLaps} // Grid (0) to last lap
                    },
                    zoom: {
                        wheel: {
                            enabled: true,
                        },
                        pinch: {
                            enabled: true
                        },
                        mode: 'y',
                        onZoomStart: function() { document.querySelectorAll('[id^="chartjs-tooltip"]').forEach(el => el.style.opacity = '0'); },
                    },
                    pan: {
                        enabled: true,
                        mode: 'y',
                        modifierKey: null,
                        onPanStart: function() { document.querySelectorAll('[id^="chartjs-tooltip"]').forEach(el => el.style.opacity = '0'); },
                    }
                }
            },
            scales: {
                x: {
                    type: 'linear',
                    min: 0,
                    max: totalTimingPoints - 1,
                    title: {
                        display: true,
                        text: t('lapHeader'),
                        color: '#fff'
                    },
                    ticks: {
                        color: '#888',
                        maxRotation: 0,
                        autoSkip: true,
                        stepSize: 1,
                        callback: function(value, index) {
                            if (value >= 0 && value < xLabels.length) {
                                return xLabels[Math.floor(value)];
                            }
                            return '';
                        }
                    },
                    grid: {
                        color: '#333'
                    }
                },
                y: {
                    reverse: true, // 0 at top, maxGap at bottom
                    title: {
                        display: true,
                        text: t('gapToCurrentLeader'),
                        color: '#fff'
                    },
                    ticks: {
                        color: '#888',
                        callback: function(value) {
                            return formatGapValue(value);
                        }
                    },
                    grid: {
                        color: function(context) {
                            if (context.tick && context.tick.value === 0) {
                                return '#666';
                            }
                            return '#333';
                        },
                        lineWidth: function(context) {
                            if (context.tick && context.tick.value === 0) {
                                return 2;
                            }
                            return 1;
                        }
                    },
                    max: 140, // Start zoomed to 140s
                    min: 0
                }
            }
        }
    });
    
    // Add double-click to reset zoom for Race Progress
    const progressCanvas = document.getElementById('progress-graph');
    if (progressCanvas) {
        progressCanvas.addEventListener('dblclick', () => {
            if (progressChartInstance) {
                progressChartInstance.resetZoom();
            }
        });
    }
}
function renderBestTimes() {
    const bestLaps = [...raceData.cars]
        .filter(d => d.bestLapTime && d.bestLapTime !== 'DNF' && parseLapTime(d.bestLapTime) < 3599)
        .sort((a, b) => parseLapTime(a.bestLapTime) - parseLapTime(b.bestLapTime));
    
    // Check if we have any WR data
    const hasWR = raceData.worldRecords && Object.keys(raceData.worldRecords).length > 0;
    
    // Get WR for best lap driver's car
    const bestDriver = bestLaps[0];
    const wr = hasWR && bestDriver ? raceData.worldRecords[bestDriver.car] : null;
    
    // Helper function to generate all rows
    const generateBestLapRows = (start = 0, end = bestLaps.length) => {
        return bestLaps.slice(start, end).map((d, idx) => {
            const i = start + idx;
            const gap = i === 0 ? '-' : `+${formatTimeDiff(parseLapTime(d.bestLapTime) - parseLapTime(bestLaps[0].bestLapTime))}`;
            const driverWR = raceData.worldRecords ? raceData.worldRecords[d.car] : null;
            const wrGap = driverWR ? `+${formatTimeDiff(parseLapTime(d.bestLapTime) - parseLapTime(driverWR.lapTime))}` : '-';
            return `
                <tr class="${i >= 10 ? 'expandable-row hidden' : ''}">
                    <td class="position pos-${i + 1}">${i + 1}</td>
                    <td>${getDriverLink(d)}</td>
                    <td>${getCarHtml(d.car)}</td>
                    <td>${d.bestLapTime}</td>
                    <td>${d.bestLapNumber != null ? `${t('lapHeader')} ${d.bestLapNumber}` : '-'}</td>
                    <td>${gap}</td>
                    ${hasWR ? `<td style="color: ${driverWR ? '#FFA500' : '#888'};">${wrGap}</td>` : ''}
                </tr>
            `;
        }).join('');
    };
    
    const html = `
        <h2>⚡ ${t('bestTimes')}</h2>
        <div class="section-grid">
            <div class="section-box">
                <h3>⚡ ${t("bestLapTime")}</h3>
                ${wr ? `<p class="wr-info">🌍 WR (${wr.car}): ${wr.lapTime} by ${wr.racer}</p>` : ''}
                <table class="compact-table">
                    <thead>
                        <tr>
                            <th>${t("rank")}</th>
                            <th>${t("driver")}</th>
                            <th>${t('car')}</th>
                            <th>${t("time")}</th>
                            <th>${t('lap')}</th>
                            <th>${t("gap")}</th>
                            ${hasWR ? `<th>${t('vsWorldRecord')}</th>` : ''}
                        </tr>
                    </thead>
                    <tbody id="best-lap-tbody">
                        ${generateBestLapRows()}
                    </tbody>
                </table>
                ${bestLaps.length > 10 ? `<button class="show-more-btn" onclick="toggleExpandTable('best-lap-tbody', this)">${t("seeMore")} (+${bestLaps.length - 10})</button>` : ''}
            </div>
            
            <div class="section-box">
                <h3>🔧 ${t("pitStopsRanking")}</h3>
                <table class="compact-table">
                    <thead>
                        <tr>
                            <th>${t("rank")}</th>
                            <th>${t("driver")}</th>
                            <th>${t("pitStops")}</th>
                            <th>${t("totalTime")}</th>
                            <th>${t("details")}</th>
                        </tr>
                    </thead>
                    <tbody id="pit-stops-tbody">
                        ${buildPitStopsByDriver().map((p, i) => `
                            <tr class="${i >= 10 ? 'expandable-row hidden' : ''}">
                                <td class="position pos-${i + 1}">${i + 1}</td>
                                <td>${getDriverLinkByIndex(p.driverIdx)}</td>
                                <td>${p.count}</td>
                                <td>${formatLapTime(p.totalTime)}</td>
                                <td>${p.count === 1 ? `
                                    ${t('lapHeader')} ${p.stops[0].lap} — ${formatPitStopActions(p.stops[0].reason)}
                                ` : `
                                    <span class="pit-details-toggle" onclick="togglePitDetails(this)" style="cursor:pointer;color:#4fc3f7;float:right;">
                                        <span class="arrow">▶</span> ${t("details")}
                                    </span>
                                    <div class="pit-details" style="display:none;margin-top:6px;">
                                        <div style="margin-bottom:6px;"><span class="position pos-${i + 1}" style="margin-right:6px;">${i + 1}</span> ${getDriverLinkByIndex(p.driverIdx)} — ${p.count} ${t('pitStops').toLowerCase()} — ${formatLapTime(p.totalTime)}</div>
                                        <table class="compact-table" style="margin:0;font-size:0.85em;">
                                            <thead><tr><th>${t("driver")}</th><th>#</th><th>${t("lapHeader")}</th><th>${t("duration")}</th><th>${t("actions")}</th></tr></thead>
                                            <tbody>
                                                ${p.stops.map((s, j) => `
                                                    <tr>
                                                        <td>${getPitStopDriverName(p.driverIdx, s.lap)}</td>
                                                        <td>${j + 1}</td>
                                                        <td>${t('lapHeader')} ${s.lap}</td>
                                                        <td>${formatLapTime(s.duration)}</td>
                                                        <td>${formatPitStopActions(s.reason)}</td>
                                                    </tr>
                                                `).join('')}
                                            </tbody>
                                        </table>
                                    </div>
                                `}</td>
                            </tr>
                        `).join('')}
                    </tbody>
                </table>
                ${buildPitStopsByDriver().length > 10 ? `<button class="show-more-btn" onclick="toggleExpandTable('pit-stops-tbody', this)">${t("seeMore")} (+${buildPitStopsByDriver().length - 10})</button>` : ''}
            </div>
            
            <div class="section-box">
                <h3>🚀 ${t("firstLap")}</h3>
                <table class="compact-table">
                    <thead>
                        <tr>
                            <th>Pos</th>
                            <th>${t("driver")}</th>
                            <th>${t("time")}</th>
                            <th>${t("gap")}</th>
                        </tr>
                    </thead>
                    <tbody id="first-lap-tbody">
                        ${(raceData.rankings.firstLap || []).map((d, i) => `
                            <tr class="${i >= 10 ? 'expandable-row hidden' : ''}">
                                <td class="position pos-${i + 1}">${i + 1}</td>
                                <td>${getDriverLinkByIndex(d.driver)}</td>
                                <td>${d.time}</td>
                                <td>${i === 0 ? '-' : d.gap}</td>
                            </tr>
                        `).join('')}
                    </tbody>
                </table>
                ${(raceData.rankings.firstLap || []).length > 10 ? `<button class="show-more-btn" onclick="toggleExpandTable('first-lap-tbody', this)">${t("seeMore")} (+${(raceData.rankings.firstLap || []).length - 10})</button>` : ''}
            </div>
            
            <div class="section-box">
                <h3>💨 ${t("topSpeed")}</h3>
                <table class="compact-table">
                    <thead>
                        <tr>
                            <th>${t("rank")}</th>
                            <th>${t("driver")}</th>
                            <th>${t("speed")}</th>
                            <th>Lap</th>
                        </tr>
                    </thead>
                    <tbody id="top-speed-tbody">
                        ${(raceData.rankings.topSpeed || []).map((d, i) => `
                            <tr class="${i >= 10 ? 'expandable-row hidden' : ''}">
                                <td class="position pos-${i + 1}">${i + 1}</td>
                                <td>${getDriverLinkByIndex(d.driver)}</td>
                                <td>${d.speed}</td>
                                <td>${t('lapHeader')} ${d.lap}</td>
                            </tr>
                        `).join('')}
                    </tbody>
                </table>
                ${(raceData.rankings.topSpeed || []).length > 10 ? `<button class="show-more-btn" onclick="toggleExpandTable('top-speed-tbody', this)">${t("seeMore")} (+${(raceData.rankings.topSpeed || []).length - 10})</button>` : ''}
            </div>
        </div>
        
        ${renderBestSectors()}
    `;
    
    document.getElementById('best-times').innerHTML = html;
}

// Render driver comparison tool
let compareChart = null;
function renderCompare() {
    const container = document.getElementById('compare');
    if (!container) return;

    const cars = raceData.cars;
    const isQual = raceData.session && raceData.session.type === 'qual';

    // Build driver options sorted by position
    const sorted = isQual
        ? [...cars].sort((a, b) => parseLapTime(a.bestLapTime) - parseLapTime(b.bestLapTime))
        : [...cars];

    // Build driver data for custom dropdowns
    const driverOptions = sorted.map((c, i) => {
        const pos = isQual ? i + 1 : c.position;
        return { value: c.username, pos, name: c.name, nameColored: c.nameColored };
    });

    function buildCustomSelect(id, optional) {
        return `<div class="csel" id="${id}">
            <div class="csel-selected"><span class="csel-text">${optional ? '— ' + (t('optional') || 'Optional') + ' —' : ''}</span><span class="csel-arrow">▾</span></div>
            <div class="csel-options">
                ${optional ? `<div class="csel-option" data-value=""><span style="color:#888;">— ${t('optional') || 'Optional'} —</span></div>` : ''}
                ${driverOptions.map(d => `<div class="csel-option" data-value="${escapeHtml(d.value)}"><span class="csel-pos">P${d.pos}</span> ${parseLFSColors(d.nameColored)}</div>`).join('')}
            </div>
        </div>`;
    }

    container.innerHTML = `
        <h2>🔍 ${t('compare') || 'Compare'}</h2>
        <div class="compare-selectors">
            ${buildCustomSelect('compare-driver-1', false)}
            <span class="compare-vs">VS</span>
            ${buildCustomSelect('compare-driver-2', false)}
            ${buildCustomSelect('compare-driver-3', true)}
            ${buildCustomSelect('compare-driver-4', true)}
            ${buildCustomSelect('compare-driver-5', true)}
        </div>
        <div id="compare-results"></div>
    `;

    // Custom select logic
    function initCustomSelect(id, defaultValue) {
        const el = document.getElementById(id);
        const selected = el.querySelector('.csel-selected');
        const optionsDiv = el.querySelector('.csel-options');
        const textEl = selected.querySelector('.csel-text');
        el._value = defaultValue || '';

        // Set initial display
        if (defaultValue) {
            const opt = optionsDiv.querySelector(`[data-value="${CSS.escape(defaultValue)}"]`);
            if (opt) textEl.innerHTML = opt.innerHTML;
        }

        selected.addEventListener('click', (e) => {
            e.stopPropagation();
            // Close all other open dropdowns
            document.querySelectorAll('.csel-options.open').forEach(o => {
                if (o !== optionsDiv) o.classList.remove('open');
            });
            optionsDiv.classList.toggle('open');
        });

        optionsDiv.querySelectorAll('.csel-option').forEach(opt => {
            opt.addEventListener('click', (e) => {
                e.stopPropagation();
                el._value = opt.dataset.value;
                textEl.innerHTML = opt.innerHTML;
                optionsDiv.classList.remove('open');
                runComparison();
            });
        });
    }

    // Close dropdowns when clicking outside
    document.addEventListener('click', () => {
        document.querySelectorAll('.csel-options.open').forEach(o => o.classList.remove('open'));
    });

    // Pre-select P1 and P2
    const defaultP1 = sorted.length > 0 ? sorted[0].username : '';
    const defaultP2 = sorted.length > 1 ? sorted[1].username : '';
    initCustomSelect('compare-driver-1', defaultP1);
    initCustomSelect('compare-driver-2', defaultP2);
    initCustomSelect('compare-driver-3', '');
    initCustomSelect('compare-driver-4', '');
    initCustomSelect('compare-driver-5', '');
    // Run initial comparison
    runComparison();
}

function runComparison() {
    const container = document.getElementById('compare-results');
    const usernames = [];
    for (let i = 1; i <= 5; i++) {
        const el = document.getElementById('compare-driver-' + i);
        const val = el ? el._value : '';
        if (val) usernames.push(val);
    }

    // Remove duplicates
    const unique = [...new Set(usernames)];
    if (unique.length < 2) {
        container.innerHTML = `<p style="color:#888; text-align:center;">${t('selectTwoDrivers') || 'Select at least 2 different drivers'}</p>`;
        return;
    }

    const drivers = unique.map(u => raceData.cars.find(c => c.username === u)).filter(Boolean);
    if (drivers.length < 2) return;

    const isQual = raceData.session && raceData.session.type === 'qual';
    const compareColors = ['#FF4444', '#4488FF', '#44DD44', '#FFA500', '#CC44FF'];

    // Compute stats for each driver
    const stats = drivers.map((d, idx) => {
        const validTimes = d.lapTimes
            .map(t => parseLapTime(t))
            .filter(t => t > 0 && t < 3599);
        const avg = validTimes.length > 0 ? validTimes.reduce((a, b) => a + b, 0) / validTimes.length : 0;
        const best = parseLapTime(d.bestLapTime);
        const stdDev = validTimes.length > 1
            ? Math.sqrt(validTimes.reduce((sum, t) => sum + Math.pow(t - avg, 2), 0) / validTimes.length)
            : 0;
        const consistency = avg > 0 ? (100 - (stdDev / avg) * 100) : 0;

        return {
            driver: d,
            color: compareColors[idx],
            validTimes,
            avg,
            best,
            stdDev,
            consistency,
            laps: d.lapsCompleted,
            position: d.position,
            gridPosition: d.gridPosition,
            topSpeed: d.topSpeed || 0,
            contacts: d.incidents?.contacts || 0,
            yellowFlags: d.incidents?.yellowFlags || 0,
            blueFlags: d.incidents?.blueFlags || 0,
            pitStops: d.pitStops?.length || 0,
            totalPitTime: (d.pitStops || []).reduce((sum, p) => sum + parseFloat((p.duration || '0').replace(',', '.')), 0),
            pitLaps: new Set((d.pitStops || []).map(p => p.lap)),
            bestSplits: d.bestSplits || []
        };
    });

    // Find global best for highlighting
    const globalBestLap = Math.min(...stats.map(s => s.best).filter(t => t > 0));
    const globalBestAvg = Math.min(...stats.map(s => s.avg).filter(t => t > 0));
    const globalBestConsistency = Math.max(...stats.map(s => s.consistency));
    const globalTopSpeed = Math.max(...stats.map(s => s.topSpeed));
    const globalLeastContacts = Math.min(...stats.map(s => s.contacts));

    const highlight = (val, best, lower) => {
        if (lower) return Math.abs(val - best) < 0.001 ? 'compare-best' : '';
        return Math.abs(val - best) < 0.001 ? 'compare-best' : '';
    };

    // Stats table
    let html = `<div class="compare-table-wrap"><table class="compare-table">
        <thead><tr>
            <th></th>
            ${stats.map(s => `<th style="border-bottom: 3px solid ${s.color};">${getDriverLink(s.driver)}</th>`).join('')}
        </tr></thead>
        <tbody>`;

    // Position
    if (!isQual) {
        html += `<tr><td class="compare-label">${t('pos')}</td>
            ${stats.map(s => `<td class="position pos-${s.position}">${s.position}</td>`).join('')}</tr>`;
        html += `<tr><td class="compare-label">${t('grid')}</td>
            ${stats.map(s => `<td>${s.gridPosition}</td>`).join('')}</tr>`;
    }

    // Laps
    html += `<tr><td class="compare-label">${t('laps')}</td>
        ${stats.map(s => `<td>${s.laps}</td>`).join('')}</tr>`;

    // Best lap
    html += `<tr><td class="compare-label">${t('bestLap')}</td>
        ${stats.map(s => {
            const isBest = s.best > 0 && Math.abs(s.best - globalBestLap) < 0.001;
            return `<td class="${isBest ? 'compare-best' : ''}">${s.best > 0 && s.best < 3599 ? formatLapTime(s.best) : '-'}</td>`;
        }).join('')}</tr>`;

    // Average lap (race only)
    if (!isQual) html += `<tr><td class="compare-label">${t('avgLap') || 'Avg Lap'}</td>
        ${stats.map(s => {
            const isBest = s.avg > 0 && Math.abs(s.avg - globalBestAvg) < 0.001;
            return `<td class="${isBest ? 'compare-best' : ''}">${s.avg > 0 ? formatLapTime(s.avg) : '-'}</td>`;
        }).join('')}</tr>`;

    // Consistency (race only)
    if (!isQual) html += `<tr><td class="compare-label">${t('consistency') || 'Consistency'}</td>
        ${stats.map(s => {
            const isBest = Math.abs(s.consistency - globalBestConsistency) < 0.01;
            return `<td class="${isBest ? 'compare-best' : ''}">${s.consistency > 0 ? s.consistency.toFixed(1) + '%' : '-'}</td>`;
        }).join('')}</tr>`;

    // Std dev (race only)
    if (!isQual) html += `<tr><td class="compare-label">${t('stdDev') || 'Std Dev'}</td>
        ${stats.map(s => `<td>${s.stdDev > 0 ? s.stdDev.toFixed(3) + 's' : '-'}</td>`).join('')}</tr>`;

    // Top speed (qualifying or when data available)
    if (isQual && stats.some(s => s.topSpeed > 10)) {
        const globalTopSpeed = Math.max(...stats.map(s => s.topSpeed));
        html += `<tr><td class="compare-label">${t('topSpeed')}</td>
            ${stats.map(s => {
                const isBest = Math.abs(s.topSpeed - globalTopSpeed) < 0.01;
                return `<td class="${isBest ? 'compare-best' : ''}">${s.topSpeed > 10 ? s.topSpeed.toFixed(1) + ' km/h' : '-'}</td>`;
            }).join('')}</tr>`;
    }

    // Best splits
    const maxSplits = Math.max(...stats.map(s => s.bestSplits.length));
    for (let i = 0; i < maxSplits; i++) {
        const splitTimes = stats.map(s => s.bestSplits[i] ? parseLapTime(s.bestSplits[i]) : Infinity);
        const bestSplit = Math.min(...splitTimes.filter(t => t < 3599));
        html += `<tr><td class="compare-label">${t('bestSector') || 'Best Sector'} ${i + 1}</td>
            ${stats.map((s, idx) => {
                const t = splitTimes[idx];
                const isBest = t < 3599 && Math.abs(t - bestSplit) < 0.001;
                return `<td class="${isBest ? 'compare-best' : ''}">${t < 3599 ? s.bestSplits[i] : '-'}</td>`;
            }).join('')}</tr>`;
    }

    // Pit stops + total pit time
    if (stats.some(s => s.pitStops > 0)) {
        html += `<tr><td class="compare-label">${t('pitStops')}</td>
            ${stats.map(s => `<td>${s.pitStops}</td>`).join('')}</tr>`;
        const minPitTime = Math.min(...stats.filter(s => s.totalPitTime > 0).map(s => s.totalPitTime));
        html += `<tr><td class="compare-label">${t('pitTime') || 'Pit Time'}</td>
            ${stats.map(s => {
                if (s.totalPitTime <= 0) return '<td>-</td>';
                const isBest = Math.abs(s.totalPitTime - minPitTime) < 0.01;
                return `<td class="${isBest ? 'compare-best' : ''}">${formatLapTime(s.totalPitTime)}</td>`;
            }).join('')}</tr>`;
    }

    // Incidents (race only)
    if (!isQual) html += `<tr><td class="compare-label">${t('incidents')}</td>
        ${stats.map(s => {
            const parts = [];
            if (s.contacts > 0) parts.push(`<span title="${t('contacts')}">💥 ${s.contacts}</span>`);
            if (s.yellowFlags > 0) parts.push(`<span title="${t('yellowFlags')}">⚠️ ${s.yellowFlags}</span>`);
            if (s.blueFlags > 0) parts.push(`<span title="${t('blueFlags')}">🔵 ${s.blueFlags}</span>`);
            return `<td>${parts.length > 0 ? parts.join(' ') : '-'}</td>`;
        }).join('')}</tr>`;

    html += `</tbody></table></div>`;

    // Lap times chart
    if (!isQual) {
        html += `<div class="compare-chart-wrap"><canvas id="compare-chart"></canvas></div>`;
    }

    container.innerHTML = html;

    if (!isQual) renderCompareChart(stats);
}

function renderCompareChart(stats) {
    const canvas = document.getElementById('compare-chart');
    if (!canvas) return;

    if (compareChart) {
        compareChart.destroy();
        compareChart = null;
    }

    // Find max laps among compared drivers
    const maxLaps = Math.max(...stats.map(s => s.driver.lapTimes.length));

    const datasets = stats.map(s => {
        const data = s.driver.lapTimes.map(t => {
            const p = parseLapTime(t);
            return (p > 0 && p < 3599) ? p : null;
        });
        return {
            label: s.driver.name,
            data: data,
            borderColor: s.color,
            backgroundColor: s.color + '33',
            borderWidth: 2,
            pointRadius: 3,
            pointHoverRadius: 5,
            tension: 0.1,
            spanGaps: false
        };
    });

    const labels = Array.from({ length: maxLaps }, (_, i) => `${t('lapLabel') || 'L'}${i + 1}`);

    // Build pit stop markers per driver
    const pitMarkers = [];
    stats.forEach(s => {
        s.pitLaps.forEach(lap => {
            if (lap >= 0 && lap < s.driver.lapTimes.length) {
                const lapTime = parseLapTime(s.driver.lapTimes[lap]);
                if (lapTime > 0 && lapTime < 3599) {
                    const pit = s.driver.pitStops.find(p => p.lap === lap);
                    pitMarkers.push({
                        lapIndex: lap,
                        y: lapTime,
                        color: s.color,
                        driver: s.driver.name,
                        duration: pit ? pit.duration : ''
                    });
                }
            }
        });
    });

    // Plugin to draw pit markers
    const pitMarkerPlugin = {
        id: 'comparePitMarkers',
        afterDatasetsDraw(chart) {
            const { ctx } = chart;
            const xScale = chart.scales.x;
            const yScale = chart.scales.y;
            pitMarkers.forEach(m => {
                const xPixel = xScale.getPixelForValue(m.lapIndex);
                const yPixel = yScale.getPixelForValue(m.y);
                ctx.save();
                ctx.fillStyle = '#FFFFFF';
                ctx.strokeStyle = m.color || '#888';
                ctx.lineWidth = 2;
                ctx.beginPath();
                ctx.arc(xPixel, yPixel, 8, 0, Math.PI * 2);
                ctx.fill();
                ctx.stroke();
                ctx.fillStyle = '#000000';
                ctx.font = 'bold 11px Arial';
                ctx.textAlign = 'center';
                ctx.textBaseline = 'middle';
                ctx.fillText('P', xPixel, yPixel);
                ctx.restore();
            });
        }
    };

    // Calculate average lap time excluding first lap and pit stop laps
    const cleanTimes = stats.flatMap(s => {
        return s.driver.lapTimes.slice(1).map((t, i) => {
            const lapIdx = i + 1;
            if (s.pitLaps.has(lapIdx)) return null;
            const p = parseLapTime(t);
            return (p > 0 && p < 3599) ? p : null;
        }).filter(v => v !== null);
    });
    const globalAvg = cleanTimes.length > 0 ? cleanTimes.reduce((a, b) => a + b, 0) / cleanTimes.length : 0;
    // Per-driver averages to detect spread
    const driverAvgs = stats.map(s => {
        const dt = s.driver.lapTimes.slice(1).map((t, i) => {
            if (s.pitLaps.has(i + 1)) return null;
            const p = parseLapTime(t);
            return (p > 0 && p < 3599) ? p : null;
        }).filter(v => v !== null);
        return dt.length > 0 ? dt.reduce((a, b) => a + b, 0) / dt.length : globalAvg;
    });
    const spread = Math.max(...driverAvgs) - Math.min(...driverAvgs);
    const margin = spread > 3 ? 6 : 2.5;
    const yMin = globalAvg > margin ? globalAvg - margin : 0;
    const yMax = globalAvg + margin;

    compareChart = new Chart(canvas, {
        type: 'line',
        data: { labels, datasets },
        plugins: [pitMarkerPlugin],
        options: {
            responsive: true,
            maintainAspectRatio: false,
            animation: false,
            interaction: {
                mode: 'index',
                intersect: false
            },
            plugins: {
                legend: {
                    labels: { color: '#ccc', usePointStyle: true }
                },
                tooltip: {
                    callbacks: {
                        label: (ctx) => {
                            if (ctx.parsed.y == null) return null;
                            const pit = pitMarkers.find(m => m.lapIndex === ctx.dataIndex && m.driver === ctx.dataset.label);
                            const pitStr = pit ? ` 🔧 ${pit.duration}s` : '';
                            return `${ctx.dataset.label}: ${formatLapTime(ctx.parsed.y)}${pitStr}`;
                        }
                    }
                },
                zoom: {
                    pan: {
                        enabled: true,
                        mode: 'y'
                    },
                    zoom: {
                        wheel: { enabled: true },
                        pinch: { enabled: true },
                        mode: 'y'
                    }
                }
            },
            scales: {
                x: {
                    ticks: { color: '#999' },
                    grid: { color: 'rgba(255,255,255,0.05)' }
                },
                y: {
                    min: yMin,
                    max: yMax,
                    ticks: {
                        color: '#999',
                        callback: (v) => formatLapTime(v)
                    },
                    grid: { color: 'rgba(255,255,255,0.08)' }
                }
            }
        }
    });

    // Double-click to reset zoom
    canvas.addEventListener('dblclick', () => {
        compareChart.resetZoom();
    });
}

// Render incidents
function renderIncidents() {
    const yellowFlags = [...raceData.cars]
        .filter(d => d.incidents.yellowFlags > 0)
        .sort((a, b) => b.incidents.yellowFlags - a.incidents.yellowFlags);
    
    const blueFlags = [...raceData.cars]
        .filter(d => d.incidents.blueFlags > 0)
        .sort((a, b) => b.incidents.blueFlags - a.incidents.blueFlags);
    
    const contacts = [...raceData.cars]
        .filter(d => d.incidents.contacts > 0)
        .sort((a, b) => b.incidents.contacts - a.incidents.contacts);
    
    const penalties = raceData.cars.filter(d => d.penalties && d.penalties.length > 0);
    
    const html = `
        <h2>⚠️ ${t('incidents')}</h2>
        <div class="section-grid">
            <div class="section-box">
                <h3>⚠️ ${t("yellowFlags")}</h3>
                <table class="compact-table">
                    <thead>
                        <tr>
                            <th>${t("rank")}</th>
                            <th>${t("driver")}</th>
                            <th>${t("count")}</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${yellowFlags.map((d, i) => `
                            <tr>
                                <td>${i + 1}</td>
                                <td>${getDriverLink(d)}</td>
                                <td class="text-warning">${d.incidents.yellowFlags}</td>
                            </tr>
                        `).join('') || '<tr><td colspan="3" class="empty-state">No yellow flags</td></tr>'}
                    </tbody>
                </table>
            </div>
            
            <div class="section-box">
                <h3>🔵 ${t("blueFlags")}</h3>
                <table class="compact-table">
                    <thead>
                        <tr>
                            <th>${t("rank")}</th>
                            <th>${t("driver")}</th>
                            <th>${t("count")}</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${blueFlags.map((d, i) => `
                            <tr>
                                <td>${i + 1}</td>
                                <td>${getDriverLink(d)}</td>
                                <td class="text-info">${d.incidents.blueFlags}</td>
                            </tr>
                        `).join('') || '<tr><td colspan="3" class="empty-state">No blue flags</td></tr>'}
                    </tbody>
                </table>
            </div>
            
            <div class="section-box">
                <h3>💥 ${t("contacts")}</h3>
                <table class="compact-table">
                    <thead>
                        <tr>
                            <th>${t("rank")}</th>
                            <th>${t("driver")}</th>
                            <th>${t("count")}</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${contacts.map((d, i) => `
                            <tr>
                                <td>${i + 1}</td>
                                <td>${getDriverLink(d)}</td>
                                <td class="text-error">${d.incidents.contacts}</td>
                            </tr>
                        `).join('') || '<tr><td colspan="3" class="empty-state">No contacts</td></tr>'}
                    </tbody>
                </table>
            </div>
            
            <div class="section-box">
                <h3>⚠️ ${t("penalties")}</h3>
                ${penalties.length > 0 ? `
                    <table class="compact-table">
                        <thead>
                            <tr>
                                <th>${t("driver")}</th>
                                <th>${t("penalties")}</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${penalties.map(d => `
                                <tr>
                                    <td>${getDriverLink(d)}</td>
                                    <td>${d.penalties.length} penalty(ies)</td>
                                </tr>
                            `).join('')}
                        </tbody>
                    </table>
                ` : '<p class="text-success text-center p-20">✓ No penalties issued</p>'}
            </div>
        </div>
    `;
    
    document.getElementById('incidents').innerHTML = html;
}

// Render analysis
function renderAnalysis() {
    // Calculate average lap and consistency for each driver
    const analysisData = raceData.cars.map(d => {
        if (!d.lapTimes || d.lapTimes.length === 0) {
            return null;
        }
        
        // Parse lap times
        const times = d.lapTimes.map(t => parseLapTime(t)).filter(t => t !== Infinity && t < 3599);
        if (times.length === 0) return null;
        
        // Calculate average
        const avg = times.reduce((sum, t) => sum + t, 0) / times.length;
        
        // Calculate standard deviation (consistency)
        const variance = times.reduce((sum, t) => sum + Math.pow(t - avg, 2), 0) / times.length;
        const stdDev = Math.sqrt(variance);
        
        // Calculate coefficient of variation (lower is more consistent)
        const cv = (stdDev / avg) * 100;
        
        return {
            driver: d,
            avgTime: avg,
            avgTimeStr: formatLapTime(avg),
            stdDev: stdDev,
            cv: cv,
            lapsCount: times.length
        };
    }).filter(a => a !== null);
    
    // Sort by average time
    const byAverage = [...analysisData].sort((a, b) => a.avgTime - b.avgTime);
    
    // Sort by consistency (lower CV is better)
    const byConsistency = [...analysisData].sort((a, b) => a.cv - b.cv);
    
    const html = `
        <h2>📉 ${t('analysis')}</h2>
        <div class="section-grid">
            <div class="section-box">
                <h3>📊 ${t('averageLap')}</h3>
                <table class="compact-table">
                    <thead>
                        <tr>
                            <th>${t("rank")}</th>
                            <th>${t("driver")}</th>
                            <th>${t('avgTime')}</th>
                            <th>${t('laps')}</th>
                            <th>${t('gapToBest')}</th>
                        </tr>
                    </thead>
                    <tbody id="avg-lap-tbody">
                        ${byAverage.map((a, i) => {
                            const gap = i === 0 ? '-' : `+${formatTimeDiff(a.avgTime - byAverage[0].avgTime)}`;
                            return `
                                <tr class="${i >= 10 ? 'expandable-row hidden' : ''}">
                                    <td class="position pos-${i + 1}">${i + 1}</td>
                                    <td>${getDriverLink(a.driver)}</td>
                                    <td>${a.avgTimeStr}</td>
                                    <td>${a.lapsCount}</td>
                                    <td>${gap}</td>
                                </tr>
                            `;
                        }).join('')}
                    </tbody>
                </table>
                ${byAverage.length > 10 ? `<button class="show-more-btn" onclick="toggleExpandTable('avg-lap-tbody', this)">${t("seeMore")} (+${byAverage.length - 10})</button>` : ''}
            </div>
            
            <div class="section-box">
                <h3>📉 ${t('lapConsistency')}</h3>
                <p class="text-small text-muted mb-15">${t('lowerIsBetter')}</p>
                <table class="compact-table">
                    <thead>
                        <tr>
                            <th>${t("rank")}</th>
                            <th>${t("driver")}</th>
                            <th>${t('consistency')}</th>
                            <th>${t('stdDev')}</th>
                            <th>${t('avgTime')}</th>
                        </tr>
                    </thead>
                    <tbody id="consistency-tbody">
                        ${byConsistency.map((a, i) => {
                            const color = a.cv < 1.0 ? '#4CAF50' : (a.cv < 2.0 ? '#FFA500' : '#F44336');
                            return `
                                <tr class="${i >= 10 ? 'expandable-row hidden' : ''}">
                                    <td class="position pos-${i + 1}">${i + 1}</td>
                                    <td>${getDriverLink(a.driver)}</td>
                                    <td style="color: ${color}; font-weight: bold;">${a.cv.toFixed(2)}%</td>
                                    <td>±${a.stdDev.toFixed(3)}s</td>
                                    <td>${a.avgTimeStr}</td>
                                </tr>
                            `;
                        }).join('')}
                    </tbody>
                </table>
                ${byConsistency.length > 10 ? `<button class="show-more-btn" onclick="toggleExpandTable('consistency-tbody', this)">${t("seeMore")} (+${byConsistency.length - 10})</button>` : ''}
            </div>
        </div>
        
        <div class="section-grid" style="margin-top: 20px;">
            <div class="section-box">
                <h3>🎯 ${t('bestTheoreticalLap')} (${t('bestSectorsCombined')})</h3>
                ${renderBestTheoreticalLap()}
            </div>
            
            <div class="section-box">
                <h3>📈 ${t('lapTimeDistribution')}</h3>
                ${renderLapTimeDistribution()}
            </div>
        </div>
    `;
    
    document.getElementById('analysis').innerHTML = html;
}

// Helper to render best theoretical lap
function renderBestTheoreticalLap() {
    if (!raceData.session.splitsPerLap || raceData.session.splitsPerLap === 0) {
        return `<p class="text-muted">${t('noSectorData')}</p>`;
    }
    
    const driversWithSplits = raceData.cars.filter(d => d.bestSplits && d.bestSplits.length > 0);
    if (driversWithSplits.length === 0) {
        return `<p class="text-muted">${t('noSectorTimes')}</p>`;
    }
    
    const hasWR = raceData.worldRecords && Object.keys(raceData.worldRecords).length > 0;
    
    const expectedSectors = (raceData.session.splitsPerLap || 0) + 1;
    // Use actual bestSplits length if less than expected (e.g. qualifying may not have finish sector)
    const firstWithSplits = driversWithSplits.find(d => d.bestSplits && d.bestSplits.length > 0);
    const numSectors = firstWithSplits ? Math.min(expectedSectors, firstWithSplits.bestSplits.length) : expectedSectors;
    
    const theoretical = driversWithSplits.map(d => {
        if (!d.bestSplits || d.bestSplits.length === 0) return null;
        
        // Take available sectors
        const relevantSplits = d.bestSplits.slice(0, numSectors);
        const parsedSplits = relevantSplits.map(s => parseLapTime(s));
        
        // Ensure driver has valid times for ALL required sectors
        if (parsedSplits.length < numSectors) return null;
        if (parsedSplits.some(t => !t || t <= 0.001 || t === Infinity)) return null;
        
        // Sum all best splits to get theoretical best lap
        const totalTime = parsedSplits.reduce((sum, t) => sum + t, 0);
        
        // Get WR for this driver's car
        const driverWR = raceData.worldRecords ? raceData.worldRecords[d.car] : null;
        const wrTime = driverWR ? parseLapTime(driverWR.lapTime) : null;
        
        return {
            driver: d,
            theoreticalTime: totalTime,
            bestActual: parseLapTime(d.bestLapTime),
            splits: relevantSplits,
            improvement: parseLapTime(d.bestLapTime) - totalTime,
            vsWR: wrTime ? totalTime - wrTime : null
        };
    }).filter(t => t !== null).sort((a, b) => a.theoreticalTime - b.theoreticalTime);
    
    if (theoretical.length === 0) {
        return '<p class="text-muted">Unable to calculate theoretical times</p>';
    }
    
    // Get WR for best theoretical driver's car
    const bestDriver = theoretical[0].driver;
    const wr = hasWR && bestDriver ? raceData.worldRecords[bestDriver.car] : null;
    
    return `
        ${wr ? `<p class="wr-info">🌍 WR (${wr.car}): ${wr.lapTime} by ${wr.racer}</p>` : ''}
        <table class="compact-table">
            <thead>
                <tr>
                    <th>${t("rank")}</th>
                    <th>${t("driver")}</th>
                    <th>${t('car')}</th>
                    <th>${t('bestActual')}</th>
                    <th>${t('potential')}</th>
                    <th>${t('theoretical')}</th>
                    ${hasWR ? `<th>${t('vsWorldRecord')}</th>` : ''}
                </tr>
            </thead>
            <tbody id="theoretical-tbody">
                ${theoretical.map((t, i) => {
                    const potentialColor = t.improvement > 0.001 ? '#4CAF50' : '#888';
                    const wrGapColor = t.vsWR !== null ? (t.vsWR < 0 ? '#FFD700' : '#FFA500') : '';
                    const wrGapStr = t.vsWR !== null ? (t.vsWR < 0 ? `-${formatTimeDiff(-t.vsWR)}` : `+${formatTimeDiff(t.vsWR)}`) : '-';
                    return `
                        <tr class="${i >= 10 ? 'expandable-row hidden' : ''}">
                            <td class="position pos-${i + 1}">${i + 1}</td>
                            <td>${getDriverLink(t.driver)}</td>
                            <td>${getCarHtml(t.driver.car)}</td>
                            <td>${t.driver.bestLapTime}</td>
                            <td style="color: ${potentialColor};">${t.improvement > 0.001 ? '-' + formatTimeDiff(t.improvement) : '0.000'}</td>
                            <td style="color: ${t.improvement > 0.001 ? '#00BFFF' : '#888'}; font-weight: ${t.improvement > 0.001 ? 'bold' : 'normal'};">${formatLapTime(t.theoreticalTime)}</td>
                            ${hasWR ? `<td style="color: ${wrGapColor};">${wrGapStr}</td>` : ''}
                        </tr>
                    `;
                }).join('')}
            </tbody>
        </table>
        ${theoretical.length > 10 ? `<button class="show-more-btn" onclick="toggleExpandTable('theoretical-tbody', this)">${t("seeMore")} (+${theoretical.length - 10})</button>` : ''}
    `;
}

// Helper to format lap time from seconds
function formatLapTime(seconds) {
    const min = Math.floor(seconds / 60);
    const sec = (seconds % 60).toFixed(3);
    return min > 0 ? `${min}:${sec.padStart(6, '0')}` : sec;
}

// Helper to render lap time distribution
function renderLapTimeDistribution() {
    // Collect all lap times from all drivers, excluding:
    // 1. First lap (lap index 0)
    // 2. Laps with pit stops
    const allLapTimes = [];
    let totalLaps = 0;
    let firstLapsDiscarded = 0;
    let pitStopLapsDiscarded = 0;
    
    raceData.cars.forEach(driver => {
        if (!driver.lapTimes || driver.lapTimes.length === 0) return;
        
        // Build set of pit stop lap numbers for this driver
        const pitStopLaps = new Set();
        if (driver.pitStops && driver.pitStops.length > 0) {
            driver.pitStops.forEach(ps => {
                if (ps.lap !== undefined && ps.lap !== null) {
                    pitStopLaps.add(ps.lap);
                }
            });
        }
        
        driver.lapTimes.forEach((lapTime, lapIndex) => {
            totalLaps++;
            const lapNumber = lapIndex + 1; // Lap numbers are 1-based
            const time = parseLapTime(lapTime);
            
            if (time === Infinity) return;
            
            // Skip first lap
            if (lapIndex === 0) {
                firstLapsDiscarded++;
                return;
            }
            
            // Skip laps with pit stops
            if (pitStopLaps.has(lapNumber)) {
                pitStopLapsDiscarded++;
                return;
            }
            
            allLapTimes.push(time);
        });
    });
    
    if (allLapTimes.length === 0) {
        return '<p class="text-muted">No valid lap time data after filtering</p>';
    }
    
    // Sort times
    allLapTimes.sort((a, b) => a - b);
    
    // Filter outliers: discard laps > 120% of fastest lap
    const fastest = allLapTimes[0];
    const threshold = fastest * 1.2;
    const preFilterCount = allLapTimes.length;
    const filteredLaps = allLapTimes.filter(time => time <= threshold);
    const outlierLapsDiscarded = preFilterCount - filteredLaps.length;
    const totalDiscarded = firstLapsDiscarded + pitStopLapsDiscarded + outlierLapsDiscarded;
    
    if (filteredLaps.length === 0) {
        return '<p class="text-muted">No valid lap time data after filtering</p>';
    }
    
    // Calculate statistics on filtered data
    const min = filteredLaps[0];
    const max = filteredLaps[filteredLaps.length - 1];
    const median = filteredLaps[Math.floor(filteredLaps.length / 2)];
    const q1 = filteredLaps[Math.floor(filteredLaps.length * 0.25)];
    const q3 = filteredLaps[Math.floor(filteredLaps.length * 0.75)];
    const range = max - min;
    
    // Create percentage-based buckets relative to fastest lap
    // Ranges: 100-100.5%, 100.5-101.75%, 101.75-103%, 103-105.25%, 105.25-107.5%, 107.5-109%, 109-112%, 112-115%, 115-120%
    const percentages = [100.0, 100.5, 101.75, 103.0, 105.25, 107.5, 109.0, 112.0, 115.0, 120.0];
    const bucketRanges = [];
    const buckets = [];
    
    for (let i = 0; i < percentages.length - 1; i++) {
        bucketRanges.push({
            min: fastest * (percentages[i] / 100),
            max: fastest * (percentages[i + 1] / 100),
            minPct: percentages[i],
            maxPct: percentages[i + 1]
        });
        buckets.push(0);
    }
    
    // Count laps in each bucket
    filteredLaps.forEach(time => {
        for (let i = 0; i < bucketRanges.length; i++) {
            if (time >= bucketRanges[i].min && time < bucketRanges[i].max) {
                buckets[i]++;
                break;
            }
            // Last bucket includes upper boundary
            if (i === bucketRanges.length - 1 && time >= bucketRanges[i].min && time <= bucketRanges[i].max) {
                buckets[i]++;
                break;
            }
        }
    });
    
    const maxBucketCount = Math.max(...buckets);
    
    return `
        <div style="margin-bottom: 20px;">
            <table class="compact-table" style="margin-bottom: 15px;">
                <tbody>
                    <tr>
                        <td><strong>${t('validLaps')}:</strong></td>
                        <td>${filteredLaps.length}</td>
                        <td><strong>${t('median')}:</strong></td>
                        <td>${formatLapTime(median)}</td>
                    </tr>
                    <tr>
                        <td><strong>${t('totalLaps')}:</strong></td>
                        <td>${totalLaps}</td>
                        <td><strong>${t('discarded')}:</strong></td>
                        <td style="color: #888;">${totalDiscarded}</td>
                    </tr>
                    ${totalDiscarded > 0 ? `
                    <tr>
                        <td colspan="4" style="font-size: 12px; color: #888; padding-left: 20px;">
                            ↳ ${t('firstLaps')}: ${firstLapsDiscarded} | ${t('pitStopsLabel')}: ${pitStopLapsDiscarded} | ${t('outliers')} (&gt;${formatLapTime(threshold)}): ${outlierLapsDiscarded}
                        </td>
                    </tr>` : ''}
                    <tr>
                        <td><strong>${t('fastest')}:</strong></td>
                        <td style="color: #4CAF50;">${formatLapTime(min)}</td>
                        <td><strong>${t('slowest')}:</strong></td>
                        <td style="color: #F44336;">${formatLapTime(max)}</td>
                    </tr>
                    <tr>
                        <td><strong>${t('q1')}:</strong></td>
                        <td>${formatLapTime(q1)}</td>
                        <td><strong>${t('q3')}:</strong></td>
                        <td>${formatLapTime(q3)}</td>
                    </tr>
                    <tr>
                        <td><strong>${t('range')}:</strong></td>
                        <td colspan="3">${formatTimeDiff(range)}</td>
                    </tr>
                </tbody>
            </table>
            
            <div style="background: #2a2a2a; padding: 15px; border-radius: 5px;">
                <h4 style="margin: 0 0 10px 0; color: #FFD700;">${t('distributionHistogram')}</h4>
                <p style="margin: 0 0 15px 0; font-size: 11px; color: #888;">${t('percentageRanges')} (${formatLapTime(fastest)})</p>
                ${buckets.map((count, i) => {
                    const range = bucketRanges[i];
                    const barWidth = maxBucketCount > 0 ? (count / maxBucketCount) * 100 : 0;
                    const minTime = formatLapTime(range.min);
                    const maxTime = formatLapTime(range.max);
                    return `
                        <div style="margin-bottom: 10px;">
                            <div style="display: flex; align-items: center; gap: 10px;">
                                <div style="width: 140px;">
                                    <div style="font-size: 11px; color: #fff; line-height: 1.2;">
                                        ${minTime.substring(0, 7)} - ${maxTime.substring(0, 7)}
                                    </div>
                                    <div style="font-size: 9px; color: #666; line-height: 1;">
                                        ${range.minPct.toFixed(1)}% - ${range.maxPct.toFixed(1)}%
                                    </div>
                                </div>
                                <div style="flex: 1; background: #1a1a1a; height: 24px; border-radius: 3px; position: relative;">
                                    <div style="background: linear-gradient(90deg, #667eea, #764ba2); height: 100%; width: ${barWidth}%; border-radius: 3px; transition: width 0.3s;"></div>
                                </div>
                                <div style="width: 40px; text-align: right; font-size: 12px; color: #fff;">
                                    ${count}
                                </div>
                            </div>
                        </div>
                    `;
                }).join('')}
            </div>
        </div>
    `;
}

// Render best sectors comparison
function renderBestSectors() {
    if (!raceData.session.splitsPerLap || raceData.session.splitsPerLap === 0) {
        return '';
    }
    
    const driversWithSplits = raceData.cars.filter(d => d.bestSplits && d.bestSplits.length > 0);
    if (driversWithSplits.length === 0) {
        return '';
    }
    
    const hasWR = raceData.worldRecords && Object.keys(raceData.worldRecords).length > 0;
    const expectedSectors = (raceData.session.splitsPerLap || 0) + 1;
    const firstWithSplits = driversWithSplits.find(d => d.bestSplits && d.bestSplits.length > 0);
    const numSectors = firstWithSplits ? Math.min(expectedSectors, firstWithSplits.bestSplits.length) : expectedSectors;
    const sectorTables = [];
    
    for (let sector = 0; sector < numSectors; sector++) {
        const sectorData = driversWithSplits.map(d => ({
            driver: d,
            time: d.bestSplits[sector] ? parseLapTime(d.bestSplits[sector]) : Infinity,
            timeStr: d.bestSplits[sector] || 'N/A'
        })).filter(s => s.time > 0.001 && s.time < 3599 && s.time !== Infinity).sort((a, b) => a.time - b.time);
        
        if (sectorData.length === 0) continue;
        
        // Get WR for best sector driver's car
        const bestDriver = sectorData[0].driver;
        const wr = hasWR && bestDriver ? raceData.worldRecords[bestDriver.car] : null;
        const wrSector = wr && wr.sectors && wr.sectors[sector] ? parseLapTime(wr.sectors[sector]) : null;
        
        const tbodyId = `sector-${sector}-tbody`;
        sectorTables.push(`
            <div class="section-box">
                <h3>⚡ ${t("bestSector")} ${sector + 1}</h3>
                ${wrSector ? `<p class="wr-info">🌍 WR (${wr.car}): ${wr.sectors[sector]}</p>` : ''}
                <table class="compact-table">
                    <thead>
                        <tr>
                            <th>${t("rank")}</th>
                            <th>${t("driver")}</th>
                            <th>${t('car')}</th>
                            <th>${t("time")}</th>
                            <th>${t("gap")}</th>
                            ${hasWR ? `<th>${t('vsWorldRecord')}</th>` : ''}
                        </tr>
                    </thead>
                    <tbody id="${tbodyId}">
                        ${sectorData.map((s, i) => {
                            const gap = i === 0 ? '-' : `+${formatTimeDiff(s.time - sectorData[0].time)}`;
                            const driverWR = raceData.worldRecords ? raceData.worldRecords[s.driver.car] : null;
                            const driverWRSector = driverWR && driverWR.sectors && driverWR.sectors[sector] ? parseLapTime(driverWR.sectors[sector]) : null;
                            const wrGap = driverWRSector ? `+${formatTimeDiff(s.time - driverWRSector)}` : '-';
                            return `
                                <tr class="${i >= 10 ? 'expandable-row hidden' : ''}">
                                    <td class="position pos-${i + 1}">${i + 1}</td>
                                    <td>${getDriverLink(s.driver)}</td>
                                    <td>${getCarHtml(s.driver.car)}</td>
                                    <td>${s.timeStr}</td>
                                    <td>${gap}</td>
                                    ${hasWR ? `<td style="color: ${driverWRSector ? '#FFA500' : '#888'};">${wrGap}</td>` : ''}
                                </tr>
                            `;
                        }).join('')}
                    </tbody>
                </table>
                ${sectorData.length > 10 ? `<button class="show-more-btn" onclick="toggleExpandTable('${tbodyId}', this)">${t("seeMore")} (+${sectorData.length - 10})</button>` : ''}
            </div>
        `);
    }
    
    if (sectorTables.length === 0) return '';
    
    return `
        <h2 style="margin-top: 30px;">📊 ${t('bestSectorTimes')}</h2>
        <div class="section-grid">
            ${sectorTables.join('')}
        </div>
    `;
}

// Calculate biggest climber (original version)
function calculateBiggestClimber() {
    return [...raceData.cars]
        .map(d => ({
            ...d,
            gain: d.gridPosition - d.position
        }))
        .filter(d => d.gain > 0)
        .sort((a, b) => b.gain - a.gain)
        .slice(0, 10);
}

// Calculate combativity (overtakes) from InSim-detected events
function calculateCombativity() {
    const overtakes = raceData.events.overtakes || [];
    const drivers = raceData.cars || [];
    
    return [...drivers]
        .map(d => {
            // Find player index for this car's driver
            const pIdx = raceData.players.findIndex(p => p.username === d.username);
            
            // Count overtakes made by this driver
            const overtakesMade = overtakes.filter(o => o.overtaker === pIdx).length;
            
            // Count times this driver was overtaken
            const overtakesReceived = overtakes.filter(o => o.overtaken === pIdx).length;
            
            return {
                ...d,
                overtakesMade: overtakesMade,
                overtakesReceived: overtakesReceived,
                total: overtakesMade - overtakesReceived
            };
        })
        .sort((a, b) => {
            // Sort by total (descending)
            if (b.total !== a.total) {
                return b.total - a.total;
            }
            // Then by overtakes made (descending)
            if (b.overtakesMade !== a.overtakesMade) {
                return b.overtakesMade - a.overtakesMade;
            }
            // Finally by overtakes received (ascending - fewer is better)
            return a.overtakesReceived - b.overtakesReceived;
        });
}

// Calculate laps led
function calculateLapsLed() {
    const race = raceData.session;
    const lapsLed = {};
    
    // For each car, count how many position entries are P1
    const totalLaps = race.laps;
    
    raceData.cars.forEach(driver => {
        const posArray = driver.positions || [];
        const leaderLaps = posArray.filter(p => p === 1).length;
        if (leaderLaps > 0) {
                lapsLed[driver.username] = {
                    name: driver.name,
                    username: driver.username,
                    laps: leaderLaps,
                    percentage: (leaderLaps / posArray.length) * 100
                };
        }
    });
    
    return Object.values(lapsLed)
        .sort((a, b) => b.laps - a.laps);
}

// Tab switching
function setupTabs() {
    document.querySelectorAll('.tab').forEach(tab => {
        tab.addEventListener('click', () => {
            const targetId = tab.dataset.tab;
            
            // Update tabs
            document.querySelectorAll('.tab').forEach(t => t.classList.remove('active'));
            tab.classList.add('active');
            
            // Update content
            document.querySelectorAll('.content').forEach(c => c.classList.remove('active'));
            document.getElementById(targetId).classList.add('active');
            
            // Redraw graphs if switching to graph tabs
            if (targetId === 'graph' && raceData) {
                setTimeout(() => renderGraph(), 100);
            }
            if (targetId === 'progress' && raceData) {
                setTimeout(() => renderProgressGraph(), 100);
            }
        });
    });
}

function setupProgressLegend() {
    // Setup clickable legend items for Race Progress graph
    const legendItems = document.querySelectorAll('#progress-legend .legend-item');
    
    legendItems.forEach((item, index) => {
        item.style.cursor = 'pointer';
        
        item.addEventListener('click', () => {
            // Toggle visibility state
            if (index === 0) { // Pit stops
                progressMarkerVisibility.pitStops = !progressMarkerVisibility.pitStops;
            } else if (index === 1) { // DNF
                progressMarkerVisibility.dnf = !progressMarkerVisibility.dnf;
            }
            
            // Update visual state
            const isHidden = 
                (index === 0 && !progressMarkerVisibility.pitStops) ||
                (index === 1 && !progressMarkerVisibility.dnf);
            
            if (isHidden) {
                item.style.opacity = '0.4';
                item.style.textDecoration = 'line-through';
            } else {
                item.style.opacity = '1';
                item.style.textDecoration = 'none';
            }
            
            // Redraw graph
            if (progressChartInstance) {
                progressChartInstance.update();
            }
        });
    });
}

function updateToggleButtonText(chartInstance, toggleElement) {
    if (!chartInstance || !toggleElement) return;
    
    const allVisible = chartInstance.data.datasets.every((dataset, index) => 
        chartInstance.isDatasetVisible(index)
    );
    
    // Find the text span inside the toggle element
    const textSpan = toggleElement.querySelector('.legend-text');
    if (textSpan) {
        textSpan.textContent = allVisible ? t('hideAll') : t('showAll');
    }
}

function setupGraphControls() {
    // Setup intelligent toggle button for Position Graph
    const positionToggle = document.getElementById('position-toggle-all');
    
    if (positionToggle) {
        positionToggle.addEventListener('click', () => {
            if (!positionChartInstance) return;
            
            // Check if all datasets are visible
            const allVisible = positionChartInstance.data.datasets.every((dataset, index) => 
                positionChartInstance.isDatasetVisible(index)
            );
            
            // If all visible → hide all, otherwise → show all
            const newState = !allVisible;
            
            positionChartInstance.data.datasets.forEach((dataset, index) => {
                positionChartInstance.setDatasetVisibility(index, newState);
                // Also update the legend item state
                const meta = positionChartInstance.getDatasetMeta(index);
                meta.hidden = !newState;
            });
            
            positionChartInstance.update();
            updateToggleButtonText(positionChartInstance, positionToggle);
        });
        
        // Listen to legend clicks to update button text
        const positionCanvas = document.getElementById('position-graph');
        if (positionCanvas) {
            positionCanvas.addEventListener('click', () => {
                setTimeout(() => updateToggleButtonText(positionChartInstance, positionToggle), 100);
            });
        }
    }
    
    // Setup intelligent toggle button for Race Progress
    const progressToggle = document.getElementById('progress-toggle-all');
    
    if (progressToggle) {
        progressToggle.addEventListener('click', () => {
            if (!progressChartInstance) return;
            
            // Check if all datasets are visible
            const allVisible = progressChartInstance.data.datasets.every((dataset, index) => 
                progressChartInstance.isDatasetVisible(index)
            );
            
            // If all visible → hide all, otherwise → show all
            const newState = !allVisible;
            
            progressChartInstance.data.datasets.forEach((dataset, index) => {
                progressChartInstance.setDatasetVisibility(index, newState);
                // Also update the legend item state
                const meta = progressChartInstance.getDatasetMeta(index);
                meta.hidden = !newState;
            });
            
            progressChartInstance.update();
            updateToggleButtonText(progressChartInstance, progressToggle);
        });
        
        // Listen to legend clicks to update button text
        const progressCanvas = document.getElementById('progress-graph');
        if (progressCanvas) {
            progressCanvas.addEventListener('click', () => {
                setTimeout(() => updateToggleButtonText(progressChartInstance, progressToggle), 100);
            });
        }
    }
}

function setupGraphLegend() {
    // Setup clickable legend items for Position Graph
    const legendItems = document.querySelectorAll('#graph .graph-legend .legend-item');
    
    legendItems.forEach((item, index) => {
        item.style.cursor = 'pointer';
        
        item.addEventListener('click', () => {
            // Toggle visibility state
            if (index === 0) { // Pit stops
                markerVisibility.pitStops = !markerVisibility.pitStops;
            } else if (index === 1) { // Personal best
                markerVisibility.personalBest = !markerVisibility.personalBest;
            } else if (index === 2) { // Fastest lap
                markerVisibility.fastestLap = !markerVisibility.fastestLap;
            } else if (index === 3) { // DNF
                markerVisibility.dnf = !markerVisibility.dnf;
            } else if (index === 4) { // Lapped
                markerVisibility.lapped = !markerVisibility.lapped;
            }
            
            // Update visual state
            const isHidden = 
                (index === 0 && !markerVisibility.pitStops) ||
                (index === 1 && !markerVisibility.personalBest) ||
                (index === 2 && !markerVisibility.fastestLap) ||
                (index === 3 && !markerVisibility.dnf) ||
                (index === 4 && !markerVisibility.lapped);
            
            if (isHidden) {
                item.style.opacity = '0.4';
                item.style.textDecoration = 'line-through';
            } else {
                item.style.opacity = '1';
                item.style.textDecoration = 'none';
            }
            
            // Redraw graph
            if (positionChartInstance) {
                positionChartInstance.update('none'); // Update without animation
            }
        });
    });
}

// Get the current driver name at a given lap for cars with relays/stints
function getStintDriverAtLap(driverData, lap) {
    if (!driverData || !driverData.stints || driverData.stints.length === 0) return null;
    for (let i = driverData.stints.length - 1; i >= 0; i--) {
        const stint = driverData.stints[i];
        if (lap >= stint.fromLap && lap <= stint.toLap) {
            return stint;
        }
    }
    // Fallback to last stint if lap is beyond range
    return driverData.stints[driverData.stints.length - 1];
}

// Track coordinates for solar calculations (future day/night detection)
const TRACK_LOCATIONS = {
    'BL': { lat: 50.80, lng: -1.80, tz: 'Europe/London' },
    'SO': { lat: 51.44, lng: -1.50, tz: 'Europe/London' },
    'FE': { lat: 17.87, lng: -77.30, tz: 'America/Jamaica' },
    'AU': { lat: 52.42, lng: -2.30, tz: 'Europe/London' },
    'KY': { lat: 34.53, lng: 135.50, tz: 'Asia/Tokyo' },
    'WE': { lat: 53.42, lng: -2.90, tz: 'Europe/London' },
    'AS': { lat: 55.26, lng: -2.60, tz: 'Europe/London' },
    'RO': { lat: 52.88, lng: -1.10, tz: 'Europe/London' },
    'LA': { lat: 51.61, lng: -1.90, tz: 'Europe/London' }
};

function getTrackLocation(trackCode) {
    if (!trackCode || trackCode.length < 2) return null;
    return TRACK_LOCATIONS[trackCode.substring(0, 2).toUpperCase()] || null;
}

// Utility functions
function getCarHtml(carName) {
    if (!carName) return 'N/A';
    let imageUrl = null;
    let linkUrl;
    if (carName.length <= 3) {
        imageUrl = `https://www.lfs.net/static/showroom/cars160/${carName}.png`;
        linkUrl = `https://www.lfs.net/cars/${carName}`;
    } else {
        linkUrl = `https://www.lfs.net/files/vehmods/${carName}`;
        if (raceData.session.carImages && raceData.session.carImages[carName])
            imageUrl = raceData.session.carImages[carName];
    }
    if (imageUrl) {
        return `<a href="${linkUrl}" target="_blank" class="link-primary car-link-with-image" title="${carName}">
            <img src="${imageUrl}" alt="${carName}" class="car-thumbnail" onerror="this.nextElementSibling.style.display='inline';this.style.display='none'">
            <span style="display:none">${carName}</span></a>`;
    }
    return `<a href="${linkUrl}" target="_blank" class="link-primary">${carName}</a>`;
}

function getDriverLink(driver) {
    // If car has multiple drivers (stints/relays), show all
    if (driver.stints && driver.stints.length > 0) {
        const uniqueDrivers = [];
        const seen = new Set();
        for (const stint of driver.stints) {
            if (!seen.has(stint.username)) {
                seen.add(stint.username);
                const name = stint.nameColored ? parseLFSColors(stint.nameColored) : escapeHtml(stint.name);
                uniqueDrivers.push(`<a href="https://www.lfsworld.net/?win=stats&racer=${encodeURIComponent(stint.username)}" target="_blank" class="link-driver">${name}</a>`);
            }
        }
        return uniqueDrivers.join(' <span class="relay-separator">/</span> ');
    }
    const displayName = driver.nameColored ? parseLFSColors(driver.nameColored) : escapeHtml(driver.name);
    return `<a href="https://www.lfsworld.net/?win=stats&racer=${encodeURIComponent(driver.username)}" target="_blank" class="link-driver">${displayName}</a>`;
}

function getDriverLinkByName(driverName) {
    // Find driver by name in raceData
    const driver = raceData.cars.find(d => d.name === driverName);
    if (driver) {
        return getDriverLink(driver);
    }
    return `<strong>${escapeHtml(driverName)}</strong>`;
}

function getDriverLinkByIndex(playerIdx) {
    // Resolve player index → find car → render link
    const p = raceData.getPlayer(playerIdx);
    const car = raceData.cars.find(d => d.username === p.username);
    if (car) return getDriverLink(car);
    const coloredName = p.nameColored ? parseLFSColors(p.nameColored) : escapeHtml(p.name);
    return `<a href="https://www.lfsworld.net/?win=stats&racer=${encodeURIComponent(p.username)}" target="_blank" class="link-driver">${coloredName}</a>`;
}

function getDriverLinkFromLapLed(lapLedEntry) {
    // LapLed entry has name and username - find full driver for colors
    const driver = raceData.cars.find(d => d.username === lapLedEntry.username);
    if (driver) {
        return getDriverLink(driver);
    }
    return `<a href="https://www.lfsworld.net/?win=stats&racer=${encodeURIComponent(lapLedEntry.username)}" target="_blank" class="link-driver">${escapeHtml(lapLedEntry.name)}</a>`;
}

function parseLapTime(timeStr) {
    if (!timeStr || timeStr === 'DNF' || timeStr === '-') return 0;
    const parts = timeStr.split(':');
    if (parts.length === 3) {
        // h:mm:ss.ms format (e.g. "1:00:00.000" = 3600s)
        return parseInt(parts[0]) * 3600 + parseInt(parts[1]) * 60 + parseFloat(parts[2]);
    }
    if (parts.length === 2) {
        const [min, sec] = parts;
        return parseInt(min) * 60 + parseFloat(sec);
    }
    return parseFloat(timeStr);
}

function formatGapValue(seconds) {
    if (seconds >= 3600) {
        const h = Math.floor(seconds / 3600);
        const m = Math.floor((seconds % 3600) / 60);
        const s = Math.floor(seconds % 60);
        return `+${h}:${String(m).padStart(2,'0')}:${String(s).padStart(2,'0')}`;
    }
    if (seconds >= 60) {
        const m = Math.floor(seconds / 60);
        const s = Math.floor(seconds % 60);
        return `+${m}:${String(s).padStart(2,'0')}`;
    }
    return '+' + seconds.toFixed(0) + 's';
}

function formatTimeDiff(seconds) {
    const min = Math.floor(seconds / 60);
    const sec = (seconds % 60).toFixed(3);
    return min > 0 ? `${min}:${sec.padStart(6, '0')}` : sec;
}

function getPitStopDriverName(carDriverIdx, lap) {
    const car = raceData.cars.find(c => c.lastDriver === carDriverIdx);
    if (!car || !car.stints || car.stints.length <= 1) return getSingleDriverLink(carDriverIdx);
    for (let i = car.stints.length - 1; i >= 0; i--) {
        if (lap >= car.stints[i].fromLap) return getSingleDriverLink(car.stints[i].driver);
    }
    return getSingleDriverLink(carDriverIdx);
}

function getSingleDriverLink(playerIdx) {
    const p = raceData.getPlayer(playerIdx);
    if (!p) return '?';
    const coloredName = p.nameColored ? parseLFSColors(p.nameColored) : escapeHtml(p.name);
    return `<a href="https://www.lfsworld.net/?win=stats&racer=${encodeURIComponent(p.username)}" target="_blank" class="link-driver">${coloredName}</a>`;
}

function togglePitDetails(el) {
    const details = el.nextElementSibling;
    const isHidden = details.style.display === 'none';
    details.style.display = isHidden ? 'block' : 'none';
    el.querySelector('.arrow').classList.toggle('open');
    const tr = el.closest('tr');
    const detailsTd = el.closest('td');
    const otherCells = Array.from(tr.children).filter(td => td !== detailsTd);
    if (isHidden) {
        otherCells.forEach(td => td.style.display = 'none');
        detailsTd.colSpan = otherCells.length + 1;
    } else {
        otherCells.forEach(td => td.style.display = '');
        detailsTd.colSpan = 1;
    }
}

function buildPitStopsByDriver() {
    const driverPits = {};
    raceData.cars.forEach(car => {
        if (!car.pitStops || car.pitStops.length === 0) return;
        const driverIdx = car.lastDriver;
        const stops = car.pitStops.map(p => {
            const durSec = parseFloat((p.duration || '0').replace(',', '.'));
            return { lap: p.lap, duration: durSec, reason: p.reason || [] };
        });
        const totalTime = stops.reduce((sum, s) => sum + s.duration, 0);
        driverPits[driverIdx] = { driverIdx, count: stops.length, totalTime, stops };
    });
    return Object.values(driverPits).sort((a, b) => a.totalTime - b.totalTime);
}

function formatPitStopActions(reasons) {
    if (!reasons || reasons.length === 0) {
        return '<span class="text-muted">-</span>';
    }
    
    const actionMap = {
        'refuel': '⛽ ' + t('pitRefuel'),
        'tires': '<img src="data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCAyNCAyNCIgd2lkdGg9IjE2IiBoZWlnaHQ9IjE2Ij4KICA8Y2lyY2xlIGN4PSIxMiIgY3k9IjEyIiByPSIxMCIgZmlsbD0iIzMzMyIgc3Ryb2tlPSIjNjY2IiBzdHJva2Utd2lkdGg9IjIiLz4KICA8Y2lyY2xlIGN4PSIxMiIgY3k9IjEyIiByPSI1IiBmaWxsPSIjNTU1IiBzdHJva2U9IiM4ODgiIHN0cm9rZS13aWR0aD0iMS41Ii8+CiAgPGNpcmNsZSBjeD0iMTIiIGN5PSIxMiIgcj0iMiIgZmlsbD0iIzk5OSIvPgogIDxsaW5lIHgxPSIxMiIgeTE9IjIiIHgyPSIxMiIgeTI9IjciIHN0cm9rZT0iIzc3NyIgc3Ryb2tlLXdpZHRoPSIxLjUiLz4KICA8bGluZSB4MT0iMTIiIHkxPSIxNyIgeDI9IjEyIiB5Mj0iMjIiIHN0cm9rZT0iIzc3NyIgc3Ryb2tlLXdpZHRoPSIxLjUiLz4KICA8bGluZSB4MT0iMiIgeTE9IjEyIiB4Mj0iNyIgeTI9IjEyIiBzdHJva2U9IiM3NzciIHN0cm9rZS13aWR0aD0iMS41Ii8+CiAgPGxpbmUgeDE9IjE3IiB5MT0iMTIiIHgyPSIyMiIgeTI9IjEyIiBzdHJva2U9IiM3NzciIHN0cm9rZS13aWR0aD0iMS41Ii8+Cjwvc3ZnPg==" alt="🛞" style="vertical-align:middle;margin-right:2px;"> ' + t('pitTires'),
        'damage': '💥 ' + t('pitDamage'),
        'setup': '🔧 ' + t('pitSetup')
    };
    
    const formatted = reasons
        .filter(r => r !== 'stop') // Skip generic 'stop'
        .map(r => actionMap[r] || r)
        .join(', ');
    
    return formatted || `<span class="text-muted">${t('pitSimple')}</span>`;
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function showError(message) {
    document.querySelector('.header').innerHTML = `
        <div class="error">${message}</div>
    `;
}

// Toggle expand/collapse table rows
function toggleExpandTable(tbodyId, button) {
    const tbody = document.getElementById(tbodyId);
    const expandableRows = tbody.querySelectorAll('.expandable-row');
    const isExpanded = !expandableRows[0].classList.contains('hidden');
    
    expandableRows.forEach(row => {
        if (isExpanded) {
            row.classList.add('hidden');
        } else {
            row.classList.remove('hidden');
        }
    });
    
    // Update button text
    if (isExpanded) {
        const hiddenCount = expandableRows.length;
        button.textContent = `${t("seeMore")} (+${hiddenCount})`;
    } else {
        button.textContent = t('seeLess');
    }
}

// Render chat messages
function renderChat() {
    const chatMessages = raceData.chat || [];
    const chatTab = document.getElementById('chat-tab');
    const chatDiv = document.getElementById('chat');
    
    // Show tab only if there are messages
    if (chatMessages.length > 0) {
        chatTab.style.display = 'block';
    } else {
        chatTab.style.display = 'none';
        return;
    }
    
    const messageCount = chatMessages.length;
    const messageWord = messageCount === 1 ? t('message') : t('messages');
    const recordedWord = currentLang === 'es' 
        ? (messageCount === 1 ? t('recorded') : t('recordedPlural'))
        : t('recorded');
    
    let html = `
        <h2>💬 ${t('sessionChat')}</h2>
        <div class="section-box">
            <p style="color: #888; margin-bottom: 20px;">
                ${messageCount} ${messageWord} ${recordedWord}
            </p>
            <div class="chat-container">
    `;
    
    chatMessages.forEach((msg, index) => {
        html += `
            <div class="chat-message">
                <span class="chat-number">${index + 1}</span>
                <span class="chat-driver">${msg.driver >= 0 ? parseLFSColors(raceData.getPlayer(msg.driver).nameColored) : '?'}:</span>
                <span class="chat-text">${msg.message}</span>
            </div>
        `;
    });
    
    html += `
            </div>
        </div>
    `;
    
    chatDiv.innerHTML = html;
}
