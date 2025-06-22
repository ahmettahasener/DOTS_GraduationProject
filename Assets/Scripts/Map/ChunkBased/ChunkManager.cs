// ChunkManager.cs
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public Transform playerTransform; // Karakterinizin Transform'u
    public float chunkSize = 10f; // Chunk'larýn bir kenar uzunluðu (örn: 10 birim)
    public int renderDistance = 3; // Oyuncunun etrafýnda kaç chunk görünecek (örn: 3x3 bir alan için 1)
                                   // renderDistance = 1 ise 3x3 alan (merkez + 8 komþu)
                                   // renderDistance = 2 ise 5x5 alan (merkez + 24 komþu)

    private Vector2Int lastPlayerChunkCoords;
    private Dictionary<Vector2Int, GameObject> activeChunks; // Aktif chunk'larý tutar

    private void Start()
    {
        activeChunks = new Dictionary<Vector2Int, GameObject>();
        lastPlayerChunkCoords = GetChunkCoordinates(playerTransform.position);
        GenerateInitialChunks();
    }

    private void Update()
    {
        Vector2Int currentPlayerChunkCoords = GetChunkCoordinates(playerTransform.position);

        if (currentPlayerChunkCoords != lastPlayerChunkCoords)
        {
            UpdateChunks(currentPlayerChunkCoords);
            lastPlayerChunkCoords = currentPlayerChunkCoords;
        }
    }

    // Dünya pozisyonundan chunk koordinatlarýný alýr
    Vector2Int GetChunkCoordinates(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / chunkSize);
        int y = Mathf.FloorToInt(worldPosition.y / chunkSize); // 2D top-down olduðu için y eksenini kullanýyoruz
        return new Vector2Int(x, y);
    }

    // Baþlangýçta chunk'larý oluþtur
    void GenerateInitialChunks()
    {
        UpdateChunks(lastPlayerChunkCoords);
    }

    // Chunk'larý güncelleme ve yönetim ana fonksiyonu
    void UpdateChunks(Vector2Int currentPlayerChunkCoords)
    {
        HashSet<Vector2Int> chunksToKeep = new HashSet<Vector2Int>();

        // Yeni görünür alandaki chunk'larý belirle ve oluþtur/aktifleþtir
        for (int x = -renderDistance; x <= renderDistance; x++)
        {
            for (int y = -renderDistance; y <= renderDistance; y++)
            {
                Vector2Int targetChunkCoords = new Vector2Int(currentPlayerChunkCoords.x + x, currentPlayerChunkCoords.y + y);
                chunksToKeep.Add(targetChunkCoords);

                if (!activeChunks.ContainsKey(targetChunkCoords))
                {
                    // Yeni bir chunk gerekiyor, havuzdan al veya oluþtur
                    GameObject chunkObj = ChunkPoolManager.Instance.GetRandomChunk(); // Rastgele chunk prefabý seçebilirsiniz
                    if (chunkObj != null)
                    {
                        chunkObj.name = "Chunk_" + targetChunkCoords.x + "_" + targetChunkCoords.y; // Kolay takip için isim verelim
                        // Chunk'ýn sol alt köþesini dünya pozisyonuna ayarlayalým
                        chunkObj.transform.position = new Vector3(targetChunkCoords.x * chunkSize, targetChunkCoords.y * chunkSize, 0);
                        ChunkController chunkController = chunkObj.GetComponent<ChunkController>();
                        if (chunkController != null)
                        {
                            chunkController.Initialize(targetChunkCoords);
                            chunkController.Activate();
                        }
                        activeChunks.Add(targetChunkCoords, chunkObj);
                    }
                }
            }
        }

        // Artýk oyuncu etrafýnda olmayan chunk'larý devre dýþý býrak
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        foreach (var entry in activeChunks)
        {
            if (!chunksToKeep.Contains(entry.Key))
            {
                chunksToRemove.Add(entry.Key);
            }
        }

        foreach (Vector2Int coords in chunksToRemove)
        {
            GameObject chunkObj = activeChunks[coords];
            ChunkController chunkController = chunkObj.GetComponent<ChunkController>();
            if (chunkController != null)
            {
                chunkController.Deactivate(); // Chunk'ý devre dýþý býrak
            }
            ChunkPoolManager.Instance.ReturnChunk(chunkObj); // Havuza geri döndür
            activeChunks.Remove(coords);
        }
    }
}