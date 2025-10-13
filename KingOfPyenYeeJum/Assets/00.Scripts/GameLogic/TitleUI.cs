using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleUI : MonoBehaviour
{
    [SerializeField]
    private int _currentLevel = 1;

    [SerializeField]
    private TextMeshProUGUI _currentLevelTxt;

    private void Start()
    {
        AddLevelCnt(0);
    }

    public void AddLevelCnt(int level)
    {
        _currentLevel += level;
        _currentLevel = Mathf.Clamp(_currentLevel, 1, 10);
        _currentLevelTxt.text = _currentLevel.ToString();
    }

    public void PlayLevel()
    {
        SceneManager.LoadScene(_currentLevel);
    }
}
