using TMPro;
using UnityEngine;

public class GameUI : GetCompoableBase, IAfterInitable
{
    [SerializeField]
    private TextMeshProUGUI _timeText;

    [SerializeField]
    private GameObject _failWidget;
    [SerializeField]
    private GameObject _successWidget;

    public void AfterInit()
    {
        Mom.GetCompo<GameRuleCheck>().TimeChanged.AddListener(TimeUIChanged);


    }

    private void TimeUIChanged(int currentTime)
    {
        _timeText.text = currentTime.ToString();
    }

    public void GameResult(bool bIsWin)
    {
        _successWidget.SetActive(bIsWin);
        _failWidget.SetActive(!bIsWin);
    }
}
