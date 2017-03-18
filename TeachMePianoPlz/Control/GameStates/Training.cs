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

        public int[] NOTE_Y = new int[]
        {
            5, 4, 10, 9, 8, 7, 6
        };

        public bool[] NEEDS_OWN_BAR = new bool[]
        {
            false, false, true, false, false, false, false
        };

        public Dictionary<char, int> LETTER_TO_INDEX = new Dictionary<char, int>()
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
            public int Index;
            public int X;
            public int Y;
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
                    if (_notes[i].X < 40 && _notes[i].X > -40 && LETTER_TO_INDEX[letter] == _notes[i].Index)
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
                    if (NEEDS_OWN_BAR[n.Index])
                        Teacher.Instance.Graphics.DrawLine(n.X - 15, n.Y + 45, n.X + 95, n.Y + 45, Color.Gray);

                    if (n.HideLetter)
                        Teacher.Instance.Graphics.DrawSprite(n.X, n.Y, GraphicsID.Notes, 7);
                    else
                        Teacher.Instance.Graphics.DrawSprite(n.X, n.Y, GraphicsID.Notes, n.Index);
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
                    AddNotes(_rng.Next(10) == 0 ? 2 : 1);
                }
            }
        }

        private void AddNotes(int noteCount)
        {
            if (noteCount > 7)
                noteCount = 7;

            List<int> availableNotes = new List<int>() { 0, 1, 2, 3, 4, 5, 6 };

            lock (_notes_lock)
            {
                for (int n = 0; n < noteCount; n++)
                {
                    int i = _rng.Next(availableNotes.Count);

                    _notes.Add(new Note()
                    {
                        Index = availableNotes[i],
                        X = Teacher.Instance.Graphics.Width,
                        Y = NOTE_Y[availableNotes[i]] * 50 + 105,
                        HideLetter = _rng.Next(10) == 0,
                    });

                    availableNotes.RemoveAt(i);
                }
            }
        }

        public void EnterState()
        {

        }
    }
}
