using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class MenuVideoController : MonoBehaviour
{
    [Header("Video Players")]
    public VideoPlayer videoPlayer;
    
    [Header("Video Files (in StreamingAssets folder)")]
    public string startAnimationFile = "start-animation.mp4";
    public string loopingMenuFile = "menu-loop.mp4";
    public GameObject startButton;  // Show after start animation
    public GameObject helpButton;  // Show after start animation
    
    private bool hasPlayedStartAnimation = false;
    
    void Start()
    {
        // Hide start button initially
        if (startButton != null)
            startButton.SetActive(false);
            helpButton.SetActive(false);
        
        // Set up video player
        videoPlayer.source = VideoSource.Url;
        videoPlayer.isLooping = false;
        
        // Hide video quad until ready (if using Material Override)
        if (videoPlayer.targetMaterialRenderer != null)
        {
            videoPlayer.targetMaterialRenderer.enabled = false;
        }
        
        // Subscribe to video end event
        videoPlayer.loopPointReached += OnStartAnimationFinished;
        
        // Show video when prepared
        videoPlayer.prepareCompleted += (source) => 
        {
            if (videoPlayer.targetMaterialRenderer != null)
                videoPlayer.targetMaterialRenderer.enabled = true;
        };
        
        // Play start animation
        PlayStartAnimation();
    }
    
    void PlayStartAnimation()
    {
        videoPlayer.url = Application.streamingAssetsPath + "/" + startAnimationFile;
        videoPlayer.isLooping = false;
        videoPlayer.prepareCompleted += (source) => source.Play();
        videoPlayer.Prepare();
    }
    
    void OnStartAnimationFinished(VideoPlayer vp)
    {
        if (!hasPlayedStartAnimation)
        {
            hasPlayedStartAnimation = true;
            
            // Unsubscribe from this event
            videoPlayer.loopPointReached -= OnStartAnimationFinished;
            
            PlayLoopingMenu();
        }
    }
    
    void PlayLoopingMenu()
    {
        videoPlayer.url = Application.streamingAssetsPath + "/" + loopingMenuFile;
        videoPlayer.isLooping = true;
        videoPlayer.prepareCompleted += (source) => 
        {
            source.Play();
            // Show start button when looping menu begins
            if (startButton != null || helpButton != null)
                startButton.SetActive(true);
                helpButton.SetActive(true);
        };
        videoPlayer.Prepare();
    }
    public void SkipToMenu()
    {
        if (!hasPlayedStartAnimation)
        {
            videoPlayer.loopPointReached -= OnStartAnimationFinished;
            hasPlayedStartAnimation = true;
            PlayLoopingMenu();
        }
    }
}