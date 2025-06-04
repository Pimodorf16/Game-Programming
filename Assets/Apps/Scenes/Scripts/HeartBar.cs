using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartBar : MonoBehaviour
{
    public GameObject heartPrefab;
    public PlayerHealth playerHealth;
    List<HealthBar> hearts = new List<HealthBar>();

    private void OnEnable()
    {
        PlayerHealth.OnPlayerDamaged += DrawHearts;
    }

    private void OnDisable()
    {
        PlayerHealth.OnPlayerDamaged -= DrawHearts;
    }

    private void Start()
    {
        DrawHearts();
    }

    public void DrawHearts()
    {
        ClearHearts();

        float maxHealthRemainder = playerHealth.maxHealth % 4;
        int heartsToMake = (int)(playerHealth.maxHealth / 4 + maxHealthRemainder);
        for (int i = 0; i < heartsToMake; i++)
        {
            CreateEmptyHeart();
        }

        for(int i = 0; i < hearts.Count; i++)
        {
            int heartStatusRemainder = (int)Mathf.Clamp(playerHealth.health - (i * 4), 0, 4);
            hearts[i].SetHeartImage((HeartStatus)heartStatusRemainder);
        }
    }

    public void CreateEmptyHeart()
    {
        GameObject newHeart = Instantiate(heartPrefab);
        newHeart.transform.SetParent(transform);

        HealthBar heartComponent = newHeart.GetComponent<HealthBar>();
        heartComponent.SetHeartImage(HeartStatus.Empty);
        hearts.Add(heartComponent);
    }

    public void ClearHearts()
    {
        foreach(Transform t in transform)
        {
            Destroy(t.gameObject);
        }
        hearts = new List<HealthBar>();
    }
}
