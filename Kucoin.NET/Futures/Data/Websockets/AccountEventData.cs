﻿using Kucoin.NET.Helpers;
using Kucoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Futures.Data.Websockets
{

    public enum EventType
    {
        OrderMargin,
        AvailableBalance,
        WithdrawalAmount
    }

    public class AccountEventData
    {
        /// <summary>
        /// The type of information contained in this class.
        /// </summary>
        [JsonIgnore]
        public EventType EventType { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }


        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonProperty("timestamp")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Current available amount
        /// </summary>
        [JsonProperty("availableBalance")]
        public decimal? AvailableBalance { get; set; }


        /// <summary>
        /// Frozen amount
        /// </summary>
        [JsonProperty("holdBalance")]
        public decimal? HoldBalance { get; set; }

        /// <summary>
        /// Current order margin
        /// </summary>
        [JsonProperty("orderMargin")]
        public decimal? OrderMargin { get; set; }

        /// <summary>
        /// Current frozen amount for withdrawal
        /// </summary>
        [JsonProperty("withdrawHold")]
        public decimal? WithdrawHold { get; set; }

    }




}
