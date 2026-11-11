using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public Vector3 direction = Vector3.up; // Up for player, Down for enemies
    public float lifetime = 3f;

    void Start()
    {
        // Backup destruction so bullets don't clutter memory
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }
}