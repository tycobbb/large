using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// the game
public class Game: MonoBehaviour {
    // -- lifecycle --
    void Update() {
        RunCommands();
    }

    // -- commands --
    /// run commands based on inputs
    void RunCommands() {
        var k = Keyboard.current;

        var r = k.rKey;
        var s = k.sKey;
        var ctrl = k.ctrlKey;

        if (ctrl.isPressed && r.wasPressedThisFrame) {
            Reset();
        }

        if (ctrl.isPressed && s.wasPressedThisFrame) {
            Screenshot();
        }
    }

    /// reset the current scene
    void Reset() {
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    /// take a screenshot of the scene
    void Screenshot() {
        var app = Application.productName.ToLower();
        ScreenCapture.CaptureScreenshot($"{app}.png");
    }
}
