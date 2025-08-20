using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Movement : MonoBehaviour
{
    [SerializeField] InputAction Thrust;
    [SerializeField] InputAction Rotation;
    [SerializeField] float thrustStrength;
    [SerializeField] float rotationStrengh;

    Rigidbody rb;
    AudioSource audioSource;

    public bool isFrozen = false;

    [SerializeField] AudioClip shieldBurstSound;       // renamed from bubbleBurstSound
    [SerializeField] AudioClip rocketThrustSound;
    [SerializeField] AudioClip shieldActivatedSound;   // renamed from bubbleActivatedSound

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        Thrust.Enable();
        Rotation.Enable();
    }

    void FixedUpdate()
    {
        if (isFrozen) return;
        ProcessThrust();
        ProcessRotation();
    }

    private void ProcessThrust()
    {
        if (Thrust.IsPressed())
        {
            rb.AddRelativeForce(Vector3.up * thrustStrength * Time.fixedDeltaTime);

            if (!audioSource.isPlaying)
            {
                audioSource.clip = rocketThrustSound;
                audioSource.Play();
            }
        }
        else
        {
            if (audioSource.isPlaying && audioSource.clip == rocketThrustSound)
                audioSource.Stop();
        }
    }

    private void ProcessRotation()
    {
        float RotationInput = Rotation.ReadValue<float>();
        if (RotationInput < 0)
        {
            ApplyRotation(rotationStrengh);
        }
        else if (RotationInput > 0)
        {
            ApplyRotation(-rotationStrengh);
        }
    }

    private void ApplyRotation(float rotationStrength)
    {
        rb.freezeRotation = true;
        transform.Rotate(Vector3.forward * rotationStrength * Time.fixedDeltaTime);
        rb.freezeRotation = false;
    }

    public void FreezeControls(float duration)
    {
        isFrozen = true;
        audioSource.Stop();

        if (shieldBurstSound != null)
        {
            audioSource.PlayOneShot(shieldBurstSound);
        }

        Invoke(nameof(UnfreezeControls), duration);
    }

    private void UnfreezeControls()
    {
        isFrozen = false;
    }

    public void PlayShieldActivatedSound()
    {
        if (shieldActivatedSound != null)
        {
            audioSource.PlayOneShot(shieldActivatedSound);
        }
    }
}
