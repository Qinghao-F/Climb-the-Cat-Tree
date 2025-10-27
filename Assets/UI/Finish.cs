using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Finish : MonoBehaviour
{
    [Header("Win UI")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private float showDelay = 0.5f;

    private AudioSource finishSound;
    private bool triggered = false;
    private GameObject playerRef;

    private void Awake()
    {
        finishSound = GetComponent<AudioSource>();

        var col = GetComponent<BoxCollider>();
        if (col != null) col.isTrigger = true;

        if (winPanel != null) winPanel.SetActive(false);
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
        // show win panel
        if (winPanel != null) winPanel.SetActive(true);

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

