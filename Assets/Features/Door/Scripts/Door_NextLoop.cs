using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class Door_NextLoop : ObjectInteract
{
    [Header("Refs")]
    [SerializeField] private CinemachineCamera doorVCam; 
    [SerializeField] private Transform doorEntryPoint;
    [SerializeField] private Transform lookTargetPoint;

    [Header("Timings")]
    [SerializeField] private float blendSeconds = 2f;
    [SerializeField] private float moveSpeed = 2f;

    private bool isTransitioning = false;

    public override void OnInteract()
    {
        if (!isTransitioning) StartCoroutine(Sequence());
    }

    private IEnumerator Sequence()
    {
        isTransitioning = true;
        GameFlowManager.Instance.SetTransitionStatus(true);

        var playerObj = GameObject.FindWithTag("Player");
        if (!playerObj) yield break;

        var player = playerObj.GetComponent<PlayerController>();
        if (!player) yield break;

        player.SetControlesActivos(false);
        player.SetCamaraActiva(false);
        player.SetStatusCharacterController(false);

        if (doorVCam != null)
        {
            doorVCam.LookAt = lookTargetPoint;
            doorVCam.gameObject.SetActive(true);
            doorVCam.Priority = 20;
        }

        yield return new WaitForSeconds(blendSeconds);

        if (doorEntryPoint != null)
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

        GameController.Instance.NextLevel();
        GameFlowManager.Instance.ActivatePreloadedScene();
    }
}
