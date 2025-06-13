using UnityEngine;
using DG.Tweening;
using TMPro;

/// <summary>
/// UI 애니메이션을 전담하는 컨트롤러 클래스
/// </summary>
public class UIAnimationController
{
    /// <summary>
    /// 골드 증가 애니메이션 - 화끈한 버전! 🔥💰
    /// </summary>
    public static void AnimateGoldIncrease(TMP_Text goldText, int oldValue, int newValue)
    {
        // 🔥 화끈한 DoTween 숫자 카운팅 애니메이션
        DOTween.To(() => oldValue, x => {
            goldText.text = x.ToString("N0");
        }, newValue, 1.5f)
        .SetEase(Ease.OutBounce)
        .OnComplete(() => {
            goldText.text = newValue.ToString("N0");
        });
        
        // 🎆 이중 스케일 펀치 효과
        goldText.transform.DOPunchScale(Vector3.one * 0.4f, 0.8f, 8, 0.8f)
            .OnComplete(() => {
                goldText.transform.DOPunchScale(Vector3.one * 0.2f, 0.4f, 4, 0.5f);
            });
        
        // 🌈 색상 변화 시퀀스
        Sequence colorSequence = DOTween.Sequence();
        colorSequence.Append(goldText.DOColor(Color.yellow, 0.3f))
                    .Append(goldText.DOColor(new Color(1f, 0.5f, 0f), 0.3f))
                    .Append(goldText.DOColor(Color.red, 0.3f))
                    .Append(goldText.DOColor(Color.black, 0.6f));
        
        // ✨ 반짝이는 효과
        DOTween.To(() => goldText.color.a, x => {
            Color currentColor = goldText.color;
            currentColor.a = x;
            goldText.color = currentColor;
        }, 0.3f, 0.2f)
        .SetLoops(6, LoopType.Yoyo)
        .SetDelay(0.5f);
        
        // 🎊 회전 흔들림 효과
        goldText.transform.DOShakeRotation(1.0f, new Vector3(0, 0, 10f), 10, 90f);
    }
    
    /// <summary>
    /// 골드 감소 애니메이션 - 돈이 나가는 느낌! 💸😱
    /// </summary>
    public static void AnimateGoldDecrease(TMP_Text goldText, int oldValue, int newValue)
    {
        // 💸 아쉬운 DoTween 숫자 카운팅 애니메이션 (더 빠르게 떨어짐)
        DOTween.To(() => oldValue, x => {
            goldText.text = x.ToString("N0");
        }, newValue, 1.0f) // 1초로 더 빠르게
        .SetEase(Ease.InQuart) // 가속하며 떨어지는 느낌
        .OnComplete(() => {
            goldText.text = newValue.ToString("N0");
        });
        
        // 😱 충격적인 스케일 효과 (움츠러들었다가 원래대로)
        goldText.transform.DOScale(0.7f, 0.3f)
            .SetEase(Ease.InBack)
            .OnComplete(() => {
                goldText.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBounce);
            });
        
        // 💔 돈이 나가는 색상 변화 (회색 → 빨간색 → 어두운 빨간색 → 검정)
        Sequence colorSequence = DOTween.Sequence();
        colorSequence.Append(goldText.DOColor(Color.gray, 0.2f))
                    .Append(goldText.DOColor(Color.red, 0.3f))
                    .Append(goldText.DOColor(new Color(0.8f, 0f, 0f), 0.3f))
                    .Append(goldText.DOColor(Color.black, 0.4f));
        
        // 😰 떨리는 효과 (돈이 나가는 충격!)
        goldText.transform.DOShakePosition(0.8f, new Vector3(15f, 10f, 0f), 15, 90f)
            .SetDelay(0.2f);
        
        // 💸 사라지는 듯한 알파 효과
        DOTween.To(() => goldText.color.a, x => {
            Color currentColor = goldText.color;
            currentColor.a = x;
            goldText.color = currentColor;
        }, 0.5f, 0.15f)
        .SetLoops(4, LoopType.Yoyo)
        .SetDelay(0.3f);
        
