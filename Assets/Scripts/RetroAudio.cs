using UnityEngine;

public class RetroAudio : MonoBehaviour
{
    public static RetroAudio Instance { get; private set; }
    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlayJump() => PlayTone(220f, 520f, 0.12f, false);
    public void PlayCoin() => PlayTone(900f, 1400f, 0.09f, false);
    public void PlayDie()  => PlayTone(320f, 55f, 0.32f, true);

    public void PlayWin()
    {
        StartCoroutine(WinSequence());
    }

    private System.Collections.IEnumerator WinSequence()
    {
        float[] freqs = { 523f, 659f, 784f, 1047f };
        for (int i = 0; i < freqs.Length; i++)
        {
            PlayTone(freqs[i], freqs[i], 0.12f, false);
            yield return new WaitForSeconds(0.11f);
        }
    }

    private void PlayTone(float f0, float f1, float dur, bool sawtooth)
    {
        int rate = 44100;
        int count = Mathf.FloorToInt(rate * dur);
        float[] samples = new float[count];
        float phase = 0f;

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / count;
            float freq = Mathf.Lerp(f0, f1, t);
            phase += 2f * Mathf.PI * freq / rate;
            float val = sawtooth ? (Mathf.Repeat(phase / (2f * Mathf.PI), 1f) - 0.5f) * 0.5f : (Mathf.Sin(phase) >= 0 ? 0.25f : -0.25f);
            samples[i] = val * Mathf.Lerp(0.08f, 0.0001f, t);
        }

        AudioClip clip = AudioClip.Create("Chiptune", count, 1, rate, false);
        clip.SetData(samples, 0);
        audioSource.PlayOneShot(clip);
    }
}