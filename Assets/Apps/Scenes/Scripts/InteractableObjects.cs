using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider2D))]
public class InteractableObjects : MonoBehaviour
{
    public enum InteractionType {NONE,Conversation,Inspect,Trigger}
    public InteractionType type;

    [Header("Events")]
    public UnityEvent conversationEvent;
    public UnityEvent inspectEvent;
    public UnityEvent triggerEvent;
    public UnityEvent customEvent;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
        gameObject.layer = 7;
    }

    public void Interact()
    {
        switch (type)
        {
            case InteractionType.Conversation:
                conversationEvent.Invoke();
                break;
            case InteractionType.Inspect:
                inspectEvent.Invoke();
                break;
            case InteractionType.Trigger:
                break;
            default:
                Debug.Log("Null Object");
                break;
        }
        customEvent.Invoke();
    }

    public void Conversation()
    {
        type = InteractionType.Conversation;
    }
    public void Inspect()
    {
        type = InteractionType.Inspect;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (type)
        {
            case InteractionType.Conversation:
                break;
            case InteractionType.Inspect:
                break;
            case InteractionType.Trigger:
                triggerEvent.Invoke();
                break;
            default:
                break;
        }
    }

    public void GotItem(GameObject item)
    {
        item.SetActive(false);
    }
}