        Debug.Log($"<color=red>💸😱 돈이 나가는 애니메이션!</color> {oldValue:N0} → {newValue:N0}");
    }
    
    /// <summary>
    /// 유리잔 개수 변화 애니메이션 - 시원한 버전! 🥃✨
    /// </summary>
    public static void AnimateGlassUpdate(TMP_Text glassText, int oldValue, int newValue)
    {
        // 🥃 시원한 숫자 카운팅
        DOTween.To(() => oldValue, x => {
            glassText.text = $"{x}개";
        }, newValue, 1.0f)
        .SetEase(Ease.OutQuart);
        
        // 💎 크리스탈 같은 반짝임 (파란색 계열)
        Sequence colorSequence = DOTween.Sequence();
        colorSequence.Append(glassText.DOColor(Color.cyan, 0.2f))
                    .Append(glassText.DOColor(new Color(0.5f, 0.8f, 1f), 0.2f))
                    .Append(glassText.DOColor(Color.black, 0.4f));
        
        // 🎯 부드러운 스케일 효과
        glassText.transform.DOPunchScale(Vector3.one * 0.25f, 0.6f, 6, 0.6f);
        
        Debug.Log($"<color=cyan>🥃✨ 유리잔 애니메이션!</color> {oldValue} → {newValue}");
    }
    
    // 애니메이션 중복 실행 방지용 플래그들
    private static bool _isOrderAnimating = false;
    private static bool _isRecipeAnimating = false;
    
    /// <summary>
    /// 주문 텍스트 업데이트 애니메이션 - 옆에서 삭삭삭 밀려드는 모던 버전! 📋💫
    /// </summary>
    public static void AnimateOrderUpdate(TMP_Text orderText, string newText)
    {
        // 🛡️ 애니메이션 중복 실행 방지
        if (_isOrderAnimating)
        {
            Debug.Log("<color=yellow>📋 주문 애니메이션 이미 실행 중 - 스킵</color>");
            orderText.text = newText; // 텍스트만 즉시 변경
            return;
        }
        
        _isOrderAnimating = true;
        
        RectTransform rectTransform = orderText.GetComponent<RectTransform>();
        
        // 🔧 기존 애니메이션 완전 정리 및 강제 위치 리셋!
        rectTransform.DOKill(true); // 완전 중단
        orderText.transform.DOKill(true); // 완전 중단
        
        // 💪 원래 위치로 강제 리셋 (애니메이션 없이 즉시)
        Vector2 originalPos = new Vector2(0, rectTransform.anchoredPosition.y); // X는 0으로 고정
        rectTransform.anchoredPosition = originalPos;
        orderText.transform.localScale = Vector3.one; // 스케일도 리셋
        
        // 📋💨 현재 텍스트를 오른쪽으로 밀어내기 (가속도 붙으면서 쭉~!)
        rectTransform.DOAnchorPosX(originalPos.x + 350f, 0.25f)
            .SetEase(Ease.InExpo)
            .OnComplete(() => {
                // 텍스트 변경
                orderText.text = newText;
                
                // 왼쪽에서 새로운 텍스트가 쭉~ 밀려들어오기
                rectTransform.anchoredPosition = new Vector2(originalPos.x - 350f, originalPos.y);
                rectTransform.DOAnchorPosX(originalPos.x, 0.35f)
                    .SetEase(Ease.OutExpo)
                    .OnComplete(() => {
                        // 🏁 애니메이션 완료 - 플래그 해제
                        _isOrderAnimating = false;
                        // 최종 위치 확실히 고정
                        rectTransform.anchoredPosition = originalPos;
                    });
            });
        
        // 💫 모던한 색상 변화 (사이버 블루 → 네온 퍼플 → 검정)
        Sequence colorSequence = DOTween.Sequence();
        colorSequence.SetDelay(0.25f)
                    .Append(orderText.DOColor(new Color(0.4f, 0.8f, 1f), 0.2f))
                    .Append(orderText.DOColor(new Color(0.6f, 0.4f, 1f), 0.2f))
                    .Append(orderText.DOColor(Color.black, 0.3f));
        
        // 💥 들어올 때 살짝 커졌다가 원래 크기로 (임팩트!)
        DOTween.Sequence()
            .SetDelay(0.25f)
            .Append(orderText.transform.DOScale(1.15f, 0.15f))
            .Append(orderText.transform.DOScale(1f, 0.2f).SetEase(Ease.OutQuart));
        
        Debug.Log($"<color=cyan>📋💫 모던한 주문 슬라이드!</color> 새 주문: {newText}");
    }
    
    /// <summary>
    /// 레시피 텍스트 업데이트 애니메이션 - 삭삭삭 슬라이드 모던 버전! 🔥💫
    /// </summary>
    public static void AnimateRecipeUpdate(TMP_Text recipeText, string newText)
    {
        // 🛡️ 애니메이션 중복 실행 방지
        if (_isRecipeAnimating)
        {
            Debug.Log("<color=yellow>🔥 레시피 애니메이션 이미 실행 중 - 스킵</color>");
            recipeText.text = newText; // 텍스트만 즉시 변경
            return;
        }
        
        _isRecipeAnimating = true;
        
        RectTransform rectTransform = recipeText.GetComponent<RectTransform>();
        
        // 🔧 기존 애니메이션 완전 정리 및 강제 위치 리셋!
        rectTransform.DOKill(true); // 완전 중단
        recipeText.transform.DOKill(true); // 완전 중단
        
        // 💪 원래 위치로 강제 리셋 (애니메이션 없이 즉시)
        Vector2 originalPos = new Vector2(0, rectTransform.anchoredPosition.y); // X는 0으로 고정
        rectTransform.anchoredPosition = originalPos;
        recipeText.transform.localScale = Vector3.one; // 스케일도 리셋
        recipeText.transform.rotation = Quaternion.identity; // 회전도 리셋
        
        // 🔥💨 현재 텍스트를 왼쪽으로 밀어내기 (가속도 붙으면서 쭉~!)
        rectTransform.DOAnchorPosX(originalPos.x - 350f, 0.25f)
            .SetEase(Ease.InExpo)
            .OnComplete(() => {
                // 텍스트 변경
                recipeText.text = newText;
                
                // 오른쪽에서 새로운 텍스트가 쭉~ 밀려들어오기
                rectTransform.anchoredPosition = new Vector2(originalPos.x + 350f, originalPos.y);
                rectTransform.DOAnchorPosX(originalPos.x, 0.35f)
                    .SetEase(Ease.OutExpo)
                    .OnComplete(() => {
                        // 🏁 애니메이션 완료 - 플래그 해제
                        _isRecipeAnimating = false;
                        // 최종 위치 확실히 고정
                        rectTransform.anchoredPosition = originalPos;
                    });
            });
        
        // 🔥💫 모던한 요리 색상 변화 (네온 오렌지 → 핫 핑크 → 검정)
        Sequence colorSequence = DOTween.Sequence();
        colorSequence.SetDelay(0.25f)
                    .Append(recipeText.DOColor(new Color(1f, 0.5f, 0.2f), 0.2f))
                    .Append(recipeText.DOColor(new Color(1f, 0.3f, 0.7f), 0.2f))
                    .Append(recipeText.DOColor(Color.black, 0.3f));
        
        // 💥 들어올 때 살짝 커졌다가 원래 크기로 (임팩트!)
        DOTween.Sequence()
            .SetDelay(0.25f)
            .Append(recipeText.transform.DOScale(1.15f, 0.15f))
            .Append(recipeText.transform.DOScale(1f, 0.2f).SetEase(Ease.OutQuart));
        
        // 🎪 부드러운 회전 효과도 유지 (살짝만)
        DOTween.Sequence()
            .SetDelay(0.25f)
            .Append(recipeText.transform.DORotate(new Vector3(0, 0, 5f), 0.2f))
            .Append(recipeText.transform.DORotate(Vector3.zero, 0.2f).SetEase(Ease.OutQuart));
        
        Debug.Log($"<color=magenta>🔥💫 모던한 레시피 슬라이드!</color> 새 레시피: {newText}");
    }
} 