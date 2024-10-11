using FluentMigrator;

namespace HomeworkApp.Dal.Migrations;

[Migration(20241011181800, TransactionBehavior.None)]
public class AddModifiedAtAndDeletedAt_InTaskComments : Migration
{
    public override void Up()
    {
        const string sql = @"
ALTER TABLE  task_comments
  ADD COLUMN modified_at timestamp with time zone NULL,
  ADD COLUMN deleted_at timestamp with time zone NULL;
        ";

        Execute.Sql(sql);
    }

    public override void Down()
    {
        const string sql = @"
ALTER TABLE  task_comments
 DROP COLUMN modified_at,
 DROP COLUMN deleted_at;
        ";
        Execute.Sql(sql);
    }
}
