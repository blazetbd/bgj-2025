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
    public float waxLevel = 100.0f;
    private float waxLimit = 100.0f;
    public float defaultWaxTickRate = -2.0f;
    private float waxTickRate;
    public int maxJumps = 2;
    public float fallMultiplier = 2.5f;

    private int jumpsLeft;
    private bool isDashing;
    private float dashTimer;
    private Rigidbody2D playerRb;

    public TextMeshProUGUI waxLevelText;
    public GameObject mainCamera;
    public GameObject anims;
    public GameObject runAnim;
    public GameObject idleAnim;

    void Start()
    {
        waxTickRate = defaultWaxTickRate;
        mainCamera = GameObject.Find("Main Camera");
        jumpsLeft = maxJumps;
        playerRb = GetComponent<Rigidbody2D>();
        InvokeRepeating("WaxTick", 1, -waxTickRate);
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        

        //run and idle animations
        if (horizontalInput < -.1f)
        {
            runAnim.SetActive(true);
            idleAnim.SetActive(false);
            anims.transform.eulerAngles = new Vector3(transform.eulerAngles.x, 0f, transform.eulerAngles.z);
        }
        else if (horizontalInput > .1f)
        {
            runAnim.SetActive(true);
            idleAnim.SetActive(false);
            anims.transform.eulerAngles = new Vector3(transform.eulerAngles.x, 180f, transform.eulerAngles.z);
        }
        else
        {
            runAnim.SetActive(false);
            idleAnim.SetActive(true);
        }

        //jump logic
        if (Input.GetKeyDown(KeyCode.Space) && jumpsLeft > 0)
        {
            playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 0f);
            playerRb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpsLeft--;
        }

        //dash logic
        if (Input.GetKeyDown(KeyCode.LeftShift) && waxLevel >= 5 && horizontalInput != 0)
        {
            isDashing = true;
            dashTimer = dashDuration;
            playerRb.linearVelocity = new Vector2(horizontalInput * dashForce, 0f);
            waxLevel -= 5;
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
            }
            return; // Skip normal movement while dashing
        }

        // Normal movement
        playerRb.linearVelocity = new Vector2(horizontalInput * speed, playerRb.linearVelocity.y);

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
            foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f) 
            {
                jumpsLeft = maxJumps; 
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
            InvokeRepeating("WaxTick", 1, -waxTickRate);
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
}
