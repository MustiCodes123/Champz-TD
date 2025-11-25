using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;

/// <summary>
/// Tower Placement - Animated bottom panel for tower selection
/// </summary>
public class TowerPlacement : MonoBehaviour
{
    [Header("Tower Prefabs")]
    [SerializeField] private GameObject hunterTowerPrefab;
    [SerializeField] private GameObject jesterTowerPrefab;
    [SerializeField] private GameObject lumberjackTowerPrefab;
    [SerializeField] private GameObject priestTowerPrefab;
    [SerializeField] private GameObject alchemistTowerPrefab;
    [SerializeField] private GameObject warriorTowerPrefab;

    [Header("Tower Costs")]
    [SerializeField] private int hunterCost = 100;
    [SerializeField] private int jesterCost = 120;
    [SerializeField] private int lumberjackCost = 150;
    [SerializeField] private int priestCost = 180;
    [SerializeField] private int alchemistCost = 140;
    [SerializeField] private int warriorCost = 160;

    [Header("UI Panel References")]
    [SerializeField] private GameObject panelParent;
    [SerializeField] private Image backgroundOverlay;
    [SerializeField] private RectTransform bottomBar;
    [SerializeField] private TextMeshProUGUI currentGoldText;

    [Header("Tower Buttons")]
    [SerializeField] private Button hunterButton;
    [SerializeField] private Button jesterButton;
    [SerializeField] private Button lumberjackButton;
    [SerializeField] private Button priestButton;
    [SerializeField] private Button alchemistButton;
    [SerializeField] private Button warriorButton;
    [SerializeField] private Button cancelButton;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private float backgroundTargetAlpha = 0.5f;
    [SerializeField] private float bottomBarSlideDistance = 300f;

    private BuildSite currentBuildSite;
    private bool isPanelOpen = false;
    private Camera mainCamera;

    // Cost text references
    private TMPro.TextMeshProUGUI hunterCostText;
    private TMPro.TextMeshProUGUI jesterCostText;
    private TMPro.TextMeshProUGUI lumberjackCostText;
    private TMPro.TextMeshProUGUI priestCostText;
    private TMPro.TextMeshProUGUI alchemistCostText;
    private TMPro.TextMeshProUGUI warriorCostText;

    void Start()
    {
        mainCamera = Camera.main;

        // Hide panel initially
        if (panelParent != null)
            panelParent.SetActive(false);

        // Initialize cost text references
        InitializeCostTexts();

        // Set initial cost text values and colors
        UpdateCostTextColors();

        // Setup button listeners
        SetupButtons();
    }

    private void InitializeCostTexts()
    {
        // Find Text_Value child in each button and cache the reference
        hunterCostText = GetCostTextFromButton(hunterButton);
        jesterCostText = GetCostTextFromButton(jesterButton);
        lumberjackCostText = GetCostTextFromButton(lumberjackButton);
        priestCostText = GetCostTextFromButton(priestButton);
        alchemistCostText = GetCostTextFromButton(alchemistButton);
        warriorCostText = GetCostTextFromButton(warriorButton);
    }

    private TMPro.TextMeshProUGUI GetCostTextFromButton(Button button)
    {
        if (button == null) return null;

        // Find child named "Text_Value"
        Transform textChild = button.transform.Find("Text_Value");
        if (textChild != null)
        {
            return textChild.GetComponent<TMPro.TextMeshProUGUI>();
        }

        Debug.LogWarning($"TowerPlacement: Text_Value child not found on button {button.name}");
        return null;
    }

    void Update()
    {
        // Handle touch/click input
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
        {
            HandleScreenTouch(Input.mousePosition);
        }

        // Update gold display if panel is open
        if (isPanelOpen)
        {
            UpdateGoldDisplay();
        }
    }

    private void HandleScreenTouch(Vector2 screenPosition)
    {
        Vector2 worldPos = mainCamera.ScreenToWorldPoint(screenPosition);
        
        // Raycast to find build site
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        
        if (hit.collider != null && hit.collider.CompareTag("BuildSite"))
        {
            BuildSite buildSite = hit.collider.GetComponent<BuildSite>();
            
            if (buildSite != null && !buildSite.IsOccupied())
            {
                currentBuildSite = buildSite;
                AudioManager.Instance.PlaySound(SoundType.ButtonClick);
                OpenPanel();
            }
        }
        else if (isPanelOpen)
        {
            ClosePanel();
        }
    }

    private void OpenPanel()
    {
        if (isPanelOpen || panelParent == null) return;

        isPanelOpen = true;
        panelParent.SetActive(true);

        // Animate background overlay
        if (backgroundOverlay != null)
        {
            Color col = backgroundOverlay.color;
            col.a = 0f;
            backgroundOverlay.color = col;
            backgroundOverlay.DOFade(backgroundTargetAlpha, animationDuration);
        }

        // Animate bottom bar sliding up
        if (bottomBar != null)
        {
            Vector2 startPos = bottomBar.anchoredPosition;
            startPos.y = -bottomBarSlideDistance;
            bottomBar.anchoredPosition = startPos;

            Vector2 targetPos = startPos;
            targetPos.y = 0f;
            bottomBar.DOAnchorPos(targetPos, animationDuration).SetEase(Ease.OutBack);
        }

        UpdateButtonStates();
        UpdateGoldDisplay();
    }

