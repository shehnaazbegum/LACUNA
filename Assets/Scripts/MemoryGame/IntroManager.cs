using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroManager : MonoBehaviour
{
    [SerializeField] private float displayTime = 2.5f;

    private void Start()
    {
        StartCoroutine(LoadMainMenu());
    }

    private IEnumerator LoadMainMenu()
    {

        yield return new WaitForSecondsRealtime(displayTime);

        SceneManager.LoadScene("MainMenu");
    }
}