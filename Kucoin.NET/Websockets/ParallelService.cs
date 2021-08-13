﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kucoin.NET.Websockets
{


    public interface IDistributable // : IComparable<IDistributable>
    {
        /// <summary>
        /// Do the work for this tenant.
        /// </summary>
        /// <returns>True if work was done, otherwise false.</returns>
        bool DoWork();

        object LockObject { get; }

    }

    public static class ParallelService
    {
        private class Distribution : IDisposable
        {
            bool disposed;
            
            private CancellationTokenSource cts = new CancellationTokenSource();

            public List<IDistributable> Tenants { get; } = new List<IDistributable>();

            public Thread Thread { get; private set; }

            public void Dispose()
            {
                if (disposed) throw new ObjectDisposedException(GetType().FullName);

                cts?.Cancel();
                Thread = null;
                cts = null;
                disposed = true;
            }

            public Distribution()
            {
                Thread = new Thread(ThreadMethod);
                Thread.IsBackground = true;
            }

            private void ThreadMethod()
            {
                var actions = new List<Action>();
                var x = new List<bool>();

                while (!cts.IsCancellationRequested)
                {
                    int i = 0;

                    lock (Tenants)
                    {
                        foreach (var t in Tenants)
                        {
                            actions.Add(new Action(() =>
                            {
                                t.DoWork();
                                i++;
                            }));
                        }
                    }

                    Parallel.Invoke(actions.ToArray());

                    Thread.Sleep((i == 0 ? 1 : i) * IdleSleepTime);
                }
            }

            public Distribution(IDistributable tenant) : this()
            {
                Tenants.Add(tenant);
            }
        }

        private static object lockObj = new object();

        private static readonly List<Distribution> distributors = new List<Distribution>();

        private static int maxTenants = 4;

        private static int idleSleepTime = 1;
        
        /// <summary>
        /// Gets or sets the maximum number of feeds per thread.
        /// </summary>
        /// <remarks>
        /// Must be a value between 1 and 255.
        /// </remarks>
        public static int MaxTenants
        {
            get => maxTenants;
            set
            {
                RedistributeServices(value);
            }
        }

        /// <summary>
        /// Gets or sets the global idle sleep time in milliseconds.
        /// </summary>
        /// <remarks>
        /// Must be a value between 1 and 100.
        /// </remarks>
        public static int IdleSleepTime
        {
            get => idleSleepTime;
            set
            {
                if (value < 1 || value > 100) throw new ArgumentOutOfRangeException();
                idleSleepTime = value;
            }
        }

        /// <summary>
        /// Register an instance of a distributable service
        /// </summary>
        /// <param name="feed"></param>
        public static void RegisterService(IDistributable feed)
        {
            lock (lockObj)
            {
                if (distributors.Where((a) => a.Tenants.Where((b) => b.Equals(feed)).Count() != 0).Count() == 0)
                {
                    var insertFeed = distributors.Where((a) => a.Tenants.Count < MaxTenants).FirstOrDefault();

                    if (insertFeed == default)
                    {
                        insertFeed = new Distribution(feed);
                        insertFeed.Thread.Start();

                        distributors.Add(insertFeed);
                    }
                    else
                    {
                        lock(insertFeed.Tenants)
                        {
                            insertFeed.Tenants.Add(feed);
                        }
                    }

                }
            }
        }

        public static void RedistributeServices(int distribution)
        {
            lock (lockObj)
            {
                if (distribution < 1 || distribution > 255) throw new ArgumentOutOfRangeException(nameof(distribution) + " must be 1 to 255");

                var allfeeds = from dist in distributors from feed in dist.Tenants select feed;

                foreach (var dist in distributors)
                {
                    dist.Dispose();
                }

                distributors.Clear();
                maxTenants = distribution;

                foreach (var feed in allfeeds)
                {
                    Monitor.Enter(feed.LockObject);
                }

                int x = 0;

                foreach (var feed in allfeeds)
                {
                    RegisterService(feed);
                }

                foreach (var feed in allfeeds)
                {
                    Monitor.Exit(feed.LockObject);
                }

            }

        }


        public static void UnregisterService(IDistributable feed)
        {
            lock (lockObj)
            {
                var distributor = distributors.Where((a) => a.Tenants.Contains(feed)).FirstOrDefault();

                lock(distributor.Tenants)
                {
                    distributor.Tenants.Remove(feed);

                    if (distributor.Tenants.Count == 0)
                    {
                        distributor.Dispose();
                        distributors.Remove(distributor);
                    }
                }
            }
        }


    }
}
