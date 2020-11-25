using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _rotationSpeed;
    
    private Vector3 _baseOffset;

    private float _defaultHeightOffset = 0;
    private float _defaultSpeed = 4f;

    //Flying mode variables
    private bool _flyMode;
    private float _flyHeightOffset = -1.25f;
    private float _flyModeSpeed = 5f;

    public float FlyDirectionHeightOffset;

    public bool FlyMode { set { _flyMode = value; } }

    private void Start()
    {
        _baseOffset = _target.position - this.transform.position;
    }


    private void FixedUpdate()
    {
        float heightOffset = _flyMode ? _flyHeightOffset + FlyDirectionHeightOffset : 0;
        float speed = _flyMode ? _flyModeSpeed : _defaultSpeed;

        Vector3 finalTarget = _target.position - _baseOffset + Vector3.up * heightOffset;
        Vector3 delta = finalTarget - this.transform.position;

        this.transform.position = delta.sqrMagnitude > 0.0001f ? this.transform.position + delta * speed * Time.fixedDeltaTime : finalTarget;


    }

    public void Rotate(Vector3 euler)
    {
        this.transform.Rotate(euler * _rotationSpeed * Time.fixedDeltaTime);
        this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, this.transform.eulerAngles.y, 0);
    }


    private float _rotationVertical = 0f;

    public void RotateVertical(Vector3 euler)
    {
        if (_rotationVertical < 70f && _rotationVertical > -70f)
        {
            _rotationVertical += euler.y * _rotationSpeed * Time.deltaTime;
            this.transform.Rotate(euler * _rotationSpeed * Time.fixedDeltaTime);
        }

    }
}
