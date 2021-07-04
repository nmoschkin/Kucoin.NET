﻿
using System;
using System.Collections.Generic;
using System.Text;

using Kucoin.NET.Data;

using Newtonsoft.Json;

namespace Kucoin.NET.Futures.Data.Order
{
    /// <summary>
    /// Paginated order details
    /// </summary>
    public class FuturesFillDetailsPage : IPaginated<FuturesFillDetails>
    {
        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("currentSize")]
        public int CurrentPage { get; set; }

        [JsonProperty("totalNum")]
        public int TotalNumber { get; set; }

        [JsonProperty("items")]
        public IList<FuturesFillDetails> Items { get; set; }

        [JsonProperty("totalPages")]
        public int TotalPages { get; set; }

    }

}
