using UnityEngine;

public class MainMenuAnimations : MonoBehaviour
{
    public float moveSpeed = 5f; // Hareket hýzý
    public float rangeX = 15f;   // X eksenindeki maksimum menzil
    public float rangeY = 15f;   // Y eksenindeki maksimum menzil
    public float targetReachThreshold = 0.1f; // Hedefe ne kadar yaklaþtýðýmýzda yeni hedef belirleyeceðimiz eþik

    private Vector3 initialPosition; // Objelerin baþlangýç konumu
    private Vector3 targetPosition;  // Objenin gideceði hedef konum

    void Start()
    {
        // Objelerin baþlangýç konumunu kaydet
        initialPosition = transform.position;
        // Ýlk hedefi belirle
        SetNewRandomTarget();
    }

    void Update()
    {
        // Objeyi hedefe doðru hareket ettir
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Hedefe yaklaþtýysak yeni bir hedef belirle
        if (Vector3.Distance(transform.position, targetPosition) < targetReachThreshold)
        {
            SetNewRandomTarget();
        }
    }

    void SetNewRandomTarget()
    {
        // Baþlangýç konumuna göre rastgele bir X ve Y ofseti oluþtur
        float randomOffsetX = Random.Range(-rangeX, rangeX);
        float randomOffsetY = Random.Range(-rangeY, rangeY);

        // Yeni hedef konumu hesapla (baþlangýç konumu + rastgele ofset)
        targetPosition = initialPosition + new Vector3(randomOffsetX, randomOffsetY, 0f);
    }

    // Objeyi belirli bir süre bekletmek isterseniz bu metodu kullanabilirsiniz.
    // Ancak bu senaryoda sürekli hareket istendiði için Update içinde daha uygun.
    // Yine de bilgi amaçlý býrakýlmýþtýr.
    /*
    IEnumerator MoveToTarget(Vector3 target)
    {
        while (Vector3.Distance(transform.position, target) > targetReachThreshold)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null; // Bir sonraki frame'e kadar bekle
        }
        SetNewRandomTarget(); // Hedefe ulaþtýðýnda yeni hedef belirle
    }
    */
}
