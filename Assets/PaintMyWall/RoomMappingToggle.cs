using UnityEngine;

public class RoomMappingToggle : MonoBehaviour
{
    public RoomMapping roomMappingScript;

    public void ToggleRoomMapping()
    {
        if (roomMappingScript != null)
        {
            roomMappingScript.enabled = !roomMappingScript.enabled;
            Debug.Log("Room Mapping " + (roomMappingScript.enabled ? "Enabled" : "Disabled"));
        }
    }
}