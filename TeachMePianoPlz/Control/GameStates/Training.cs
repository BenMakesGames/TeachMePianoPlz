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
            5, 4, 3, 2, 1, 7, 6
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
        private List<Note> _notes = new List<Note>();

        private int _speed = 4;

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

            for(int i = _notes.Count - 1; i >= 0; i--)
            {
                if(_notes[i].X < 80 && _notes[i].X > -80 && LETTER_TO_INDEX[letter] == _notes[i].Index)
                {
                    _notes.RemoveAt(i);
                    _hit_notes++;
                    _streak++;
                    return;
                }
            }

            _bad_notes_played++;
            _streak = 0;
        }

        public void Draw()
        {
            // draw the staff
            Teacher.Instance.Graphics.DrawLine(0, 180 - 90, Teacher.Instance.Graphics.Width, 180 - 90, Color.Gray);
            Teacher.Instance.Graphics.DrawLine(0, 360 - 90, Teacher.Instance.Graphics.Width, 360 - 90, Color.Gray);
            Teacher.Instance.Graphics.DrawLine(0, 540 - 90, Teacher.Instance.Graphics.Width, 540 - 90, Color.Gray);
            Teacher.Instance.Graphics.DrawLine(0, 720 - 90, Teacher.Instance.Graphics.Width, 720 - 90, Color.Gray);
            Teacher.Instance.Graphics.DrawLine(0, 900 - 90, Teacher.Instance.Graphics.Width, 900 - 90, Color.Gray);

            Teacher.Instance.Graphics.DrawLine(80, 0, 80, Teacher.Instance.Graphics.Height, Color.White);

            // draw the notes
            foreach (Note n in _notes)
            {
                if(n.HideLetter)
                    Teacher.Instance.Graphics.DrawSprite(n.X, n.Y, GraphicsID.Notes, 7);
                else
                    Teacher.Instance.Graphics.DrawSprite(n.X, n.Y, GraphicsID.Notes, n.Index);
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
                return _hit_notes / (_hit_notes + _bad_notes_played + _missed_notes);
        }

        public const int NEW_NOTE_COOLDOWN = 120;
        private int _new_note_heat = 0;

        public void Update()
        {
            for(int i = _notes.Count - 1; i >= 0; i--)
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

            if(_new_note_heat > 0)
            {
                _new_note_heat--;
            }
            else
            {
                _new_note_heat = NEW_NOTE_COOLDOWN - 1;

                if (_rng.Next(10) > 0)
                {
                    AddNote();

                    if (_rng.Next(10) == 0)
                        AddNote();
                }
            }
        }

        private void AddNote()
        {
            int note = _rng.Next(7);
            _notes.Add(new Note()
            {
                Index = note,
                X = Teacher.Instance.Graphics.Width,
                Y = NOTE_Y[note] * 90,
                HideLetter = _rng.Next(10) == 0,
            });
        }

        public void EnterState()
        {

        }
    }
}
