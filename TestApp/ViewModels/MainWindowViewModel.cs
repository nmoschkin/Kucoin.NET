﻿using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.User;
using Kucoin.NET.Observable;
using Kucoin.NET.Rest;
using Kucoin.NET.Websockets.Observations;
using Kucoin.NET.Websockets.Public;

using Kucoin.NET.Futures.Websockets;
using Kucoin.NET.Futures.Data.Market;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

using FancyCandles;

using KuCoinApp.Localization.Resources;
using KuCoinApp.ViewModels;
using KuCoinApp.Views;
using Windows.ApplicationModel.Activation;
using Kucoin.NET.Websockets;
using System.Net.WebSockets;
using System.Windows;
using Kucoin.NET.Helpers;

namespace KuCoinApp
{

    public class MainWindowViewModel : WindowViewModelBase, IObserver<Ticker>, IObserver<KlineFeedMessage<KlineCandle>>
    {
        protected Level3 level2Feed;
                
        protected Accounts accountWnd;

        protected FuturesWindow fwindow;

        //protected Level3Depth50 level2Feed50;

        protected TickerFeed tickerFeed;

        protected KlineFeed<KlineCandle> klineFeed;

        protected IDisposable tickerSubscription;

        protected IDisposable klineSubscription;

        protected Credentials credWnd;

        protected IntRange lastRange;

        protected bool isCredWndShowing;

        protected Market market;

        protected User user;

        protected bool isLoggedIn;

        protected ObservableCollection<Account> accounts;

        protected ObservableDictionary<string, TradingSymbol> symbols;

        protected TradingSymbol symbol;

        protected KlineType kt = KlineType.Min1;

        KlineCandle lastCandle = new KlineCandle(new Candle() { Timestamp = DateTime.Now, Type = KlineType.Min1 });

        protected Ticker nowTicker = null;

        protected decimal? nowPrice = null;

        protected decimal? volume = null;

        protected DateTime? volumeTime = null;

        protected CurrencyViewModel currency, quoteCurrency;

        protected ObservableCollection<FancyCandles.ICandle> data = new ObservableCollection<FancyCandles.ICandle>();

        protected ObservableCollection<CurrencyViewModel> currencies = new ObservableCollection<CurrencyViewModel>();

        protected Level3Observation level2;

        protected List<SymbolViewModel> recentSymbols = new List<SymbolViewModel>();

        protected ObservableStaticMarketDepthUpdate marketUpdate;

        protected string priceFormat = "0.00";

        protected string sizeFormat = "0.00";

        public override event EventHandler AskQuit;

        int l2delay = 50;

        public ICommand RefreshSymbolsCommand { get; protected set; }
        public ICommand RefreshKlineCommand { get; protected set; }
        public ICommand RefreshPriceCommand { get; protected set; }
        public ICommand EditCredentialsCommand { get; protected set; }

        public ICommand ShowAccountsCommand { get; protected set; }
        public ICommand ShowFuturesCommand { get; protected set; }

        public ObservableStaticMarketDepthUpdate MarketUpdate
        {
            get => marketUpdate;
            set
            {
                SetProperty(ref marketUpdate, value);
            }
        }

        public IntRange LastCandleRange
        {
            get => lastRange;
            set
            {
                if (SetProperty(ref lastRange, value))
                {
                    App.Current.Settings.LastCandleRange = value;
                }
            }
        }

        public bool IsCredShowing
        {
            get => isCredWndShowing;
            protected set
            {
                SetProperty(ref isCredWndShowing, value);
            }
        }


        public Level3Observation Level3
        {
            get => level2;
            set
            {
                SetProperty(ref level2, value);
            }
        }

        public ObservableCollection<Account> Accounts
        {
            get => accounts;
            set
            {
                SetProperty(ref accounts, value);
            }
        }

        public bool IsLoggedIn
        {
            get => isLoggedIn;
            set
            {
                SetProperty(ref isLoggedIn, value);
            }
        }


        public User User
        {
            get => user;
            protected set
            {
                SetProperty(ref user, value);
            }
        }

        public ObservableCollection<CurrencyViewModel> Currencies
        {
            get => currencies;
            set
            {
                SetProperty(ref currencies, value);
            }
        }

