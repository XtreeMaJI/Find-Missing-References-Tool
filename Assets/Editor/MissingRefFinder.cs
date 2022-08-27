using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

public class MissingRefFinder : EditorWindow
{
    private List<GameObject> objectsWithMissingRef = new List<GameObject>();
    private bool shouldCheckScene = false; // Проверять ли ссылки в объектах на сцене
    private bool shouldUpdateList = false;

    private GameObject obj;

    [MenuItem("Tools/Missing references finder")]
    public static void ShowWindow()
    {
        GetWindow<MissingRefFinder>();
    }

    private void OnGUI()
    {
        shouldCheckScene = EditorGUILayout.Toggle("Should check references in objects from scene?", shouldCheckScene);

        if (GUILayout.Button("Find Misssing References"))
        {
            shouldUpdateList = true;
        }

        if (shouldUpdateList)
        {
            UpdateObjectList();
        }

    }

    private void UpdateObjectList()
    {
        objectsWithMissingRef.Clear();

        string searchFolder = "Assets";
        string[] paths = AssetDatabase.FindAssets("", new[] { searchFolder });
        foreach (var path in paths)
        {
            GameObject obj = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(path), typeof(GameObject)) as GameObject;
            if (obj == null)
                continue;

            //Debug.Log(AssetDatabase.GUIDToAssetPath(path));
            objectsWithMissingRef.Add(obj);
            obj = EditorGUILayout.ObjectField(obj, typeof(GameObject), false) as GameObject;
            CheckObjectComponents(obj);
        }
    }

    private void CheckObjectComponents(GameObject obj)
    {
        //Component[] components = obj.GetComponents<Component>();
        //BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
        /* foreach (Component comp in components)
         {
             Debug.Log(comp.ToString());
             foreach (PropertyInfo p in comp.GetType().GetProperties(flags))
             {
                 Debug.Log(p.ToString() + p.GetType().IsClass);
             }
         }*/

        /* foreach (Component comp in components)
         {
            // Debug.Log(comp.ToString());
             foreach (FieldInfo p in comp.GetType().GetFields(flags))
             {
                 if(p.Name == "cubeObj")
                     Debug.Log(p.GetValue(p));
             }
         }*/

        Component[] components = obj.GetComponents<Component>();
        foreach (Component comp in components)
        {
            CheckComponentProperties(comp);
        }
    }

    private void CheckComponentProperties(Component comp)
    {
        SerializedObject sGameObj = new SerializedObject(comp);
        SerializedProperty sProp = sGameObj.GetIterator();
        sProp.NextVisible(true);
        while (sProp.NextVisible(true))
        {
            if (sProp.propertyType != SerializedPropertyType.ObjectReference)
                continue;

            if (sProp.objectReferenceInstanceIDValue != 0 && !sProp.objectReferenceValue)
            {
                Debug.Log(sProp.displayName);
            }
            //Debug.Log(sProp.objectReferenceInstanceIDValue + " " + sProp.objectReferenceValue);
        }
    }

}
