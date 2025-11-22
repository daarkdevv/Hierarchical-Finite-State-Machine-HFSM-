using System.Threading.Tasks;

public abstract class IActivity
{
    public virtual async Task PerformActivity()
    {
        await Task.CompletedTask;
    }
}
