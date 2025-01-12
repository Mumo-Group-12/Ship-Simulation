using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;


[System.Serializable]
public class PostScenarioData
{
    public string scenario_name;
    public int failed_crises;
    public int failed_navigations;
    public List<float> crisis_reaction_times;
    public List<float> obstacle_reaction_times;
    public PostScenarioData(string scenario_name, int failed_crises, int failed_navigations, List<float> crisis_reaction_times, List<float> obstacle_reaction_times) {
        this.scenario_name = scenario_name;
        this.failed_navigations = failed_navigations;
        this.failed_crises = failed_crises;
        this.obstacle_reaction_times = obstacle_reaction_times;
        this.crisis_reaction_times = crisis_reaction_times;
    }
}

public class ScenarioHandler : MonoBehaviour
{
    [SerializeField] private List<Scenario> training_scenarios;
    [SerializeField] private List<Scenario> audio_scenarios;
    [SerializeField] private List<Scenario> visual_scenarios;
    [SerializeField] private List<Scenario> audiovisual_scenarios;

    [SerializeField] private CrisisHandler crisis_handler;
    [SerializeField] private ObstacleNavigationHandler navigation_handler;

    [SerializeField] private List<Scenario> scenarios = new List<Scenario>(); //A playlist of scenarios
    private int scenario_index = -1;
    private Scenario current_scenario;
    [SerializeField] private GameObject success_text = null;
    [SerializeField] private GameObject scenario_text = null;
    
    public List<PostScenarioData> data = new List<PostScenarioData>();

    private enum ScenarioState
    {
        Running,
        Idle
    }
    private ScenarioState my_scenario_state;

   void Start()
   {
        for(int i = 0; i < training_scenarios.Count; i++) //Add all training scenarios to playlist
        {
            print(training_scenarios[i]);
            scenarios.Add(training_scenarios[i]);
        }
        while (audio_scenarios.Count > 0 || visual_scenarios.Count > 0 || audiovisual_scenarios.Count > 0) //Add all experiment scenarios to playlist in a random order
        {
            int selection = Random.Range(0, 3); //pick one of the three categories in this dumb loop
            if (selection == 0 && audio_scenarios.Count > 0)
            {
                ExtractScenario(audio_scenarios);
                audio_scenarios.Clear();
            } else if (selection == 1 && visual_scenarios.Count > 0)
            {
                ExtractScenario(visual_scenarios);
                visual_scenarios.Clear();
            } else if (audiovisual_scenarios.Count > 0)
            {
                ExtractScenario(audiovisual_scenarios);
                audiovisual_scenarios.Clear();
            }
        }
        my_scenario_state = ScenarioState.Idle;
   }
    
    //Add random scenario from list, remove said scenario from list
    public void ExtractRandomScenario (List<Scenario> ext_scenarios)
    {
        int selection = Random.Range(0, ext_scenarios.Count);
        scenarios.Add(ext_scenarios[selection]);
        ext_scenarios.RemoveAt(selection);
    }

    public void ExtractScenario(List<Scenario> ext_scenarios)
    {
        List<Scenario> clone = new List<Scenario>(ext_scenarios);
        for(int i = 0; i < ext_scenarios.Count; i++)
        {
            scenarios.Add(ext_scenarios[i]);
        }
    }


    //If both parts of a scenario are done, we are done with this scenario
    public bool ScenarioIsDone ()
    {
        return crisis_handler.IsDone() && navigation_handler.IsDone();
    }

    //Load the next scenario in scenario queue into the system
    public void NextScenario ()
    {
        scenario_index += 1;
        if (scenario_index >= scenarios.Count)
        {
            //export data
            success_text.GetComponent<TextMeshProUGUI>().text = "Printed!";
            PrintResultsToFile();
            return;
        }
        current_scenario = scenarios[scenario_index];
        navigation_handler.SetAudioVisual(current_scenario.can_hear, current_scenario.can_see);
        crisis_handler.SetCrisisSchedule(scenarios[scenario_index].crisis_schedule);
        navigation_handler.SetTrack(scenarios[scenario_index].navigation_track);
    }
    
   //Handle input and text display
   void Update()
   {
        if(scenario_index + 1 < scenarios.Count)
        {
            scenario_text.GetComponent<TextMeshProUGUI>().text = "Up next: " + scenarios[scenario_index + 1].name;
        }
        else
        {
            scenario_text.GetComponent<TextMeshProUGUI>().text = "Up next: The ceaseless void (You are done!)";
        }
        if (my_scenario_state == ScenarioState.Idle && Input.GetKeyDown(KeyCode.Space))
        {
            success_text.SetActive(false);
            NextScenario();
            my_scenario_state = ScenarioState.Running;
            scenario_text.SetActive(false);
        }
        else if (my_scenario_state == ScenarioState.Running && ScenarioIsDone())
        {
            scenario_text.SetActive(true);
            success_text.SetActive(true);
            my_scenario_state = ScenarioState.Idle;
            data.Add(new PostScenarioData(current_scenario.name, 
                                            crisis_handler.GetFails(), 
                                            navigation_handler.GetFails(), 
                                            crisis_handler.GetReactionTimes(), 
                                            navigation_handler.GetReactionTimes()
                                            ));
            if (scenario_index == scenarios.Count)
            {
                success_text.GetComponent<TextMeshProUGUI>().text = "All clear! \n Press Spacebar to print results to file.";
            }
        }
    }
   void PrintResultsToFile()
   {
        File.WriteAllText(Application.dataPath + "/Experiment.txt", "");
        using (StreamWriter sw = new StreamWriter(Application.dataPath + "/Experiment.txt", true))
        {
            sw.WriteLine("===================================");
            sw.WriteLine("Raw test data");
            sw.WriteLine("===================================");
            foreach (PostScenarioData scenario_data in data)
            {
                sw.WriteLine(scenario_data.scenario_name);
                sw.WriteLine("Collisions: " + scenario_data.failed_navigations);
                sw.WriteLine("Failed crises: " + scenario_data.failed_crises);
                sw.WriteLine("-----------------------------------");
                sw.WriteLine("Crisis reaction Times:");
                for (int i = 0; i < scenario_data.crisis_reaction_times.Count; i++)
                {
                    float line = scenario_data.crisis_reaction_times[i];
                    if (line != Mathf.Infinity)
                        sw.WriteLine("Crisis " + i + ": " + line + "s");
                    else
                        sw.WriteLine("Crisis " + i + ": " + "Failed");
                }
                sw.WriteLine("-----------------------------------");
                sw.WriteLine("Obstacle reaction Times:");
                for (int i = 0; i < scenario_data.obstacle_reaction_times.Count; i++)
                {
                    float line = scenario_data.obstacle_reaction_times[i];
                    if (line != Mathf.Infinity)
                        sw.WriteLine("Obstacle " + i + ": " + line + "s");
                    else
                        sw.WriteLine("Obstacle " + i + ": " + "Failed");
                }
                sw.WriteLine("===================================");
           }
        }
   }
}
