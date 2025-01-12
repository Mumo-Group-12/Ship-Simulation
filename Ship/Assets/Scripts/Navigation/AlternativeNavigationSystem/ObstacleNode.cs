using UnityEngine;

[System.Serializable]
public class ObstacleNode
{
    public bool left_oriented = false; //Which way is the obstacle pointing
    [Range(3, 15)] public float delta_y = 2; //Distance needed to be covered before next obstacle is generated
}
