using TMPro;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
    public static HealthUI Instance { get; private set; }
    [SerializeField] private TextMeshProUGUI healthText;
    private int health = 3;

/*
    private void OnEnable()
    {
        if (PlayerInfo.Instance != null)
        {
            PlayerInfo.Instance.OnHealthChanged.AddListener(UpdateHealth);
            PlayerInfo.Instance.OnDeath.AddListener(OnPlayerDeath);
        }
    }

    private void OnDisable()
    {
        if (PlayerInfo.Instance != null)
        {
            PlayerInfo.Instance.OnHealthChanged.RemoveListener(UpdateHealth);
            PlayerInfo.Instance.OnDeath.RemoveListener(OnPlayerDeath);
        }
    }

    private void Start()
    {
        if (PlayerInfo.Instance != null)
            UpdateHealth(PlayerInfo.Instance.GetCurrentHealth());
    }

    private void UpdateHealth(int newHealth)
    {
        if (healthText != null)
            healthText.text = $"Health: {newHealth}";
        else
            Debug.LogWarning("HealthUI: No TextMeshProUGUI assigned");
    }

    private void OnPlayerDeath()
    {
        if (healthText != null)
            healthText.text = "You Died";
    }
    */


    // temp until I figure out event driven method
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        UpdateUI();
    }
    public void Damage(int damage = 1)
    {
        health -= damage;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (healthText != null) healthText.SetText($"Health: {health}");
        else Debug.LogWarning("HealthUI: healthText Undefine");
    }
}
