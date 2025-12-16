using UnityEngine;

namespace WhatTheStrike.FriendshipCombo
{
    /// <summary>
    /// 友情コンボの管理と実行
    /// </summary>
    public class FriendshipComboManager : MonoBehaviour
    {
        [Header("プレハブ")]
        [SerializeField] private Laser laserPrefab;
        [SerializeField] private EnergyCircle energyCirclePrefab;
        [SerializeField] private SpeedUp speedUpPrefab;
        [SerializeField] private Explosion explosionPrefab;

        // シングルトン
        private static FriendshipComboManager instance;
        public static FriendshipComboManager Instance => instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 友情コンボを実行
        /// </summary>
        public void ExecuteCombo(Player.Player owner, FriendshipComboType type)
        {
            FriendshipComboBase combo = null;

            switch (type)
            {
                case FriendshipComboType.Laser:
                    if (laserPrefab != null)
                        combo = Instantiate(laserPrefab, owner.transform.position, Quaternion.identity);
                    else
                        combo = CreateDefaultCombo<Laser>(owner);
                    break;

                case FriendshipComboType.EnergyCircle:
                    if (energyCirclePrefab != null)
                        combo = Instantiate(energyCirclePrefab, owner.transform.position, Quaternion.identity);
                    else
                        combo = CreateDefaultCombo<EnergyCircle>(owner);
                    break;

                case FriendshipComboType.SpeedUp:
                    if (speedUpPrefab != null)
                        combo = Instantiate(speedUpPrefab, owner.transform.position, Quaternion.identity);
                    else
                        combo = CreateDefaultCombo<SpeedUp>(owner);
                    break;

                case FriendshipComboType.Explosion:
                    if (explosionPrefab != null)
                        combo = Instantiate(explosionPrefab, owner.transform.position, Quaternion.identity);
                    else
                        combo = CreateDefaultCombo<Explosion>(owner);
                    break;

                case FriendshipComboType.None:
                default:
                    return;
            }

            if (combo != null)
            {
                combo.Execute(owner);
            }
        }

        /// <summary>
        /// デフォルトのコンボオブジェクトを生成
        /// </summary>
        private T CreateDefaultCombo<T>(Player.Player owner) where T : FriendshipComboBase
        {
            var obj = new GameObject(typeof(T).Name);
            obj.transform.position = owner.transform.position;
            return obj.AddComponent<T>();
        }
    }
}
