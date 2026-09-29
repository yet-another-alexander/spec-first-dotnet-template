namespace SpecFirst.Service.Tests.Spec.Integration;

/// <summary>
/// The unit of comparison is the statement, not the line: a column is only declared inside its own CREATE TABLE.
/// </summary>
public sealed class SchemaDumpTests
{
    private const string Contract = """
        --
        -- PostgreSQL database dump
        --
        SET statement_timeout = 0;
        \restrict abc
        CREATE TABLE public.orders (
            id uuid NOT NULL,
            status character varying(16) NOT NULL
        );
        ALTER TABLE ONLY public.orders
            ADD CONSTRAINT pk_orders PRIMARY KEY (id);

        """;

    [Fact]
    public void A_statement_is_one_unit_and_noise_is_dropped()
    {
        var statements = SchemaDump.Normalise(Contract);

        statements.ShouldBe([
            "CREATE TABLE public.orders (\n    id uuid NOT NULL,\n    status character varying(16) NOT NULL\n);",
            "ALTER TABLE ONLY public.orders\n    ADD CONSTRAINT pk_orders PRIMARY KEY (id);",
        ]);
    }

    [Fact]
    public void A_column_declared_under_another_table_is_still_drift()
    {
        var declared = SchemaDump.Normalise(Contract).ToHashSet(StringComparer.Ordinal);
        var actual = SchemaDump.Normalise(
            Contract
                + """
                CREATE TABLE public.order_lines (
                    id bigint NOT NULL,
                    status character varying(16) NOT NULL
                );

                """
        );

        var drift = actual.Where(statement => !declared.Contains(statement)).ToList();

        drift.Count.ShouldBe(1);
        drift[0].ShouldStartWith("CREATE TABLE public.order_lines");
    }
}
