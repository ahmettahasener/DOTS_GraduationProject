// ChunkController.cs
using UnityEngine;

public class ChunkController : MonoBehaviour
{
    public Vector2Int ChunkCoordinates { get; private set; } // Bu chunk'ýn ýzgaradaki koordinatlarý

    public void Initialize(Vector2Int coordinates)
    {
        ChunkCoordinates = coordinates;
        // Chunk'a özgü ek baþlangýç iþlemleri burada yapýlabilir
    }

    // Chunk'ý devre dýþý býrakýrken çaðrýlacak metod
    public void Deactivate()
    {
        // Örneðin, tüm child objelerini kapatabilir veya render'larýný devre dýþý býrakabilirsiniz
        gameObject.SetActive(false);
    }

    // Chunk'ý aktifleþtirirken çaðrýlacak metod
    public void Activate()
    {
        gameObject.SetActive(true);
        // Chunk'a özgü ek aktivasyon iþlemleri burada yapýlabilir
    }
}