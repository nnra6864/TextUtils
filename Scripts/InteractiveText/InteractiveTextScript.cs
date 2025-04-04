using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace NnUtils.Modules.TextUtils.Scripts.InteractiveText
{
    public class InteractiveTextScript : MonoBehaviour
    {
        /// Stores the unprocessed text
        protected List<string> _text = new();

        /// Stores dynamic text values <br/>
        /// Key represents the index of the text element that dynamic text is made from <br/>
        /// Value represents the list of all the dynamic text instances found in that text element
        private readonly Dictionary<int, List<DynamicText>> _dynamicText = new();

        private bool _canUpdate;
        
        protected virtual void Start()
        {
            UpdateData();
        }
        
        // TODO: Call UpdateData when config loads

        /// Updates all the data
        public void UpdateData() => UpdateText(_text);

        /// Handles text being changed
        private void UpdateText(List<string> text)
        {
            // Prevent too early updates
            _canUpdate = false;
            
            // Assign text elements
            _text = text;

            // Clear and fill the _dynamicText dictionary
            _dynamicText.Clear();
            for (var i = 0; i < _text.Count; i++)
                _dynamicText.Add(i, InteractiveTextProcessing.GetDynamicText(_text[i]));

            // Stop all the running dynamic text routines
            foreach (var routine in _updateDynamicTextRoutines) StopCoroutine(routine);

            // Update text
            for (var i = 0; i < _text.Count; i++)
            {
                // If there is no dynamic text, just set the text
                if (_dynamicText[i].Count == 0)
                {
                    SetText(_text[i], i);
                    continue;
                }

                // Update all dynamic text instances
                for (var o = 0; o < _dynamicText[i].Count; o++)
                    _updateDynamicTextRoutines.Add(StartCoroutine(UpdateDynamicTextRoutine(i, o)));
            }

            // Allow updates once all the data is loaded
            _canUpdate = true;
        }

        /// Stores all the update dynamic text routines
        private readonly List<Coroutine> _updateDynamicTextRoutines = new();

        /// Updates an instance of dynamic text and triggers the text update
        private IEnumerator UpdateDynamicTextRoutine(int i, int o)
        {
            // Wait till data is ready
            yield return new WaitWhile(() => !_canUpdate);
            
            var dt = _dynamicText[i][o];
        
            while (true)
            {
                // Store the start time
                var startTime = Time.realtimeSinceStartup;

                // Update dt
                if (dt.Async)
                {
                    var task = Task.Run(() => dt.Func());
                    yield return new WaitUntil(() => task.IsCompleted);
                    
                    // If task finished running, run SetText()
                    if (task.Status == TaskStatus.RanToCompletion) dt.Text = task.Result;
                }
                else dt.Text = dt.Func();


                // Set text
                SetText(InteractiveTextProcessing.ReplaceWithDynamicText(_text[i], new(_dynamicText[i])), i);
                
                // Get interval and break if null
                var interval = dt.Interval;
                if (interval == null) yield break;
        
                // Calculate and wait for remaining time
                var elapsedTime = Time.realtimeSinceStartup - startTime;
                var remainingTime = Mathf.Max(0, (float)interval - elapsedTime);
                if (remainingTime > 0) yield return new WaitForSecondsRealtime(remainingTime);
            }
        }
        
        /// Sets the text of the ui component
        protected virtual void SetText(string text, int index) { }
    }
}