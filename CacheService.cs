﻿using Newtonsoft.Json;
using StackExchange.Redis;

namespace RedisDotNet7API
{
    public class CacheService
    {
        private IDatabase _db;

        public CacheService(IConnectionMultiplexer redis)
        {
            //ConfigureRedis();
            _db = redis.GetDatabase();
        }
        
        private void ConfigureRedis()
        {
            _db = ConnectionHelper.Connection.GetDatabase();
        }

        public T GetData<T>(string key)
        {
            var value = _db.StringGet(key);
            if (!string.IsNullOrEmpty(value))
            {
                return JsonConvert.DeserializeObject<T>(value);
            }

            return default;
        }

        public bool SetData<T>(string key, T value, DateTimeOffset expTime)
        {
            TimeSpan expiryTime = expTime.DateTime.Subtract(DateTime.Now);
            var isSet = _db.StringSet(key, JsonConvert.SerializeObject(value), expiryTime);

            return isSet;
        }

        public object RemoveData(string key)
        {
            bool isKeyExist = _db.KeyExists(key);
            if (isKeyExist)
            {
                return _db.KeyDelete(key);
            }

            return false;
        }
    }
}
