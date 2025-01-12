using UnityEngine;

[System.Serializable]
public class CrisisData
{
    public enum CrisisType {
        Fire,
        Leak,
        Anomaly
    }
    public CrisisType crisis_type;
    [Range(1, 15)] public float delta_t;
}
