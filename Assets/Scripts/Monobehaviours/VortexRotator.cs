using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VortexRotator : MonoBehaviour
{
    private const float RotationSpeed = 180f;

    private void Update()
    {
        var rotation = RotationSpeed * Time.deltaTime;
        transform.Rotate(0f, rotation, 0f);
    }
}
