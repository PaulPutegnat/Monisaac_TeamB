using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JeffsBehavior : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        Debug.Log("Picked up!");
    }
}
