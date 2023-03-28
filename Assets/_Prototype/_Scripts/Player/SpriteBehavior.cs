using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteBehavior : MonoBehaviour
{
    [SerializeField] private Transform spriteTransform;
    [Header("Orientation")]
    [Tooltip("Always face front")]
    [SerializeField] private bool faceFront;
    [Tooltip("Always face camera")]
    [SerializeField] private bool faceCam;

    private Vector3 initialRotation;

    void Awake()
    {
        initialRotation = spriteTransform.rotation.eulerAngles;
    }

    void Update()
    {
        if (faceCam) spriteTransform.LookAt(2 * spriteTransform.position - Camera.main.transform.position);
        if (faceFront) spriteTransform.rotation = Quaternion.Euler(initialRotation);
    }
}
