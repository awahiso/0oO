namespace OrZero
{
    /// <summary>
    /// Game シーンの進行ステート（SPEC §1.2）。
    /// ステートを持つのは GameFlowController だけ。bool フラグで分岐せず、この enum で管理する
    /// </summary>
    public enum GameState
    {
        Countdown,  // 開始前のカウントダウン（3・2・1・GO）。T9 で使い始める
        Playing,    // 出題・回答の受付・タイマーの減少
        Paused,     // ポーズ中（出題文字を隠し、タイマーを止める）。T14 で使い始める
        Miss,       // 不正解の演出中（正解を見せる）
        TimeUp,     // 時間切れの演出中。T3 で使い始める
        Result,     // リザルトの表示中。T9 以降で使い始める
    }
}
