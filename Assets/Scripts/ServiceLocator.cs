using System;
using System.Collections.Generic;
using UnityEngine;

public class ServiceLocator
{
    private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
    private static ServiceLocator _instance;

    private static ServiceLocator Instance
    {
        get
        {
            if(_instance == null)
                _instance = new ServiceLocator();

            return _instance;
        }
    }

    public static void AddService(object service)
    {
        if (Instance._services.ContainsKey(service.GetType()))
        {
            Debug.Log($"{nameof(service)} was already added.");
            return;
        }
        
        Instance._services.Add(service.GetType(), service);
    }

    public static void RemoveService(Type service)
    {
        Instance._services.Remove(service);
    }

    public static T GetService<T>() where T : class
    {
        return (T)Instance._services[typeof(T)];
    }
}
