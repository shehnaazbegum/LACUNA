using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigation : MonoBehaviour
{
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadGameSelect()
    {
        SceneManager.LoadScene("GameSelect");
    }

    public void LoadEasy()
    {
        SceneManager.LoadScene("MemoryEasy");
    }

    public void LoadMedium()
    {
        SceneManager.LoadScene("MemoryMedium");
    }

    public void LoadHard()
    {
        SceneManager.LoadScene("MemoryHard");
    }

    public void LoadExtraHard()
    {
        SceneManager.LoadScene("MemoryGame");
    }

    public void LoadStats()
    {
        SceneManager.LoadScene("Stats");
    }

    public void LoadResult()
    {
        SceneManager.LoadScene("Result");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}