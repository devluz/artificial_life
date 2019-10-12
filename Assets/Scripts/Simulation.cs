using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulation : MonoBehaviour
{
    public static readonly float RADIUS = 20;


    public int _NumberOfFood = 20;
    public int _NumberOfLife = 4;

    public GameObject _Ground;
    public GameObject _Barrier;

    public GameObject _LifePrefab;
    public GameObject _FoodPrefab;

    // Start is called before the first frame update
    void Start()
    {
        MakeBarrier();
        SpawnFood();
        SpawnLife();
        //needed otherwise rigidbody.position isn't updated yet to the position we placed the objects!
        Physics.SyncTransforms();
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
            Vector2 pos = Random.insideUnitCircle * (RADIUS - 2);

            GameObject food = Instantiate(_FoodPrefab);
            food.transform.position = new Vector3(pos.x, 2, pos.y);
            food.transform.parent = this.transform;
            food.transform.name = "Food_" + i;
        }
    }

    private void SpawnLife()
    {
        float circ = Mathf.PI * 2 * RADIUS;
        float angle_step = Mathf.PI * 2 / _NumberOfLife;
        for (int i = 0; i < _NumberOfLife; i++)
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
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
