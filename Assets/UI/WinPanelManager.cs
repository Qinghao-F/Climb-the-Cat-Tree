using UnityEngine;
using UnityEngine.UI; 
using TMPro; 

public class WinPanelManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text scoreText; 
    
    [Tooltip("Array of 3 Image components for the stars.")]
    [SerializeField] private Image[] starImages = new Image[3];

    [Header("Star Rating Thresholds")]
    public int oneStarThreshold = 10;
    public int twoStarThreshold = 30;
    public int threeStarThreshold = 70;

    [Header("Star Sprites")]
    public Sprite emptyStarSprite;
    public Sprite filledStarSprite;

    /// <summary>
    /// Call this function when the player completes the level.
    /// </summary>
    public void ShowWinPanel()
    {
        // Activate the panel
        gameObject.SetActive(true);

        // Get the PlayerInfo instance (using your existing singleton pattern)
        PlayerInfo playerInfo = PlayerInfo.Instance;
        
        if (playerInfo != null)
        {
            int finalScore = playerInfo.GetScore(); 
            scoreText.text = $"Score: {finalScore}";
            UpdateStarRating(finalScore);
        }
        else
        {
            Debug.LogError("WinPanelManager: PlayerInfo Instance not found.");
        }
    }

    private void UpdateStarRating(int score)
    {
        Debug.Log($"Updating stars: score={score}, filled sprite={filledStarSprite}, empty sprite={emptyStarSprite}");
        int starsEarned = 0;

        // Determine star count based on score and thresholds
        if (score >= threeStarThreshold)
        {
            starsEarned = 3;
        }
        else if (score >= twoStarThreshold)
        {
            starsEarned = 2;
        }
        else if (score >= oneStarThreshold)
        {
            starsEarned = 1;
        }
        
        // Update the visual star images
        for (int i = 0; i < starImages.Length; i++)
        {
            Debug.Log($"Setting star {i} to {(i < starsEarned ? "filled" : "empty")}");

            if (starImages[i] == null) continue;

            if (i < starsEarned)
            {
                starImages[i].sprite = filledStarSprite;
            }
            else
            {
                starImages[i].sprite = emptyStarSprite;
            }
            starImages[i].gameObject.SetActive(true);
        }

        Debug.Log($"Player earned {starsEarned} stars with a score of {score}.");
    }
    
    public void HideWinPanel()
    {
        gameObject.SetActive(false);
    }
}