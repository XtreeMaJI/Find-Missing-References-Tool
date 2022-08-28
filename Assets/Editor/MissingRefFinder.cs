using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class MissingRefFinder : EditorWindow
{
    class SceneInProject
    {
        public string path;
        public string name;
        public bool shouldCheck;
        public SceneInProject(string newPath, string newName, bool flagShouldCheck)
        {
            path = newPath;
            name = newName;
            shouldCheck = flagShouldCheck;
        }
    }

    private List<ObjectWithMissingRef> objectsWithMissingRef = new List<ObjectWithMissingRef>();
    private bool shouldCheckScenes = false; // Проверять ли ссылки в объектах на сцене

    public Vector2 scrollPos = new Vector2();

    private List<SceneInProject> scenesInProject = new List<SceneInProject>();

    [MenuItem("Tools/Missing references finder")]
    public static void ShowWindow()
    {
        GetWindow<MissingRefFinder>();
    }

    private void OnEnable()
    {
        UpdateScenesList();
    }

    private void OnGUI()
    {
        DrawScenesList();

        if (GUILayout.Button("Find Misssing References"))
        {
            UpdateObjectList();
        }

        DrawObjectList();

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

            //Добавляем объект в список обектов с отсутствующими ссылками, если таковые у него есть
            TryAddObjToList(obj);
        }

        CheckObjectsFromSelectedScenes();
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
        //Если объект уже в списке, то не проверяем его
        foreach(ObjectWithMissingRef elem in objectsWithMissingRef)
        {
            if (elem.gameObj == obj)
                return;
        }

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
        //Для объектов, являющихся дочерними
        ShowParentsChain(obj.gameObj);

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Missing Reference", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("In component", EditorStyles.boldLabel);
        GUILayout.EndHorizontal();

        foreach (var comp in obj.componentsWitMissingRef)
        {
            foreach(var missingRef in comp.missingRefs)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(missingRef);
                EditorGUILayout.LabelField(comp.name);
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

    private void UpdateScenesList()
    {
        scenesInProject.Clear();
        string searchFolder = "Assets";
        string filter = "";
        string[] paths = AssetDatabase.FindAssets(filter, new[] { searchFolder });
        foreach (var path in paths)
        {
            SceneAsset asset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(path), typeof(SceneAsset)) as SceneAsset;
            if (asset)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(path);
                SceneInProject scene = new SceneInProject(assetPath, asset.name, false);
                scenesInProject.Add(scene);
            }
        }
    }

    private void DrawScenesList()
    {
        EditorGUILayout.LabelField("Scenes To Check");
        foreach(var scene in scenesInProject)
            scene.shouldCheck = EditorGUILayout.Toggle(scene.name, scene.shouldCheck);
    }

    private void CheckObjectsFromSelectedScenes()
    {
        bool shouldCheckScenes = false;

        //Открываем отмеченные сцены
        foreach (var scene in scenesInProject)
        {
            if (!scene.shouldCheck)
            {
                continue;
            }

            EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Additive);
            shouldCheckScenes = true;
        }

        //Закрываем лишние
        foreach (var scene in scenesInProject)
        {
            if (!scene.shouldCheck && EditorSceneManager.sceneCount > 1)
            {
                EditorSceneManager.CloseScene(EditorSceneManager.GetSceneByName(scene.name), true);
                continue;
            }
        }

        if (shouldCheckScenes)
            CheckObjectsFromScene();
    }

}
