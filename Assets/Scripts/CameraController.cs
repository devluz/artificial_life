using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private float mRotX = -30f;
    private float mRotY = 0f;
    private float mDistance = 40f;

    private float speed = 500f;
    private float zoomspeed = -500f;
    public Transform target;



    private void Start()
    {


    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            mRotX += Input.GetAxis("Mouse Y") * speed * Time.deltaTime;
            mRotY += Input.GetAxis("Mouse X") * speed * Time.deltaTime;
            mRotX = Mathf.Min(89.9f, Mathf.Max(-89.9f, mRotX));
        }
        mDistance += Input.GetAxis("Mouse ScrollWheel") * zoomspeed * Time.deltaTime;
        transform.position = target.position + Quaternion.Euler(mRotX, mRotY, 0f) * (mDistance * Vector3.forward);
        transform.LookAt(target.position, Vector3.up);
    }
}
