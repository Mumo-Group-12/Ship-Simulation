using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
class Crisis
{
    public CrisisData.CrisisType crisis_type;
    public int room_number;
    public float crisis_health = 3;
    public float crisis_time = 3;
    public int crisis_id = -1;

    public Crisis(CrisisData.CrisisType type, int room_number, float health, float time)
    {
        this.crisis_type = type;
        this.room_number = room_number;
        this.crisis_health = health;
        this.crisis_time = time;
    }
}