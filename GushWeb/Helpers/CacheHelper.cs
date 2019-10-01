using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GushWeb.Helpers
{
    public static class CacheHelper
    {
        private static Dictionary<string, object> CacheDictionary = new Dictionary<string, object>();
        public static void Add(string key, object value)
        {
            CacheDictionary.Add(key, value);
        }

        public static void Remove(string key)
        {
            CacheDictionary.Remove(key);
        }

        public static void Clear()
        {
            CacheDictionary.Clear();
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        public static T Get<T>(string key)
        {
            return (T)CacheDictionary[key];
        }

        /// <summary>
        /// 判断缓存是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool Exsits(string key)
        {
            return CacheDictionary.ContainsKey(key);
        }

        /// <summary>
        /// 缓存获取方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">缓存字典容器对应key</param>
        /// <param name="func">委托方法 传入操作对象</param>
        /// <returns></returns>
        public static T GetCache<T>(string key, Func<T> func)
        {
            T t = default(T);
            if (CacheHelper.Exsits(key))
            {
                //缓存存在，直接获取原数据
                t = CacheHelper.Get<T>(key);
            }
            else
            {
                //缓存不存在，去生成缓存，并加入容器
                t = func.Invoke();
                CacheHelper.Add(key, t);
            }
            return t;
        }
    }
}