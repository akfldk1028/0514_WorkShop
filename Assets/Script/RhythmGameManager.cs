using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.UI;

public enum RhythmResult
{
    Fail,
    Good,
    Perfect
}

public class RhythmGameManager : MonoBehaviour
{
    [Header("Sound Setting")]
    public AudioClip soundClip;
    public AudioClip inputClip;
    public AudioSource audioSource;

    [Header("Metronome Setting")]
    public AudioClip metronomeClip;
    public AudioSource metronomeSource;

    [Header("Countdown TMP Text")]
    public TextMeshProUGUI Num;

    [Header("Jundgment Range")]
    [SerializeField] private float perfectWindow = 0.15f;
    [SerializeField] private float goodWindow = 0.3f;

    [Header("Rythmn Key Setting")]
    [SerializeField] private float interval = 0.65f;
    [SerializeField] private List<string> rhythmPattern = new List<string> { "A", "S", "AD", "_", "D", "_", "D", "A" };

    private List<float> expectedTimes = new List<float>();
    private List<float> inputTimes = new List<float>();
    private List<string> inputKeys = new List<string>();

    [Header("Key UI")]
    public GameObject KeyUI;
    public List<Sprite> keySprites; // 순서: A, S, D, E, Z (Z: Space)
    public List<string> keyLabels = new List<string> { "A", "S", "D", "E", "Z" }; // Z: Space
    public List<Image> keyImages; // T1~T8
    private Dictionary<string, Sprite> keySpriteDict;

    private bool useMetronome = true;

    public TextMeshProUGUI recipeName;

    // 코루틴
    private Coroutine rhythmCoroutine;
    private Coroutine inputCoroutine;
    private Coroutine metronomeCoroutine;

    private void Start()
    {
        KeyUI.SetActive(false);
    }

    private void InitKeySpriteDict()
    {
        if (keySpriteDict != null) return;

        keySpriteDict = new Dictionary<string, Sprite>();
        for (int i = 0; i < keyLabels.Count; i++)
        {
            keySpriteDict[keyLabels[i]] = keySprites[i];
        }
    }

    public void ShowKeyCombinationUI(List<string> keys)
    {
        InitKeySpriteDict();

        for (int i = 0; i < keyImages.Count; i++)
        {
            if (i < keys.Count && keys[i] != "_")
            {
                string key = keys[i].ToUpper();

                if (keySpriteDict.ContainsKey(key))
                {
                    keyImages[i].sprite = keySpriteDict[key];
                    keyImages[i].gameObject.SetActive(true);
                }
                else
                {
                    keyImages[i].gameObject.SetActive(false);
                }
            }
            else
            {
                keyImages[i].gameObject.SetActive(false);
            }
        }
    }

    public void StartRhythmSequence()
    {
        Data.RecipeData data = Managers.Ingame.getRandomRecipe();
        Debug.Log(data.RecipeName);
        Debug.Log(string.Join(", ", data.KeyCombination));

        rhythmPattern = new List<string>(data.KeyCombination);
        Debug.Log("rhythmPattern:" + string.Join(", ", rhythmPattern));

        recipeName.text = data.RecipeName;

        ShowKeyCombinationUI(data.KeyCombination);
        TrimTrailingSilence();
        rhythmCoroutine = StartCoroutine(InitAndStart());
        KeyUI.SetActive(true);
    }

    private void TrimTrailingSilence()
    {
        for (int i = rhythmPattern.Count - 1; i >= 0; i--)
        {
            if (rhythmPattern[i] != "_")
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
            if (rhythmPattern[i] != "_")
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
                        {
                            if (k == KeyCode.Space)
                                keysPressed.Add("Z");
                            else
                                keysPressed.Add(k.ToString().ToUpper());
                        }
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
        bool allPerfect = true;

        for (int i = 0; i < rhythmPattern.Count; i++)
        {
            string expectedKey = SortString(rhythmPattern[i].ToUpper());
            string actualKey = SortString(inputKeys[i].ToUpper());
            float expectedTime = expectedTimes[i];
            float actualTime = inputTimes[i];

            Debug.Log($"[{i}] expected: {expectedKey}, actual: {actualKey}");

            if (expectedKey == "_")
            {
                if (actualKey != "") allCorrect = false;
                continue;
            }

            if (actualKey == "" || actualKey != expectedKey)
            {
                allCorrect = false;
                continue;
            }

            float diff = Mathf.Abs(actualTime - expectedTime);
            if (diff > goodWindow)
            {
                allCorrect = false;
            }
            else if (diff > perfectWindow)
            {
                allPerfect = false;
            }
        }

        Debug.Log("입력한 키 값: " + string.Join(", ", inputKeys));

        if (!allCorrect)
        {
            InGameManager.Instance.EndRhythmGame(RhythmResult.Fail);
        }
        else if (allPerfect)
        {
            InGameManager.Instance.EndRhythmGame(RhythmResult.Perfect);
        }
        else
        {
            InGameManager.Instance.EndRhythmGame(RhythmResult.Good);
        }
    }

    public void ForceStopAndFail()
    {
        if (rhythmCoroutine != null) StopCoroutine(rhythmCoroutine);
        if (inputCoroutine != null) StopCoroutine(inputCoroutine);
        if (metronomeCoroutine != null) StopCoroutine(metronomeCoroutine);

        useMetronome = false;
        InGameManager.Instance.EndRhythmGame(RhythmResult.Fail);
    }
}
