using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class UIInputBinder : MonoBehaviour
{
    [Header("Opcional: referencias (se autocompletan)")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private InputSystemUIInputModule uiModule;

    [Header("Nombres en tu .inputactions")]
    [SerializeField] private string uiMapName = "UI";
    [SerializeField] private string aNavigate = "Navigate";
    [SerializeField] private string aSubmit = "Submit";
    [SerializeField] private string aCancel = "Cancel";
    [SerializeField] private string aPoint = "Point";      // opcional si tenés cursor
    [SerializeField] private string aClick = "Click";      // idem
    [SerializeField] private string aScroll = "Scroll";     // idem

    void Awake()
    {
        if (!playerInput)
        {
            var tagged = GameObject.FindWithTag("Player");
            if (tagged && tagged.TryGetComponent(out PlayerInput piFromPlayer))
                playerInput = piFromPlayer;
            if (!playerInput)
                playerInput = FindAnyObjectByType<PlayerInput>();
        }

        if (!uiModule)
        {
            var es = EventSystem.current ? EventSystem.current : FindAnyObjectByType<EventSystem>();
            if (!es) { Debug.LogError("[UIInputBinder] No hay EventSystem en escena."); return; }
            uiModule = es.GetComponent<InputSystemUIInputModule>();
            if (!uiModule) uiModule = es.gameObject.AddComponent<InputSystemUIInputModule>();
        }

        if (playerInput && playerInput.actions)
            WireUIModule(playerInput.actions);
    }

    private void WireUIModule(InputActionAsset asset)
    {
        // Busca acciones en el mapa UI (o por nombre suelto si no especificaste mapa)
        var nav = asset.FindAction($"{uiMapName}/{aNavigate}", false) ?? asset.FindAction(aNavigate, false);
        var sub = asset.FindAction($"{uiMapName}/{aSubmit}", false) ?? asset.FindAction(aSubmit, false);
        var can = asset.FindAction($"{uiMapName}/{aCancel}", false) ?? asset.FindAction(aCancel, false);

        // Opcionales (mouse/pointer)
        var poi = asset.FindAction($"{uiMapName}/{aPoint}", false) ?? asset.FindAction(aPoint, false);
        var clk = asset.FindAction($"{uiMapName}/{aClick}", false) ?? asset.FindAction(aClick, false);
        var scr = asset.FindAction($"{uiMapName}/{aScroll}", false) ?? asset.FindAction(aScroll, false);

        // Asignación correcta para módulos que esperan InputActionReference
        if (nav != null) uiModule.move = InputActionReference.Create(nav);
        if (sub != null) uiModule.submit = InputActionReference.Create(sub);
        if (can != null) uiModule.cancel = InputActionReference.Create(can);

        if (poi != null) uiModule.point = InputActionReference.Create(poi);
        if (clk != null) uiModule.leftClick = InputActionReference.Create(clk);
        if (scr != null) uiModule.scrollWheel = InputActionReference.Create(scr);

        // Habilitar por las dudas
        nav?.Enable(); sub?.Enable(); can?.Enable();
        poi?.Enable(); clk?.Enable(); scr?.Enable();
    }

    // Llamá estos helpers desde tu flujo (pausa/menú) si lo necesitás:
    public void SwitchToUI() { if (playerInput) playerInput.SwitchCurrentActionMap(uiMapName); }
    public void SwitchToGameplay() { if (playerInput) playerInput.SwitchCurrentActionMap("Gameplay"); }
}
