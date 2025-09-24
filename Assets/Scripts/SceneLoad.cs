using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneButton : MonoBehaviour
{
    public void LoadScene(string sceneGame)
    {
        SceneManager.LoadScene(sceneGame);
        ScoreManager.Instance.score = 0;
    }
}
