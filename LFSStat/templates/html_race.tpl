<html>
<head>
<link rel="stylesheet" type="text/css" href="results3.css">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8">
<title>{lg_doctitle}</title>
</head>
<body>
<body style="background-image: url(image/background.jpg);">
[include .\include\header_race.tpl]
<table width="100%"><tr><td align="left"><h1>{lg_doctitle}</h1></td><td align="right"><div style="font-size: 7pt;"><a href="http://www.lfsforum.net/showthread.php?t=24933">{VERSIONSHORT}</a></div></td></tr></table>

<p>

<table>
<td>
<table class="tab">
<tr><td class="vth">{lg_server}</td><td class="vtb2">{ServerName}</td>
<td style="background-color: rgb(0, 0, 0);" rowspan="4"><img alt="Track Image" title="Track Image" src="tracks/{TrackImg}"></td></tr>
<tr><td class="vth">{lg_track}</td><td class="vtb">{TrackNameFull} ({TrackNameShort})</td></tr>
<tr><td class="vth">{lg_duration}</td><td class="vtb">{RaceLength}</td></tr>
<tr><td class="vth">{lg_conditions}</td><td class="vtb">{RaceConditions}</td></tr>
</table>
</td>
<td>
<div align = center>
<a href="#rrs">{lg_raceres}</a>&nbsp;
<br>
<a href="#hcl">{lg_highclimb}</a>&nbsp;
<a href="#ral">{lg_racelead}</a>&nbsp;
<a href="#lal">{lg_laplead}</a>&nbsp;
<br>
<a href="#dev">{lg_stability}</a>&nbsp;
<a href="#avgl">{lg_avglap}</a>&nbsp;
<a href="#spu">{lg_firstlap}</a>&nbsp;
<br>
<a href="#spl">{lg_bestsplits}</a>&nbsp;
<br>
<a href="#bst">{lg_bestlap}</a>&nbsp;
<a href="#bpl">{lg_bestposlap}</a>&nbsp;
<a href="#top">{lg_topspeed}</a>&nbsp;
<br>
<a href="#pit">{lg_pitstop}</a>&nbsp;
<a href="#rel">{lg_relay}</a>&nbsp;
<br>
<a href="#yfl">{lg_yellowflagcaused}</a>&nbsp;
<a href="#bfl">{lg_blueflagshown}</a>&nbsp;
<a href="#pen">{lg_pens}</a>&nbsp;
<br>
<a href="#lbl">{lg_lblgraph}</a>&nbsp;
<a href="#rpr">{lg_rpgraph}</a>&nbsp;
<br>
<a href="{linklbl}">{lg_lbl}</a>&nbsp;
<a href="{linkchat}">{lg_chat}</a>&nbsp;
</div>
</td>
</table>

<p>

<table>
<td style="vertical-align:top">
<a name="rrs"></a><table class="tab"><tr class="titl"><td colspan="11">{lg_raceres}</td></tr>
<tr class="titl2"><td>P</td><td colspan="2">{lg_racer}</td><td>{lg_plate}</td><td>{lg_car}</td><td style="text-align: center">{lg_racetime}</td><td>{lg_bestlap}</td><td>{lg_laps}</td><td>{lg_pit}</td><td>{lg_flags}</td><td>{lg_penalty}</td></tr>
[RaceResults <tr class="row"><td>{Position}</td><td style="text-align:left; font-weight: bold; background: #c9c5c2;">{PlayerNameColoured}</td><td style="text-align:left"><a href="http://lfsworld.com/?win=stats&racer={UserNameLink}">{UserName}</a></td><td style="text-align:left">{Plate}</td><td style="text-align:left">{Car}</td><td>{Gap}</td><td>{BestLap}</td><td>{LapsDone}</td><td>{PitsDone}</td><td>{Flags}</td><td>{Penalty}</td></tr>]
</table>
</td>
<td style="vertical-align:top">
<a name="rrs"></a><table class="tab"><tr class="titl"><td colspan="3">{lg_startord}</td></tr>
<tr class="titl2"><td>P</td><td colspan="2">{lg_racer}</td></tr>
[StartOrder <tr class="row"><td>{Position}</td><td style="text-align:left;">{PlayerName}</td><td style="text-align:left"><a href="http://lfsworld.com/?win=stats&racer={UserNameLink}">{UserName}</a></td></tr>]
</table>
</td>
</table>

<p>

