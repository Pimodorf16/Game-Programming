using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class InteractionSystem : MonoBehaviour
{
    [Header("Detection Parameters")]

    public Transform detectionPoint;

    const float detectionRadius = 0.2f;

    public LayerMask detectionLayer;

    public GameObject detectedObject;

    [Header("Input")]
    public InputActionReference interact;

    [Header("Others")]
    public bool interactAllow = true;
    public bool inDialogue = false;
    public bool inSelection = false;
    public bool canSelect = false;
    public bool inspect = false;

    private void Start()
    {
        interactAllow = true;
    }

    private void OnEnable()
    {
        interact.action.started += Interact;
    }

    private void OnDisable()
    {
        interact.action.started -= Interact;
    }

    private void Interact(InputAction.CallbackContext obj)
    {
        if (DetectObject())
        {
            if (interactAllow == true)
            {
                detectedObject.GetComponent<InteractableObjects>().Interact();
            }else if(inDialogue == true)
            {
                if(inSelection == true)
                {
                    FindObjectOfType<DialogueManager>().SelectConfirm();
                }else if(inSelection == false)
                {
                    FindObjectOfType<DialogueManager>().DisplayNextSentence();
                }
            }
        }
    }

    public void Inspect()
    {
        if (DetectObject())
        {
            if (interactAllow == true)
            {
                detectedObject.GetComponent<InteractableObjects>().Interact();
            }
        }
    }

    bool DetectObject()
    {
        Collider2D obj = Physics2D.OverlapCircle(detectionPoint.position, detectionRadius, detectionLayer);

        if (obj == null)
        {
            detectedObject = null;
            return false;
        }
        else
        {
            detectedObject = obj.gameObject;
            return true;
        }

    }
}
