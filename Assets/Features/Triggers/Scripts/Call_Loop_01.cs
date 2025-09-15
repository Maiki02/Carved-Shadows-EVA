using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Call_Loop_01 : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Door doorToClose;
    [SerializeField] private PhoneClose phoneClose;

    [Header("Audio Configuration")]
    [SerializeField] private AudioClip phoneCallClip;

    [Header("Dialog Configuration")]
    [SerializeField] private PhoneMessages phoneMessages;

    [Header("Configuración")]
    [SerializeField] private float ringDuration = 10f;
    [SerializeField] private bool triggerOnce = true;

    [Header("Blackout")]
    [Tooltip("Controlador del corte de luz estilo cine.")]
    [SerializeField] private BlackoutController blackoutController;

    private bool hasTriggered = false;
    private bool callCompleted = false;
    private Coroutine phoneSequenceCoroutine;

    private void Awake()
    {
        var triggerCollider = GetComponent<Collider>();
        if (triggerCollider) triggerCollider.isTrigger = true;

        if (doorToClose != null)
            doorToClose.SetType(TypeDoorInteract.Close);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (callCompleted) return;
        if (!other.CompareTag("Player")) return;
        if (triggerOnce && hasTriggered) return;

        hasTriggered = true;
        StartCallSequence();
    }

    public void StartCallSequence()
    {
        if (phoneSequenceCoroutine != null)
            StopCoroutine(phoneSequenceCoroutine);

        phoneSequenceCoroutine = StartCoroutine(CallSequenceCoroutine());
    }

    public void StopCallSequence()
    {
        if (phoneSequenceCoroutine != null)
        {
            StopCoroutine(phoneSequenceCoroutine);
            phoneSequenceCoroutine = null;
        }
        if (phoneClose != null) phoneClose.StopRinging();
    }

    public void OnCallCompleted(bool isWithCall)
    {
        if (!isWithCall) return;
        callCompleted = true;
        StartCoroutine(PostCallSequence());
    }

    public DialogData[] GetCallDialogs()
    {
        return phoneMessages ? phoneMessages.GetPrimerLoopMessages() : GetHardcodedCallDialogs();
    }

    public AudioClip GetPhoneCallClip() => phoneCallClip;

    private DialogData[] GetHardcodedCallDialogs()
    {
        return new DialogData[]
        {
            new DialogData("Perdón por llamar a estas horas (dice suspirando)", 4f),
            new DialogData("Euri, yo...", 2f),
            new DialogData("También perdón por no avisar que nos fuimos.", 4f),
            new DialogData("Euri, hola ¿Me escuchas?", 3f),
            new DialogData("Es que te vimos durmiendo y no quisimos despertarte.", 5f),
            new DialogData("Hoy tampoco vamos a pasar la noche en casa,", 4f),
            new DialogData("creo que todavía necesitamos… Todavía necesito algo más de tiempo.", 6f),
            new DialogData("¿Estás hablando con papi?", 3f),
            new DialogData("Si hija, estoy hablando con papi.", 3f),
            new DialogData("Anda a acostarte que ya es tarde corazón.", 4f),
            new DialogData("Perdón Jorge, se por lo que estás pasando y todo lo que has hecho pero yo…", 6f),
            new DialogData("", 2f),
            new DialogData("Euri? Alice? Hola?", 3f)
        };
    }

    private DialogData[] GetFinalDialogs()
    {
        return new DialogData[]
        {
            new DialogData("Mierda, voy a tener que revisar la caja eléctrica…", 4f),
            new DialogData("aunque no quiera volver a bajar allí, detesto ese lugar.", 5f)
        };
    }

    private IEnumerator CallSequenceCoroutine()
    {
        if (phoneClose != null)
        {
            phoneClose.StartRinging();
            Debug.Log("[Call_Loop_01] Teléfono empezó a sonar");
        }

        yield return new WaitForSeconds(ringDuration);

        if (phoneClose != null)
        {
            phoneClose.StopRinging();
            Debug.Log("[Call_Loop_01] Tiempo de ring agotado - teléfono dejó de sonar");
        }
    }

    private IEnumerator PostCallSequence()
    {
        Debug.Log("[Call_Loop_01] Post-llamada: iniciando blackout…");

        // 1) Blackout cinematográfico (esperamos a que termine el flicker + apagado)
        if (blackoutController != null)
            yield return blackoutController.BlackoutRoutine();

        // 2) Diálogo del prota después del corte
        var finalDialogs = GetFinalDialogs();
        if (DialogController.Instance != null)
        {
            DialogController.Instance.ShowDialogSequence(finalDialogs);
            float total = 0f; foreach (var d in finalDialogs) total += d.duration;
            yield return new WaitForSeconds(total);
        }

        // 3) Habilitar puerta
        if (doorToClose != null)
        {
            doorToClose.SetType(TypeDoorInteract.OpenAndClose);
            Debug.Log("[Call_Loop_01] Puerta configurada para permitir interacción");
        }

        Debug.Log("[Call_Loop_01] Secuencia post-llamada completada");
    }

    public void FinishCall(bool isWithCall)
    {
        if (hasTriggered && isWithCall) OnCallCompleted(isWithCall);
    }

    public void ResetTrigger()
    {
        hasTriggered = false;
        callCompleted = false;
        Debug.Log("[Call_Loop_01] Trigger reseteado");
    }

    public bool IsCallCompleted => callCompleted;
    public float RingDuration => ringDuration;

    private void OnDrawGizmos()
    {
        var col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = callCompleted ? Color.green : (hasTriggered ? Color.red : Color.yellow);
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
    }
}
