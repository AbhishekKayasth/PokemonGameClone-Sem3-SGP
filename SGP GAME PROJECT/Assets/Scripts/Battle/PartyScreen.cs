/*
	Module name - PartyScreen
	Module creation date - 02-Oct-2021
    @author -Mitren Kadiwala
*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] Text messageText;

    PartyMemberUI[] memberSlots;
    List<Pokemon> pokemons;

    int selection = 0;

    public Pokemon SelectedMember => pokemons[selection];

    /// <summary>
    /// Party screen can be called from different states like ActionSelection, RunningTurn, AboutToUse
    /// </summary>
    public BattleState? CalledFrom { get; set; }

    // Initializes members
    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);
    }

    // Sets Pokemon data in memberSlots
    public void SetPartyData(List<Pokemon> pokemons)
    {
        this.pokemons = pokemons;
        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < pokemons.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].SetData(pokemons[i]);
            }
            else
                memberSlots[i].gameObject.SetActive(false);
        }

        UpdateMemberSelection(selection);

        messageText.text = "Choose a Pokemon";
    }

    // Handles UI updation
    public void HandleUpdate(Action onSelected, Action onBack)
	{
        int prevSelection = selection;

		if(Input.GetKeyDown(KeyCode.RightArrow))
			++selection;
		else if(Input.GetKeyDown(KeyCode.LeftArrow))
			--selection;
		else if(Input.GetKeyDown(KeyCode.DownArrow))
			selection += 2;
		else if(Input.GetKeyDown(KeyCode.UpArrow))
			selection -= 2;

		selection = Mathf.Clamp(selection, 0 , pokemons.Count - 1);

        if(selection != prevSelection)
		    UpdateMemberSelection(selection);

		if(Input.GetKeyDown(KeyCode.Z))
		{
            onSelected?.Invoke();
		}
		else if(Input.GetKeyDown(KeyCode.X))
		{
            onBack?.Invoke();
		}
	}

    // Updates UI selection
    public void UpdateMemberSelection(int selectedMember)
    {
        for(int i = 0; i < pokemons.Count; i++)
        {
            if(i == selectedMember)
                memberSlots[i].SetSelected(true);
            else
                memberSlots[i].SetSelected(false);
        }
    }

    // Sets text in message box
    public void SetMessageText(string message)
    {
        messageText.text = message;
    }
}