    public void ClosePanel()
    {
        if (!isPanelOpen) return;

        isPanelOpen = false;

        // Animate background overlay fade out
        if (backgroundOverlay != null)
        {
            backgroundOverlay.DOFade(0f, animationDuration);
        }

        // Animate bottom bar sliding down
        if (bottomBar != null)
        {
            Vector2 targetPos = bottomBar.anchoredPosition;
            targetPos.y = -bottomBarSlideDistance;
            bottomBar.DOAnchorPos(targetPos, animationDuration).SetEase(Ease.InBack)
                .OnComplete(() => panelParent.SetActive(false));
        }

        currentBuildSite = null;
    }

    private void SetupButtons()
    {
        if (hunterButton != null)
            hunterButton.onClick.AddListener(() => PlaceTower(hunterTowerPrefab, hunterCost));
        
        if (jesterButton != null)
            jesterButton.onClick.AddListener(() => PlaceTower(jesterTowerPrefab, jesterCost));
        
        if (lumberjackButton != null)
            lumberjackButton.onClick.AddListener(() => PlaceTower(lumberjackTowerPrefab, lumberjackCost));
        
        if (priestButton != null)
            priestButton.onClick.AddListener(() => PlaceTower(priestTowerPrefab, priestCost));
        
        if (alchemistButton != null)
            alchemistButton.onClick.AddListener(() => PlaceTower(alchemistTowerPrefab, alchemistCost));
        
        if (warriorButton != null)
            warriorButton.onClick.AddListener(() => PlaceTower(warriorTowerPrefab, warriorCost));
        
        if (cancelButton != null)
            cancelButton.onClick.AddListener(() => {
                AudioManager.Instance.PlaySound(SoundType.ButtonClick);
                ClosePanel();
            });
    }

    private void PlaceTower(GameObject towerPrefab, int cost)
    {
        AudioManager.Instance.PlaySound(SoundType.ButtonClick);

        if (currentBuildSite == null || towerPrefab == null) return;

        // Check if player can afford
        if (GameManager.Instance != null && !GameManager.Instance.CanAfford(cost))
        {
            Debug.Log("Not enough gold!");
            // TODO: Show "Not enough gold" feedback
            return;
        }

        // Spawn tower
        GameObject tower = Instantiate(towerPrefab, currentBuildSite.transform.position, Quaternion.identity);
        
        // Deduct cost
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SpendGold(cost);
        }

        // Play tower place sound
        AudioManager.Instance.PlaySound(SoundType.TowerPlace);

        // Mark build site as occupied
        currentBuildSite.PlaceTower(tower);

        ClosePanel();
    }

    private void UpdateButtonStates()
    {
        if (GameManager.Instance == null) return;

        int currentGold = GameManager.Instance.GetCurrentGold();

        // Enable/disable buttons based on affordability
        SetButtonState(hunterButton, currentGold >= hunterCost);
        SetButtonState(jesterButton, currentGold >= jesterCost);
        SetButtonState(lumberjackButton, currentGold >= lumberjackCost);
        SetButtonState(priestButton, currentGold >= priestCost);
        SetButtonState(alchemistButton, currentGold >= alchemistCost);
        SetButtonState(warriorButton, currentGold >= warriorCost);
    }

    private void SetButtonState(Button button, bool canAfford)
    {
        if (button == null) return;

        button.interactable = canAfford;
        
        // Visual feedback - dim button if can't afford
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = canAfford ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
        }
    }

    private void UpdateGoldDisplay()
    {
        if (currentGoldText != null && GameManager.Instance != null)
        {
            currentGoldText.text = $"Gold: {GameManager.Instance.GetCurrentGold()}";
        }

        // Update cost text colors
        UpdateCostTextColors();
    }

    private void UpdateCostTextColors()
    {
        if (GameManager.Instance == null) return;

        int currentGold = GameManager.Instance.GetCurrentGold();

        // Update each cost text color based on affordability
        UpdateCostTextColor(hunterCostText, hunterCost, currentGold);
        UpdateCostTextColor(jesterCostText, jesterCost, currentGold);
        UpdateCostTextColor(lumberjackCostText, lumberjackCost, currentGold);
        UpdateCostTextColor(priestCostText, priestCost, currentGold);
        UpdateCostTextColor(alchemistCostText, alchemistCost, currentGold);
        UpdateCostTextColor(warriorCostText, warriorCost, currentGold);
    }

    private void UpdateCostTextColor(TMPro.TextMeshProUGUI costText, int cost, int currentGold)
    {
        if (costText == null) return;

        costText.text = cost.ToString();

        // Set color based on affordability
        if (currentGold >= cost)
        {
            costText.color = Color.white; // Or green if you prefer
        }
        else
        {
            costText.color = Color.red;
        }
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
