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
using System.Drawing;

namespace Graph
{
	/// <summary>
	/// Summary description for Colors.
	/// </summary>
	internal class Colors
	{
		private static Color[] colors = 
			{
				Color.Red,
				Color.Aqua,
				Color.MediumBlue,
				Color.Orange,
				Color.Plum,
				Color.Tan,
				Color.Chartreuse,
				Color.Violet,
				Color.Firebrick,
				Color.SkyBlue,
				Color.Silver,
				Color.YellowGreen,
				Color.Chocolate,
				Color.LightSeaGreen,
				Color.Gold,
				Color.Black,
				Color.Lavender,
				Color.Beige,
				Color.LightBlue,
				Color.ForestGreen
			};

		public static Color GetColor(int colorN)
		{
			if (colorN >= 0 && colorN < colors.GetLength(0))
				return colors[colorN];
			else
				return Color.Black;
		}
	}
}
