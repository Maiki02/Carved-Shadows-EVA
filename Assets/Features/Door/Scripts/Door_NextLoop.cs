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

        // Mover al jugador hasta el EntryPoint (SOLO EN XZ)
        if (doorEntryPoint)
        {
            float fixedY = playerObj.transform.position.y; // mantenemos la Y inicial del player

            // Target en el plano XZ con Y fija
            Vector3 targetXZ = new Vector3(doorEntryPoint.position.x, fixedY, doorEntryPoint.position.z);

            // Distancia horizontal (ignora Y)
            while (Vector2.Distance(
                       new Vector2(playerObj.transform.position.x, playerObj.transform.position.z),
                       new Vector2(doorEntryPoint.position.x, doorEntryPoint.position.z)) > 0.05f)
            {
                // Rotar suavemente hacia la puerta (solo yaw)
                Vector3 lookDir = doorEntryPoint.position - playerObj.transform.position;
                lookDir.y = 0f;
                if (lookDir.sqrMagnitude > 0.0001f)
                {
                    Quaternion lookRot = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
                    playerObj.transform.rotation = Quaternion.Slerp(playerObj.transform.rotation, lookRot, 10f * Time.deltaTime);
                }

                // Avanzar solo en XZ y clamplear Y
                Vector3 next = Vector3.MoveTowards(playerObj.transform.position, targetXZ, moveSpeed * Time.deltaTime);
                next.y = fixedY;
                playerObj.transform.position = next;

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
