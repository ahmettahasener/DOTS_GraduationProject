// ChunkPoolManager.cs
using System.Collections.Generic;
using UnityEngine;

public class ChunkPoolManager : MonoBehaviour
{
    public static ChunkPoolManager Instance { get; private set; } // Singleton Pattern
    public GameObject[] chunkPrefabs; // Farklý chunk prefab'larýnýzý buraya sürükleyin
    public int initialPoolSize = 10; // Baþlangýçta havuzda kaç chunk olacak

    private Dictionary<string, Queue<GameObject>> chunkPools; // Her prefab tipi için ayrý bir kuyruk

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        InitializePool();
    }

    private void InitializePool()
    {
        chunkPools = new Dictionary<string, Queue<GameObject>>();
        foreach (GameObject prefab in chunkPrefabs)
        {
            string prefabName = prefab.name;
            chunkPools.Add(prefabName, new Queue<GameObject>());

            for (int i = 0; i < initialPoolSize; i++)
            {
                GameObject chunk = Instantiate(prefab, transform);
                chunk.SetActive(false); // Baþlangýçta inaktif
                chunkPools[prefabName].Enqueue(chunk);
            }
        }
    }

    public GameObject GetChunk(string prefabName)
    {
        if (chunkPools.ContainsKey(prefabName) && chunkPools[prefabName].Count > 0)
        {
            GameObject chunk = chunkPools[prefabName].Dequeue();
            chunk.SetActive(true);
            return chunk;
        }
        else
        {
            // Havuzda yoksa veya yeterli deðilse yeni bir tane oluþtur
            foreach (GameObject prefab in chunkPrefabs)
            {
                if (prefab.name == prefabName)
                {
                    GameObject newChunk = Instantiate(prefab, transform);
                    return newChunk;
                }
            }
            Debug.LogError($"Chunk prefab not found: {prefabName}");
            return null;
        }
    }

    public void ReturnChunk(GameObject chunkToReturn)
    {
        chunkToReturn.SetActive(false);
        string prefabName = chunkToReturn.name.Replace("(Clone)", ""); // Prefab adýný bul
        if (chunkPools.ContainsKey(prefabName))
        {
            chunkPools[prefabName].Enqueue(chunkToReturn);
        }
        else
        {
            // Bu durum genellikle olmaz, ancak hata ayýklama için býrakýlabilir
            Debug.LogWarning($"Attempted to return unknown chunk: {chunkToReturn.name}");
            Destroy(chunkToReturn); // Güvenlik için yok et
        }
    }

    // Belirli bir prefab adýyla rastgele bir chunk alma fonksiyonu
    public GameObject GetRandomChunk()
    {
        if (chunkPrefabs.Length == 0) return null;
        GameObject randomPrefab = chunkPrefabs[Random.Range(0, chunkPrefabs.Length)];
        return GetChunk(randomPrefab.name);
    }
}