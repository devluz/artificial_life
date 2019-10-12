using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtificialLife : MonoBehaviour
{
    private static bool VERBOSE_LOG = false;

    [Serializable]
    public class Properties
    {
        public readonly static float MOVEMENT_SPEED_MAX = 20;
        public readonly static float VIEW_DISTANCE_MAX = 20;
        public float _MovementSpeed = 5;
        public float _ViewDistance = 5;

        public Properties(float speed, float view)
        {
            _MovementSpeed = speed;
            _ViewDistance = view;
        }

        public Properties()
        {

        }
    }
    public Properties properties = new Properties();
    private Rigidbody mRb;


    private int mEaten = 0;
    public int Eaten
    {
        get
        {
            return mEaten;
        }
    }

    public enum AlState
    {
        LookingForFood,
        MovingToFood,
        Napping
    }

    private AlState mState = AlState.LookingForFood;
    public AlState State
    {
        get
        {
            return mState;
        }
    }


    private Vector3 mRandomDir = new Vector3(0,0,0);


    private Food mTarget = null;

    private void Start()
    {
        mRb = GetComponent<Rigidbody>();
    }

    public void SetProperties(Properties props)
    {
        properties = props;
        UpdateColor();
    }

    private void UpdateColor()
    {
        var renderer = this.gameObject.GetComponent<MeshRenderer>();

        float speed = properties._MovementSpeed / Properties.MOVEMENT_SPEED_MAX;
        float view = properties._ViewDistance / Properties.VIEW_DISTANCE_MAX;



        renderer.material.color = new Color(speed, 0.5f, view);
    }

    private void Update()
    {
        
    }

    //Creates a random direction & avoids unstable numbers in the process
    private static Vector3 GetRandomDir()
    {
        Vector3 res;
        do
        {

            res = UnityEngine.Random.onUnitSphere;
            res.y = 0;

        } while (res.magnitude < 0.01f);
        res.Normalize();
        return res;
    }

    private void Move(Vector3 dir)
    {
        Vector3 curPos = mRb.position;

        Vector3 movePos = curPos + dir * properties._MovementSpeed * Time.fixedDeltaTime;
        
        mRb.MovePosition(movePos);
    }

    public void SimulationStep(int step)
    {
        if(step % Simulation.STEPS_PER_SEC == 0)
        {
            mRandomDir = GetRandomDir();
        }



        if(mState == AlState.LookingForFood)
        {
            Move(mRandomDir);

            var pos = this.transform.position;
            Collider[] res = Physics.OverlapSphere(this.mRb.position, properties._ViewDistance, Food.GetLayerMask());
            Array.Sort(res, (Collider a, Collider b) => {
                float dista = Vector3.Distance(this.mRb.position, a.transform.position);
                float distb = Vector3.Distance(this.mRb.position, b.transform.position);
                if (distb == dista)
                    return 0;
                else if (distb < dista)
                    return 1;
                else
                    return -1;
            });
            if(res != null && res.Length > 0)
            {
                mState = AlState.MovingToFood;
                mTarget = res[0].GetComponent<Food>();
                float distance = Vector3.Distance(mTarget.transform.position, this.mRb.position);
                if(VERBOSE_LOG)
                    Debug.Log(this.mRb.position + " found food at " + mTarget.transform.position + "  " + distance);
            }


        }
        else if(mState == AlState.MovingToFood)
        {
            if(mTarget != null)
            {
                Vector3 dir = mTarget.rb.position - mRb.position;
                dir.y = 0;
                dir.Normalize();
                Move(dir);
            }
            else
            {
                mState = AlState.LookingForFood;
            }
        }

        //special case. due to some troubles in the physics they can be pushed over the edge
        //or through it -> switch them to nap time
        if(this.mRb.position.y < -10)
        {
            this.mState = AlState.Napping;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //check if collusion object still exists
        if(collision != null && collision.gameObject != null) 
        {
            //is it food?
            Food f = collision.gameObject.GetComponent<Food>();
            if(f != null)
            {
                if(mEaten < 2)
                {
                    if(VERBOSE_LOG)
                        Debug.Log(this.gameObject.name + " ate " + f.name);
                    Destroy(f.gameObject);
                    mEaten++;

                    if(mEaten == 2)
                    {
                        mState = AlState.Napping;
                    }
                }
            }
        }
    }
}
