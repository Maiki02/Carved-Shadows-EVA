using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TutorialController : MonoBehaviour
{
    public static TutorialController Instance;

    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TextMeshProUGUI tutorialText;

    private int currentStep = 0;
    private bool isTutorialActive = true;

    private bool hasInteracted = false; // Indica si el jugador ha interactuado con un objeto

    private bool cerroElInspect = false; // Indica si el jugador ha cerrado el inspect
    private bool algunaVezAbrioElInspect = false; // Indica si el jugador ha abierto el inspect al menos una vez


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Asegura que solo haya una instancia del tutorial
        }
    }

    void Start()
    {
        this.ResetTutorial();
        ShowTutorialText();
    }

    private void Update()
    {
        if (!isTutorialActive) return;

        switch (currentStep)
        {
            case 0:
                if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
                    AdvanceStep();
                break;
            case 1:
                if (cerroElInspect) AdvanceStep();
                break;
                /*case 2:
                    if (hasRotated) AdvanceStep();
                    break;*/
        }

        if(GameController.Instance.IsInspecting)
        {
            if (!algunaVezAbrioElInspect)
            {
                algunaVezAbrioElInspect = true;
            }
        }

        if (algunaVezAbrioElInspect && !GameController.Instance.IsInspecting)
        {
            cerroElInspect = true;
            this.FinishTutorial();
        }

    }

    /*public void NotifyInteraction()
    {
        //Si no está en tutorial o ya ha interactuado, no hacemos nada
        if (!isTutorialActive || hasInteracted) return;
        //Si el paso actual es 0, no hacemos nada (esperamos a que se mueva)
        if (currentStep == 0) return;

        hasInteracted = true;
    }*/


    private void AdvanceStep()
    {
        currentStep++;
        ShowTutorialText();
        AudioController.Instance.PlaySFX(AudioType.TutorialStep);

        if (currentStep >= 2)
        {
            this.FinishTutorial();
        }
    }

    private void ShowTutorialText()
    {
        switch (currentStep)
        {
            case 0: this.SetTextTutorial("Usa A | W | S | D para moverte"); break;
            case 1: this.SetTextTutorial("Interactúa con el objeto de la mesa presionando (E)"); break;
                //case 2: UIManager.Instance.Show("Gira el objeto con el mouse"); break;
        }
    }

    private void SetTextTutorial(string text)
    {
        tutorialText.text = text;
    }

    public void FinishTutorial()
    {
        isTutorialActive = false;
        tutorialPanel.SetActive(false);

        DialogController.Instance.ShowDialog("Tutorial Completo. Volviendo al menú...", 3f);
        StartCoroutine(this.RedirectToMenu());        

    }

    private IEnumerator RedirectToMenu()
    {
        yield return new WaitForSeconds(3f);
    }

    public void ResetTutorial()
    {
        currentStep = 0;
        isTutorialActive = true;
        hasInteracted = false;

        tutorialPanel.SetActive(true);
    }
}
