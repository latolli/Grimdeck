using UnityEngine;
using UnityEngine.InputSystem;

public class OrbitCamera : MonoBehaviour
{
    public Transform target;          // the player
    public float distance = 10f;
    public float minDistance = 4f;
    public float maxDistance = 20f;
    public float rotationSpeed = 150f;
    public float zoomSpeed = 5f;
    public float minPitch = 20f;
    public float maxPitch = 80f;
    public float combatSideDistance = 10f;
    public float combatHeight = 5f;
    public float stateTransitionDuration = 0.5f;

    private float yaw = 0f;
    private float pitch = 45f;
    private float stateTransitionTime;
    private Vector3 transitionStartPosition;
    private Quaternion transitionStartRotation;
    private bool hasInitialized;
    private GameState lastGameState;

    private CombatManager combatManager;

    // Camera position in combat
    private Vector3 combatCameraPosition;
    private Vector3 combatLookAtPosition;

    void Start()
    {
        combatManager = CombatManager.Instance;
        if (combatManager != null)
        {
            lastGameState = combatManager.gameState;
        }
    }

    void LateUpdate()
    {
        if (target == null || combatManager == null) return;

        if (combatManager.gameState != lastGameState)
        {
            transitionStartPosition = transform.position;
            transitionStartRotation = transform.rotation;
            stateTransitionTime = 0f;
            lastGameState = combatManager.gameState;
        }

        if (combatManager.gameState == GameState.Free)
        {
            // Rotate with middle-click drag
            if (Mouse.current != null && Mouse.current.middleButton.isPressed)
            {
                Vector2 mouseDelta = Mouse.current.delta.ReadValue();
                yaw += mouseDelta.x * rotationSpeed * Time.deltaTime * 0.1f;
                pitch -= mouseDelta.y * rotationSpeed * Time.deltaTime * 0.1f;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            }

            // Zoom with scroll wheel
            float scroll = Mouse.current != null ? Mouse.current.scroll.ReadValue().y : 0f;
            distance -= scroll * zoomSpeed * Time.deltaTime;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        Vector3 desiredPosition;
        Vector3 lookAtPosition = combatManager.gameState == GameState.Free
            ? target.position + Vector3.up * 1.5f
            : combatLookAtPosition;

        if (combatManager.gameState == GameState.Free)
        {
            Quaternion orbitRotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 offset = orbitRotation * new Vector3(0f, 0f, -distance);
            desiredPosition = target.position + offset;
        }
        else
        {
            desiredPosition = combatCameraPosition;
        }

        Quaternion desiredRotation = //Quaternion.Euler(45f, 0f, 0f);
            Quaternion.LookRotation(lookAtPosition - desiredPosition, Vector3.up);

        if (!hasInitialized)
        {
            transform.SetPositionAndRotation(desiredPosition, desiredRotation);
            hasInitialized = true;
            return;
        }

        if (stateTransitionDuration <= 0f)
        {
            transform.SetPositionAndRotation(desiredPosition, desiredRotation);
            return;
        }

        stateTransitionTime += Time.deltaTime;
        float transitionProgress =
            Mathf.Clamp01(stateTransitionTime / stateTransitionDuration);
        float easedProgress = Mathf.SmoothStep(0f, 1f, transitionProgress);
        transform.position = Vector3.Lerp(transitionStartPosition, desiredPosition, easedProgress);
        transform.rotation = Quaternion.Slerp(
            transitionStartRotation,
            desiredRotation,
            easedProgress);
    }

    public void SetCombatCameraPosition(Vector3 playerPosition, Vector3 combatCenter)
    {
        // Find out final camera position for the combat
        Vector3 finalCameraPosition = combatCenter;
        Vector3 diff = combatCenter - playerPosition;
        if (diff.x > 0.0)
        {
            finalCameraPosition += new Vector3(0, 10, -6);
        }
        else if (diff.x < 0.0)
        {
            finalCameraPosition += new Vector3(0, 10, 6);
        }
        else if (diff.z > 0.0)
        {
            finalCameraPosition += new Vector3(-6, 10, 0);
        }
        else if (diff.z < 0.0)
        {
            finalCameraPosition += new Vector3(6, 10, 0);
        }
        combatCameraPosition = finalCameraPosition;
        combatLookAtPosition = combatCenter;
    }
}