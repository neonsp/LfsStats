# LFS Stats v3.2.1

**Generador de estadísticas y visor interactivo para [Live for Speed](https://www.lfs.net/).**

## Novedades (desde v3.2.0)

- **Corregida visibilidad de DNF**: Los pilotos que inician en parrilla pero se desconectan antes de completar el primer split ahora aparecen correctamente en los resultados como DNF
- Mejorado el seguimiento de posiciones de parrilla para desconexiones tempranas
- Mejor preservación de estadísticas de pilotos al abandonar la carrera

### Actualizaciones anteriores (v3.2.0)

- Mejorado ordenamiento de resultados: pilotos con mismo número de vueltas ahora se ordenan por tiempo de finalización
- Corregidos marcadores en gráfico de comparación de pilotos
- Mejorada precisión de tiempos tras reconexiones

LFS Stats se conecta a un servidor de Live for Speed (o replay) mediante InSim, captura los datos de carrera en tiempo real y los exporta como JSON. El visor web incluido renderiza gráficos interactivos, tablas y análisis a partir de los datos exportados — sin necesidad de procesamiento en servidor.

![License](https://img.shields.io/badge/license-GPL--3.0-blue)

🇬🇧 [Read in English](README.md) · 🇫🇷 [Lire en français](README.fr.md) · 🇵🇹 [Ler em português](README.pt.md)

## Índice

- [Cómo Funciona](#cómo-funciona)
- [Inicio Rápido](#inicio-rápido)
- [Funcionalidades del Visor](#funcionalidades-del-visor)
  - [Pestañas](#pestañas)
  - [Tarjeta Resumen](#tarjeta-resumen)
  - [Gráficos Interactivos](#gráficos-interactivos)
  - [Comparador de Pilotos](#comparador-de-pilotos)
  - [Tipos de Sesión](#tipos-de-sesión)
  - [Tema Oscuro / Claro](#tema-oscuro--claro)
  - [Internacionalización](#internacionalización)
- [Despliegue del Visor](#despliegue-del-visor)
  - [Servidor Web](#despliegue-en-servidor-web)
  - [Uso Local](#uso-local-sin-servidor)
- [Referencia de Configuración](#referencia-de-configuración)
  - [LFSStats.cfg](#lfsstatscfg)
  - [Opciones de Línea de Comandos](#opciones-de-línea-de-comandos)
- [Personalizar el JSON Exportado](#personalizar-el-json-exportado)
- [Esquema JSON](#esquema-json)
- [Estructura del Proyecto](#estructura-del-proyecto)
- [Dependencias](#dependencias)
- [Créditos](#créditos)
- [Licencia](#licencia)

## Cómo Funciona

```
┌──────────┐    InSim     ┌───────────┐    JSON     ┌─────────────────┐
│  LFS     │ ◄──────────► │ LFS Stats │ ──────────► │  Visor Stats    │
│  Server  │   TCP/UDP    │  (C#)     │   Export    │  (HTML+JS)      │
└──────────┘              └───────────┘             └─────────────────┘
```

1. **LFS Stats** se conecta a un servidor de Live for Speed mediante el protocolo InSim
2. Captura todos los eventos: tiempos por vuelta, sectores, pit stops, adelantamientos, mensajes de chat, penalizaciones, etc.
3. Cuando la sesión termina, exporta todo como un **archivo JSON**
4. El **Visor de Stats** renderiza el JSON en un dashboard interactivo

## Inicio Rápido

### 1. Configurar LFS

Abre el puerto InSim en LFS:
```
/insim 29999
```

### 2. Configurar LFS Stats

Edita `LFSStats.cfg`:
```ini
host = 127.0.0.1       # IP del servidor LFS
port = 29999            # Puerto InSim (debe coincidir con el comando /insim)
adminPassword =         # Contraseña de admin del servidor (si es necesaria)
TCPmode = true          # TCP recomendado
raceDir = results       # Directorio de salida para los archivos JSON
```

### 3. Ejecutar

```bash
LFSStats.exe
```

El menú interactivo de consola permite:
- **F** — Avance rápido del replay
- **L** — Alternar preservar vueltas en ESC pit (por defecto: ON)
- **Q** — Salir de forma segura

### 4. Ver Resultados

Abre el visor con tu archivo JSON:
```
stats_viewer.html?json=race.json
```

O abre `stats_viewer.html` directamente y arrastra tu archivo JSON.

Se incluye un JSON de ejemplo en `viewer/examples/endurance.json` (carrera de resistencia de 5 horas, 10 coches mod, 195 vueltas).

### Demo en Vivo

- [Sesión de Clasificación](https://www.cesav.es/mkportal/liga/stats/stats_viewer.html?json=resultados/qual.json) — 32 pilotos, Aston
- [Sesión de Carrera](https://www.cesav.es/mkportal/liga/stats/stats_viewer.html?json=resultados/race.json) — Carrera completa con adelantamientos, pit stops, incidentes
- [Carrera de Resistencia](https://www.cesav.es/mkportal/liga/stats/stats_viewer.html?json=resultados/endurance.json) — 5 horas de resistencia con relevos de pilotos



## Funcionalidades del Visor

### Pestañas

| Pestaña | Descripción |
|---------|-------------|
| 🏁 **Resultados** | Clasificación final con colores de podio, posiciones de parrilla, mejores vueltas, pit stops, incidentes |
| 📋 **Resumen** | Orden de parrilla, mayores remontadas, vueltas lideradas, estadísticas de combatividad |
| 🔄 **Stints de Pilotos** | Información de relevos/stints para carreras de resistencia (se oculta automáticamente si no aplica) |
| 📊 **Vuelta a Vuelta** | Tabla transpuesta de tiempos por vuelta con celdas coloreadas |
| 📈 **Gráfico de Posiciones** | Gráfico interactivo Chart.js con marcadores de pit stop, DNF, vuelta rápida y mejor personal |
| ⏱️ **Progreso de Carrera** | Gráfico de diferencia con el líder mostrando la dinámica de la carrera |
| ⚡ **Mejores Tiempos** | Mejor vuelta, mejor vuelta teórica, mejores sectores, velocidades punta |
| 🔍 **Comparar** | Comparación lado a lado de pilotos (hasta 5) con gráfico de tiempos por vuelta |
| 📉 **Análisis** | Distribución de tiempos, métricas de consistencia, filtrado de outliers |
| ⚠️ **Incidentes** | Banderas amarillas, banderas azules, contactos por piloto |
| 💬 **Chat** | Mensajes de chat del juego capturados durante la sesión |

### Tarjeta Resumen

Una barra infográfica entre la cabecera y las pestañas mostrando estadísticas clave de un vistazo:
- 🏆 Ganador / Pole Position
- ⚡ Vuelta Rápida (resaltado púrpura, estilo F1)
- 💨 Velocidad Punta
- 👥 Pilotos / 🚫 DNFs
- 🔄 Total Vueltas / ⚔️ Adelantamientos / 🔧 Pit Stops / 💥 Contactos

Cada tarjeta es clickable y navega a la pestaña correspondiente.

### Gráficos Interactivos

Todos los gráficos incluyen:
- **Zoom y Pan** — Rueda del ratón para zoom, arrastrar para desplazar
- **Doble clic** — Resetear zoom
- **Tooltips al pasar** — Clasificación completa en cada punto de timing con cambios de posición, diferencias, pit stops
- **Leyenda interactiva** — Clic en nombres de pilotos para mostrar/ocultar, botones "Mostrar Todo / Ocultar Todo"
- **Marcadores de eventos** — Pit stops (🔧), DNF (✕), vuelta rápida (★), mejor personal (●)

### Comparador de Pilotos

Selecciona de 2 a 5 pilotos para comparar:
- Tabla de estadísticas: posición, parrilla, vueltas, mejor vuelta, sectores, pit stops, incidentes
- Gráfico de tiempos por vuelta con marcadores de pit stop
- Zoom inteligente del eje Y centrado en la media de tiempos
- En clasificación oculta estadísticas irrelevantes (vuelta media, consistencia, gráfico)

### Tipos de Sesión

| Tipo de Sesión | Funcionalidades |
|---|---|
| **Carrera** | Análisis completo: posiciones, diferencias, adelantamientos, stints, combatividad |
| **Clasificación** | Gráfico de posición temporal, evolución de mejor vuelta, zona de límite de tiempo |
| **Práctica** | Análisis básico de tiempos por vuelta |

### Tema Oscuro / Claro

Alterna entre tema oscuro y claro con el botón 🌙/☀️ (esquina superior derecha). La preferencia se guarda en `localStorage` y se aplica instantáneamente — incluyendo todos los gráficos Chart.js. Los códigos de color LFS (`^0`–`^9`) se renderizan con ajustes de contraste por tema.

### Internacionalización

El visor detecta automáticamente el idioma del navegador con 16 idiomas soportados:

🇬🇧 Inglés · 🇪🇸 Español · 🇫🇷 Francés · 🇵🇹 Portugués · 🇩🇪 Alemán · 🇮🇹 Italiano · 🇵🇱 Polaco · 🇷🇺 Ruso · 🇹🇷 Turco · 🇫🇮 Finlandés · 🇸🇪 Sueco · 🇱🇹 Lituano · 🇯🇵 Japonés · 🇨🇳 Chino · 🇳🇱 Neerlandés · 🇩🇰 Danés

## Despliegue del Visor

### Despliegue en Servidor Web

Sube los archivos del visor a tu servidor web:
```
/tu-sitio/lfsstats/
  ├── stats_viewer.html
  ├── stats_renderer.js
  ├── stats.css
  ├── translations.js
  └── race.json          ← tus archivos JSON exportados
```

Accede mediante URL con el parámetro `json`:
```
https://tu-sitio.com/lfsstats/stats_viewer.html?json=race.json
https://tu-sitio.com/lfsstats/stats_viewer.html?json=clasificacion.json
https://tu-sitio.com/lfsstats/stats_viewer.html?json=resultados/ronda1.json
```

Puedes organizar los archivos JSON en subdirectorios:
```
/lfsstats/
  ├── stats_viewer.html
  ├── temporada2025/
  │   ├── ronda1.json
  │   ├── ronda2.json
  │   └── ronda3_clasif.json
  └── resistencia/
      └── carrera_24h.json
```
```
stats_viewer.html?json=temporada2025/ronda1.json
stats_viewer.html?json=resistencia/carrera_24h.json
```

### Uso Local (Sin Servidor)

Abre `stats_viewer.html` directamente en tu navegador. Aparecerá una zona de arrastrar y soltar — suelta tu archivo JSON para cargarlo.

## Referencia de Configuración

### LFSStats.cfg

| Opción | Por Defecto | Descripción |
|--------|-------------|-------------|
| `host` | `127.0.0.1` | Dirección IP o nombre del servidor LFS |
| `port` | `29999` | Puerto InSim (configurado en LFS con `/insim <puerto>`) |
| `adminPassword` | *(vacío)* | Contraseña de administrador del servidor |
| `TCPmode` | `true` | Usar TCP (`true`) o UDP (`false`) para la conexión InSim |
| `isLocal` | `true` | `true` para servidor local, `false` para host remoto |
| `pracDir` | `results` | Directorio de salida para estadísticas de práctica |
| `qualDir` | `results` | Directorio de salida para estadísticas de clasificación |
| `raceDir` | `results` | Directorio de salida para estadísticas de carrera |
| `exportOnRaceSTart` | `yes` | Exportar al reiniciar sesión: `yes`, `no`, o `ask` |
| `askForFileNameOnRST` | `false` | Preguntar nombre de archivo al exportar |
| `exportOnSTAte` | `no` | Exportar al cambiar estado (interrupción): `yes`, `no`, o `ask` |
| `askForFileNameOnSTA` | `false` | Preguntar nombre de archivo al cambiar estado |
| `preserveLapsOnPit` | `true` | Conservar datos de vueltas cuando un piloto hace ESC-pit y vuelve a entrar |
| `defaultLogoUrl` | *(vacío)* | URL del logo por defecto escrito en `metadata.logoUrl` en cada exportación JSON |
| `pubStatIDkey` | *(vacío)* | Clave API PubStat de LFS World para récords mundiales |

### Opciones de Línea de Comandos

```
LFSStats.exe [opciones]

  -c, --config <archivo>  Archivo de configuración (por defecto: LFSStats.cfg)
  -i, --interval <ms>     Intervalo de refresco InSim: 1-1000 ms (por defecto: 100)
  -v, --verbose <nivel>   Nivel de detalle: 0-4 (por defecto: 1)
      --version           Mostrar información de versión
  -h, --help              Mostrar esta información
```

**Niveles de detalle:**
- `0` — Programa (solo errores)
- `1` — Sesión (inicio/fin de sesión, resultados)
- `2` — Vuelta (completar vueltas)
- `3` — Sector (tiempos de sector)
- `4` — Info (todos los eventos, conexiones, debug)

### Personalizar el JSON Exportado

La sección `metadata` se coloca al inicio del archivo JSON para facilitar su edición:

```json
{
  "metadata": {
    "exportedAt": "2025-07-11T18:30:00Z",
    "mprUrl": "https://tu-sitio.com/replays/carrera.mpr",
    "logoUrl": "https://tu-sitio.com/images/logo-liga.png"
  },
  ...
}
```

- **mprUrl** — Enlace al archivo de replay MPR para descarga. Se muestra como botón de descarga en la cabecera del visor.
- **logoUrl** — URL de una imagen de logo (liga, equipo, evento). Se muestra en la esquina superior derecha de la cabecera del visor.

Ambos campos se exportan como cadenas vacías por defecto. Edítalos directamente en el archivo JSON tras la exportación.

## Esquema JSON

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

Los jugadores se almacenan como un array indexado. Todas las referencias a pilotos en `cars[]`, `chat[]` y `events` usan índices enteros sobre `players[]`.

## Estructura del Proyecto

```
LFS Stats/
├── LFSStats/
│   ├── Class/
│   │   ├── CloseIntercept.cs   — Manejador de cierre multiplataforma
│   │   ├── Configuration.cs    — Parser del archivo de configuración
│   │   ├── ExportStats.cs      — Lógica de exportación JSON
│   │   ├── LFSWorld.cs         — API de LFS World (récords mundiales)
│   │   ├── PlayerIdentity.cs   — Asociación usuario/nickname
│   │   ├── SessionInfo.cs      — Metadatos de sesión
│   │   └── SessionStats.cs     — Estadísticas por piloto y timing
│   ├── Extensions/
│   │   └── Extensions.cs       — Extensiones helper de TimeSpan
│   ├── Model/
│   │   ├── ChatEntry.cs        — Modelo de mensaje de chat
│   │   ├── JsonModels.cs       — Modelos de serialización JSON
│   │   └── Verbose.cs          — Enum de niveles de detalle
│   ├── LFSClient.cs            — Conexión InSim y manejo de eventos
│   ├── Main.cs                 — Punto de entrada y UI de consola
│   ├── Options.cs              — Parser de argumentos de línea de comandos
│   └── viewer/
│       ├── stats_viewer.html   — HTML del visor
│       ├── stats_renderer.js   — Motor de renderizado
│       ├── stats.css           — Estilos
│       ├── translations.js     — i18n (16 idiomas)
│       └── examples/
│           └── endurance.json  — Ejemplo: carrera de resistencia de 5 horas
├── Graph/                      — Generación de gráficos legacy (System.Drawing)
├── LICENSE                     — GNU GPLv3
└── README.md
```

## Dependencias

| Paquete | Versión | Propósito |
|---------|---------|-----------|
| [InSimDotNet](https://github.com/alexmcbride/insimdotnet) | 2.7.2.1 | Librería del protocolo InSim de LFS |
| [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json) | 13.0.4 | Serialización JSON |
| [Chart.js](https://www.chartjs.org/) | 4.x | Gráficos interactivos (CDN, solo visor) |
| [chartjs-plugin-zoom](https://www.chartjs.org/chartjs-plugin-zoom/) | 2.x | Zoom y pan (CDN, solo visor) |

## Créditos

Creado originalmente por **Robert B. (Gai-Luron)**, **JackCY** y **Yamakawa** (2007-2008).

Ampliado por **Ricardo (NeoN)** con exportación JSON, visor web interactivo, gráficos Chart.js, detección de adelantamientos, soporte multi-sesión, comparador de pilotos, soporte de relevos/stints, internacionalización y arquitectura de código moderna.

## Licencia

[GNU General Public License v3.0](LICENSE)
