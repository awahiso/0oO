using UnityEngine;

namespace OrZero
{
    /// <summary>
    /// ゲーム全体の調整値をまとめた ScriptableObject（SPEC §4.1）。
    /// 値は Inspector で調整する。実行中はプログラムから書き換えない（エディタではアセットに保存されてしまうため）。
    /// 項目はタスクごとに必要になったものを足していく
    /// </summary>
    [CreateAssetMenu(fileName = "GameBalance", menuName = "OrZero/Game Balance")]
    public class GameBalanceData : ScriptableObject
    {
        // ===== 出題 =====
        [SerializeField, Min(1)] private int maxSameAnswerStreak = 4;   // 同じ答えを続けてよい最大回数（回）。SPEC §1.4 ルール5

        // ===== 制限時間 =====
        [SerializeField, Min(0.01f)] private float startSeconds = 60f;    // 開始時の残り時間（秒）
        [SerializeField, Min(0.01f)] private float maxSeconds = 99.99f;   // 残り時間の上限（秒）。表示を整数2桁に収めるため
        [SerializeField, Min(1)] private int extendInterval = 3;          // 何問正解するごとに延長するか（問）
        [SerializeField, Min(0f)] private float extendSeconds = 1.0f;     // 1回の延長で増やす秒数（秒）

        /// <summary>同じ答えを続けてよい最大回数（回）</summary>
        public int MaxSameAnswerStreak => maxSameAnswerStreak;

        /// <summary>開始時の残り時間（秒）</summary>
        public float StartSeconds => startSeconds;

        /// <summary>残り時間の上限（秒）</summary>
        public float MaxSeconds => maxSeconds;

        /// <summary>何問正解するごとに延長するか（問）</summary>
        public int ExtendInterval => extendInterval;

        /// <summary>1回の延長で増やす秒数（秒）</summary>
        public float ExtendSeconds => extendSeconds;

        /// <summary>
        /// Inspector で値を変えたときに、組み合わせのおかしな設定を警告する（エディタでのみ呼ばれる）
        /// </summary>
        private void OnValidate()
        {
            if (startSeconds > maxSeconds)
            {
                Debug.LogWarning($"GameBalanceData: 開始時の残り時間（{startSeconds}秒）が上限（{maxSeconds}秒）を超えています", this);
            }
        }
    }
}
