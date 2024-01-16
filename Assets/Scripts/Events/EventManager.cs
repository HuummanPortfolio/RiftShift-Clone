using UnityEngine.Events;

public class EventManager
{
    public static UnityAction OnTimerStart;
    public static UnityAction<bool> OnTimerPause;
    public static UnityAction OnTimerEnd;
}
