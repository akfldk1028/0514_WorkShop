using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class RhythmGameManager : MonoBehaviour
{
    [Header("���� ����")]
    public AudioClip soundClip;
    public AudioClip inputClip;
    public AudioSource audioSource;

    [Header("��Ʈ�γ� ����")]
    public AudioClip metronomeClip;
    public AudioSource metronomeSource;

    [Header("ī��Ʈ�ٿ� TMP �ؽ�Ʈ")]
    public TextMeshProUGUI Num;

    [Header("���� ���� ����")]
    [SerializeField] private float perfectWindow = 0.15f;
    [SerializeField] private float goodWindow = 0.3f;

    [Header("���� ����")]
    [SerializeField] private float interval = 0.65f;
    [SerializeField] private List<string> rhythmPattern = new List<string> { "A", "S", "AD", "0", "D", "0", "D", "A" };

    private List<float> expectedTimes = new List<float>();
    private List<float> inputTimes = new List<float>();
    private List<string> inputKeys = new List<string>();

    [Header("Ű �Է� �ȳ�")]
    public TextMeshProUGUI keyText;

    private bool useMetronome = true;

    // �ڷ�ƾ �ڵ鷯
    private Coroutine rhythmCoroutine;
    private Coroutine inputCoroutine;
    private Coroutine metronomeCoroutine;

    public void StartRhythmSequence()
    {
        TrimTrailingSilence();
        if (keyText != null) //Ű �ؽ�Ʈ�� ����
        {
            string patternText = string.Join("  ", rhythmPattern); // "0" ����
            keyText.text = patternText;
        }

        rhythmCoroutine = StartCoroutine(InitAndStart());
    }

    private void TrimTrailingSilence()
    {
        for (int i = rhythmPattern.Count - 1; i >= 0; i--)
        {
            if (rhythmPattern[i] != "0")
            {
                rhythmPattern = rhythmPattern.GetRange(0, i + 1);
                break;
            }
        }
    }

    private IEnumerator InitAndStart()
    {
        yield return new WaitForSeconds(1f);
        useMetronome = true;
        metronomeCoroutine = StartCoroutine(MetronomeLoop());
        rhythmCoroutine = StartCoroutine(PlayRhythm());
    }

    private IEnumerator MetronomeLoop()
    {
        while (useMetronome)
        {
            if (metronomeClip != null && metronomeSource != null)
                metronomeSource.PlayOneShot(metronomeClip);

            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator PlayRhythm()
    {
        expectedTimes.Clear();
        inputTimes.Clear();
        inputKeys.Clear();

        for (int i = 0; i < rhythmPattern.Count; i++)
        {
            if (rhythmPattern[i] != "0")
                audioSource.PlayOneShot(soundClip);

            yield return new WaitForSeconds(interval);
        }

        yield return StartCoroutine(PlayCountdownBeats());

        inputCoroutine = StartCoroutine(WaitForInputs());
    }

    private IEnumerator PlayCountdownBeats()
    {
        string[] countdown = { "3", "2", "1" };

        foreach (string c in countdown)
        {
            if (Num != null) Num.text = c;
            yield return new WaitForSeconds(interval);
        }

        if (Num != null) Num.text = "";
    }

    private IEnumerator WaitForInputs()
    {
        float inputStartTime = Time.time;

        for (int i = 0; i < rhythmPattern.Count; i++)
        {
            float expectedTime = inputStartTime + i * interval;
            expectedTimes.Add(expectedTime);

            float slotEnd = expectedTime + interval;
            bool inputReceived = false;
            float recordedTime = -1f;
            string pressedKeyString = "";

            while (Time.time < slotEnd)
            {
                if (!inputReceived)
                {
                    List<string> keysPressed = new List<string>();

                    foreach (KeyCode k in System.Enum.GetValues(typeof(KeyCode)))
                    {
                        if (Input.GetKey(k))
                            keysPressed.Add(k.ToString());
                    }

                    if (keysPressed.Count > 0)
                    {
                        keysPressed.Sort();
                        pressedKeyString = string.Join("", keysPressed);
                        inputReceived = true;
                        recordedTime = Time.time;

                        if (audioSource != null && inputClip != null)
                            audioSource.PlayOneShot(inputClip);
                    }
                }
                yield return null;
            }

            inputTimes.Add(inputReceived ? recordedTime : -1f);
            inputKeys.Add(inputReceived ? pressedKeyString : "");
        }

        useMetronome = false;
        JudgeResults();
    }

    private string SortString(string input)
    {
        return new string(input.ToCharArray().OrderBy(c => c).ToArray());
    }

    private void JudgeResults()
    {
        bool allCorrect = true;

        for (int i = 0; i < rhythmPattern.Count; i++)
        {
            string expectedKey = SortString(rhythmPattern[i]);
            string actualKey = SortString(inputKeys[i]);
            float expectedTime = expectedTimes[i];
            float actualTime = inputTimes[i];

            if (expectedKey == "0")
            {
                if (actualKey == "") continue;
                allCorrect = false;
            }
            else
            {
                if (actualKey == "")
                {
                    allCorrect = false;
                    continue;
                }

                if (actualKey != expectedKey)
                {
                    allCorrect = false;
                    continue;
                }

                float diff = Mathf.Abs(actualTime - expectedTime);
                if (diff > goodWindow)
                {
                    allCorrect = false;
                }
            }
        }
        if (keyText != null)
            keyText.text = "";
        //InGameManager.Instance.OnRhythmResult(allCorrect);
    }

    public void ForceStopAndFail()
    {
        if (rhythmCoroutine != null) StopCoroutine(rhythmCoroutine);
        if (inputCoroutine != null) StopCoroutine(inputCoroutine);
        if (metronomeCoroutine != null) StopCoroutine(metronomeCoroutine);

        useMetronome = false;
        //InGameManager.Instance.OnRhythmResult(false);
    }
}
