using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OrZero
{
    /// <summary>
    /// 回答ボタン。押した瞬間（離す前）に、担当する答えを通知する（SPEC §1.4 ルール7）。
    /// uGUI の Button.onClick は離したときに発火するため使わない。
    /// どちらの答えのボタンかは Inspector の answerType で決める（左右を入れ替えるときはここを変えるだけ）
    /// </summary>
    public class AnswerButton : MonoBehaviour, IPointerDownHandler
    {
        // ===== 設定・参照（Inspector で設定） =====
        [SerializeField] private GlyphType answerType = GlyphType.LetterO;   // このボタンが担当する答え
        [SerializeField] private TMP_Text glyphLabel;      // ボタンに大きく出す文字（O／0）
        [SerializeField] private TMP_Text categoryLabel;   // 文字の種類のラベル（英字／数字）

        /// <summary>ボタンが押された瞬間に呼ばれる。引数はこのボタンが担当する答え</summary>
        public event Action<GlyphType> Pressed;

        /// <summary>このボタンが担当する答え</summary>
        public GlyphType AnswerType => answerType;

        /// <summary>
        /// 起動時に、担当する答えに合わせてラベルを書き換える（左右を入れ替えても表記がずれないように）
        /// </summary>
        private void Awake()
        {
            ApplyLabels();
        }

        /// <summary>
        /// ボタンが押された瞬間に EventSystem から呼ばれる
        /// </summary>
        /// <param name="eventData">押したときの情報（どのマウスボタンか など）</param>
        public void OnPointerDown(PointerEventData eventData)
        {
            // 左クリックだけを回答として扱う（右クリック・中クリックでは答えない）
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            Pressed?.Invoke(answerType);
        }

        /// <summary>
        /// 担当する答えに合わせて、ボタンの文字とラベルを設定する
        /// </summary>
        private void ApplyLabels()
        {
            if (glyphLabel != null)
            {
                glyphLabel.text = answerType == GlyphType.LetterO ? "O" : "0";
            }

            if (categoryLabel != null)
            {
                categoryLabel.text = answerType == GlyphType.LetterO ? "英字" : "数字";
            }
        }
    }
}
