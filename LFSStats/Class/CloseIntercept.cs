using System;

namespace LFSStatistics
{
    /// <summary>
    /// Cross-platform handler for console close/cancel events (Ctrl+C, Ctrl+Break).
    /// Replaces the previous Win32 P/Invoke implementation for .NET Core compatibility.
    /// </summary>
    public class ConsoleCtrl : IDisposable
    {
        public enum ConsoleEvent
        {
            CtrlC = 0,
            CtrlBreak = 1,
            CtrlClose = 2,
            CtrlLogoff = 5,
            CtrlShutdown = 6
        }

        public delegate void ControlEventHandler(ConsoleEvent consoleEvent);

        public event ControlEventHandler ControlEvent;

        public ConsoleCtrl()
        {
            Console.CancelKeyPress += OnCancelKeyPress;
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        }

        ~ConsoleCtrl()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            Console.CancelKeyPress -= OnCancelKeyPress;
            AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
        }

        private void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true; // Prevent immediate termination
            ControlEvent?.Invoke(ConsoleEvent.CtrlC);
        }

        private void OnProcessExit(object sender, EventArgs e)
        {
            ControlEvent?.Invoke(ConsoleEvent.CtrlClose);
        }
    }
}
