using UnityEngine;
using UnityEngine.UI;

public class DropDownManager : MonoBehaviour
{
    private string websiteURL = "https://glamour-space.onrender.com/";

    public GameObject inventoryPanel;
    public GameObject toolsPanel;

    void Start()
    {
        inventoryPanel.SetActive(false);
        toolsPanel.SetActive(false);
    }

    public void ToggleInventoryPanel()
    {
        bool newState = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(newState);
        toolsPanel.SetActive(false); // Deactivate toolsPanel if inventoryPanel is activated
    }

    public void ToggleToolsPanel()
    {
        bool newState = !toolsPanel.activeSelf;
        toolsPanel.SetActive(newState);
        inventoryPanel.SetActive(false); // Deactivate inventoryPanel if toolsPanel is activated
    }

    public void OpenWebsite()
    {
        Application.OpenURL(websiteURL);
    }
}
