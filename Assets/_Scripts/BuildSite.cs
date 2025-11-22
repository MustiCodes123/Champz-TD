using UnityEngine;

/// <summary>
/// Build Site - Simple marker for tower placement
/// </summary>
public class BuildSite : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color availableColor = Color.green;
    [SerializeField] private Color occupiedColor = Color.red;

    private bool isOccupied = false;
    private GameObject placedTower;

    void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        UpdateVisuals();
        
        // Ensure proper tag
        if (!gameObject.CompareTag("BuildSite"))
            gameObject.tag = "BuildSite";
    }

    public bool IsOccupied() => isOccupied;

    public void PlaceTower(GameObject tower)
    {
        isOccupied = true;
        placedTower = tower;
        UpdateVisuals();
    }

    public GameObject GetPlacedTower() => placedTower;

    public void RemoveTower()
    {
        if (placedTower != null)
            Destroy(placedTower);
        
        isOccupied = false;
        placedTower = null;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isOccupied ? occupiedColor : availableColor;
        }
    }
}
