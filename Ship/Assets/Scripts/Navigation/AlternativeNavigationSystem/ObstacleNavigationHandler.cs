using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class ObstacleNavigationHandler : MonoBehaviour
{
    private Vector2 m_ship_position;
    [SerializeField] private float m_ship_forward_speed = 1;
    [SerializeField] private float m_ship_dodge_speed = 5;

    [SerializeField] private ObstacleTrack track;

    [SerializeField] private GameObject ship_proxy;
    [SerializeField] private GameObject obstacle_prefab;
    [SerializeField] private GameObject obstacle_object;
    private Vector2 obstacle_target_position;

    [SerializeField] private AudioSource nav_audio_source = null;
    [SerializeField] private AudioSource nav_boom_source = null;
    private int obstacle_index = -1;

    private bool obstacles_remaining = false;
    private float distance_until_next_obstacle = 0;
    private float y_offset = -100;
    private bool dodged_current_obstacle;
    private bool visible_obstacle = false;
    private bool disabled_obstacle = false;

    private int failed_navigations = 0;

    [SerializeField] private GameObject audio_post_it;
    [SerializeField] private GameObject screen;
    [SerializeField] private Material screen_on_material;
    [SerializeField] private Material screen_off_material;
    private bool hearing = false;
    private List<float> reaction_times;




    //Move obstacle left or right (works best with inputs of -3 or 3)
    public void MoveShip(int direction)
    {
        if (!visible_obstacle)
            return;
        obstacle_target_position = Vector3.right * direction;
        bool dodged_in_right_direction = track.obstacles[obstacle_index].left_oriented && Math.Abs(direction) < 0 || !track.obstacles[obstacle_index].left_oriented && Math.Abs(direction) > 0;
        dodged_current_obstacle = dodged_in_right_direction;
    }




    public List<float> GetReactionTimes()
    {
        return reaction_times;
    }

    private void Start()
    {
        nav_audio_source.volume = 0;
        reaction_times = new List<float>();
    }

    //Set whether or not the user will hear audio and/or see visuals or not
    public void SetAudioVisual(bool audio, bool visual)
    {
        screen.GetComponent<Renderer>().material = visual ? screen_on_material : screen_off_material;
        audio_post_it.SetActive(!visual);
        hearing = audio;
    }

    //Set obstacle course playlist and immediately play it
    public void SetTrack(ObstacleTrack track){
        reaction_times = new List<float>();
        nav_audio_source.volume = 0;
        this.track = track;
        obstacles_remaining = true;
        obstacle_index = -1;
        NewObstacle();
        failed_navigations = 0;
    }


    //Are there no obstacles left? Then we are done.
    public bool IsDone()
    {
        return !obstacles_remaining && distance_until_next_obstacle <= 0;
    }

    //Generate a new obstacle, returning true on success and false on fail.
    public bool NewObstacle()
    {
        obstacle_index += 1;
        if (obstacle_index >= track.obstacles.Count)
            return false;
        reaction_times.Add(0);
        dodged_current_obstacle = false;
        obstacle_object.SetActive(true);
        distance_until_next_obstacle = track.obstacles[obstacle_index].delta_y;
        obstacle_target_position = new Vector2(0, distance_until_next_obstacle);
        Vector2 obstacle_new_position = new Vector2(0, distance_until_next_obstacle);
        obstacle_object.transform.position = obstacle_new_position;
        if(track.obstacles[obstacle_index].left_oriented)
        {
            obstacle_object.transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            obstacle_object.transform.localScale = new Vector3(1, 1, 1);
        }
        disabled_obstacle = false;
        return true;
    }

    public int GetFails()
    {
        return failed_navigations;
    }

    //Handle obstacles and noise
    private void Update()
    {
        if(obstacles_remaining)
        {
            distance_until_next_obstacle -= m_ship_forward_speed * Time.deltaTime; //handles indication on when to generate next target
            Vector2 obst_pos = Vector3.MoveTowards(obstacle_object.transform.position, obstacle_target_position, Time.deltaTime * m_ship_dodge_speed); //Linearly interpolate x-value of positional vector
            obst_pos.y = distance_until_next_obstacle; //set obstacle y-position to match time until next generation
            obstacle_object.transform.position = !disabled_obstacle? obst_pos : new Vector3(0, -1000, 0); //offset so this does not mess with any other systems

            if(distance_until_next_obstacle <= -4) //Duct tape solution DO NOT REMOVE UNTIL CLEARED WITH ME
            {
                bool list_within_index = NewObstacle();
                if (!list_within_index)
                {
                    obstacles_remaining = false;
                }
            }
            RaycastHit2D hit = Physics2D.Raycast(m_ship_position, Vector2.up, 3f); //collision check
            visible_obstacle = obstacle_object.transform.position.y < 3f && obstacle_object.transform.position.y > -4; //Is obstacle visible to the human eye?
            if (hit.collider != null) //if the ray hits an object, collision is impending. Sound the alarms (if we have hearing on)
            {
                if(!dodged_current_obstacle) //If we have not started to move out of the way of the object, count reaction time.
                    reaction_times[obstacle_index] += Time.deltaTime;
                if (hearing) 
                    nav_audio_source.volume = 1;
                nav_audio_source.pitch = 0.8f + (4f - hit.distance) * 0.2f;
                nav_audio_source.panStereo = track.obstacles[obstacle_index].left_oriented? 1 : -1;
                if(hit.distance < 0.1f)
                {
                    reaction_times[obstacle_index] = Mathf.Infinity;
                    disabled_obstacle = true;
                    nav_boom_source.Play();
                    failed_navigations += 1;
                    obstacle_object.SetActive(false);
                }
            }
            else
            {
                nav_audio_source.volume = 0;
            }
            if(Input.GetKeyDown(KeyCode.LeftArrow))
            {
                MoveShip(3);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                MoveShip(-3);
            }

        }
       
    }




}
