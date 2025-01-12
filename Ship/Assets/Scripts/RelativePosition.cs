using UnityEngine;

public class RelativePosition : MonoBehaviour
{
    public GameObject plane; // Reference to the plane GameObject
    public GameObject cube; // Reference to the cube GameObject
    public Camera arCamera; // Reference to the AR Camera
    public Texture2D planeTexture; // Texture applied to the plane

    void Update()
    {
        if (plane != null && cube != null && arCamera != null && planeTexture != null)
        {
            // Get the position of the cube relative to the AR camera
            Vector3 cubeRelativeToCamera = arCamera.transform.InverseTransformPoint(cube.transform.position);

            // Get the position of the plane relative to the AR camera
            Vector3 planeRelativeToCamera = arCamera.transform.InverseTransformPoint(plane.transform.position);

            // Get the position of the cube relative to the plane
            Vector3 cubeRelativeToPlane = plane.transform.InverseTransformPoint(cube.transform.position);

            // Log the results (optional)
            Debug.Log("Cube relative to AR Camera: " + cubeRelativeToCamera);
            Debug.Log("Plane relative to AR Camera: " + planeRelativeToCamera);
            Debug.Log("Cube relative to Plane: " + cubeRelativeToPlane);

            // Map the cube's position on the plane to the plane's texture coordinates
            Renderer planeRenderer = plane.GetComponent<Renderer>();
            if (planeRenderer != null)
            {
                Vector2 planeSize = new Vector2(planeRenderer.bounds.size.x, planeRenderer.bounds.size.z);

                // Normalize the cube's local position to texture UV coordinates (0 to 1)
                Vector2 uvCoords = new Vector2(
                    (cubeRelativeToPlane.x / planeSize.x) + 0.5f,
                    (cubeRelativeToPlane.z / planeSize.y) + 0.5f
                );

                // Convert UV coordinates to pixel coordinates on the texture
                int pixelX = Mathf.RoundToInt(uvCoords.x * planeTexture.width);
                int pixelY = Mathf.RoundToInt(uvCoords.y * planeTexture.height);

                // Draw a pixel on the plane's texture at the calculated position
                planeTexture.SetPixel(pixelX, pixelY, Color.red);
                planeTexture.Apply();

                Debug.Log("Cube pixel position on texture: (" + pixelX + ", " + pixelY + ")");
            }
        }
        else
        {
            Debug.LogWarning("Please assign the Plane, Cube, AR Camera, and Plane Texture in the Inspector.");
        }
    }
}
