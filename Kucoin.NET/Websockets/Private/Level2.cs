﻿using Kucoin.NET.Data.Websockets;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using System.Text;
using System.Threading.Tasks;
using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Interfaces;
using System.Net.Http;
using Kucoin.NET.Websockets.Observations;
using Kucoin.NET.Observable;
using System.Linq;
using Kucoin.NET.Helpers;

namespace Kucoin.NET.Websockets.Private
{

    /// <summary>
    /// Standard Level 2 feed implementation with observables and UI data binding support.
    /// </summary>
    public class Level2 : Level2Base<OrderBook<OrderUnit>, OrderUnit, Level2Update, Level2Observation>
    {
        /// <summary>
        /// Create a new Level 2 feed with the specified credentials.
        /// </summary>
        /// <param name="key">API Key</param>
        /// <param name="secret">API Secret</param>
        /// <param name="passphrase">API Passphrase</param>
        /// <param name="isSandbox">Is Sandbox Mode</param>
        /// <remarks>
        /// You must either create this instance on the main / UI thread or call <see cref="Dispatcher.Initialize"/> prior to 
        /// creating an instance of this class or an <see cref="InvalidOperationException"/> will be raised.
        /// </remarks>
        public Level2(
            string key,
            string secret,
            string passphrase,
            bool isSandbox = false)
            : base(key, secret, passphrase, isSandbox)
        {
            if (!Dispatcher.Initialized)
            {
                throw new InvalidOperationException("You must call Kucoin.NET.Helpers.Dispatcher.Initialize() with a SynchronizationContext before instantiating this class.");
            }
        }

        public override string AggregateEndpoint => "/api/v2/market/orderbook/level2";

        public override string Subject => "trade.l2update";

        public override string Topic => "/market/level2";

        /// <summary>
        /// Create a new Level 2 feed with the specified credentials.
        /// </summary>
        /// <param name="credProvider"><see cref="ICredentialsProvider"/> implementation.</param>
        /// <remarks>
        /// You must either create this instance on the main / UI thread or call <see cref="Dispatcher.Initialize"/> prior to 
        /// creating an instance of this class or an <see cref="InvalidOperationException"/> will be raised.
        /// </remarks>
        public Level2(ICredentialsProvider credProvider) : base(credProvider)
        {
            if (!Dispatcher.Initialized)
            {
                throw new InvalidOperationException("You must call Kucoin.NET.Helpers.Dispatcher.Initialize() with a SynchronizationContext before instantiating this class.");
            }
        }

        public override async Task<OrderBook<OrderUnit>> GetAggregatedOrder(string symbol)
        {
            return await GetPartList(symbol, 0);
        }


        /// <summary>
        /// Get the Level 2 Data Book for the specified trading symbol.
        /// </summary>
        /// <param name="symbol">The trading symbol.</param>
        /// <param name="pieces">The number of pieces.</param>
        /// <returns>The part book snapshot.</returns>
        /// <remarks>
        /// Settings the number of pieces to 0 returns the full market depth. 
        /// Use 0 to calibrate a full level 2 feed.
        /// </remarks>
        public async Task<OrderBook<OrderUnit>> GetPartList(string symbol, int pieces)
        {
            var curl = pieces > 0 ? $"{AggregateEndpoint}_{pieces}" : AggregateEndpoint;
            var param = new Dictionary<string, object>();

            param.Add("symbol", (string)symbol);

            var jobj = await MakeRequest(HttpMethod.Get, curl, 5, false, param);
            var result = jobj.ToObject<OrderBook<OrderUnit>>();

            foreach (var ask in result.Asks)
            {
                if (ask is ISequencedOrderUnit seq)
                    seq.Sequence = result.Sequence;
            }

            foreach (var bid in result.Bids)
            {
                if (bid is ISequencedOrderUnit seq)
                    seq.Sequence = result.Sequence;
            }

            return result;
        }

        protected override Level2Observation CreateNewObservation(string symbol)
        {
            return new Level2Observation(this, symbol, defaultPieces);
        }


    }



}
