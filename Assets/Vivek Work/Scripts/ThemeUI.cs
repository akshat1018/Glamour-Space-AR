using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThemeUI : MonoBehaviour
{
    public WallThemeManager themeApplier;

    // when a user selects a theme button
    public void OnThemeButtonPressed(int themeIndex)
    {
        themeApplier.ApplyTheme(themeIndex);
    }
}

