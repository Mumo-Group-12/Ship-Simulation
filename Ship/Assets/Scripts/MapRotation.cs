using UnityEngine;

public class MapRotation : MonoBehaviour
{
    public Transform shipMap;
    void Update()
    {
        transform.rotation = shipMap.rotation;
    }
}
