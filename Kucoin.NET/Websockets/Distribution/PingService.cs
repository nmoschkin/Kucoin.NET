
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace Kucoin.NET.Websockets.Distribution
{

    public static class PingService
    {

        private static readonly List<IPingable> feeds = new List<IPingable>();

        private static Thread PingerThread;

        private static CancellationTokenSource cts;

        static PingService()
        {
        }
        
        public static void RegisterService(IPingable feed)
        {
            if (!feeds.Contains(feed))
            {
                int x = feed.Interval;

                if (x % 10 != 0)
                {
                    x = x - (x % 10);
                }

                feed.Interval = x;
                if (x == 0) throw new ArgumentOutOfRangeException("Interval cannot be 0.");
                feeds.Add(feed);    
            }

            if (PingerThread == null)
            {
                cts = new CancellationTokenSource();    

                PingerThread = new Thread(PingerMethod);
                PingerThread.IsBackground = true;   
                PingerThread.Start();   
            }
        }

        public static void CancelPingerThread()
        {
            cts?.Cancel();

            PingerThread = null;
            cts = null;
        }

        public static void UnregisterService(IPingable feed)
        {
            if (feeds.Contains(feed))
            {
                feeds.Remove(feed); 
            }

            if (feeds.Count == 0)
            {
                CancelPingerThread();
            }
        }

        private static void PingerMethod()
        {            
            while (!cts.IsCancellationRequested)
            {
                try
                {
                    if (feeds.Count == 0)
                    {
                        return;
                    }

                    long pingMax = 0;

                    foreach (var feed in feeds)
                    {
                        if (feed.Interval > pingMax)
                        {
                            pingMax = feed.Interval;
                        }
                    }

                    for (int i = 0; i < pingMax; i += 10)
                    {
                        Task.Delay(10, cts.Token).ConfigureAwait(false).GetAwaiter().GetResult(); 
                        if (cts.IsCancellationRequested) return;

                        foreach (var feed in feeds)
                        {
                            if (cts.IsCancellationRequested) return;

                            if (i == feed.Interval)
                            {
                                if (feed is IAsyncPingable aping)
                                {
                                    _ = aping.Ping();
                                }
                                else
                                {
                                    feed.Ping();
                                }
                            }
                        }
                    }

                    if (cts.IsCancellationRequested) return;
                }
                catch //(Exception ex)
                {

                    return;
                }

            }
        }
        

    }
}
