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
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Xml;
using static NPlot.LegendBase;

namespace Graph
{
    /// <summary>
    /// Summary description for Settings.
    /// </summary>
    internal class Settings
    {
        public static Font commonFont = new Font("Verdana", 10f, FontStyle.Regular, GraphicsUnit.Pixel);

        public static Font titleFont = new Font("Verdana", 12f, FontStyle.Bold, GraphicsUnit.Pixel);

        public static Font lapTimesFont = new Font("Verdana", 11f, FontStyle.Bold, GraphicsUnit.Pixel);

        public static BorderType legendBorderType = (BorderType)1;

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

        public static bool OutputLapTimes = false;

        public static double LimitMultiplier = 2.0;

        public static bool LimitToGlobalBestLap = true;

        public static int graphWidth = 800;

        public static int graphHeight = 600;

        public static int graphMinMax = 300;

        private static string GetConfigValue(XmlDocument doc, string path)
        {
            string text = "";
            XmlNode xmlNode = doc.SelectSingleNode(path);
            if (xmlNode != null)
            {
                XmlNodeReader xmlNodeReader = new XmlNodeReader(xmlNode);
                while (xmlNodeReader.Read())
                {
                    if (!string.IsNullOrEmpty(xmlNodeReader.Value))
                    {
                        string text2 = "\r\n ";
                        string value = xmlNodeReader.Value;
                        value = value.Trim(text2.ToCharArray());
                        if (!string.IsNullOrEmpty(value))
                        {
                            text += value;
                        }
                    }
                }
            }

            return text;
        }

        private static void GetConfigString(XmlDocument doc, string key, ref string keyvalue)
        {
            string configValue = GetConfigValue(doc, key);
            if (!string.IsNullOrEmpty(configValue))
            {
                keyvalue = configValue;
            }
        }

        private static void GetConfigInt(XmlDocument doc, string key, ref int keyvalue)
        {
            string configValue = GetConfigValue(doc, key);
            if (!string.IsNullOrEmpty(configValue))
            {
                try
                {
                    keyvalue = int.Parse(configValue);
                }
                catch
                {
                    Console.WriteLine(key + " must be an positive integer value");
                }
            }
        }

        private static void GetConfigDouble(XmlDocument doc, string key, ref double keyvalue)
        {
            string configValue = GetConfigValue(doc, key);
            if (!string.IsNullOrEmpty(configValue))
            {
                try
                {
                    configValue = configValue.Replace(".", NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);
                    configValue = configValue.Replace(",", NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);
                    keyvalue = double.Parse(configValue);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(key + " must be a positive double value (" + ex.ToString() + ")");
                }
            }
        }

        private static void GetConfigBool(XmlDocument doc, string key, ref bool keyvalue)
        {
            string configValue = GetConfigValue(doc, key);
            if (!string.IsNullOrEmpty(configValue))
            {
                try
                {
                    keyvalue = bool.Parse(configValue);
                }
                catch
                {
                    Console.WriteLine(key + " value must be true/false only");
                }
            }
        }

        public static bool ReadSettings(string fileName)
        {
            XmlDocument xmlDocument = new XmlDocument();
            if (!File.Exists(fileName))
            {
                Console.WriteLine("Can't open " + fileName + "!");
                return false;
            }

            try
            {
                xmlDocument.Load(fileName);
            }
            catch (XmlException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            GetConfigString(xmlDocument, "GraphConfiguration/TSVInputDirectory", ref TSVInputDirectory);
            GetConfigString(xmlDocument, "GraphConfiguration/GraphOutputDirectory", ref GraphOutputDirectory);
            GetConfigInt(xmlDocument, "GraphConfiguration/GraphWidth", ref graphWidth);
            GetConfigInt(xmlDocument, "GraphConfiguration/GraphHeight", ref graphHeight);
            GetConfigInt(xmlDocument, "GraphConfiguration/RaceProgressGraphMinValue", ref graphMinMax);
            GetConfigString(xmlDocument, "GraphConfiguration/RaceProgressGraphTitle", ref RaceProgressGraphTitle);
            GetConfigString(xmlDocument, "GraphConfiguration/RaceProgressGraphXAxisLabel", ref RaceProgressGraphXAxisLabel);
            GetConfigString(xmlDocument, "GraphConfiguration/RaceProgressGraphYAxisLabel", ref RaceProgressGraphYAxisLabel);
            GetConfigString(xmlDocument, "GraphConfiguration/LapByLapGraphXAxisLabel", ref LapByLapGraphXAxisLabel);
            GetConfigString(xmlDocument, "GraphConfiguration/LapByLapGraphYAxisLabel", ref LapByLapGraphYAxisLabel);
            GetConfigBool(xmlDocument, "GraphConfiguration/LapByLapGraphDisplayPositions", ref LapByLapGraphDisplayPositions);
            GetConfigString(xmlDocument, "GraphConfiguration/LapTimesYAxisLabel", ref LapTimesYAxisLabel);
            GetConfigBool(xmlDocument, "GraphConfiguration/LimitLapTimes", ref LimitLapTimes);
            GetConfigBool(xmlDocument, "GraphConfiguration/OutputLapTimes", ref OutputLapTimes);
            GetConfigDouble(xmlDocument, "GraphConfiguration/LimitMultiplier", ref LimitMultiplier);
            GetConfigBool(xmlDocument, "GraphConfiguration/LimitToGlobalBestLap", ref LimitToGlobalBestLap);
            return true;
        }
    }
}
