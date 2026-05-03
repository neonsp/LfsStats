# LFS Stats v3.2.1

**Gerador de estatísticas e visualizador interativo para [Live for Speed](https://www.lfs.net/).**

## Novidades (desde v3.2.0)

- **Corrigida visibilidade de DNF**: Pilotos que iniciam no grid mas desconectam antes de completar o primeiro split agora aparecem corretamente nos resultados como DNF
- Melhorado o rastreamento de posições de grid para desconexões precoces
- Melhor preservação de estatísticas de pilotos ao abandonar a corrida

### Atualizações anteriores (v3.2.0)

- Melhorada ordenação de resultados: pilotos com mesmo número de voltas agora ordenados por tempo de conclusão
- Corrigidos marcadores no gráfico de comparação de pilotos
- Melhorada precisão de tempos após reconexões

O LFS Stats conecta-se a um servidor de Live for Speed (ou replay) via InSim, captura os dados de corrida em tempo real e exporta-os como JSON. O visualizador web incluído renderiza gráficos interativos, tabelas e análises a partir dos dados exportados — sem necessidade de processamento no servidor.

![License](https://img.shields.io/badge/license-GPL--3.0-blue)

🇬🇧 [Read in English](README.md) · 🇪🇸 [Leer en español](README.es.md) · 🇫🇷 [Lire en français](README.fr.md)

## Índice

- [Como Funciona](#como-funciona)
- [Início Rápido](#início-rápido)
- [Funcionalidades do Visualizador](#funcionalidades-do-visualizador)
  - [Abas](#abas)
  - [Cartão Resumo](#cartão-resumo)
  - [Gráficos Interativos](#gráficos-interativos)
  - [Comparador de Pilotos](#comparador-de-pilotos)
  - [Tipos de Sessão](#tipos-de-sessão)
  - [Tema Escuro / Claro](#tema-escuro--claro)
  - [Internacionalização](#internacionalização)
- [Implantação do Visualizador](#implantação-do-visualizador)
  - [Servidor Web](#implantação-em-servidor-web)
  - [Uso Local](#uso-local-sem-servidor)
- [Referência de Configuração](#referência-de-configuração)
  - [LFSStats.cfg](#lfsstatscfg)
  - [Opções de Linha de Comando](#opções-de-linha-de-comando)
- [Personalizar o JSON Exportado](#personalizar-o-json-exportado)
- [Esquema JSON](#esquema-json)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Dependências](#dependências)
- [Créditos](#créditos)
- [Licença](#licença)

## Como Funciona

```
┌──────────┐    InSim     ┌───────────┐    JSON     ┌─────────────────┐
│  LFS     │ ◄──────────► │ LFS Stats │ ──────────► │  Visualizador   │
│  Server  │   TCP/UDP    │  (C#)     │   Export    │  (HTML+JS)      │
└──────────┘              └───────────┘             └─────────────────┘
```

1. **LFS Stats** conecta-se a um servidor Live for Speed via protocolo InSim
2. Captura todos os eventos: tempos de volta, setores, pit stops, ultrapassagens, mensagens de chat, penalizações, etc.
3. Quando a sessão termina, exporta tudo como um **ficheiro JSON**
4. O **Visualizador de Stats** renderiza o JSON num dashboard interativo

## Início Rápido

### 1. Configurar o LFS

Abra a porta InSim no LFS:
```
/insim 29999
```

### 2. Configurar o LFS Stats

Edite o `LFSStats.cfg`:
```ini
host = 127.0.0.1       # IP do servidor LFS
port = 29999            # Porta InSim (deve corresponder ao comando /insim)
adminPassword =         # Palavra-passe de admin do servidor (se necessário)
TCPmode = true          # TCP recomendado
raceDir = results       # Diretório de saída para os ficheiros JSON
```

### 3. Executar

```bash
LFSStats.exe
```

O menu interativo da consola permite:
- **F** — Avanço rápido do replay
- **L** — Alternar preservar voltas em ESC pit (por defeito: ON)
- **Q** — Sair em segurança

### 4. Ver Resultados

Abra o visualizador com o seu ficheiro JSON:
```
stats_viewer.html?json=race.json
```

Ou abra `stats_viewer.html` diretamente e arraste o seu ficheiro JSON.

Um ficheiro JSON de exemplo está incluído em `viewer/examples/endurance.json` (corrida de endurance de 5 horas, 10 carros mod, 195 voltas).

### Demo ao Vivo

- [Sessão de Qualificação](https://www.cesav.es/mkportal/liga/stats/stats_viewer.html?json=resultados/qual.json) — 32 pilotos, Aston
- [Sessão de Corrida](https://www.cesav.es/mkportal/liga/stats/stats_viewer.html?json=resultados/race.json) — Corrida completa com ultrapassagens, pit stops, incidentes
- [Corrida de Endurance](https://www.cesav.es/mkportal/liga/stats/stats_viewer.html?json=resultados/endurance.json) — 5 horas de endurance com relays de pilotos



## Funcionalidades do Visualizador

### Abas

| Aba | Descrição |
|-----|-----------|
| 🏁 **Resultados** | Classificação final com cores de pódio, posições de grelha, melhores voltas, pit stops, incidentes |
| 📋 **Resumo** | Ordem de grelha, maiores subidas, voltas lideradas, estatísticas de combatividade |
| 🔄 **Stints de Pilotos** | Informações de relay/stints para corridas de endurance (oculto automaticamente se não aplicável) |
| 📊 **Volta a Volta** | Tabela transposta de tempos de volta com células coloridas |
| 📈 **Gráfico de Posições** | Gráfico interativo Chart.js com marcadores de pit stop, DNF, volta mais rápida e melhor pessoal |
| ⏱️ **Progresso da Corrida** | Gráfico de diferença para o líder mostrando a dinâmica da corrida |
| ⚡ **Melhores Tempos** | Melhor volta, melhor volta teórica, melhores setores, velocidades de ponta |
| 🔍 **Comparar** | Comparação lado a lado de pilotos (até 5) com gráfico de tempos de volta |
| 📉 **Análise** | Distribuição de tempos de volta, métricas de consistência, filtragem de outliers |
| ⚠️ **Incidentes** | Bandeiras amarelas, bandeiras azuis, contactos por piloto |
| 💬 **Chat** | Mensagens de chat do jogo capturadas durante a sessão |

### Cartão Resumo

Uma barra infográfica entre o cabeçalho e as abas mostrando estatísticas-chave num relance:
- 🏆 Vencedor / Pole Position
- ⚡ Volta Mais Rápida (destaque roxo, estilo F1)
- 💨 Velocidade de Ponta
- 👥 Pilotos / 🚫 DNFs
- 🔄 Total Voltas / ⚔️ Ultrapassagens / 🔧 Pit Stops / 💥 Contactos

Cada cartão é clicável e navega para a aba correspondente.

### Gráficos Interativos

Todos os gráficos incluem:
- **Zoom e Pan** — Roda do rato para zoom, arrastar para deslocar
- **Duplo clique** — Repor zoom
- **Tooltips ao passar** — Classificação completa em cada ponto de cronometragem com mudanças de posição, diferenças, pit stops
- **Legenda interativa** — Clique nos nomes dos pilotos para mostrar/ocultar, botões "Mostrar Tudo / Ocultar Tudo"
- **Marcadores de eventos** — Pit stops (🔧), DNF (✕), volta mais rápida (★), melhor pessoal (●)

### Comparador de Pilotos

Selecione de 2 a 5 pilotos para comparar:
- Tabela de estatísticas: posição, grelha, voltas, melhor volta, setores, pit stops, incidentes
- Gráfico de tempos de volta com marcadores de pit stop
- Zoom inteligente do eixo Y centrado na média de tempos
- Em qualificação oculta estatísticas irrelevantes (volta média, consistência, gráfico)

### Tipos de Sessão

| Tipo de Sessão | Funcionalidades |
|---|---|
| **Corrida** | Análise completa: posições, diferenças, ultrapassagens, stints, combatividade |
| **Qualificação** | Gráfico de posição temporal, evolução da melhor volta, zona de limite de tempo |
| **Prática** | Análise básica de tempos de volta |

### Tema Escuro / Claro

Alterne entre tema escuro e claro com o botão 🌙/☀️ (canto superior direito). A preferência é salva no `localStorage` e aplicada instantaneamente — incluindo todos os gráficos Chart.js. Os códigos de cor LFS (`^0`–`^9`) são renderizados com ajustes de contraste por tema.

### Internacionalização

O visualizador deteta automaticamente o idioma do navegador com 16 idiomas suportados:

🇬🇧 Inglês · 🇪🇸 Espanhol · 🇫🇷 Francês · 🇵🇹 Português · 🇩🇪 Alemão · 🇮🇹 Italiano · 🇵🇱 Polaco · 🇷🇺 Russo · 🇹🇷 Turco · 🇫🇮 Finlandês · 🇸🇪 Sueco · 🇱🇹 Lituano · 🇯🇵 Japonês · 🇨🇳 Chinês · 🇳🇱 Neerlandês · 🇩🇰 Dinamarquês

## Implantação do Visualizador

### Implantação em Servidor Web

Carregue os ficheiros do visualizador para o seu servidor web:
```
/seu-site/lfsstats/
  ├── stats_viewer.html
  ├── stats_renderer.js
  ├── stats.css
  ├── translations.js
  └── race.json          ← os seus ficheiros JSON exportados
```

Aceda via URL com o parâmetro `json`:
```
https://seu-site.com/lfsstats/stats_viewer.html?json=race.json
https://seu-site.com/lfsstats/stats_viewer.html?json=qualificacao.json
https://seu-site.com/lfsstats/stats_viewer.html?json=resultados/ronda1.json
```

Pode organizar os ficheiros JSON em subdiretórios:
```
/lfsstats/
  ├── stats_viewer.html
  ├── temporada2025/
  │   ├── ronda1.json
  │   ├── ronda2.json
  │   └── ronda3_qualif.json
  └── endurance/
      └── corrida_24h.json
```
```
stats_viewer.html?json=temporada2025/ronda1.json
stats_viewer.html?json=endurance/corrida_24h.json
```

### Uso Local (Sem Servidor)

Abra `stats_viewer.html` diretamente no seu navegador. Aparecerá uma zona de arrastar e largar — largue o seu ficheiro JSON para o carregar.

## Referência de Configuração

### LFSStats.cfg

| Opção | Por Defeito | Descrição |
|-------|-------------|-----------|
| `host` | `127.0.0.1` | Endereço IP ou nome do servidor LFS |
| `port` | `29999` | Porta InSim (configurada no LFS com `/insim <porta>`) |
| `adminPassword` | *(vazio)* | Palavra-passe de administrador do servidor |
| `TCPmode` | `true` | Usar TCP (`true`) ou UDP (`false`) para a conexão InSim |
| `isLocal` | `true` | `true` para servidor local, `false` para host remoto |
| `pracDir` | `results` | Diretório de saída para estatísticas de prática |
| `qualDir` | `results` | Diretório de saída para estatísticas de qualificação |
| `raceDir` | `results` | Diretório de saída para estatísticas de corrida |
| `exportOnRaceSTart` | `yes` | Exportar ao reiniciar sessão: `yes`, `no`, ou `ask` |
| `askForFileNameOnRST` | `false` | Perguntar nome do ficheiro ao exportar |
| `exportOnSTAte` | `no` | Exportar na mudança de estado (interrupção): `yes`, `no`, ou `ask` |
| `askForFileNameOnSTA` | `false` | Perguntar nome do ficheiro na mudança de estado |
| `preserveLapsOnPit` | `true` | Manter dados de voltas quando um piloto faz ESC-pit e volta a entrar |
| `defaultLogoUrl` | *(vazio)* | URL do logo padrão escrito em `metadata.logoUrl` em cada exportação JSON |
| `pubStatIDkey` | *(vazio)* | Chave API PubStat do LFS World para recordes mundiais |

### Opções de Linha de Comando

```
LFSStats.exe [opções]

  -c, --config <ficheiro> Ficheiro de configuração (por defeito: LFSStats.cfg)
  -i, --interval <ms>     Intervalo de atualização InSim: 1-1000 ms (por defeito: 100)
  -v, --verbose <nível>   Nível de detalhe: 0-4 (por defeito: 1)
      --version           Mostrar informações de versão
  -h, --help              Mostrar esta informação
```

**Níveis de detalhe:**
- `0` — Programa (apenas erros)
- `1` — Sessão (início/fim de sessão, resultados)
- `2` — Volta (voltas completadas)
- `3` — Setor (tempos de setor)
- `4` — Info (todos os eventos, conexões, debug)

### Personalizar o JSON Exportado

A secção `metadata` é colocada no início do ficheiro JSON para facilitar a edição:

```json
{
  "metadata": {
    "exportedAt": "2025-07-11T18:30:00Z",
    "mprUrl": "https://seu-site.com/replays/corrida.mpr",
    "logoUrl": "https://seu-site.com/images/logo-liga.png"
  },
  ...
}
```

- **mprUrl** — Link para o ficheiro de replay MPR para download. Mostrado como botão de download no cabeçalho do visualizador.
- **logoUrl** — URL de uma imagem de logo (liga, equipa, evento). Exibida no canto superior direito do cabeçalho do visualizador.

Ambos os campos são exportados como strings vazias por defeito. Edite-os diretamente no ficheiro JSON após a exportação.

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

Os jogadores são armazenados num array indexado. Todas as referências a pilotos em `cars[]`, `chat[]` e `events` usam índices inteiros sobre `players[]`.

## Estrutura do Projeto

```
LFS Stats/
├── LFSStats/
│   ├── Class/
│   │   ├── CloseIntercept.cs   — Gestor de fecho multiplataforma
│   │   ├── Configuration.cs    — Parser do ficheiro de configuração
│   │   ├── ExportStats.cs      — Lógica de exportação JSON
│   │   ├── LFSWorld.cs         — API LFS World (recordes mundiais)
│   │   ├── PlayerIdentity.cs   — Associação utilizador/nickname
│   │   ├── SessionInfo.cs      — Metadados de sessão
│   │   └── SessionStats.cs     — Estatísticas por piloto e cronometragem
│   ├── Extensions/
│   │   └── Extensions.cs       — Extensões helper de TimeSpan
│   ├── Model/
│   │   ├── ChatEntry.cs        — Modelo de mensagem de chat
│   │   ├── JsonModels.cs       — Modelos de serialização JSON
│   │   └── Verbose.cs          — Enum de níveis de detalhe
│   ├── LFSClient.cs            — Conexão InSim e gestão de eventos
│   ├── Main.cs                 — Ponto de entrada e UI de consola
│   ├── Options.cs              — Parser de argumentos de linha de comando
│   └── viewer/
│       ├── stats_viewer.html   — HTML do visualizador
│       ├── stats_renderer.js   — Motor de renderização
│       ├── stats.css           — Estilos
│       ├── translations.js     — i18n (16 idiomas)
│       └── examples/
│           └── endurance.json  — Exemplo: corrida de endurance de 5 horas
├── Graph/                      — Geração de gráficos legacy (System.Drawing)
├── LICENSE                     — GNU GPLv3
└── README.md
```

## Dependências

| Pacote | Versão | Propósito |
|--------|--------|-----------|
| [InSimDotNet](https://github.com/alexmcbride/insimdotnet) | 2.7.2.1 | Biblioteca do protocolo InSim do LFS |
| [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json) | 13.0.4 | Serialização JSON |
| [Chart.js](https://www.chartjs.org/) | 4.x | Gráficos interativos (CDN, apenas visualizador) |
| [chartjs-plugin-zoom](https://www.chartjs.org/chartjs-plugin-zoom/) | 2.x | Zoom e pan (CDN, apenas visualizador) |

## Créditos

Criado originalmente por **Robert B. (Gai-Luron)**, **JackCY** e **Yamakawa** (2007-2008).

Expandido por **Ricardo (NeoN)** com exportação JSON, visualizador web interativo, gráficos Chart.js, deteção de ultrapassagens, suporte multi-sessão, comparador de pilotos, suporte de relays/stints, internacionalização e arquitetura de código moderna.

## Licença

[GNU General Public License v3.0](LICENSE)
