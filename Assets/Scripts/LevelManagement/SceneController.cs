using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public event Action BeforeSceneUnload;
    public event Action AfterSceneLoad;

    public CanvasGroup faderCanvasGroup;
    public float fadeDuration = 1f;
    public int sceneIndex = 1;

    private bool isFading;

    private IEnumerator Start()
    {
        faderCanvasGroup.alpha = 1f;

        yield return StartCoroutine(LoadSceneAndSetActive(sceneIndex));

        StartCoroutine(Fade(0f));
    }

    public void FadeAndLoadScene(int sceneIndex)
    {
        if (!isFading)
        {
            StartCoroutine(FadeAndSwitchScenes(sceneIndex));
        }
    }

    public IEnumerator FadeAndSwitchScenes(int sceneIndex)
    {
        yield return StartCoroutine(Fade(1f));

        if (BeforeSceneUnload != null)
            BeforeSceneUnload();

        yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);

        yield return StartCoroutine(LoadSceneAndSetActive(sceneIndex));

        if (AfterSceneLoad != null)
            AfterSceneLoad();

        yield return StartCoroutine(Fade(0f));

    }


    private IEnumerator LoadSceneAndSetActive(int sceneIndex)
    {
        yield return SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);

        Scene newlyLoadedScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
        SceneManager.SetActiveScene(newlyLoadedScene);
    }

    private IEnumerator Fade(float finalAlpha)
    {
        isFading = true;

        faderCanvasGroup.blocksRaycasts = true;

        float fadeSpeed = Mathf.Abs(faderCanvasGroup.alpha - finalAlpha) / fadeDuration;
        while (!Mathf.Approximately(faderCanvasGroup.alpha, finalAlpha))
        {
            faderCanvasGroup.alpha = Mathf.MoveTowards(faderCanvasGroup.alpha, finalAlpha, fadeSpeed * Time.deltaTime);

            yield return null;
        }

        isFading = false;
        faderCanvasGroup.blocksRaycasts = false;
    }

}
