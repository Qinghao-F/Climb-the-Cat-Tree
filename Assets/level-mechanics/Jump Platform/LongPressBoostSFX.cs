using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(Collider))]
public class LongPressBoostSFX : MonoBehaviour
{
    [Header("Jump Pad Sound")]
    [SerializeField] private AudioClip jumpPadSFX;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;
    [SerializeField] private bool use2DSound = true;
    [SerializeField] private Vector2 pitchRandom = new Vector2(0.98f, 1.02f);
    [SerializeField] private float minImpactSpeed = 1.0f;
    [SerializeField] private float cooldown = 0.1f;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    private float _nextPlayableTime;

    private void OnCollisionEnter(Collision collision)
    {
        if (jumpPadSFX == null) return;
        if (!collision.collider.CompareTag("Player")) return;

        if (collision.relativeVelocity.magnitude < minImpactSpeed) return;

        if (Time.time < _nextPlayableTime) return;
        _nextPlayableTime = Time.time + cooldown;

        PlayOneShotAt(jumpPadSFX, collision.GetContact(0).point);
    }

    private void PlayOneShotAt(AudioClip clip, Vector3 pos)
    {
        var go = new GameObject($"JumpPadSFX_{clip.name}");
        go.transform.position = pos;
        var src = go.AddComponent<AudioSource>();

        src.outputAudioMixerGroup = sfxMixerGroup;
        src.spatialBlend = use2DSound ? 0f : 1f;
        src.rolloffMode = AudioRolloffMode.Linear;
        src.minDistance = 2f;
        src.maxDistance = 20f;
        src.playOnAwake = false;

        src.pitch = Mathf.Clamp(Random.Range(pitchRandom.x, pitchRandom.y), 0.5f, 2f);

        src.PlayOneShot(clip, volume);
        Destroy(go, clip.length / Mathf.Max(src.pitch, 0.01f) + 0.05f);
    }
}

