using System;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
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

    AudioSource audioSource;
    bool isControllable = true;
    bool isCollidable = true;

    bool isInvinsible = false;

    bool hasShield = false;
    // I used righid body cache refrence here just to practice coding and do some crazy stuff in game 
    //  to incerease my coding skills 
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
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
            Debug.Log("shield absorbed the hit!");
            hasShield = false;
            shieldVisuals.SetActive(false);
            StartCoroutine(InvincibilityDelay());

            rb.freezeRotation = true;
            Vector3 bounceDirection = other.contacts[0].normal;
            rb.AddForce(bounceDirection * shieldBounceForce, ForceMode.Impulse);
            rb.freezeRotation = false;

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
        switch (other.tag) // Check tag of the collided object
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
        Destroy(powerUp);
    }

    void StartCrashSequence()
    {
        if (isInvinsible)
        {
            return;
        }

        else
        {
            isControllable = false;
            audioSource.Stop();
            crashParticles.Play();
            audioSource.PlayOneShot(CrashSFX);
            GetComponent<Movement>().enabled = false;
            // rb.useGravity = false;
            Invoke("ReloadLevel", levelLoadDelayTime);

        }


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
        isInvinsible = true;
        Debug.Log("Invincibility ON");

        yield return new WaitForSeconds(invincibilityTime);

        isInvinsible = false;
        Debug.Log("Invincibility OFF");
    }
}
