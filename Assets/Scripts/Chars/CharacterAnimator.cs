using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public enum FaceDir { down , up , left, right};

public class CharacterAnimator : MonoBehaviour
{
    //Parameters
    public float moveX;
    public float moveY;
    public bool isMoving;
    public FaceDir lookDir;

    List<Sprite> walkUpSprites;
    List<Sprite> walkDownSprites;
    List<Sprite> walkRightSprites;
    List<Sprite> walkLeftSprites;

    [SerializeField] List<Sprite> Frames;

    //States
    SpriteAnimator walkUpAnim;
    SpriteAnimator walkDownAnim;
    SpriteAnimator walkRightAnim;
    SpriteAnimator walkLeftAnim;

    SpriteAnimator currentAnim;

    //References
    SpriteRenderer spriteRenderer;

    private void Start()
    {
        ChangeFaceDir(lookDir);

        walkUpSprites = new List<Sprite>();
        walkDownSprites = new List<Sprite>();
        walkLeftSprites = new List<Sprite>();
        walkRightSprites = new List<Sprite>();

        var WalkUpFrames = new int[] { 6, 5, 6, 7 };
        var WalkDownFrames = new int[] { 0, 1, 0, 2 };
        var WalkRightFrames = new int[] { 8, 9 };
        var WalkLeftFrames = new int[] { 3, 4 };

        for (int i = 0; i < WalkUpFrames.Length; i++)
        { walkUpSprites.Add(Frames[WalkUpFrames[i]]); }
        for (int i = 0; i < WalkDownFrames.Length; i++)
        { walkDownSprites.Add(Frames[WalkDownFrames[i]]); }
        for (int i = 0; i < WalkRightFrames.Length; i++)
        { walkRightSprites.Add(Frames[WalkRightFrames[i]]); }
        for (int i = 0; i < WalkLeftFrames.Length; i++)
        { walkLeftSprites.Add(Frames[WalkLeftFrames[i]]); }

        spriteRenderer = GetComponent<SpriteRenderer>();
        walkUpAnim = new SpriteAnimator(walkUpSprites, spriteRenderer);
        walkDownAnim = new SpriteAnimator(walkDownSprites, spriteRenderer);
        walkRightAnim = new SpriteAnimator(walkRightSprites, spriteRenderer);
        walkLeftAnim = new SpriteAnimator(walkLeftSprites, spriteRenderer);

        currentAnim = walkDownAnim;
    }

    private void Update()
    {
        var prevAnim = currentAnim;

        if (moveX == 1) 
        { currentAnim = walkRightAnim; }
        else if (moveX == -1) 
        { currentAnim = walkLeftAnim; }
        if (moveY == 1) 
        { currentAnim = walkUpAnim; }
        else if (moveY == -1)
        { currentAnim = walkDownAnim; }

        if (prevAnim != currentAnim) 
        { currentAnim.Start(); }

        if (isMoving)
        { currentAnim.HandleUpdate(); }
        else 
        { currentAnim.Start(); }
    }

    public void ChangeFaceDir(FaceDir faceDir)
    {
        lookDir = faceDir;

        if (faceDir == FaceDir.down)
        { moveX = 0; moveY = -1; }
        else if (faceDir == FaceDir.up)
        { moveX = 0; moveY = 1; }
        else if (faceDir == FaceDir.left)
        { moveX = -1; moveY = 0; }
        else
        { moveX = 1; moveY = 0; }
    }
}
