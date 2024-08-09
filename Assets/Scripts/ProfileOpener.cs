using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfileOpener : MonoBehaviour
{
    public GameObject profileMenu;

    private void Start()
    {
        profileMenu.SetActive(false);
    }
    public void OpenProfile()
    {
        profileMenu.SetActive(true);
    }

    public void CloseProfile()
    {
        profileMenu.SetActive(false);
    }
}


