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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace LFSStat
{
/// <summary>
/// Class to catch console control events (ie CTRL-C) in C#.
/// Calls SetConsoleCtrlHandler() in Win32 API
/// </summary>
public class ConsoleCtrl: IDisposable
{
/// <summary>
/// The event that occurred.
/// </summary>
public enum ConsoleEvent
{
CtrlC = 0,CtrlBreak = 1,CtrlClose = 2,CtrlLogoff = 5,CtrlShutdown = 6
}

/// <summary>
/// Handler to be called when a console event occurs.
/// </summary>
public delegate void ControlEventHandler(ConsoleEvent consoleEvent);

/// <summary>
/// Event fired when a console event occurs
/// </summary>
public event ControlEventHandler ControlEvent;

ControlEventHandler eventHandler;

public ConsoleCtrl()
{
// save this to a private var so the GC doesn't collect it...
eventHandler = new ControlEventHandler(Handler);
SetConsoleCtrlHandler(eventHandler, true);
}

~ConsoleCtrl()
{Dispose(false);}

public void Dispose()
{
Dispose(true);
GC.SuppressFinalize(this);
}

void Dispose(bool disposing)
{
 if (eventHandler != null)
 {
  SetConsoleCtrlHandler(eventHandler, false);
  eventHandler = null;
 }
}

private void Handler(ConsoleEvent consoleEvent)
{
if (ControlEvent != null)
 ControlEvent(consoleEvent);
}

[DllImport("kernel32.dll")]
static extern bool SetConsoleCtrlHandler(ControlEventHandler e, bool add);
}

}