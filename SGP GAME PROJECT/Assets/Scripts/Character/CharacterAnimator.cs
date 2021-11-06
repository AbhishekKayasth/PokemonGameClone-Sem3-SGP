/*
	@author - Taufik Mansuri
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// To determine direction to face
public enum FacingDirection { Up, Down, Left, Right }

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] List<Sprite> walkDownSprites;
    [SerializeField] List<Sprite> walkUpSprites;
    [SerializeField] List<Sprite> walkRightSprites;
    [SerializeField] List<Sprite> walkLeftSprites;
    [SerializeField] FacingDirection defaultDirection = FacingDirection.Down;

    // Parameters
    public float MoveX { get; set; }
    public float MoveY { get; set; }
    public bool IsMoving { get; set; }
    
    // Default Direction to face
    public FacingDirection DefaultDirection { get => defaultDirection; }

    // States (FacingDirection)
    SpriteAnimator walkDownAnim;
    SpriteAnimator walkUpAnim;
    SpriteAnimator walkRightAnim;
    SpriteAnimator walkLeftAnim;

    // To keep in track of current situation
    SpriteAnimator currentAnim;
    bool wasPreviouslyMoving;
    // refrences
    SpriteRenderer spriteRenderer;

    // Called when the GameObject is Initialized
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        walkDownAnim = new SpriteAnimator(walkDownSprites,spriteRenderer);
        walkUpAnim = new SpriteAnimator(walkUpSprites,spriteRenderer);
        walkRightAnim = new SpriteAnimator(walkRightSprites,spriteRenderer);
        walkLeftAnim = new SpriteAnimator(walkLeftSprites,spriteRenderer);
        SetFacingDirection(defaultDirection);
        currentAnim = walkDownAnim;
    }

    // Called on each frame
    private void Update()
    {
        var prevAnim = currentAnim;

        if (MoveX == 1)
            currentAnim = walkRightAnim;
        else if (MoveX == -1)
            currentAnim = walkLeftAnim;
        else if (MoveY == 1)
            currentAnim = walkUpAnim;
        else if (MoveY == -1)
            currentAnim = walkDownAnim;

        if (currentAnim !=prevAnim || IsMoving != wasPreviouslyMoving )
            currentAnim.Start();

        if (IsMoving)
            currentAnim.HandleUpdate();
        else
            spriteRenderer.sprite = currentAnim.Frames[0];
        
        wasPreviouslyMoving = IsMoving;
    }

    // Sets Direction to face
    public void SetFacingDirection(FacingDirection dir)
    {
        if ( dir == FacingDirection.Right)
            MoveX= 1;
        else if ( dir == FacingDirection.Left)
            MoveX= -1;
        else if ( dir == FacingDirection.Down)
            MoveY= -1;
        else if ( dir == FacingDirection.Up)
            MoveY= 1;
    }
}