﻿using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Order;
using Kucoin.NET.Futures.Data.Trade;
using Kucoin.NET.Helpers;
using Kucoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Futures.Data.Websockets
{
    public class AdvancedOrder
    {


        /// <summary>
        /// Order ID
        /// </summary>
        [JsonProperty("orderId")]
        public string OrderId { get; set; }


        /// <summary>
        /// Ticker symbol of the contract
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }


        /// <summary>
        /// Message type: open (place order), triggered (order triggered), cancel (cancel order)
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }


        /// <summary>
        /// Order type: stop order
        /// </summary>
        [JsonProperty("orderType")]
        public string OrderType { get; set; }


        /// <summary>
        /// Buy or sell
        /// </summary>
        [JsonProperty("side")]
        public Side Side { get; set; }


        /// <summary>
        /// Quantity
        /// </summary>
        [JsonProperty("size")]
        public decimal Size { get; set; }


        /// <summary>
        /// Order price. For market orders, the value is null
        /// </summary>
        [JsonProperty("orderPrice")]
        public decimal OrderPrice { get; set; }


        /// <summary>
        /// Stop order types
        /// </summary>
        [JsonProperty("stop")]
        public string Stop { get; set; }


        /// <summary>
        /// Trigger price of stop orders
        /// </summary>
        [JsonProperty("stopPrice")]
        public decimal StopPrice { get; set; }


        /// <summary>
        /// Trigger price type of stop orders
        /// </summary>
        [JsonProperty("stopPriceType")]
        public string StopPriceType { get; set; }


        /// <summary>
        /// Mark to show whether the order is triggered. Only for triggered type of orders
        /// </summary>
        [JsonProperty("triggerSuccess")]
        public bool TriggerSuccess { get; set; }


        /// <summary>
        /// Error code, which is used only when the trigger of the triggered type of orders fails
        /// </summary>
        [JsonProperty("error")]
        public string Error { get; set; }


        /// <summary>
        /// Time the order created
        /// </summary>
        [JsonProperty("createdAt")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime CreatedAt { get; set; }


        /// <summary>
        /// Timestamp - nanosecond
        /// </summary>
        [JsonProperty("ts")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InNanoseconds)]
        public DateTime Timestamp { get; set; }

    }


}
