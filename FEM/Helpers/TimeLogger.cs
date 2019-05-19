using System;
using System.Diagnostics;

namespace FEM.Helpers
{
    class TimeLogger
    {
        private Stopwatch timer;
        public TimeLogger()
        {
            timer = new Stopwatch();
            timer.Start();
        }

        public void LogTime (string message) {
            timer.Stop();

            TimeSpan ts = timer.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            Console.WriteLine("TIMER: " + message + " in " + elapsedTime);

            timer.Reset();
            timer.Start();
        }
    }
}
