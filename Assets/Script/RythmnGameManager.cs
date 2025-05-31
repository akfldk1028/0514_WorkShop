using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmGameManager : MonoBehaviour
{
    [Header("리듬 설정")]
    private List<string> rhythmPattern = new List<string> { "A", "0", "0", "0", "D" }; // 1: 소리 있음, 0: 무음
    private float interval = 1f;

    [Header("사운드 설정")]
    public AudioClip soundClip;
    public AudioSource audioSource;

    [Header("판정 오차 범위")]
    private float perfectWindow = 0.2f;
    private float goodWindow = 0.3f;

    [Header("메트로놈 설정")]
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
        yield return new WaitForSeconds(1f); // 시작 전 준비 시간

        useMetronome = true;
        StartCoroutine(MetronomeLoop()); // 메트로놈 시작

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

        Debug.Log("리듬 재생 시작");

        for (int i = 0; i < rhythmPattern.Count; i++)
        {
            if (rhythmPattern[i] != "0")
            {
                audioSource.PlayOneShot(soundClip);
                Debug.Log($"비트 {i + 1}: 소리");
            }
            else
            {
                Debug.Log($"비트 {i + 1}: 무음");
            }

            yield return new WaitForSeconds(interval);
        }

        Debug.Log("입력 시작");
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

        useMetronome = false; // 메트로놈 정지
        JudgeResults();
    }

    void JudgeResults()
    {
        Debug.Log("결과 판정:");
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
                    Debug.Log($"[{i + 1}] 정답 (무음 + 입력 없음)");
                }
                else
                {
                    Debug.Log($"[{i + 1}] 오답 (무음인데 입력함)");
                    allCorrect = false;
                }
            }
            else
            {
                if (actualTime == -1f)
                {
                    Debug.Log($"[{i + 1}] Miss (입력 없음)");
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
            Debug.Log("전체 정답.");
        else
            Debug.Log("오답 포함.");
    }
}
