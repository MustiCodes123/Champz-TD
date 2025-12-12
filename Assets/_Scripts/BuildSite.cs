using UnityEngine;

/// <summary>
/// Build Site - Simple marker for tower placement
/// </summary>
public class BuildSite : MonoBehaviour
{
    [Header("Tower Placement")]
    private float towerYOffset = 0.2f;

    private bool isOccupied = false;
    private GameObject placedTower;

    void Start()
    { 
        // Ensure proper tag
        if (!gameObject.CompareTag("BuildSite"))
            gameObject.tag = "BuildSite";
    }

    public bool IsOccupied() => isOccupied;

    public void PlaceTower(GameObject tower)
    {
        isOccupied = true;
        placedTower = tower;
        
        // Apply Y offset to tower position
        Vector3 offsetPosition = transform.position;
        offsetPosition.y += towerYOffset;
        tower.transform.position = offsetPosition;
    }

    public GameObject GetPlacedTower() => placedTower;

    public void RemoveTower()
    {
        if (placedTower != null)
            Destroy(placedTower);
        
        isOccupied = false;
        placedTower = null;
    }

}
