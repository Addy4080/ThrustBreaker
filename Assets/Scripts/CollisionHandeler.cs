using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

public class CollisionHandeler : MonoBehaviour
{
    [SerializeField] float levelLoadDelayTime = 0f;
    [SerializeField] AudioClip CrashSFX;
    [SerializeField] AudioClip SuccessSFX;
    [SerializeField] ParticleSystem successParticles;
    [SerializeField] ParticleSystem crashParticles;
    [SerializeField] GameObject shieldVisuals;
    [SerializeField] float invincibilityTime = 1f;
    [SerializeField] float shieldBounceForce = 0.5f;
    [SerializeField] Transform cameraTransform;
    [SerializeField] float freezeDuration = 0.5f; // adjustable in Inspector

    AudioSource audioSource;
    bool isControllable = true;
    bool isCollidable = true;
    bool isInvincible = false;   // FIX: renamed spelling to match
    bool hasShield = false;
    Rigidbody rb;
    Movement movementScript;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        movementScript = GetComponent<Movement>();
    }

    void Update()
    {
        RespondToDebugKeys();
    }

    private void RespondToDebugKeys()
    {
        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            NextLevel();
        }
        else if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            isCollidable = !isCollidable;
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (!isControllable || !isCollidable) { return; }

        if (hasShield && other.gameObject.tag != "Friendly" && other.gameObject.tag != "Finish" && other.gameObject.tag != "Shield")
        {
            Debug.Log("Shield absorbed the hit!");

            // Turn off shield
            hasShield = false;
            shieldVisuals.SetActive(false);

            // Start invincibility (FIX: will now end properly)
            // Start invincibility (ends properly)
                StartCoroutine(InvincibilityDelay());

                // --- MINIMAL FIX: bounce only along WORLD X, but choose left/right from LOCAL X ---
                // Stop current movement
            rb.velocity = Vector3.zero;          
            rb.angularVelocity = Vector3.zero;  

            // Bounce along local X axis
            Vector3 bounce = transform.right * shieldBounceForce;
            rb.AddForce(bounce, ForceMode.Impulse);

            // Freeze Y/Z movement and all rotation
            rb.constraints = RigidbodyConstraints.FreezePositionY | 
                            RigidbodyConstraints.FreezePositionZ | 
                            RigidbodyConstraints.FreezeRotation;


                // Disable player control
                isControllable = false;
                if (movementScript != null) movementScript.FreezeControls(freezeDuration);

                // Camera shake
                StartCoroutine(CameraShake(0.2f, 0.1f));

                // Restore after short delay
                StartCoroutine(RestoreControl());
                return;

        }

        switch (other.gameObject.tag)
        {
            case "Friendly":
                Debug.Log("hi friend");
                break;
            case "Finish":
                SuccessSequence();
                break;
            case "Shield":
                Debug.Log("hallo");
                break;
            default:
                StartCrashSequence();
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case "Shield":
                ShieldSequence(other.gameObject);
                break;
            default:
                break;
        }
    }

    private void ShieldSequence(GameObject powerUp)
    {
        hasShield = true;
        shieldVisuals.SetActive(true);

        if (movementScript != null)
        {
            movementScript.PlayShieldActivatedSound(); // ✅ renamed from bubble
        }

        Destroy(powerUp);
    }

    void StartCrashSequence()
    {
        if (isInvincible) return;  // FIX: was staying true forever
        isControllable = false;
        audioSource.Stop();
        crashParticles.Play();
        audioSource.PlayOneShot(CrashSFX);
        GetComponent<Movement>().enabled = false;
        Invoke("ReloadLevel", levelLoadDelayTime);
    }

    void SuccessSequence()
    {
        isControllable = false;
        audioSource.Stop();
        audioSource.PlayOneShot(SuccessSFX);
        successParticles.Play();
        GetComponent<Movement>().enabled = false;
        Invoke("NextLevel", levelLoadDelayTime);
    }

    private void NextLevel()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        int nextScene = currentScene + 1;
        if (nextScene == SceneManager.sceneCountInBuildSettings)
        {
            nextScene = 0;
        }
        SceneManager.LoadScene(nextScene);
    }

    private void ReloadLevel()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentScene);
    }

    IEnumerator InvincibilityDelay()
    {
        isInvincible = true;
        Debug.Log("Invincibility ON");
        yield return new WaitForSeconds(invincibilityTime);
        isInvincible = false;
        Debug.Log("Invincibility OFF");
    }

    IEnumerator CameraShake(float duration, float magnitude)
    {
        Vector3 originalPos = cameraTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            float y = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            cameraTransform.localPosition = originalPos + new Vector3(x, y, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        cameraTransform.localPosition = originalPos;
    }

    IEnumerator RestoreControl()
    {
        yield return new WaitForSeconds(freezeDuration);
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        isControllable = true;
    }
}
