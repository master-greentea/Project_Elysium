using UnityEngine;
using Cinemachine;

[ExecuteInEditMode]
[SaveDuringPlay]
// Lock cinemachine camera Y
public class LockCameraY : CinemachineExtension
{
    [Tooltip("Lock the camera's Y position to this value")]
    public float y = 6;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (stage == CinemachineCore.Stage.Body)
        {
            var pos = state.RawPosition;
            pos.y = y;
            state.RawPosition = pos;
        }
    }
}
