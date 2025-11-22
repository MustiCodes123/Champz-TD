using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// Tower Upgrade Manager - Handles upgrading existing towers
/// </summary>
public class TowerUpgradePanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private Image backgroundOverlay;
    [SerializeField] private RectTransform upgradeBox;
    
    [Header("Tower Info")]
    [SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private TextMeshProUGUI towerLevelText;
    [SerializeField] private TextMeshProUGUI towerStatsText;
    
    [Header("Upgrade Button")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI upgradeCostText;
    [SerializeField] private Button sellButton;
    [SerializeField] private TextMeshProUGUI sellValueText;
    [SerializeField] private Button closeButton;

    [Header("Animation")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private float backgroundAlpha = 0.5f;

    private BuildSite currentBuildSite;
    private Tower currentTower;
    private TowerData currentTowerData;
    private bool isPanelOpen = false;

    void Start()
    {
        if (upgradePanel != null)
            upgradePanel.SetActive(false);

        SetupButtons();
    }

    void Update()
    {
        // Handle click on towers
        if (Input.GetMouseButtonDown(0) && !isPanelOpen)
        {
            CheckTowerClick(Input.mousePosition);
        }
    }

    private void CheckTowerClick(Vector2 screenPosition)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        Vector2 worldPos = mainCamera.ScreenToWorldPoint(screenPosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("BuildSite"))
        {
            BuildSite buildSite = hit.collider.GetComponent<BuildSite>();
            
            if (buildSite != null && buildSite.IsOccupied())
            {
                GameObject towerObj = buildSite.GetPlacedTower();
                if (towerObj != null)
                {
                    OpenUpgradePanel(buildSite, towerObj);
                }
            }
        }
    }

    private void OpenUpgradePanel(BuildSite buildSite, GameObject towerObj)
    {
        currentBuildSite = buildSite;
        currentTower = towerObj.GetComponent<Tower>();
        currentTowerData = towerObj.GetComponent<TowerData>();

        if (currentTowerData == null)
        {
            Debug.LogError("Tower missing TowerData component!");
            return;
        }

        isPanelOpen = true;
        
        if (upgradePanel != null)
            upgradePanel.SetActive(true);

        // Animate background
        if (backgroundOverlay != null)
        {
            Color col = backgroundOverlay.color;
            col.a = 0f;
            backgroundOverlay.color = col;
            backgroundOverlay.DOFade(backgroundAlpha, animationDuration);
        }

        // Animate upgrade box
        if (upgradeBox != null)
        {
            upgradeBox.localScale = Vector3.zero;
            upgradeBox.DOScale(Vector3.one, animationDuration).SetEase(Ease.OutBack);
        }

        UpdatePanelInfo();
    }

    public void CloseUpgradePanel()
    {
        if (!isPanelOpen) return;

        isPanelOpen = false;

        // Animate background
        if (backgroundOverlay != null)
        {
            backgroundOverlay.DOFade(0f, animationDuration);
        }

        // Animate upgrade box
        if (upgradeBox != null)
        {
            upgradeBox.DOScale(Vector3.zero, animationDuration).SetEase(Ease.InBack)
                .OnComplete(() => upgradePanel.SetActive(false));
        }

        currentBuildSite = null;
        currentTower = null;
        currentTowerData = null;
    }

    private void UpdatePanelInfo()
    {
        if (currentTowerData == null) return;

        // Tower name
        if (towerNameText != null)
            towerNameText.text = currentTowerData.towerType.ToString();

        // Tower level
        if (towerLevelText != null)
            towerLevelText.text = $"Level {currentTowerData.currentLevel}/{currentTowerData.maxLevel}";

        // Tower stats
        if (towerStatsText != null)
        {
            towerStatsText.text = $"Range: {currentTowerData.GetCurrentRange():F1}\n" +
                                   $"Damage: {currentTowerData.GetCurrentDamage():F0}\n" +
                                   $"Fire Rate: {currentTowerData.GetCurrentFireRate():F1}";
        }

        // Upgrade button
        if (upgradeButton != null)
        {
            bool canUpgrade = currentTowerData.CanUpgrade();
            upgradeButton.interactable = canUpgrade;

            if (canUpgrade)
            {
                int upgradeCost = currentTowerData.GetUpgradeCost();
                bool canAfford = GameManager.Instance != null && GameManager.Instance.CanAfford(upgradeCost);
                upgradeButton.interactable = canAfford;

                if (upgradeCostText != null)
                    upgradeCostText.text = $"Upgrade: {upgradeCost}G";
            }
            else
            {
                if (upgradeCostText != null)
                    upgradeCostText.text = "MAX LEVEL";
            }
        }

        // Sell button
        if (sellButton != null && sellValueText != null)
        {
            int sellValue = CalculateSellValue();
            sellValueText.text = $"Sell: {sellValue}G";
        }
    }

    private int CalculateSellValue()
    {
        if (currentTowerData == null) return 0;

        // Sell for 70% of total investment
        int totalCost = GetTowerBaseCost(currentTowerData.towerType);
        
        for (int i = 1; i < currentTowerData.currentLevel; i++)
        {
            if (i - 1 < currentTowerData.upgradeCosts.Length)
                totalCost += currentTowerData.upgradeCosts[i - 1];
        }

        return Mathf.RoundToInt(totalCost * 0.7f);
    }

    private int GetTowerBaseCost(TowerType type)
    {
        // Get this from TowerPlacement script - for now hardcoded
        switch (type)
        {
            case TowerType.Hunter: return 100;
            case TowerType.Jester: return 120;
            case TowerType.Lumberjack: return 150;
            case TowerType.Priest: return 180;
            case TowerType.Alchemist: return 140;
            case TowerType.Warrior: return 160;
            default: return 100;
        }
    }

    private void SetupButtons()
    {
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(UpgradeTower);

        if (sellButton != null)
            sellButton.onClick.AddListener(SellTower);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseUpgradePanel);
    }

    private void UpgradeTower()
    {
        if (currentTowerData == null || !currentTowerData.CanUpgrade()) return;

        int upgradeCost = currentTowerData.GetUpgradeCost();

        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.CanAfford(upgradeCost))
            {
                GameManager.Instance.SpendGold(upgradeCost);
                currentTowerData.Upgrade();

                // Visual feedback
                if (currentTower != null)
                {
                    currentTower.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f);
                }

                UpdatePanelInfo();
            }
        }
    }

    private void SellTower()
    {
        if (currentBuildSite == null) return;

        int sellValue = CalculateSellValue();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddGold(sellValue);
        }

        currentBuildSite.RemoveTower();
        CloseUpgradePanel();
    }
}
