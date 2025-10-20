using TMPro;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
    public static HealthUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI healthText;

    void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnEnable()
    {
        // Subscribe to PlayerInfo events when UI is enabled
        if (PlayerInfo.Instance != null)
        {
            PlayerInfo.Instance.OnHealthChanged.AddListener(UpdateHealth);
            PlayerInfo.Instance.OnDeath.AddListener(OnPlayerDeath);
        }
    }

    void OnDisable()
    {
        // Unsubscribe when UI is disabled (prevents memory leaks)
        if (PlayerInfo.Instance != null)
        {
            PlayerInfo.Instance.OnHealthChanged.RemoveListener(UpdateHealth);
            PlayerInfo.Instance.OnDeath.RemoveListener(OnPlayerDeath);
        }
    }

    void Start()
    {
        // Initialize UI with current health
        if (PlayerInfo.Instance != null)
        {
            UpdateHealth(PlayerInfo.Instance.GetCurrentHealth());
        }
    }

    private void UpdateHealth(int newHealth)
    {
        if (healthText != null)
        {
            healthText.text = $"Health: {newHealth}";
        }
        else
        {
            Debug.LogWarning("HealthUI: No TextMeshProUGUI assigned");
        }
    }

    private void OnPlayerDeath()
    {
        if (healthText != null)
        {
            healthText.text = "You Died!";
        }
    }
}
