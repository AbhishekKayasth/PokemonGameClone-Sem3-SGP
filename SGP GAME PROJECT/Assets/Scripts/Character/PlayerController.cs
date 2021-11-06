/*
	@author - SamirAli Mukhi
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour, ISavable
{
	// Parameters
	[SerializeField] string name;
    [SerializeField] Sprite sprite;
	
	private Vector2 input;
	private Character character;

	public string Name { get => name; }
	public Sprite Sprite { get => sprite; }
	public Character Character => character;
	
	// Called when scene is loaded
	private void Awake()
	{
		character = GetComponent<Character>();
	}

	// Handles input and updates
	public void HandleUpdate()
	{
		if (!character.IsMoving)
		{
			input.x = Input.GetAxisRaw("Horizontal");
			input.y = Input.GetAxisRaw("Vertical");

			//Remove diagonal movement
			if (input.x != 0) input.y = 0;
			// If there is non zero input move player 
			if (input != Vector2.zero)
			{
				StartCoroutine(character.Move(input, OnMoveOver));
			}
		}

		character.HandleUpdate();

		// Interact if there is anything interactable 
		if (Input.GetKeyDown(KeyCode.Z))
		{
			Interact();
		}
	}

	//For interacting with NPCs
	void Interact()
	{
		var facingDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
		var interactPos = transform.position + facingDir;

		var collider = Physics2D.OverlapCircle(interactPos , 0.3f , GameLayers.i.InteractableLayer);
		if (collider != null)
		{
			collider.GetComponent<Interactable>()?.Interact(transform);
		}
	}

	// If there is an event trigger (Trainer) triggers it
	private void OnMoveOver() 
	{
		var colliders = (Physics2D.OverlapCircleAll(transform.position - new Vector3(0, character.OffsetY), 0.2f, GameLayers.i.TrigerrableLayers));

		foreach(var collider in colliders)
		{
			var trigerrable = collider.GetComponent<IPlayerTriggerable>();
			if(trigerrable != null)
			{
				trigerrable.OnPlayerTriggered(this);
				break;
			}
		}
	}

	// Saves current state 
    public object CaptureState()
    {
		var saveData = new PlayerSaveData()
		{
			position = new float[] { transform.position.x, transform.position.y },
			pokemons = GetComponent<PokemonParty>().Pokemons.Select(p => p.GetSaveData()).ToList()
		};
		
		return saveData;
    }

	// Loads save data
    public void RestoreState(object state)
    {
		var saveData = (PlayerSaveData)state;

		// Restore position
		transform.position = new Vector3(saveData.position[0], saveData.position[1]);

		// Restore Party
		GetComponent<PokemonParty>().Pokemons = saveData.pokemons.Select( s => new Pokemon(s)).ToList();
    }
}

// This class defines properties for saving
[Serializable]
public class PlayerSaveData 
{
	public float[] position;
	public List<PokemonSaveData> pokemons;
}