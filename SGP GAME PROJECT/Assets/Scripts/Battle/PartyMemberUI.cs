/*
	Module name - PartyMemberUI
	Module creation date - 01-Oct-2021
	@author -Mitren Kadiwala
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    // referance variables
	[SerializeField] Text nameText;
	[SerializeField] Text levelText;
	[SerializeField] HPBar hpBar;

	// cache variable
	Pokemon _pokemon;

	// Set data of given Pokemon on HUD UI elements
	public void SetData(Pokemon pokemon)
	{
		_pokemon = pokemon; // caching into local variable
		nameText.text = pokemon.Base.Name;
		levelText.text = "Lvl " + pokemon.Level;
		hpBar.SetHP((float)pokemon.HP / pokemon.MaxHp);
	}

	// Highlights Member UI if input is true
	public void SetSelected(bool selected)
	{
		if(selected)
			nameText.color = GlobalSettings.i.HighlightedColor;
		else
			nameText.color = Color.black;
	}
}
