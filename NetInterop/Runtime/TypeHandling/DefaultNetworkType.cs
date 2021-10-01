﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Concurrent;
using NetInterop.Transport.Core.Abstractions.Packets;
using NetInterop.Abstractions;

namespace NetInterop
{
    public class DefaultNetworkType<T> : INetworkType, INetworkType<T>
    {
        private readonly IPointerProvider pointerProvider;
        private readonly IActivator<T> activator;
        private readonly bool isDisposable;
        private ConcurrentBag<ushort> freedIds = new ConcurrentBag<ushort>();
        private readonly IDeactivator<T> disposer;
        private ushort instanceIndex = 0;
        private T[] instances = new T[ushort.MaxValue];
        private readonly object locker = new object();

        public ushort Id { get; set; }

        public DefaultNetworkType(ushort id, IPointerProvider pointerProvider, IActivator<T> activator, IDeactivator<T> disposer = null)
        {
            this.activator = activator ?? throw new NullReferenceException(nameof(activator)); ;
            this.Id = id;
            this.pointerProvider = pointerProvider ?? throw new ArgumentNullException(nameof(pointerProvider));
            isDisposable = typeof(T).GetInterface(nameof(IDisposable)) != null;
            this.disposer = disposer;
        }

        public INetPtr AllocPtr()
        {
            ushort instance = GetNewAddress();

            T newInstance = activator.CreateInstance();

            lock (locker)
            {
                instances[instance] = newInstance;
            }

            return pointerProvider.Create<T>(Id, instance);
        }

        public T GetPtr(INetPtr<T> ptr)
        {
            lock (locker)
            {
                return instances[ptr.PtrAddress];
            }
        }

        public void SetPtr(INetPtr ptr, object value)
        {
            if (ptr.PtrType == 0)
            {
                throw new NullReferenceException($"Provided ptr was null: {ptr}");
            }
            if (value is T isT)
            {
                lock (locker)
                {
                    instances[ptr.PtrAddress] = isT;
                }
            }
            else
            {
                throw new InvalidCastException($"Failed to cast pointer {ptr} to {typeof(T).FullName}, expected type {this.Id} got {ptr.PtrType} ({value})");
            }
        }

        public void SetPtr(INetPtr<T> ptr, T value)
        {
            lock (locker)
            {
                instances[ptr.PtrAddress] = value;
            }
        }

        object INetworkType.GetPtr(INetPtr ptr)
        {
            lock (locker)
            {
                return instances[ptr.PtrAddress];
            }
        }

        public void Free(INetPtr ptr)
        {
            DisposeManagedT(this.GetPtr(ptr.As<T>()));

            freedIds.Add(ptr.PtrAddress);
        }

        public void FreeAll()
        {
            T[] safeCopy = new T[instances.Length];

            lock (locker)
            {
                instances.CopyTo(safeCopy, 0);
            }

            for (int i = 0; i < safeCopy.Length; i++)
            {
                DisposeManagedT(safeCopy[i]);
            }

            freedIds = new ConcurrentBag<ushort>();

            lock (locker)
            {
                instances = new T[ushort.MaxValue];
            }
        }

        private ushort GetNewAddress()
        {
            if (freedIds.IsEmpty)
            {
                return instanceIndex++;
            }

            if (freedIds.TryTake(out ushort newId))
            {
                return newId;
            }

            return instanceIndex++;
        }

        private void DisposeManagedT(T instance)
        {
            disposer?.DestroyInstance(instance);
        }
    }
}
