using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GetCompoParent : MonoBehaviour
{
    protected Dictionary<Type,IGetCompoable> _components = new Dictionary<Type, IGetCompoable>();

    protected virtual void Awake()
    {
        IGetCompoable[] babies = GetComponentsInChildren<IGetCompoable>(true);

        {        
            for(int i = 0; i < babies.Length; i++)
            {
                babies[i].Init(this);
            }
            
        }

        IAfterInitable[] babies2 = GetComponentsInChildren<IAfterInitable>(true);
        {
            for (int i = 0; i < babies2.Length; i++)
            {
                babies2[i].AfterInit();
            }
        }

    }

    public virtual void AddCompoDic(Type type, IGetCompoable compo)
    {
        if(!_components.ContainsKey(type))
        _components.Add(type, compo);
    }

    public virtual void RemoveCompoDic(Type type)
    {
        if (_components.ContainsKey(type))
            _components.Remove(type);
    }
    public virtual void AddRealCOmpo<T>() where T : Component,IGetCompoable
    {
        T instance = gameObject.AddComponent<T>();
        _components.Add(instance.GetType(), instance);
    }


    public virtual T GetCompo<T>(bool isIncludeChild = false) where T : Component,IGetCompoable
    {
        if (_components.TryGetValue(typeof(T), out var component))
        {
            return (T)component;
        }

        if (isIncludeChild == false) return default; 
        //-----Under code is Ailian's Langage ---I think that Linq is not good idea shit!

        Type findType = _components.Keys.FirstOrDefault(type => type.IsSubclassOf(typeof(T)));
        
        if (findType != null)
            return (T)_components[findType];

        return default;
    }
}
