using System;

namespace WhatTheStrike.Domain.ValueObjects
{
    /// <summary>
    /// ダメージを表す値オブジェクト
    /// </summary>
    public readonly struct Damage : IEquatable<Damage>
    {
        public int RawValue { get; }

        public Damage(int value)
        {
            RawValue = Math.Max(0, value);
        }

        /// <summary>
        /// 防御力を考慮した最終ダメージを計算（最低1ダメージ保証）
        /// </summary>
        public int CalculateFinal(int defense)
        {
            return Math.Max(1, RawValue - defense);
        }

        public bool Equals(Damage other) => RawValue == other.RawValue;
        public override bool Equals(object? obj) => obj is Damage other && Equals(other);
        public override int GetHashCode() => RawValue.GetHashCode();
        public static bool operator ==(Damage left, Damage right) => left.Equals(right);
        public static bool operator !=(Damage left, Damage right) => !(left == right);
        public override string ToString() => $"Dmg:{RawValue}";
    }
}
