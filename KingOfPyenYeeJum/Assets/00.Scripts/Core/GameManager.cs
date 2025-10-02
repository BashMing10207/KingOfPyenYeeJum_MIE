using UnityEngine;

public class GameManager : GetCompoParent
{
    public static GameManager Instance;


    protected override void Awake()
    {
        Instance = this;
        base.Awake();
    }

    public override T GetCompo<T>(bool isIncludeChild = false)
    {
        if (base.GetCompo<T>(isIncludeChild) == null)
        {
            AddRealCOmpo<T>();
        }
        return base.GetCompo<T>(isIncludeChild);

        //Create COmpo when No Compo HEHEHA
    }
}
