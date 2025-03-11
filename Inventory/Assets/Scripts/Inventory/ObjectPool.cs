using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string poolName;      // Pool name (e.g. "WeaponPool")
        public GameObject prefab;    // Prefab for the pool
        public int size;             // Number of instances in pool
        public Transform instantiateTransform; // Transform for instantiation position
    }

    public List<Pool> pools;  // List of pools to initialize
    private Dictionary<string, Queue<GameObject>> poolDictionary;
    public static ObjectPool Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        // Create each pool and enqueue inactive objects
        foreach (var pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < pool.size; i++)
            {
                // Устанавливаем позицию для инициализации
                GameObject obj = Instantiate(pool.prefab, pool.instantiateTransform.position, Quaternion.identity);
                obj.SetActive(true); // Делаем объект активным сразу
                objectPool.Enqueue(obj);
            }
            poolDictionary.Add(pool.poolName, objectPool);
        }
    }

    public GameObject Get(string poolName)
    {
        if (poolDictionary.TryGetValue(poolName, out Queue<GameObject> pool) && pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        Debug.LogWarning($"No available object in pool: {poolName}, instantiating a new one.");

        if (pools.Exists(p => p.poolName == poolName))
        {
            Pool poolData = pools.Find(p => p.poolName == poolName);
            GameObject newObj = Instantiate(poolData.prefab, poolData.instantiateTransform.position + Vector3.up, Quaternion.identity);
            newObj.SetActive(true);
            return newObj;
        }

        return null;
    }

    // Return an object back to the pool and deactivate it
    public void Return(string poolName, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(poolName))
            poolDictionary[poolName] = new Queue<GameObject>();

        // Сбрасываем состояние объекта
        var item = obj.GetComponent<Item>();
        if (item != null) item.ResetState();

        obj.SetActive(false);
        poolDictionary[poolName].Enqueue(obj);
    }
}