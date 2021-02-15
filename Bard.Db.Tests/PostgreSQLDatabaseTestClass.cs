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
                databaseName: "PostgreSQL_StartStop_latest",
                postgresUser: "Db_user",
                password: "Password1");

            var result = db.StartDatabase();

            _output.WriteLine(result);

            db.StopDatabase();
        }

        [Fact]
        public void Start_And_Delete_NonOfficialPostgreSQL_latest()
        {
            var db = new PostgresDatabase(
                databaseName: "Bitnami_PostgreSQL_latest",
                postgresUser: "Db_user",
                password: "Password1",
                imageName: "bitnami/postgresql");

            var result = db.StartDatabase();

            _output.WriteLine(result);

            db.DeleteDatabase();
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
        
        [Fact]
        public void Start_And_delete_PostgreSQL_latest()
        {
            var db = new PostgresDatabase(
                databaseName: "PostgreSQL_Delete_latest",
                postgresUser: "Db_user",
                password: "Password1");

            var result = db.StartDatabase();

            _output.WriteLine(result);

            db.DeleteDatabase();
        }
    }
}

