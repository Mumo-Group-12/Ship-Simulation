using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MarkerRoomMapping
{
    public GameObject marker;  // The marker
    public GameObject room;    // The room the marker is in
}
public class MarkerTracker : MonoBehaviour
{
    public Camera ARCamera; // The AR camera
    public List<GameObject> markerTargets; // Our markers
    public List<MarkerRoomMapping> markerRoomMappings; // List to track markers and their rooms

    void Start() // Initialize the markers
    {
        markerRoomMappings = new List<MarkerRoomMapping>();
        foreach (GameObject marker in markerTargets) { markerRoomMappings.Add(new MarkerRoomMapping { marker = marker, room = null }) ; }
    }

    void Update()
    {
        for(int i = 0; i < markerTargets.Count; i++)
        {
            CheckIfInRoom(i);
        }
    }

    void CheckIfInRoom(int i) // The markers' actual positions are fricked, so we're doing rays
    {
        // Create a ray from the tracker to the camera
        Vector3 trackerPosition = markerTargets[i].transform.position;
        Vector3 cameraPosition = ARCamera.transform.position;
        Vector3 rayDirection = (cameraPosition - trackerPosition).normalized;

        Ray ray = new Ray(trackerPosition, rayDirection);


        // Check if the ray intersects a room's collider (the markers' actual positions are fricked)
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            markerRoomMappings[i].room = hit.collider.gameObject;
        }
        else { markerRoomMappings[i].room = null; }
    }
}