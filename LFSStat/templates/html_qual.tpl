<html>
<head>
<link rel="stylesheet" type="text/css" href="results3.css">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8">
<title>{lg_qualDocTitle}</title>
</head>
<body>
<body style="background-image: url(image/background.jpg);">
[include .\include\header_qual.tpl]
<table width="100%"><tr><td align="left"><h1>{lg_qualDocTitle}</h1></td><td align="right"><div style="font-size: 7pt;"><a href="http://www.lfsforum.net/showthread.php?t=24933">{VERSIONSHORT}</a></div></td></tr><table>

<p>

<table>
<td>
<table class="tab">
<tr><td class="vth">{lg_server}</td><td class="vtb2">{ServerName}</td>
<td style="background-color: rgb(0, 0, 0);" rowspan="4"><img alt="Track Image" title="Track Image" src="tracks/{TrackImg}"></td></tr>
<tr><td class="vth">{lg_track}</td><td class="vtb">{TrackNameFull} ({TrackNameShort})</td></tr>
<tr><td class="vth">{lg_duration}</td><td class="vtb">{QualLength} min</td></tr>
<tr><td class="vth">{lg_conditions}</td><td class="vtb">{QualConditions}</td></tr>
</table>
</td>
<td>
<div align = center>
<a href="#rrs">{lg_qualres}</a>&nbsp;
<br>
<a href="#dev">{lg_stability}</a>&nbsp;
<a href="#avgl">{lg_avglap}</a>&nbsp;
<br>
<a href="#spl">{lg_bestsplits}</a>&nbsp;
<br>
<a href="#bpl">{lg_bestposlap}</a>&nbsp;
<a href="#top">{lg_topspeed}</a>&nbsp;
<br>
<a href="{linkchat}">{lg_chat}</a>&nbsp;
</div>
</td>
</table>

<p>

<a name="rrs"></a><table class="tab"><tr class="titl"><td colspan="10">{lg_qualres}</a></td></tr>
<tr class="titl2"><td>P</td><td colspan="2">{lg_racer}</td><td>{lg_car}</td><td>Plate</td><td>{lg_time}</td><td>{lg_gap}</td><td>{lg_lap}</td><td>{lg_flags}</td></tr>
[QualResults <tr class="row"><td>{Position}</td><td style="text-align:left; font-weight: bold; background: #c9c5c2;">{PlayerNameColoured}</td><td style="text-align:left"><a href="http://lfsworld.com/?win=stats&racer={UserNameLink}">{UserName}</a></td><td>{Car}</td><td>{NumberPlate}</td><td>{BestLap}</td><td>{Difference}</td><td>{BestLapLap}/{LapsDone}</td><td>{Flags}</td></tr>]
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
<a name="bpl"></a><table class="tab"><tr class="titl"><td colspan="7">{lg_bestposlap}</td></tr>
<tr class="titl2"><td>P</td><td colspan="2">{lg_racer}</td><td>{lg_time}</td><td>{lg_gap}</td><td>Best lap</td></tr>
[BestPossibleLap <tr class="row"><td>{Position}</td><td style="text-align:left;">{PlayerName}</td><td style="text-align:left">{UserName}</td><td>{LapTime}</td><td>{Difference}</td><td>{DifferenceToBestLap}</td></tr>]
</table>
&nbsp;&nbsp;<font size="-2">Combined best lap time: <strong>{CombinedBestLap}</strong></font>
</td>
<td style="vertical-align:top">
<a name="top"></a><table class="tab"><tr class="titl"><td colspan="6">{lg_topspeed}</td></tr>
<tr class="titl2"><td>P</td><td colspan="2">{lg_racer}</td><td>Speed</td><td>{lg_gap}</td><td>{lg_lap}</td></tr>
[TopSpeed <tr class="row"><td>{Position}</td><td style="text-align:left;">{PlayerName}</td><td style="text-align:left">{UserName}</td><td>{TopSpeed}</td><td>+{Difference}</td><td>{TopSpeedLap}</td></tr>]
</table>
&nbsp;&nbsp;<font size="-2">&nbsp;&nbsp;<strong>&nbsp;&nbsp;</strong></font>
</td>
</table>

<p>

[include .\include\footer_qual.tpl]

</body>
</html>
