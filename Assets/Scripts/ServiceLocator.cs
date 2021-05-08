using System;
using System.Collections.Generic;
using UnityEngine;

public class ServiceLocator
{
    private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
    private static ServiceLocator _instance;

    private static readonly object lockObj = new object();
    
    private static ServiceLocator Instance
    {
        get
        {
            if(_instance == null)
                _instance = new ServiceLocator();

            return _instance;
        }
    }

    public static void AddService<T>(object service)
    {
        lock (lockObj)
        {
            if (Instance._services.ContainsKey(typeof(T)))
            {
                Debug.Log($"{typeof(T)} was already added.");
                return;
            }
        
            Instance._services.Add(typeof(T), service);
        }
    }

    public static void RemoveService<T>()
    {
        lock (lockObj)
        {
            Instance._services.Remove(typeof(T));
        }
    }

    public static T GetService<T>() where T : class
    {
        lock (lockObj)
        {
            return (T)Instance._services[typeof(T)];
        }
    }
}
