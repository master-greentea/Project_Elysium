using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Compass : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private CameraSwitcher cameraSwitcher;
    private float targetRotationZ;
    
    
    void Update()
    {
        targetRotationZ = playerController.currentCameraDirection switch
        {
            CameraDirection.South => 270,
            CameraDirection.SouthWest => 225,
            CameraDirection.West => 180,
            CameraDirection.NorthWest => 135,
            CameraDirection.North => 90,
            CameraDirection.NorthEast => 45,
            CameraDirection.East => 0,
            CameraDirection.SouthEast => 315,
        };

        if (!cameraSwitcher.isDefault)
        {
            targetRotationZ = 270;
        }
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(-90, 0, targetRotationZ), 2f * Time.deltaTime);
    }
}
