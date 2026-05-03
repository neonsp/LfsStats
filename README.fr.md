# LFS Stats v3.2.1

**Générateur de statistiques et visualiseur interactif pour [Live for Speed](https://www.lfs.net/).**

## Nouveautés (depuis v3.2.0)

- **Correction de la visibilité des DNF** : Les pilotes qui démarrent sur la grille mais se déconnectent avant de compléter le premier split apparaissent maintenant correctement dans les résultats comme DNF
- Amélioration du suivi des positions de grille pour les déconnexions précoces
- Meilleure préservation des statistiques des pilotes quittant la course

### Mises à jour précédentes (v3.2.0)

- Amélioration du tri des résultats : pilotes avec même nombre de tours maintenant triés par temps de complétion
- Correction des marqueurs dans le graphique de comparaison
- Amélioration de la précision des temps après reconnexions

LFS Stats se connecte à un serveur Live for Speed (ou replay) via InSim, capture les données de course en temps réel et les exporte en JSON. Le visualiseur web inclus génère des graphiques interactifs, des tableaux et des analyses à partir des données exportées — aucun traitement côté serveur requis.

![License](https://img.shields.io/badge/license-GPL--3.0-blue)

🇬🇧 [Read in English](README.md) · 🇪🇸 [Leer en español](README.es.md) · 🇵🇹 [Ler em português](README.pt.md)

## Table des Matières

- [Comment ça Fonctionne](#comment-ça-fonctionne)
- [Démarrage Rapide](#démarrage-rapide)
- [Fonctionnalités du Visualiseur](#fonctionnalités-du-visualiseur)
  - [Onglets](#onglets)
  - [Carte Résumé](#carte-résumé)
  - [Graphiques Interactifs](#graphiques-interactifs)
  - [Comparateur de Pilotes](#comparateur-de-pilotes)
  - [Types de Session](#types-de-session)
  - [Thème Sombre / Clair](#thème-sombre--clair)
  - [Internationalisation](#internationalisation)
- [Déploiement du Visualiseur](#déploiement-du-visualiseur)
  - [Serveur Web](#déploiement-sur-serveur-web)
  - [Utilisation Locale](#utilisation-locale-sans-serveur)
- [Référence de Configuration](#référence-de-configuration)
  - [LFSStats.cfg](#lfsstatscfg)
  - [Options en Ligne de Commande](#options-en-ligne-de-commande)
- [- [Personnaliser le JSON Exporté](#personnaliser-le-json-exporté)
)
- [Structure du Projet](#structure-du-projet)
- [Dépendances](#dépendances)
- [Crédits](#crédits)
- [Licence](#licence)

## Comment ça Fonctionne

```
┌──────────┐    InSim     ┌───────────┐    JSON     ┌─────────────────┐
│  LFS     │ ◄──────────► │ LFS Stats │ ──────────► │  Visualiseur    │
│  Server  │   TCP/UDP    │  (C#)     │   Export    │  (HTML+JS)      │
└──────────┘              └───────────┘             └─────────────────┘
```

1. **LFS Stats** se connecte à un serveur Live for Speed via le protocole InSim
2. Il capture tous les événements : temps au tour, secteurs, arrêts aux stands, dépassements, messages de chat, pénalités, etc.
3. Lorsque la session se termine, il exporte tout sous forme de **fichier JSON**
4. Le **Visualiseur de Stats** transforme le JSON en un tableau de bord interactif

## Démarrage Rapide

### 1. Configurer LFS

Ouvrez le port InSim dans LFS :
```
/insim 29999
```

### 2. Configurer LFS Stats

Éditez `LFSStats.cfg` :
```ini
host = 127.0.0.1       # IP du serveur LFS
port = 29999            # Port InSim (doit correspondre à la commande /insim)
adminPassword =         # Mot de passe admin du serveur (si nécessaire)
TCPmode = true          # TCP recommandé
raceDir = results       # Répertoire de sortie pour les fichiers JSON
```

### 3. Exécuter

```bash
LFSStats.exe
```

Le menu interactif de la console permet de :
- **F** — Avance rapide du replay
- **L** — Basculer la préservation des tours en ESC pit (par défaut : ON)
- **Q** — Quitter en toute sécurité

### 4. Voir les Résultats

Ouvrez le visualiseur avec votre fichier JSON :
```
stats_viewer.html?json=race.json
```

Ou ouvrez `stats_viewer.html` directement et glissez-déposez votre fichier JSON.

Un fichier JSON d'exemple est inclus dans `viewer/examples/endurance.json` (course d'endurance de 5 heures, 10 voitures mod, 195 tours).

### Démo en Direct

- [Session de Qualification](https://www.cesav.es/mkportal/liga/stats/stats_viewer.html?json=resultados/qual.json) — 32 pilotes, Aston
- [Session de Course](https://www.cesav.es/mkportal/liga/stats/stats_viewer.html?json=resultados/race.json) — Course complète avec dépassements, arrêts aux stands, incidents
- [Course d'Endurance](https://www.cesav.es/mkportal/liga/stats/stats_viewer.html?json=resultados/endurance.json) — 5 heures d'endurance avec relais de pilotes



## Fonctionnalités du Visualiseur

### Onglets

| Onglet | Description |
|--------|-------------|
| 🏁 **Résultats** | Classement final avec couleurs de podium, positions de grille, meilleurs tours, arrêts aux stands, incidents |
| 📋 **Aperçu** | Ordre de grille, meilleures remontées, tours menés, statistiques de combativité |
| 🔄 **Stints Pilotes** | Informations relais/stints pour les courses d'endurance (masqué automatiquement si non applicable) |
| 📊 **Tour par Tour** | Tableau transposé des temps au tour avec cellules colorées |
| 📈 **Graphique de Positions** | Graphique interactif Chart.js avec marqueurs d'arrêt aux stands, DNF, tour le plus rapide et meilleur personnel |
| ⏱️ **Progression de Course** | Graphique d'écart au leader montrant la dynamique de la course |
| ⚡ **Meilleurs Temps** | Meilleur tour, meilleur tour théorique, meilleurs secteurs, vitesses de pointe |
| 🔍 **Comparer** | Comparaison côte à côte de pilotes (jusqu'à 5) avec graphique de temps au tour |
| 📉 **Analyse** | Distribution des temps au tour, métriques de régularité, filtrage des valeurs aberrantes |
| ⚠️ **Incidents** | Drapeaux jaunes, drapeaux bleus, contacts par pilote |
| 💬 **Chat** | Messages de chat du jeu capturés pendant la session |

### Carte Résumé

Une barre infographique entre l'en-tête et les onglets montrant les statistiques clés en un coup d'œil :
- 🏆 Vainqueur / Pole Position
- ⚡ Tour le Plus Rapide (surligné en violet, style F1)
- 💨 Vitesse de Pointe
- 👥 Pilotes / 🚫 DNFs
- 🔄 Total Tours / ⚔️ Dépassements / 🔧 Arrêts aux Stands / 💥 Contacts

Chaque carte est cliquable et navigue vers l'onglet correspondant.

### Graphiques Interactifs

Tous les graphiques incluent :
- **Zoom et Défilement** — Molette de la souris pour zoomer, glisser pour se déplacer
- **Double-clic** — Réinitialiser le zoom
- **Infobulles au survol** — Classement complet à chaque point de chronométrage avec changements de position, écarts, arrêts aux stands
- **Légende interactive** — Cliquez sur les noms des pilotes pour afficher/masquer, boutons « Tout Afficher / Tout Masquer »
- **Marqueurs d'événements** — Arrêts aux stands (🔧), DNF (✕), tour le plus rapide (★), meilleur personnel (●)

### Comparateur de Pilotes

Sélectionnez de 2 à 5 pilotes à comparer :
- Tableau de statistiques : position, grille, tours, meilleur tour, secteurs, arrêts aux stands, incidents
- Graphique de temps au tour avec marqueurs d'arrêt aux stands
- Zoom intelligent de l'axe Y centré sur la moyenne des temps
- En qualification, masque les statistiques non pertinentes (tour moyen, régularité, graphique)

### Types de Session

| Type de Session | Fonctionnalités |
|---|---|
| **Course** | Analyse complète : positions, écarts, dépassements, stints, combativité |
| **Qualification** | Graphique de position temporel, évolution du meilleur tour, zone de limite de temps |
| **Essais** | Analyse basique des temps au tour |

### Thème Sombre / Clair

Basculez entre le thème sombre et clair avec le bouton 🌙/☀️ (coin supérieur droit). La préférence est sauvegardée dans `localStorage` et appliquée instantanément — y compris tous les graphiques Chart.js. Les codes couleur LFS (`^0`–`^9`) sont rendus avec des ajustements de contraste par thème.

### Internationalisation

Le visualiseur détecte automatiquement la langue du navigateur avec 16 langues supportées :

🇬🇧 Anglais · 🇪🇸 Espagnol · 🇫🇷 Français · 🇵🇹 Portugais · 🇩🇪 Allemand · 🇮🇹 Italien · 🇵🇱 Polonais · 🇷🇺 Russe · 🇹🇷 Turc · 🇫🇮 Finnois · 🇸🇪 Suédois · 🇱🇹 Lituanien · 🇯🇵 Japonais · 🇨🇳 Chinois · 🇳🇱 Néerlandais · 🇩🇰 Danois

## Déploiement du Visualiseur

### Déploiement sur Serveur Web

Téléchargez les fichiers du visualiseur sur votre serveur web :
```
/votre-site/lfsstats/
  ├── stats_viewer.html
  ├── stats_renderer.js
  ├── stats.css
  ├── translations.js
  └── race.json          ← vos fichiers JSON exportés
```

Accédez via URL avec le paramètre `json` :
```
https://votre-site.com/lfsstats/stats_viewer.html?json=race.json
https://votre-site.com/lfsstats/stats_viewer.html?json=qualification.json
https://votre-site.com/lfsstats/stats_viewer.html?json=resultats/manche1.json
```

Vous pouvez organiser les fichiers JSON en sous-répertoires :
```
/lfsstats/
  ├── stats_viewer.html
  ├── saison2025/
  │   ├── manche1.json
  │   ├── manche2.json
  │   └── manche3_qualif.json
  └── endurance/
      └── course_24h.json
```
```
stats_viewer.html?json=saison2025/manche1.json
stats_viewer.html?json=endurance/course_24h.json
```

### Utilisation Locale (Sans Serveur)

Ouvrez `stats_viewer.html` directement dans votre navigateur. Une zone de glisser-déposer apparaîtra — déposez votre fichier JSON pour le charger.

## Référence de Configuration

### LFSStats.cfg

| Option | Par Défaut | Description |
|--------|------------|-------------|
| `host` | `127.0.0.1` | Adresse IP ou nom du serveur LFS |
| `port` | `29999` | Port InSim (configuré dans LFS avec `/insim <port>`) |
| `adminPassword` | *(vide)* | Mot de passe administrateur du serveur |
| `TCPmode` | `true` | Utiliser TCP (`true`) ou UDP (`false`) pour la connexion InSim |
| `isLocal` | `true` | `true` pour serveur local, `false` pour hôte distant |
| `pracDir` | `results` | Répertoire de sortie pour les statistiques d'essais |
| `qualDir` | `results` | Répertoire de sortie pour les statistiques de qualification |
| `raceDir` | `results` | Répertoire de sortie pour les statistiques de course |
| `exportOnRaceSTart` | `yes` | Exporter lors du redémarrage de session : `yes`, `no`, ou `ask` |
| `askForFileNameOnRST` | `false` | Demander le nom de fichier lors de l'export |
| `exportOnSTAte` | `no` | Exporter lors du changement d'état (interruption) : `yes`, `no`, ou `ask` |
| `askForFileNameOnSTA` | `false` | Demander le nom de fichier lors du changement d'état |
| `preserveLapsOnPit` | `true` | Conserver les données de tours lorsqu'un pilote fait ESC-pit et revient |
| `defaultLogoUrl` | *(vide)* | URL du logo par défaut écrit dans `metadata.logoUrl` pour chaque export JSON |
| `pubStatIDkey` | *(vide)* | Clé API PubStat de LFS World pour les records du monde |

### Options en Ligne de Commande

```
LFSStats.exe [options]

  -c, --config <fichier>  Fichier de configuration (par défaut : LFSStats.cfg)
  -i, --interval <ms>     Intervalle de rafraîchissement InSim : 1-1000 ms (par défaut : 100)
  -v, --verbose <niveau>  Niveau de détail : 0-4 (par défaut : 1)
      --version           Afficher les informations de version
  -h, --help              Afficher cette information
```

**Niveaux de détail :**
- `0` — Programme (erreurs uniquement)
- `1` — Session (début/fin de session, résultats)
- `2` — Tour (tours complétés)
- `3` — Secteur (temps de secteur)
- `4` — Info (tous les événements, connexions, debug)

### Personnaliser le JSON Exporté

La section `metadata` est placée en haut du fichier JSON pour faciliter l'édition :

```json
{
  "metadata": {
    "exportedAt": "2025-07-11T18:30:00Z",
    "mprUrl": "https://votre-site.com/replays/course.mpr",
    "logoUrl": "https://votre-site.com/images/logo-ligue.png"
  },
  ...
}
```

- **mprUrl** — Lien vers le fichier replay MPR à télécharger. Affiché comme bouton de téléchargement dans l'en-tête du visualiseur.
- **logoUrl** — URL d'une image de logo (ligue, équipe, événement). Affichée dans le coin supérieur droit de l'en-tête du visualiseur.

Les deux champs sont exportés comme chaînes vides par défaut. Éditez-les directement dans le fichier JSON après l'exportation.

## Schéma JSON

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

Les joueurs sont stockés dans un tableau indexé. Toutes les références aux pilotes dans `cars[]`, `chat[]` et `events` utilisent des indices entiers sur `players[]`.

## Structure du Projet

```
LFS Stats/
├── LFSStats/
│   ├── Class/
│   │   ├── CloseIntercept.cs   — Gestionnaire de fermeture multiplateforme
│   │   ├── Configuration.cs    — Parseur du fichier de configuration
│   │   ├── ExportStats.cs      — Logique d'exportation JSON
│   │   ├── LFSWorld.cs         — API LFS World (records du monde)
│   │   ├── PlayerIdentity.cs   — Association utilisateur/pseudo
│   │   ├── SessionInfo.cs      — Métadonnées de session
│   │   └── SessionStats.cs     — Statistiques par pilote et chronométrage
│   ├── Extensions/
│   │   └── Extensions.cs       — Extensions helper de TimeSpan
│   ├── Model/
│   │   ├── ChatEntry.cs        — Modèle de message de chat
│   │   ├── JsonModels.cs       — Modèles de sérialisation JSON
│   │   └── Verbose.cs          — Enum des niveaux de détail
│   ├── LFSClient.cs            — Connexion InSim et gestion des événements
│   ├── Main.cs                 — Point d'entrée et UI console
│   ├── Options.cs              — Parseur d'arguments en ligne de commande
│   └── viewer/
│       ├── stats_viewer.html   — HTML du visualiseur
│       ├── stats_renderer.js   — Moteur de rendu
│       ├── stats.css           — Styles
│       ├── translations.js     — i18n (16 langues)
│       └── examples/
│           └── endurance.json  — Exemple : course d'endurance de 5 heures
├── Graph/                      — Génération de graphiques legacy (System.Drawing)
├── LICENSE                     — GNU GPLv3
└── README.md
```

## Dépendances

| Paquet | Version | Objectif |
|--------|---------|----------|
| [InSimDotNet](https://github.com/alexmcbride/insimdotnet) | 2.7.2.1 | Bibliothèque du protocole InSim de LFS |
| [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json) | 13.0.4 | Sérialisation JSON |
| [Chart.js](https://www.chartjs.org/) | 4.x | Graphiques interactifs (CDN, visualiseur uniquement) |
| [chartjs-plugin-zoom](https://www.chartjs.org/chartjs-plugin-zoom/) | 2.x | Zoom et défilement (CDN, visualiseur uniquement) |

## Crédits

Créé à l'origine par **Robert B. (Gai-Luron)**, **JackCY** et **Yamakawa** (2007-2008).

Étendu par **Ricardo (NeoN)** avec l'exportation JSON, le visualiseur web interactif, les graphiques Chart.js, la détection de dépassements, le support multi-session, le comparateur de pilotes, le support relais/stints, l'internationalisation et une architecture de code moderne.

## Licence

[GNU General Public License v3.0](LICENSE)
