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
            // Run the base start
            base.Start();
            
            // Get all text elements
            _textElements = GetComponents<TMP_Text>().ToList();
            
            // Store text
            _text = _textElements.Select(x => x.text).ToList();
            
            // Update data
            UpdateData();

            // Clear the text to avoid the flicker
            _textElements.ForEach(x => x.text = "");
        }

        protected override void SetText(string text, int index)
        {
            _textElements[index].text = text;
        }
    }
}