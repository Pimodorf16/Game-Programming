using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objectSpawner : MonoBehaviour
{
    public float wait = 5f;
    public int summon = 1;
    public float summonLimit = 6f;
    public GameObject enemy;
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Spawn());
    }

    IEnumerator Spawn()
    {
        while (true)
        {
            yield return new WaitForSeconds(wait);

            if(wait > 2f)
            {
                wait = wait - 0.04f;
            }

            int randomSpawn = Random.Range(0, 10);
            float randomX = Random.Range(-7f, 8f);

            float randomRotate = Random.Range(0f, 360f);
            Quaternion rotation = Quaternion.Euler(0, 0, randomRotate);
            
            GameObject item;

            for(int i = 0; i<summon; i++)
            {
                item = Instantiate(enemy, new Vector2(randomX, summonLimit), rotation);

                int angularDirection;
                if (Random.Range(0, 2) == 0)
                {
                    angularDirection = -1;
                }
                else
                {
                    angularDirection = 1;
                }

                item.GetComponent<Rigidbody2D>().angularVelocity = angularDirection * Random.Range(50f, 200f);
            }
        }
    }
}
