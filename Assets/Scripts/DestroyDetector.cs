using UnityEngine;

public class DestroyDetector : MonoBehaviour
{
    private void OnDisable()
    {
        // Unity always disables an object and its components right before destroying it.
        // If the application is quitting or scene unloading, we ignore it.
        if (!gameObject.scene.isLoaded) return;

        Debug.LogWarning($"[{gameObject.name}] OnDisable called. Current activeSelf: {gameObject.activeSelf}. Stack trace:\n{System.Environment.StackTrace}", this);
    }

    private void OnDestroy()
    {
        if (!gameObject.scene.isLoaded) return;
        Debug.LogError($"[{gameObject.name}] OnDestroy reached.", this);
    }
}
