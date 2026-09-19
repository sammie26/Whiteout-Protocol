using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// follows same logic as the LoseScreen script

public class WinScreen : MonoBehaviour
{
    [SerializeField] private Button restartButton;
    [SerializeField] private Button exitButton;

    private void Start()
    {
       
        if (restartButton == null)
        {
            Debug.LogError("Restart button is not assigned in the Inspector!");
        }
        else
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(OnRestartButtonClicked);
            restartButton.interactable = true;
        }

        if (exitButton == null)
        {
            Debug.LogError("Exit button is not assigned in the Inspector!");
        }
        else
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(OnExitButtonClicked);
            exitButton.interactable = true;
        }

        
        if (restartButton != null && exitButton != null && restartButton == exitButton)
        {
            Debug.LogError("Restart and Exit buttons are assigned to the same GameObject!");
        }
    }

    private void OnDestroy()
    {
       
        if (restartButton != null) restartButton.onClick.RemoveAllListeners();
        if (exitButton != null) exitButton.onClick.RemoveAllListeners();
        Debug.Log("WinScreenButtonUI destroyed, listeners cleared.");
    }

    private void OnRestartButtonClicked()
    {
        Debug.Log("Restart button clicked, reloading scene.");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnExitButtonClicked()
    {
        Debug.Log("Exit button clicked.");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        Debug.Log("Stopping play mode in Unity Editor.");
#else
        Application.Quit();
        Debug.Log("Application.Quit() called.");
#endif
    }
}