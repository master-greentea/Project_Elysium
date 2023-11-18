using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VHSButton : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] public Button button;
    [SerializeField] private TextMeshProUGUI buttonTextMeshPro;
    [HideInInspector] public string buttonText;

    private void Awake()
    {
        buttonText = buttonTextMeshPro.text;
    }

    private void Update()
    {
        if (gameObject == EventSystem.current.currentSelectedGameObject)
        {
           SetText("> " + buttonText);
        }
        else
        {
            SetText(buttonText);
        }
        // button activation visual
        buttonTextMeshPro.color = button.enabled ? Color.white : Color.black;
    }
    
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        if (button.enabled)
        {
            button.Select();
        }
    }
    
    private void SetText(string text)
    {
        buttonTextMeshPro.text = text;
    }
}
