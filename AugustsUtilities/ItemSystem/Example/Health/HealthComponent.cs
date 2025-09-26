using System;
using UnityEngine;
using RPG.Utilities;
namespace AugustsUtility.ItemSystem.Example
{

    public class HealthComponent : MonoBehaviour
    {
        [SerializeField]
        private float maxHealth = 20f;
        public float MaxHealth
        {
            get => maxHealth;
            set
            {
                maxHealth = value;
                CurrentHealth = value;
            }
        }
        public float CurrentHealth
        {
            get; set;
        }

        public event Action HealthChanged;
        public event Action Died;
        public event Action ReceivedDamage;

        private void Awake()
        {
            CurrentHealth = MaxHealth;
        }

        public void Restore(float amount)
        {
            if (amount < 0)
            {
                Debug.Log("Cannot Heal A Negative Amount");
                return;
            }
            else
            {
                CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
                HealthChanged?.Invoke();
            }
        }
        public void FullHeal()
        {
            Restore(MaxHealth);
        }
        public void Damaged(float amount)
        {
            CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
            HealthChanged?.Invoke();
            ReceivedDamage?.Invoke();
            CheckDeath();
        }
        private void CheckDeath()
        {
            if (CurrentHealth == 0)
            {
                Died?.Invoke();
                this.CallDeferred(() => { Destroy(gameObject); });

            }
        }


    }

}

