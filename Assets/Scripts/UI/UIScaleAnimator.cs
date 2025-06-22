using UnityEngine;
using System.Collections; // IEnumerator için gerekli
using UnityEngine.UI;     // UI elemanlarý için gerekli

public class UIScaleAnimator : MonoBehaviour
{
    public float scaleSpeed = 1f;    // Büyüme/Küçülme hýzý
    public float maxScaleFactor = 1.2f; // Orijinal boyutun kaç katýna kadar büyüyecek (örn: 1.2 = %120)
    public float minScaleFactor = 1.0f; // Orijinal boyutun kaç katýna kadar küçülecek (örn: 1.0 = %100, yani orijinal boyut)

    private Vector3 originalScale; // UI objesinin orijinal boyutu
    private bool scalingUp = true; // Büyüyor muyuz, küçülüyor muyuz?

    void Start()
    {
        // UI objesinin baþlangýç boyutunu kaydet
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (scalingUp)
        {
            // Hedeflenen boyuta doðru büyüt
            transform.localScale = Vector3.MoveTowards(transform.localScale, originalScale * maxScaleFactor, scaleSpeed * Time.deltaTime);

            // Hedeflenen boyuta ulaþtýysak yönü deðiþtir
            if (transform.localScale.x >= originalScale.x * maxScaleFactor - 0.01f) // Küçük bir eþik ekledik
            {
                scalingUp = false;
            }
        }
        else
        {
            // Orijinal boyuta doðru küçült
            transform.localScale = Vector3.MoveTowards(transform.localScale, originalScale * minScaleFactor, scaleSpeed * Time.deltaTime);

            // Orijinal boyuta ulaþtýysak yönü deðiþtir
            if (transform.localScale.x <= originalScale.x * minScaleFactor + 0.01f) // Küçük bir eþik ekledik
            {
                scalingUp = true;
            }
        }
    }
}