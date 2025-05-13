using UnityEngine;
using System.Collections.Generic;

public class Box : MonoBehaviour
{
    [SerializeField] private List<GameObject> spawnablePrefabs; // Editable list in Inspector
    [SerializeField] private Transform spawnPoint; // Optional for offset
    [SerializeField, Range(0f, 1f)] private float emptyBoxChance = 0.2f; // Chance for empty box
    private int destroyCounter = 0;

    public void BreakBox(Vector2 throwDirection)
    {
        SpawnItem(throwDirection);
        TriggerFeedbackEffects();
        Destroy(gameObject);
    }

    private void SpawnItem(Vector2 direction)
    {
        // Check for empty box
        if (Random.value < emptyBoxChance) return;

        // Return if no prefabs assigned
        if (spawnablePrefabs == null || spawnablePrefabs.Count == 0) return;

        // Select random prefab from list
        int randomIndex = Random.Range(0, spawnablePrefabs.Count);
        GameObject selectedPrefab = spawnablePrefabs[randomIndex];

        // Skip if prefab is null
        if (selectedPrefab == null) return;

        // Instantiate at spawn point or box position
        Vector3 spawnPosition = spawnPoint ? spawnPoint.position : transform.position;
        GameObject spawnedItem = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);

        // Try to get Coin component and throw if it exists
        var coinScript = spawnedItem.GetComponent<Coin>();
        if (coinScript != null)
        {
            coinScript.Throw(direction);
        }
    }

    private void TriggerFeedbackEffects()
    {
        // Camera shake
        if (CameraShakeManager.Instance != null)
        {
            CameraShakeManager.Instance.ShakeCamera(CameraShakeIntensity.Light);
        }
        else
        {
            Debug.LogWarning("CameraShakeManager not found in scene.");
        }

        // Vibration (light tier)
        if (AndroidHapticManager.Instance != null)
        {
            AndroidHapticManager.Instance.Vibrate(VibrationIntensity.Light);
        }
        else
        {
            Debug.LogWarning("VibrationManager not found in scene.");
        }
    }
}