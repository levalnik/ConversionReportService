using FluentMigrator;

namespace ConversionReportService.Infrastructure.DataAccess.Database.Migrations;

[Migration(0001)]
public class CreateInitialTables : Migration {
    public override void Up()
    {
        Create.Table("report_requests")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("product_id").AsInt64().NotNullable()
            .WithColumn("checkout_id").AsInt64().NotNullable()
            .WithColumn("period_start").AsDateTime().NotNullable()
            .WithColumn("period_end").AsDateTime().NotNullable()
            .WithColumn("status").AsString(32).NotNullable()
            .WithColumn("created_at").AsDateTime().NotNullable();

        Create.Index("idx_report_requests_status")
            .OnTable("report_requests")
            .OnColumn("status");

        Create.Index("idx_report_requests_product_checkout")
            .OnTable("report_requests")
            .OnColumn("product_id").Ascending()
            .OnColumn("checkout_id").Ascending();

        Create.Index("idx_report_requests_created")
            .OnTable("report_requests")
            .OnColumn("created_at");



        Create.Table("report_results")
            .WithColumn("request_id").AsInt64().PrimaryKey()
            .WithColumn("conversion_ratio").AsDouble().NotNullable()
            .WithColumn("payments_count").AsInt32().NotNullable()
            .WithColumn("generated_at").AsDateTime().NotNullable();

        Create.ForeignKey("fk_report_results_request")
            .FromTable("report_results").ForeignColumn("request_id")
            .ToTable("report_requests").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.Cascade);
    }

    public override void Down()
    {
        Delete.ForeignKey("fk_report_results_request").OnTable("report_results");

        Delete.Table("report_results");

        Delete.Index("idx_report_requests_status").OnTable("report_requests");
        Delete.Index("idx_report_requests_product_checkout").OnTable("report_requests");
        Delete.Index("idx_report_requests_created").OnTable("report_requests");

        Delete.Table("report_requests");
    }
}
