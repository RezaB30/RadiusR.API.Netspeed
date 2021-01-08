using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;

namespace RadiusR.API.Netspeed
{
    public static class CacheManager
    {
        static MemoryCache _cache = new MemoryCache("register");
        static string _namePrefix = "REG_";
        public static void Set(string password, string key, TimeSpan duration)
        {
            if (_cache.Get(key) == null)
            {
                _cache.Set(_namePrefix + key, password, GetPolicy(duration));
            }
        }
        public static string Get(string key)
        {
            var cacheKey = _cache.Get(_namePrefix + key);
            if (cacheKey == null)
            {
                return string.Empty;
            }
            return cacheKey as string;
        }
        private static CacheItemPolicy GetPolicy(TimeSpan _cacheDuration)
        {
            return new CacheItemPolicy()
            {
                AbsoluteExpiration = DateTimeOffset.Now.Add(_cacheDuration),
            };
        }
    }
}