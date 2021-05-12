﻿using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;
using Kucoin.NET.Json;

namespace Kucoin.NET.Data.Order
{
    /// <summary>
    /// Market order
    /// </summary>
    public class MarketOrder : OrderBase
    {
        public MarketOrder()
        {
            Type = OrderType.Market;
        }

        /// <summary>
        /// [Optional] Desired amount in base currency
        /// </summary>
        [JsonProperty("size")]
        public decimal? Size { get; set; }

        /// <summary>
        /// [Optional] The desired amount of quote currency to use
        /// </summary>
        [JsonProperty("funds")]
        public decimal? Funds { get; set; }
    }
}
