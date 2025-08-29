using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerContoller : MonoBehaviour
{
    public float speed = 10.0f;
    public float horizontalInput;
    public float jumpForce = 5.0f;
    public float dashForce = 20f;
    public float dashDuration = .2f;
    public float dashCooldown = 1f;
    public float waxLevel = 100.0f;
    private float waxLimit = 100.0f;
    public float defaultWaxTickRate = -10.0f;
    private float waxTickRate;
    public int maxJumps = 2;
    public float fallMultiplier = 2.5f;
    public int lives = 3;

    private int jumpsLeft;
    private bool isDashing = false;
    private bool isJumping = false;
    private bool isLanding = false;
    private float dashTimer;
    private float nextDashTime;
    private Rigidbody2D playerRb;
    private Vector2 startPos;
    private Vector2 restartPos;
    private int ingredientCount = 0;
    public int ingredientCountNeeded = 1;
    
    public GameObject mainCamera;
    public GameObject anims;
    public GameObject runAnim;
    public GameObject idleAnim;
    public GameObject jumpStartAnim;
    public GameObject jumpEndAnim;
    public GameObject landAnim;
    public GameObject dashAnim;
    public GameObject doorObject;
    private Door door;
    public GameObject lifePrefab;
    public Transform livesPanel;
    public Slider waxBar;

    private List<GameObject> lifeIcons = new List<GameObject>();

    void Start()
    {
        doorObject = GameObject.Find("Door");
        mainCamera = GameObject.Find("Main Camera");
        livesPanel = GameObject.Find("LivesPanel").transform;
        waxBar = GameObject.Find("WaxBar").GetComponent<Slider>();
        jumpStartAnim.GetComponent<Animator>().speed = 0.75f;
        if (doorObject == null || mainCamera == null || livesPanel == null || waxBar == null)
        {
            Debug.Log("WARNING! OBJECT NOT FOUND! CHECK HIERARCHY");
        }
        else
        {
            door = doorObject.GetComponent<Door>();
        }
        startPos = transform.position;
        restartPos = startPos;
        nextDashTime = 0f;
        waxTickRate = defaultWaxTickRate;
        jumpsLeft = maxJumps;
        playerRb = GetComponent<Rigidbody2D>();
        UpdateLivesDisplay();
    }

    void Update()
    {
        //handle wax updates
        if (waxLevel >= 0 && waxLevel <= waxLimit)
        {
            waxLevel += waxTickRate * Time.deltaTime;
        }

        waxLevel = Mathf.Clamp(waxLevel, 0, waxLimit);
        waxBar.value = waxLevel;


        horizontalInput = Input.GetAxisRaw("Horizontal");
        if (isLanding || isDashing) return;

        //run and idle animations
        if (horizontalInput < -.1f)
        {
            if (!isJumping)
            {
                runAnim.SetActive(true);
                idleAnim.SetActive(false);
            }
            anims.transform.eulerAngles = new Vector3(transform.eulerAngles.x, 0f, transform.eulerAngles.z);
        }
        else if (horizontalInput > .1f)
        {
            if (!isJumping)
            {
                runAnim.SetActive(true);
                idleAnim.SetActive(false);
            }
            anims.transform.eulerAngles = new Vector3(transform.eulerAngles.x, 180f, transform.eulerAngles.z);
        }
        else
        {
            if (!isJumping)
            {
                runAnim.SetActive(false);
                idleAnim.SetActive(true);
            }
        }


        //jump logic
        if (Input.GetKeyDown(KeyCode.Space) && jumpsLeft > 0)
        {
            playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 0f);
            playerRb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpsLeft--;
            isJumping = true;

            PlayJumpAnimation();
        }

        //dash logic
        if (!isDashing && Time.time >= nextDashTime && Input.GetKeyDown(KeyCode.LeftShift) && horizontalInput != 0 && waxLevel >= 5)
        {
            isDashing = true;
            dashTimer = dashDuration;
            playerRb.linearVelocity = new Vector2(horizontalInput * dashForce, 0f);
        }
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            PlayAnimation(dashAnim);
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f)
            {
                waxLevel -= 5;
                dashAnim.SetActive(false);
                isDashing = false;
                nextDashTime = Time.time + dashCooldown;
            }
            return;
        }

        playerRb.linearVelocity = new Vector2(horizontalInput * speed, playerRb.linearVelocity.y);

        if (isLanding) return;

        if (playerRb.linearVelocity.y < -.1f && jumpsLeft < maxJumps)
        {
            PlayAnimation(jumpEndAnim);
        }

        if (playerRb.linearVelocity.y < 0)
        {
            playerRb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    void LateUpdate()
    {
        mainCamera.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, -10);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            jumpEndAnim.SetActive(false);
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    jumpsLeft = maxJumps;
                    isJumping = false;

                    PlayLandingAnimation(.2f);

                    break;
                }
            }
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("WaxPile"))
        {
            WaxPile waxPile = collision.gameObject.GetComponent<WaxPile>();

            if (waxPile.hasBeenTouched == false)
            {
                waxPile.hasBeenTouched = true;
                restartPos = collision.transform.position;
                Debug.Log("Checkpoint set");
            }
            else
            {
                Debug.Log("Checkpoint already set");
            }

            waxTickRate = 20.0f;
        }
        else if (collision.gameObject.CompareTag("Projectile"))
        {
            if (waxLevel >= 5)
            {
                waxLevel -= 5;
            }
            else
            {
                waxLevel = 0;
            }
        }
        else if (collision.gameObject.CompareTag("DeathArea"))
        {
            playerRb.linearVelocity = Vector2.zero;
            if (playerRb != null && lives != 0)
            {
                playerRb.position = restartPos;
                LoseLife();
            }
            else if (lives == 0)
            {
                playerRb.position = startPos;
                lives = 3;
                UpdateLivesDisplay();
            }
            Debug.Log("Moved player to " + restartPos);
        }
        else if (collision.gameObject.CompareTag("Ingredient"))
        {
            ingredientCount += 1;
            Destroy(collision.gameObject);
            if (ingredientCount == ingredientCountNeeded && door != null)
            {
                door.OpenDoor();
            }
        }
        else if (collision.gameObject.CompareTag("Door"))
        {
            if (ingredientCount == ingredientCountNeeded)
            {
                Debug.Log("End of Level! (logic here later)");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("WaxPile"))
        {
            waxTickRate = defaultWaxTickRate;
        }
    }

    public void UpdateLivesDisplay()
    {
        foreach (var icon in lifeIcons)
        {
            Destroy(icon);
        }
        lifeIcons.Clear();

        for (int i = 0; i < lives; i++)
        {
            GameObject life = Instantiate(lifePrefab, livesPanel);
            lifeIcons.Add(life);
        }
    }

    public void LoseLife()
    {
        if (lives > 0)
        {
            lives--;
            UpdateLivesDisplay();
        }
    }

    private void PlayJumpAnimation()
    {
        runAnim.SetActive(false);
        idleAnim.SetActive(false);
        jumpEndAnim.SetActive(false);
        dashAnim.SetActive(false);

        jumpStartAnim.SetActive(false);
        jumpStartAnim.SetActive(true);
    }

    private void PlayAnimation(GameObject anim)
    {
        runAnim.SetActive(false);
        idleAnim.SetActive(false);
        jumpStartAnim.SetActive(false);
        jumpEndAnim.SetActive(false);
        dashAnim.SetActive(false);

        anim.SetActive(true);
    }

    private void PlayLandingAnimation(float duration = .2f)
    {
        StartCoroutine(LandingRoutine(duration));
    }

    private IEnumerator LandingRoutine(float duration)
    {
        isLanding = true;
        runAnim.SetActive(false);
        idleAnim.SetActive(false);
        jumpStartAnim.SetActive(false);
        jumpEndAnim.SetActive(false);

        landAnim.SetActive(true);

        yield return new WaitForSeconds(duration);

        isLanding = false;
        landAnim.SetActive(false);
        if (Mathf.Abs(horizontalInput) > 0.1f)
            runAnim.SetActive(true);
        else
            idleAnim.SetActive(true);
    }
}
