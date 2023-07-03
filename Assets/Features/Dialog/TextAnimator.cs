using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace MagicSwords.Features.Dialog
{
    public class TextAnimator : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private float _delay = 0.1f;
        [SerializeField] private string[] _monologue;

        private IEnumerator Start()
        {
            yield return TypingStart(0);
            yield return TypingStart(1);
        }

        private IEnumerator ShowText(string currentText)
        {
            for (var i = 0; i < currentText.Length; i++)
            {
                _text.text = currentText[..i];
                yield return new WaitForSeconds(_delay);
            }
        }

        private IEnumerator TypingStart(int index)
        {
            return ShowText(_monologue[index]);
        }
    }
}
