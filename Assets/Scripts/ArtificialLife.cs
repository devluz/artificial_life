using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtificialLife : MonoBehaviour
{
    public float _MovementSpeed = 1;
    public float _ViewDistance = 5;
    
    private Rigidbody mRb;


    private int mEaten = 0;

    enum State
    {
        LookingForFood,
        MovingToFood,
        Napping
    }

    private State mState = State.LookingForFood;

    /// <summary>
    /// Counts the physics steps used for easy timing of regular tasks
    /// </summary>
    private int mStep = 0;
    private readonly int STEPS_PER_SEC = 50;
    private Vector3 mRandomDir = new Vector3(0,0,0);


    private Food mTarget = null;

    private void Start()
    {
        mRb = GetComponent<Rigidbody>();
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

        Vector3 movePos = curPos + dir * _MovementSpeed * Time.fixedDeltaTime;
        
        mRb.MovePosition(movePos);
    }

    private void FixedUpdate()
    {
        if(mStep % STEPS_PER_SEC == 0)
        {
            mRandomDir = GetRandomDir();
        }



        if(mState == State.LookingForFood)
        {
            Move(mRandomDir);

            var pos = this.transform.position;
            Collider[] res = Physics.OverlapSphere(this.mRb.position, 20, Food.GetLayerMask());
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
                mState = State.MovingToFood;
                mTarget = res[0].GetComponent<Food>();
                float distance = Vector3.Distance(mTarget.transform.position, this.mRb.position);
                Debug.Log(this.mRb.position + " found food at " + mTarget.transform.position + "  " + distance);
            }


        }
        else if(mState == State.MovingToFood)
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
                mState = State.LookingForFood;
            }
        }

        mStep++;
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
                    Debug.Log(this.gameObject.name + " ate " + f.name);
                    Destroy(f.gameObject);
                    mEaten++;

                    if(mEaten == 2)
                    {
                        mState = State.Napping;
                    }
                }
            }
        }
    }
}
