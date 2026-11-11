using UnityEngine;

public class TimeSystem : MonoBehaviour
{
    private const int EnergyPerPeriod = 100;

    public bool SpendEnergy(int amount)
    {
        if (GameSession.Instance == null || amount < 0) return false;
        SessionData data = GameSession.Instance.Data;
        int day = data.DayNumber;
        TimePeriod period = data.Period;
        int energy = data.Energy - amount;

        while (energy <= 0)
        {
            if (period == TimePeriod.Day)
            {
                period = TimePeriod.Night;
                UIManager.ShowToast("Наступила ночь");
            }
            else
            {
                period = TimePeriod.Day;
                day++;
                UIManager.ShowToast("Наступил день " + day);
            }
            energy += EnergyPerPeriod;
        }

        data.SetTime(energy, day, period);
        return true;
    }
}
