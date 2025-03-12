using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string poolName;
        public GameObject prefab;
        public int size;
        public Transform instantiateTransform;
    }

    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> _poolDictionary;
    public static ObjectPool Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;

        _poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(
                    pool.prefab,
                    pool.instantiateTransform.position,
                    Quaternion.identity
                );
                obj.SetActive(true);
                objectPool.Enqueue(obj);
            }
            _poolDictionary.Add(pool.poolName, objectPool);
        }
    }

    public GameObject Get(string poolName)
    {
        if (!_poolDictionary.TryGetValue(poolName, out Queue<GameObject> pool)) return null;

        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        Pool poolData = pools.Find(p => p.poolName == poolName);
        if (poolData == null) return null;

        GameObject newObj = Instantiate(
            poolData.prefab,
            poolData.instantiateTransform.position + Vector3.up,
            Quaternion.identity
        );
        newObj.SetActive(true);
        return newObj;
    }

    public void Return(string poolName, GameObject obj)
    {
        if (!_poolDictionary.ContainsKey(poolName))
            _poolDictionary[poolName] = new Queue<GameObject>();

        Item item = obj.GetComponent<Item>();
        if (item != null) item.ResetState();

        obj.SetActive(false);
        _poolDictionary[poolName].Enqueue(obj);
    }
}