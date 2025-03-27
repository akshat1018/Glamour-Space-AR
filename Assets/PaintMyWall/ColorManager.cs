using UnityEngine;
using UnityEngine.UI;

public class ColorManager : MonoBehaviour
{
    public Material[] wallMaterials; // Array of materials for different colors
    public Button[] colorButtons; // Array of color buttons

    private void Start()
    {
        // Assign onClick events to each button
        for (int i = 0; i < colorButtons.Length; i++)
        {
            int index = i; // Capture the index for the lambda
            colorButtons[i].onClick.AddListener(() => ChangeColor(index));
        }
    }

    private void ChangeColor(int index)
    {
        if (index >= 0 && index < wallMaterials.Length)
        {
            WallPainter wallPainter = FindObjectOfType<WallPainter>();
            if (wallPainter != null && wallPainter.currentWall != null)
            {
                Renderer wallRenderer = wallPainter.currentWall.GetComponent<Renderer>();
                if (wallRenderer != null)
                {
                    wallRenderer.material = wallMaterials[index];
                }
            }
        }
    }
}