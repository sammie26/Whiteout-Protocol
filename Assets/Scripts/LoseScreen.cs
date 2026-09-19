using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

// https://www.youtube.com/watch?v=wG93etH9aBg&t=480s
// https://www.youtube.com/watch?v=gSfdCke3684


public class LoseScreen : MonoBehaviour
{
    [SerializeField] private Button restartButton;
    [SerializeField] private Button exitButton;
    private float delayTime = 10f;
    private bool canExecuteAction = false;
    private bool restartButtonClicked = false;
    private bool exitButtonClicked = false;

    void Start()
    {
       
        restartButtonClicked = false;
        exitButtonClicked = false;
        Debug.Log($"Initial flags in Start: restartButtonClicked: {restartButtonClicked}, exitButtonClicked: {exitButtonClicked}");

        

        
        if (restartButton == null)
        {
            Debug.LogError("Restart button is not assigned in the Inspector!");
        }
        else
        {
            Debug.Log("Restart button assigned: " + restartButton.name);
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
            //https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Events.UnityEventBase.RemoveAllListeners.html
            
            
            Debug.Log("Exit button assigned: " + exitButton.name);
            exitButton.onClick.RemoveAllListeners(); 
            exitButton.onClick.AddListener(OnExitButtonClicked);
            exitButton.interactable = true; 
        }


        StartCoroutine(DelayAction());   // to allow the game to continue running while waiting for the delay 
    }

    void OnDestroy()
    {
        // clears flags when the script is destroyed
        restartButtonClicked = false;
        exitButtonClicked = false;
        if (restartButton != null) restartButton.onClick.RemoveAllListeners();
        if (exitButton != null) exitButton.onClick.RemoveAllListeners();
        Debug.Log("ButtonUI destroyed, flags and listeners cleared.");
    }


    
    //https://gamedevbeginner.com/coroutines-in-unity-when-and-how-to-use-them/#how_to_write_a_coroutine
    private IEnumerator DelayAction()
    {
        yield return new WaitForSeconds(delayTime);
        canExecuteAction = true;
        Debug.Log($"Button actions can now be executed after 10 seconds. restartButtonClicked: {restartButtonClicked}, exitButtonClicked: {exitButtonClicked}");

        if (restartButtonClicked)
        {
            Debug.Log("Executing delayed restart action...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else if (exitButtonClicked) // check on Application.Quit() alternative 
        {
            Debug.Log("Executing delayed exit action (button pressed)...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            Debug.Log("Stopping play mode in Unity Editor.");
#else
            Application.Quit(); // Quit application in build
            Debug.Log("Application.Quit() called.");
#endif
        }
        else
        {
            Debug.Log("No buttons pressed, executing default exit action...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; 
            Debug.Log("Stopping play mode in Unity Editor (default).");
#else
            Application.Quit(); // Quit application in build
            Debug.Log("Application.Quit() called (default).");
#endif
        }
    }

    public void OnRestartButtonClicked()
    {
        Debug.Log("Restart button pressed.");
        Debug.Log($"Flags before check: restartButtonClicked: {restartButtonClicked}, exitButtonClicked: {exitButtonClicked}");
        if (canExecuteAction)
        {
            Debug.Log("Restarting game...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else if (!exitButtonClicked && !restartButtonClicked)
        {
            restartButtonClicked = true;
            if (restartButton != null) restartButton.interactable = false; // disables restart button until the delay is over
            if (exitButton != null) exitButton.interactable = false; // disables exit button until the delay is over
            Debug.Log("Restart action queued for execution after 10-second delay.");
        }
        else
        {
            Debug.Log("Cannot queue restart: Another action is already queued.");
        }
    }

    public void OnExitButtonClicked()
    {
        Debug.Log("Exit button pressed.");
        Debug.Log($"Flags before check: restartButtonClicked: {restartButtonClicked}, exitButtonClicked: {exitButtonClicked}");
        if (canExecuteAction)
        {
            Debug.Log("Exiting game...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; 
            Debug.Log("Stopping play mode in Unity Editor.");
#else
            Application.Quit(); // Quit application in build
            Debug.Log("Application.Quit() called.");
#endif
        }
        else if (!restartButtonClicked && !exitButtonClicked)
        {
            exitButtonClicked = true;
            if (exitButton != null) exitButton.interactable = false; 
            if (restartButton != null) restartButton.interactable = false; 
            Debug.Log("Exit action queued for execution after 10-second delay.");
        }
        else
        {
            Debug.Log("Cannot queue exit: Another action is already queued.");
        }
    }
}