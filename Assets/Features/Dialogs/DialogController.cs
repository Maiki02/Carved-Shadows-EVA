
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;




public class DialogController : MonoBehaviour
{
    public static DialogController Instance { get; private set; }

    [Header("Dialog Settings")]
    [SerializeField] private GameObject dialogContainer; // Asigna el Panel en el Inspector
    [SerializeField] private TextMeshProUGUI dialogText; // Asigna el Text en el Inspector

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        dialogContainer.SetActive(false);
    }

    /// Muestra un diálogo en pantalla durante 'duration' segundos.
    public void ShowDialog(string dialog, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(ShowCoroutine(dialog, duration));
    }

    private IEnumerator ShowCoroutine(string dialog, float duration)
    {
        dialogText.text = dialog;
        dialogContainer.SetActive(true);
        yield return new WaitForSeconds(duration);
        dialogContainer.SetActive(false);
    }


    /// <summary>
    /// Muestra una secuencia de diálogos, cada uno con su propio tiempo
    /// </summary>
    public void ShowDialogSequence(DialogData[] messages)
    {
        StopAllCoroutines();
        StartCoroutine(ShowSequenceCoroutine(messages));
    }


    private IEnumerator ShowSequenceCoroutine(DialogData[] messages)
    {
        dialogContainer.SetActive(true);
        foreach (var msg in messages)
        {
            dialogText.text = msg.dialogText;
            yield return new WaitForSeconds(msg.duration);
        }
        dialogContainer.SetActive(false);
    }
}
