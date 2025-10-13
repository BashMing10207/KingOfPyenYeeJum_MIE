
using System;
using System.Collections.Generic;
using UnityEngine;

public class BashUtils
{
   
}

[Serializable]
public class BashTuple <T1,T2>
{
    public T1 One;
    public T2 Two;
    public BashTuple(T1 one, T2 two)
    {
        One = one;
        Two = two;
    }

    public void SetOne(T1 one)
    {
        One = one;
    }
    public void SetTwo(T2 two)
    {
        Two = two;
    }
}
[Serializable]
public class BashList<T>
{
    public List<T> List;

    public BashList(List<T> list)
    {
        List = list;
    }
    public BashList()
    {
        List = new List<T>();
    }
}