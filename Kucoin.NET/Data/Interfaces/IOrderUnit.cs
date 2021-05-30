﻿using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Order;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Kucoin.NET.Data.Interfaces
{
    public interface IOrderUnit : ICloneable
    {
        decimal Price { get; set; }

        decimal Size { get; set; }
    }

    public interface IAtomicOrderUnit : IOrderUnit
    {
        string OrderId { get; set; }

        DateTime Timestamp { get; set; }
    }

    public interface ISequencedOrderUnit : IOrderUnit
    {
        long Sequence { get; set; }
    }

    public interface IFuturesOrderUpdate : IOrderUnit
    {
        Side Side { get; set; }
    }

    public interface IOrderUnitList<T> where T: IOrderUnit
    {
        IList<T> Asks { get; }

        IList<T> Bids { get; }
    }

    public interface IOrderUnitListProvider<T> where T : IOrderUnit
    {
        IOrderUnitList<T> OrderList { get; set; }
    }


    public interface IKeyedOrderUnitList<T> where T: IOrderUnit
    {
        SortedKeyedOrderUnitBase<T> Asks { get; }

        SortedKeyedOrderUnitBase<T> Bids { get; }

    }

    public interface IOrderBook<T> : IKeyedOrderUnitList<T> where T: IOrderUnit
    {
        long Sequence { get; set; }

        DateTime Timestamp { get; set; }
    }

}
