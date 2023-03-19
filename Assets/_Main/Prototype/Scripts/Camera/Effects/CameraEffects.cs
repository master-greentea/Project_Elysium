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
    
    // rewind
    [Header("Rewind")]
    [SerializeField] private ScriptableRendererFeature rewindBlit;
    [SerializeField] private Material rewindMaterial;

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
        unscaledTimeMaterials[0].SetFloat("_contrast", GameManager.isGamePaused || GameManager.isGameEnded ? 22f : 100f);
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

    public void ToggleRewind(bool isOn)
    {
        if (isOn) rewindBlit.SetActive(true);
        StartCoroutine(EffectWeightShift(rewindMaterial, isOn, .1f, rewindBlit));
    }

    private IEnumerator EffectWeightShift(Material effectMaterial, bool isOn, float duration)
    {
        var timer = 0f;
        while (timer < duration)
        {
            effectMaterial.SetFloat("_Weight", Mathf.Lerp(effectMaterial.GetFloat("_Weight"), isOn? 1 : 0, timer / duration));
            timer += Time.deltaTime;
            yield return null;
        }
        effectMaterial.SetFloat("_Weight", isOn ? 1 : 0);
    }
    
    private IEnumerator EffectWeightShift(Material effectMaterial, bool isOn, float duration, ScriptableRendererFeature blit)
    {
        blit.SetActive(true);
        var timer = 0f;
        while (timer < duration)
        {
            effectMaterial.SetFloat("_Weight", Mathf.Lerp(effectMaterial.GetFloat("_Weight"), isOn? 1 : 0, timer / duration));
            timer += Time.deltaTime;
            yield return null;
        }
        effectMaterial.SetFloat("_Weight", isOn ? 1 : 0);
        blit.SetActive(isOn);
    }

    private void OnDisable()
    {
        foreach (var material in unscaledTimeMaterials)
        {
            material.SetFloat("_unscaledTime", 0);
        }
        unscaledTimeMaterials[0].SetFloat("_contrast", 100f);
        rewindBlit.SetActive(false);
    }
}
