using System;

namespace WhatTheStrike.Domain.ValueObjects
{
    /// <summary>
    /// HPを表す値オブジェクト
    /// </summary>
    public readonly struct Health : IEquatable<Health>
    {
        public int Current { get; }
        public int Max { get; }

        public Health(int max) : this(max, max) { }

        public Health(int current, int max)
        {
            Max = max > 0 ? max : 1;
            Current = Math.Clamp(current, 0, Max);
        }

        public bool IsAlive => Current > 0;
        public bool IsDead => Current <= 0;
        public float Ratio => (float)Current / Max;

        public Health TakeDamage(int damage)
        {
            return new Health(Current - damage, Max);
        }

        public Health Heal(int amount)
        {
            return new Health(Current + amount, Max);
        }

        public bool Equals(Health other) => Current == other.Current && Max == other.Max;
        public override bool Equals(object? obj) => obj is Health other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Current, Max);
        public static bool operator ==(Health left, Health right) => left.Equals(right);
        public static bool operator !=(Health left, Health right) => !(left == right);
        public override string ToString() => $"{Current}/{Max}";
    }
}
