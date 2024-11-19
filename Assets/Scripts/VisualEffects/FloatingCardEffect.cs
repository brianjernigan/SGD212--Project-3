using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingCardEffect : MonoBehaviour
{
    private float _rotationAmplitude = 5f;
    private float _frequency = 1f;
    private float _minY = 25f;
    private float _maxY = 35f;
    private float _bobSpeed = 5f;
    private float _currentYDirection = 1f;
    
    private float _phaseOffset;

    private void Start()
    {
        _phaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        //RotateCard();
        //BobCard();
    }

    private void RotateCard()
    {
        var rotationAngle = Mathf.Sin(Time.time * _frequency + _phaseOffset) * _rotationAmplitude;

        transform.rotation = Quaternion.Euler(90f, rotationAngle, 180f);
    }

    private void BobCard()
    {
        transform.position += new Vector3(0f, _currentYDirection * _bobSpeed * Time.deltaTime, 0f);

        if (transform.position.y >= _maxY)
        {
            _currentYDirection = -1f;
            transform.position = new Vector3(transform.position.x, _maxY, transform.position.z);
        } 
        else if (transform.position.y <= _minY)
        {
            _currentYDirection = 1f;
            transform.position = new Vector3(transform.position.x, _minY, transform.position.z);
        }
    }
}
