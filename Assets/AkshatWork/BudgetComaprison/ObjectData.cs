using UnityEngine;

[System.Serializable]
public class ObjectData
{
    public GameObject objectToPlace;
    public string objectName;   // Name of the object (e.g., "Luxury Sofa")
    public string category;     // Object category (e.g., "Sofa", "Chair")
    public float price;         // Price of the object
    public Sprite image;        // Image of the object (UI display)
}
