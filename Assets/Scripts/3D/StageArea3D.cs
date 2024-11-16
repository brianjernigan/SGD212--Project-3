using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageArea3D : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Card"))
        {
            Debug.Log("Entering Stage");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Card"))
        {
            Debug.Log("Within Stage");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Card"))
        {
            Debug.Log("Exiting Stage");
        }
    }
}
