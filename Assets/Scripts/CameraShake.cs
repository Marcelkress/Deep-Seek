using Cinemachine;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private CinemachineVirtualCamera virtualCamera;
    private CinemachineBasicMultiChannelPerlin noise;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void StartShake(float amplitude)
    {
        noise.m_AmplitudeGain = amplitude;
    }

    public void StopShake()
    {
        noise.m_AmplitudeGain = 0;
    }
}
