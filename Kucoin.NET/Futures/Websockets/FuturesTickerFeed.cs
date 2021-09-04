﻿using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Futures.Data.Market;
using Kucoin.NET.Websockets;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Futures.Websockets
{
    /// <summary>
    /// Futures Market Ticker Feed
    /// </summary>
    public class FuturesTickerFeed : SymbolTopicFeedBase<FuturesTicker>
    {
        public FuturesTickerFeed() : base(null, futures: true)
        {
        }

        public override bool IsPublic => true;

        public override string Subject => "tickerV2";

        public override string Topic => "/contractMarket/tickerV2";

        protected override Task HandleMessage(FeedMessage msg)
        {
            return base.HandleMessage(msg);
        }

    }
}
