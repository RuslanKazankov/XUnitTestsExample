using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeworkApp.Bll.Models;
public record TaskMessage
{
    public required long TaskId { get; init; }
    public required string Comment { get; init; }
    public required bool IsDeleted { get; init; }
    public required DateTimeOffset At { get; init; }
}
