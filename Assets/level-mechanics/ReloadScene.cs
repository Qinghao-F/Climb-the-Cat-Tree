using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Put this on the GameObject that has the Animator (playerAnim).
/// The Animation Event on Player_die should call Restart().
/// </summary>
public class AnimEvent_RestartScene : MonoBehaviour
{
    // Called by the Animation Event at the end of Player_die
    public void Restart()   // must be public for Animation Event
    {
        // In case you paused/slow-mo the game somewhere
        if (Time.timeScale != 1f) Time.timeScale = 1f;

        // Reload current scene
        Scene active = SceneManager.GetActiveScene();
        SceneManager.LoadScene(active.buildIndex);
        // or: SceneManager.LoadScene(active.name);
    }
}
