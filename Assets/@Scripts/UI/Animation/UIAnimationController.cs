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
    
    /// <summary>
    /// 주문 텍스트 업데이트 애니메이션 - 옆에서 삭삭삭 밀려드는 모던 버전! 📋💫
    /// </summary>
    public static void AnimateOrderUpdate(TMP_Text orderText, string newText)
    {
        RectTransform rectTransform = orderText.GetComponent<RectTransform>();
        Vector2 originalPos = rectTransform.anchoredPosition;
        
        // 📋💨 현재 텍스트를 오른쪽으로 밀어내기 (삭삭삭!)
        rectTransform.DOAnchorPosX(originalPos.x + 300f, 0.2f) // 오른쪽으로 300픽셀 밀어내기
            .SetEase(Ease.InQuart)
            .OnComplete(() => {
                // 텍스트 변경
                orderText.text = newText;
                
                // 왼쪽에서 새로운 텍스트가 밀려들어오기
                rectTransform.anchoredPosition = new Vector2(originalPos.x - 300f, originalPos.y);
                rectTransform.DOAnchorPosX(originalPos.x, 0.25f) // 원래 위치로 슬라이드
                    .SetEase(Ease.OutBack); // 살짝 튕기는 효과
            });
        
        // 💫 모던한 색상 변화 (사이버 블루 → 네온 퍼플 → 검정)
        Sequence colorSequence = DOTween.Sequence();
        colorSequence.SetDelay(0.2f) // 밀려들어올 때 시작
                    .Append(orderText.DOColor(new Color(0.4f, 0.8f, 1f), 0.15f)) // 사이버 블루
                    .Append(orderText.DOColor(new Color(0.6f, 0.4f, 1f), 0.15f)) // 네온 퍼플
                    .Append(orderText.DOColor(Color.black, 0.3f));
        
        // 💥 들어올 때 살짝 커졌다가 원래 크기로 (임팩트!)
        DOTween.Sequence()
            .SetDelay(0.2f)
            .Append(orderText.transform.DOScale(1.15f, 0.1f))
            .Append(orderText.transform.DOScale(1f, 0.15f).SetEase(Ease.OutQuart));
        
        Debug.Log($"<color=cyan>📋💫 모던한 주문 슬라이드!</color> 새 주문: {newText}");
    }
    
    /// <summary>
    /// 레시피 텍스트 업데이트 애니메이션 - 삭삭삭 슬라이드 모던 버전! 🔥💫
    /// </summary>
    public static void AnimateRecipeUpdate(TMP_Text recipeText, string newText)
    {
        RectTransform rectTransform = recipeText.GetComponent<RectTransform>();
        Vector2 originalPos = rectTransform.anchoredPosition;
        
        // 🔥💨 현재 텍스트를 왼쪽으로 밀어내기 (주문과 반대 방향!)
        rectTransform.DOAnchorPosX(originalPos.x - 300f, 0.2f) // 왼쪽으로 300픽셀 밀어내기
            .SetEase(Ease.InQuart)
            .OnComplete(() => {
                // 텍스트 변경
                recipeText.text = newText;
                
                // 오른쪽에서 새로운 텍스트가 밀려들어오기
                rectTransform.anchoredPosition = new Vector2(originalPos.x + 300f, originalPos.y);
                rectTransform.DOAnchorPosX(originalPos.x, 0.25f) // 원래 위치로 슬라이드
                    .SetEase(Ease.OutBack); // 살짝 튕기는 효과
            });
        
        // 🔥💫 모던한 요리 색상 변화 (네온 오렌지 → 핫 핑크 → 검정)
        Sequence colorSequence = DOTween.Sequence();
        colorSequence.SetDelay(0.2f) // 밀려들어올 때 시작
                    .Append(recipeText.DOColor(new Color(1f, 0.5f, 0.2f), 0.15f)) // 네온 오렌지
                    .Append(recipeText.DOColor(new Color(1f, 0.3f, 0.7f), 0.15f)) // 핫 핑크
                    .Append(recipeText.DOColor(Color.black, 0.3f));
        
        // 💥 들어올 때 살짝 커졌다가 원래 크기로 (임팩트!)
        DOTween.Sequence()
            .SetDelay(0.2f)
            .Append(recipeText.transform.DOScale(1.15f, 0.1f))
            .Append(recipeText.transform.DOScale(1f, 0.15f).SetEase(Ease.OutQuart));
        
        // 🎪 부드러운 회전 효과도 유지 (살짝만)
        DOTween.Sequence()
            .SetDelay(0.2f)
            .Append(recipeText.transform.DORotate(new Vector3(0, 0, 5f), 0.15f))
            .Append(recipeText.transform.DORotate(Vector3.zero, 0.15f).SetEase(Ease.OutQuart));
        
        Debug.Log($"<color=magenta>🔥💫 모던한 레시피 슬라이드!</color> 새 레시피: {newText}");
    }
} 