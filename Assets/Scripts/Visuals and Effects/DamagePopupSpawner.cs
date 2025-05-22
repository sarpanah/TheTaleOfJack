using UnityEngine;

public class DamagePopupSpawner : MonoBehaviour
{
    [SerializeField] private GameObject popupPrefab;
    [SerializeField] private Transform popupSpawnPoint;

    SkeletonEnemyHealthManager skeletonEnemyHealthManager;
    private void Start()
    {
        skeletonEnemyHealthManager = GetComponent<SkeletonEnemyHealthManager>();
        skeletonEnemyHealthManager.OnDamaged += SpawnPopup;
    }

    private void SpawnPopup(int damage)
    {
        Vector3 spawnPos = popupSpawnPoint ? popupSpawnPoint.position : transform.position;
        GameObject popupGO = Instantiate(popupPrefab, spawnPos, Quaternion.identity);
        popupGO.GetComponent<DamagePopup>()?.SetDamage(damage);
    }

    private void OnDestroy()
    {
        skeletonEnemyHealthManager.OnDamaged -= SpawnPopup;
    }
}
