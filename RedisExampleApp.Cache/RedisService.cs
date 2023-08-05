using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace RedisExampleApp.Cache
{
    public class RedisService
    {
        private readonly ConnectionMultiplexer _redis;

        public RedisService(string stringUrl)
        {
            _redis = ConnectionMultiplexer.Connect(stringUrl);
        }

        public IDatabase GetDb(int db)
        {
            return _redis.GetDatabase(db);
        }

    }
}
