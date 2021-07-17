﻿using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Order;
using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Helpers;
using Kucoin.NET.Websockets.Observations;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Websockets.Public
{
    /// <summary>
    /// Calibrated Level 3 Full Match Engine Feed base class and core logic implementation.
    /// </summary>
    public abstract class Level3Base<TBookOut, TUnitOut, TBookIn, TUnitIn, TUpdate, TObservation> :
        KucoinBaseWebsocketFeed
        where TBookOut : IAtomicOrderBook<TUnitOut>, new()
        where TUnitOut : IAtomicOrderUnit, new()
        where TUpdate : ILevel3Update, new()
        where TUnitIn : IAtomicOrderUnit, new()
        where TBookIn : KeyedAtomicOrderBook<TUnitIn>, new()
        where TObservation : Level3ObservationBase<TBookOut, TUnitOut, TBookIn, TUnitIn, TUpdate>
    {

        internal readonly Dictionary<string, TObservation> activeFeeds = new Dictionary<string, TObservation>();

        protected object lockObj = new object();
        protected long cycle = 0;

        protected int defaultPieces = 50;
        protected int updateInterval = 500;
        protected int updcalc = 500 * 10_000;

        protected int resets;

        protected FeedState state = FeedState.Disconnected;

        /// <summary>
        /// Get the feed subscription subject
        /// </summary>
        public abstract string Subject { get; }

        /// <summary>
        /// Get the feed subscription topic
        /// </summary>
        public abstract string Topic { get; }

        /// <summary>
        /// Get the order book retrieval end point.
        /// </summary>
        public abstract string OrderBookEndPoint { get; }

        public override bool IsPublic => false;

        /// <summary>
        /// Event that gets fired when the feed for a symbol has been calibrated and is ready to be used.
        /// </summary>
        public virtual event EventHandler<Level3SymbolCalibratedEventArgs<TBookOut, TUnitOut, TBookIn, TUnitIn, TUpdate>> SymbolCalibrated;

        public Level3Base(ICredentialsProvider credProvider) : base(credProvider)
        {
            recvBufferSize = 131072;
        }

        public Level3Base(string key, string secret, string passphrase, bool isSandbox = false, bool futures = false) : base(key, secret, passphrase, isSandbox: isSandbox, futures: futures)
        {
            recvBufferSize = 131072;
        }

        /// <summary>
        /// Gets or sets a length of time (in milliseconds) that indicates how often the order book is pushed to the UI thread.
        /// </summary>
        /// <remarks>
        /// The default value is 500 milliseconds.
        /// If this value is set to 0, automatic updates are disabled.
        /// </remarks>
        public virtual int UpdateInterval
        {
            get => updateInterval;
            set
            {
                // the minimum value is 5 milliseconds
                if (value < 5) value = 5;

                if (SetProperty(ref updateInterval, value))
                {
                    updcalc = updateInterval * 10_000;
                }
            }
        }


        /// <summary>
        /// Gets the number of resets since the last subscription.
        /// </summary>
        public int Resets
        {
            get => resets;
            internal set
            {
                SetProperty(ref resets, value);
            }
        }

        /// <summary>
        /// Gets or sets the default number of pieces for new observations.
        /// </summary>
        /// <remarks>
        /// The default value is 50 pieces.
        /// 
        /// To always include the full market depth, set this value to 0.
        /// 
        /// The value of this property only affects newly created observations.  Changing this value does not change the number of pieces
        /// in the live order books of observations that have already been created.  You can use the <see cref="ILevel2OrderBookProvider{TBook, TUnit, TUpdate}.Pieces"/> property
        /// on individual observations to change their default number of pieces.
        /// </remarks>
        public virtual int DefaultPieces
        {
            get => defaultPieces;
            set
            {
                SetProperty(ref defaultPieces, value);
            }
        }

        public FeedState State
        {
            get => state;
            set
            {
                SetProperty(ref state, value);
            }
        }

        protected override void OnConnected()
        {
            _ = Ping();
            State = FeedState.Connected;
            base.OnConnected();
        }

        protected override void OnDisconnected()
        {
            State = FeedState.Disconnected;
            base.OnDisconnected();
        }

        /// <summary>
        /// Create a new instance of a class derived from <see cref="Level2ObservationBase{TBook, TUnit, TUpdate}"/>.
        /// </summary>
        /// <param name="symbol">The trading symbol.</param>
        /// <returns>A new observation.</returns>
        protected abstract TObservation CreateNewObservation(string symbol);

        /// <summary>
        /// Adds a Level 2 subscription for the specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol to subscribe.</param>
        /// <returns></returns>
        public async Task<TObservation> AddSymbol(string symbol)
        {
            var p = await AddSymbols(new string[] { symbol });
            return p[symbol];
        }

        /// <summary>
        /// Adds a Level 2 subscription for the specified symbols.
        /// </summary>
        /// <param name="symbols">The symbols to subscribe.</param>
        /// <returns></returns>
        public virtual async Task<Dictionary<string, TObservation>> AddSymbols(IEnumerable<string> symbols)
        {
            if (disposed) throw new ObjectDisposedException(nameof(Level3Base<TBookOut, TUnitOut, TBookIn, TUnitIn, TUpdate, TObservation>));
            if (!Connected)
            {
                await Connect();
            }

            var sb = new StringBuilder();
            var lnew = new Dictionary<string, TObservation>();

            foreach (var sym in symbols)
            {
                if (activeFeeds.ContainsKey(sym))
                {
                    if (!lnew.ContainsKey(sym))
                    {
                        lnew.Add(sym, activeFeeds[sym]);
                    }
                    continue;
                }

                if (sb.Length > 0) sb.Append(',');
                sb.Append(sym);

                var obs = CreateNewObservation(sym);
                activeFeeds.Add(sym, obs);

                if (!lnew.ContainsKey(sym))
                {
                    lnew.Add(sym, activeFeeds[sym]);
                }
            }

            var topic = $"{Topic}:{sb}";

            var e = new FeedMessage()
            {
                Type = "subscribe",
                Id = connectId.ToString("d"),
                Topic = topic,
                Response = true,
                PrivateChannel = false
            };

            await Send(e);

            State = FeedState.Subscribed;
            return lnew;
        }

        /// <summary>
        /// Remove a Level 2 subscription for the specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol to remove.</param>
        /// <returns></returns>
        internal virtual async Task RemoveSymbol(string symbol)
        {
            await RemoveSymbols(new string[] { symbol });
        }

        /// <summary>
        /// Remove a Level 2 subscription for the specified symbols.
        /// </summary>
        /// <param name="symbols">The symbols to remove.</param>
        /// <returns></returns>
        internal virtual async Task RemoveSymbols(IEnumerable<string> symbols)
        {
            if (disposed) throw new ObjectDisposedException(nameof(Level3Base<TBookOut, TUnitOut, TBookIn, TUnitIn, TUpdate, TObservation>));
            if (!Connected) return;

            var sb = new StringBuilder();

            foreach (var sym in symbols)
            {
                if (activeFeeds.ContainsKey(sym))
                {
                    if (!activeFeeds[sym].Disposed)
                    {
                        activeFeeds[sym].Dispose();
                    }

                    activeFeeds.Remove(sym);
                }

                if (sb.Length > 0) sb.Append(',');
                sb.Append(sym);
            }

            var topic = $"{Topic}:{sb}";

            var e = new FeedMessage()
            {
                Type = "unsubscribe",
                Id = connectId.ToString("d"),
                Topic = topic,
                Response = true,
                PrivateChannel = false
            };
            
            await Send(e);

            if (activeFeeds.Count == 0)
            {
                cycle = 0;
                Resets = 0;
                State = FeedState.Unsubscribed;
            }
        }


        /// <summary>
        /// Get the full Level 3 Atomic Order Book for the specified trading symbol.
        /// </summary>
        /// <param name="symbol">The trading symbol.</param>
        /// <returns>The part book snapshot.</returns>
        /// <remarks>
        /// Returns the full market depth. 
        /// Use this to calibrate a full level 3 feed.
        /// </remarks>
        public virtual async Task<TBookIn> GetAtomicOrderBook(string symbol)
        {
            var curl = OrderBookEndPoint;
            var param = new Dictionary<string, object>();

            param.Add("symbol", symbol);

            var jobj = await MakeRequest(HttpMethod.Get, curl, auth: !IsPublic, reqParams: param);
            var result = jobj.ToObject<TBookIn>();


            if (typeof(ISequencedOrderUnit).IsAssignableFrom(typeof(TUnitOut)))
            {
                foreach (var ask in result.Asks)
                {
                    ((ISequencedOrderUnit)ask).Sequence = result.Sequence;
                }

                foreach (var bid in result.Bids)
                {
                    ((ISequencedOrderUnit)bid).Sequence = result.Sequence;
                }
            }

            jobj = null;
            GC.Collect(2);

            return result;
        }

        /// <summary>
        /// Initialize the order book with a call to <see cref="GetAtomicOrderBook(string)"/>.
        /// </summary>
        /// <param name="symbol">The symbol to initialize.</param>
        /// <remarks>
        /// This method is typically called after the feed has been buffered.
        /// </remarks>
        protected virtual async Task InitializeOrderBook(string symbol)
        {
            if (!activeFeeds.ContainsKey(symbol)) return;

            var af = activeFeeds[symbol];

            var data = await GetAtomicOrderBook(af.Symbol);

            af.FullDepthOrderBook = data;
            af.Initialized = true;
            Resets++;
        }

        protected override async Task HandleMessage(FeedMessage msg)
        {
            if (msg.Type == "message")
            {
                if (cycle == 0) cycle = DateTime.UtcNow.Ticks;

                var i = msg.Topic.IndexOf(":");

                if (i != -1)
                {
                    var symbol = msg.Topic.Substring(i + 1);

                    if (activeFeeds.TryGetValue(symbol, out TObservation af))
                    {
                        var update = msg.Data.ToObject<TUpdate>();
                        update.Subject = msg.Subject;

                        if (!af.Calibrated)
                        {
                            _ = Task.Run(() => State = FeedState.Initializing);

                            af.OnNext(update);

                            if ((DateTime.UtcNow.Ticks - cycle) >= (updcalc == 0 ? 600_000 : updcalc))
                            {
                                await InitializeOrderBook(af.Symbol);

                                lock (lockObj)
                                {
                                    af.Calibrate();
                                    af.RequestPush();

                                    cycle = DateTime.UtcNow.Ticks;
                                }

                                _ = Task.Run(() =>
                                {
                                    SymbolCalibrated?.Invoke(this, new Level3SymbolCalibratedEventArgs<TBookOut, TUnitOut, TBookIn, TUnitIn, TUpdate>(af));
                                    State = FeedState.Running;
                                });

                            }
                        }
                        else
                        {
                            lock (lockObj)
                            {
                                af.OnNext(update);

                                if (updateInterval == 0) return;

                                if ((DateTime.UtcNow.Ticks - cycle) >= updcalc)
                                {
                                    af.RequestPush();
                                    cycle = DateTime.UtcNow.Ticks;
                                }
                            }
                        }
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (activeFeeds != null && activeFeeds.Count > 0)
                {
                    TObservation[] feeds = new TObservation[activeFeeds.Count];
                    activeFeeds.Values.CopyTo(feeds, 0);

                    foreach (var feed in feeds)
                    {
                        feed.Dispose();
                    }

                }
            }

            base.Dispose(disposing);
        }
    }
}
