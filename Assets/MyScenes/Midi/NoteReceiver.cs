using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MidiPlayerTK;

public class NoteReceiver : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnNotes(List<MPTKEvent> mptkEvents)
    {
        Debug.Log("Received " + mptkEvents.Count + " MIDI Events");
        // Loop on each MIDI events
        foreach (MPTKEvent mptkEvent in mptkEvents)
        {
            // Log if event is a note on
            // if (mptkEvent.Command == MPTKCommand.NoteOn)
                // Debug.Log($"Note on Time:{mptkEvent.RealTime} millisecond  Note:{mptkEvent.Value}  Duration:{mptkEvent.Duration} millisecond  Velocity:{mptkEvent.Velocity}");

            // Uncomment to display all MIDI events
            Debug.Log(mptkEvent.ToString());
        }
    }
}
