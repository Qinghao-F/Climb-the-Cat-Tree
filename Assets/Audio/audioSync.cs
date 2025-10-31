using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class AudioSync : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public AudioSource audioSource;

    void Awake()
    {
        videoPlayer.Prepare();
        StartCoroutine(PlayWhenReady());
    }

    IEnumerator PlayWhenReady()
    {
        // Wait until the video is prepared
        while (!videoPlayer.isPrepared)
            yield return null;

        // Play both on the same frame
        videoPlayer.Play();
        audioSource.Play();
    }
}
