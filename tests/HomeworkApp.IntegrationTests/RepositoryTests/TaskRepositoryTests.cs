using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using HomeworkApp.Dal.Models;
using HomeworkApp.Dal.Repositories.Interfaces;
using HomeworkApp.IntegrationTests.Creators;
using HomeworkApp.IntegrationTests.Fakers;
using HomeworkApp.IntegrationTests.Fixtures;
using Xunit;

namespace HomeworkApp.IntegrationTests.RepositoryTests;

[Collection(nameof(TestFixture))]
public class TaskRepositoryTests
{
    private readonly ITaskRepository _repository;

    public TaskRepositoryTests(TestFixture fixture)
    {
        _repository = fixture.TaskRepository;
    }

    [Fact]
    public async Task Add_Task_Success()
    {
        // Arrange
        const int count = 5;

        var tasks = TaskEntityV1Faker.Generate(count);

        // Act
        var results = await _repository.Add(tasks, default);

        // Asserts
        results.Should().HaveCount(count);
        results.Should().OnlyContain(x => x > 0);
    }

    [Fact]
    public async Task Get_SingleTask_Success()
    {
        // Arrange
        var tasks = TaskEntityV1Faker.Generate();
        var taskIds = await _repository.Add(tasks, default);
        var expectedTaskId = taskIds.First();
        var expectedTask = tasks.First()
            .WithId(expectedTaskId);

        // Act
        var results = await _repository.Get(new TaskGetModel()
        {
            TaskIds = new[] { expectedTaskId }
        }, default);

        // Asserts
        results.Should().HaveCount(1);
        var task = results.Single();

        task.Should().BeEquivalentTo(expectedTask);
    }

    [Fact]
    public async Task AssignTask_Success()
    {
        // Arrange
        var assigneeUserId = Create.RandomId();

        var tasks = TaskEntityV1Faker.Generate();
        var taskIds = await _repository.Add(tasks, default);
        var expectedTaskId = taskIds.First();
        var expectedTask = tasks.First()
            .WithId(expectedTaskId)
            .WithAssignedToUserId(assigneeUserId);
        var assign = AssignTaskModelFaker.Generate()
            .First()
            .WithTaskId(expectedTaskId)
            .WithAssignToUserId(assigneeUserId);

        // Act
        await _repository.Assign(assign, default);

        // Asserts
        var results = await _repository.Get(new TaskGetModel()
        {
            TaskIds = new[] { expectedTaskId }
        }, default);

        results.Should().HaveCount(1);
        var task = results.Single();

        expectedTask = expectedTask with {Status = assign.Status};
        task.Should().BeEquivalentTo(expectedTask);
    }

    [Fact]
    public async Task GetSubTasksInStatus_Success()
    {
        // Arrange
        var expectedTitle = "Expected title";
        var parentsTasks = TaskEntityV1Faker.Generate();
        var expectedParentId = parentsTasks.First().Id;

        var subTasks = TaskEntityV1Faker.Generate(10)
            .Select(p => p.WithParentTaskId(expectedParentId).WithTitle(expectedTitle))
            .ToArray();
        var parentsTaskIds = await _repository.Add(parentsTasks, default);
        var subTaskIds = await _repository.Add(subTasks, default);
        var expectedTaskId = subTaskIds[0];

        var expectedTaskStatuses = new TaskStatus [] { (TaskStatus)subTasks[0].Status };

        SubTaskModel expectedFirstSubTask = new SubTaskModel {
            TaskId = expectedTaskId,
            Title = expectedTitle,
            Status = expectedTaskStatuses[0],
            ParentTaskIds = new long[] {expectedParentId, expectedTaskId},
        };

        // Act
        var results = await _repository.GetSubTasksInStatus(expectedParentId, expectedTaskStatuses, default);

        // Asserts
        results.Should().NotBeEmpty();
        results.Should().OnlyContain(st => expectedTaskStatuses.Contains(st.Status));

        var subTask = results.Where(st => st.TaskId == expectedTaskId).FirstOrDefault();
        subTask.Should().NotBeNull();
        subTask.Should().BeEquivalentTo(expectedFirstSubTask);
    }
}
