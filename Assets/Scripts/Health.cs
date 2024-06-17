
using UnityEngine;

namespace DefaultNamespace
{
    public class Health : MonoBehaviour
    {
        [SerializeField] public int MaxHealth = 100;
        private float currentHealth;

        private void Start()
        {
            currentHealth = MaxHealth;
        }

      
    }
}