        public CurrencyViewModel Currency
        {
            get => currency;
            set
            {
                SetProperty(ref currency, value);
            }
        }
        public CurrencyViewModel QuoteCurrency
        {
            get => quoteCurrency;
            set
            {
                SetProperty(ref quoteCurrency, value);
            }
        }

        public string PriceFormat
        {
            get => priceFormat;
            set
            {
                SetProperty(ref priceFormat, value);
            }
        }

        public string SizeFormat
        {
            get => sizeFormat;
            set
            {
                SetProperty(ref sizeFormat, value);
            }
        }

        public KlineType KlineType
        {
            get => kt;
            set
            {
                var oval = kt;

                if (SetProperty(ref kt, value))
                {
                    UpdateSymbol((string)Symbol, (string)Symbol, true, true, oval);

                    Volume = null;
                    VolumeTime = null;
                }
            }
        }


        public Level3 Level3Feed
        {
            get => level2Feed;
            set
            {
                SetProperty(ref level2Feed, value);
            }
        }

        public Ticker RealTimeTicker
        {
            get => nowTicker;
            set
            {
                SetProperty(ref nowTicker, value);
            }
        }

        public decimal? RealTimePrice
        {
            get => nowPrice;
            set
            {
                SetProperty(ref nowPrice, value);
            }
        }

        public decimal? Volume
        {
            get => volume;
            set
            {
                SetProperty(ref volume, value);
            }
        }

        public DateTime? VolumeTime
        {
            get => volumeTime;
            set
            {
                SetProperty(ref volumeTime, value);
            }
        }

        public ObservableDictionary<string, TradingSymbol> Symbols
        {
            get => symbols;
            set
            {
                SetProperty(ref symbols, value);
            }
        }

        public ObservableCollection<FancyCandles.ICandle> Data
        {
            get => data;
            set
            {
                SetProperty(ref data, value);
            }
        }

        public TradingSymbol Symbol
        {
            get => symbol;
            set
            {
                var ot = symbol;

                if (SetProperty(ref symbol, value))
                {
                    UpdateSymbol((string)ot, (string)value);

                    var tp = symbol.TradingPair;

                    foreach (var curr in currencies)
                    {
                        if (curr.Currency.Currency == tp[0])
                        {
                            if (!curr.ImageLoaded) _ = curr.LoadImage();
                            Currency = curr;
                        }
                        else if (curr.Currency.Currency == tp[1])
                        {
                            if (!curr.ImageLoaded) _ = curr.LoadImage();
                            QuoteCurrency = curr;
                        }
                    }

                    OnPropertyChanged(nameof(SymbolDescription));
                }
                else
                {
                    Data?.Clear();
                }
            }
        }

        public string SymbolDescription
        {
            get
            {
                if (market.Currencies == null || ((string)symbol) == null || !((string)symbol).Contains("-")) return null;

                var tsp = ((string)symbol).Split("-");

                MarketCurrency c1, c2;
                c1 = c2 = null; 

                foreach (var c in market.Currencies)
                {
                    if (c.Currency == tsp[0])
                    {
                        c1 = c;
                    }
                    else if (c.Currency == tsp[1])
                    {
                        c2 = c;
                    }
                }

                if (c1 != null && c2 != null)
                {
                    return string.Format("{0} - {1}", c1.FullName, c2.FullName);
                }
                else
                {
                    return (string)symbol;
                }
            }
        }

        protected void MakeFormats(string symbol)
        {
            var sym = market.Symbols[symbol];
            var inc = "0" + sym.QuoteIncrement.ToString().Replace("1", "0");

            SizeFormat = inc;
            
            inc = sym.PriceIncrement.ToString().Replace("1", "0");

            PriceFormat = inc;
        }



        SymbolViewModel selSym;

        protected void PushSymbol(string symbol)
        {
            App.Current.Settings.PushSymbol(symbol);

            App.Current.Dispatcher.Invoke(() =>
            {
                MakeRecentSymbols();
            });
        }

