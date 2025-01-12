using System;

namespace NnUtils.Modules.TextUtils.Scripts.InteractiveText
{
    public class DynamicText
    {
        public string Text;
        public float? Interval;
        public Func<string> Func;

        public DynamicText(string text, Func<string> func, float? interval)
        {
            Text = text;
            Func = func;
            Interval = interval;
        }
    }
}