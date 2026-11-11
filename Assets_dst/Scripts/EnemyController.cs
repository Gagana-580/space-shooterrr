
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
        uiManager = FindFirstObjectByType<UIManager>();
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
            GameObject player =
                GameObject.FindGameObjectWithTag("Player");

            if (player == null)
                return;

            GameObject bullet = Instantiate(
                enemyFireballPrefab,
                firePoint.position,
                Quaternion.identity
            );

            Vector3 directionToPlayer =
                (player.transform.position - firePoint.position).normalized;

            Projectile projectileScript =
                bullet.GetComponent<Projectile>();

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
                AudioSource.PlayClipAtPoint(
                    hitSound,
                    transform.position
                );
            }

            Destroy(other.gameObject);
            TakeDamage(1);
        }
        else if (other.CompareTag("Player"))
        {
            PlayerController player =
                other.GetComponent<PlayerController>();

            if (player != null)
            {
                player.TakeDamage();
            }

            TakeDamage(health);
        }
    }

    void TakeDamage(int damageAmount)
    {
        if (health <= 0)
            return;

        health -= damageAmount;

        Debug.Log("Enemy took damage! Health: " + health);

        if (health <= 0)
        {
            if (uiManager != null)
            {
                uiManager.AddScore(scoreValue);
            }

            Die();
        }
    }

    void Die()
    {
        Debug.Log("Enemy died!");

        EnemyRewardDrop rewardDrop =
            GetComponent<EnemyRewardDrop>();

        if (rewardDrop != null)
        {
            Debug.Log("Reward script found!");

            rewardDrop.DropReward();
        }
        else
        {
            Debug.LogWarning("Reward script NOT found!");
        }

        Destroy(gameObject);
    }
}
