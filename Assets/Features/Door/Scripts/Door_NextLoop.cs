using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class Door_NextLoop : ObjectInteract
{
    [Header("Refs")]
    [SerializeField] private CinemachineCamera doorVCam;
    [SerializeField] private Transform doorEntryPoint;
    [SerializeField] private Transform lookTargetPoint; // solo referencia p/inspector

    [Header("Timings")]
    [SerializeField] private float blendSeconds = 2f;
    [SerializeField] private float moveSpeed = 2f;

    private bool isTransitioning;

    protected override void Awake()
    {
        if (doorVCam)
        {
            doorVCam.enabled  = false;
            doorVCam.Priority = 1;
        }

        if (!TryGetComponent<Collider>(out _))
        {
            var mf = GetComponent<MeshFilter>();
            if (mf != null)
            {
                var mc = gameObject.AddComponent<MeshCollider>();
                mc.convex = true;
            }
        }
    }

    public override void OnInteract()
    {
        if (!isTransitioning && doorVCam != null)
            StartCoroutine(Sequence());
    }

    private IEnumerator Sequence()
    {
        isTransitioning = true;
        GameFlowManager.Instance.SetTransitionStatus(true);

        var playerObj = GameObject.FindWithTag("Player");
        if (!playerObj) yield break;

        var player = playerObj.GetComponent<PlayerController>();
        if (!player) yield break;

        // Bloquear control y cámara del jugador
        player.SetControlesActivos(false);
        player.SetCamaraActiva(false);
        player.SetStatusCharacterController(false);

        // Activar la cámara de la puerta
        doorVCam.enabled  = true;
        doorVCam.Priority = 1000;   // que gane seguro

        // Dejar que el Brain la tome y luego esperar el blend
        yield return null;
        yield return new WaitForSeconds(blendSeconds);

        // Mover al jugador hasta el EntryPoint
        if (doorEntryPoint)
        {
            while (Vector3.Distance(playerObj.transform.position, doorEntryPoint.position) > 0.05f)
            {
                playerObj.transform.position = Vector3.MoveTowards(
                    playerObj.transform.position,
                    doorEntryPoint.position,
                    moveSpeed * Time.deltaTime
                );
                yield return null;
            }
        }

        yield return new WaitForSeconds(0.2f);

        // Cambio de escena
        GameController.Instance.NextLevel();
        GameFlowManager.Instance.ActivatePreloadedScene();

        // Dormir la vcam para evitar rebotes
        doorVCam.Priority = 1;
        doorVCam.enabled  = false;
    }
}