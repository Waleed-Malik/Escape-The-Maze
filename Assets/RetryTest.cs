using UnityEngine;
using UnityEngine.SceneManagement;

public class RetryTest : MonoBehaviour
{
    public void Retry()
    {
        Time.timeScale = 1f;
        Debug.Log("Button works!");
        SceneManager.LoadScene(0);
    }
}
