using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class MissingRefFinder : EditorWindow
{
    private List<ObjectWithMissingRef> objectsWithMissingRef = new List<ObjectWithMissingRef>();
    private bool shouldCheckScenes = false; // Проверять ли ссылки в объектах на сцене

    public Vector2 scrollPos = new Vector2();

    [MenuItem("Tools/Missing references finder")]
    public static void ShowWindow()
    {
        GetWindow<MissingRefFinder>();
    }

    private void OnGUI()
    {
        shouldCheckScenes = EditorGUILayout.Toggle("Should check references in objects from scenes?", shouldCheckScenes);

        if (GUILayout.Button("Find Misssing References"))
        {
            UpdateObjectList();
        }

        DrawObjectList();

    }

    private void UpdateObjectList()
    {
        objectsWithMissingRef.Clear();
        List<string> pathsToScenes = new List<string>();

        string searchFolder = "Assets";
        string filter = "";
        string[] paths = AssetDatabase.FindAssets(filter, new[] { searchFolder });
        foreach (var path in paths)
        {
            SceneAsset asset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(path), typeof(SceneAsset)) as SceneAsset;
            if (asset)
                pathsToScenes.Add(AssetDatabase.GUIDToAssetPath(path));

            GameObject obj = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(path), typeof(GameObject)) as GameObject;
            if (obj == null)
                continue;

            //Добавляем объект в список обектов с отсутствующими ссылками, если таковые у него есть
            TryAddObjToList(obj);
        }

        if(shouldCheckScenes)
        {
            foreach(string scenePath in pathsToScenes)
            {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                CheckObjectsFromScene();
            }
        }

    }

    private void CheckObjectsFromScene()
    {
        List<GameObject> objects = new List<GameObject>(FindObjectsOfType<GameObject>());
        foreach(GameObject obj in objects)
        {
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
        EditorGUILayout.LabelField("Missing Reference", EditorStyles.boldLabel);
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
