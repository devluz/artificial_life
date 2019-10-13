using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Configuration for simulation and the life forms
/// </summary>
public class Config
{

    /// <summary>
    /// Values that can mutate and change between individuals but remain constant during
    /// the life time of an induvidual
    /// </summary>
    [Serializable]
    public class Genes
    {
        public readonly static float MOVEMENT_SPEED_MAX = 10;
        public readonly static float VIEW_DISTANCE_MAX = 10;

        public float _MovementSpeed = 5;
        public float _ViewDistance = 5;

        public Genes(float speed, float view)
        {
            _MovementSpeed = speed;
            _ViewDistance = view;
        }

        public Genes()
        {

        }

        public Color GetColor()
        {
            float speed = _MovementSpeed / MOVEMENT_SPEED_MAX;
            float view = _ViewDistance / VIEW_DISTANCE_MAX;

            //make sure they stay within the 0-1 range
            speed = Mathf.Max(0, Mathf.Min(1, speed));
            view = Mathf.Max(0, Mathf.Min(1, view));

            Color c = new Color(speed, 0.5f, view);
            return c;
        }


        public Config.Genes Mutate()
        {

            Config.Genes res = new Config.Genes();
            res._MovementSpeed = _MovementSpeed + (UnityEngine.Random.value - 0.5f) * 2.0f;
            res._ViewDistance = _ViewDistance + (UnityEngine.Random.value - 0.5f) * 2.0f;

            return res;
        }

        /// <summary>
        /// Genes can change the status a life form is create with during the start of a round
        /// e.g. a gene might give it more or less energy.
        /// </summary>
        /// <returns></returns>
        public Status CreateStatus()
        {
            Status s = new Status();
            s._Eaten = 0;
            s._Energy = 1;
            return s;
        }
    }

    /// <summary>
    /// Stuff that changes during the lifetime
    /// </summary>
    [Serializable]
    public class Status
    {
        /// <summary>
        /// maximum amount of food they can eat. After that they stop moving and rest
        /// </summary>
        public static int MAX_FOOD = 2;

        public float _Energy = 1;

        public int _Eaten = 0;

        public bool IsAlive
        {
            get
            {
                return _Energy > 0;
            }
        }
    }


    /// <summary>
    /// Called every time a life form wants to move. Calculate the distance
    /// based on its status and genes and returns it.
    /// 
    /// Can be used to update change in status based on the movement and updates in general.
    /// 
    /// OnMove won't be called if the life form is dead or napping (finished eating)
    /// </summary>
    /// <param name="status"></param>
    /// <param name="genes"></param>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static float OnMove(Status status, Genes genes, Vector3 dir)
    {
        //reduce energy based on movement with quadratic cost
        status._Energy -= genes._MovementSpeed * genes._MovementSpeed * Time.fixedDeltaTime / 100;
        //reduce energy based on the view distance but with lower cost (linear)
        status._Energy -= genes._ViewDistance / 100 * Time.fixedDeltaTime;

        float distance = genes._MovementSpeed * Time.fixedDeltaTime;
        return distance;
    }



    /// <summary>
    /// This is called at the end of the round for each life form. 
    /// Return null or 0 elements if the genes die out or
    /// 1 or more genes for the offspring.
    /// 
    /// Return the same genes if you want your life form to continue without change in the next round
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="parentStatus"></param>
    /// <returns></returns>
    public static Genes[] CreateOffspring(Genes parent, Status parentStatus)
    {
        List<Genes> offspring = new List<Genes>();

        if (parentStatus._Eaten == 1)
        {
            //keep the original alife for another round 
            offspring.Add(parent);
        }
        else if (parentStatus._Eaten >= 2)
        {
            //keep original around
            offspring.Add(parent);

            // + 1 mutated offspring
            offspring.Add(parent.Mutate());
        }
        else
        {
            //life form hasn't eaten anything -> die out
        }

        return offspring.ToArray();
    }


}
