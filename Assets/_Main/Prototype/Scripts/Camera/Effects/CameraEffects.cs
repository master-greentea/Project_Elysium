using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

public class CameraEffects : MonoBehaviour
{
    [SerializeField] private Volume vol;
    private DepthOfField dof;
    
    private float timeElapsed;
    [SerializeField] private float focusShiftGap;
    private bool doShift;

    [SerializeField] private Material[] unscaledTimeMaterials;

    void Start()
    {
        vol.profile.TryGet( out dof );
    }

    // Update is called once per frame
    void Update()
    {
        FocusShift();
        foreach (var material in unscaledTimeMaterials)
        {
            material.SetFloat("_unscaledTime", Time.unscaledTime);
        }
        unscaledTimeMaterials[0].SetFloat("_contrast", GameManager.isGamePaused ? 22f : 100f);
    }

    void FocusShift()
    {
        if (timeElapsed < focusShiftGap)
        {
            if (doShift)
            {
                if (timeElapsed < 1.8f) dof.focalLength.value = Mathf.Lerp(70f, 200f, timeElapsed / 1.8f);
                if (timeElapsed < 2.4f) dof.focalLength.value = Mathf.Lerp(200f, 50f, timeElapsed / 2.4f);
                if (timeElapsed < 0.8f) dof.focalLength.value = Mathf.Lerp(50f, 70f, timeElapsed / 0.8f);
            }
            timeElapsed += Time.deltaTime;
        }
        else
        {
            doShift = false;
            float rand = Random.Range(0f, 1f);
            if (rand < .2f) {doShift = true;}
            timeElapsed = 0;
        }
    }
}
