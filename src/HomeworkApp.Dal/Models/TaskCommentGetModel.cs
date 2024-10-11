namespace HomeworkApp.Dal.Models;

public record TaskCommentGetModel
{
    public required long TaskId { get; init; }
    public required bool IncludeDeleted { get; init; }
}
