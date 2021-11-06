/*
	@author - Taufik Mansuri
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour, Interactable, ISavable
{
    // properties
    [SerializeField] string name;
    [SerializeField] Sprite sprite;
    [SerializeField] Dialogue dialogue;
    [SerializeField] Dialogue dialogAfterBattle;
    [SerializeField] GameObject exclamation;
    [SerializeField] GameObject fov;

    // local variables
    bool battleLost = false;
    Character character;

    public string Name { get => name; }
    public Sprite Sprite { get => sprite; }

    // Called when scene is loaded
    private void Awake()
    {
        character = GetComponent<Character>();
    }

    // Called when gameObject is initialized
    private void Start()
    {
        SetFovRotation(character.Animator.DefaultDirection);
    }

    // Called each frame
    private void Update()
    {   
        character.HandleUpdate();
    }
    
    // Interaction
    public void Interact (Transform initiator)
    {
        character.LookTowards(initiator.position);

        if (!battleLost)
        {
            StartCoroutine(DialogueManager.Instance.ShowDialogue( dialogue, () =>
            {
                AudioManager.i.PlayMusic(SoundLibrary.GetClipFromName("Trainer Battle"));
                StartCoroutine(GameController.Instance.StartTrainerBattle(this));
            }));
        }
        else
        {
            StartCoroutine(DialogueManager.Instance.ShowDialogue(dialogAfterBattle));
        }
    }

    // Trainer Battle trigger
    public IEnumerator TriggerTrainerBattle(PlayerController player)
    {
        //Show Exclamation
        exclamation.SetActive(true);
        yield return new WaitForSeconds(2f);
        exclamation.SetActive(false);

        //Walk toward The Player
        var diff = player.transform.position - transform.position;
        var moveVec = diff - diff.normalized;
        moveVec = new Vector2 (Mathf.Round(moveVec.x),Mathf.Round(moveVec.y));

        yield return character.Move(moveVec);

        //Show dialogue
        StartCoroutine(DialogueManager.Instance.ShowDialogue( dialogue, () =>
        {
            AudioManager.i.PlayMusic(SoundLibrary.GetClipFromName("Trainer Battle"), 1.5f);
            StartCoroutine(GameController.Instance.StartTrainerBattle(this));
        }));
    }

    // When trainer loses a battle
    public void BattleLost()
    {
        battleLost = true;
        fov.gameObject.SetActive(false);
    }

    // Rotate FOV in a direction
    public void SetFovRotation(FacingDirection dir)
    {
        float angle = 0f;
        if (dir == FacingDirection.Right)
        angle = 90f;
        else if (dir == FacingDirection.Up)
        angle = 180f;
        else if (dir == FacingDirection.Left)
        angle = 270f;
        
        fov.transform.eulerAngles = new Vector3(0f, 0f, angle);
    }

    // For saving
    public object CaptureState()
    {
        return battleLost;
    }

    // For loading 
    public void RestoreState(object state)
    {
        battleLost = (bool)state;
        if(battleLost)
            fov.gameObject.SetActive(false);
    }
}