<table>
<td style="vertical-align:top">
<a name="hcl"></a><table class="tab"><tr class="titl"><td colspan="6">{lg_highclimb}</td></tr>
<tr class="titl2"><td>P</td><td colspan="2">{lg_racer}</td><td>{lg_start}</td><td>{lg_finish}</td><td>{lg_gain}</td></tr>
[HighestClimber <tr class="row"><td>{Position}</td><td style="text-align:left;">{PlayerName}</td><td style="text-align:left">{UserName}</td><td>{StartPos}</td><td>{FinishPos}</td><td>{Difference}</td></tr>]
</table>
</td>
<td style="vertical-align:top">
<a name="ral"></a><table class="tab"><tr class="titl"><td colspan="4">{lg_racelead}</td></tr>
<tr class="titl2"><td>N</td><td colspan="2">{lg_racer}</td><td>{lg_laps}</td></tr>
[RaceLeader <tr class="row"><td>{Position}</td><td style="text-align:left;">{PlayerName}</td><td style="text-align:left">{UserName}</td><td>{LapsLead}</td></tr>]
</table>
</td>
<td style="vertical-align:top">
<a name="lal"></a><table class="tab"><tr class="titl"><td colspan="4">{lg_laplead}</td></tr>
<tr class="titl2"><td>P</td><td colspan="2">{lg_racer}</td><td>{lg_laps}</td></tr>
[LapsLed <tr class="row"><td>{Position}</td><td style="text-align:left;">{PlayerName}</td><td style="text-align:left">{UserName}</td><td>{LapsLed}</td></tr>]
</table>
</td>
</table>

<p>

<table>
<td style="vertical-align:top">
<a name="dev"></a><table class="tab"><tr class="titl"><td colspan="6">{lg_stability}</td></tr>
<tr class="titl2"><td>P</td><td colspan="2">{lg_racer}</td><td>{lg_dev}</td><td>{lg_gap}</td><td style="text-align:right">{lg_laps}</td></tr>
[LapTimesStability <tr class="row"><td>{Position}</td><td style="text-align:left;">{PlayerName}</td><td style="text-align:left">{UserName}</td><td>{Deviation}</td><td>{Difference}</td><td>{LapsDone}</td></tr>]
</table>
</td>
<td style="vertical-align:top">
<a name="avgl"></a><table class="tab"><tr class="titl"><td colspan="6">{lg_avglap}</td></tr>
<tr class="titl2"><td>P</td><td colspan="2">{lg_racer}</td><td>{lg_time}</td><td>{lg_gap}</td><td>{lg_laps}</td></tr>
[AverageLap <tr class="row"><td>{Position}</td><td style="text-align:left;">{PlayerName}</td><td style="text-align:left">{UserName}</td><td>{LapTime}</td><td>{Difference}</td><td>{Laps}</td></tr>]
</table>
</td>
<td style="vertical-align:top">
<a name="spu"></a><table class="tab"><tr class="titl"><td colspan="5">{lg_firstlap}</td></tr>
<tr class="titl2"><td>P</td><td colspan="2">{lg_racer}</td><td>{lg_time}</td><td>{lg_gap}</td></tr>
[FirstLap <tr class="row"><td>{Position}</td><td style="text-align:left;">{PlayerName}</td><td style="text-align:left">{UserName}</td><td>{LapTime}</td><td>{Difference}</td></tr>]
</table>
</td>
</table>

<p>

<a name="spl"></a>
<table>
[[BestSplitTable 
<td style="vertical-align:top">
<table class="tab"><tr class="titl"><td colspan="7">{lg_bestsplit} {SplitNumber}</td></tr>
<tr class="titl2"><td>P</td><td colspan="2">{lg_racer}</td><td>{lg_time}</td><td>{lg_gap}</td><td>{lg_lap}</td></tr>
[BestSplit <tr class="row"><td>{Position}</td><td style="text-align:left;">{PlayerName}</td><td style="text-align:left">{UserName}</td><td>{SplitTime}</td><td>{Difference}</td><td>{Lap}</td></tr>]
</table>
</td>
]]
</table>

<p>

