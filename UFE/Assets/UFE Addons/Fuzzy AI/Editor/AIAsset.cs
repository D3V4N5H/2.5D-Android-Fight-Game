using UnityEngine;
using UnityEditor;
using System;

public class AIAsset
{
	[MenuItem("Assets/Create/U.F.E./A.I. File")]
    public static void CreateAsset ()
    {
        ScriptableObjectUtility.CreateAsset<AIInfo> ();
    }
}
