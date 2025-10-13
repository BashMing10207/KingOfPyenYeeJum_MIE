using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [SerializeField]
    private string _sceneName;

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    public void ChangeScene(int num)
    {
        SceneManager.LoadScene(num);
    }
    public void ChangeScene()
    {
        SceneManager.LoadScene(_sceneName);
    }
}
