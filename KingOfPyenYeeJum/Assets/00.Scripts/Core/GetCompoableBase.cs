using UnityEngine;

public class GetCompoableBase : MonoBehaviour,IGetCompoable
{
    [HideInInspector]
    public GetCompoParent Mom;

    public void Init(GetCompoParent mom)
    {
        mom.AddCompoDic(this.GetType(), this);
        Mom = mom;
    }

    private void Start()
    {
        if(Mom == null)
        {
            Transform trm = GetComponent<Transform>();

            while(trm.parent != null && trm.gameObject.GetComponent<GetCompoParent>())
            {
                trm = trm.parent;
            }

            Init(trm.GetComponent<GetCompoParent>());
        }
    }
}
