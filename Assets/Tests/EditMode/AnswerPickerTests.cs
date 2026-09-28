using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace OrZero.Tests
{
    /// <summary>
    /// AnswerPicker（答えの抽選）のテスト（EditMode）
    /// SPEC §1.4 公平性ルール5「同じ答えの連続は最大4回」を守っているかを確かめる
    /// </summary>
    public class AnswerPickerTests
    {
        // ===== テストで共通に使う値 =====
        private const int DrawCount = 10000;   // 統計を見るときの抽選回数（回）
        private const int FixedSeed = 12345;   // 結果を再現するための固定シード
        private const int StreakLimit = 4;     // SPEC で決めた同じ答えの連続上限（回）

        /// <summary>
        /// 1万回抽選したとき、同じ答えの最長の連続がちょうど上限（4回）になる。
        /// 上限を超えないこと（ルールが効いている）と、上限まで続くこと（厳しくしすぎていない）の両方を確かめる
        /// </summary>
        [Test]
        public void PickNext_TenThousandDraws_LongestStreakEqualsLimit()
        {
            // ローカル変数は関数の先頭で宣言する
            AnswerPicker picker;       // テスト対象
            List<GlyphType> answers;   // 抽選結果
            int longestStreak;         // 最長の連続回数（回）

            // 準備: 上限4回・固定シードで初期化する
            picker = new AnswerPicker();
            picker.Initialize(StreakLimit, new System.Random(FixedSeed));

            // 実行: 1万回抽選して、最長の連続回数を数える
            answers = DrawMany(picker, DrawCount);
            longestStreak = GetLongestStreak(answers);

            // 確認: 最長の連続がちょうど上限と同じ
            Assert.AreEqual(StreakLimit, longestStreak);
        }

        /// <summary>
        /// 1万回抽選したとき、英字と数字がおおむね半々（英字の割合が 47〜53%）になる
        /// </summary>
        [Test]
        public void PickNext_TenThousandDraws_RatioIsAboutHalf()
        {
            // ローカル変数は関数の先頭で宣言する
            AnswerPicker picker;       // テスト対象
            List<GlyphType> answers;   // 抽選結果
            int letterCount;           // 英字が出た回数（回）
            double letterRatio;        // 英字の割合（0〜1）

            // 準備: 上限4回・固定シードで初期化する
            picker = new AnswerPicker();
            picker.Initialize(StreakLimit, new System.Random(FixedSeed));

            // 実行: 1万回抽選して、英字の割合を出す
            answers = DrawMany(picker, DrawCount);
            letterCount = answers.FindAll(answer => answer == GlyphType.LetterO).Count;
            letterRatio = (double)letterCount / answers.Count;

            // 確認: 英字の割合が 47〜53% に収まる
            Assert.That(letterRatio, Is.InRange(0.47, 0.53));
        }

        /// <summary>
        /// 上限を1回にすると、英字と数字が必ず交互に出る
        /// </summary>
        [Test]
        public void PickNext_LimitIsOne_AlwaysAlternates()
        {
            // ローカル変数は関数の先頭で宣言する
            AnswerPicker picker;       // テスト対象
            List<GlyphType> answers;   // 抽選結果
            int i;                     // ループ用の添字

            // 準備: 上限1回で初期化する
            picker = new AnswerPicker();
            picker.Initialize(1, new System.Random(FixedSeed));

            // 実行: 100回抽選する
            answers = DrawMany(picker, 100);

            // 確認: どの答えも、ひとつ前の答えと違う
            for (i = 1; i < answers.Count; i++)
            {
                Assert.AreNotEqual(answers[i - 1], answers[i], $"{i}回目が前の答えと同じ");
            }
        }

        /// <summary>
        /// 同じシードで初期化すれば、同じ並びの答えが出る（テストや不具合の再現に必要）
        /// </summary>
        [Test]
        public void PickNext_SameSeed_ReturnsSameSequence()
        {
            // ローカル変数は関数の先頭で宣言する
            AnswerPicker firstPicker;    // 1つ目のテスト対象
            AnswerPicker secondPicker;   // 2つ目のテスト対象（同じシード）

            // 準備: 同じ上限・同じシードで2つ初期化する
            firstPicker = new AnswerPicker();
            firstPicker.Initialize(StreakLimit, new System.Random(777));
            secondPicker = new AnswerPicker();
            secondPicker.Initialize(StreakLimit, new System.Random(777));

            // 実行・確認: 100回分の並びが一致する
            CollectionAssert.AreEqual(DrawMany(firstPicker, 100), DrawMany(secondPicker, 100));
        }

        /// <summary>
        /// Initialize を呼ばずに抽選すると、実装ミスとして例外になる
        /// </summary>
        [Test]
        public void PickNext_BeforeInitialize_ThrowsInvalidOperationException()
        {
            // ローカル変数は関数の先頭で宣言する
            AnswerPicker picker;   // テスト対象（Initialize しない）

            // 準備
            picker = new AnswerPicker();

            // 実行・確認
            Assert.Throws<InvalidOperationException>(() => picker.PickNext());
        }

        /// <summary>
        /// 連続上限に1未満を渡すと、設定ミスとして例外になる
        /// </summary>
        [Test]
        public void Initialize_LimitBelowOne_ThrowsArgumentOutOfRangeException()
        {
            // ローカル変数は関数の先頭で宣言する
            AnswerPicker picker;   // テスト対象

            // 準備
            picker = new AnswerPicker();

            // 実行・確認
            Assert.Throws<ArgumentOutOfRangeException>(() => picker.Initialize(0, new System.Random(FixedSeed)));
        }

        /// <summary>
        /// 乱数を渡さずに初期化すると、例外になる
        /// </summary>
        [Test]
        public void Initialize_NullRandom_ThrowsArgumentNullException()
        {
            // ローカル変数は関数の先頭で宣言する
            AnswerPicker picker;   // テスト対象

            // 準備
            picker = new AnswerPicker();

            // 実行・確認
            Assert.Throws<ArgumentNullException>(() => picker.Initialize(StreakLimit, null));
        }

        /// <summary>
        /// 指定した回数だけ抽選して、結果を並べて返す（テスト用の補助）
        /// </summary>
        /// <param name="picker">抽選に使う AnswerPicker（初期化済み）</param>
        /// <param name="count">抽選する回数（回）</param>
        /// <returns>抽選した答えの並び</returns>
        private static List<GlyphType> DrawMany(AnswerPicker picker, int count)
        {
            // ローカル変数は関数の先頭で宣言する
            List<GlyphType> answers;   // 抽選結果
            int i;                     // ループ用の添字

            answers = new List<GlyphType>(count);
            for (i = 0; i < count; i++)
            {
                answers.Add(picker.PickNext());
            }
            return answers;
        }

        /// <summary>
        /// 答えの並びの中で、同じ答えが最も長く続いた回数を数える（テスト用の補助）
        /// </summary>
        /// <param name="answers">答えの並び</param>
        /// <returns>最長の連続回数（回）</returns>
        private static int GetLongestStreak(List<GlyphType> answers)
        {
            // ローカル変数は関数の先頭で宣言する
            int longest;   // これまでの最長（回）
            int current;   // いま続いている回数（回）
            int i;         // ループ用の添字

            longest = 0;
            current = 0;
            for (i = 0; i < answers.Count; i++)
            {
                // 前と同じなら連続を伸ばし、違えば1からやり直す
                current = (i > 0 && answers[i] == answers[i - 1]) ? current + 1 : 1;
                longest = Math.Max(longest, current);
            }
            return longest;
        }
    }
}
