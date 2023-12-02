using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    // input
    private PrototypePlayerInput input;
    public bool isDefault;
    [SerializeField] private GameObject topDown;
    [SerializeField] private GameObject defaultCam;
    [SerializeField] private SpriteBehavior spriteBehavior;

    void OnEnable()
    {
        input.Player.Enable();
    }
    void OnDisable()
    {
        input.Player.Disable();
    }
    
    void Awake()
    {
        input = new PrototypePlayerInput();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.IsGamePaused) return;
        
        if (input.Player.SwitchCamera.WasPressedThisFrame())
        {
            isDefault = !isDefault;
            topDown.SetActive(!isDefault);
            defaultCam.SetActive(isDefault);
        }

        spriteBehavior.facing = isDefault ? SpriteFacing.FaceCam : SpriteFacing.FaceTop;
    }
}
