using System.Collections;
using UnityEngine;

public class Turret : MonoBehaviour
{
    public GameObject target;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 1.0f;
    public GameObject idleObject;
    public GameObject fireObject;

    private Vector3 firePointPos;
    private Animator idleAnim;

    void Start()
    {
        firePointPos = firePoint.position;
        idleAnim = idleObject.GetComponent<Animator>();
        idleAnim.speed = 0.208f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            //Debug.Log("Player!");
            target = collision.gameObject;
            InvokeRepeating("Shoot", 1, 1);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            //Debug.Log("");
            target = null;
            CancelInvoke("Shoot");
        }
    }

    private void Shoot()
    {
        StartCoroutine(ShootAnim(.5f));
        if (target != null && projectilePrefab != null)
        {
            Vector3 spawnPos = firePointPos;
            spawnPos.z = 0;
            GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

            Vector2 direction = (target.transform.position - firePointPos).normalized;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.Euler(0, 0, angle);

            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = direction * projectileSpeed;
            }
            //Debug.Log("Shot " + shotCount + target);  
        }
    }

    private IEnumerator ShootAnim(float duration)
    {
        idleObject.SetActive(false);
        fireObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        fireObject.SetActive(false);
        idleObject.SetActive(true);
    }
}
