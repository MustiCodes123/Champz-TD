using UnityEngine;

/// <summary>
/// Build Site - Simple marker for tower placement
/// </summary>
public class BuildSite : MonoBehaviour
{

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
