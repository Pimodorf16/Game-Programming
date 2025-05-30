using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController2D controller;

    public Animator animator;
    
    public InputActionReference move;
    public InputActionReference jump;

    public Vector2 moveDirection;
    public float moveSpeed = 40f;
    private float movement = 0f;
    private bool jumping = false;
    private bool onAir = false;
    public bool startOnAir = false;
    public bool landing = false;

    // Update is called once per frame
    void Update()
    {
        moveDirection = move.action.ReadValue<Vector2>();
        movement = moveDirection.x * moveSpeed;

        animator.SetFloat("Speed", Mathf.Abs(movement));
    }

    private void FixedUpdate()
    {
        controller.Move(movement * Time.fixedDeltaTime, false, jumping, landing);
        jumping = false;
    }

    private void OnEnable()
    {
        jump.action.started += Jump;
    }

    private void OnDisable()
    {
        jump.action.started -= Jump;
    }

    private void Jump(InputAction.CallbackContext obj)
    {
        jumping = true;
        landing = false;
        animator.SetBool("IsJumping", true);
        StartCoroutine(OnTheAir());
    }

    public void OnLanding()
    {
        if(onAir == true)
        {
            onAir = false;
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsLanding", true);
            StartCoroutine(Landing());
        }
    }

    public void NotOnAir()
    {
        startOnAir = false;
    }
    
    public void StartOnTheAir()
    {
        if(startOnAir == false)
        {
            StartCoroutine(OnTheAir());
            startOnAir = true;
        }
        
    }

    IEnumerator Landing()
    {
        landing = true;
        yield return new WaitForSeconds(0.2f);
        animator.SetBool("IsLanding", false);
        yield return new WaitForSeconds(0.5f);
        landing = false;
    }

    IEnumerator OnTheAir()
    {
        yield return new WaitForSeconds(0.2f);
        onAir = true;
    }
}
