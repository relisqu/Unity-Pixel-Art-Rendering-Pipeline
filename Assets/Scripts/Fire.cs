using System;
using DefaultNamespace;
using UnityEngine;

public class Fire : MonoBehaviour
{
    private void OnParticleCollision(GameObject other)
    {
        if (other.TryGetComponent(out Enemy enemy))
        {
        }
    }
}