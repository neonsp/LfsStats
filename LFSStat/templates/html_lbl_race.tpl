<html>
<head>
<link rel="stylesheet" type="text/css" href="results3.css">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8">
<title>{lg_reslbl}</title>
</head>
<body>
<body>
[include .\include\header_lbl_race.tpl]
<table width="100%">
  <tbody>
    <tr>
      <td align="left">
      <h1>{lg_reslbl}</h1>
      </td>
      <td align="right">
      <div style="font-size: 7pt;"><a href="http://www.lfsforum.net/showthread.php?t=24933">{VERSIONSHORT}</a></div>
      </td>
    </tr>
  </tbody>
</table>
<br>
<table class='tab' width=100%>
<tr class='row'>
[Percent <td style='text-align: center; background-color:{percbackcolor}'>{Percent} % </td>]
</tr></table><br>
<table class="tab">
	<tbody>
    	<tr>
     	 <td class="vth">{lg_bestlap}</td>
      	<td class="vtb">
      	{Bestlap} ( {Racerbestlap} )<BR>
		</td>
   		</tr>
	</tbody>
</table>
<table class='tab' width=100%>
<tr  class='titl'>
<td></td>
[Headpos <td>{Pos}</td>]
</tr>
<tr class='titl'>
<td>{lg_lap}</td>
[Headracer <td>{Racer}</td>]
[[Resultracer <tr class='row'>
</tr>
<td class='titl' style='text-align: center;'>{lap}</td>
[Resultline <td style='background-color:{Bgcolor};text-align: center;'>{Ltime}</td>]
</tr>
]]
<tr class='titl'>
<th>Total</th>
[Total 	<th style='text-align: center;vertical-align: top'><small>{Gap}</small><BR><small><small>{Ttime}</small></small><BR><small><small>{Penalty}</small></small><BR></th>]
</tr>
</table>

<p>

[include .\include\footer_lbl_race.tpl]

</body>
</html>
