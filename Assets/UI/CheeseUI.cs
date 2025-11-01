using TMPro;
using UnityEngine;

public class CheeseUI : MonoBehaviour
{
    public static CheeseUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI cheeseText;

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
        // Subscribe to PlayerInfo score events when UI is enabled
        if (PlayerInfo.Instance != null)
        {
            PlayerInfo.Instance.OnScoreChanged.AddListener(UpdateCheese);
        }
    }

    void OnDisable()
    {
        // Unsubscribe when UI is disabled (prevents memory leaks)
        if (PlayerInfo.Instance != null)
        {
            PlayerInfo.Instance.OnScoreChanged.RemoveListener(UpdateCheese);
        }
    }

    void Start()
    {
        // Initialize UI with current score
        if (PlayerInfo.Instance != null)
        {
            UpdateCheese(PlayerInfo.Instance.GetScore());
        }
    }

    private void UpdateCheese(int newScore)
    {
        if (cheeseText != null)
        {
            cheeseText.text = $"Cheese: {newScore}";
        }
        else
        {
            Debug.LogWarning("CheeseUI: No TextMeshProUGUI assigned");
        }
    }
}