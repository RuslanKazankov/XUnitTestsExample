using FluentAssertions;
using HomeworkApp.Dal.Entities;
using HomeworkApp.Dal.Models;
using HomeworkApp.Dal.Repositories.Interfaces;
using HomeworkApp.IntegrationTests.Fakers;
using HomeworkApp.IntegrationTests.Fixtures;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace HomeworkApp.IntegrationTests.RepositoryTests;

[Collection(nameof(TestFixture))]

public class TaskCommentRepositoryTests
{
    private readonly ITaskCommentRepository _repository;
    private readonly ITaskRepository _taskRepository;

    public TaskCommentRepositoryTests(TestFixture fixture)
    {
        _repository = fixture.TaskCommentRepository;
        _taskRepository = fixture.TaskRepository;
    }

    [Fact]
    public async Task Add_TaskComment_Success()
    {
        // Arrange
        var taskComment = TaskCommentEntityV1Faker.Generate()
            .Single();

        // Act
        var id = await _repository.Add(taskComment, default);

        // Asserts
        id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SetDeleted_TaskComment_Success()
    {
        // Arrange
        var task = TaskEntityV1Faker.Generate();
        var taskId = (await _taskRepository.Add(task, default))
            .Single();

        var taskComment = TaskCommentEntityV1Faker.Generate()
            .Single()
            .WithTaskId(taskId);
        var id = await _repository.Add(taskComment, default);

        // Act
        await _repository.SetDeleted(id, default);

        // Asserts
        var resultComment = (await _repository.Get(new TaskCommentGetModel()
            {
                IncludeDeleted = true,
                TaskId = taskId
            },
            default))
            .Single();
        resultComment.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Get_TaskComments_Success()
    {
        // Arrange
        var task = TaskEntityV1Faker.Generate();
        var taskId = (await _taskRepository.Add(task, default))
            .Single();

        var count = 2;
        var taskComments = TaskCommentEntityV1Faker.Generate(count)
            .Select(x => x.WithTaskId(taskId))
            .ToArray();
        var firstId = await _repository.Add(taskComments[0], default);
        var secondId = await _repository.Add(taskComments[1], default);

        var commentsGetModel = new TaskCommentGetModel
        {
            TaskId = taskId, 
            IncludeDeleted = true
        };

        // Act
        var results = await _repository.Get(commentsGetModel, default);

        // Asserts
        results.Should().HaveCount(count);

        results.Should().BeEquivalentTo(taskComments, options => options
            .Including(x => x.TaskId)
            .Including(x => x.Message));
    }

    [Fact]
    public async Task Get_TaskComments_NotIncludeDeleted_Success()
    {
        // Arrange
        var tasks = TaskEntityV1Faker.Generate();
        var taskId = (await _taskRepository.Add(tasks, default))
            .Single();

        var count = 2;
        var taskComments = TaskCommentEntityV1Faker.Generate(count)
            .Select(x => x.WithTaskId(taskId))
            .ToArray();
        var firstId = await _repository.Add(taskComments[0], default);
        var secondId = await _repository.Add(taskComments[1], default);

        await _repository.SetDeleted(firstId, default);

        var commentsGetModel = new TaskCommentGetModel
        {
            TaskId = taskId, 
            IncludeDeleted = false
        };

        // Act
        var results = await _repository.Get(commentsGetModel, default);

        // Asserts
        results.Should().OnlyContain(x => x.DeletedAt == null);
    }

    [Fact]
    public async Task Update_TaskComment_Success()
    {
        // Arrange
        var tasks = TaskEntityV1Faker.Generate();
        var taskId = (await _taskRepository.Add(tasks, default))
            .Single();

        const string oldMessage = "oldMessage";
        const string updateMessage = "updateMessage";

        var taskComment = TaskCommentEntityV1Faker.Generate()
            .Single()
            .WithTaskId(taskId)
            .WithMessage(oldMessage);
        var taskCommentId = await _repository.Add(taskComment, default);

        var updateComment = taskComment
            .WithMessage(updateMessage)
            .WithId(taskCommentId);

        // Act
        await _repository.Update(updateComment, default);

        // Asserts
        var result = (await _repository.Get(new TaskCommentGetModel()
        {
            TaskId = taskId,
            IncludeDeleted = true
        }, default))
            .Single();

        result.Message.Should().Be(updateMessage);
        result.ModifiedAt.Should().NotBeNull();
    }
}
