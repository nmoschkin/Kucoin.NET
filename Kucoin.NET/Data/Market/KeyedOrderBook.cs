﻿using Kucoin.NET.Observable;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Newtonsoft.Json;
using Kucoin.NET.Rest;
using Kucoin.NET.Helpers;
using Kucoin.NET.Json;
using Kucoin.NET.Data.Order;

namespace Kucoin.NET.Data.Market
{

    /// <summary>
    /// Provides the standard, observable order book implementation.
    /// </summary>
    /// <typeparam name="T">The type of the order unit.</typeparam>
    public class KeyedOrderBook<T> : IKeyedOrderBook<Level2KeyedCollection<T>, T> where T: IOrderUnit 
    {
        /// <summary>
        /// The sequence number of the order
        /// </summary>
        [JsonProperty("sequence")]
        [KeyProperty]
        public long Sequence { get; set; }

        /// <summary>
        /// Asks (sell)
        /// </summary>
        [JsonProperty("asks")]
        public Level2KeyedCollection<T> Asks { get; set; } = new Level2KeyedCollection<T>();

        /// <summary>
        /// Bids (buy)
        /// </summary>
        [JsonProperty("bids")]
        public Level2KeyedCollection<T> Bids { get; set; } = new Level2KeyedCollection<T>(true);

        /// <summary>
        /// The time stamp of the order
        /// </summary>
        [JsonProperty("time")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public virtual DateTime Timestamp { get; set; }

        IList<T> IOrderUnitList<T>.Asks => Asks;

        IList<T> IOrderUnitList<T>.Bids => Bids;

        IList<T> IDataSeries<T, T, IList<T>, IList<T>>.Data1 => Asks;

        IList<T> IDataSeries<T, T, IList<T>, IList<T>>.Data2 => Bids;
    }

    /// <summary>
    /// Provides the standard, observable order book implementation.
    /// </summary>
    /// <typeparam name="T">The type of the order unit.</typeparam>
    public class ObservableOrderBook<T> : 
        ObservableBase, 
        IOrderBook<T> 
        where T : IOrderUnit
    {
        protected long seq;
        protected DateTime time;
        protected ObservableCollection<T> asks = new ObservableCollection<T>();
        protected ObservableCollection<T> bids = new ObservableCollection<T>();

        /// <summary>
        /// The sequence number of the order
        /// </summary>
        [JsonProperty("sequence")]
        [KeyProperty]
        public long Sequence
        {
            get => seq;
            set
            {
                SetProperty(ref seq, value);
            }
        }

        /// <summary>
        /// Asks (sell)
        /// </summary>
        [JsonProperty("asks")]
        public ObservableCollection<T> Asks
        {
            get => asks;
            set
            {
                SetProperty(ref asks, value);
            }
        }

        /// <summary>
        /// Bids (buy)
        /// </summary>
        [JsonProperty("bids")]
        public ObservableCollection<T> Bids
        {
            get => bids;
            set
            {
                SetProperty(ref bids, value);
            }
        }

        /// <summary>
        /// The time stamp of the order
        /// </summary>
        [JsonProperty("time")]
        [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InMilliseconds)]
        public virtual DateTime Timestamp
        {
            get => time;
            set
            {
                SetProperty(ref time, value);
            }
        }

        IList<T> IOrderUnitList<T>.Asks => Asks;

        IList<T> IOrderUnitList<T>.Bids => Bids;

        IList<T> IDataSeries<T, T, IList<T>, IList<T>>.Data1 => Asks;

        IList<T> IDataSeries<T, T, IList<T>, IList<T>>.Data2 => Bids;
    }

}