<table>
<td style="vertical-align:top">
<a name="bst"></a><table class="tab"><tr class="titl"><td colspan="7">{lg_bestlap}</td></tr>
<tr class="titl2"><td>P</td><td colspan="2">{lg_racer}</td><td>{lg_time}</td><td>{lg_gap}</td><td>{lg_lap}</td></tr>
[BestLap <tr class="row"><td>{Position}</td><td style="text-align:left;">{PlayerName}</td><td style="text-align:left">{UserName}</td><td>{LapTime}</td><td>{Difference}</td><td>{Lap}</td></tr>]
</table>
&nbsp;&nbsp;<font size="-2">&nbsp;&nbsp;<strong>&nbsp;&nbsp;</strong></font>
</td>
<td style="vertical-align:top">
<a name="bpl"></a><table class="tab"><tr class="titl"><td colspan="7">{lg_bestposlap}</td></tr>
<tr class="titl2"><td>P</td><td colspan="2">{lg_racer}</td><td>{lg_time}</td><td>{lg_gap}</td><td>{lg_bestlap}</td></tr>
[BestPossibleLap <tr class="row"><td>{Position}</td><td style="text-align:left;">{PlayerName}</td><td style="text-align:left">{UserName}</td><td>{LapTime}</td><td>{Difference}</td><td>{DifferenceToBestLap}</td></tr>]
</table>
&nbsp;&nbsp;<font size="-2">Combined best lap time: <strong>{CombinedBestLap}</strong></font>
</td>
<td style="vertical-align:top">
<a name="top"></a><table class="tab"><tr class="titl"><td colspan="6">{lg_topunit}</td></tr>
<tr class="titl2"><td>P</td><td colspan="2">{lg_racer}</td><td>{lg_speed}</td><td>{lg_gap}</td><td>{lg_lap}</td></tr>
[TopSpeed <tr class="row"><td>{Position}</td><td style="text-align:left;">{PlayerName}&nbsp;</td><td style="text-align:left">{UserName}</td><td>{TopSpeed}</td><td>{Difference}</td><td>{TopSpeedLap}</td></tr>]
</table>
&nbsp;&nbsp;<font size="-2">&nbsp;&nbsp;<strong>&nbsp;&nbsp;</strong></font>
</td>
</table>

<p>

<table>
<td style="vertical-align:top">
<a name="pit"></a><table class="tab"><tr class="titl"><td colspan="5">{lg_pitstop}</td></tr>
<tr class="titl2"><td>P</td><td colspan="2">{lg_racer}</td><td>{lg_ptinfo}</td><td>{lg_stoptime}</td></tr>
[PitStops <tr class="row"><td>{Position}</td><td style="text-align:left;">{PlayerName}</td><td style="text-align:left">{UserName}</td><td style="text-align:left">{PitInfo}</td><td>{PitTime}</td></tr>]
</table>
</td>
<td style="vertical-align:top">
<a name="rel"></a><table class="tab"><tr class="titl"><td colspan="5">{lg_relay}</td></tr>
<tr class="titl2"><td>P</td><td colspan="2">{lg_racer}</td><td>{lg_relay}</td><td>{lg_laps}</td></tr>
[Relay <tr class="row"><td>{Position}</td><td style="text-align:left;">{PlayerName}</td><td style="text-align:left">{UserName}</td><td style="text-align:left">{Relays}</td><td style="text-align:left">{Laps}</td></tr>]
</table>
</td>
</table>

<p>

<table>
<td style="vertical-align:top">
<a name="yfl"></a><table class="tab"><tr class="titl"><td colspan="4">{lg_yellowflagcaused}</td></tr>
<tr class="titl2"><td>P</td><td colspan="2">{lg_racer}</td><td>{lg_flags}</td></tr>
[YellowFlagCausers <tr class="row"><td>{Position}</td><td style="text-align:left;">{PlayerName}</td><td style="text-align:left">{UserName}</td><td>{YellowFlagsCount}</td></tr>]
</table>
</td>
<td style="vertical-align:top">
<a name="bfl"></a><table class="tab"><tr class="titl"><td colspan="4">{lg_blueflagshown}</td></tr>
<tr class="titl2"><td>P</td><td colspan="2">{lg_racer}</td><td>{lg_flags}</td></tr>
[BlueFlagCausers <tr class="row"><td>{Position}</td><td style="text-align:left;">{PlayerName}</td><td style="text-align:left">{UserName}</td><td>{BlueFlagsCount}</td></tr>]
</table>
</td>
<td style="vertical-align:top">
<a name="pen"></a><table class="tab"><tr class="titl"><td colspan="5">{lg_pens}</td></tr>
<tr class="titl2"><td>P</td><td colspan="2">{lg_racer}</td><td>{lg_pen_info}</td><td>{lg_pens}</td></tr>
[Penalties <tr class="row"><td>{Position}</td><td style="text-align:left;">{PlayerName}</td><td style="text-align:left">{UserName}</td><td style="text-align:left">{PenaltyInfo}</td><td>{PenaltyCount}</td></tr>]
</table>
</td>
</table>

<p>

<a name="lbl"><img alt="Lap by lap graph" title="Lap by lap graph" src="graph/{LapByLapGraphFileName}"></a>

<p>

<a name="rpr"><img alt="Race progress graph" title="Race progress graph" src="graph/{RaceProgressGraphFileName}"></a>

<p>

[include .\include\footer_race.tpl]

</body>
</html>
