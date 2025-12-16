using UnityEngine;

namespace WhatTheStrike.Gimmicks
{
    /// <summary>
    /// 重力方向
    /// </summary>
    public enum GravityDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    /// <summary>
    /// 重力バリア - 通過すると速度が変化
    /// </summary>
    public class GravityBarrier : GimmickBase
    {
        [Header("重力バリア設定")]
        [SerializeField] private GravityDirection direction = GravityDirection.Down;
        [SerializeField] private float strength = 10f;

        // 方向ベクトルを取得
        private Vector2 DirectionVector
        {
            get
            {
                return direction switch
                {
                    GravityDirection.Up => Vector2.up,
                    GravityDirection.Down => Vector2.down,
                    GravityDirection.Left => Vector2.left,
                    GravityDirection.Right => Vector2.right,
                    _ => Vector2.zero
                };
            }
        }

        public override void OnPlayerEnter(Player.Player player)
        {
            // エントリーエフェクト
            PlayEnterEffect();
        }

        public override void OnPlayerStay(Player.Player player)
        {
            if (player.State != Player.PlayerState.Moving) return;

            // プレイヤーに重力を加える
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForce(DirectionVector * strength, ForceMode2D.Force);
            }
        }

        public override void OnPlayerExit(Player.Player player)
        {
            // 出口エフェクト
            PlayExitEffect();
        }

        private void PlayEnterEffect()
        {
            // 侵入エフェクト
        }

        private void PlayExitEffect()
        {
            // 脱出エフェクト
        }

        private void OnDrawGizmos()
        {
            // エディタで方向を表示
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, transform.localScale);

            // 矢印で方向を示す
            Vector3 dir = DirectionVector;
            Gizmos.DrawLine(transform.position, transform.position + dir * 2f);
        }
    }
}
