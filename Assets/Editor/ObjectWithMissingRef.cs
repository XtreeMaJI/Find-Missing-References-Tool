using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ObjectWithMissingRef
{
    public string name;
    public List<ComponentWithMissingRef> componentsWitMissingRef = new List<ComponentWithMissingRef>();

    public GameObject gameObj { get; private set; }

    public ObjectWithMissingRef(GameObject newObj)
    {
        gameObj = newObj;
    }

    public void AddMissingRefToComponent(string compName, string missingRef)
    {
        ComponentWithMissingRef comp = TryGetComponent(compName);
        if (comp == null)
        {
            comp = new ComponentWithMissingRef(compName);
            componentsWitMissingRef.Add(comp);
        }

        comp.AddMissingRef(missingRef);

    }
        
    private ComponentWithMissingRef TryGetComponent(string compName)
    {
        foreach(ComponentWithMissingRef comp in componentsWitMissingRef)
        {
            if (comp.name == compName)
                return comp;
        }

        return null;
    }

    public void FillObjectData()
    {
        List<string> objComponents = new List<string>();
        Component[] components = gameObj.GetComponents<Component>();
        foreach (Component comp in components)
        {
            FindComponentMissingProperties(comp);
        }
    }

    private void FindComponentMissingProperties(Component comp)
    {
        string compName = comp.GetType().Name;
        SerializedObject serializedComp = new SerializedObject(comp);
        SerializedProperty prop = serializedComp.GetIterator();
        prop.NextVisible(true);
        while (prop.NextVisible(true))
        {
            if (prop.propertyType != SerializedPropertyType.ObjectReference)
                continue;

            if (prop.objectReferenceInstanceIDValue != 0 && !prop.objectReferenceValue)
            {
                if (prop.depth == 0)
                    AddMissingRefToComponent(compName, prop.displayName);
                else
                    AddMissingRefToComponent(compName, prop.propertyPath);
            }
        }
    }

    public bool hasMissingRefs()
    {
        foreach(var comp in componentsWitMissingRef)
        {
            if(comp.missingRefs.Count > 0)
            {
                return true;
            }
        }
        return false;
    }

}
