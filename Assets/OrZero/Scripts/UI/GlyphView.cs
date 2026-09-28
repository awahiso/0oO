using TMPro;
using UnityEngine;

namespace OrZero
{
    /// <summary>
    /// 画面中央の出題文字を表示するクラス。
    /// いまは答えの種類に応じて「O」か「0」を出すだけ（スタイルの反映は T7 で足す）
    /// </summary>
    public class GlyphView : MonoBehaviour
    {
        // ===== 参照（Inspector で設定） =====
        [SerializeField] private TMP_Text glyphText;   // 出題文字を表示する TMP テキスト

        /// <summary>
        /// 出題文字を表示する
        /// </summary>
        /// <param name="answer">表示する答えの種類</param>
        public void Show(GlyphType answer)
        {
            glyphText.text = answer == GlyphType.LetterO ? "O" : "0";
            glyphText.enabled = true;
        }
    }
}
