using System;
using NUnit.Framework;

namespace OrZero.Tests
{
    /// <summary>
    /// GameTimer（残り時間）のテスト（EditMode）
    /// SPEC §1.1「60.00秒から減少、0.00 で時間切れ、3コンボごとに +1.0秒（上限 99.99秒）」を確かめる
    /// </summary>
    public class GameTimerTests
    {
        // ===== テストで共通に使う値 =====
        private const float Tolerance = 0.0001f;   // float の比較で許す誤差（秒）
        private const float MaxSeconds = 99.99f;   // SPEC で決めた残り時間の上限（秒）

        /// <summary>
        /// 経過時間の分だけ残り時間が減る
        /// </summary>
        [Test]
        public void Tick_OneAndHalfSeconds_DecreasesRemaining()
        {
            // ローカル変数は関数の先頭で宣言する
            GameTimer timer;   // テスト対象

            // 準備: 60秒から始める
            timer = new GameTimer();
            timer.Reset(60f);

            // 実行: 1.5秒進める
            timer.Tick(1.5f);

            // 確認: 58.5秒になり、まだ時間切れではない
            Assert.AreEqual(58.5f, timer.RemainingSeconds, Tolerance);
            Assert.IsFalse(timer.IsTimeUp);
        }

        /// <summary>
        /// 残り時間を超えて進めると、0 で止まって時間切れになる（マイナスにならない）
        /// </summary>
        [Test]
        public void Tick_PastZero_ClampsToZeroAndIsTimeUp()
        {
            // ローカル変数は関数の先頭で宣言する
            GameTimer timer;   // テスト対象

            // 準備: 残り1秒
            timer = new GameTimer();
            timer.Reset(1f);

            // 実行: 2秒進める
            timer.Tick(2f);

            // 確認: 0 で止まり、時間切れ
            Assert.AreEqual(0f, timer.RemainingSeconds, Tolerance);
            Assert.IsTrue(timer.IsTimeUp);
        }

        /// <summary>
        /// 60秒ぶんのフレーム（1/60秒×3601回）を進めると、float の誤差があっても必ず時間切れになる
        /// </summary>
        [Test]
        public void Tick_SixtySecondsOfFrames_ReachesTimeUp()
        {
            // ローカル変数は関数の先頭で宣言する
            GameTimer timer;   // テスト対象
            int i;             // ループ用の添字

            // 準備: 60秒から始める
            timer = new GameTimer();
            timer.Reset(60f);

            // 実行: 1/60秒ずつ、60秒＋1フレームぶん進める
            for (i = 0; i < 3601; i++)
            {
                timer.Tick(1f / 60f);
            }

            // 確認: 時間切れになっている
            Assert.IsTrue(timer.IsTimeUp);
        }

        /// <summary>
        /// マイナスの経過時間を渡しても、残り時間は増えない
        /// </summary>
        [Test]
        public void Tick_NegativeDelta_DoesNotIncreaseRemaining()
        {
            // ローカル変数は関数の先頭で宣言する
            GameTimer timer;   // テスト対象

            // 準備: 10秒から始める
            timer = new GameTimer();
            timer.Reset(10f);

            // 実行: マイナスの経過時間を渡す
            timer.Tick(-1f);

            // 確認: 10秒のまま
            Assert.AreEqual(10f, timer.RemainingSeconds, Tolerance);
        }

        /// <summary>
        /// 延長すると、その秒数だけ残り時間が増える
        /// </summary>
        [Test]
        public void Extend_OneSecond_AddsToRemaining()
        {
            // ローカル変数は関数の先頭で宣言する
            GameTimer timer;   // テスト対象

            // 準備: 10秒から始める
            timer = new GameTimer();
            timer.Reset(10f);

            // 実行: 1秒延長する
            timer.Extend(1f, MaxSeconds);

            // 確認: 11秒になる
            Assert.AreEqual(11f, timer.RemainingSeconds, Tolerance);
        }

        /// <summary>
        /// 延長しても、上限（99.99秒）を超えない
        /// </summary>
        [Test]
        public void Extend_OverMax_StopsAtMax()
        {
            // ローカル変数は関数の先頭で宣言する
            GameTimer timer;   // テスト対象

            // 準備: 99秒から始める
            timer = new GameTimer();
            timer.Reset(99f);

            // 実行: 5秒延長する
            timer.Extend(5f, MaxSeconds);

            // 確認: 上限の 99.99秒で止まる
            Assert.AreEqual(MaxSeconds, timer.RemainingSeconds, Tolerance);
        }

        /// <summary>
        /// 時間切れの後でも、Reset すれば開始時の残り時間に戻る
        /// </summary>
        [Test]
        public void Reset_AfterTimeUp_RestoresStartSeconds()
        {
            // ローカル変数は関数の先頭で宣言する
            GameTimer timer;   // テスト対象

            // 準備: 一度時間切れにする
            timer = new GameTimer();
            timer.Reset(1f);
            timer.Tick(5f);

            // 実行: 60秒で始め直す
            timer.Reset(60f);

            // 確認: 60秒に戻り、時間切れではない
            Assert.AreEqual(60f, timer.RemainingSeconds, Tolerance);
            Assert.IsFalse(timer.IsTimeUp);
        }

        /// <summary>
        /// マイナスの開始秒で Reset すると、設定ミスとして例外になる
        /// </summary>
        [Test]
        public void Reset_NegativeStart_ThrowsArgumentOutOfRangeException()
        {
            // ローカル変数は関数の先頭で宣言する
            GameTimer timer;   // テスト対象

            // 準備
            timer = new GameTimer();

            // 実行・確認
            Assert.Throws<ArgumentOutOfRangeException>(() => timer.Reset(-1f));
        }

        /// <summary>
        /// マイナスの秒数で延長すると、設定ミスとして例外になる
        /// </summary>
        [Test]
        public void Extend_NegativeSeconds_ThrowsArgumentOutOfRangeException()
        {
            // ローカル変数は関数の先頭で宣言する
            GameTimer timer;   // テスト対象

            // 準備
            timer = new GameTimer();
            timer.Reset(10f);

            // 実行・確認
            Assert.Throws<ArgumentOutOfRangeException>(() => timer.Extend(-1f, MaxSeconds));
        }
    }
}
