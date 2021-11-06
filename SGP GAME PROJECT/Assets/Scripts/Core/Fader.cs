/*
	@author - Mitren Kadiwala
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

//Fades off the screen and gives animation kind of thing while changig the scenes
public class Fader : MonoBehaviour
{
    Image image;

    private void Awake() 
    {
        image = GetComponent<Image>();   
    }

    public IEnumerator FadeIn(float time)
    {
        yield return image.DOFade(1f , time).WaitForCompletion();
    }

    public IEnumerator FadeOut(float time)
    {
        yield return image.DOFade(0f , time).WaitForCompletion();
    }
}
