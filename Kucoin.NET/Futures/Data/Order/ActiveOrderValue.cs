﻿using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Futures.Data.Order
{
    public class ActiveOrderValue
    {


        /// <summary>
        /// Total number of the unexecuted buy orders
        /// </summary>
        [JsonProperty("openOrderBuySize")]
        public decimal OpenOrderBuySize { get; set; }


        /// <summary>
        /// Total number of the unexecuted sell orders
        /// </summary>
        [JsonProperty("openOrderSellSize")]
        public decimal OpenOrderSellSize { get; set; }


        /// <summary>
        /// Value of all the unexecuted buy orders
        /// </summary>
        [JsonProperty("openOrderBuyCost")]
        public decimal OpenOrderBuyCost { get; set; }


        /// <summary>
        /// Value of all the unexecuted sell orders
        /// </summary>
        [JsonProperty("openOrderSellCost")]
        public decimal OpenOrderSellCost { get; set; }


        /// <summary>
        /// Settlement currency
        /// </summary>
        [JsonProperty("settleCurrency")]
        public string SettleCurrency { get; set; }

    }


}
