using FluentMigrator;

namespace HomeworkApp.Dal.Migrations;

[Migration(20241011183900, TransactionBehavior.None)]
public class AddTaskCommentV1Type : Migration
{
    public override void Up()
    {
        const string sql = @"
DO $$
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'task_comments_v1') THEN
            CREATE TYPE task_comments_v1 as
            (
                  id                  bigint
                , task_id             bigint
                , author_user_id      bigint
                , message             text
                , at                  timestamp with time zone
                , modified_at         timestamp with time zone
                , deleted_at          timestamp with time zone
            );
        END IF;
    END
$$;";

        Execute.Sql(sql);
    }

    public override void Down()
    {
        const string sql = @"
DO $$
    BEGIN
        DROP TYPE IF EXISTS users_v1;
    END
$$;";

        Execute.Sql(sql);
    }
}
