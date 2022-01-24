using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    public void ReloadScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name);
    }

    public void StartStandardGame(int index)
    {
        DataManager.instance.selectedGameMode = GameMode.SingleScreen;
        LoadSceneByIndex(index);
    }

    public void ExitToMenu(int index)
    {
        SceneManager.LoadScene(index);
    }

    private void LoadSceneByIndex(int index)
    {
        SceneManager.LoadScene(index);
    }
}
