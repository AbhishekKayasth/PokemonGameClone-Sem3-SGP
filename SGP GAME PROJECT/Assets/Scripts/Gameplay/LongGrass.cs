/*
	@author - Taufik Mansuri
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongGrass : MonoBehaviour, IPlayerTriggerable
{
     public void OnPlayerTriggered(PlayerController player)
    {
       if (UnityEngine.Random.Range(1, 101) <= 10)
			{
                player.Character.Animator.IsMoving =false;
                AudioManager.i.PlayMusic(SoundLibrary.GetClipFromName("Battle (Wild)"), 1.5f);
				StartCoroutine(GameController.Instance.StartBattle());
			}
    }
}
