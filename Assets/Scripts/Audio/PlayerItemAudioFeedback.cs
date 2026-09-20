using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class PlayerItemAudioFeedback :
    MonoBehaviour
{
    [Header("Item Handling")]
    [SerializeField]
    private AudioClip looseItemPickupClip;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource =
            GetComponent<AudioSource>();
    }

    public void PlayLooseItemPickup()
    {
        if (audioSource == null ||
            looseItemPickupClip == null)
        {
            return;
        }

        audioSource.PlayOneShot(
            looseItemPickupClip
        );
    }
}