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

    public Vector2 scrollPos = new Vector2();

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

            //Добавляем объект в список обектов с пропущенными ссылками, если таковые у него есть
            TryAddObjToList(obj);
        }
    }

    private void TryAddObjToList(GameObject obj)
    {
        var missingRefObj = new ObjectWithMissingRef(obj);
        missingRefObj.FillObjectData();

        if (missingRefObj.hasMissingRefs())
        {
            objectsWithMissingRef.Add(missingRefObj);
        }

        foreach(Transform child in obj.transform)
        {
            TryAddObjToList(child.gameObject);
        }
    }

    private void DrawObjectList()
    {
        GUILayout.BeginVertical();
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        foreach (var obj in objectsWithMissingRef)
        {
            DrawElemFromList(obj);
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private void DrawElemFromList(ObjectWithMissingRef obj)
    {
        GUILayout.BeginVertical();

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Object with missing reference", EditorStyles.boldLabel);
        EditorGUILayout.ObjectField(obj.gameObj, typeof(GameObject), false);
        //Для объектов, являющихся дочерними префабами
        ShowParentsChain(obj.gameObj);

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Component", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Missing References", EditorStyles.boldLabel);
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

    private void ShowParentsChain(GameObject obj)
    {
        if (!obj)
            return;

        Transform parent = obj.transform.parent;

        if (!parent)
            return;

        EditorGUILayout.LabelField("Located in");

        while (parent)
        {
            EditorGUILayout.ObjectField(parent.gameObject, typeof(GameObject), false);
            parent = parent.transform.parent;
        }   
    }
}
