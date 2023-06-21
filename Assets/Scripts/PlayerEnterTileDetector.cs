using System;
using UnityEngine;

public class PlayerEnterTileDetector : MonoBehaviour
{
    public event Action<GameObject> OnPlayerEnterTile;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnPlayerEnterTile?.Invoke(gameObject);
        }
    }
}