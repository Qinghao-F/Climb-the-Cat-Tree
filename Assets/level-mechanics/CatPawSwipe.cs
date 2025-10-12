using UnityEngine;
using System.Collections;

public class CatPawSwipe : MonoBehaviour
{
    [Header("Swipe Settings")]
    public float swipeInSpeed = 50f;
    public float swipeOutSpeed = 10f;
    public float swipeDistance = 15f;
    public float swipeInterval = 3f; // Time between swipes
    public float upwardAngle = 30f; // Diagonal upward angle in degrees

    [Header("Warning Animation")]
    public float warningTime = 1f; // Warning before swipe
    public float flashSpeed = 10f; // Flash speed for warning

    [Header("Damage Settings")]
    public int damage = 10;

    [Header("Swipe Direction")]
    public SwipeDirection direction = SwipeDirection.FromLeft;

    private Vector3 startPosition;
    private Vector3 offScreenPosition;
    private Vector3 warningPosition;
    private SpriteRenderer spriteRenderer;
    private Collider pawCollider;

    public enum SwipeDirection
    {
        FromLeft,
        FromRight
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        pawCollider = GetComponent<Collider>();
        pawCollider.enabled = false; // Disable collider initially

        CalculatePositions();
        transform.position = offScreenPosition;

        StartCoroutine(SwipeCycle());
    }

    void CalculatePositions()
    {
        startPosition = transform.position;

        float angleRad = upwardAngle * Mathf.Deg2Rad;
        float horizontalDist = swipeDistance * Mathf.Cos(angleRad);
        float verticalDist = swipeDistance * Mathf.Sin(angleRad);

        switch (direction)
        {
            case SwipeDirection.FromLeft:
                offScreenPosition = startPosition - new Vector3(horizontalDist, verticalDist, 0);
                warningPosition = offScreenPosition + new Vector3(horizontalDist * 0.2f, verticalDist * 0.2f, 0);
                break;
            case SwipeDirection.FromRight:
                offScreenPosition = startPosition + new Vector3(horizontalDist, -verticalDist, 0);
                warningPosition = offScreenPosition - new Vector3(horizontalDist * 0.2f, -verticalDist * 0.2f, 0);
                break;
        }
    }

    IEnumerator SwipeCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(swipeInterval);

            // Warning animation
            yield return StartCoroutine(PlayWarningAnimation());

            // Swipe in
            yield return StartCoroutine(SwipeIn());

            // Pause briefly
            yield return new WaitForSeconds(0.2f);

            // Swipe out
            yield return StartCoroutine(SwipeOut());
        }
    }

    IEnumerator PlayWarningAnimation()
    {
        float elapsed = 0f;
        Vector3 startPos = offScreenPosition;
        Vector3 endPos = warningPosition;

        while (elapsed < warningTime)
        {
            float t = elapsed / warningTime;

            // Slowly move toward warning position
            transform.position = Vector3.Lerp(startPos, endPos, t);

            // Flash the sprite
            float alpha = Mathf.PingPong(Time.time * flashSpeed, 1f);
            Color c = spriteRenderer.color;
            c.a = alpha;
            spriteRenderer.color = c;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset after warning
        transform.position = offScreenPosition;
        Color resetColor = spriteRenderer.color;
        resetColor.a = 1f;
        spriteRenderer.color = resetColor;
    }

    IEnumerator SwipeIn()
    {
        pawCollider.enabled = true; // Enable damage

        while (Vector3.Distance(transform.position, startPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, startPosition, swipeInSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = startPosition;
    }

    IEnumerator SwipeOut()
    {
        while (Vector3.Distance(transform.position, offScreenPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, offScreenPosition, swipeOutSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = offScreenPosition;
        pawCollider.enabled = false; // Disable collider after swipe
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInfo playerInfo = other.GetComponent<PlayerInfo>();
            if (playerInfo != null)
            {
                playerInfo.TakeDamage(damage);
                Debug.Log($"Cat paw hit player for {damage} damage");
                
                // temp solution until I figure out event driven method?
                HealthUI.Instance?.Damage(damage);
            }
            else
            {
                Debug.LogError("Player has no PlayerInfo component");
            }
        }
    }
}