using System.Collections.Generic;

namespace aeric.rewind_plugin {
    public class RewindEventStream<T> where T: struct {
        public RewindEventStream(int maxEvents) {
            eventList = new List<T>(maxEvents);
            timesList = new List<float>(maxEvents);
        }

        private List<T> eventList;
        private List<float> timesList;

        public void addEvent(T anEvent, float time) {
            eventList.Add(anEvent);
            timesList.Add(time);
        }

        public void ClearEvents() {
            eventList.Clear();
            timesList.Clear();
        }

        public T getEvent(int eventIndex) {
            return eventList[eventIndex];
        }
        
        //the end index is exclusive
        public (int eventIndexStart, int eventIndexEnd) findEventsInRange(float startTime, float endTime) {
            //find the indices for all events where the corresponding time is >=startTime and < endtime 
            int eventIndexStart = -1;
            
            for (int i = 0; i < timesList.Count; i++) {
                if (timesList[i] < startTime) continue;
                if (timesList[i] < endTime) {
                    eventIndexStart = i;
                }
                break;
            }

            int eventIndexEnd = eventIndexStart+1;
            if (eventIndexStart != -1) {
                for (int i = eventIndexStart; i < timesList.Count; i++) {
                    if (timesList[i] < endTime) continue;
                    eventIndexEnd = i;
                    break;
                }
            }

            return (eventIndexStart, eventIndexEnd);
        }
    }
}