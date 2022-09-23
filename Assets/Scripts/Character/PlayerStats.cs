using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    [SerializeField]
    private PlayerHealth playerHealth;

    public class Stat
    {
        private float min, max, value;

        public Stat(float min, float max, float initialValue)
        {
            this.min = min;
            this.max = max;
            this.value = initialValue;
        }
        public void increment(float value)
        {
            this.value += value;
            this.value = (this.value < min) ? min : (this.value > max) ? max : this.value;
        }

        public float Value => value;
    }

    public enum StatType
    {
        Health,
        Armor,
        Stamina,
        Speed,
        Strength
    }

    private Stat[] stats;

    private void Start()
    {
        stats = new Stat[Enum.GetNames(typeof(StatType)).Length];
        for (int i = 0; i < stats.Length; i++)
            stats[i] = new Stat(0, int.MaxValue, 10);
    }

    public Stat GetStat(StatType statType)
    {
        return stats[(int)statType];
    }
}
