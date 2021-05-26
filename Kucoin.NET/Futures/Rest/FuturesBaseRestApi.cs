﻿using Kucoin.NET.Data.Interfaces;
using Kucoin.NET.Rest;

using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Text;

namespace Kucoin.NET.Futures.Rest
{
    public abstract class FuturesBaseRestApi : KucoinBaseRestApi
    {
        public FuturesBaseRestApi(ICredentialsProvider cred) : base(cred, futures: true)
        {
            if (cred != null && cred.GetFutures() != true)
            {
                throw new InvalidCredentialException("Must be marked as Futures credentials.");
            }
        }

        public FuturesBaseRestApi(
            string key,
            string secret,
            string passphrase,
            bool isSandbox = false)
            : base(
                key,
                secret,
                passphrase,
                isSandbox:
                isSandbox,
                futures: true)
        {
        }
    }
}
