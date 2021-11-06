/*
	Module name - BattleSystem
	Module creation date - 04-Sep-2021
	@author - Group 07
*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;

// For defining different states 
public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, PartyScreen, AboutToUse, MoveToForget, BattleOver }
public enum BattleAction { Move, SwitchPokemon, UseItem, Run }

public class BattleSystem : MonoBehaviour
{
	[Header("Battle Units")]
	// References for battle units
	[SerializeField] BattleUnit playerUnit;
	[SerializeField] BattleUnit enemyUnit;

	[Header("References for UI elements")]
	// References for UI
	[SerializeField] BattleDialogueBox dialogueBox;
	[SerializeField] MoveSelectionUI moveSelectionUI;
	[SerializeField] PartyScreen partyScreen;

	[Header("For Trainer Battle")]
	// References for objects needed in trainer battle
	[SerializeField] Image playerImage;
	[SerializeField] Image trainerImage;
	[SerializeField] GameObject pokeballSprite;
	
	public event Action<bool> OnBattleOver; // Perform when battle is over

	// Local Variables
	
	// To keep track of battle states
	BattleState state;
	// for selection
	int currentAction;
	int currentMove;
	bool aboutToUseChoice = true;
	// To keep track of parties and wild Pokemon
	PokemonParty playerParty;
	PokemonParty trainerParty;
	// For trainer battles
	bool isTrainerBattle = false;
	PlayerController player;
	TrainerController trainer;
	Vector3 playerOG;
	Vector3 trainerOG;
	// For wild battles
	Pokemon wildPokemon;
	int escapeAttemps;

	MoveBase moveToLearn;

	// Starts a wild battle
	public void StartBattle(PokemonParty playerParty , Pokemon wildPokemon)
	{
		this.playerParty = playerParty;
		this.wildPokemon = wildPokemon;
		player = playerParty.GetComponent<PlayerController>();
		
		StartCoroutine(SetupBattle());
	}

	// Starts a trainer battle
	public void StartTrainerBattle(PokemonParty playerParty , PokemonParty trainerParty )
	{
		this.playerParty = playerParty;
		this.trainerParty = trainerParty;

		isTrainerBattle = true;
		player = playerParty.GetComponent<PlayerController>();
		trainer = trainerParty.GetComponent<TrainerController>(); 

		StartCoroutine(SetupBattle());
	}

	// Set up battle scene
	public IEnumerator SetupBattle()
	{
		playerUnit.Clear();
		enemyUnit.Clear();
		dialogueBox.EnableActionSelector(false);
		
		// if it is a wild battle
		if (!isTrainerBattle)
		{
			playerUnit.gameObject.SetActive(false);
			playerImage.gameObject.SetActive(true);
			enemyUnit.Setup(wildPokemon);
			playerImage.sprite = player.Sprite;
			playerOG = playerImage.transform.localPosition;
			yield return dialogueBox.TypeDialogue($"A wild {enemyUnit.Pokemon.Base.Name} appeared.");
			
			playerImage.transform.DOLocalMoveX(-1500, 1.8f);
			playerUnit.gameObject.SetActive(true);

			playerUnit.Setup(playerParty.GetHealthyPokemon());
			yield return dialogueBox.TypeDialogue($"Go {playerUnit.Pokemon.Base.Name}!");
			dialogueBox.SetMoveNames(playerUnit.Pokemon.Moves);
		}
		else // it is a trainer battle
		{
			playerUnit.gameObject.SetActive(false);
			enemyUnit.gameObject.SetActive(false);
			// trainer images
			playerImage.gameObject.SetActive(true);
			trainerImage.gameObject.SetActive(true);
			playerImage.sprite = player.Sprite;
			trainerImage.sprite = trainer.Sprite;

			trainerOG = new Vector3(trainerImage.transform.localPosition.x, trainerImage.transform.localPosition.y);
			playerOG = new Vector3(playerImage.transform.localPosition.x, playerImage.transform.localPosition.y);

			trainerImage.transform.localPosition = new Vector3(1500, trainerOG.y);
			playerImage.transform.localPosition = new Vector3(-1500, playerOG.y);
			
			var sequence = DOTween.Sequence();
			sequence.Append(trainerImage.transform.DOLocalMoveX(trainerOG.x, 2f));
			sequence.Join(playerImage.transform.DOLocalMoveX(playerOG.x, 2f));
			
			yield return dialogueBox.TypeDialogue($"{trainer.Name} would like to battle");
			yield return new WaitForSeconds(2f);

			sequence.Append(trainerImage.transform.DOLocalMoveX(1500, 1.8f));
			
			//Send out first pokemon on the Trainer
			enemyUnit.gameObject.SetActive(true);
			var enemyPokemon = trainerParty.GetHealthyPokemon();
			enemyUnit.Setup(enemyPokemon);
			yield return dialogueBox.TypeDialogue($"{trainer.Name} send out {enemyPokemon.Base.Name}");

			//Send out first pokemon from player party
			sequence.Append(playerImage.transform.DOLocalMoveX(-1500, 1.8f));
			playerUnit.gameObject.SetActive(true);
			var playerPokemon = playerParty.GetHealthyPokemon();
			playerUnit.Setup(playerPokemon);
			yield return dialogueBox.TypeDialogue($"Go {playerPokemon.Base.Name}!");
			dialogueBox.SetMoveNames(playerUnit.Pokemon.Moves);
		}

		escapeAttemps = 0; // You cannot escape from trainer battle
		partyScreen.Init();		
		ActionSelection();
	}
	
	// To perform actions after battle is over
	void BattleOver(bool won)
	{
		state = BattleState.BattleOver;
		isTrainerBattle = false;
		playerParty.Pokemons.ForEach( p => p.OnBattleOver());
		playerImage.transform.localPosition = playerOG;
		trainerImage.transform.localPosition = trainerOG;
		trainerImage.gameObject.SetActive(false);
		OnBattleOver(won);
	}

	// Enable UI for action selection phase
	void ActionSelection()
	{
		state = BattleState.ActionSelection;
		dialogueBox.SetDialogue("Choose an action");
		dialogueBox.EnableActionSelector(true);
	}
	
	// Enable UI for move selection phase
	void MoveSelection()
	{
		state = BattleState.MoveSelection;
		dialogueBox.EnableActionSelector(false);
		dialogueBox.EnableDialogueText(false);
		dialogueBox.EnableMoveSelector(true);
	}

	// Opens Party Screen
	void OpenPartyScreen()
	{
		partyScreen.CalledFrom = state;
		state = BattleState.PartyScreen;
		partyScreen.SetPartyData(playerParty.Pokemons);
		partyScreen.gameObject.SetActive(true);
	}

	// When trainer is about to use another Pokemon
	IEnumerator AboutToUse(Pokemon newPokemon)
	{
		state = BattleState.Busy;
		yield return dialogueBox.TypeDialogue($"{trainer.Name} is about to use {newPokemon.Base.Name}. Do you want to change pokemon?");

		state = BattleState.AboutToUse;
		dialogueBox.EnableChoiceBox(true);
	}

	// Enable UI for forget move selection
	IEnumerator ChooseMoveToForget(Pokemon pokemon, MoveBase newMove)
	{
		state = BattleState.MoveToForget;
		yield return dialogueBox.TypeDialogue($"Choose a move you want to forget");
		moveSelectionUI.gameObject.SetActive(true);
		moveSelectionUI.SetMoveData(pokemon.Moves.Select(x => x.Base).ToList(), newMove);
		moveToLearn = newMove;

		state = BattleState.MoveToForget;
	}

	// Runs the turn according to action states
	IEnumerator RunTurns(BattleAction playerAction)
	{
		state = BattleState.RunningTurn;

		// If action is to fight 
		if(playerAction == BattleAction.Move)
		{
			playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[currentMove];
			enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.GetRandomMove();
			
			// cache Priority
			int playerMovePriority = playerUnit.Pokemon.CurrentMove.Base.Priority;
			int enemyMovePriority = enemyUnit.Pokemon.CurrentMove.Base.Priority;
			
			// Decide who goes first
			bool playerGoesFirst = true;
			if(enemyMovePriority > playerMovePriority)
				playerGoesFirst = false;
			else if(enemyMovePriority == playerMovePriority)
				playerGoesFirst = playerUnit.Pokemon.Speed >=enemyUnit.Pokemon.Speed;
			var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
			var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;
			var secondPokemon = secondUnit.Pokemon;

			// Start the turn
			// first move
			yield return RunMove(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
			yield return RunAfterTurn(firstUnit);
			if(state == BattleState.BattleOver) yield break;

			// second move
			if(secondPokemon.HP > 0)
			{
				yield return RunMove(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
				yield return RunAfterTurn(secondUnit);

				if(state == BattleState.BattleOver) yield break;
			}
		}
		else // If action is different
		{
			// Switch Pokemon state
			if(playerAction == BattleAction.SwitchPokemon)
			{
				var selectedPokemon = partyScreen.SelectedMember;
				state = BattleState.Busy;
				yield return SwitchPokemon(selectedPokemon);
			}
			else if (playerAction == BattleAction.UseItem) // Item use state
			{
				dialogueBox.EnableActionSelector(false);
				// Here an item will be used from selection
				// Currently only usable item is Pokeball
				yield return ThrowPokeball();
			}
			else if (playerAction == BattleAction.Run) // Run
			{
				// Try to Run from Battle
				yield return TryToEscape();
			}

			// Perform enemy action
			var enemyMove = enemyUnit.Pokemon.GetRandomMove();
			yield return RunMove(enemyUnit, playerUnit, enemyMove);
			yield return RunAfterTurn(enemyUnit);
			if(state == BattleState.BattleOver) yield break;
		}

		// If battle is not over go to action selection again
		if (state != BattleState.BattleOver)
		{
			ActionSelection();
		}
	}	

	// Performs a move
	IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
	{
		bool canRunMove = sourceUnit.Pokemon.OnBeforeMove(); // Check if source unit can perform a move
		// If cannot perform a move
		if(!canRunMove)
		{
			// It is most likely due to a status Condition
			yield return ShowStatusChanges(sourceUnit.Pokemon);
			yield return sourceUnit.Hud.UpdateHP();
			yield break; 
		}

		// Even if source unit can perform a move a status Condition can affect it
		yield return ShowStatusChanges(sourceUnit.Pokemon);

		move.PP--; // Reduce the PP count of the move 
		yield return dialogueBox.TypeDialogue($"{sourceUnit.Pokemon.Base.Name} used {move.Base.Name}");

		// If the move hits
		if(CheckIfMoveHits(move, sourceUnit.Pokemon, targetUnit.Pokemon))
		{
			// Play animations
			sourceUnit.PlayAttackAnimation();
			yield return new WaitForSeconds(1f);
			targetUnit.PlayHitAnimation();
			// If it is a status move, apply effects
			if(move.Base.Category == MoveCategory.Status)
			{
				yield return RunMoveEffects(move.Base.Effects, sourceUnit.Pokemon, targetUnit.Pokemon, move.Base.Target);
			}
			else
			{
				// If it is not status move then apply damage
				var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
				yield return targetUnit.Hud.UpdateHP();
				yield return ShowDamageDetails(damageDetails);
			}

			// Apply secondary effects if the move contains
			if(move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.Pokemon.HP > 0)
			{
				foreach(var secondary in move.Base.Secondaries)
				{
					var rnd = UnityEngine.Random.Range(1,101); // For random chance
					if(rnd <= secondary.Chance)
					{
						yield return RunMoveEffects(secondary, sourceUnit.Pokemon, targetUnit.Pokemon, secondary.Target);
					}
				}
			}

			// Check if target fainted
			if (targetUnit.Pokemon.HP <= 0)
			{
				yield return HandlePokemonFainted(targetUnit);
			}
		}
		else 
		{
			// Attack missed
			yield return dialogueBox.TypeDialogue($"{sourceUnit.Pokemon.Base.Name}'s attack missed");
		}
	}

	// Performs after turn effects
	IEnumerator RunAfterTurn(BattleUnit sourceUnit)
	{
		if(state == BattleState.BattleOver) yield break;

		yield return new WaitUntil(() => state == BattleState.RunningTurn);

		// Status Conditions efffects (i.e. HP reduction)
		sourceUnit.Pokemon.OnAfterTurn();
		yield return ShowStatusChanges(sourceUnit.Pokemon);
		yield return sourceUnit.Hud.UpdateHP();
		// If Pokemon faints due to Condition
		if (sourceUnit.Pokemon.HP <= 0)
		{
			yield return HandlePokemonFainted(sourceUnit);
			yield return new WaitUntil(() => state == BattleState.RunningTurn);
		}
	}

	// Performs Stat boosts and effects 
	IEnumerator RunMoveEffects(MoveEffects effects, Pokemon source, Pokemon target, MoveTarget moveTarget)
	{
		// If there are stat booosts to apply
		if(effects.Boosts != null)
		{
			if(moveTarget == MoveTarget.Self)
				source.ApplyBoosts(effects.Boosts);
			else
				target.ApplyBoosts(effects.Boosts);
		}

		// If there are status conditions
		if(effects.Status != ConditionID.none)
		{
			// apply a status condition only when there is none present already
			target.SetStatus(effects.Status);
		}

		// If there are Volatile Statuses
		if(effects.VolatileStatus != ConditionID.none)
		{
			// apply only if there is none present
			target.SetVolatileStatus(effects.VolatileStatus);
		}

		// Show changes
		yield return ShowStatusChanges(source);
		yield return ShowStatusChanges(target);
	}

	// Returns true is move hits target
	bool CheckIfMoveHits(Move move, Pokemon source, Pokemon target) 
	{
		// If move is set for - AlwaysHits
		if (move.Base.AlwaysHits)
			return true;

		// Count the final move accuracy according to accuracy and evasion stats
		float moveAccuracy = move.CountMoveAccuracy(source.StatBoosts[Stat.Accuracy], target.StatBoosts[Stat.Evasion]);
		
		return UnityEngine.Random.Range(1,101) <= moveAccuracy;
	}

	// Shows status changes for pokemon
	IEnumerator ShowStatusChanges(Pokemon pokemon)
	{
		while(pokemon.StatusChanges.Count > 0)
		{
			var message = pokemon.StatusChanges.Dequeue();
			yield return dialogueBox.TypeDialogue(message);
		}
	}

	// Handles actions like exp gain when a Pokemon faints
	IEnumerator HandlePokemonFainted(BattleUnit faintedUnit)
	{
		yield return dialogueBox.TypeDialogue($"{faintedUnit.Pokemon.Base.Name} fainted");
		faintedUnit.PlayFaintAnimation();
		yield return new WaitForSeconds(2f);
		// Check if it was a player Unit
		if(!faintedUnit.IsPlayerUnit)
		{
			// If it was enemy Unit, the player will gain EXP
			int expYield = faintedUnit.Pokemon.Base.ExpYield;
			int enemyLevel = faintedUnit.Pokemon.Level;
			float trainerBonus = (isTrainerBattle) ? 1.5f : 1f;

			int expGain = Mathf.FloorToInt((expYield * enemyLevel * trainerBonus) / 7);
			playerUnit.Pokemon.Exp += expGain;
			yield return dialogueBox.TypeDialogue($"{playerUnit.Pokemon.Base.Name} gained {expGain} exp");
			yield return playerUnit.Hud.SetExpSmooth();

			// Check the Pokemon can level up
			while (playerUnit.Pokemon.CheckForLevelUp())
			{
				playerUnit.Hud.SetLevel();
				yield return dialogueBox.TypeDialogue($"{playerUnit.Pokemon.Base.Name} grew to level {playerUnit.Pokemon.Level}");
				
				// If there is a new move that can be learned at this new level 
				var newMove = playerUnit.Pokemon.GetLearnableMoveAtCurrentLevel();
				if(newMove != null)
				{
					// Learn new move
					if(playerUnit.Pokemon.Moves.Count < PokemonBase.MaxNumOfMoves)
					{
						// If Pokemon knows < 4 moves
						playerUnit.Pokemon.LearnMove(newMove);
						yield return dialogueBox.TypeDialogue($"{playerUnit.Pokemon.Base.Name} learned {newMove.Base.Name}");
						dialogueBox.SetMoveNames(playerUnit.Pokemon.Moves);
					}
					else
					{
						// If Pokemon already knows 4 moves
						// Forget a move to learn new move
						yield return dialogueBox.TypeDialogue($"{playerUnit.Pokemon.Base.Name} is trying to learn {newMove.Base.Name}");
						yield return dialogueBox.TypeDialogue($"But it cannot learn more than {PokemonBase.MaxNumOfMoves} moves");
						yield return ChooseMoveToForget(playerUnit.Pokemon, newMove.Base);
						yield return new WaitUntil(() => state != BattleState.MoveToForget);
						yield return new WaitForSeconds(2f);
					}
				}
				// If there is EXP gain left after level up
				yield return playerUnit.Hud.SetExpSmooth(true);
			}
		}
		CheckBattleOver(faintedUnit);
	}

	// Check battle is over after the Pokemon fainted
	void CheckBattleOver(BattleUnit faintedUnit)
	{
		// If player unit fainted 
		if(faintedUnit.IsPlayerUnit)
		{
			var nextPokemon = playerParty.GetHealthyPokemon();
			if(nextPokemon != null)
				OpenPartyScreen();
			else
				BattleOver(false);
		}
		else // Enemy unit fainted
		{
			// If it is not trainer battle
			if (!isTrainerBattle)
			{
				BattleOver(true);	
			}
			else
			{
				// Send another pokemon from trainer if available
				var nextPokemon = trainerParty.GetHealthyPokemon();
				if (nextPokemon != null)
					StartCoroutine(AboutToUse(nextPokemon));
				else
					BattleOver(true);
			}
		}
	}

	// Shows damage details
	IEnumerator ShowDamageDetails(DamageDetails damageDetails)
	{
		// If the move was a critical hit
		if (damageDetails.Critical > 1f)
			yield return dialogueBox.TypeDialogue("A critical hit!");

		// If the move had a type effectiveness
		if(damageDetails.TypeEffectiveness > 1f)
			yield return dialogueBox.TypeDialogue("It's super effective!");
		else if(damageDetails.TypeEffectiveness < 1f)
			yield return dialogueBox.TypeDialogue("It's not very effective");
	}

	// Handles Updates according to Battle states
	public void HandleUpdate()
	{
		if (state == BattleState.ActionSelection)
		{
			HandleActionSelection();
		}
		else if (state == BattleState.MoveSelection)
		{
			HandleMoveSelection();
		}
		else if (state == BattleState.PartyScreen)
		{
			HandlePartySelection();
		}
		else if ( state == BattleState.AboutToUse)
		{
			HandleAboutToUse();
		}
		else if(state == BattleState.MoveToForget)
		{
			// This action deals move selection
			Action<int> onMoveSelected  = (moveIndex) =>
			{
				moveSelectionUI.gameObject.SetActive(false);
				if(moveIndex == PokemonBase.MaxNumOfMoves)
				{
					// Don't learn the move
					StartCoroutine(dialogueBox.TypeDialogue($"{playerUnit.Pokemon.Base.Name} did not learn {moveToLearn.Name}"));
				}
				else
				{
					// Forget the selectedMove and learn new move
					var selectedMove = playerUnit.Pokemon.Moves[moveIndex].Base;
					StartCoroutine(dialogueBox.TypeDialogue($"{playerUnit.Pokemon.Base.Name} forgot {selectedMove.Name} and learned {moveToLearn.Name}"));

					playerUnit.Pokemon.Moves[moveIndex] = new Move(moveToLearn);
				}

				moveToLearn = null;
				state = BattleState.RunningTurn;
			};

			moveSelectionUI.HandleMoveSelection(onMoveSelected);
		}
	}

	// Handles selection of action
	void HandleActionSelection()
	{
		// Update selection according to arrow keys
		if(Input.GetKeyDown(KeyCode.RightArrow))
			++currentAction;
		else if(Input.GetKeyDown(KeyCode.LeftArrow))
			--currentAction;
		else if(Input.GetKeyDown(KeyCode.DownArrow))
			currentAction+=2;
		else if(Input.GetKeyDown(KeyCode.UpArrow))
			currentAction-=2;

		currentAction = Mathf.Clamp(currentAction, 0, 3);
		dialogueBox.UpdateActionSelection(currentAction);

		// To confirm selection
		if (Input.GetKeyDown(KeyCode.Z))
		{
			if (currentAction == 0)
			{
				// Fight
				MoveSelection();
			}
			else if (currentAction == 1)
			{
				// Bag
				StartCoroutine(RunTurns(BattleAction.UseItem));
			}
			else if (currentAction == 2)
			{
				// Pokemon
				OpenPartyScreen();
			}
			else if (currentAction == 3)
			{
				// Run
				StartCoroutine(RunTurns(BattleAction.Run));
			}
		}
	}

	// Handles selection of moves to perform
	void HandleMoveSelection()
	{
		// Update selection according to movement keys
		if(Input.GetKeyDown(KeyCode.RightArrow))
			++currentMove;
		else if(Input.GetKeyDown(KeyCode.LeftArrow))
			--currentMove;
		else if(Input.GetKeyDown(KeyCode.DownArrow))
			currentMove+=2;
		else if(Input.GetKeyDown(KeyCode.UpArrow))
			currentMove-=2;

		currentMove = Mathf.Clamp(currentMove, 0 , playerUnit.Pokemon.Moves.Count - 1);
		dialogueBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

		// For confirming the selection
		if (Input.GetKeyDown(KeyCode.Z))
		{
			var move = playerUnit.Pokemon.Moves[currentMove];
			
			if(move.PP == 0) 
				return;

			dialogueBox.EnableMoveSelector(false);
			dialogueBox.EnableDialogueText(true);

			StartCoroutine(RunTurns(BattleAction.Move));
		}

		// If input is Back
		else if (Input.GetKeyDown(KeyCode.X))
		{
			dialogueBox.EnableMoveSelector(false);
			dialogueBox.EnableDialogueText(true);
			ActionSelection();
		}
	}

	// Handles selection of Party members
	void HandlePartySelection()
	{
		// This action is called when selection is confirmed
		Action onSelected = () =>
		{
			var selectedMember = partyScreen.SelectedMember;
			if(selectedMember.HP <= 0)
			{
				partyScreen.SetMessageText("You can't send out a fainted Pokemon");
				return;
			}
			if(selectedMember == playerUnit.Pokemon)
			{
				partyScreen.SetMessageText($"{selectedMember.Base.Name} is already in battle");
				return;
			}

			partyScreen	.gameObject.SetActive(false);

			if (partyScreen.CalledFrom == BattleState.ActionSelection)
			{
				StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
			}
			else
			{
				state = BattleState.Busy;
				bool isTrainerAboutToUse = partyScreen.CalledFrom == BattleState.AboutToUse;
				StartCoroutine(SwitchPokemon(selectedMember, isTrainerAboutToUse));
			}
			
			partyScreen.CalledFrom = null;
		};

		// This action is called when pressed back key
		Action onBack = () => 
		{
			if (playerUnit.Pokemon.HP <= 0 )
			{
				partyScreen.SetMessageText("You have to choose Pokemon to continue");
				return;
			}

			partyScreen.gameObject.SetActive(false);
			if (partyScreen.CalledFrom == BattleState.AboutToUse )
			{
				StartCoroutine(SendNextTrainerPokemon());
			}
			else
				ActionSelection();

			partyScreen.CalledFrom = null;
		};

		partyScreen.HandleUpdate(onSelected, onBack);
	}

	// Handles about to use scenario 
	void HandleAboutToUse()
	{
		// selection for choice yes / no
		if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
		aboutToUseChoice = !aboutToUseChoice;
		dialogueBox.UpdateChoiceBox(aboutToUseChoice);
		// confirm selection
		if (Input.GetKeyDown(KeyCode.Z))
		{
			dialogueBox.EnableChoiceBox(false);
			if (aboutToUseChoice == true)
			{
				//Yes option 
				OpenPartyScreen();
			}
			else
			{
				//No option 
				StartCoroutine(SendNextTrainerPokemon());
			}
		}
		else if (Input.GetKeyDown(KeyCode.X))
		{
			dialogueBox.EnableChoiceBox(false);
			StartCoroutine(SendNextTrainerPokemon());
		}
	}

	// Switchs out current Pokemon with a new Pokemon
	IEnumerator SwitchPokemon(Pokemon newPokemon, bool isTrainerAboutToUse=false)
	{
		if(playerUnit.Pokemon.HP > 0)
		{
			yield return dialogueBox.TypeDialogue($"Come back {playerUnit.Pokemon.Base.Name}");
			playerUnit.PlayFaintAnimation();
			yield return new WaitForSeconds(2f);
		}

		playerUnit.Setup(newPokemon);
		dialogueBox.SetMoveNames(newPokemon.Moves);

		yield return dialogueBox.TypeDialogue($"Go {newPokemon.Base.Name}!");

		if(isTrainerAboutToUse)
			StartCoroutine(SendNextTrainerPokemon());
		else
			state = BattleState.RunningTurn;
	}

	// Sends next Pokemon available in trainer party
	IEnumerator SendNextTrainerPokemon ()
	{
		state = BattleState.Busy;

		var nextPokemon = trainerParty.GetHealthyPokemon();

		enemyUnit.Setup(nextPokemon);
		yield return dialogueBox.TypeDialogue($"{trainer.Name} send out {nextPokemon.Base.Name}!");
		state = BattleState.RunningTurn;
	}

	// Throws Pokeball in attempt of catching Pokemon
	IEnumerator ThrowPokeball()	
	{
		state = BattleState.Busy;
		// If tried this in trainer battle, prevent it
		if (isTrainerBattle)
		{
			yield return dialogueBox.TypeDialogue($"You can't steal the trainer pokemon!");
			state = BattleState.RunningTurn;
			yield break;
		}

		yield return dialogueBox.TypeDialogue($"{player.Name} used POKEBALL!");

		var pokeballObj = Instantiate(pokeballSprite,playerUnit.transform.position - new Vector3(2, 0), Quaternion.identity);
		var pokeball = pokeballObj.GetComponent<SpriteRenderer>();

		// Animation
		yield return pokeball.transform.DOJump(enemyUnit.transform.position + new Vector3(0, 2), 2f, 1 ,1f).WaitForCompletion();
		yield return enemyUnit.PlayerCaptureAnimation();
		yield return pokeball.transform.DOMoveY(enemyUnit.transform.position.y - 1.3f, 0.5f).WaitForCompletion();

		int shakeCount = TryToCatchPokemon(enemyUnit.Pokemon);

		for (int i = 0; i < Mathf.Min(shakeCount, 3); ++i)
		{
			yield return new WaitForSeconds(0.5f);
			yield return pokeball.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f).WaitForCompletion();
		}

		if (shakeCount == 4)
		{
			// Pokemon is caught
			yield return dialogueBox.TypeDialogue($"{enemyUnit.Pokemon.Base.Name} was caught");
			yield return pokeball.DOFade(0, 1.5f).WaitForCompletion();

			playerParty.AddPokemon(enemyUnit.Pokemon);
			yield return dialogueBox.TypeDialogue($"{enemyUnit.Pokemon.Base.Name} has beeen added your party");

			Destroy(pokeball);
			BattleOver(true);
		}
		else
		{
			// pokemon broke out 
			yield return new WaitForSeconds(1f);
			pokeball.DOFade(0, 0.2f);
			yield return enemyUnit.PlayerBreakOutAnimation();

			if (shakeCount < 2)
				yield return dialogueBox.TypeDialogue($"{enemyUnit.Pokemon.Base.Name} broke free");
			else
				yield return dialogueBox.TypeDialogue($"Almost caught it");

			Destroy(pokeball);
			state = BattleState.RunningTurn;
		}
	}

	// Determine catch sucess and return it in form of an int shakeCount (4 means caught)
	int TryToCatchPokemon(Pokemon pokemon)
	{
		float a = (3 * pokemon.MaxHp  - 2 * pokemon.HP) * pokemon.Base.CatchRate *  ConditionsDB.GetStatusBonus(pokemon.Status) / (3 * pokemon.MaxHp);

		if (a >= 255)
			return 4;

		float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

		int shakeCount = 0;

		while (shakeCount < 4)
		{
			if (UnityEngine.Random.Range(0,65535) >= b)
				break;

			++shakeCount;
		}
		return shakeCount;
	}

	// Try to escape from battle
	IEnumerator TryToEscape()
	{
		state = BattleState.Busy;

		if (isTrainerBattle)
		{
			yield return dialogueBox.TypeDialogue($"You can't run from trainer battles!");
			state = BattleState.RunningTurn;
			yield break;
		}
		++escapeAttemps;

		int playerSpeed = playerUnit.Pokemon.Speed;
		int enemySpeed = enemyUnit.Pokemon.Speed;

		// If player speed is more than run immediately
		if (enemySpeed < playerSpeed)
		{
			yield return dialogueBox.TypeDialogue($"Ran away safely!");
			BattleOver(true);
		}
		else
		{
			// Determine a temp float
			float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttemps;
			f = f % 256;

			// Count a random chance and try to run from battle
			if (UnityEngine.Random.Range(0, 256) < f)
			{
				yield return dialogueBox.TypeDialogue($"Ran away safely!");
				BattleOver(true);
			}
			else
			{
				yield return dialogueBox.TypeDialogue($"Can't escape!");
				state = BattleState.RunningTurn;
			}
		}
	}	
}