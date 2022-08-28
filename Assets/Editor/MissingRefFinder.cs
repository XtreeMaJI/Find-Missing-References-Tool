using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

public class MissingRefFinder : EditorWindow
{
    private List<ObjectWithMissingRef> objectsWithMissingRef = new List<ObjectWithMissingRef>();
    private bool shouldCheckScene = false; // Проверять ли ссылки в объектах на сцене
    private bool shouldUpdateList = false;

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
            DrawObjectList();
        }

    }

    private void UpdateObjectList()
    {
        objectsWithMissingRef.Clear();

        string searchFolder = "Assets";
        string filter = "";
        string[] paths = AssetDatabase.FindAssets(filter, new[] { searchFolder });
        foreach (var path in paths)
        {
            GameObject obj = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(path), typeof(GameObject)) as GameObject;
            if (obj == null)
                continue;

            var missingRefObj = new ObjectWithMissingRef(obj);
            missingRefObj.FillObjectData();

            if(missingRefObj.hasMissingRefs())
            {
                objectsWithMissingRef.Add(missingRefObj);
            }

        }
    }

    private void DrawObjectList()
    {
        GUILayout.BeginVertical();
        foreach (var obj in objectsWithMissingRef)
        {
            DrawListElem(obj);
        }
        GUILayout.EndVertical();
    }

    private void DrawListElem(ObjectWithMissingRef obj)
    {
        GUILayout.BeginVertical();

        EditorGUILayout.ObjectField(obj.gameObj, typeof(GameObject), false);
        
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Prefab");
        EditorGUILayout.LabelField("Missing References");
        GUILayout.EndHorizontal();

        foreach (var comp in obj.componentsWitMissingRef)
        {
            foreach(var missingRef in comp.missingRefs)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(comp.name);
                EditorGUILayout.LabelField(missingRef);
                GUILayout.EndHorizontal();
            }
        }

        GUILayout.EndVertical();
    }


}
