using HomeworkApp.Dal.Entities;
using HomeworkApp.Dal.Models;
using System.Threading;
using System.Threading.Tasks;

namespace HomeworkApp.Dal.Repositories.Interfaces;
public interface ITaskCommentRepository
{
    Task<long> Add(TaskCommentEntityV1 model, CancellationToken token);
    Task Update(TaskCommentEntityV1 model, CancellationToken token);
    Task SetDeleted(long taskCommentId, CancellationToken token);
    Task<TaskCommentEntityV1[]> Get(TaskCommentGetModel model, CancellationToken token);

}
