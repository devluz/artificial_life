﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulation : MonoBehaviour
{
    public static readonly float RADIUS = 20;

    public enum SimState
    {
        NotYetStarted,
        InRound,
        InBetweenRounds,
        Ended
    }
    private SimState mState = SimState.NotYetStarted;

    public int _NumberOfFood = 20;
    public int _NumberOfLife = 4;

    public GameObject _Ground;
    public GameObject _Barrier;

    public GameObject _LifePrefab;
    public GameObject _FoodPrefab;

    public readonly static int STEPS_PER_SEC = 50;
    private int mStep = 0;

    List<ArtificialLife.Properties> mNextGen = null;

    private int mRound = 0;

    public float _TimeFactor = 1;


    public float _BreakBeforeRound = 0.1f;
    public float _BreakAfterRound = 2;

    

    // Start is called before the first frame update
    void Start()
    {
        mNextGen = new List<ArtificialLife.Properties>();
        mNextGen.Add(new ArtificialLife.Properties(2, 2));
        mNextGen.Add(new ArtificialLife.Properties(2, 2));
        mNextGen.Add(new ArtificialLife.Properties(2, 2));
        mNextGen.Add(new ArtificialLife.Properties(2, 2));

        Physics.autoSimulation = false;
        MakeBarrier();
        PrepareNextRound();
        StartRound();
    }
    private float mPhysicsTimer;
    void Update()
    {
        if (_TimeFactor < 0)
            _TimeFactor = 0;

        if (_TimeFactor == 0)
            return;

        DateTime start = DateTime.Now;

        if (mState == SimState.InRound)
        {
            mPhysicsTimer += Time.deltaTime * _TimeFactor;

            // Catch up with the game time.
            // Advance the physics simulation in portions of Time.fixedDeltaTime
            // Note that generally, we don't want to pass variable delta to Simulate as that leads to unstable results.
            while (mPhysicsTimer >= Time.fixedDeltaTime)
            {
                mPhysicsTimer -= Time.fixedDeltaTime;
                SimulationStep();
                Physics.SyncTransforms();
                Physics.Simulate(Time.fixedDeltaTime);

                //skip if we take so long that the FPS drop below 20
                //and skip if the round ended anyway
                if( (DateTime.Now - start).TotalSeconds  > 0.05 || mState != SimState.InRound)
                {
                    mPhysicsTimer = 0;
                    break;
                }
            }
        }
    }

    private void SimulationStep()
    {
        ArtificialLife[] als = GetComponentsInChildren<ArtificialLife>();
        foreach(var v in als)
        {
            v.SimulationStep(mStep);
        }
        if (mStep % STEPS_PER_SEC == 1) 
        {
            Food[] foods = GetComponentsInChildren<Food>();
            if(foods.Length == 0)
            {
                Debug.Log("Ending round. Everything is eaten");
                EndRound();
                return;
            }

            int napping = 0;
            foreach(var v in als)
            {
                if(v.State == ArtificialLife.AlState.Napping)
                {
                    napping++;
                }
            }

            if(als.Length == napping) {

                Debug.Log("Ending round. Everyone is napping.");
                EndRound();
                return;
            }


            if(mStep > STEPS_PER_SEC * 60)
            {
                Debug.Log("Ending round due to timeout.");
                EndRound();
                return;
            }
        }
        mStep++;
    }

    private void MakeBarrier()
    {
        float circ = Mathf.PI * 2 * RADIUS;
        int sphere_count = Mathf.CeilToInt(circ);
        float angle_step = Mathf.PI * 2 / sphere_count;
        for (int i = 0; i < sphere_count; i++)
        {
            float own_angle = angle_step * i;

            Vector3 pos = new Vector3();
            pos.x = Mathf.Sin(own_angle);
            pos.z = Mathf.Cos(own_angle);

            pos = pos * RADIUS;

            pos.y = 0.5f;

            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = pos;
            sphere.transform.localScale = new Vector3(2, 2, 2);
            sphere.transform.parent = _Barrier.transform;
        }
    }

    private void SpawnFood()
    {
        for(int i = 0; i < _NumberOfFood; i++)
        {
            Vector2 pos = UnityEngine.Random.insideUnitCircle * (RADIUS - 2);

            GameObject food = Instantiate(_FoodPrefab);
            food.transform.position = new Vector3(pos.x, 2, pos.y);
            food.transform.parent = this.transform;
            food.transform.name = "Food_" + i;
        }
    }

    private void SpawnLife()
    {
        int num = _NumberOfLife;
        if (mNextGen != null)
        {
            num = mNextGen.Count;
        }
        float angle_step = Mathf.PI * 2 / num;

        for (int i = 0; i < num; i++)
        {
            float own_angle = angle_step * i;

            Vector3 pos = new Vector3();
            pos.x = Mathf.Sin(own_angle);
            pos.z = Mathf.Cos(own_angle);

            pos = pos * (RADIUS - 2);

            pos.y = 0.5f;

            GameObject life = Instantiate(_LifePrefab);
            life.transform.position = pos;
            life.transform.parent = this.transform;
            life.transform.name = "Life_" + i;
            //if we already have population info then
            //override the defaults
            if(mNextGen != null)
            {
                life.GetComponent<ArtificialLife>().SetProperties(mNextGen[i]);

            }
        }
    }


    void PrepareNextRound()
    {
        SpawnFood();
        SpawnLife();
        //needed otherwise rigidbody.position isn't updated yet to the position we placed the objects!
        Physics.SyncTransforms();
    }
    private void StartRound()
    {
        mRound++;
        Debug.Log("Start round " + mRound);
        mStep = 0;
        mState = SimState.InRound;
    }

    /// <summary>
    /// Ends the round and does all the mutating for the next round.
    /// 
    /// </summary>
    private void EndRound()
    {
        mState = SimState.InBetweenRounds;
        mNextGen = new List<ArtificialLife.Properties>();


        ArtificialLife[] als = GetComponentsInChildren<ArtificialLife>();
        foreach (var v in als)
        {
            if(v.Eaten == 1)
            {
                mNextGen.Add(v.properties);
            }else if (v.Eaten >= 2)
            {
                mNextGen.Add(v.properties);
                mNextGen.Add(Mutate(v.properties));
            }
        }

        if (mNextGen.Count == 0)
        {
            Debug.LogWarning("They all died! :(");
            mState = SimState.Ended;
        }
        else
        {
            StartCoroutine(CoroutinePrepareRound());
        }
    }

    private void Cleanup()
    {
        Food[] foods = GetComponentsInChildren<Food>();
        foreach (var v in foods)
        {
            Destroy(v.gameObject);
        }
        ArtificialLife[] als = GetComponentsInChildren<ArtificialLife>();
        foreach (var v in als)
        {
            Destroy(v.gameObject);
        }
        Physics.SyncTransforms();
    }

    ArtificialLife.Properties Mutate(ArtificialLife.Properties props)
    {

        ArtificialLife.Properties res = new ArtificialLife.Properties();
        res._MovementSpeed = props._MovementSpeed + (UnityEngine.Random.value - 0.5f) * 2.0f;
        res._ViewDistance = props._ViewDistance + (UnityEngine.Random.value - 0.5f) * 2.0f;

        return res;
    }



    private IEnumerator CoroutinePrepareRound()
    {
        //pause before cleaning up possily allowing the user
        //to inspect what happened in this round
        yield return new WaitForSecondsRealtime(_BreakBeforeRound);
        //removing all left overs of the last round
        Cleanup();
        //setup map for the next round
        PrepareNextRound();

        //again wait a little to allow the user to see the new
        //life 
        yield return new WaitForSecondsRealtime(_BreakAfterRound);
        StartRound();
    }

}

