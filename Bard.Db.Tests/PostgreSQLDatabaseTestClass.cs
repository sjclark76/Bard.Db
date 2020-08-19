using Xunit;
using Xunit.Abstractions;

namespace Bard.Db.Tests
{
    public class PostgreSqlDatabaseTestClass
    {
        private readonly ITestOutputHelper _output;

        public PostgreSqlDatabaseTestClass(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Start_And_Stop_PostgreSQL_latest()
        {
            var db = new PostgresDatabase(
                "PostgreSQL_latest",
                "Db_user",
                "Password1");

            var result = db.StartDatabase();

            _output.WriteLine(result);

            db.StopDatabase();
        }

        [Fact]
        public void Start_And_Stop_PostgreSQL_alpine()
        {
            var db = new PostgresDatabase(
                "PostgreSQL_alpine",
                "Db_user",
                "Password1",
                tagName: "alpine");

            var result = db.StartDatabase();

            _output.WriteLine(result);

            db.StopDatabase();
        }
    }
}

