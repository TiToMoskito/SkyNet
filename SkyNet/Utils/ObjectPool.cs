using System;
using System.Collections.Generic;

namespace SkyNet
{
    public class ObjectPool<T> where T : new()
    {
        private List<T> mPool;

        public ObjectPool()
        {
            mPool = new List<T>();
        }

        public void Return(T obj)
        {
            if (mPool.Contains(obj))
                SkyLog.Error("Duplicate return for {0}: \r\n{1}", obj, Environment.StackTrace);
            mPool.Add(obj);
        }

        public T Get()
        {
            if (mPool.Count <= 0)
                return new T();
            T obj = mPool[0];
            mPool.RemoveAt(0);
            return obj;
        }
    }
}
