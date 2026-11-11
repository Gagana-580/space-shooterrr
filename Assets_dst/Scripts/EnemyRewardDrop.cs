
using UnityEngine;

public class EnemyRewardDrop : MonoBehaviour
{
    [Header("Reward Prefabs")]
    public GameObject healthPrefab;
    public GameObject shieldPrefab;
    public GameObject doubleBulletPrefab;

    [Header("Testing")]
    [Range(0, 3)]
    public int testReward = 0;

    // 0 = Random
    // 1 = Health
    // 2 = Shield
    // 3 = Double Bullet

    public void DropReward()
    {
        GameObject reward = null;

        if (testReward == 1)
            reward = healthPrefab;
        else if (testReward == 2)
            reward = shieldPrefab;
        else if (testReward == 3)
            reward = doubleBulletPrefab;
        else
        {
            GameObject[] rewards =
            {
                healthPrefab,
                shieldPrefab,
                doubleBulletPrefab
            };

            reward = rewards[Random.Range(0, rewards.Length)];
        }

        if (reward != null)
        {
            Instantiate(
                reward,
                transform.position,
                Quaternion.identity
            );

            Debug.Log("Dropped reward: " + reward.name);
        }
    }
}