        protected void MakeRecentSymbols()
        {
            var sym = App.Current.Settings.Symbols;

            var rc = new List<SymbolViewModel>();

            if (symbols == null || symbols.Count == 0 || sym == null || sym.Length == 0) return;

            foreach (var s in sym)
            {
                foreach (var sobj in symbols)
                {
                    if (s == sobj.Symbol)
                    {
                        var snew = new SymbolViewModel(sobj);
                        rc.Add(snew);
                    }
                }
            }

            RecentSymbols = rc;
        }

        public SymbolViewModel SelectedSymbol
        {
            get => selSym;
            set
            {
                if (SetProperty(ref selSym, value))
                {
                    if ((symbol?.Symbol ?? null) != (selSym?.Symbol ?? null))
                    {
                        foreach (var sym in Symbols)
                        {
                            if (sym.Symbol == selSym?.Symbol)
                            {
                                Symbol = sym;
                                return;
                            }
                        }
                    }
                }
            }
        }

        public List<SymbolViewModel> RecentSymbols
        {
            get => recentSymbols;
            set
            {
                SetProperty(ref recentSymbols, value);
            }
        }

        protected void UpdateSymbol(
            string oldSymbol, 
            string newSymbol, 
            bool force = false, 
            bool isKlineChange = false, 
            KlineType? oldKline = null)
        {

            PushSymbol(newSymbol);
            MakeFormats(newSymbol);

            foreach (var sym in recentSymbols)
            {
                if (sym.Data.Symbol == newSymbol)
                {
                    SelectedSymbol = sym;
                    break;
                }
            }

            if (force || (oldSymbol != (string)symbol && oldSymbol != null))
            {
                tickerFeed.UnsubscribeOne(oldSymbol).ContinueWith((t) =>
                {
                    klineFeed.RemoveSymbol(oldSymbol, oldKline ?? KlineType).ContinueWith((t) =>
                    {
                        RefreshData().ContinueWith(async (t) =>
                        {

                            await tickerFeed.SubscribeOne(newSymbol);
                            await klineFeed.AddSymbol(newSymbol, KlineType);

                            Volume = null;
                            VolumeTime = null;

                            if (isKlineChange)
                            {
                                return;
                            }

                            if (Level3 != null)
                            {
                                Level3.Dispose();
                            }

                            if (level2Feed == null || level2Feed.Connected == false)
                            {
                                level2Feed = new Level3(cred);
                                level2Feed.MonitorThroughput = true;
                                await level2Feed.Connect();
                            }

                            await level2Feed.AddSymbol(newSymbol).ContinueWith((t) =>
                            {
                                App.Current?.Dispatcher?.Invoke(() =>
                                {
                                    Level3 = t.Result;
                                    Level3.IsObservationDisabled = false;
                                    Level3.Interval = l2delay;
                                });

                            });

                        });
                    });
                });

            }
            else
            {
                RefreshData().ContinueWith(async (t2) =>
                {
                    if (level2Feed == null || level2Feed.Connected == false)
                    {
                        level2Feed = new Level3(cred);
                        level2Feed.MonitorThroughput = true;
                        await level2Feed.Connect();
                    }

                    level2Feed?.AddSymbol(newSymbol).ContinueWith((t) =>
                    {
                        App.Current?.Dispatcher?.Invoke(() =>
                        {
                            Level3 = t.Result;
                            Level3.IsObservationDisabled = false;
                            Level3.Interval = l2delay;
                        });

                    });

                    await tickerFeed.SubscribeOne(newSymbol);
                    await klineFeed.AddSymbol(newSymbol, KlineType);

                });
            }


        }

        bool finit = false;

