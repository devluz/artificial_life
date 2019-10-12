using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    private Rigidbody mRb;
    public Rigidbody rb
    {
        get
        {
            return mRb;
        }
    }


    private bool mIsEaten = false;
    public bool IsEaten
    {
        get
        {
            return mIsEaten;
        }
    }


    public void Eat()
    {
        mIsEaten = true;
    }


    public static int GetLayerMask()
    {
        return LayerMask.GetMask("Food");
    }

    private void Awake()
    {
        mRb = GetComponent<Rigidbody>();
    }
    
    private void Update()
    {
        
    }
}
