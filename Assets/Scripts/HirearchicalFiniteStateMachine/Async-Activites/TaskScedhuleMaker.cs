
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class TaskScedhuleMaker
{
    Queue<Func<Awaitable>> tasks;
    
    bool isRunning = false;

    public TaskScedhuleMaker()
    {
        tasks = new Queue<Func<Awaitable>>();
    }

   public async Awaitable AddTask(Func<Awaitable> task)
    {
        
        while (isRunning )
            await Awaitable.NextFrameAsync();

        // Once idle, enqueue and process
        tasks.Enqueue(task);

        isRunning = true;
        await TaskDoer(); // Wait for all queued tasks to finish
    }
   public async Awaitable TaskDoer()
    {
        while (tasks.Count > 0)
        {
            var currentTask = tasks.Dequeue();
            await currentTask();
        }

        isRunning = false;
        
    } 


}