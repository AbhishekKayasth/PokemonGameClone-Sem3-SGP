/*
	@author - Taufik Mansuri
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Character : MonoBehaviour
{
    public float moveSpeed;
    public bool IsMoving { get; private set;}
	public float OffsetY {get; private set; } = 0.3f;
	public CharacterAnimator Animator { get => animator; }

    CharacterAnimator animator;

	// Called when scene is loaded
    private void Awake()
    {
        animator = GetComponent<CharacterAnimator>();
		SetPositionAndSnapToTile(transform.position);
    }

	// Sets position in a tile
	public void SetPositionAndSnapToTile(Vector2 pos)
	{
		// First floor function removes the decimal value and then we add 0.5 so 
		// that we dont have to manually set each player position in transform function
		pos.x = Mathf.Floor(pos.x) + 0.5f;
		pos.y = Mathf.Floor(pos.y) + 0.5f + OffsetY;

		transform.position = pos;
	}

	// Handles Movement
   	public IEnumerator Move(Vector2 moveVec, Action OnMoveOver=null)
	{
		animator.MoveX = Mathf.Clamp(moveVec.x, -1f, 1f);
		animator.MoveY = Mathf.Clamp(moveVec.y, -1f, 1f);

		var targetPos = transform.position;
		targetPos.x += moveVec.x;
		targetPos.y += moveVec.y;

        if (!IsPathClear(targetPos)) yield break;
        
		IsMoving = true;

		while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
		{
			transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
			yield return null;
		}

		transform.position = targetPos;

		IsMoving = false;

		OnMoveOver?.Invoke();
	}

	// Handles Upddate and movement status from animator
    public void HandleUpdate()
    {
        animator.IsMoving = IsMoving;
    }

	// Checks if path for target is clear
	private bool IsPathClear(Vector3 targetPos)
	{
		var diff = targetPos - transform.position;
		var dir = diff.normalized;

		if(Physics2D.BoxCast(transform.position + dir,new Vector2(0.2f,0.2f),0f,dir,diff.magnitude - 1, GameLayers.i.SolidLayer | GameLayers.i.InteractableLayer | GameLayers.i.PlayerLayer) == true)
			return false;

		return true;
	}

	// Returns true if the target position is walkable
    private bool IsWalkable(Vector3 targetPos)
	{
		if (Physics2D.OverlapCircle(targetPos, 0.2f, GameLayers.i.SolidLayer | GameLayers.i.InteractableLayer) != null)
		{
			return false;
		}
		return true;
	}

	// Sets character to look towards target
	public void LookTowards  (Vector3 targetPos)
	{
		var xdiff = Mathf.Floor(targetPos.x) - Mathf.Floor(transform.position.x);
		var ydiff = Mathf.Floor(targetPos.y) - Mathf.Floor(transform.position.y);

		// Prevent character to look diagonally
		if (xdiff == 0 || ydiff == 0)
		{
			animator.MoveX = Mathf.Clamp(xdiff, -1f, 1f);
			animator.MoveY = Mathf.Clamp(ydiff, -1f, 1f);	
		}
		else
			Debug.LogError("Error is Look Towards: You can't ask the character to look diagonally");
	}
}
