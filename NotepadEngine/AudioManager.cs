using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using NAudio;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace NotepadEngine
{
    public static class AudioManager
    {
        private static Dictionary<string, (WaveOutEvent, AudioFileReader)> _EffectsAdded = new Dictionary<string, (WaveOutEvent, AudioFileReader)>();
        private static MixingSampleProvider mixingSample;

        public static void AddEffect(string audioFile, string effectName)
        {
            WaveOutEvent waveOut = new WaveOutEvent();
            AudioFileReader afr = new AudioFileReader(audioFile);
            waveOut.Init(afr);
            _EffectsAdded.Add(effectName, (waveOut, afr));
        }

        public static void PlayEffect(string effectName)
        {
            if (_EffectsAdded.ContainsKey(effectName))
            {
                _EffectsAdded[effectName].Item2.Seek(0, SeekOrigin.Begin);
                _EffectsAdded[effectName].Item1.Play();
                return;
            }
            throw new InvalidOperationException("Invalid effect.");
        }

        public static void PlayEffectDelayed(string effectName, int MillisecondDelay)
        {
            Task.Run(() =>
            {
                Task.Delay(MillisecondDelay);
                if (_EffectsAdded.ContainsKey(effectName))
                {
                    _EffectsAdded[effectName].Item2.Seek(0, SeekOrigin.Begin);
                    _EffectsAdded[effectName].Item1.Play();
                    return;
                }
                throw new InvalidOperationException("Invalid effect.");
            });
        }
    }
}
