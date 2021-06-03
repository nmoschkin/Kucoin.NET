﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Observable;

using System.Text;
using System.ComponentModel;
using System.Threading;
using Kucoin.NET.Helpers;
using System.Linq;
using System.Threading.Tasks;
using Kucoin.NET.Websockets.Public;
using System.Runtime.CompilerServices;

namespace Kucoin.NET.Websockets.Observations
{
    /// <summary>
    /// Standard Spot Market Level 2 observation and order book provider.
    /// </summary>
    public class Level2Observation : Level2ObservationBase<OrderBook<OrderUnit>, OrderUnit, Level2Update>, ILevel2OrderBookProvider
    {

        protected bool calibrated;
        protected bool initialized;

        protected List<Level2Update> orderBuffer = new List<Level2Update>();
        internal Level2Observation(KucoinBaseWebsocketFeed parent, string symbol, int pieces = 50) : base(parent, symbol, pieces)
        {
        }

        public override void RequestPush()
        {
            lock (lockObj)
            {
                pushRequested = true;
            }
        }

        /// <summary>
        /// Gets a value indicating that this order book is initialized with the full-depth (preflight) order book.
        /// </summary>
        public override bool Initialized
        {
            get => !disposed ? initialized : throw new ObjectDisposedException(nameof(Level2Observation));
            internal set
            {
                if (disposed) throw new ObjectDisposedException(nameof(Level2Observation));
                SetProperty(ref initialized, value);
            }
        }

        /// <summary>
        /// Gets a value indicating that this observation has been calibrated.
        /// </summary>
        public override bool Calibrated
        {
            get => !disposed ? calibrated : throw new ObjectDisposedException(nameof(Level2Observation));
            protected set
            {
                if (disposed) throw new ObjectDisposedException(nameof(Level2Observation));
                SetProperty(ref calibrated, value);
            }
        }

