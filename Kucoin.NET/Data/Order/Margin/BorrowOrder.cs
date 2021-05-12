﻿using Kucoin.NET.Helpers;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Data.Order.Margin
{
    public class BorrowOrder
    {


        /// <summary>
        /// OrderId
        /// </summary>
        [JsonProperty("orderId")]
        public string OrderId { get; set; }


        /// <summary>
        /// Currency
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }


        /// <summary>
        /// Size
        /// </summary>
        [JsonProperty("size")]
        public decimal Size { get; set; }


        /// <summary>
        /// Filled
        /// </summary>
        [JsonProperty("filled")]
        public decimal Filled { get; set; }


        /// <summary>
        /// MatchList
        /// </summary>
        [JsonProperty("matchList")]
        public List<BorrowMatch> MatchList { get; set; }


        /// <summary>
        /// Status
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; }

    }

    public class BorrowMatch
    {


        /// <summary>
        /// Currency
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }


        /// <summary>
        /// DailyIntRate
        /// </summary>
        [JsonProperty("dailyIntRate")]
        public decimal DailyIntRate { get; set; }


        /// <summary>
        /// Size
        /// </summary>
        [JsonProperty("size")]
        public decimal Size { get; set; }


        /// <summary>
        /// Term
        /// </summary>
        [JsonProperty("term")]
        public long Term { get; set; }


        /// <summary>
        /// InternalTimestamp
        /// </summary>
        [JsonProperty("timestamp")]
        internal long InternalTimestamp { get; set; }

        [JsonIgnore]
        public DateTime Timestamp => EpochTime.MillisecondsToDate(InternalTimestamp);



        /// <summary>
        /// TradeId
        /// </summary>
        [JsonProperty("tradeId")]
        public long TradeId { get; set; }

    }


}