        public async Task RefreshData()
        {
            if (!Kucoin.NET.Helpers.Dispatcher.Initialized)
            {
                Kucoin.NET.Helpers.Dispatcher.Initialize(new DispatcherSynchronizationContext(App.Current.Dispatcher));
            }

            //if (cred != null && (level2Feed == null || level2Feed.Connected == false))
            //{
            //    if (finit) return;

            //    level2Feed = new Level3();
            //    await level2Feed.Connect();

            //    if (level2Feed.Connected == false)
            //    {
            //        System.Windows.MessageBox.Show(
            //            AppResources.ErrorNoInternet, 
            //            "", 
            //            System.Windows.MessageBoxButton.OK, 
            //            System.Windows.MessageBoxImage.Error);

            //        return;
            //    }
            //    finit = true;
            //}

            //if (tickerFeed == null || tickerFeed.Connected == false)
            //{
            //    if (finit) return;

            //    tickerFeed = new TickerFeed();
            //    klineFeed = new KlineFeed<KlineCandle>();

            //    await tickerFeed.Connect(true);

            //    if (tickerFeed.Connected == false)
            //    {
            //        System.Windows.MessageBox.Show(
            //            AppResources.ErrorNoInternet,
            //            "",
            //            System.Windows.MessageBoxButton.OK,
            //            System.Windows.MessageBoxImage.Error);
                    
            //        return;

            //    }

            //    await klineFeed.MultiplexInit(tickerFeed);
            //    finit = true;
            //}

            if (finit)
            {
                UpdateSymbol((string)symbol, (string)symbol, true);
            }
            else
            {
                var kc = await market.GetKline<KlineCandle, FancyCandles.ICandle, ObservableCollection<FancyCandles.ICandle>>((string)symbol, KlineType, startTime: KlineType.GetStartDate(200));

                Volume = null;
                VolumeTime = null;

                Data = kc;

                App.Current?.Dispatcher?.Invoke(() =>
                {
                    LastCandle = (KlineCandle)kc?.LastOrDefault();
                });
            }
        }

        public void FocusCredWindow()
        {
            if (credWnd != null)
            {
                credWnd.Focus();
            }
        }

        public bool EditCredentials()
        {
            if (cred == null)
            {
                cred = CryptoCredentials.LoadFromStorage(App.Current.Seed);
            }

            credWnd = new Credentials(cred);

            IsCredShowing = true;

            var b = credWnd.ShowDialog();

            IsCredShowing = false;
            credWnd = null;

            if (b == true)
            {
                return true;
            }
            else
            {
                return false;
            }

            
        }

        public KlineCandle LastCandle
        {
            get => lastCandle;
            set
            {
                if (SetProperty(ref lastCandle, value))
                {
                    if (data.Count == 0 || !Candle.IsTimeInCandle((ITypedCandle<KlineType>)data.LastOrDefault(), lastCandle.Timestamp))
                    {
                        data.Add(lastCandle);
                    }
                    else
                    {
                        data[data.Count - 1] = lastCandle;
                    }
                }

            }
        }

        void IObserver<KlineFeedMessage<KlineCandle>>.OnNext(KlineFeedMessage<KlineCandle> ticker)
        {
            if (ticker.Symbol == (string)symbol && KlineType == ticker.Candles.Type)
            {
                App.Current?.Dispatcher.Invoke(() =>
                {
                    Volume = ticker.Candles.Volume;
                    VolumeTime = ticker.Timestamp;
                });
            }
            else
            {
                return;
            }

        }

        void IObserver<Ticker>.OnNext(Ticker ticker)
        {
            if ((string)ticker.Symbol != (string)symbol) return;
            Task.Run(() =>
            {
                decimal p = ticker.Price;
                var n = DateTime.Now;

                n = n.AddSeconds(-1 * n.Second);

                KlineCandle kl = null;

                if ((data?.Count ?? 0) == 0 || !Candle.IsTimeInCandle(LastCandle, ticker.Timestamp))
                {
                    RefreshData().Wait();
                }

                kl = ((KlineCandle)data.LastOrDefault()) ?? new KlineCandle() { Timestamp = n, Type = this.KlineType };

                if (p < kl.LowPrice)
                {
                    kl.LowPrice = p;
                }
                else if (p > kl.HighPrice)
                {
                    kl.HighPrice = p;
                }
                    
                kl.ClosePrice = p;

                if (VolumeTime != null && Candle.IsTimeInCandle(kl, (DateTime)VolumeTime)) 
                {
                    kl.Volume = Volume ?? 0;
                }

                RealTimeTicker = ticker;
                RealTimePrice = ticker.Price;

            }).Wait();

        }

