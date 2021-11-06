/*
	Module name - BattleDialogueBox
	Module creation date - 03-Sep-2021
	@author - Abhishek Kayasth
*/
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BattleHud : MonoBehaviour
{
	[Header("References for HUD elements")]
	// referance variables
	[SerializeField] Text nameText;
	[SerializeField] Text levelText;
	[SerializeField] Text statusText;
	[SerializeField] HPBar hpBar;
	[SerializeField] GameObject expBar;

	[Header("Status text color")]
	// For status text color
	[SerializeField] Color psnColor; // Poison status
	[SerializeField] Color brnColor; // Burn status
	[SerializeField] Color slpColor; // Sleep status
	[SerializeField] Color parColor; // Paralysed status
	[SerializeField] Color frzColor; // Freeze status

	// cache variables
	Pokemon _pokemon;

	// To keep track of status and appropriate color
	Dictionary<ConditionID , Color> statusColors;

	// Set data of given Pokemon on HUD UI elements
	public void SetData(Pokemon pokemon)
	{
		_pokemon = pokemon; // caching into local variable
		nameText.text = pokemon.Base.Name;
		SetLevel();
		hpBar.SetHP((float)pokemon.HP / pokemon.MaxHp);
		SetExp();

		statusColors = new Dictionary<ConditionID, Color>()
		{
			{ConditionID.psn , psnColor},
			{ConditionID.brn , brnColor},
			{ConditionID.slp , slpColor},
			{ConditionID.par , parColor},
			{ConditionID.frz , frzColor},
		};
		SetStatusText();
		
		_pokemon.OnStatusChanged += SetStatusText;
	}

	// For setting text of the status effect
	void SetStatusText()
	{
		if (_pokemon.Status == null)
		{
			statusText.text = "";
		}
		else 
		{
			statusText.text = _pokemon.Status.Id.ToString().ToUpper();
			statusText.color = statusColors[_pokemon.Status.Id];
		}
	}

	// For setting level text
	public void SetLevel()
	{
		levelText.text = "Lvl " + _pokemon.Level;
	}

	// For setting EXP
	public void SetExp()
	{
		if(expBar == null) return;

		float normalizedExp = GetNormalizedExp();
		expBar.transform.localScale = new Vector3(normalizedExp, 1 , 1);
	}

	// Display EXP with animation
	public IEnumerator SetExpSmooth(bool reset=false)
	{
		if(expBar == null) yield break;

		if(reset)
			expBar.transform.localScale = new Vector3(0, 1, 1);

		float normalizedExp = GetNormalizedExp();
		yield return expBar.transform.DOScaleX(normalizedExp, 1.5f).WaitForCompletion();
	}

	// It returns normalized EXP for exp bar Display
	float GetNormalizedExp()
	{
		int currLevelExp = _pokemon.Base.GetExpForLevel(_pokemon.Level);
		int nextLevelExp = _pokemon.Base.GetExpForLevel(_pokemon.Level + 1);

		float normalizedExp = (float)(_pokemon.Exp - currLevelExp) / (nextLevelExp - currLevelExp);
		return Mathf.Clamp01(normalizedExp);
	}

	// Updates HP bar with animation 
	public IEnumerator UpdateHP()
	{
		if(_pokemon.HpChanged)
		{
			yield return hpBar.SetHPSmooth((float)_pokemon.HP / _pokemon.MaxHp);
			_pokemon.HpChanged = false;
		}
	}
}