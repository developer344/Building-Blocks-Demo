using UnityEngine;

/// <summary>
/// Builds the potion sound effects in code, so the project needs no audio files.
/// </summary>
public static class PotionSounds
{
    private const int SampleRate = 44100;

    /// <summary>Three bubbly gulps, about one second long.</summary>
    public static AudioClip CreateGulps()
    {
        var samples = new float[SampleRate];
        var random = new System.Random(7);
        foreach (float start in new[] { 0.02f, 0.32f, 0.62f })
            AddGulp(samples, start, random);
        return Build("Gulps", samples);
    }

    /// <summary>A short rising sparkle of bell-like notes.</summary>
    public static AudioClip CreateChime()
    {
        var samples = new float[(int)(SampleRate * 1.4f)];
        float[] notes = { 784f, 988f, 1175f, 1568f };   // G5, B5, D6, G6
        for (int n = 0; n < notes.Length; n++)
            AddBell(samples, n * 0.09f, notes[n], 0.25f);
        return Build("Chime", samples);
    }

    private static void AddGulp(float[] samples, float startTime, System.Random random)
    {
        const float duration = 0.2f;
        int start = (int)(startTime * SampleRate);
        int count = (int)(duration * SampleRate);
        double phase = 0;
        for (int i = 0; i < count && start + i < samples.Length; i++)
        {
            float t = i / (float)count;
            // A falling pitch with a soft swell reads as a swallow.
            float frequency = Mathf.Lerp(330f, 120f, t);
            phase += 2 * Mathf.PI * frequency / SampleRate;
            float envelope = Mathf.Pow(Mathf.Sin(Mathf.PI * t), 2f);
            float bubble = (float)(random.NextDouble() * 2 - 1) * 0.12f;
            samples[start + i] += ((float)System.Math.Sin(phase) * 0.7f + bubble) * envelope * 0.8f;
        }
    }

    private static void AddBell(float[] samples, float startTime, float frequency, float amplitude)
    {
        int start = (int)(startTime * SampleRate);
        for (int i = 0; start + i < samples.Length; i++)
        {
            float t = i / (float)SampleRate;
            float envelope = Mathf.Exp(-t * 4f) * Mathf.Min(1f, t * 200f);
            float tone = Mathf.Sin(2 * Mathf.PI * frequency * t) + 0.3f * Mathf.Sin(2 * Mathf.PI * frequency * 2.01f * t);
            samples[start + i] += tone * envelope * amplitude;
        }
    }

    private static AudioClip Build(string name, float[] samples)
    {
        for (int i = 0; i < samples.Length; i++)
            samples[i] = Mathf.Clamp(samples[i], -1f, 1f);

        var clip = AudioClip.Create(name, samples.Length, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
