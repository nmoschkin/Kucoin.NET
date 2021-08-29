﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Websockets.Distribution
{
    public interface IFeedState
    {  
        /// <summary>
        /// Gets the current state of the feed.
        /// </summary>
        FeedState State { get; }

        /// <summary>
        /// Refresh the current state of the feed.
        /// </summary>
        void RefreshState();
    }
}