        public async Task<bool> LoginUser()
        {
            cred = CryptoCredentials.LoadFromStorage(App.Current.Seed);

            if (cred.IsFilled)
            {
                try
                {
                    user = new User(cred);
                    Accounts = await user.GetAccountList();

                    IsLoggedIn = true;

                    Kucoin.NET.Helpers.Dispatcher.InvokeOnMainThread(async (o) =>
                    {
                        try
                        {
                            tickerFeed?.Dispose();
                        }
                        catch { }
                        try
                        {
                            klineFeed?.Dispose();
                        }
                        catch { }
                        try
                        {
                            level2Feed?.Dispose();
                        }
                        catch { }

                        level2Feed = new Level3(cred);
                        level2Feed.MonitorThroughput = true;

                        tickerFeed = new TickerFeed();
                        klineFeed = new KlineFeed<KlineCandle>();

                        tickerFeed.Subscribe(this);
                        klineFeed.Subscribe(this);

                        symbol = null;

                        await Initialize();
                    });

                    return true;
                }
                catch
                {                    
                }
            }

            Accounts = null;
            IsLoggedIn = false;

            return false;

        }

        public void OnError(Exception ex)
        {
            throw ex;
        }

        public void OnCompleted()
        {
        }

        public MainWindowViewModel()
        {
            App.Current.Settings.PropertyChanged += Settings_PropertyChanged;
            market = Market.Instance;
            lastRange = App.Current.Settings.LastCandleRange;

            // Make sure you call this from the main thread,
            // alternately, create the feeds on the main thread
            // this will allow them to synchronize.
            Kucoin.NET.Helpers.Dispatcher.Initialize();

            // another way to initialize the dispatcher is
            // to create a new feed on the main thread, like this.
            tickerFeed = new TickerFeed();
            klineFeed = new KlineFeed<KlineCandle>();

            // The IObservable<T> / IObserver<T> pattern:
            tickerSubscription = tickerFeed.Subscribe(this);
            klineSubscription = klineFeed.Subscribe(this);

            RefreshSymbolsCommand = new SimpleCommand(async (obj) =>
            {
                await market.RefreshSymbolsAsync();
                await market.RefreshCurrenciesAsync();

                var rec = App.Current.Settings.MostRecentSymbol;

                OnPropertyChanged(nameof(RecentSymbols));

                if (string.IsNullOrEmpty(rec)) return;

                foreach(var sym in symbols)
                {
                    if (sym.Symbol == rec)
                    {
                        Symbol = sym;
                        return;
                    }
                }

            });

            RefreshKlineCommand = new SimpleCommand(async (obj) =>
            {
                await RefreshData();
            });

            ShowAccountsCommand = new SimpleCommand((obj) =>
            {

                if (accountWnd == null || !accountWnd.IsLoaded)
                {
                    accountWnd = new Accounts(cred);
                    accountWnd.Closed += AccountWnd_Closed;
                }

                accountWnd.Show();
                accountWnd.Focus();


            });

            EditCredentialsCommand = new SimpleCommand(async (obj) =>
            {
                if (EditCredentials())
                {
                    await LoginUser();
                }
            });

            QuitCommand = new SimpleCommand((obj) =>
            {
                AskQuit?.Invoke(this, new EventArgs());
            });

            ShowFuturesCommand = new SimpleCommand((obj) =>
            {
                if (App.Current.Futures != null)
                {
                    App.Current.Futures.Show();
                    App.Current.Futures.Focus();
                }
                else
                {
                    App.Current.Futures = new FuturesWindow();
                    App.Current.Futures.Show();
                }

            });

            Task.Run(async () =>
            {
                await Initialize();
            });
        }

        protected void AccountWnd_Closed(object sender, EventArgs e)
        {
            accountWnd.Closed -= AccountWnd_Closed;
            accountWnd = null;

            GC.Collect(0);
        }

