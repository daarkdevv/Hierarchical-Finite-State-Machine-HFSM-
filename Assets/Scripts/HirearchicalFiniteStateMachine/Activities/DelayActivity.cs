using System.Threading.Tasks;
using UnityEngine;

public class DelayActivity : IActivity
{
    float seconds;
    public DelayActivity(float seconds)
    {
        this.seconds = seconds;
    }

    public override async Task PerformActivity()
    {
        Debug.Log("Waiting");
        await Awaitable.WaitForSecondsAsync(seconds);
        Debug.Log("Finished Waiting");

        await base.PerformActivity();
    }
}
