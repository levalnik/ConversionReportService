using FluentMigrator;

namespace ConversionReportService.Infrastructure.DataAccess.Database.Migrations;

[Migration(0002)]
public class AddExternalRequestIdToReportRequests : Migration
{
    public override void Up()
    {
        Alter.Table("report_requests")
            .AddColumn("external_request_id").AsInt64().Nullable();

        Execute.Sql("""
            UPDATE report_requests
            SET external_request_id = id
            WHERE external_request_id IS NULL
        """);

        Alter.Column("external_request_id")
            .OnTable("report_requests")
            .AsInt64()
            .NotNullable();

        Create.Index("report_requests_external_request_id")
            .OnTable("report_requests")
            .OnColumn("external_request_id")
            .Unique();
    }

    public override void Down()
    {
        Delete.Index("report_requests_external_request_id")
            .OnTable("report_requests");

        Delete.Column("external_request_id")
            .FromTable("report_requests");
    }
}
