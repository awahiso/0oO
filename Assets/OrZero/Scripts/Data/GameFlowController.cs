using UnityEngine;

namespace OrZero
{
    /// <summary>
    /// Game シーンの進行を enum のステートで管理するクラス（SPEC §1.2）。
    /// ステートを持つのはこのクラスだけで、UI は表示と入力の通知だけを受け持つ。
    /// いまは Playing（出題・回答）・Miss（不正解）・TimeUp（時間切れ）を使う
    /// </summary>
    public class GameFlowController : MonoBehaviour
    {
        // ===== 調整値・参照（Inspector で設定） =====
        [SerializeField] private GameBalanceData balanceData;    // ゲーム全体の調整値
        [SerializeField] private GlyphView glyphView;            // 画面中央の出題文字
        [SerializeField] private AnswerButton[] answerButtons;   // 回答ボタン（英字用と数字用を1つずつ）

        // ===== 実行時の状態（確認用に Inspector へ表示） =====
        [SerializeField] private GameState currentState = GameState.Playing;      // 現在のステート
        [SerializeField] private AnswerPicker answerPicker = new AnswerPicker();  // 答えの抽選（同じ答えの連続上限つき）
        [SerializeField] private GameTimer gameTimer = new GameTimer();           // 残り時間（秒）
        [SerializeField] private GlyphType currentAnswer = GlyphType.LetterO;     // いま出している問題の答え
        [SerializeField] private int correctCount;                                // 正解数（ミスで即終了のため、コンボ数と同じ）

        /// <summary>
        /// 起動時に Inspector の設定漏れを確かめる。漏れがあればこのコンポーネントを止める
        /// （止めると OnEnable・Start・Update も呼ばれないので、以降のエラーが連鎖しない）
        /// </summary>
        private void Awake()
        {
            if (!HasValidSettings())
            {
                enabled = false;
            }
        }

        /// <summary>
        /// 有効になったとき、回答ボタンの通知を受け取り始める
        /// </summary>
        private void OnEnable()
        {
            // ローカル変数は関数の先頭で宣言する
            int i;   // ループ用の添字

            for (i = 0; i < answerButtons.Length; i++)
            {
                answerButtons[i].Pressed += HandleAnswerPressed;
            }
        }

        /// <summary>
        /// 無効になったとき、回答ボタンの通知の受け取りをやめる
        /// </summary>
        private void OnDisable()
        {
            // ローカル変数は関数の先頭で宣言する
            int i;   // ループ用の添字

            for (i = 0; i < answerButtons.Length; i++)
            {
                answerButtons[i].Pressed -= HandleAnswerPressed;
            }
        }

        /// <summary>
        /// 最初のフレームの前に、抽選と残り時間を準備して出題を始める
        /// </summary>
        private void Start()
        {
            // 実行時の状態は、Inspector に残った値に左右されないようにここで必ず設定し直す
            correctCount = 0;
            answerPicker.Initialize(balanceData.MaxSameAnswerStreak, new System.Random());
            gameTimer.Reset(balanceData.StartSeconds);

            // カウントダウン（T9）ができるまでは、すぐに出題から始める
            ChangeState(GameState.Playing);
        }

        /// <summary>
        /// 毎フレーム、現在のステートに応じた更新を行う
        /// </summary>
        private void Update()
        {
            switch (currentState)
            {
                case GameState.Playing:
                    UpdatePlaying();
                    break;

                default:
                    // Playing 以外のステートには、まだ毎フレームの処理がない
                    break;
            }
        }

        /// <summary>
        /// Playing 中の毎フレームの処理（残り時間を減らし、0 になったら時間切れにする）
        /// </summary>
        private void UpdatePlaying()
        {
            gameTimer.Tick(Time.deltaTime);
            if (gameTimer.IsTimeUp)
            {
                ChangeState(GameState.TimeUp);
            }
        }