        /// <summary>
        /// Remove the piece from the order book.
        /// </summary>
        /// <param name="pieces"></param>
        /// <param name="price"></param>
        protected void RemovePiece(IList<IOrderUnit> pieces, decimal price)
        {
            int i, c = pieces.Count;

            for (i = 0; i < c; i++)
            {
                if (pieces[i].Price == price)
                {
                    pieces.RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>
        /// Sequence the changes into the order book.
        /// </summary>
        /// <param name="changes">The changes to sequence.</param>
        /// <param name="pieces">The collection to change (either an ask or a bid collection)</param>
        protected void SequencePieces(IList<OrderUnit> changes, KeyedCollection<decimal, OrderUnit> pieces)
        {
            foreach (var change in changes)
            {
                decimal cp = change.Price;

                if (change.Size == 0.0M)
                {
                    pieces.Remove(cp);
                }
                else
                {
                    if (pieces.Contains(cp))
                    {
                        var piece = pieces[cp];

                        piece.Size = change.Size;
                        if (piece is ISequencedOrderUnit seqpiece && change is ISequencedOrderUnit seqchange)
                            seqpiece.Sequence = seqchange.Sequence;
                    }
                    else
                    {
                        pieces.Add(change);
                    }
                }
            }
        }

        /// <summary>
        /// De-initialize and clear all data and begin a new calibration.
        /// 
        /// </summary>
        public override void Reset()
        {
            orderBuffer = new List<Level2Update>();

            Initialized = false;
            Calibrated = false;
        }

        bool bf = false;

        /// <summary>
        /// Calibrate the order book from cached data.
        /// </summary>
        public override void Calibrate()
        {
            calibrated = true;

            bf = true;
            foreach (var q in orderBuffer)
            {
                if (q.SequenceStart > fullDepth.Sequence) OnNext(q);
            }
            bf = false;

            orderBuffer.Clear();

            OnPropertyChanged(nameof(Calibrated));
        }

        /// <summary>
        /// <see cref="IObserver{T}"/> implementation for <see cref="Level2Update"/> data.
        /// </summary>
        /// <param name="value"></param>
        public override void OnNext(Level2Update value)
        {
            if (disposed) return;

            lock (lockObj)
            {
                try
                {
                    if (!calibrated)
                    {
                        orderBuffer.Add(value);
                        return;
                    }
                    else if (value.SequenceEnd <= fullDepth.Sequence)
                    {
                        return;
                    }
                    else if (fullDepth == null)
                    {
                        return;
                    }

                    SequencePieces(value.Changes.Asks, fullDepth.Asks);
                    SequencePieces(value.Changes.Bids, fullDepth.Bids);

                    fullDepth.Sequence = value.SequenceEnd;
                    fullDepth.Timestamp = DateTime.Now;
                }
                catch (Exception ex)
                {
                    string e = ex.Message;
                }
            }
        }

        /// <summary>
        /// Copy the changes from a preflight source to a live destination.
        /// </summary>
        /// <param name="src">Source data.</param>
        /// <param name="dest">Destination collection.</param>
        /// <param name="pieces">The number of pieces to copy.</param>
        protected void CopyTo(IList<OrderUnit> src, IList<OrderUnit> dest, int pieces)
        {
            int i, c = pieces < src.Count ? pieces : src.Count;
            int x = dest.Count;

            if (x != c)
            {
                x = 0;
                dest.Clear();

                foreach (var piece in src)
                {
                    dest.Add(piece);
                    if (++x == c) break;
                }
            }
            else
            {
                for (i = 0; i < c; i++)
                {
                    dest[i] = src[i];
                }
            }

        }

        /// <summary>
        /// Push the preflight book to the live feed.
        /// </summary>
        protected override void PushLive()
        {
            lock (lockObj)
            {
                if (orderBook == null)
                {
                    var ob = new OrderBook<OrderUnit>();
                    OrderBook = ob;
                }

                if (Dispatcher.Initialized)
                {
                    Dispatcher.InvokeOnMainThread((o) =>
                    {
                        orderBook.Sequence = fullDepth.Sequence;
                        orderBook.Timestamp = fullDepth.Timestamp;

                        CopyBook();
                    });
                }
                else
                {
                    orderBook.Sequence = fullDepth.Sequence;
                    orderBook.Timestamp = fullDepth.Timestamp;

                    CopyBook();
                }
            }
        }

        protected void CopyBook()
        {
            CopyTo(fullDepth.Asks, orderBook.Asks, pieces);
            CopyTo(fullDepth.Bids, orderBook.Bids, pieces);
        }

        #region IDisposable pattern

        protected override void Dispose(bool disposing)
        {
            if (disposed) return;

            disposed = true;

            cts?.Cancel();
            PushThread.Dispose();
            PushThread = null;

            if (disposing)
            {
                if (connectedFeed != null)
                {
                    ((Level2)connectedFeed).RemoveSymbol(symbol).Wait();
                }
            }

            lock (lockObj)
            {
                connectedFeed = null;
                fullDepth = default;
                orderBook = default;
                pieces = 0;
                symbol = null;
                calibrated = false;
                initialized = false;
            }
        }

        public override void OnCompleted()
        {
        }

        public override void OnError(Exception error)
        {
            throw error;
        }

        ~Level2Observation()
        {
            Dispose(false);
        }

        #endregion

    }

    /// <summary>
    /// Custom level 2 observation for custom Level 2 feed implementations.
    /// </summary>
    /// <typeparam name="TBook">The type of your custom order book.</typeparam>
    /// <typeparam name="TUnit">The type of your custom order pieces.</typeparam>
    public class CustomLevel2Observation<TBook, TUnit> : Level2ObservationBase<TBook, TUnit, Level2Update>
        where TBook : IOrderBook<TUnit>, new()
        where TUnit : IOrderUnit, new()
    {

        protected bool calibrated;
        protected bool initialized;

        protected List<Level2Update> orderBuffer = new List<Level2Update>();

        public CustomLevel2Observation(KucoinBaseWebsocketFeed parent, string symbol, int pieces = 50) : base(parent, symbol, pieces)
        {
        }

        public override void RequestPush()
        {
            lock (lockObj)
            {
                pushRequested = true;
            }
        }

        /// <summary>
        /// Gets a value indicating that this order book is initialized with the full-depth (preflight) order book.
        /// </summary>
        public override bool Initialized
        {
            get => !disposed ? initialized : throw new ObjectDisposedException(nameof(Level2Observation));
            internal set
            {
                if (disposed) throw new ObjectDisposedException(nameof(Level2Observation));
                SetProperty(ref initialized, value);
            }
        }

        /// <summary>
        /// Gets a value indicating that this observation has been calibrated.
        /// </summary>
        public override bool Calibrated
        {
            get => !disposed ? calibrated : throw new ObjectDisposedException(nameof(Level2Observation));
            protected set
            {
                if (disposed) throw new ObjectDisposedException(nameof(Level2Observation));
                SetProperty(ref calibrated, value);
            }
        }

        /// <summary>
        /// Remove the piece from the order book.
        /// </summary>
        /// <param name="pieces"></param>
        /// <param name="price"></param>
        protected void RemovePiece(IList<IOrderUnit> pieces, decimal price)
        {
            int i, c = pieces.Count;

            for (i = 0; i < c; i++)
            {
                if (pieces[i].Price == price)
                {
                    pieces.RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>
        /// Sequence the changes into the order book.
        /// </summary>
        /// <param name="changes">The changes to sequence.</param>
        /// <param name="pieces">The collection to change (either an ask or a bid collection)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SequencePieces(IList<OrderUnit> changes, KeyedCollection<decimal, TUnit> pieces)
        {
            foreach (var change in changes)
            {
                decimal cp = change.Price;

                if (change.Size == 0.0M)
                {
                    pieces.Remove(cp);
                }
                else
                {
                    if (pieces.Contains(cp))
                    {
                        var piece = pieces[cp];

                        piece.Size = change.Size;
                        if (piece is ISequencedOrderUnit seqpiece && change is ISequencedOrderUnit seqchange)
                            seqpiece.Sequence = seqchange.Sequence;
                    }
                    else
                    {
                        var newPiece = new TUnit();

                        newPiece.Price = change.Price;
                        newPiece.Size = change.Size;

                        if (newPiece is ISequencedOrderUnit seqpiece && change is ISequencedOrderUnit seqchange)
                            seqpiece.Sequence = seqchange.Sequence;


                        pieces.Add(newPiece);
                    }
                }
            }
        }

        /// <summary>
        /// De-initialize and clear all data and begin a new calibration.
        /// 
        /// </summary>
        public override void Reset()
        {
            orderBuffer = new List<Level2Update>();

            Initialized = false;
            Calibrated = false;
        }

        bool bf = false;

        /// <summary>
        /// Calibrate the order book from cached data.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Calibrate()
        {
            calibrated = true;
            bf = true;
            foreach (var q in orderBuffer)
            {
                if (q.SequenceStart > fullDepth.Sequence) OnNext(q);
            }
            bf = false;
            orderBuffer.Clear();

            OnPropertyChanged(nameof(Calibrated));
        }

        /// <summary>
        /// <see cref="IObserver{T}"/> implementation for <see cref="Level2Update"/> data.
        /// </summary>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void OnNext(Level2Update value)
        {
            if (disposed) return;

            lock (lockObj)
            {
                try
                {
                    if (!calibrated)
                    {
                        orderBuffer.Add(value);
                        return;
                    }
                    else if (fullDepth == null)
                    {
                        return;
                    }
                    else if (value.SequenceEnd <= fullDepth.Sequence)
                    {
                        return;
                    }

                    SequencePieces(value.Changes.Asks, fullDepth.Asks);
                    SequencePieces(value.Changes.Bids, fullDepth.Bids);

                    fullDepth.Sequence = value.SequenceEnd;
                    fullDepth.Timestamp = DateTime.Now;
                }
                catch 
                {

                }
            }
        }

        /// <summary>
        /// Copy the changes from a preflight source to a live destination.
        /// </summary>
        /// <param name="src">Source data.</param>
        /// <param name="dest">Destination collection.</param>
        /// <param name="pieces">The number of pieces to copy.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void CopyTo(IList<TUnit> src, IList<TUnit> dest, int pieces)
        {
            int i, c = pieces < src.Count ? pieces : src.Count;
            int x = dest.Count;

            if (x != c)
            {
                x = 0;
                dest.Clear();

                foreach (var piece in src)
                {
                    dest.Add(piece);
                    if (++x == c) break;
                }
            }
            else
            {
                for (i = 0; i < c; i++)
                {
                    dest[i] = src[i];
                }
            }

        }

        /// <summary>
        /// Push the preflight book to the live feed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void PushLive()
        {
            lock (lockObj)
            {
                if (orderBook == null)
                {
                    var ob = new TBook();
                    OrderBook = ob;
                }

                if (Dispatcher.Initialized)
                {
                    Dispatcher.InvokeOnMainThread((o) =>
                    {
                        orderBook.Sequence = fullDepth.Sequence;
                        orderBook.Timestamp = fullDepth.Timestamp;

                        CopyBook();
                    });
                }
                else
                {
                    orderBook.Sequence = fullDepth.Sequence;
                    orderBook.Timestamp = fullDepth.Timestamp;

                    CopyBook();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void CopyBook()
        {
            CopyTo(fullDepth.Asks, orderBook.Asks, pieces);
            CopyTo(fullDepth.Bids, orderBook.Bids, pieces);
        }

        #region IDisposable pattern

        protected override void Dispose(bool disposing)
        {
            if (disposed) return;

            disposed = true;

            cts?.Cancel();
            PushThread.Dispose();
            PushThread = null;

            if (disposing)
            {
                if (connectedFeed != null)
                {
                    ((Level2)connectedFeed).RemoveSymbol(symbol).Wait();
                }
            }

            lock (lockObj)
            {
                connectedFeed = null;
                fullDepth = default;
                orderBook = default;
                pieces = 0;
                symbol = null;
                calibrated = false;
                initialized = false;
            }
        }

        public override void OnCompleted()
        {
        }

        public override void OnError(Exception error)
        {
            throw error;
        }

        ~CustomLevel2Observation()
        {
            Dispose(false);
        }

        #endregion

    }

}