using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimator 
{
    SpriteRenderer spriteRenderer;
    List<Sprite> frames;
    float frameRate;

    int currentFrame;
    float timer;


    public SpriteAnimator(List<Sprite> _frames, SpriteRenderer _spriteRenderer, float _frameRate=0.12f)
    {
        frames = _frames;
        spriteRenderer = _spriteRenderer;
        frameRate = _frameRate;
    }

    public void Start()
    {
        currentFrame = 0;
        timer = Mathf.Infinity;
        spriteRenderer.sprite = frames[0];
    }

    public void HandleUpdate()
    {
        timer += Time.deltaTime;
        if (timer > frameRate)
        {
            currentFrame = (currentFrame + 1) % frames.Count;
            spriteRenderer.sprite = frames[currentFrame];
            timer = 0;
        }
    }
}
