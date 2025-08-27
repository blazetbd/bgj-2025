using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
    public float defaultWaxTickRate = -2.0f;
    private float waxTickRate;
    public int maxJumps = 2;
    public float fallMultiplier = 2.5f;

    private int jumpsLeft;
    private bool isDashing = false;
    private bool isJumping = false;
    private bool isLanding = false;
    private float dashTimer;
    private float nextDashTime;
    private Rigidbody2D playerRb;

    public TextMeshProUGUI waxLevelText;
    public GameObject mainCamera;
    public GameObject anims;
    public GameObject runAnim;
    public GameObject idleAnim;
    public GameObject jumpStartAnim;
    public GameObject jumpEndAnim;
    public GameObject landAnim;

    void Start()
    {
        jumpStartAnim.GetComponent<Animator>().speed = 0.75f;
        nextDashTime = 0f;
        waxTickRate = defaultWaxTickRate;
        mainCamera = GameObject.Find("Main Camera");
        jumpsLeft = maxJumps;
        playerRb = GetComponent<Rigidbody2D>();
        InvokeRepeating("WaxTick", 1, 1);
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        if (isLanding) return;


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


        waxLevelText.text = "Wax %: " + waxLevel;
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f)
            {
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
            waxTickRate = 10.0f;
            CancelInvoke("WaxTick");
            InvokeRepeating("WaxTick", .2f, .2f);
        }
        else if (collision.gameObject.CompareTag("Projectile"))
        {
            waxLevel -= 5;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("WaxPile"))
        {
            waxTickRate = defaultWaxTickRate;
            CancelInvoke("WaxTick");
            InvokeRepeating("WaxTick", 1, 1);
        }
    }

    private void WaxTick()
    {
        if (waxTickRate > 0)
        {
            waxLevel = Mathf.Min(waxLevel + waxTickRate, waxLimit);
        }
        else
        {
            waxLevel = Mathf.Max(waxLevel + waxTickRate, 0);
        }
    }
    private void PlayJumpAnimation()
    {
        runAnim.SetActive(false);
        idleAnim.SetActive(false);
        jumpEndAnim.SetActive(false);

        jumpStartAnim.SetActive(false);
        jumpStartAnim.SetActive(true);
    }

    private void PlayAnimation(GameObject anim)
    {
        runAnim.SetActive(false);
        idleAnim.SetActive(false);
        jumpStartAnim.SetActive(false);
        jumpEndAnim.SetActive(false);

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
