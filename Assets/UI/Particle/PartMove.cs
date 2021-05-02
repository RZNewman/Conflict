using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartMove : MonoBehaviour
{
    public Vector3 movement;

    Vector3 start;
    float birth;
    // Start is called before the first frame update
    void Start()
    {
        start = transform.localPosition;
        birth = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = Vector3.Lerp(start, start + movement, (Time.time - birth) / GameConstants.clientViewpiplelineActionTime);
    }
}
