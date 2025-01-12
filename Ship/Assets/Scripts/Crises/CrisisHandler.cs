using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class CrisisHandler : MonoBehaviour
{
    [SerializeField] private CrisisSchedule crisis_schedule;
    private int crisis_index;
    private float time_until_next_crisis;
    [SerializeField] private List<Crisis> active_crises = new List<Crisis> {};
    [SerializeField] private List<GameObject> crisis_icons = new List<GameObject> { };
    [SerializeField] private CrewHandler crew_handler;
    private int failed_crises = 0;
    private bool crises_in_queue = false;
    [SerializeField] private AudioSource audio_source;
    [SerializeField] private AudioSource audio_source_fail;
    [SerializeField] private AudioSource audio_source_success;
    [SerializeField] private List<Material> crisesMaterials;
    [SerializeField] private GameObject crisisIcon;
    private List<float> reaction_times;

    const float CRISIS_TIME = 8;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    public int GetFails(){ 
        return failed_crises; 
    }

    public List<float> GetReactionTimes()
    {
        return reaction_times;
    }

    //Load a new Cisis schedule, start playing immediately
    public void SetCrisisSchedule(CrisisSchedule schedule)
    {
        reaction_times = new List<float>();
        failed_crises = 0;
        crisis_index = -1;
        crisis_schedule = schedule;
        time_until_next_crisis = 0;
        crises_in_queue = true;
        NextCrisis();
    }

    //Generate next crisis
    public void NextCrisis()
    {
        if (crisis_index > -1 && crisis_schedule.events[crisis_index] != null)
        {
            reaction_times.Add(0);
            audio_source.Play();
            CrisisData crisis_data = crisis_schedule.events[crisis_index];
            int room = SelectCrisisRoom(crisis_data.crisis_type);
            Crisis cris = new Crisis(crisis_data.crisis_type, room, 0.01f, CRISIS_TIME);
            cris.crisis_id = crisis_index;
            active_crises.Add(cris);
            // Add crisis icon and place it over the room
            GameObject crisIcon = Instantiate(crisisIcon, crew_handler.rooms[room].transform); // (hopefully) this makes the icon a child of the room and fixes the position where we want it
            crisIcon.transform.localPosition = new Vector3(0, 0.1f, 0);
            crisIcon.GetComponent<Renderer>().material = crisesMaterials[(int)cris.crisis_type];
            // TODO: change material depending on crisis
            crisis_icons.Add(crisIcon);
        }
        crisis_index += 1;
        if (crisis_index < crisis_schedule.events.Count)
            time_until_next_crisis = crisis_schedule.events[crisis_index].delta_t;
        else
        {
            crises_in_queue = false;
        }
    }


    // Very greedy alg
    // Get random room
    public int SelectCrisisRoom(CrisisData.CrisisType type) 
    {
        int iterations = 0;
        int room = Random.Range(0, 5);
        while (CrewMatchesSpecialization(crew_handler.GetCrewOfRoom(room), type) || RoomHasSameCrisis(room, type))
        {
            iterations++;
            room = Random.Range(0, 5);
            if (iterations == 100) 
                break;
        }
        return room;
    }

    public bool IsDone()
    {
        return !crises_in_queue && active_crises.Count <= 0;
    }


    // Update is called once per frame
    void Update()
    {
        if (crises_in_queue)
        {
            time_until_next_crisis -= Time.deltaTime;
            if (time_until_next_crisis <= 0)
            {
                NextCrisis();
            }
        }
        List<Crisis> active_crises_clone = new List<Crisis>(active_crises);
        List<GameObject> crisis_icons_clone = new List<GameObject>(crisis_icons);
        for (int i = 0; i < active_crises.Count; i++)
        {
            Crisis crisis = active_crises[i];
            GameObject crisis_icon = crisis_icons[i];
            List<CrewMate> crew_in_room = crew_handler.GetCrewOfRoom(crisis.room_number);
            reaction_times[crisis.crisis_id] += Time.deltaTime;
            if (CrewMatchesSpecialization(crew_in_room, crisis.crisis_type))
            {
                crisis.crisis_health -= Time.deltaTime;
                if (crisis.crisis_health <= 0)
                {
                    active_crises_clone.Remove(crisis);
                    Destroy(crisis_icon);
                    crisis_icons_clone.Remove(crisis_icon);
                    audio_source_success.Play();
                }
            }
            else
            {
                crisis.crisis_time -= Time.deltaTime;
            }
            if (crisis.crisis_time < 0)
            {
                reaction_times[crisis.crisis_id] = Mathf.Infinity;
                active_crises_clone.Remove(crisis);
                Destroy(crisis_icon);
                crisis_icons_clone.Remove(crisis_icon);
                failed_crises += 1;
                audio_source_fail.Play();
            }
        }
        active_crises = active_crises_clone;
        crisis_icons = crisis_icons_clone;
    }

    bool CrewMatchesSpecialization(List<CrewMate> crew_in_room, CrisisData.CrisisType crisis_type)
    {
        foreach(CrewMate crew in crew_in_room)
        {
            if(crew.specialization == crisis_type)
            {
                return true;
            }
        }
        return false;
    }

    bool RoomHasSameCrisis(int room_index, CrisisData.CrisisType crisis_type)
    {
        for(int i = 0; i < active_crises.Count; i++)
        {
            if (active_crises[i].room_number == room_index)
            {
                return true;
            }
        }
        return false;
    }
}
