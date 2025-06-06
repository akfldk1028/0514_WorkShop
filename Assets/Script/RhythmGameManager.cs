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
    // Sound related variables
    [Header("Sound Setting")]
    public AudioClip soundClip;
    public AudioSource audioSource;

    [Header("Key Sound Settings")]
    public AudioClip soundA;  // Sound for key A
    public AudioClip soundS;  // Sound for key S
    public AudioClip soundD;  // Sound for key D
    public AudioClip soundW;  // Sound for key W
    public AudioClip soundZ;  // Sound for Space key
    private Dictionary<string, AudioClip> keySoundDict;

    [Header("Metronome Setting")]
    public AudioClip metronomeClip;
    public AudioSource metronomeSource;

    [Header("Countdown TMP Text")]
    public TextMeshProUGUI Num;

    [Header("Judgment Range")]
    [SerializeField] private float perfectWindow = 0.15f;
    [SerializeField] private float goodWindow = 0.3f;

    [Header("Rhythm Key Setting")]
    [SerializeField] private float interval = 0.65f;
    [SerializeField] private List<string> rhythmPattern = new List<string> { "A", "S", "AD", "_", "D", "_", "D", "A" };

    private List<float> expectedTimes = new List<float>();
    private List<float> inputTimes = new List<float>();
    private List<string> inputKeys = new List<string>();

    [Header("Key UI")]
    public GameObject KeyUI;
    public List<Sprite> keySprites;  // Order: A, S, D, W, Z (Z: Space)
    public List<string> keyLabels = new List<string> { "A", "S", "D", "W", "Z" };  // Z represents Space
    public List<Image> keyImages;  // T1~T8
    private Dictionary<string, Sprite> keySpriteDict;

    private bool useMetronome = true;

    [Header("Rhythm Game UI")]
    public TextMeshProUGUI recipeName;
    public TextMeshProUGUI resultText;

    // Coroutine references
    private Coroutine rhythmCoroutine;
    private Coroutine inputCoroutine;
    private Coroutine metronomeCoroutine;

    // Current recipe being played
    private Data.RecipeData currentRecipe;

    private void Start()
    {
        KeyUI.SetActive(false);
        resultText.gameObject.SetActive(false);  // 시작할 때 결과 텍스트 숨기기
        InitKeySoundDict();
    }

    private void InitKeySoundDict() // Initialize dictionary for key sounds
    {
        keySoundDict = new Dictionary<string, AudioClip>
        {
            { "A", soundA },
            { "S", soundS },
            { "D", soundD },
            { "W", soundW },
            { "Z", soundZ }
        };
    }

    private void InitKeySpriteDict() // Initialize dictionary for key sprites
    {
        if (keySpriteDict != null) return;

        keySpriteDict = new Dictionary<string, Sprite>();
        for (int i = 0; i < keyLabels.Count; i++)
        {
            keySpriteDict[keyLabels[i]] = keySprites[i];
        }
    }

    public void ShowKeyCombinationUI(List<string> keys) //show key combination ui
    {
        InitKeySpriteDict();

        for (int i = 0; i < keyImages.Count; i++)
        {
            if (i < keys.Count)
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
        }
    }

    private IEnumerator WaitASecond() // Wait for 1 second
    {
        yield return new WaitForSeconds(1f);
    }

    public void StartRhythmSequence()
    {
        WaitASecond();

        // 이전에 실패한 레시피가 없을 경우에만 새로운 레시피 가져오기
        if (currentRecipe == null)
        {
            currentRecipe = Managers.Ingame.getRandomRecipe();
        }

        Debug.Log(currentRecipe.RecipeName);
        Debug.Log(string.Join(", ", currentRecipe.KeyCombination));

        rhythmPattern = new List<string>(currentRecipe.KeyCombination);
        Debug.Log("rhythmPattern:" + string.Join(", ", rhythmPattern));

        recipeName.text = currentRecipe.RecipeName;

        ShowKeyCombinationUI(currentRecipe.KeyCombination);
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
            {
                string keyString = rhythmPattern[i][0].ToString().ToUpper();
                if (keySoundDict.ContainsKey(keyString) && keySoundDict[keyString] != null)
                {
                    audioSource.PlayOneShot(keySoundDict[keyString]);
                }
            }

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

    private void DimKeyUI(int index) // Dim the UI of pressed key
    {
        if (index >= 0 && index < keyImages.Count)
        {
            Image image = keyImages[index];
            Color color = image.color;
            color = new Color(color.r * 0.5f, color.g * 0.5f, color.b * 0.5f, 0.3f);
            image.color = color;
        }
    }

    private void RestoreKeyUI() // Restore UI brightness to original state
    {
        foreach (Image image in keyImages)
        {
            Color color = image.color;
            color = new Color(1f, 1f, 1f, 1f);
            image.color = color;
        }
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
                            string key = (k == KeyCode.Space) ? "Z" : k.ToString().ToUpper();
                            keysPressed.Add(key);
                            if (keySoundDict.ContainsKey(key) && keySoundDict[key] != null)
                            {
                                audioSource.PlayOneShot(keySoundDict[key]);
                            }
                        }
                    }

                    if (keysPressed.Count > 0)
                    {
                        keysPressed.Sort();
                        pressedKeyString = string.Join("", keysPressed);
                        inputReceived = true;
                        recordedTime = Time.time;
                    }

                    // 입력 패턴이 맞는지 확인하고 흐리게 처리
                    bool isCorrectInput = false;
                    if (rhythmPattern[i] == "_") //none input
                    {
                        if (!inputReceived)
                        {
                            isCorrectInput = true;
                            recordedTime = Time.time;
                        }
                    }
                    else if (inputReceived) //key input
                    {
                        string expectedKey = rhythmPattern[i].ToUpper();  // 패턴을 대문자로 변환
                        foreach (string key in keysPressed)
                        {
                            if (key == expectedKey)
                            {
                                isCorrectInput = true;
                                break;
                            }
                        }
                    }

                    if (isCorrectInput) //correct input
                    {
                        DimKeyUI(i);
                        inputReceived = true;
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
            StartCoroutine(HandleGameEnd(RhythmResult.Fail));
        }
        else if (allPerfect)
        {
            StartCoroutine(HandleGameEnd(RhythmResult.Perfect));
        }
        else
        {
            StartCoroutine(HandleGameEnd(RhythmResult.Good));
        }
    }

    public void ForceStopAndFail()
    {
        if (rhythmCoroutine != null) StopCoroutine(rhythmCoroutine);
        if (inputCoroutine != null) StopCoroutine(inputCoroutine);
        if (metronomeCoroutine != null) StopCoroutine(metronomeCoroutine);

        useMetronome = false;
        StartCoroutine(HandleGameEnd(RhythmResult.Fail));
    }

    private IEnumerator HandleGameEnd(RhythmResult result) // Handle game end state and result display
    {
        yield return WaitASecond();
        KeyUI.SetActive(false);
        
        // Show result text with color
        resultText.gameObject.SetActive(true);
        if (result == RhythmResult.Fail)
        {
            resultText.text = "Bad";
            resultText.color = Color.red;
            
            // Restart rhythm game if fail
            yield return WaitASecond();  // Show result text briefly
            RestoreKeyUI();
            resultText.gameObject.SetActive(false);
            StartRhythmSequence();  // Restart with same recipe
        }
        else
        {
            resultText.text = "Clear";
            resultText.color = Color.blue;
            currentRecipe = null;  // Clear current recipe on success
            
            // Wait for space input only on success
            while (!Input.GetKeyDown(KeyCode.Space))
            {
                yield return null;
            }

            // Prepare for next sequence
            RestoreKeyUI();
            KeyUI.SetActive(true);
            resultText.gameObject.SetActive(false);
            
            // Send result to game manager
            Managers.Ingame.EndRhythmGame(result);
        }
    }

}
