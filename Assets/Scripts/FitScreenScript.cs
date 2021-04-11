using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ControllerScript))]
public class FitScreenScript : MonoBehaviour {
#if UNITY_EDITOR
    private ControllerScript _controllerScript;

    private int _prevWidth;
    private int _prevHeight;

    private void Awake() {
        _controllerScript = GetComponent<ControllerScript>();
        _prevHeight = Screen.height;
        _prevWidth = Screen.width;
        StartCoroutine(ScreenSizeCheckRoutine());
    }

    IEnumerator ScreenSizeCheckRoutine() {
        while (true) {
            if (Screen.width != _prevWidth || Screen.height != _prevHeight) {
                Debug.Log("Window size changed, redrawing");
                _controllerScript.RedrawBoard();
                _prevHeight = Screen.height;
                _prevWidth = Screen.width;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
#endif
}