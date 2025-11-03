using System;
using UnityEngine;

public static class Extensions
{
    public static GameObject GetChildRecursive(String childName, Transform nodeToCheck)
    {
        var childCount = nodeToCheck.childCount;

        GameObject result = null;
        
        if(nodeToCheck.name ==  childName) return nodeToCheck.gameObject;
        if (childCount == 0) return null;

        for (int i = 0; i < childCount; i++)
        {
                
            result = GetChildRecursive(childName, nodeToCheck.GetChild(i));
            if (result != null) return result;
        }
        return result;
    }
}
