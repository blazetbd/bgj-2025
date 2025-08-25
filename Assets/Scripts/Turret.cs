using UnityEngine;

public class Turret : MonoBehaviour
{
    public GameObject target;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 1.0f;
    private int shotCount = 0;
    void Start()
    {

    }


    void Update()
    {

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
        if (target != null && projectilePrefab != null)
        {
            shotCount += 1;
            Vector3 spawnPos = firePoint.position;
            spawnPos.z = 0;
            GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

            Vector2 direction = (target.transform.position - firePoint.position).normalized;

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
}
