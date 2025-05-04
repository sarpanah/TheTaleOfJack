using UnityEngine;

public class Box : MonoBehaviour
{
    public GameObject coinPrefab; // Assign in Inspector
    public Transform coinSpawnPoint; // Optional for offset
    private int destroyCounter = 0;

    void Update()
    {
        Debug.Log(destroyCounter);
    }

    public void BreakBox(Vector2 throwDirection)
    {
        SpawnCoin(throwDirection);
        Destroy(gameObject);
    }
    private void SpawnCoin(Vector2 direction)
    {
    GameObject coin = Instantiate(coinPrefab, coinSpawnPoint ? coinSpawnPoint.position : transform.position, Quaternion.identity);
    var coinScript = coin.GetComponent<Coin>();
    if (coinScript != null)
    {
        coinScript.Throw(direction);
    }
    }
}
