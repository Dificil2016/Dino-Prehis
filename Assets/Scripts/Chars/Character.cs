using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Character : MonoBehaviour
{
    Vector2 moveDirection = Vector2.zero;

    public float moveSpeed = 7;
    public bool isMoving;
    public CharacterAnimator animator;

    private void Awake()
    {
        SetPositionAndSnap(transform.position);
    }

    public IEnumerator Move(Vector2 moveVector, Action OnMoveOver=null)
    {
        moveDirection = Vector2.zero;
        var targetPos = transform.position;

        if (moveVector.x != 0)
        { moveDirection.x = moveVector.x; }
        else if (moveVector.y != 0)
        { moveDirection.y = moveVector.y; }

        animator.moveX = Mathf.Clamp(moveDirection.x, -1, 1);
        animator.moveY = Mathf.Clamp(moveDirection.y, -1, 1);
        targetPos.x += moveDirection.x;
        targetPos.y += moveDirection.y;

        if (IsPathClear(targetPos))
        {
            isMoving = true;
            animator.isMoving = isMoving;
            while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                yield return null;
            }
            SetPositionAndSnap(targetPos);

            isMoving = false; 
        }

        OnMoveOver?.Invoke();
    }

    public void HandleUpdate()
    {
        if (!isMoving)
        { animator.isMoving = false; }
    }

    public void SetPositionAndSnap(Vector2 pos)
    {
        pos.x = Mathf.Floor(pos.x) + 0.5f;
        pos.y = Mathf.Floor(pos.y) + 0.5f;

        transform.position = pos;
    }

    private bool IsPathClear(Vector3 targetPos)
    {
        var diff = targetPos - transform.position;
        var dir = diff.normalized;
        if (Physics2D.BoxCast(transform.position + dir, new Vector2(0.2f, 0.2f), 0f, dir, diff.magnitude - 1, GameLayers.i.WallLayer | GameLayers.i.InteractableLayer | GameLayers.i.PlayerLayer) == true)
        { return false; }
        else { return true; }
    }

    public void LookTowards(Vector3 targetPos)
    {
        var xDiff = Mathf.Floor(targetPos.x) - Mathf.Floor(transform.position.x);
        var yDiff = Mathf.Floor(targetPos.y) - Mathf.Floor(transform.position.y);

        if (xDiff == 0 || yDiff == 0) 
        {
            animator.moveX = Mathf.Clamp(xDiff, -1, 1);
            animator.moveY = Mathf.Clamp(yDiff, -1, 1);
        }
    }
}
