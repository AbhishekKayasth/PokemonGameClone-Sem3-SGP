using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Public Enum for game states
public enum GameState { FreeRoam, Battle, Dialogue, Menu, PartyScreen, Cutscene, Paused }

public class GameController : MonoBehaviour
{
	// SerializedField var (Visible in Unity Editor)
	[SerializeField] PlayerController playerController;
	[SerializeField] BattleSystem battleSystem;
	[SerializeField] Camera worldCamera;
	[SerializeField] PartyScreen partyScreen;
	// Local variables
	GameState state;

	GameState stateBeforePause;

	//Getting information about current Scene
	public SceneDetails CurrentScene {get; private set; }

	//Getting information about previous state
	public SceneDetails PrevScene {get; private set; }

	MenuController menuController;
	TrainerController trainer;

	public static GameController Instance { get; private set;}

	private void Awake()
	{
		Instance = this;

		menuController = GetComponent<MenuController>();

		ConditionsDB.Init();
		PokemonDB.Init();
		MoveDB.Init();
	}

	private void Start()
	{
		battleSystem.OnBattleOver += EndBattle;

		partyScreen.Init();

		DialogueManager.Instance.OnShowDialog += () =>
		{
			state = GameState.Dialogue;
		};

		DialogueManager.Instance.OnCloseDialog += () =>
		{
			if(state == GameState.Dialogue)
				state = GameState.FreeRoam;
		};

		menuController.onMenuSelected += OnMenuSelected;

		menuController.onBack += () => 
		{
			state = GameState.FreeRoam;
		};
	}

	//To solve the bug which will cause the player to move continouly in another portal in weird way
	public void PauseGame(bool pause)
	{
		if(pause)
		{
			stateBeforePause = state;
			state = GameState.Paused;
		}
		else
		{
			state = stateBeforePause;
		}

	}
	
	// This method makes game state into battle state
	public void StartBattle()
	{
		state = GameState.Battle;
		battleSystem.gameObject.SetActive(true);
		worldCamera.gameObject.SetActive(false);

		var playerParty = playerController.GetComponent<PokemonParty>();
		var wildPokemon = CurrentScene.GetComponent<MapArea>().GetRandomWildPokemon();

		var wildPokemonCopy = new Pokemon (wildPokemon.Base, wildPokemon.Level);

		battleSystem.StartBattle(playerParty, wildPokemonCopy);
	}
	
	public void StartTrainerBattle(TrainerController trainer)
	{
		state = GameState.Battle;
		battleSystem.gameObject.SetActive(true);
		worldCamera.gameObject.SetActive(false);

		this.trainer = trainer;
		var playerParty = playerController.GetComponent<PokemonParty>();
		var trainerParty = trainer.GetComponent<PokemonParty>();

		battleSystem.StartTrainerBattle(playerParty, trainerParty);
	}

	public void OnEnterTrainersView(TrainerController trainer)
	{
		state = GameState.Cutscene;
		StartCoroutine(trainer.TriggerTrainerBattle(playerController));
	}

	// This method reverts game state back into free roam state
	void EndBattle(bool won)
	{
		if (trainer != null && won == true)
		{
			trainer.BattleLost();
			trainer=null;
		}

		state = GameState.FreeRoam;
		battleSystem.gameObject.SetActive(false);
		worldCamera.gameObject.SetActive(true);
	}
	private void Update()
	{
		// Check current state and handle updates according to it
		if (state == GameState.FreeRoam)
		{
			playerController.HandleUpdate();

			if(Input.GetKeyDown(KeyCode.Return))
			{
				menuController.OpenMenu();
				state = GameState.Menu;
			}
		}
		else if (state == GameState.Battle)
		{
			battleSystem.HandleUpdate();
		}
		else if (state == GameState.Dialogue)
		{
			DialogueManager.Instance.HandleUpdate();
		}
		else if( state == GameState.Menu)
		{
			menuController.HandleUpdate();
		}
		else if(state == GameState.PartyScreen)
		{
			Action onSelected = () => 
			{
				// TO-DO : summary screen
			};

			Action onBack = () =>
			{
				partyScreen.gameObject.SetActive(false);
				state = GameState.Menu;
			};

			partyScreen.HandleUpdate(onSelected, onBack);
		}
	}

	public void SetCurrentScene(SceneDetails currScene)
	{
		PrevScene = CurrentScene;
		CurrentScene = currScene;
	}

	void OnMenuSelected(int selectedItem)
	{
		if(selectedItem == 0)
		{
			// Pokemon
			partyScreen.gameObject.SetActive(true);
			partyScreen.SetPartyData(playerController.GetComponent<PokemonParty>().Pokemons);
			state = GameState.PartyScreen;
		}
		else if(selectedItem == 1)
		{
			// Bag
		}
		else if(selectedItem == 2)
		{
			// Save
			SavingSystem.i.Save("saveSlot1");
			menuController.CloseMenu();
			state = GameState.FreeRoam;
		}
		else if(selectedItem == 3)
		{
			// Load
			SavingSystem.i.Load("saveSlot1");
			menuController.CloseMenu();
			state = GameState.FreeRoam;
		}
	}
}
