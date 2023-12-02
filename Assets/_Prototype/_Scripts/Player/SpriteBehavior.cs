using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpriteFacing {
    FaceFront, FaceCam, FaceTop
}

public class SpriteBehavior : MonoBehaviour
{
    [SerializeField] private Transform spriteTransform;

    [Header("Orientation")]
    [SerializeField] public SpriteFacing facing;

    private Vector3 initialRotation;

    void Awake()
    {
        initialRotation = spriteTransform.rotation.eulerAngles;
    }

    void Update()
    {
        switch (facing)
        {
            case SpriteFacing.FaceCam:
                spriteTransform.LookAt(2 * spriteTransform.position - Camera.main.transform.position);
                break;
            case SpriteFacing.FaceFront:
                spriteTransform.rotation = Quaternion.Euler(initialRotation);
                break;
            case SpriteFacing.FaceTop:
                spriteTransform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
                break;
            default:
                return;
        }
    }
}
