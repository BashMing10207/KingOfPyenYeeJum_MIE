using UnityEngine;

public interface IGetCompoable
{
    public void Init(GetCompoParent mom)
    {
        mom.AddCompoDic(this.GetType(),this);
    }
}
