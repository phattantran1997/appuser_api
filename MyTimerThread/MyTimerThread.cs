using System;
using System.Threading;

namespace WebService.MyTimerThread
{
    public class MyTimerThread
    {
        private Thread threadTimer = null;
        private bool isStart = false;
        private int timeInterval = 1000;
        private int timeIntervalRestore = 1000;
        private Action callback = null;
        private Object lockObj = new Object();

        public MyTimerThread(Action callback, int timeInterval, int timeIntervalRestore)
        {
            this.timeInterval = timeInterval;
            this.timeIntervalRestore = timeIntervalRestore;
            this.callback = callback;
        }

        public void StartTimerThread()
        {
            lock (lockObj)
            {
                StopTimerThread();
                try
                {
                    isStart = true;
                    threadTimer = new Thread(TimerThreadThreadProc);
                    threadTimer.Start();
                }
                catch { }
            }
        }

        public void StopTimerThread()
        {
            lock (lockObj)
            {
                if (threadTimer != null)
                {
                    try
                    {
                        isStart = false;
                        threadTimer.Interrupt();
                        threadTimer.Join();
                    }
                    catch { }
                    finally
                    {
                        threadTimer = null;
                    }
                }
            }
        }

        private void TimerThreadThreadProc()
        {
            try
            {
                while (isStart)
                {
                    Thread.Sleep(timeInterval);
                    callback();
                }
            }
            catch { }
            finally
            {
                if (isStart)
                {
                    Thread.Sleep(timeIntervalRestore);
                    StartTimerThread();
                }
            }
        }

        public int GetTimeInterval()
        {
            return timeInterval;
        }
    }
}
