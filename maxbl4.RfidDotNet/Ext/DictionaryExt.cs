using System;
using System.Collections.Generic;

namespace maxbl4.RfidDotNet.Ext
{
    public static class DictionaryExt
    {
        public static TV Get<TK, TV>(this IDictionary<TK, TV> dict, TK key, Func<TK,TV> def = null)
        {
            if (def == null)
                def = x => default(TV);
            if (dict.TryGetValue(key, out var v))
                return v;
            return def(key);
        }
        
        public static void AddOrUpdate<TK,TV>(this IDictionary<TK, TV> dict, TK key, TV newValue, Func<TK,TV,TV> update)
        {
            if (!dict.ContainsKey(key))
                dict[key] = newValue;
            else
                dict[key] = update(key, dict[key]);
        }
    }
}