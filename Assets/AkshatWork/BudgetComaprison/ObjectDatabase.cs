using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ObjectDatabase", menuName = "Database/ObjectDatabase")]
public class ObjectDatabase : ScriptableObject
{
    public List<ObjectData> objectsList; // List of all interior design objects
}
