using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Services.Animations
{
    public static class AnimationChange
    {
        // change animation state
        public static void ChangeAnimationState(Animator animator, string currentState, string newState, bool directionBased, bool playerLeft)
        {
            // check where player is facing
            if (directionBased && playerLeft) newState += "L";
            // prevent changing to same state
            if (currentState == newState) return;
            // change state
            animator.Play(newState);
        }
    }
}

