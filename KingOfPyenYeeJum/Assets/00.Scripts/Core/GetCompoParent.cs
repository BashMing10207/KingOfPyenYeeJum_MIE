using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GetCompoParent : MonoBehaviour
{
    protected Dictionary<Type,IGetCompoable> _components;

    protected void Awake()
    {
        IGetCompoable[] babies = GetComponentsInChildren<IGetCompoable>(true);

        {        
            int i = 0;
            while (babies.Length > 0)
            {
                babies[i].Init(this);
                i++;
            }
        }

        IAfterInitable[] babies2 = GetComponentsInChildren<IAfterInitable>(true);
        {
            int i = 0;
            while (babies2.Length > 0)
            {
                babies2[0].Init();
                i++;
            }
        }


    }

    public void AddCompoDic(Type type, IGetCompoable compo)
    {
        if(!_components.ContainsKey(type))
        _components.Add(type, compo);
    }

    public void AddRealCOmpo<T>() where T : Component,IGetCompoable
    {
        T instance = gameObject.AddComponent<T>();
        _components.Add(instance.GetType(), instance);
    }


    public T GetCompo<T>(bool isIncludeChild = false) where T : IGetCompoable
    {
        if (_components.TryGetValue(typeof(T), out var component))
        {
            return (T)component;
        }

        if (isIncludeChild == false) return default; 

        return default;
    }
}
