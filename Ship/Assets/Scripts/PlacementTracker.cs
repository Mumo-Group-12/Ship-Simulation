using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlacementTracker : MonoBehaviour
{
    public Vector3 screenPosition;
    public GameObject currentRoom;
    public GameObject shipMap;
    public Camera shipMapCamera;
    private List<GameObject> rooms = new List<GameObject>();
    void Start()
    {
        // Get all rooms in the shipMap
        foreach (Transform child in shipMap.transform)
        {
            rooms.Add(child.GameObject());
        }
    }

    void Update()
    {
        screenPosition = shipMapCamera.WorldToScreenPoint(transform.position);
        bool inRoom = false;

        // Check if the object is currently in a room (above a panel on the canvas from the ship map camera perspective)
        foreach (GameObject room in rooms)
        {
            Vector2 localPoint;
            RectTransform rectTransform = room.GetComponent<RectTransform>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPosition, shipMapCamera, out localPoint);
            if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPosition, shipMapCamera))
            {
                currentRoom = room;
                inRoom = true;
                break; // Don't need to look if we are in any other rooms (as long as we DON'T OVERLAP THEM!!!)
            }
        }
        // If we are not in any room, set currentRoom to null
        if (!inRoom)
        {
            currentRoom = null;
        }
    }
}
