using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Firebase.Auth; // Add this line

public class References : MonoBehaviour
{
    public static string userName = "";
    public static string userEmail = "";
    
    public static void SetUserData(FirebaseUser user) // Explicit type
    {
        userName = user?.DisplayName ?? "Guest";
        userEmail = user?.Email ?? "";
    }
    
    public static void ClearUserData()
    {
        userName = "";
        userEmail = "";
    }
}