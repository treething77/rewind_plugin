using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace aeric.rewind_plugin
{
    public class PlaybackControls : MonoBehaviour
    {
        private float currentTime;
        
        private void OnGUI()
        {
            // Create a horizontal layout group for the replay controls
            GUILayout.BeginHorizontal();

            // Add a button to start the replay playback
            if (GUILayout.Button("Play Replay"))
            {
              //  replaySystem.StartPlayback();
            }

            // Add a button to stop the replay playback
            if (GUILayout.Button("Stop Replay"))
            {
             //   replaySystem.StopPlayback();
            }

            GUILayout.EndHorizontal();
            
            // Add a scrubber component to control the replay time
            currentTime = GUILayout.HorizontalSlider(currentTime, 0.0f, 5.0f);

            // Check if the scrubber value has changed
            if (currentTime != 0.0f)//replaySystem.CurrentTime)
            {
                // Set the replay time to the scrubber value
                //replaySystem.SetPlaybackTime(currentTime);
            }

            // Add a label to display the current time
            GUILayout.Label("Time: " + currentTime.ToString("F2"));

        }
    

    }
}