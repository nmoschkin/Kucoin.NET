﻿using KuCoin.NET.Data;
using KuCoin.NET.Data.Market;
using KuCoin.NET.Data.Websockets;
using KuCoin.NET.Websockets.Distribution.Services;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Websockets.Distribution
{

    public interface IMarketPresenter
    : IDistributable,
        IInitializable,
        ISymbol,
        IMarketDepth,
        IPresentable
    {
    }

    public interface IMarketPresenter<TKey, TValue>
    : IMarketPresenter, IDistributable<TKey, TValue>
      where TValue : IStreamableObject
    {
    }

    public interface IMarketPresenter<TObservable, TKey, TValue> 
    :  IMarketPresenter<TKey, TValue>, IPresentable<TObservable>
       where TValue : IStreamableObject
    {
    }

    public interface IMarketPresenter<TInternal, TObservable, TKey, TValue> 
        : IDistributable<TKey, TValue>, 
        IInitializable<TKey, TInternal>, 
        ISymbol, 
        IMarketDepth, 
        IPresentable<TInternal, TObservable> 
        where TValue : IStreamableObject
    {
    }



    /// <summary>
    /// Base class for all market presentation where the raw data and the presented data are two different objects, with the presented data being data prepared from the raw data at regular intervals.
    /// </summary>
    /// <typeparam name="TInternal"></typeparam>
    /// <typeparam name="TObservable"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public abstract class MarketPresenter<TInternal, TObservable, TValue> : 
        InitializableObject<string, TValue, TInternal>, 
        IMarketPresenter<TInternal, TObservable, string, TValue> 
        where TValue : IStreamableObject
    {
        public MarketPresenter(IDistributor parent, string symbol) : base(parent, symbol)
        {
            Symbol = symbol;
            PresentationService.RegisterService(this);
        }

        public abstract bool IsPresentationDisabled { get; set; }
        public abstract TObservable PresentedData { get; protected set; }
        public abstract bool PreferDispatcher { get; }
        public abstract int Interval { get; set; }
        public abstract int MarketDepth { get; set; }

        public string Symbol
        {
            get => key;
            set
            {
                SetProperty(ref key, value);    
            }
        }

        public abstract void PresentData();
        protected override void Dispose(bool disposing)
        {
            PresentationService.UnregisterService(this);    
            base.Dispose(disposing);
        }
    }
}
