using UnityEngine;

public class AntiAircraftGunController : MonoBehaviour
{
    [SerializeField] private Transform pivotal;

    [SerializeField] private float minElevate;
    [SerializeField] private float maxElevate;

    private float elevationAngle;
    private float horizontalAngle;

    private Quaternion initialRotation;

    void Start()
    {
        initialRotation = pivotal.localRotation;
    }

    void Update()
    {
        MouseXMovement();
        MouseYMovement();
        CombinedXYRotation();
    }

    private void MouseYMovement()
    {
        float mouseY = Input.GetAxis("Mouse Y");
        elevationAngle += mouseY * -60f * Time.deltaTime;
        elevationAngle = Mathf.Clamp(elevationAngle, -maxElevate, -minElevate );
        pivotal.localRotation = initialRotation * Quaternion.Euler(elevationAngle, 0f, 0f);
    }

    private void MouseXMovement()
    {
        float mouseX = Input.GetAxis("Mouse X");
        horizontalAngle += mouseX * 80f * Time.deltaTime;
        pivotal.localRotation = initialRotation * Quaternion.Euler(0f, horizontalAngle, 0f);
    }

    private void CombinedXYRotation()
    {
        Quaternion mouseRotation = Quaternion.Euler(elevationAngle, horizontalAngle, 0f);
        pivotal.localRotation = initialRotation * mouseRotation;
    }
}
