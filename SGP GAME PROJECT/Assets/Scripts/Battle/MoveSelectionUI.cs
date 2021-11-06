/*
	Module name - MoveSelectionUI
	Module creation date - 25-Oct-2021
    @author - Abhishek Kayasth
*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveSelectionUI : MonoBehaviour
{
    [SerializeField] List<Text> moveText;

    int currentSelection = 0;

    // Sets current available moves and new move names in UI
    public void SetMoveData(List<MoveBase> currentMoves, MoveBase newMove)
    {
        for(int i = 0;  i < currentMoves.Count; i++)
        {
            moveText[i].text = currentMoves[i].Name;
        }

        moveText[currentMoves.Count].text = newMove.Name;
    }

    // Handles Move selection input 
    public void HandleMoveSelection(Action<int> onSelected)
    {
		if(Input.GetKeyDown(KeyCode.DownArrow))
			++currentSelection;
		else if(Input.GetKeyDown(KeyCode.UpArrow))
			--currentSelection;

        UpdateMoveSelection(currentSelection);
        currentSelection = Mathf.Clamp(currentSelection, 0, PokemonBase.MaxNumOfMoves);

        if (Input.GetKeyDown(KeyCode.Z))
			onSelected.Invoke(currentSelection);
    }

    // Handles Move selection UI updates
    public void UpdateMoveSelection(int selection)
    {
        for(int i = 0; i < PokemonBase.MaxNumOfMoves + 1; i++)
        {
            if(i == selection)
                moveText[i].color = GlobalSettings.i.HighlightedColor;
            else
                moveText[i].color = Color.black;
        }
    }
}
