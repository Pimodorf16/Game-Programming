using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cinemachine;

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    public Animator dialogueBoxAnimator;
    public Animator nameBoxAnimator;
    public Animator portraitAnimator;

    public CinemachineVirtualCamera conversationCam;

    private Queue<string> sentences;
    
    // Start is called before the first frame update
    void Start()
    {
        sentences = new Queue<string>();
    }

    public void StartDialogue(Dialogue dialogue)
    {
        FindObjectOfType<InteractionSystem>().interactAllow = false;
        FindObjectOfType<InteractionSystem>().inDialogue = true;

        dialogueBoxAnimator.SetBool("IsOpen", true);
        nameBoxAnimator.SetBool("IsOpen", true);
        portraitAnimator.SetBool("IsOpen", true);

        conversationCam.Priority = 30;

        nameText.text = dialogue.name;
        
        sentences.Clear();

        foreach(string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if(sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        foreach(char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return null;
        }
    }

    public void EndDialogue()
    {
        //Debug.Log("EndDialogue");
        dialogueBoxAnimator.SetBool("IsOpen", false);
        nameBoxAnimator.SetBool("IsOpen", false);
        portraitAnimator.SetBool("IsOpen", false);
        conversationCam.Priority = 1;
        FindObjectOfType<InteractionSystem>().inDialogue = false;
        FindObjectOfType<InteractionSystem>().interactAllow = true;
    }

}
