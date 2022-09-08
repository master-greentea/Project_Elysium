using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationChange : MonoBehaviour
{
    private string currentState;
    // change animation state
    public void ChangeAnimationState(Animator animator, string newState, bool directionBased, bool playerLeft)
    {
        // check where player is facing
        if (directionBased && playerLeft) newState += "L";
        // prevent changing to same state
        if (currentState == newState) return;
        // change state
        animator.Play(newState);
        currentState = newState;
    }
}

