using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Midi;
using Microsoft.Xna.Framework;

namespace TeachMePianoPlz
{
    public class Training: IGameState
    {
        private MIDIListener _midi_listener;

        public const int BASE_OCTAVE = 4;

        public Dictionary<char, int> NOTE_TO_INDEX = new Dictionary<char, int>()
        {
            { 'A', 0 },
            { 'B', 1 },
            { 'C', 2 },
            { 'D', 3 },
            { 'E', 4 },
            { 'F', 5 },
            { 'G', 6 },
        };

        public class Note
        {
            public char Letter;
            public int Octave;
            public int X;
            public bool HideLetter;
        }

        private Random _rng = new Random();

        // notes can be removed at any time (from MIDI input), so we need a lock
        private object _notes_lock = new { };

        private List<Note> _notes = new List<Note>();

        private int _speed = 6;

        public int _hit_notes = 0;
        public int _missed_notes = 0;
        public int _bad_notes_played = 0;
        public int _streak = 0;

        public Training()
        {
            // sub-optimal...
            _midi_listener = new MIDIListener(GetNote);
            _midi_listener.Start();
        }

        public void GetNote(NoteOnMessage m)
        {
            char letter = m.Pitch.NotePreferringSharps().Letter;

            lock (_notes_lock)
            {
                for (int i = _notes.Count - 1; i >= 0; i--)
                {
                    if (_notes[i].X < 40 && _notes[i].X > -40 && letter == _notes[i].Letter)
                    {
                        _notes.RemoveAt(i);
                        _hit_notes++;
                        _streak++;
                        return;
                    }
                }
            }

            _bad_notes_played++;
            _streak = 0;
        }

        public void Draw()
        {
            // draw the staff
            Teacher.Instance.Graphics.DrawLine(0, 150, Teacher.Instance.Graphics.Width, 150, Color.Gray);
            Teacher.Instance.Graphics.DrawLine(0, 250, Teacher.Instance.Graphics.Width, 250, Color.Gray);
            Teacher.Instance.Graphics.DrawLine(0, 350, Teacher.Instance.Graphics.Width, 350, Color.Gray);
            Teacher.Instance.Graphics.DrawLine(0, 450, Teacher.Instance.Graphics.Width, 450, Color.Gray);
            Teacher.Instance.Graphics.DrawLine(0, 550, Teacher.Instance.Graphics.Width, 550, Color.Gray);

            Teacher.Instance.Graphics.DrawLine(39, 0, 39, Teacher.Instance.Graphics.Height, Color.White);
            Teacher.Instance.Graphics.DrawLine(40, 0, 40, Teacher.Instance.Graphics.Height, Color.White);

            // draw the notes
            lock (_notes_lock)
            {
                foreach (Note n in _notes)
                {
                    if ((n.Letter <= 'C' && n.Octave == BASE_OCTAVE) || n.Octave < BASE_OCTAVE)
                    {
                        for(int y = NoteY('C', BASE_OCTAVE); y <= NoteY(n.Letter, n.Octave); y += 100)
                            Teacher.Instance.Graphics.DrawLine(n.X - 15, y + 45, n.X + 95, y + 45, Color.Gray);
                    }

                    if (n.HideLetter)
                        Teacher.Instance.Graphics.DrawSprite(n.X, NoteY(n), GraphicsID.Notes, 7);
                    else
                        Teacher.Instance.Graphics.DrawSprite(n.X, NoteY(n), GraphicsID.Notes, NOTE_TO_INDEX[n.Letter]);
                }
            }

            DrawNumber(4, Teacher.Instance.Graphics.Height - 42, (int)(OverallHitPercent() * 100), true);
            DrawNumber(200, Teacher.Instance.Graphics.Height - 42, _streak, false);

            DrawNumber(600, Teacher.Instance.Graphics.Height - 42, _hit_notes, false);
            DrawNumber(800, Teacher.Instance.Graphics.Height - 42, _bad_notes_played, false);
            DrawNumber(1000, Teacher.Instance.Graphics.Height - 42, _missed_notes, false);
        }

        private void DrawNumber(int x, int y, int value, bool percent)
        {
            string stringValue = value.ToString();

            int i = 0;

            foreach (char c in stringValue)
            {
                Teacher.Instance.Graphics.DrawSprite(x + i * 38, y, GraphicsID.Digits, c - '0');
                i++;
            }

            if(percent)
                Teacher.Instance.Graphics.DrawSprite(x + i * 38, y, GraphicsID.Digits, 10);
        }

        private float OverallHitPercent()
        {
            if (_hit_notes + _bad_notes_played + _missed_notes == 0)
                return 0;
            else
                return (float)_hit_notes / (_hit_notes + _bad_notes_played + _missed_notes);
        }

        public const int NEW_NOTE_COOLDOWN = 120;
        private int _new_note_heat = 0;

        public void Update()
        {
            lock (_notes_lock)
            {
                for (int i = _notes.Count - 1; i >= 0; i--)
                {
                    if (_notes[i].X <= -160 + _speed)
                    {
                        _notes.RemoveAt(i);
                        _missed_notes++;
                        _streak = 0;
                    }
                    else
                        _notes[i].X -= _speed;
                }
            }

            if(_new_note_heat > 0)
            {
                _new_note_heat--;
            }
            else
            {
                _new_note_heat = NEW_NOTE_COOLDOWN - 1;

                if (_rng.Next(10) > 0)
                {
                    AddNotes(_rng.Next(1, 3));
                }
            }
        }

        private void AddNotes(int noteCount)
        {
            if (noteCount > 7)
                noteCount = 7;

            List<Note> availableNotes = new List<Note>()
            {
                //new Note() { Letter = 'B', Octave = BASE_OCTAVE },
                new Note() { Letter = 'C', Octave = BASE_OCTAVE },
                new Note() { Letter = 'D', Octave = BASE_OCTAVE },
                new Note() { Letter = 'E', Octave = BASE_OCTAVE },
                new Note() { Letter = 'F', Octave = BASE_OCTAVE },
                new Note() { Letter = 'G', Octave = BASE_OCTAVE },
                //new Note() { Letter = 'A', Octave = BASE_OCTAVE + 1 },
            };

            lock (_notes_lock)
            {
                for (int i = 0; i < noteCount; i++)
                {
                    int n = _rng.Next(availableNotes.Count);

                    Note note = availableNotes[n];
                    note.X = Teacher.Instance.Graphics.Width;
                    note.HideLetter = _rng.Next(10) == 0;

                    _notes.Add(note);

                    availableNotes.RemoveAt(n);
                }
            }
        }

        public int NoteY(Note n)
        {
            return NoteY(n.Letter, n.Octave);
        }

        public int NoteY(char letter, int octave)
        {
            return (12 - NOTE_TO_INDEX[letter]) * 50 + 105 + (BASE_OCTAVE - octave) * 7 * 50;
        }

        public void EnterState()
        {

        }
    }
}
