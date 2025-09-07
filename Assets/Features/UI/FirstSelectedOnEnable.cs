using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class FirstSelectedOnEnable : MonoBehaviour
{
    [SerializeField] private GameObject firstSelected;

    void OnEnable()
    {
        if (!firstSelected) return;
        StartCoroutine(SelectNextFrame());
    }

    private IEnumerator SelectNextFrame()
    {
        yield return null;
        if (EventSystem.current)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelected);
        }
    }
}
