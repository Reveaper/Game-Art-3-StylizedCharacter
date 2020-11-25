using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    [SerializeField] private DirectionalMotion _motion;

    [SerializeField] private CameraBehaviour _cameraPivot;

    private Vector2 _previousJoystickInput;
    private float _inputLerpSpeed = 0.25f;

    private void Update()
    {
        ButtonInputs();
    }

    private void FixedUpdate()
    {
        MoveMotion();
        RotateCamera();
    }

    private void ButtonInputs()
    {
        if (Input.GetButtonDown("Fly"))
            _motion.Fly();

        if (Input.GetButtonDown("Idle1"))
            _motion.Idle1();


        if (Input.GetButtonDown("Idle2"))
            _motion.Idle2();
    }

    private void MoveMotion()
    {
        Vector2 joystickInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        Vector2 inputLerped = Vector2.Lerp(_previousJoystickInput, joystickInput, _inputLerpSpeed);




        Vector3 direction = Vector3.ProjectOnPlane(_cameraPivot.transform.forward, Vector3.up) * inputLerped.y + Vector3.ProjectOnPlane(_cameraPivot.transform.right, Vector3.up) * inputLerped.x;
        _motion.Move(direction, inputLerped);

        _previousJoystickInput = inputLerped;
    }

    private void RotateCamera()
    {
        float h = Input.GetAxis("HorizontalRightJoystick");

        Vector3 euler = new Vector3(0, h, 0);
        _cameraPivot.Rotate(euler);

        float v = Input.GetAxis("VerticalRightJoystick");
        Vector3 eulerV = new Vector3(v, 0, 0);
        _cameraPivot.RotateVertical(eulerV);
    }
}
