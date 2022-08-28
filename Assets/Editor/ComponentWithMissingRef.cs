using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentWithMissingRef
{
    public string name;
    public List<string> missingRefs = new List<string>();

    public ComponentWithMissingRef(string newName)
    {
        name = newName;
    }

    public void AddMissingRef(string newMissingRef)
    {
        missingRefs.Add(newMissingRef);
    }

}
