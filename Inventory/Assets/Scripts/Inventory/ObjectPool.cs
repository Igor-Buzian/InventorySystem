using UnityEngine;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string poolName; // Имя пула
        public GameObject prefab; // Префаб
        public int size; // Размер пула
    }

    public List<Pool> pools; // Список пулов
    private Dictionary<string, Queue<GameObject>> poolDictionary;
    public static ObjectPool Instance { get; private set; }



    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (var pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.poolName, objectPool);
        }
    }

    public GameObject Get(string poolName)
    {
        if (poolDictionary.ContainsKey(poolName) && poolDictionary[poolName].Count > 0)
        {
            GameObject obj = poolDictionary[poolName].Dequeue();
            obj.SetActive(true);
            return obj;
        }
        return null; // Если пул пуст, вернем null
    }

    public void Return(string poolName, GameObject obj)
    {
        if (poolDictionary.ContainsKey(poolName))
        {
            //obj.SetActive(false);
            poolDictionary[poolName].Enqueue(obj);
        }
    }
}