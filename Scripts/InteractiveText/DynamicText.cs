using System;

namespace NnUnityTextUtils.InteractiveText
{
    public class DynamicText
    {
        public string Text;
        public Func<string> Func;
        public float? Interval;
        public bool Async;

        public DynamicText(string text, Func<string> func, float? interval, bool async)
        {
            Text = text;
            Func = func;
            Interval = interval;
            Async = async;
        }
    }
}
