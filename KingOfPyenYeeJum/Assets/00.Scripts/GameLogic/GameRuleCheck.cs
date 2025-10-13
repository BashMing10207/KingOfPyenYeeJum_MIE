using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class GameRuleCheck : GetCompoableBase, IAfterInitable
{
    public int EmptySlots { get; private set; }
    public int FilledSlots { get; private set; }

    public int RemainingTime = 100;

    public UnityEvent OnFailed;
    public UnityEvent OnSuccess;

    public void AfterInit()
    {
        Mom.GetCompo<GameTurnCheck>().OnSwap.AddListener(CheckGameOver);
        Mom.GetCompo<GameTurnCheck>().OnSwap.AddListener(CheckGameWin);
        SetEmptySlots(0);
        SetFilledSlots(0);
    }

    public void Start()
    {
        StartCoroutine(Timer());
    }

    public void SetEmptySlots(int emptySlots)
    {
        this.EmptySlots = emptySlots;
    }
    public void SetFilledSlots(int filledSlots)
    {
        this.FilledSlots = filledSlots;
    }
    public void AddEmptySlots(int emptySlots)
    {
        this.EmptySlots += emptySlots;
    }
    public void AddFilledSlots(int filledSlots)
    {
        this.FilledSlots += filledSlots;
    }


    private void CheckGameOver()
    {
        if(this.EmptySlots <= 0)
        {
            OnFailed?.Invoke();
        }
    }

    private void CheckGameWin()
    {
        if(this.FilledSlots <= 0)
        {
            OnFailed?.Invoke();
        }
    }

    private IEnumerator Timer()
    {
        yield return null;
        while (true)
        {
            yield return new WaitForSeconds(1);
            RemainingTime--;
            if(RemainingTime <= 0)
            {
                OnFailed?.Invoke();
                break;
            }

        }
    }
}
