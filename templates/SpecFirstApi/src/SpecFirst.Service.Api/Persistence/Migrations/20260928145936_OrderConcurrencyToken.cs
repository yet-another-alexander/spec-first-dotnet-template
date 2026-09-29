using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpecFirst.Service.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OrderConcurrencyToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Orders gained xmin as a concurrency token. xmin is a PostgreSQL system column that every table already has,
            // so there is nothing to create; this migration only records the model change in the snapshot.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Nothing to drop; see Up.
        }
    }
}
