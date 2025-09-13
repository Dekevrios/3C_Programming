using UnityEngine;

public class PlayerAudioManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource footstepSFX;

    [SerializeField]
    private AudioSource landingSFX;

    [SerializeField]
    private AudioSource punchSFX;

    [SerializeField]
    private AudioSource glideSFX;

    private void PlayFootstepSFX()
    {
        footstepSFX.volume = Random.Range(0.7f, 1f);
        footstepSFX.pitch = Random.Range(0.5f, 2.5f);
        footstepSFX.Play();
    }

    private void PlayLandingSFX()
    {
        landingSFX.volume = Random.Range(0.7f, 1f);
        landingSFX.pitch = Random.Range(0.5f, 2.5f);
        landingSFX.Play();
    }

    private void PlayPunchSFX()
    {
        punchSFX.volume = Random.Range(0.7f, 1f);
        punchSFX.pitch = Random.Range(0.5f, 1.5f);
        punchSFX.Play();
    }

    public void PlayGlideSfx()
    {
        glideSFX.Play();

    }

    public void StopGlideSFX()
    {
        glideSFX.Stop();
    }
}
