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


    public static int GetLayerMask()
    {
        return LayerMask.GetMask("Food");
    }

    private void Start()
    {
        mRb = GetComponent<Rigidbody>();
    }
    
    private void Update()
    {
        
    }
}
