using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticMonoBehaviour : Manager
{
    public static StaticMonoBehaviour Instance { get { return GetManager<StaticMonoBehaviour>(); } }
}
