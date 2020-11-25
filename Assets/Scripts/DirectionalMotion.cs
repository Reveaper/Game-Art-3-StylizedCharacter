using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class DirectionalMotion : MonoBehaviour
{
    private Animator _animator;
    private Rigidbody _rigidBody;
    private CapsuleCollider _collider;

    [SerializeField] private CameraBehaviour _cameraBehaviour;

    [SerializeField] private LayerMask _groundMask;

    [SerializeField] private float _movementSpeed = 1f;
    [SerializeField] private float _rotationSpeed = 10f;

    [Header("Flying")]
    [SerializeField] private float _flyInputStrength = 0.25f;
    [SerializeField] private float _flyBankStrength = 50f;
    [SerializeField] private float _flyingBaseSpeed = 6f;
    private float _flyBankAmount;
    private float _flyVerticalClamp = 0.8f;

    public bool FlyModeEntry;


    private void Start()
    {
        _animator = this.GetComponent<Animator>();
        _rigidBody = this.GetComponent<Rigidbody>();
        _collider = this.GetComponent<CapsuleCollider>();
    }

    private void SmoothRotateTowards(Vector3 targetForward)
    {
        Quaternion toRotation = Quaternion.LookRotation(targetForward);
        Quaternion newRotation = Quaternion.Lerp(transform.rotation, toRotation, _rotationSpeed * Time.deltaTime);
        transform.rotation = newRotation;
    }

    #region FixedUpdate

    private void FixedUpdate()
    {
        bool isGrounded = Physics.Raycast(this.transform.position + Vector3.up * 0.1f, Vector3.down, 0.3f, _groundMask);
        _animator.SetBool("IsGrounded", isGrounded);

        //_animator.SetFloat("Forward", 0);

        if(_animator.GetBool("Flying"))
            HandleFlying();

        if(FlyModeEntry)
            _rigidBody.AddForce(-Physics.gravity, ForceMode.Acceleration);
    }

    private void HandleFlying()
    {
        float heightStrength = Vector3.Dot(this.transform.forward, Vector3.down);

        _cameraBehaviour.FlyDirectionHeightOffset = -heightStrength * 1.5f;

        heightStrength += 1;//flying up == 0 / flying down == 2
        heightStrength = Mathf.Max(0.5f, heightStrength);

        Vector3 velocity = this.transform.forward * _flyingBaseSpeed;
        velocity.y *= heightStrength;

        _rigidBody.velocity = velocity;
        _rigidBody.AddForce(-Physics.gravity, ForceMode.Acceleration);

        if(_animator.GetBool("CanLand"))
            FlyGroundDetection();
    }

    private void FlyGroundDetection()
    {
        //detect ground
        RaycastHit rayHit;
        bool hasGroundBeenDetected = Physics.Raycast(this.transform.position, this.transform.forward, out rayHit, 3f, _groundMask);
        bool isGroundFacingUp = Vector3.Dot(Vector3.up, rayHit.normal) > 0.8f;

        if (hasGroundBeenDetected && isGroundFacingUp)
        {
            _animator.SetTrigger("Land");
            _animator.SetBool("Flying", false);
            ResetRotationWithForward();
            _cameraBehaviour.FlyMode = false;
            _animator.SetBool("CanLand", false);
        }
    }

    #endregion FixedUpdate

    #region MovementInput
    public void Move(Vector3 direction, Vector2 joystickInput)
    {
        if (direction.sqrMagnitude > 0.001f)
        {
            if (_animator.GetBool("Flying"))
                HandleFlyingInput(joystickInput);
            else if (_animator.GetBool("IsGrounded") && !FlyModeEntry)
                SmoothRotateTowards(direction);

            float forwardMovement = direction.magnitude > 1 ? 1 : direction.magnitude;

            _animator.SetFloat("Forward", forwardMovement);
        }
        else
        {
            _animator.SetFloat("Forward", 0f);
        }
    }

    private void HandleFlyingInput(Vector2 joystickInput)
    {
        _animator.SetFloat("FlyingDive", joystickInput.y);

        float dotProduct = Vector3.Dot(this.transform.forward, Vector3.up);
        float verticalClamp = 0;

        if ((dotProduct <= _flyVerticalClamp && joystickInput.y > 0) || (dotProduct >= -_flyVerticalClamp && joystickInput.y < 0))
            verticalClamp = 1;

        Vector3 horizontal = Vector3.Scale(this.transform.right, new Vector3(1, 0, 1)) * joystickInput.x * _flyInputStrength;
        Vector3 vertical = this.transform.up * joystickInput.y * (_flyInputStrength * verticalClamp);
        Vector3 newForwardVector = this.transform.forward + horizontal + vertical;

        SmoothRotateTowards(newForwardVector);
        HandleFlyingBankRotation(joystickInput);
    }

    private void HandleFlyingBankRotation(Vector2 joystickInput)
    {
        _flyBankAmount = -joystickInput.x * _flyBankStrength;

        Vector3 eulerAngles = this.transform.localEulerAngles;
        eulerAngles.z = _flyBankAmount;
        this.transform.localEulerAngles = eulerAngles;
        /*
        Vector3 cameraEulerAngles = _cameraBehaviour.transform.localEulerAngles;
        cameraEulerAngles.z = _flyBankAmount / 10f;
        _cameraBehaviour.transform.localEulerAngles = cameraEulerAngles;*/
    }


    #endregion MovementInput

    private void OnAnimatorMove()
    {
        Vector3 velocity = _rigidBody.velocity;

        if (_animator.GetBool("IsGrounded"))
        {
            velocity = (_animator.deltaPosition * _movementSpeed) / Time.deltaTime;
            velocity.y = _rigidBody.velocity.y;
        }

        if(FlyModeEntry)
        {
            velocity = _animator.deltaPosition / Time.deltaTime;
        }

        _rigidBody.velocity = velocity;
        _rigidBody.angularVelocity = _animator.angularVelocity;
    }

    #region StateInitializers



    public void Fly()
    {
        if (!_animator.GetBool("Flying") && _animator.GetBool("IsGrounded") && _animator.GetFloat("Forward") > 0.5f && !FlyModeEntry)
        {
            _animator.SetTrigger("InitializeFly");
            _animator.SetBool("CanLand", false);
            _animator.ResetTrigger("Land");
            FlyModeEntry = true;
            _collider.enabled = false;
        }
        else if(_animator.GetBool("Flying"))
        {
            _animator.ResetTrigger("InitializeFly");
            _animator.SetTrigger("Land");
            _animator.SetBool("Flying", false);
            ResetRotationWithForward();
            _animator.SetBool("CanLand", true);
            _cameraBehaviour.FlyMode = false;
            FlyModeEntry = false;
        }
        /*
        if (!_animator.GetBool("Flying") && _animator.GetBool("IsGrounded"))
        {
            _animator.SetBool("Flying", true);
            _animator.ResetTrigger("Land");
            _cameraBehaviour.FlyMode = true;
            _animator.SetBool("CanLand", false);
        }
        else if (_animator.GetBool("Flying"))
        {
            _animator.SetBool("Flying", false);
            ResetRotationWithForward();
            _cameraBehaviour.FlyMode = false;
            _animator.SetBool("CanLand", true);
        }*/
    }

    private void ResetRotationWithForward()
    {

        this.transform.rotation = Quaternion.LookRotation(Vector3.Scale(this.transform.forward, new Vector3(1, 0, 1)), Vector3.up);
    }

    #endregion StateInitializers

    public void PickRandomIdleAnim()
    {
        /*
        if(_animator.GetFloat("Forward") < 0.01f)
        {
            _animator.ResetTrigger("Idle1");
            _animator.ResetTrigger("Idle2");
            _animator.ResetTrigger("Idle3");

            int chance = UnityEngine.Random.Range(0, 3);
            if (chance >= 1)
            {
                int randomNumber = UnityEngine.Random.Range(1, 4);
                string animParamName = "Idle" + randomNumber;
                _animator.SetTrigger(animParamName);
            }

        }*/
    }

    public void Idle1()
    {
        _animator.ResetTrigger("Idle1");
        _animator.ResetTrigger("Idle2");
        _animator.ResetTrigger("Idle3");
        _animator.SetTrigger("Idle1");
    }
    public void Idle2()
    {
        _animator.ResetTrigger("Idle1");
        _animator.ResetTrigger("Idle2");
        _animator.ResetTrigger("Idle3");
        _animator.SetTrigger("Idle2");
    }



    private void JumpMotion(float strength)
    {
        Vector3 velocity = _rigidBody.velocity;
        velocity.y = strength;
        _rigidBody.velocity = velocity;
    }

    public void EnableCollider()
    {
        _collider.enabled = true;
    }

    public void EnableFlyMode()
    {
        if(_animator.GetFloat("Forward") > 0.5f && FlyModeEntry)
        {
            _collider.enabled = true;
            JumpMotion(10f);
            FlyModeEntry = false;
            _animator.SetBool("Flying", true);
            _animator.ResetTrigger("Land");
            _cameraBehaviour.FlyMode = true;
            _animator.SetBool("CanLand", false);
        }

    }
}
