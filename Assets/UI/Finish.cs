using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Finish : MonoBehaviour
{
    [Header("Win UI")]
    [SerializeField] private WinPanelManager winPanelManager;
    [SerializeField] private float showDelay = 0.5f;

    private AudioSource finishSound;
    private bool triggered = false;
    private GameObject playerRef;

    private void Awake()
    {
        finishSound = GetComponent<AudioSource>();

        var col = GetComponent<BoxCollider>();
        if (col != null) col.isTrigger = true;

        if (winPanelManager != null)
            winPanelManager.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        if (other.CompareTag("Player") || other.gameObject.name == "Player")
        {
            triggered = true;
            playerRef = other.gameObject;

            if (finishSound != null) finishSound.Play();

            Invoke(nameof(finishLevel), showDelay);
        }
    }

    private void finishLevel()
    {
        // Show win panel with score and stars
        if (winPanelManager != null)
        {
            winPanelManager.ShowWinPanel();
        }
        else
        {
            Debug.LogError("Finish: WinPanelManager reference is missing!");
        }

        // pause
        Time.timeScale = 0f;

        if (playerRef != null)
        {
            var rb = playerRef.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // win animation
            // var anim = playerRef.GetComponent<Animator>();
            // if (anim != null) anim.SetTrigger("win");
        }
    }
}
