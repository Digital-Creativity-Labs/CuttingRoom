using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class EditorWindowUtils
{
    public static T GetWindowIfOpen<T>(bool utility = false, string title = null, bool focus = false) where T : EditorWindow
    {
        T instance = null;

        if (EditorWindow.HasOpenInstances<T>())
        {
            instance = EditorWindow.GetWindow<T>(utility, title, focus);
        }

        return instance;
    }
}
