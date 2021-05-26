﻿using Kucoin.NET.Helpers;
using Kucoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Data.Market
{
    /// <summary>
    /// Spot / Mark Price Data
    /// </summary>
    public class Price
    {
        /// <summary>
        /// Symbol
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        /// <summary>
        /// Granularity
        /// </summary>
        [JsonProperty("granularity")]
        public long Granularity { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonProperty("timestamp")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        internal DateTime? InternalTimestamp { get; set; }

        /// <summary>
        /// Timepoint
        /// </summary>
        [JsonProperty("timePoint")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        internal DateTime? InternalTimepoint { get; set; }

        /// <summary>
        /// Time Stamp
        /// </summary>
        [JsonIgnore]
        public DateTime Timestamp => InternalTimestamp ?? InternalTimepoint ?? DateTime.Now;

        /// <summary>
        /// Value
        /// </summary>
        [JsonProperty("value")]
        public decimal Value { get; set; }

    }
}
