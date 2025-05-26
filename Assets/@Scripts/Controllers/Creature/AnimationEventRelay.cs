using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    public Customer customer;

    // 애니메이션 이벤트에서 호출
    public void SiparisStateGec()
    {
        if (customer != null)
            customer.SiparisStateGec();
    }

    public void SiparisVer()
    {
        if (customer != null)
            customer.SiparisVer();
    }
}