using WhatTheStrike.Domain.ValueObjects;

namespace WhatTheStrike.Domain.Services
{
    /// <summary>
    /// ダメージ計算のドメインサービス
    /// </summary>
    public static class DamageCalculator
    {
        /// <summary>
        /// 攻撃力と防御力からダメージを計算
        /// </summary>
        public static Damage Calculate(int attack, int defense = 0)
        {
            return new Damage(attack);
        }

        /// <summary>
        /// ダメージを適用して新しいHealthを返す
        /// </summary>
        public static Health ApplyDamage(Health health, Damage damage, int defense = 0)
        {
            int finalDamage = damage.CalculateFinal(defense);
            return health.TakeDamage(finalDamage);
        }
    }
}
