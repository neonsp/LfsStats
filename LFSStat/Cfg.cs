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
namespace LFS
{
	class Configurator
	{
		System.Collections.Hashtable ht = new System.Collections.Hashtable();

        /// <summary>
        /// Load pairs of keys and values from file
        /// </summary>
        /// <param name="filepath">Path and name of file.</param>
        /// <returns></returns>
		public bool Load(string filepath)
		{
			using (System.IO.StreamReader sr = new System.IO.StreamReader(filepath))
			{

                int linecounter = 0;

                while(true)
				{
                    string line = sr.ReadLine();
                    linecounter++;
						
					if(line == null)
						return true;

                    while (sr.Peek() == '\t')
                    {
                        line += sr.ReadLine().Remove(0,1);
                        linecounter++;
                    }
                    
					if(line.Length == 0)
						continue;

					if(line[0] == '#') //skip comments
						continue;

                    string key, val;
                    int index = line.IndexOf('='); //look for first "="
                    if (index == -1)
                        throw new System.Exception(string.Format("Corrupted line #{2} ('{1}') in file {0} (can not find '=' symbol)",filepath,line,linecounter));

                    key = line.Substring(0, index);
                    val = line.Substring(index + 1);
                    System.Diagnostics.Debug.WriteLine(string.Format("{0} = {1}", key, val));

                    ht[key.Trim()] = val.Trim();
				}
			}
		}

        /// <summary>
        /// Retrieves value of given key.
        /// </summary>
        /// <param name="Key">Key.</param>
        /// <returns>Value of Key.</returns>
		public string Get(string Key)
		{
			if(ht.ContainsKey(Key))
				return ht[Key] as string;
			else
				return "";
		}
	}
}