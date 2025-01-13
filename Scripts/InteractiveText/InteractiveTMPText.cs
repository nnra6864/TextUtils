using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace NnUtils.Modules.TextUtils.Scripts.InteractiveText
{
    [RequireComponent(typeof(TMP_Text))]
    public class InteractiveTMPText : InteractiveTextScript
    {
        private List<TMP_Text> _textElements;

        protected override void Start()
        {
            _textElements = GetComponents<TMP_Text>().ToList();
            base.Start();
            UpdateData(_textElements.Select(x => x.text).ToList());
        }

        protected override void SetText(string text, int index)
        {
            _textElements[index].text = text;
        }
    }
}