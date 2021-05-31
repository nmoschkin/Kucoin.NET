﻿using Kucoin.NET.Helpers;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuCoinApp.Converters
{
    public class AttachedAccountConverter : JsonConverter<ICredentialsProvider>
    {
        public override ICredentialsProvider ReadJson(JsonReader reader, Type objectType, ICredentialsProvider existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return (ICredentialsProvider)serializer.Deserialize(reader, typeof(CryptoCredentials));
        }

        public override void WriteJson(JsonWriter writer, ICredentialsProvider value, JsonSerializer serializer)
        {
            if (value is CryptoCredentials cred)
            {
                serializer.Serialize(writer, value);
            }
            else
            {
                return;
            }
        }
    }
}
