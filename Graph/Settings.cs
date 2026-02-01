/*
    LFSStat, Insim Replay statistics for Live For Speed Game
    Copyright (C) 2008 Jaroslav Èerný alias JackCY, Robert B. alias Gai-Luron and Monkster.
    Jack.SC7@gmail.com, lfsgailuron@free.fr

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
/*
 * Based on Graph v1.20 for LFS stats! (c) Alexander 'smith' Rudakov (piercemind@gmail.com)
 */
using System;
using System.Xml;


namespace Graph
{
	/// <summary>
	/// Summary description for Settings.
	/// </summary>
	internal class Settings
	{
		public static System.Drawing.Font
			commonFont = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
		public static System.Drawing.Font
			titleFont = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
		public static System.Drawing.Font
			lapTimesFont = new System.Drawing.Font("Verdana", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
		public static NPlot.LegendBase.BorderType
			legendBorderType = NPlot.LegendBase.BorderType.Line;
		public static string TSVInputDirectory = ".";
		public static string GraphOutputDirectory = "graph/";
		public static string LapByLapGraphTitle = "Lap by lap graph";
		public static string LapByLapGraphXAxisLabel = "Lap";
		public static string LapByLapGraphYAxisLabel = "Position";
		public static bool LapByLapGraphDisplayPositions = true;
		public static string RaceProgressGraphTitle = "Race progress graph";
		public static string RaceProgressGraphXAxisLabel = "Lap";
		public static string RaceProgressGraphYAxisLabel = "Difference, seconds";
		public static string LapTimesYAxisLabel = "Lap time, seconds";
		public static bool LimitLapTimes = true;
		public static bool OutputLapTimes = true;
		public static double LimitMultiplier = 2.0;
		public static bool LimitToGlobalBestLap = true;

		public static int graphWidth = 960;
		public static int graphHeight = 600;
		public static int graphMinMax = 300;

		private static string GetConfigValue(XmlDocument doc, string path)
		{
			string result = "";
			XmlNode child = doc.SelectSingleNode(path);
			if (child != null)
			{
				XmlNodeReader nr = new XmlNodeReader(child);
				while (nr.Read())
					//					if (nr.Value != "")
					if (!String.IsNullOrEmpty(nr.Value))
					{
						String delimeters = "\r\n ";
						String outline = nr.Value;
						outline = outline.Trim(delimeters.ToCharArray());
						//						if (outline != "")
						if (!String.IsNullOrEmpty(outline))
							result += outline;
					}
			}

			return result;
		}

		private static void GetConfigString(XmlDocument doc, string key, ref string keyvalue)
		{
			string tmpvalue = GetConfigValue(doc, key);
			if (!String.IsNullOrEmpty(tmpvalue))
				keyvalue = tmpvalue;
		}

		private static void GetConfigInt(XmlDocument doc, string key, ref int keyvalue)
		{
			string tmpvalue = GetConfigValue(doc, key);
			//			if (tmpvalue != "")
			if (!String.IsNullOrEmpty(tmpvalue))
				try
				{
					//					keyvalue = int.Parse(tmpvalue);
					keyvalue = System.Int32.Parse(tmpvalue);
				}
				catch
				{
					System.Console.WriteLine(key + " must be an positive integer value");
				}
		}

		private static void GetConfigDouble(XmlDocument doc, string key, ref double keyvalue)
		{
			string tmpvalue = GetConfigValue(doc, key);
			if (!String.IsNullOrEmpty(tmpvalue))
				try
				{
					tmpvalue = tmpvalue.Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);
					tmpvalue = tmpvalue.Replace(",", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);
					keyvalue = System.Double.Parse(tmpvalue);
				}
				catch (Exception e)
				{
					System.Console.WriteLine(key + " must be a positive double value (" + e.ToString() + ")");
				}
		}


		private static void GetConfigBool(XmlDocument doc, string key, ref bool keyvalue)
		{
			string tmpvalue = GetConfigValue(doc, key);
			if (!String.IsNullOrEmpty(tmpvalue))
				try
				{
					keyvalue = bool.Parse(tmpvalue);
				}
				catch
				{
					System.Console.WriteLine(key + " value must be true/false only");
				}

		}

		public static bool ReadSettings(string fileName)
		{
			XmlDocument doc = new XmlDocument();

			if (!System.IO.File.Exists(fileName))
			{
				System.Console.WriteLine("Can't open " + fileName + "!");
				return false;
			}
			else
			{
				try
				{
					doc.Load(fileName);
				}
				catch (System.Xml.XmlException ex)
				{
					System.Console.WriteLine(ex.Message);
					return false;
				}

				GetConfigString(doc, "GraphConfiguration/TSVInputDirectory", ref Settings.TSVInputDirectory);
				GetConfigString(doc, "GraphConfiguration/GraphOutputDirectory", ref Settings.GraphOutputDirectory);
				GetConfigInt(doc, "GraphConfiguration/GraphWidth", ref Settings.graphWidth);
				GetConfigInt(doc, "GraphConfiguration/GraphHeight", ref Settings.graphHeight);
				GetConfigInt(doc, "GraphConfiguration/RaceProgressGraphMinValue", ref Settings.graphMinMax);
				GetConfigString(doc, "GraphConfiguration/RaceProgressGraphTitle", ref Settings.RaceProgressGraphTitle);
				GetConfigString(doc, "GraphConfiguration/RaceProgressGraphXAxisLabel", ref Settings.RaceProgressGraphXAxisLabel);
				GetConfigString(doc, "GraphConfiguration/RaceProgressGraphYAxisLabel", ref Settings.RaceProgressGraphYAxisLabel);
				GetConfigString(doc, "GraphConfiguration/LapByLapGraphXAxisLabel", ref Settings.LapByLapGraphXAxisLabel);
				GetConfigString(doc, "GraphConfiguration/LapByLapGraphYAxisLabel", ref Settings.LapByLapGraphYAxisLabel);
				GetConfigBool(doc, "GraphConfiguration/LapByLapGraphDisplayPositions", ref Settings.LapByLapGraphDisplayPositions);
				GetConfigString(doc, "GraphConfiguration/LapTimesYAxisLabel", ref Settings.LapTimesYAxisLabel);
				GetConfigBool(doc, "GraphConfiguration/LimitLapTimes", ref Settings.LimitLapTimes);
				GetConfigBool(doc, "GraphConfiguration/OutputLapTimes", ref Settings.OutputLapTimes);
				GetConfigDouble(doc, "GraphConfiguration/LimitMultiplier", ref Settings.LimitMultiplier);
				GetConfigBool(doc, "GraphConfiguration/LimitToGlobalBestLap", ref Settings.LimitToGlobalBestLap);
				return true;
			}
		}
	}
}
