using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmGameManager : MonoBehaviour
{
    [Header("���� ����")]
    private List<string> rhythmPattern = new List<string> { "A", "0", "0", "0", "D" }; // 1: �Ҹ� ����, 0: ����
    private float interval = 1f;

    [Header("���� ����")]
    public AudioClip soundClip;
    public AudioSource audioSource;

    [Header("���� ���� ����")]
    private float perfectWindow = 0.2f;
    private float goodWindow = 0.3f;

    [Header("��Ʈ�γ� ����")]
    public AudioClip metronomeClip;
    public AudioSource metronomeSource;

    private List<float> expectedTimes = new List<float>();
    private List<float> inputTimes = new List<float>();
    private bool useMetronome = true;

    void Start()
    {
        StartCoroutine(InitAndStart());
    }

    IEnumerator InitAndStart()
    {
        yield return new WaitForSeconds(1f); // ���� �� �غ� �ð�

        useMetronome = true;
        StartCoroutine(MetronomeLoop()); // ��Ʈ�γ� ����

        StartCoroutine(PlayRhythm());
    }

    IEnumerator MetronomeLoop()
    {
        while (useMetronome)
        {
            if (metronomeClip != null && metronomeSource != null)
            {
                metronomeSource.PlayOneShot(metronomeClip);
            }
            yield return new WaitForSeconds(interval);
        }
    }

    IEnumerator PlayRhythm()
    {
        expectedTimes.Clear();
        inputTimes.Clear();

        Debug.Log("���� ��� ����");

        for (int i = 0; i < rhythmPattern.Count; i++)
        {
            if (rhythmPattern[i] != "0")
            {
                audioSource.PlayOneShot(soundClip);
                Debug.Log($"��Ʈ {i + 1}: �Ҹ�");
            }
            else
            {
                Debug.Log($"��Ʈ {i + 1}: ����");
            }

            yield return new WaitForSeconds(interval);
        }

        Debug.Log("�Է� ����");
        StartCoroutine(WaitForInputs());
    }

    IEnumerator WaitForInputs()
    {
        float inputStartTime = Time.time;

        for (int i = 0; i < rhythmPattern.Count; i++)
        {
            float expectedTime = inputStartTime + i * interval;
            expectedTimes.Add(expectedTime);

            float slotEnd = expectedTime + interval;
            bool inputReceived = false;
            float recordedTime = -1f;

            while (Time.time < slotEnd)
            {
                if (!inputReceived && Input.GetKeyDown(KeyCode.Space))
                {
                    inputReceived = true;
                    recordedTime = Time.time;
                }
                yield return null;
            }

            inputTimes.Add(inputReceived ? recordedTime : -1f);
        }

        useMetronome = false; // ��Ʈ�γ� ����
        JudgeResults();
    }

    void JudgeResults()
    {
        Debug.Log("��� ����:");
        bool allCorrect = true;

        for (int i = 0; i < rhythmPattern.Count; i++)
        {
            bool isSound = rhythmPattern[i] != "0";
            float expectedTime = expectedTimes[i];
            float actualTime = inputTimes[i];

            if (!isSound)
            {
                if (actualTime == -1f)
                {
                    Debug.Log($"[{i + 1}] ���� (���� + �Է� ����)");
                }
                else
                {
                    Debug.Log($"[{i + 1}] ���� (�����ε� �Է���)");
                    allCorrect = false;
                }
            }
            else
            {
                if (actualTime == -1f)
                {
                    Debug.Log($"[{i + 1}] Miss (�Է� ����)");
                    allCorrect = false;
                }
                else
                {
                    float diff = Mathf.Abs(actualTime - expectedTime);

                    if (diff <= perfectWindow)
                        Debug.Log($"[{i + 1}] Perfect ({diff:F3}s)");
                    else if (diff <= goodWindow)
                    {
                        Debug.Log($"[{i + 1}] Good ({diff:F3}s)");
                        allCorrect = false;
                    }
                    else
                    {
                        Debug.Log($"[{i + 1}] Miss ({diff:F3}s)");
                        allCorrect = false;
                    }
                }
            }
        }

        if (allCorrect)
            Debug.Log("��ü ����.");
        else
            Debug.Log("���� ����.");
    }
}
