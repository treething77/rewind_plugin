using System.Collections.Generic;

namespace aeric.rewind_plugin {
    /// <summary>
    /// A list of events each with an associated time. We have logic to find the events within a time range
    /// for playback.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RewindEventStream<T> where T: struct {
        public RewindEventStream(int maxEvents) {
            _eventList = new List<T>(maxEvents);
            _timesList = new List<float>(maxEvents);
        }

        private List<T> _eventList;
        private List<float> _timesList;

        public void addEvent(T anEvent, float time) {
            _eventList.Add(anEvent);
            _timesList.Add(time);
        }

        public void ClearEvents() {
            _eventList.Clear();
            _timesList.Clear();
        }

        public T getEvent(int eventIndex) {
            return _eventList[eventIndex];
        }
        
        //the end index is exclusive
        public (int eventIndexStart, int eventIndexEnd) findEventsInRange(float startTime, float endTime) {
            //find the indices for all events where the corresponding time is >=startTime and < endtime 
            int eventIndexStart = -1;
            
            for (int i = 0; i < _timesList.Count; i++) {
                if (_timesList[i] < startTime) continue;
                if (_timesList[i] < endTime) {
                    eventIndexStart = i;
                }
                break;
            }

            int eventIndexEnd = eventIndexStart+1;
            if (eventIndexStart != -1) {
                for (int i = eventIndexStart; i < _timesList.Count; i++) {
                    if (_timesList[i] < endTime) continue;
                    eventIndexEnd = i;
                    break;
                }
            }

            return (eventIndexStart, eventIndexEnd);
        }
    }
}