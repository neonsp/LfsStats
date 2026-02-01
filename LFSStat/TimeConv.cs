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
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace LFSStat
{
    class timeConv
    {
        public static bool isTime( string strTime ){
            Regex regTime = new Regex( "^[0-9]{1}\\.[0-9]{2}\\.[0-9]{2}" );

            return regTime.IsMatch(strTime);

        }
        public static long HMSToLong(int m, int s, int d)
        {
            return (long)((m * 60 * 100) + s * 100 + d ) * 10;
        }
        public static long HMSToLong(string time)
        {
            try
            {
                string[] sp = time.Split('.');
                int m = Int32.Parse(sp[0]);
                int s = Int32.Parse(sp[1]);
                int d = Int32.Parse(sp[2]);
                return HMSToLong(m, s, d);
            }
            catch
            {
                return HMSToLong(0, 0, 0);
            }
        }
        public static string LongToHMS(long val)
        {
            long sec;
            long min;
            long hun;
            string sign = "";

            if (val < 0)
            {
                val = -val;
                sign = "-";
            }
            sec = val / 1000;
            hun = sec * 1000;
            hun = (val - hun) / 10;

            val = sec;
            min = val / 60;
            sec = min * 60;
            sec = val - sec;
            return string.Format("{3}{0}.{1,2:D2}.{2,2:D2}", min, sec, hun, sign);
        }


    }
}