        /// <summary>
        /// ステートを切り替え、そのステートに入ったときの処理を行う
        /// </summary>
        /// <param name="nextState">次のステート</param>
        private void ChangeState(GameState nextState)
        {
            currentState = nextState;
            EnterState(nextState);
        }

        /// <summary>
        /// ステートに入ったときの処理
        /// </summary>
        /// <param name="state">入ったステート</param>
        private void EnterState(GameState state)
        {
            switch (state)
            {
                case GameState.Playing:
                    ShowNextQuestion();
                    break;

                case GameState.Miss:
                    // リザルト画面（T11）ができるまでは Console に出すだけ
                    Debug.Log($"GAME OVER（仮）: 正解は {currentAnswer}／正解数 {correctCount}", this);
                    break;

                case GameState.TimeUp:
                    // リザルト画面（T11）ができるまでは Console に出すだけ
                    Debug.Log($"TIME UP（仮）: 正解数 {correctCount}", this);
                    break;

                default:
                    // ほかのステートの処理は、それぞれのタスク（T9・T14）で足す
                    break;
            }
        }

        /// <summary>
        /// 回答ボタンが押されたときの処理（AnswerButton の Pressed から呼ばれる）
        /// </summary>
        /// <param name="pressedAnswer">押されたボタンが担当する答え</param>
        private void HandleAnswerPressed(GlyphType pressedAnswer)
        {
            // 回答を受け付けるのは Playing 中だけ（Miss・TimeUp になった後の入力は無視する）
            if (currentState != GameState.Playing)
            {
                return;
            }

            if (pressedAnswer == currentAnswer)
            {
                // 正解: 正解数を増やし、決まった問題数ごとに時間を延長して、すぐ次の問題へ
                correctCount++;
                if (correctCount % balanceData.ExtendInterval == 0)
                {
                    gameTimer.Extend(balanceData.ExtendSeconds, balanceData.MaxSeconds);
                }
                ShowNextQuestion();
            }
            else
            {
                // 不正解: 1回で即ゲームオーバー
                ChangeState(GameState.Miss);
            }
        }

        /// <summary>
        /// 次の問題を抽選して表示する
        /// </summary>
        private void ShowNextQuestion()
        {
            currentAnswer = answerPicker.PickNext();
            glyphView.Show(currentAnswer);
        }

        /// <summary>
        /// Inspector の設定漏れ・設定ミスがないか確かめる
        /// </summary>
        /// <returns>問題がなければ true</returns>
        private bool HasValidSettings()
        {
            // ローカル変数は関数の先頭で宣言する
            int letterButtonCount;   // 英字用のボタンの数
            int digitButtonCount;    // 数字用のボタンの数
            int i;                   // ループ用の添字

            if (balanceData == null || glyphView == null || answerButtons == null)
            {
                Debug.LogError("GameFlowController: balanceData・glyphView・answerButtons を Inspector で設定してください", this);
                return false;
            }

            // 英字用と数字用のボタンが1つずつそろっているか（左右の入れ替えミスで、答えられない問題が出ないように）
            letterButtonCount = 0;
            digitButtonCount = 0;
            for (i = 0; i < answerButtons.Length; i++)
            {
                if (answerButtons[i] == null)
                {
                    Debug.LogError($"GameFlowController: answerButtons の {i} 番目が未設定です", this);
                    return false;
                }

                if (answerButtons[i].AnswerType == GlyphType.LetterO)
                {
                    letterButtonCount++;
                }
                else if (answerButtons[i].AnswerType == GlyphType.DigitZero)
                {
                    digitButtonCount++;
                }
            }

            if (letterButtonCount != 1 || digitButtonCount != 1)
            {
                Debug.LogError($"GameFlowController: 英字用と数字用のボタンを1つずつ設定してください（いまは英字 {letterButtonCount}・数字 {digitButtonCount}。AnswerButton の answerType を確認）", this);
                return false;
            }

            return true;
        }
    }
}
