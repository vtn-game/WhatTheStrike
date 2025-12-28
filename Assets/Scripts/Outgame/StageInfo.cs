#nullable enable

using UnityEngine;

namespace WhatTheStrike.Outgame
{
    /// <summary>
    /// ステージ情報
    /// </summary>
    [System.Serializable]
    public class StageInfo
    {
        [SerializeField] private string sceneName = "";
        [SerializeField] private string displayName = "";
        [SerializeField] private Sprite? characterImage;
        [SerializeField] private bool isCleared;

        public string SceneName => sceneName;
        public string DisplayName => displayName;
        public Sprite? CharacterImage => characterImage;
        public bool IsCleared
        {
            get => isCleared;
            set => isCleared = value;
        }

        public StageInfo(string sceneName, string displayName, Sprite? characterImage = null)
        {
            this.sceneName = sceneName;
            this.displayName = displayName;
            this.characterImage = characterImage;
            this.isCleared = false;
        }
    }
}
