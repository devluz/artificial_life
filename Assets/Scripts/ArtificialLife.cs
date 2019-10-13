using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtificialLife : MonoBehaviour
{
    private static bool VERBOSE_LOG = false;

    public Config.Genes properties = new Config.Genes();

    public Config.Status status = null;





    private Rigidbody mRb;
    public Rigidbody rb
    {
        get
        {
            return mRb;
        }
    }


    public int Eaten
    {
        get
        {
            return status._Eaten;
        }
    }

    public enum AlState
    {
        LookingForFood,
        MovingToFood,
        Napping,
        Dead
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

    private void Awake()
    {
        mRb = GetComponent<Rigidbody>();
        status = properties.CreateStatus();
    }

    public void SetProperties(Config.Genes props)
    {
        properties = props;
        status = properties.CreateStatus();
        UpdateColor();
    }

    private void UpdateColor()
    {
        var renderer = this.gameObject.GetComponent<MeshRenderer>();

        renderer.material.color = properties.GetColor();
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
        float distance = Config.OnMove(this.status, this.properties, mRandomDir);

        //movement calculation and updating unity physics

        Vector3 curPos = this.rb.position;
        Vector3 movement = dir * distance;
        Vector3 movePos = curPos + movement;
        rb.MovePosition(movePos);
    }

    public void SimulationStep(int step)
    {
        if(step % Simulation.STEPS_PER_SEC == 0)
        {
            mRandomDir = GetRandomDir();
        }



        if(mState == AlState.LookingForFood)
        {
            //move into random direction
            Move(mRandomDir);

            //Check if we find any fod
            Collider[] res = Physics.OverlapSphere(this.mRb.position, properties._ViewDistance, Food.GetLayerMask());
            if(res != null && res.Length > 0)
            {
                //sort based on distance
                Array.Sort(res, (Collider a, Collider b) => {
                    float dista = Vector3.Distance(this.mRb.position, a.attachedRigidbody.position);
                    float distb = Vector3.Distance(this.mRb.position, b.attachedRigidbody.position);

                    if (distb == dista)
                        return 0;
                    else if (distb < dista)
                        return 1;
                    else
                        return -1;
                });
                //target closest food we find. Unity doesn't remove food immediately that is eaten
                //so we also have to make sure we don't target food that was already eaten
                foreach(var c in res)
                {
                    Food f = c.GetComponent<Food>();
                    if(f.IsEaten == false)
                    {
                        //found food!
                        mState = AlState.MovingToFood;
                        mTarget = res[0].GetComponent<Food>();
                        if (VERBOSE_LOG)
                        {
                            float distance = Vector3.Distance(mTarget.rb.position, this.mRb.position);
                            Debug.Log(this.mRb.position + " found food at " + mTarget.rb.position + "  " + distance);
                        }
                    }

                }
            }


        }
        else if(mState == AlState.MovingToFood)
        {
            if(mTarget != null && mTarget.IsEaten == false)
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



        if (this.mState != AlState.Dead)
        {

            //it fell off the platform -> dead
            if (this.mRb.position.y < -10)
            {
                this.mState = AlState.Dead;
            }

            //died due to a change in status
            if (status.IsAlive == false)
            {
                this.mState = AlState.Dead;
            }

            if(this.mState == AlState.Dead)
            {
                this.mRb.constraints = RigidbodyConstraints.None;
                this.mRb.AddRelativeForce(new Vector3(0, 0, 1), ForceMode.Force);
            }
        }
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(mState != AlState.MovingToFood && mState != AlState.LookingForFood)
        {
            //ignore the collision if the life form doesn't actively wants to eat
            return;
        }

        //check if collusion object still exists
        if(collision != null && collision.gameObject != null) 
        {
            //is it food?
            Food f = collision.gameObject.GetComponent<Food>();
            if(f != null && f.IsEaten == false)
            {
                if(status._Eaten < Config.Status.MAX_FOOD)
                {
                    if(VERBOSE_LOG)
                        Debug.Log(this.gameObject.name + " ate " + f.name);

                    //Using Destroy alone will make the results undeterministic as it
                    //depends on the CPU time / FPS when the object will actually be deleted
                    f.Eat();
                    f.gameObject.SetActive(false);
                    Destroy(f.gameObject);
                    status._Eaten++;

                    if(status._Eaten == Config.Status.MAX_FOOD)
                    {
                        mState = AlState.Napping;
                    }
                }
            }
        }
    }
}
