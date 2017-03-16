using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Midi;

namespace TeachMePianoPlz
{
    public class MIDIListener
    {
        private InputDevice _midi_input;

        public MIDIListener(InputDevice.NoteOnHandler l)
        {
            _midi_input = InputDevice.InstalledDevices[0];
            _midi_input.Open();

            _midi_input.NoteOn += l;
        }

        public void Start()
        {
            if(!_midi_input.IsReceiving)
                _midi_input.StartReceiving(null);
        }

        public void Stop()
        {
            if (_midi_input.IsReceiving)
                _midi_input.StopReceiving();
        }
    }
}
