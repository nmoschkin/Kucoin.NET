# Kucoin.NET v1.0a
KuCoin and KuCoin Futures API Libraries for .NET.

_This library is a client for the KuCoin API as described in (https://docs.kucoin.com/#general)._

__ATTENTION__: __The WPF Project has been moved to the 'legacy' branch.__

### September 7th, 2021: 

After a major rewrite of a portion of the Websockets feeds to support parallel delegation and Level 3 direct feeds, I have
decided to sunset the WPF projects and I have removed the FancyCandles submodule.

Going forward, I will be developing UI/UX apps with MAUI or WinUI 3 / Project Reunion, in other repositories.

This repository is now reduced to the core library, the console app and a credentials editing tool.

The entire project has been retargeted to .NET 6, except for Kucoin.NET.Std, which is .NET Standard 2.0.  

## Installation and Getting Started

There are two solutions, one that contains projects that reference the .NET 6 library, and the other that contains projects that reference the .NET Standard 2.0 library.

__The usage guidelines are as follows:__

 * New projects targeting Desktop, Console, .NET MAUI, or Blazor should use the .NET 6.0 library.
 * Older projects targeting the .NET Framework or Xamarin, or Mono projects should use the .NET Standard 2.0 library.

 _Note: Even though the projects in the .NET Standard solution reference the .NET Standard KuCoin.NET library, they are still native apps built in .NET 6._

## Creating Credentials

The Windows Forms-based credentials editor will let you enter your KuCoin API credentials and save them to the disk, encrypted using a 6 digit pin.

_(Note: .NET 5/6 Forms projects will generally compile in Linux without modification, as Mono wraps the Gtk+ libs.)_

You can use whatever pin you like, but the credentials you use will be loaded for that pin.  If you use another pin you will essentially create a
new set of credentials.  

After you have created the credentials, you can run the console app and type in the same pin you used to load the credentials.

The API library, itself, does not have any mechanism for persisting or storing credentials.  If you wish to use an alternative to the
__CryptoCredentials__ class, provided, you will have to implement your own __ICredentialsProvider__.

![](docs/docimg2.png?raw=true)

## Running the Sample Console App
  
The sample app starts multiple Level3Direct feeds (see below.)

When you run the sample app, it will look for credentials configured by the Windows Forms app, by default.  

Program.cs includes a skeleton class implementation of __ICredentialsProvider__ if you wish to use that, instead.

The user will be prompted to enter their pin, and then be asked to provide either a number to list the top _n_ feeds by 24-hr volume, or provide a list of feeds separated by commas.

The console app only demonstrates the ability to track Level 3 feeds.  Other functionality is not demonstrated in the examples, at this time.

![](docs/docimg1.png?raw=true)

## Using the Library

_(Note: A Wiki will be coming soon!)_

The starting point for the entire system is the __Kucoin.NET.KuCoin__ static class.  From here you will be able to register credentials 
and create and acquire __IServiceFactory__ and  __ISymbolDataService__ instances, as well as initialize market data and the dispatcher.

### REST API 

Inside the __Kucoin.NET.Rest__ namespace you will find three objects, __Market__, __Trade__, __Margin__, and __User__.  __Margin__, __Trade__, and __User__ require API Keys.  

There are two credentials providers, there is the default __MemoryEncryptedCredentialsProvider__ that is in the Kucoin.NET library, and then the __CryptoCredentials__ class in the example app and the credentials editor app.

The __MemoryEncryptedCredentialsProvider__ will store the credentials encrypted in memory with a random seed, until they are needed.  __CryptoCredentials__ has the ability to load and save encrypted credentials sets to disk.  A 6 digit numeric pin is required to save and load credentials.  Both of these classes implement the __ICredentialsProvider__ interface which you can use to write your own provider.

### Websocket Feeds

Public feeds in the namespace __Kucoin.NET.Websockets.Public__:

  - __AllTickerFeed__ - Pushes all symbol tickers as they are updated.
  - __KlineFeed__ - Pushes the K-Line feed for the subscribed symbols.
  - __MarketFeed__ - Pushes an entire market.
  - __SnapshotFeed__ - Pushes market snapshots.
  - __TickerFeed__ - Pushes basic symbol price tickers.

Private feeds in the namespace __Kucoin.NET.Websockets.Private__:

  - __Level2__ - Pushes the full-depth Level 2 market feed (calibrated).
  - __Level2Depth5/Level2Depth50__ - Pushes the 5/50 best ask/bid static market depth feeds.
  - __Level3__ - Level 3 Full Match Engine 
  - __Level3Direct__ - Level 3 Full Match Engine that bypasses the parallel service dispatcher and updates order books from the socket thread. 

All of the feeds support multiplexing.  You may create a single feed object, and use that object's connection to start sub-channels that will be served to the multiplex child classes.  Multiplexing is implemented in the __KucoinBaseWebsocketFeed__ abstract class.  
  
  * Note: You cannot multiplex a private feed onto a public feed.

All of the feeds except for order book feeds implement the __IObservable<T>__ pattern.

## UI/UX Notes

In order to use any of the feeds in UI/UX/MVVM setting, you will need to initialize the __Kucoin.NET.Helpers.Dispatcher__ static class with a __SynchronizationContext__ from the __Dispatcher__ provided by your application (usually the App class, itself.)  Feed observations will not execute correctly without a __SynchronizationContext__, because they need to inform the UI thread. I'm currently researching alternatives to initializing a dispatcher, so these requirements may change, in the future.

## Other Notes

The library, itself, is pretty well documented, so far.

This project is epic, and there are bound to be bugs.  Feel free to open issues, and I will get to them, as I can, if I don't find the bugs, first.

## And finally, I take donations!  

If you like my work, and find it may be useful, you can donate crypto if you like!

My Ethereum wallet address is: 
  - 0xb97a29b4349cb3f66b7f2143c6ba1362b8ec4e7d

My Stellar Lumens (XLM) address is:
  - Address: GAJ4BSGJE6UQHZAZ5U5IUOABPDCYPKPS3RFS2NVNGFGFXGVQDLBQJW2P
  - Memo: 1870588215

My KCS wallet is:
  - 0xb97a29b4349cb3f66b7f2143c6ba1362b8ec4e7d


   


