
using UnityEngine;

public class RewardPickup : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float pickupDistance = 0.4f;

    [Header("Reward")]
    public string rewardName = "Health";

    private Transform player;

    void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    void Update()
    {
        if (player == null)
            return;

        // Move toward the player
        transform.position = Vector3.MoveTowards(
            transform.position,
            player.position,
            moveSpeed * Time.deltaTime
        );

        // Check whether the reward reached the player
        if (Vector2.Distance(
            transform.position,
            player.position) <= pickupDistance)
        {
            Debug.Log(rewardName + " collected!");

            // Visual pickup for now
            Destroy(gameObject);
        }
    }
}
