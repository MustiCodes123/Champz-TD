using UnityEngine;
using DG.Tweening;

/// <summary>
/// Floating text that animates upwards and fades out
/// Used for damage numbers and gold rewards
/// </summary>
public class FloatingText : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float moveDistance = 1f;
    [SerializeField] private float duration = 1.5f;
    [SerializeField] private Ease moveEase = Ease.OutQuad;

    private TMPro.TextMeshPro textMesh;
    private Color originalColor;

    void Awake()
    {
        textMesh = GetComponent<TMPro.TextMeshPro>();
        if (textMesh != null)
        {
            originalColor = textMesh.color;
        }
    }

    public void Initialize(string text, Color color)
    {
        if (textMesh != null)
        {
            textMesh.text = text;
            textMesh.color = color;
        }

        AnimateAndDestroy();
    }

    private void AnimateAndDestroy()
    {
        Vector3 targetPosition = transform.position + Vector3.up * moveDistance;

        // Move upward
        transform.DOMove(targetPosition, duration).SetEase(moveEase);

        // Fade out
        if (textMesh != null)
        {
            textMesh.DOFade(0f, duration).SetEase(Ease.InQuad);
        }

        // Destroy after animation
        Destroy(gameObject, duration);
    }

    public static void Create(Vector3 position, string text, Color color)
    {
        // Create a simple TextMeshPro object
        GameObject textObj = new GameObject("FloatingText");
        textObj.transform.position = position;

        TMPro.TextMeshPro tmp = textObj.AddComponent<TMPro.TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = 4;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = color;
        tmp.sortingOrder = 100;

        FloatingText floatingText = textObj.AddComponent<FloatingText>();
        floatingText.textMesh = tmp;
        floatingText.AnimateAndDestroy();
    }
}
