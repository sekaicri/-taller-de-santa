using System.Collections.Generic;
using UnityEngine;

namespace VrClass
{
    /// <summary>
    /// The game sounds. Each one is looked for in Assets/Resources/Feedback (tick, go, grab, wrap, good, bad, win, lose).
    /// If the file is missing, a short tone is made by code instead, so the game works without audio files.
    /// A tone is a list of notes and every note fades out. The clips are created once and reused.
    /// </summary>
    public static class FeedbackTones
    {
        const int SAMPLE_RATE = 44100;
        const float VOLUME = 0.35f;
        const float TWO_PI = Mathf.PI * 2f;
        const int CHANNELS = 1;
        const string FOLDER = "Feedback";

        static readonly Dictionary<string, AudioClip> _cache = new Dictionary<string, AudioClip>();

        public static AudioClip Tick => Get("Tick", 0.08f, 880f);                    // each number of the countdown
        public static AudioClip Go => Get("Go", 0.3f, 1320f);                        // the countdown ends
        public static AudioClip Grab => Get("Grab", 0.05f, 440f);                    // a toy is picked up
        public static AudioClip Wrap => Get("Wrap", 0.14f, 500f, 750f);              // the trigger wraps the toy
        public static AudioClip Good => Get("Good", 0.35f, 660f, 880f);              // a toy is accepted (going up)
        public static AudioClip Bad => Get("Bad", 0.4f, 220f, 165f);                 // a toy is rejected (going down)
        public static AudioClip Win => Get("Win", 0.9f, 523f, 659f, 784f, 1047f);    // the order is complete
        public static AudioClip Lose => Get("Lose", 0.8f, 440f, 392f, 330f, 262f);   // time is up

        // Builds (or finds) the clip with this name: the seconds are split equally between the notes.
        static AudioClip Get(string name, float seconds, params float[] notes)
        {
            if (_cache.TryGetValue(name, out var cached) && cached != null) return cached;

            // A real sound in Assets/Resources/Feedback (tick.mp3, good.mp3...) wins over the made-up tone.
            var real = Resources.Load<AudioClip>(FOLDER + "/" + name.ToLowerInvariant());
            if (real != null) { _cache[name] = real; return real; }

            var samplesPerNote = Mathf.CeilToInt(SAMPLE_RATE * seconds / notes.Length);
            var samples = new float[samplesPerNote * notes.Length];
            for (var note = 0; note < notes.Length; note++)
            {
                for (var i = 0; i < samplesPerNote; i++)
                {
                    var time = i / (float)SAMPLE_RATE;
                    var fade = 1f - i / (float)samplesPerNote;
                    samples[note * samplesPerNote + i] = Mathf.Sin(TWO_PI * notes[note] * time) * VOLUME * fade;
                }
            }

            var clip = AudioClip.Create(name, samples.Length, CHANNELS, SAMPLE_RATE, false);
            clip.SetData(samples, 0);
            _cache[name] = clip;
            return clip;
        }
    }
}
