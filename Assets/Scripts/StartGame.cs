using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    public void OnStartGame()
    {
        SceneManager.LoadScene("InstructionScene");
    }

    public void OnExit()
    {
        // save any game data here
        #if UNITY_EDITOR
              UnityEditor.EditorApplication.isPlaying = false;
        #else
              Application.Quit();
        #endif
    }
}
