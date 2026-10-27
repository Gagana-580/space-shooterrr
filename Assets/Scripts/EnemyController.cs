using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int health = 3;
    public int scoreValue = 100;

    [Header("Shooting Settings")]
    public GameObject enemyFireballPrefab;
    public Transform firePoint;
    public float fireRate = 1.5f;
    private float nextFireTime;

    [Header("Audio Settings")]
    public AudioClip hitSound;

    private UIManager uiManager;

    void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
        nextFireTime = Time.time + fireRate;
    }

    void Update()
    {
        if (Time.time >= nextFireTime)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        nextFireTime = Time.time + fireRate;

        if (enemyFireballPrefab != null && firePoint != null)
        {
            // Find the player first
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            // If the player is dead (null), stop running this shooting code immediately
            if (player == null) return;

            // Otherwise, spawn the bullet and shoot at the player
            GameObject bullet = Instantiate(enemyFireballPrefab, firePoint.position, Quaternion.identity);
            Vector3 directionToPlayer = (player.transform.position - firePoint.position).normalized;

            Projectile projectileScript = bullet.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                projectileScript.direction = directionToPlayer;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerBullet"))
        {
            if (hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, transform.position);
            }

            Destroy(other.gameObject);
            TakeDamage(1);
        }
        else if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().TakeDamage();
            TakeDamage(health);
        }
    }

    void TakeDamage(int damageAmount)
    {
        health -= damageAmount;

        if (health <= 0)
        {
            if (uiManager != null) uiManager.AddScore(scoreValue);
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}