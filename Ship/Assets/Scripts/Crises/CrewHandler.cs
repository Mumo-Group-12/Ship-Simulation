using System.Collections.Generic;
using UnityEngine;
class CrewHandler : MonoBehaviour
{
    public List<CrewMate> crew;
    public List<GameObject> rooms;
    [SerializeField] private MarkerTracker marker_tracker;

    void Update()
    {
        //// Convert rooms (gameObjects) to numbers (ints)
        for (int i = 0; i < crew.Count; i++)
        {
            for (int j = 0; j < rooms.Count; j++)
            {
                if (marker_tracker.markerRoomMappings[i].room == rooms[j])
                {
                    crew[i].location = j; break;
                }
                crew[i].location = -1;
            }
        }
    }

    public List<CrewMate> GetCrewOfRoom(int room_id) //
    {
        List<CrewMate> maties = new List<CrewMate>();
        foreach (CrewMate c in crew)
        {
            if(c.location == room_id) maties.Add(c);
        }

        return maties;
    }

}

[System.Serializable]
class CrewMate
{
    public string name;
    public CrisisData.CrisisType specialization;
    public int location;
}