        protected override async Task Initialize()
        {

            if (market == null) market = Market.Instance;
            await CurrencyViewModel.UpdateCurrencies();

            await market.RefreshSymbolsAsync().ContinueWith(async (t) =>
            {
                await market.RefreshCurrenciesAsync();

                foreach (var c in market.Currencies)
                {
                    currencies.Add(new CurrencyViewModel(c, false));
                }

                await App.Current?.Dispatcher?.Invoke(async () => {

                    string pin;

                    if (CryptoCredentials.Pin == null)
                    {
                        pin = await PinWindow.GetPin(App.Current.MainWindow);
                    }
                    else
                    {
                        pin = CryptoCredentials.Pin;
                    }
                   
                    cred = CryptoCredentials.LoadFromStorage(App.Current.Seed, pin, false);

                    if (cred != null && !cred.IsFilled) cred = null;

                    if (cred == null && PinWindow.LastCloseResult == false)
                    {
                        AskQuit?.Invoke(this, new EventArgs());
                        return;
                    }

                    // Bring up the testing console.

                    // Uncomment to use.
                    // await Program.TestMain(cred);

                    // Note, closing the testing console once it is open will close the program.

                    Symbols = market.Symbols;

                    var rec = App.Current.Settings.MostRecentSymbol;
                    OnPropertyChanged(nameof(RecentSymbols));

                    if (cred != null)
                    {
                        Level3Feed = new Level3(cred);
                        level2Feed.MonitorThroughput = true;

                        if (!cred.Sandbox)
                        {

                            // connection sharing / multiplexing

                            // you can attach a public client feed
                            // to a protected host feed, but not vice-versa.
                            // the protected feed needs credentials, 
                            // and the public feed does not include them.

                            // we connect level2Feed.

                            // give its own socket because of the speed of data.
                            //await level2Feed.Connect(true);
                            try
                            {
                                if (!await level2Feed?.Connect())
                                {
                                    MessageBox.Show("Credentials Invalid");
                                    cred = null;
                                }

                                await tickerFeed.Connect(true);
                                await klineFeed.MultiplexInit(tickerFeed);
                            }
                            catch (Exception ex)
                            {
                                Kucoin.NET.Helpers.Dispatcher.InvokeOnMainThread((o) =>
                                {
                                    MessageBox.Show("Credentials Invalid");
                                    cred = null;
                                });
                            }

                            //await level3Feed.Connect();

                            // for testing futures
                            //await futuresl2.Connect();

                            // we attach tickerFeed and klineFeed 
                            // by calling MultiplexInit with the host feed.
                            //await tickerFeed.MultiplexInit(level2Feed);
                            //await klineFeed.MultiplexInit(level2Feed);

                            // Now we have the multiplex host (tickerfeed)
                            // and the multiplexed client (klineFeed)

                            // the connection only exists on the multiplex host
                            // which serves as the connection maintainer.
                        }
                        else
                        {
                            try
                            {
                                await level2Feed?.Connect();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Credentials Invalid");
                                cred = null;
                            }

                            await tickerFeed.Connect(true);
                            await klineFeed.MultiplexInit(tickerFeed);
                        }
                    }
                    else
                    {
                        CryptoCredentials.Pin = pin;

                        await tickerFeed.Connect(true);
                        await klineFeed.MultiplexInit(tickerFeed);
                    }

                    if (cred == null || !cred.IsFilled)
                    {
                        _ = Task.Run(() =>
                        {
                            Kucoin.NET.Helpers.Dispatcher.InvokeOnMainThread(async (obj) =>
                            {
                                if (EditCredentials())
                                {
                                    await LoginUser();
                                }
                            });
                        });

                        return;
                    }

                    if (rec == null) return;

                    foreach (var sym in symbols)
                    {
                        if (sym.Symbol == rec)
                        {
                            Symbol = sym;

                            if (cred == null)
                            {
                                _ = Task.Run(() =>
                                {
                                    Kucoin.NET.Helpers.Dispatcher.InvokeOnMainThread(async (obj) =>
                                    {
                                        if (EditCredentials())
                                        {
                                            await LoginUser();
                                        }
                                    });
                                });

                                return;
                            }

                            return;
                        }
                    }

                });
            });


        }

        private async void Level3Feed_FeedDisconnected(object sender, FeedDisconnectedEventArgs e)
        {
            if (Level3Feed.Disposed == false)
            {
                await Level3Feed.Connect();
                UpdateSymbol((string)symbol, (string)symbol, true);
            }

        }

        public override void Dispose()
        {
            level2Feed?.Dispose();

            tickerSubscription.Dispose();
            tickerFeed.Dispose();

            try
            {
                App.Current.Settings.PropertyChanged -= Settings_PropertyChanged;
            }
            catch { }
        }

        ~MainWindowViewModel()
        {
            Dispose();
        }

        protected void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Settings.Symbols))
            {
                OnPropertyChanged(nameof(RecentSymbols));
            }
        }
    }
}
