using TMPro;
using UnityEngine;

public class CheeseUI : MonoBehaviour
{
    public static CheeseUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI cheeseText;
    private int cheese = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        UpdateUI();
    }

    public void Add(int amount = 1)
    {
        cheese += amount;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (cheeseText != null) cheeseText.SetText($"Cheese: {cheese}");
        else Debug.LogWarning("CheeseUI: cheeseText Undefine");
    }
}
