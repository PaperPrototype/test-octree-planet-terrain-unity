
using System;
using Unity.Jobs;

public class JobCompleter
{
    public Func<JobHandle> schedule;
    public Action onComplete;
    public JobCompleter(Func<JobHandle> schedule, Action onComplete)
    {
        this.schedule = schedule;
        this.onComplete = onComplete;
    }
}