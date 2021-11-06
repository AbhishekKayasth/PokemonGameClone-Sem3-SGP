/*
	Module name - BattleUnit
	Module creation date - 04-Sep-2021
    @author - Abhishek Kayasth
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // Tool for animation

public class BattleUnit : MonoBehaviour
{
    [SerializeField] bool isPlayerUnit;
    [SerializeField] BattleHud hud;

    public bool IsPlayerUnit { get { return isPlayerUnit; } }
    public BattleHud Hud { get { return hud; } }
    public Pokemon Pokemon { get; set; }

    // cache variables
    Image image;
    Vector3 originalPos;
    Color originalColor;

    // Called when scene is loaded (Before starting)
	private void Awake()
	{
        image = GetComponent<Image>();
        originalPos = image.transform.localPosition;
        originalColor = image.color;
	}

    // Set up Pokemon image and HUD 
	public void Setup(Pokemon pokemon)
    {
        Pokemon = pokemon;

        if (isPlayerUnit)
            image.sprite = Pokemon.Base.BackSprite;
        else
            image.sprite = Pokemon.Base.FrontSprite;

        // Reset the scale & color of image if it was changed before
        transform.localScale = new Vector3(1, 1, 1);
        image.color = originalColor;

        StartCoroutine(PlayEnterAnimation());
    }

    // Disables HUD 
    public void Clear()
    {
       hud.gameObject.SetActive(false);
    }

    // Animations
    public IEnumerator PlayEnterAnimation()
    {
        if (isPlayerUnit)
            image.transform.localPosition = new Vector3(-1500, originalPos.y);
        else
            image.transform.localPosition = new Vector3(1500, originalPos.y);

        yield return image.transform.DOLocalMoveX(originalPos.x, 2f).WaitForCompletion();

        hud.gameObject.SetActive(true);
        hud.SetData(Pokemon);
    }

    public void PlayAttackAnimation()
    {
        var sequence = DOTween.Sequence();
        if (isPlayerUnit)
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x + 25f, 0.25f));
        else
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x - 25f, 0.25f));

        sequence.Append(image.transform.DOLocalMoveX(originalPos.x, 0.25f));
    }

    public void PlayHitAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOColor(Color.gray, 0.1f));
        sequence.Append(image.DOColor(originalColor, 0.2f));
    }

    public void PlayFaintAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.transform.DOLocalMoveY(originalPos.y - 150f, 0.5f));
        sequence.Join(image.DOFade(0, 0.5f));
    }

    public IEnumerator PlayerCaptureAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOFade(0, 0.5f));
        sequence.Join(transform.DOLocalMoveY(originalPos.y + 50f, 0.5f));
        sequence.Join(transform.DOScale(new Vector3(0.3f, 0.3f, 1f),0.5f));
        yield return sequence.WaitForCompletion();
    }

    public IEnumerator PlayerBreakOutAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOFade(1, 0.5f));
        sequence.Join(transform.DOLocalMoveY(originalPos.y, 0.5f));
        sequence.Join(transform.DOScale(new Vector3(1f, 1f, 1f),0.5f));
        yield return sequence.WaitForCompletion();
    }
}
