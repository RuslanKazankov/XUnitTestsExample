using Dapper;
using HomeworkApp.Dal.Entities;
using HomeworkApp.Dal.Models;
using HomeworkApp.Dal.Repositories.Interfaces;
using HomeworkApp.Dal.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace HomeworkApp.Dal.Repositories;
public class TaskCommenRepository : PgRepository, ITaskCommentRepository
{
    public TaskCommenRepository(
        IOptions<DalOptions> dalSettings) : base(dalSettings.Value)
    {
    }

    public async Task<long> Add(TaskCommentEntityV1 model, CancellationToken token)
    {
        const string sqlQuery = @"
insert into task_comments (task_id, author_user_id, message, at, modified_at, deleted_at)
select task_id, author_user_id, message, at, modified_at, deleted_at
  from UNNEST(@TaskComment)
returning id;
";

        await using var connection = await GetConnection();
        var ids = await connection.QueryAsync<long>(
            new CommandDefinition(
                sqlQuery,
                new
                {
                    TaskComment = model
                },
                cancellationToken: token));

        return ids.Single();
    }

    public async Task<TaskCommentEntityV1[]> Get(TaskCommentGetModel model, CancellationToken token)
    {
        const string sqlQuery = @"
select id
     , task_id
     , author_user_id
     , message
     , at
     , modified_at
     , deleted_at
  from task_comments
 where task_id = @TaskCommentId
   and (deleted_at is null = @IncludeDeleted 
       or deleted_at is not null)
 order by at desc
";

        await using var connection = await GetConnection();
        var taskComments = await connection.QueryAsync<TaskCommentEntityV1>(
            new CommandDefinition(
                sqlQuery,
                new
                {
                    TaskCommentId = model.TaskId,
                    IncludeDeleted = model.IncludeDeleted
                },
                cancellationToken: token
            ));

        return taskComments.ToArray();
    }

    public async Task SetDeleted(long taskCommentId, CancellationToken token)
    {
        const string sqlQuery = @"
update task_comments
   set deleted_at = @DeletedAt
 where id = @Id        
";

        await using var connection = await GetConnection();
        await connection.QueryAsync<TaskCommentEntityV1>(
            new CommandDefinition(
                sqlQuery,
                new
                {
                    Id = taskCommentId,
                    DeletedAt = DateTimeOffset.UtcNow,
                },
                cancellationToken: token
            ));
    }

    public async Task Update(TaskCommentEntityV1 model, CancellationToken token)
    {
        const string sqlQuery = @"
update task_comments
   set message = @Message
     , modified_at = @ModifiedAt
 where id = @Id        
";

        await using var connection = await GetConnection();
        await connection.QueryAsync<TaskCommentEntityV1>(
            new CommandDefinition(
                sqlQuery,
                new
                {
                    Id = model.Id,
                    Message = model.Message,
                    ModifiedAt = DateTimeOffset.UtcNow,
                },
                cancellationToken: token
            ));
    }
}
