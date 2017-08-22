using System;
using System.Collections.Generic;

namespace Castalia.Mvvm.Services.Core
{
    public class ServiceContainer
    {
        private readonly Dictionary<Type, object> serviceMap;
        private readonly object serviceMapLock;

        public static readonly ServiceContainer Instance = new ServiceContainer();

        private ServiceContainer()
        {
            serviceMap = new Dictionary<Type, object>();
            serviceMapLock = new object();
        }

        public void AddService<TServiceContract>(TServiceContract implementation)
            where TServiceContract : class
        {
            lock (serviceMapLock)
            {
                serviceMap[typeof (TServiceContract)] = implementation;
            }
        }

        public TServiceContract GetService<TServiceContract>()
            where TServiceContract : class
        {
            object service;
            lock (serviceMapLock)
            {
                serviceMap.TryGetValue(typeof(TServiceContract), out service);
            }
            return service as TServiceContract;
        }

    }
}