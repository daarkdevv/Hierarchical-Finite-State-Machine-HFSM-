using System.Collections.Generic;
using System.Threading.Tasks;

public class Sequential
{
    public bool IsDone { get; private set; }
    List<IActivity> activities;

    public Sequential(List<IActivity> activities)
    {
        this.activities = activities;
    }

    public async Task RunAsync()
    {
        if (IsDone) return;

        int index;

        for (index = 0; index < activities.Count; index++)
        {
            var act = activities[index];
            if (act != null)
                await act.PerformActivity();
        }
        IsDone = true;
    }
}
