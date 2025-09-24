using System;
using UnityEngine;
using RPG.Utilities;


public class ManaComponent : MonoBehaviour
{
    [SerializeField]
    private float maxMana = 20f;
    public float MaxMana
    {
        get => maxMana;
        set
        {
            maxMana = value;
            CurrentMana = value;
        }
    }
    public float CurrentMana
    {
        get; set;
    }

    public event Action ManaChanged;

    private void Awake()
    {
        CurrentMana = MaxMana;
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
            CurrentMana = Mathf.Min(CurrentMana + amount, MaxMana);
            ManaChanged?.Invoke();
        }
    }
    public void RestoreFull()
    {
        Restore(MaxMana);
    }
    public void Reduce(float amount)
    {
        CurrentMana = Mathf.Max(CurrentMana - amount, 0);
        ManaChanged?.Invoke();
    }


}



