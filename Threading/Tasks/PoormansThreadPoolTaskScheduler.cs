using System.Collections.Generic;
using System.Threading.Tasks;

namespace PoormansTPL.Threading.Tasks
{
    public class PoormansThreadPoolTaskScheduler : TaskScheduler
    {
        protected override void QueueTask(Task task)
        {
            PoormansThreadPool.EnqueueTaskInternal(() => base.TryExecuteTask(task));
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return base.TryExecuteTask(task);
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            throw new System.NotImplementedException();
        }
    }
}