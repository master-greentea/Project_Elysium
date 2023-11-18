using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VHSButton : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] public Button button;
    [SerializeField] private TextMeshProUGUI buttonTextMeshPro;
    [HideInInspector] public string buttonText;

    [Header("Alternative Button Nav")]
    private Navigation defaultNav;
    [HideInInspector] public bool isAltNav;
    [SerializeField] private bool isAltNavUp;
    [SerializeField] private Button altNavUpToButton;
    [SerializeField] private bool isAltNavDown;
    [SerializeField] private Button altNavDownToButton;

    private void Awake()
    {
        buttonText = buttonTextMeshPro.text;
        defaultNav = button.navigation;
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

        AlternativeNavigation();
    }

    private void AlternativeNavigation()
    {
        if (!isAltNav)
        {
            button.navigation = defaultNav;
            return;
        }
        var newNav = new Navigation();
        newNav.mode = Navigation.Mode.Explicit;
        newNav.selectOnDown = defaultNav.selectOnDown;
        newNav.selectOnUp = defaultNav.selectOnUp;
        if (isAltNavUp)
        {
            newNav.selectOnUp = altNavUpToButton;
        }
        if (isAltNavDown)
        {
            newNav.selectOnDown = altNavDownToButton;
        }
        button.navigation = newNav;
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
