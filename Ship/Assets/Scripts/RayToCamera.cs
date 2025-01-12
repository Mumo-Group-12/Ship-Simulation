using UnityEngine;

public class RayToCamera : MonoBehaviour
{
    public Camera mainCamera; // Assign the main camera here
    public GameObject plane; // Assign the background plane
    public GameObject marker; // Assign the imageTarget here (not the marker itself!)

    void Update()
    {
        PlaceObjectOnPlane();
    }

    void PlaceObjectOnPlane()
    {
        // Create a ray from the tracker to the camera
        Vector3 trackerPosition = transform.position;
        Vector3 cameraPosition = mainCamera.transform.position;
        Vector3 rayDirection = (cameraPosition - trackerPosition).normalized;

        Ray ray = new Ray(trackerPosition, rayDirection);

        // Get the plane's normal and a point on the plane
        Plane planeObject = new Plane(
            plane.transform.up, // Plane's normal
            plane.transform.position // A point on the plane
        );

        // Calculate the intersection point of the ray and the plane
        if (planeObject.Raycast(ray, out float enter))
        {
            // Find the intersection point
            Vector3 intersectionPoint = ray.GetPoint(enter);

            // Set the object's position to the intersection point
            marker.transform.position = intersectionPoint;
        }
    }
}