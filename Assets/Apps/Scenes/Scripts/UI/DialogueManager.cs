using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cinemachine;
using UnityEngine.Events;

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    public Animator dialogueBoxAnimator;
    public Animator nameBoxAnimator;
    public Animator portraitAnimator;
    public Animator yesBoxAnimator;
    public Animator noBoxAnimator;

    public CinemachineVirtualCamera conversationCam;

    [Header("Events")]
    public UnityEvent yesEvent;
    public UnityEvent noEvent;

    private Queue<string> sentences;

    Dialogue[] dialogue;

    private bool selection;
    private bool end = false;

    private int currentDialogue = 0;

    public bool yesNo = true;
    
    // Start is called before the first frame update
    void Start()
    {
        sentences = new Queue<string>();
    }

    public void StartDialogue(Dialogue[] dialogue)
    {
        FindObjectOfType<InteractionSystem>().interactAllow = false;
        FindObjectOfType<InteractionSystem>().inDialogue = true;

        this.dialogue = dialogue;

        dialogueBoxAnimator.SetBool("IsOpen", true);
        nameBoxAnimator.SetBool("IsOpen", true);
        portraitAnimator.SetBool("IsOpen", true);

        conversationCam.Priority = 30;

        DisplayNextDialogue(dialogue[currentDialogue]);
    }

    public void DisplayNextSentence()
    {
        if(sentences.Count == 1)
        {
            Debug.Log("Sentence Count = 1");
            if (selection == true)
            {
                FindObjectOfType<InteractionSystem>().inSelection = true;

                yesBoxAnimator.SetBool("IsOpen", true);
                noBoxAnimator.SetBool("IsOpen", true);

                SelectAnim();
                StartCoroutine(WaitSelect());
            }
        }
        else if(sentences.Count == 0) 
        {
            Debug.Log("Sentence Count = 0");
            if(end == true)
            {
                EndDialogue();
                Debug.Log("End");
                return;
            }
            else
            {
                currentDialogue = +1;

                DisplayNextDialogue(dialogue[currentDialogue]);
                Debug.Log("Next Dialogue");
                return;
            }
        }

        Debug.Log("Continue Sentence");
        string sentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
        Debug.Log("displaying: " + sentence);

        
        
    }

    public void DisplayNextDialogue(Dialogue d)
    {
        nameText.text = d.name;
        selection = d.selection;
        end = d.end;
        yesEvent = d.yesEvent;
        noEvent = d.noEvent;

        sentences.Clear();

        foreach (string sentence in d.sentences)
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
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
        noBoxAnimator.SetBool("OnHover", false);
        yesBoxAnimator.SetBool("OnHover", false);
        dialogueBoxAnimator.SetBool("IsOpen", false);
        nameBoxAnimator.SetBool("IsOpen", false);
        portraitAnimator.SetBool("IsOpen", false);
        yesBoxAnimator.SetBool("IsOpen", false);
        noBoxAnimator.SetBool("IsOpen", false);
        yesBoxAnimator.ResetTrigger("Selected");
        noBoxAnimator.ResetTrigger("Selected");
        conversationCam.Priority = 1;
        currentDialogue = 0;
        FindObjectOfType<InteractionSystem>().inDialogue = false;
        FindObjectOfType<InteractionSystem>().interactAllow = true;
    }

    public void SelectLeft()
    {
        if(yesNo == true)
        {
            yesNo = true;
        }else if(yesNo == false)
        {
            yesNo = true;
        }

        SelectAnim();
    }

    public void SelectRight()
    {
        if (yesNo == true)
        {
            yesNo = false;
        }
        else if (yesNo == false)
        {
            yesNo = false;
        }

        SelectAnim();
    }

    public void SelectAnim()
    {
        if (yesNo == true)
        {
            noBoxAnimator.SetBool("OnHover", false);
            yesBoxAnimator.SetBool("OnHover", true);
        }
        else if (yesNo == false)
        {
            yesBoxAnimator.SetBool("OnHover", false);
            noBoxAnimator.SetBool("OnHover", true);
        }
    }

    public void SelectConfirm()
    {
        if(yesNo == true)
        {
            yesBoxAnimator.SetTrigger("Selected");
            dialogue[currentDialogue].yesEvent.Invoke();
        }
        else if(yesNo == false)
        {
            noBoxAnimator.SetTrigger("Selected");
            dialogue[currentDialogue].noEvent.Invoke();
        }

        FindObjectOfType<InteractionSystem>().inSelection = false;
        FindAnyObjectByType<InteractionSystem>().canSelect = false;
    }

    public void SkipDialogue(int i)
    {
        currentDialogue =+ i;

        if (end == true)
        {
            EndDialogue();
            Debug.Log("End");
            return;
        }
        else
        {
            StartCoroutine(CloseYesNo());
            DisplayNextDialogue(dialogue[currentDialogue]);

            Debug.Log("Next Dialogue Close Yes No");
            return;
        }
    }

    IEnumerator CloseYesNo()
    {
        yield return new WaitForSeconds(1.5f);

        noBoxAnimator.SetBool("OnHover", false);
        yesBoxAnimator.SetBool("OnHover", false);
        yesBoxAnimator.SetBool("IsOpen", false);
        noBoxAnimator.SetBool("IsOpen", false);
        yesBoxAnimator.ResetTrigger("Selected");
        noBoxAnimator.ResetTrigger("Selected");
    }

    IEnumerator WaitSelect()
    {
        yield return new WaitForSeconds(0.6f);
        FindAnyObjectByType<InteractionSystem>().canSelect = true;
    }
}
