using System;
using UnityEngine;

namespace OrZero
{
    /// <summary>
    /// 次に出題する答え（英字のO／数字の0）を抽選するクラス。
    /// 同じ答えが続きすぎると「そろそろ逆が来る」と読めてしまうため、
    /// 連続回数に上限を設ける（SPEC §1.4 公平性ルール5）。
    /// Unity の機能は SerializeField 属性しか使わないので、EditMode テストで確かめられる。
    /// </summary>
    [Serializable]
    public class AnswerPicker
    {
        // ===== 設定（Initialize で受け取る。確認用に Inspector へ表示） =====
        [SerializeField] private int maxSameAnswerStreak = 4;   // 同じ答えを続けてよい最大回数（回）

        // ===== 実行時の状態（確認用に Inspector へ表示） =====
        [SerializeField] private GlyphType lastAnswer = GlyphType.LetterO;   // 直前に出した答え
        [SerializeField] private int sameAnswerStreak;                         // 直前の答えが続いている回数（回）。0 はまだ1問も出していない状態

        // 抽選に使う乱数。System.Random は Unity がシリアライズできない型なので、
        // 例外的に SerializeField にせず NonSerialized で持つ（テストではシードを固定したものを渡す）
        [NonSerialized] private System.Random random;

        /// <summary>
        /// 抽選の準備をする。出題を始める前に必ず呼ぶ（呼び直すと連続回数もリセットされる）
        /// </summary>
        /// <param name="maxStreak">同じ答えを続けてよい最大回数（1以上）</param>
        /// <param name="randomSource">抽選に使う乱数（テストではシードを固定したものを渡す）</param>
        public void Initialize(int maxStreak, System.Random randomSource)
        {
            // 上限が1未満だと、どちらの答えも出せない状態になるので設定ミスとして止める
            if (maxStreak < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxStreak), maxStreak, "同じ答えの連続上限は1以上にしてください");
            }

            // 乱数がないと抽選できないので止める
            if (randomSource == null)
            {
                throw new ArgumentNullException(nameof(randomSource));
            }

            maxSameAnswerStreak = maxStreak;
            random = randomSource;

            // まだ1問も出していない状態に戻す（Inspector に残った値に左右されないように）
            lastAnswer = GlyphType.LetterO;
            sameAnswerStreak = 0;
        }

        /// <summary>
        /// 次の答えを抽選する。英字・数字は半々の確率で選び、
        /// 同じ答えが上限回数まで続いているときだけ、もう一方に切り替える
        /// </summary>
        /// <returns>次に出題する答え</returns>
        public GlyphType PickNext()
        {
            // ローカル変数は関数の先頭で宣言する
            GlyphType next;   // 今回の答え

            // Initialize を呼ばずに使うのは実装ミスなので、すぐ気づけるように止める
            if (random == null)
            {
                throw new InvalidOperationException("AnswerPicker.Initialize を先に呼んでください");
            }

            // 英字・数字を半々の確率で選ぶ
            next = random.Next(2) == 0 ? GlyphType.LetterO : GlyphType.DigitZero;

            // 同じ答えが上限まで続いているなら、もう一方に切り替える
            if (sameAnswerStreak >= maxSameAnswerStreak && next == lastAnswer)
            {
                next = GetOpposite(lastAnswer);
            }

            // 連続回数を更新する（同じ答えなら +1、違う答えなら 1 からやり直し）
            if (sameAnswerStreak > 0 && next == lastAnswer)
            {
                sameAnswerStreak++;
            }
            else
            {
                lastAnswer = next;
                sameAnswerStreak = 1;
            }

            return next;
        }

        /// <summary>
        /// もう一方の答えを返す（英字 ⇔ 数字）
        /// </summary>
        /// <param name="answer">元の答え</param>
        /// <returns>反対側の答え</returns>
        private static GlyphType GetOpposite(GlyphType answer)
        {
            return answer == GlyphType.LetterO ? GlyphType.DigitZero : GlyphType.LetterO;
        }
    }
}
