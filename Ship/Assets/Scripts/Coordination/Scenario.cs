using UnityEngine;

[CreateAssetMenu(fileName = "Scenario", menuName = "Scriptable Objects/Scenario")]
public class Scenario : ScriptableObject
{
    public CrisisSchedule crisis_schedule;
    public ObstacleTrack navigation_track;
    public bool can_see;
    public bool can_hear;
}
