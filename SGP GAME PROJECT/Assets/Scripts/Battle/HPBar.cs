/*
	Module name - HPBar
	Module creation date - 05-Sep-2021
	@author - Abhishek Kayasth
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBar : MonoBehaviour
{
	[SerializeField] GameObject health;

	// Sets HP bar length according to value
	public void SetHP(float hpNormalized)
	{
		health.transform.localScale = new Vector3(hpNormalized, 1f);
	}

	// Sets HP with a smooth animation
	public IEnumerator SetHPSmooth(float newHP)
	{
		float currHP = health.transform.localScale.x;
		float changeAmt = currHP - newHP;

		while (currHP - newHP > Mathf.Epsilon)
		{
			currHP -= changeAmt * Time.deltaTime;
			health.transform.localScale = new Vector3(currHP, 1f);
			yield return null;
		}
		health.transform.localScale = new Vector3(newHP, 1f);
	}
}
