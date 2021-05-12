﻿using System;
using System.Collections.Generic;
using System.Text;

using Kucoin.NET.Rest;
using Kucoin.NET.Data.Order;
using System.Threading.Tasks;
using System.Net.Http;
using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Interfaces;

namespace Kucoin.NET.Rest
{
    public class Order : KucoinBaseRestApi
    {

        public Order(ICredentialsProvider credProvider, bool isSandbox = false) : base(credProvider, isSandbox)
        {
        }

        public Order(string key, string secret, string passphrase, bool isSandbox = false) : base(key, secret, passphrase, isSandbox)
        {
        }

        public async Task<IList<OrderDetails>> ListOrders(
            OrderStatus? status = null, 
            string symbol = null,
            Side? side = null,
            OrderType? type = null,
            TradeType? tradeType = null,
            DateTime? startAt = null,
            DateTime? endAt = null
            )
        {
            var lp = new OrderListParams(status, symbol, side, type, tradeType, startAt, endAt);

            return await ListOrders(lp);
        }

        public async Task<IList<OrderDetails>> ListOrders(OrderListParams listParams)
        {
            return await GetAllPaginatedResults<OrderDetails, OrderDetailsPage>(HttpMethod.Get, "/api/v1/orders", reqParams: listParams.ToDict());
        }

        public async Task<string[]> CancelAllOrders(string symbol = null, TradeType? tradeType = null)
        {
            //  /api/v1/orders

            var dict = new Dictionary<string, object>();

            if (symbol != null)
            {
                dict.Add("symbol", symbol);
            }

            if (tradeType != null)
            {
                dict.Add("tradeType", tradeType);
            }

            var jobj = await MakeRequest(HttpMethod.Delete, "/api/v1/orders", reqParams: dict);
            return jobj["cancelledOrderIds"].ToObject<string[]>();
        }

        public async Task<string> DeleteOrderByClientId(string clientOid)
        {
            var jobj = await MakeRequest(HttpMethod.Delete, $"/api/v1/order/client-order/{clientOid}");
            return jobj["cancelledOrderId"].ToObject<string>();

        }
        public async Task<string[]> DeleteOrderById(string orderId)
        {
            var jobj = await MakeRequest(HttpMethod.Delete, $"/api/v1/orders/{orderId}");
            return jobj["cancelledOrderIds"].ToObject<string[]>();
        }

        /// <summary>
        /// Create a market margin order.
        /// </summary>
        /// <param name="orderDetails">The order details.</param>
        /// <returns></returns>
        public async Task<OrderReceipt> CreateMarketMarginOrder(MarketOrder orderDetails)
        {
            return await CreateMarginOrder(orderDetails);
        }

        /// <summary>
        /// Createa a market spot order.
        /// </summary>
        /// <param name="orderDetails">The order details.</param>
        /// <returns></returns>
        public async Task<OrderReceipt> CreateMarketSpotOrder(MarketOrder orderDetails)
        {
            return await CreateSpotOrder(orderDetails);
        }

        /// <summary>
        /// Create a limit margin order.
        /// </summary>
        /// <param name="orderDetails">The order details.</param>
        /// <returns></returns>
        public async Task<OrderReceipt> CreateLimitMarginOrder(LimitOrder orderDetails)
        {
            return await CreateMarginOrder(orderDetails);
        }

        /// <summary>
        /// Create a limit spot order.
        /// </summary>
        /// <param name="orderDetails">The order details.</param>
        /// <returns></returns>
        public async Task<OrderReceipt> CreateLimitSpotOrder(LimitOrder orderDetails)
        {
            return await CreateSpotOrder(orderDetails);
        }

        /// <summary>
        /// Create a margin order.
        /// </summary>
        /// <param name="orderDetails">The order details.</param>
        /// <returns></returns>
        public async Task<OrderReceipt> CreateMarginOrder(OrderBase orderDetails)
        {
            var jobj = await MakeRequest(HttpMethod.Post, "/api/v1/margin/order", reqParams: orderDetails.ToDict());
            return jobj.ToObject<OrderReceipt>();

        }

        /// <summary>
        /// Create a spot order.
        /// </summary>
        /// <param name="orderDetails">The order details.</param>
        /// <returns></returns>
        public async Task<OrderReceipt> CreateSpotOrder(OrderBase orderDetails)
        {
            var jobj = await MakeRequest(HttpMethod.Post, "/api/v1/orders", reqParams: orderDetails.ToDict());
            return jobj.ToObject<OrderReceipt>();
        }


    }
}
