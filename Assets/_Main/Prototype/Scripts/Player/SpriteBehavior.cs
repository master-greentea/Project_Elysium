using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteBehavior : MonoBehaviour
{
    [Header("Orientation")]
    [Tooltip("Always face front")]
    [SerializeField] private bool faceFront;
    [Tooltip("Always face camera")]
    [SerializeField] private bool faceCam;

    private Vector3 initialRotation;

    void Awake()
    {
        initialRotation = transform.rotation.eulerAngles;
    }

    void Update()
    {
        if (faceCam) transform.LookAt(2 * transform.position - Camera.main.transform.position);
        if (faceFront) transform.rotation = Quaternion.Euler(initialRotation);
    }
}
