using System;
using UnityEngine;

namespace OrZero
{
    /// <summary>
    /// 残り時間を管理するクラス（SPEC §1.1）。
    /// 経過時間で減らし、0 で止めて時間切れにする。延長は上限つき。
    /// Unity の機能は SerializeField 属性しか使わないので、EditMode テストで確かめられる
    /// </summary>
    [Serializable]
    public class GameTimer
    {
        // ===== 実行時の状態（確認用に Inspector へ表示） =====
        [SerializeField] private float remainingSeconds;   // 残り時間（秒）。0 未満にはしない

        /// <summary>残り時間（秒）</summary>
        public float RemainingSeconds => remainingSeconds;

        /// <summary>時間切れかどうか（残り時間が 0 以下）</summary>
        public bool IsTimeUp => remainingSeconds <= 0f;

        /// <summary>
        /// 残り時間を開始時の秒数に戻す。プレイを始める前に必ず呼ぶ
        /// </summary>
        /// <param name="startSeconds">開始時の残り時間（秒。0 以上）</param>
        public void Reset(float startSeconds)
        {
            // マイナスから始めるのは設定ミスなので止める
            if (startSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(startSeconds), startSeconds, "開始時の残り時間は0以上にしてください");
            }

            remainingSeconds = startSeconds;
        }

        /// <summary>
        /// 経過時間の分だけ残り時間を減らす。0 より下には減らさない
        /// </summary>
        /// <param name="deltaSeconds">前のフレームからの経過時間（秒）</param>
        public void Tick(float deltaSeconds)
        {
            // 経過時間がマイナスになることは通常ないが、万一でも残り時間が増えないようにする
            remainingSeconds = Math.Max(0f, remainingSeconds - Math.Max(0f, deltaSeconds));
        }

        /// <summary>
        /// 残り時間を延長する。上限を超える分は切り捨てる
        /// </summary>
        /// <param name="seconds">延長する秒数（0 以上）</param>
        /// <param name="maxSeconds">残り時間の上限（秒）</param>
        public void Extend(float seconds, float maxSeconds)
        {
            // マイナスの延長は設定ミスなので止める
            if (seconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(seconds), seconds, "延長する秒数は0以上にしてください");
            }

            remainingSeconds = Math.Min(remainingSeconds + seconds, maxSeconds);
        }
    }
}
