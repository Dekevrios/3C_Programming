using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField]
    public CameraState cameraState;
    [SerializeField]
    private CinemachineCamera fpsCamera;
    [SerializeField]
    private CinemachineCamera tpsCamera;
    [SerializeField]
    private InputManager inputManager;

    private CinemachinePanTilt panTilt;
    private CinemachineOrbitalFollow orbitalFollow;

    void Awake()
    {
        panTilt = fpsCamera.GetComponent<CinemachinePanTilt>();
    }

    private void Start()
    {
        inputManager.OnChangePOV += SwitchCamera;
    }

    private void OnDestroy()
    {
        inputManager.OnChangePOV -= SwitchCamera;
    }

    public void SetTPSFieldOfView(float fieldOfView)
    {
        tpsCamera.Lens.FieldOfView = fieldOfView;
    }
    public void SetFPSClampedCamera(bool isClamped, Vector3 playerRotation)
    {
        if (isClamped)
        {
            panTilt.PanAxis.Wrap = false;
            panTilt.PanAxis.Range = new Vector2(playerRotation.y - 45f, playerRotation.y + 45f);

        }
        else
        {
            panTilt.PanAxis.Range = new Vector2(-180f, 180f);
            panTilt.PanAxis.Wrap = true;
        }
    }

    private void SwitchCamera()
    {
        if (cameraState == CameraState.ThirdPerson)
        {
            cameraState = CameraState.FirstPerson;
            tpsCamera.gameObject.SetActive(false);
            fpsCamera.gameObject.SetActive(true);
        }
        else
        {
            cameraState = CameraState.ThirdPerson;
            tpsCamera.gameObject.SetActive(true);
            fpsCamera.gameObject.SetActive(false);
        }

    }
}
