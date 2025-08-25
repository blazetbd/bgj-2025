using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerContoller : MonoBehaviour
{
    public float speed = 10.0f;
    public float horizontalInput;
    public float jumpForce = 5.0f;
    public float waxLevel = 100.0f;
    private float waxLimit = 100.0f;
    public float waxTickRate = -1.0f;
    public int maxJumps = 2;
    private int jumpsLeft;
    private Rigidbody2D playerRb;
    public TextMeshProUGUI waxLevelText;

    void Start()
    {
        jumpsLeft = maxJumps;
        playerRb = GetComponent<Rigidbody2D>();
        InvokeRepeating("WaxTick", 1, -waxTickRate);
    }

    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        transform.Translate(Vector2.right * horizontalInput * Time.deltaTime * speed);

        if (Input.GetKeyDown(KeyCode.Space) && jumpsLeft > 0)
        {
            playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 0f); 
            playerRb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpsLeft--;
        }

        waxLevelText.text = "Wax %: " + waxLevel;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            jumpsLeft = maxJumps;
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
            waxTickRate = -1.0f;
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
