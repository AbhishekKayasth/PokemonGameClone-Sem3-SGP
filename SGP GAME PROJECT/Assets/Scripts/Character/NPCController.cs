/*
	@author - Taufik Mansuri
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  enum NPCState { Idle, Walking, Dialogue }

public class NPCController : MonoBehaviour, Interactable
{
    // Parameters
    [SerializeField] Dialogue dialogue;
    [SerializeField] List<Vector2> movementPattern;
    [SerializeField] float timeBetweenPattern;

    // local and cache variables
    NPCState state;

    float idleTimer = 0f;
    int currentPattern = 0;

    Character character;
    
    // Called when scene is loaded
    private void Awake() 
    {
        character = GetComponent<Character>();
    }

    // For Interaction
    public void Interact(Transform initiator)
    {
        if (state == NPCState.Idle)
        {
            state = NPCState.Dialogue;
            character.LookTowards(initiator.position);
        
            StartCoroutine(DialogueManager.Instance.ShowDialogue(dialogue, () => 
            {
                idleTimer = 0f;
                state = NPCState.Idle;
            }));
        }
    }

    // Called on each frame
    private void Update()
    {     
        if (state == NPCState.Idle)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer > timeBetweenPattern)
            {
                idleTimer = 0f;
                if (movementPattern.Count > 0)              
                StartCoroutine(Walk());
            }
        }
        character.HandleUpdate();
    }

    // Handles walks
    IEnumerator Walk()
    {
        state = NPCState.Walking;

        var oldPos = transform.position;

        yield return character.Move (movementPattern[currentPattern]);

        if (transform.position != oldPos)      
            currentPattern = (currentPattern + 1) % movementPattern.Count;

        state = NPCState.Idle;
    }
}