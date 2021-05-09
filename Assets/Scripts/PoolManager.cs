using System;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    [SerializeField] private List<PooledObject> objects;
    
    private readonly Dictionary<string, Queue<GameObject>> _objectsPool = new Dictionary<string, Queue<GameObject>>();

    private void Awake()
    {
        GameLoader.ServiceLocatorInitialized.AddListener(OnServiceLocatorInitialized);
    }

    private void OnServiceLocatorInitialized()
    {
        ServiceLocator.AddService<PoolManager>(this);
    }

    public void InitializePool()
    {
        Debug.Log("Pool Manager initializing...");
        foreach (var obj in objects)
            InstantiateObjects(obj.prefab, obj.poolingCount);   
    }
    
    public GameObject GetPooledObjectByTag(string tag)
    {
        if (!_objectsPool.ContainsKey(tag))
            return null;

        if (_objectsPool[tag].Count == 0)
            ExtendPoolByTag(tag);

        GameObject obj = _objectsPool[tag].Dequeue();
        while (obj == null)
            obj = _objectsPool[tag].Dequeue();
        
        obj.SetActive(true);
        return obj;
    }

    public T GetPooledObjectByTag<T>(string tag) where T : class
    {
        GameObject obj = GetPooledObjectByTag(tag);

        if (obj == null)
            return null;

        return obj.GetComponent<T>();
    }

    public void GiveObjectToPool(GameObject obj)
    {
        if(!_objectsPool.ContainsKey(obj.tag))
            return;
        
        obj.SetActive(false);
        obj.transform.SetParent(transform);
        obj.transform.position = Vector3.zero;
        _objectsPool[obj.tag].Enqueue(obj);
    }

    private void ExtendPoolByTag(string tag)
    {
        foreach (var obj in objects)
        {
            if(obj.prefab.tag.Equals(tag))
                InstantiateObjects(obj.prefab, obj.poolingCount);
        }
    }
    
    private void InstantiateObjects(GameObject prefab, int count)
    {
        if (!_objectsPool.ContainsKey(prefab.tag))
            _objectsPool.Add(prefab.tag, new Queue<GameObject>());
        
        GameObject newObj = null;
        for (int i = 0; i < count; i++)
        {
            newObj = Instantiate(prefab, transform);
            newObj.SetActive(false);
            _objectsPool[prefab.tag].Enqueue(newObj);
        }
    }
    
    [Serializable]
    private class PooledObject
    {
        public GameObject prefab;
        public int poolingCount;
    }
}
