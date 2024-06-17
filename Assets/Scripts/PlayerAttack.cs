using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace DefaultNamespace
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAttack : MonoBehaviour
    {
        [SerializeField]private float MainAttackDelay;
        [SerializeField]private float OffAttackDelay;
        
        private Animator _animator;
        private bool _isAttacking;
        [SerializeField] private ParticleSystem FireParticles;
        private void Start()
        {
            _animator = GetComponent<Animator>();
        }

        
        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && !_isAttacking)
            {
                StartCoroutine(StartMainAttack());
            }
            else if (Input.GetMouseButton(1))
            {
                StartCoroutine(StartOffAttack());
                FireParticles.Play();
            }
            
        }

        public IEnumerator StartMainAttack()
        {
            _isAttacking = true;
            _animator.SetTrigger("Attack");
            yield return new WaitForSeconds(MainAttackDelay);
            _isAttacking = false;
        }
        public IEnumerator StartOffAttack()
        {
            if (_isAttacking) yield return null;
            _isAttacking = true;
            yield return new WaitForSeconds(OffAttackDelay);
            _isAttacking = false;
        }
    }
}