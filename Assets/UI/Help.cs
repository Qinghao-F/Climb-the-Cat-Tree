using UnityEngine;
using UnityEngine.SceneManagement;

public class Help : MonoBehaviour
{
    [SerializeField] private GameObject helpPanel;

    private void Awake()
    {
        if (helpPanel != null) helpPanel.SetActive(false);
    }
    public void ShowHelp()
    {
        if (helpPanel != null) helpPanel.SetActive(true);
    }

    public void HideHelp()
    {
        if (helpPanel != null) helpPanel.SetActive(false);
    }
}
