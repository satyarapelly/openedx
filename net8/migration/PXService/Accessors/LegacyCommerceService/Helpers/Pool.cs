// <copyright file="Pool.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.Helpers
{
    using System;
    using System.Collections.Generic;

    public class PoolManager
    {
        private PoolManager() { }
        private static PoolManager _instance = new PoolManager();
        public static PoolManager GetInstance() { return _instance; }

        private Object syncObj = new object();
        private Dictionary<PoolKey, Pool> _dic = new Dictionary<PoolKey, Pool>();
        public Pool GetPool(Type type, string certLocation, Func<Object> factory)
        {
            Pool p = null;
            PoolKey key = new PoolKey(type, certLocation);
            if (!_dic.TryGetValue(key, out p))
            {
                lock (syncObj)
                {
                    if (!_dic.TryGetValue(key, out p))
                    {
                        p = new Pool(factory);
                        _dic.Add(key, p);
                    }
                }
            }
            return p;
        }
    }

    public class PoolKey
    {
        private Type type;
        private string certLocation;

        public PoolKey(Type type, string certLocation)
        {
            this.type = type;
            this.certLocation = certLocation;
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj.GetType() == typeof(PoolKey))
            {
                PoolKey key2 = (PoolKey) obj;
                return key2.type == this.type && key2.certLocation == this.certLocation;
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            return this.type.ToString() + this.certLocation ?? string.Empty;
        }

        public override int GetHashCode()
        {
            return (this.type.ToString() + this.certLocation ?? string.Empty).GetHashCode();
        }
    }

    public class Pool
    {
        private Stack<Object> itemSlots;
        private HashSet<Object> flags;
        private Func<Object> createObjMethod;
        private object syncobj = new object();
        public Pool(Func<Object> factory)
        {
            this.createObjMethod = factory;
            itemSlots = new Stack<Object>();
            flags = new HashSet<object>();
        }

        public Object Get()
        {
            lock (syncobj)
            {
                // if empty, create one and return directly
                if (itemSlots.Count <= 0)
                {
                    //Console.WriteLine(DateTime.Now.ToString() + " " + Thread.CurrentThread.Name + " Pool.Get() itemSlots.Count <= 0");
                    return createObjMethod();
                }

                //Console.WriteLine(DateTime.Now.ToString() + " " + Thread.CurrentThread.Name + " Pool.Get() itemSlots has free objs");
                Object o = itemSlots.Pop();
                flags.Remove(o);
                return o;
            }
        }

        public void Return(Object value)
        {
            lock (syncobj)
            {
                if (!flags.Contains(value))
                {
                    //Console.WriteLine(DateTime.Now.ToString() + " " + Thread.CurrentThread.Name + " Pool.Return() flags.Contains(obj)==false");

                    itemSlots.Push(value);
                    flags.Add(value);
                }
                else
                {
                    //Console.WriteLine(DateTime.Now.ToString() + " " + Thread.CurrentThread.Name + " Pool.Return() flags.Contains(obj)==true");

                }
            }
        }
    }

    public class PooledObjWrapper<T> : IDisposable where T : new()
    {

        private bool isDisposed;
        private Pool pool = null;
        private T target;

        public T Target { get { return target; } }

        public PooledObjWrapper(string certLocation)
        {
            //Console.WriteLine(DateTime.Now.ToString() + " " + Thread.CurrentThread.Name + " PooledObjWrapper.ctor() BEGIN");
            pool = PoolManager.GetInstance().GetPool(typeof(T), certLocation, () => { return new T(); });
            target = (T)pool.Get();
            if (target == null)
            {
                throw new InvalidOperationException("Cannot get object from pool.");
            }
        }

        ~PooledObjWrapper()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            //Console.WriteLine(DateTime.Now.ToString() + " " + Thread.CurrentThread.Name + " PooledObjWrapper.Dispose(" + disposing + ") BEGIN");
            if (!isDisposed)
            {
                if (disposing)
                {
                    // method called by Dispose() explicitly
                }
                pool.Return(target);
            }
            isDisposed = true;
        }
    }

}
