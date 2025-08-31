using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class LoadingManager : MonoBehaviour {
  [SerializeField] private Slider progressBar;
  [SerializeField] private TextMeshProUGUI percentText; // opcional
    [SerializeField] private string nextSceneName = "MenuScene"; // nombre de la escena a cargar

  void Start() {
    StartCoroutine(LoadAsync());
  }

  IEnumerator LoadAsync() {
    AsyncOperation op = SceneManager.LoadSceneAsync(nextSceneName);
    op.allowSceneActivation = false;

    while (!op.isDone) {
      // op.progress va de 0 a 0.9; para normalizar a 0–1:
      float prog = Mathf.Clamp01(op.progress / 0.9f);
      progressBar.value = prog;
      if (percentText != null) percentText.text = Mathf.RoundToInt(prog * 100) + "%";

      // si ya cargó completamente (llegó a 0.9), se activa la escena
      if (op.progress >= 0.9f) {
        op.allowSceneActivation = true;
      }

      yield return null;
    }
  }
}
