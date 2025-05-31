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

        if (landing == true)
        {
            if (Mathf.Abs(movement) > 0.01f)
            {
                StopCoroutine(Landing1());
                StartCoroutine(Landing2());
            }
        }
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
            landing = true;
            if(Mathf.Abs(movement) > 0.01f)
            {
                StartCoroutine(Landing2());
            }
            else
            {
                StartCoroutine(Landing1());
            }
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

    IEnumerator Landing2()
    {
        yield return new WaitForSeconds(0.1f);
        animator.SetBool("IsLanding", false);
        landing = false;
    }

    IEnumerator Landing1()
    {
        yield return new WaitForSeconds(0.4f);
        animator.SetBool("IsLanding", false);
        landing = false;
    }

    IEnumerator OnTheAir()
    {
        yield return new WaitForSeconds(0.2f);
        onAir = true;
    }
}
