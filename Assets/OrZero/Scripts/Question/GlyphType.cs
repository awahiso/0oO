namespace OrZero
{
    /// <summary>
    /// 出題される文字の種類（＝答えの種類）。
    /// 画面下のボタンの「英字」「数字」に対応する
    /// </summary>
    public enum GlyphType
    {
        LetterO,    // 英字側（既定は「O」。スタイルによっては「o」「Ｏ」なども英字側）
        DigitZero,  // 数字側（既定は「0」。スタイルによっては「０」なども数字側）
    }
}
