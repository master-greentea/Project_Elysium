using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpriteBehavior : MonoBehaviour
{
    [Header("testing")]
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
