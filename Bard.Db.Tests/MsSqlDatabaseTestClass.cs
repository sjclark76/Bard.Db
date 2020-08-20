using Xunit;
using Xunit.Abstractions;

namespace Bard.Db.Tests
{
    
    public class MsSqlDatabaseTestClass
    {
        private readonly ITestOutputHelper _output;

        public MsSqlDatabaseTestClass(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Start_And_Stop_SQL_server_2017_latest()
        {
            var db = new MsSqlDatabase(
                databaseName: "BardDB_SQL_2017",
                saPassword: "Password1",
                portNumber: "1066",
                tagName: "2017-latest");

            var result = db.StartDatabase();

            _output.WriteLine(result);

            db.StopDatabase();
        }

        [Fact]
        public void Start_And_Stop_SQL_server_2019_latest()
        {
            var db = new MsSqlDatabase("BardDB_SQL_2019", 
                "Password1", 
                "1066");

            var result = db.StartDatabase();

            _output.WriteLine(result);

            db.StopDatabase();
        }

        [Fact]
        public void Start_And_Stop_SQL_server_2019_CU6_ubuntu_16_04()
        {
            var db = new MsSqlDatabase("BardDB_SQL_2019_CU6_ubuntu_16.04", 
                "Password1", 
                "1066",
                "2019-CU6-ubuntu-16.04");

            var result = db.StartDatabase();

            _output.WriteLine(result);

            db.StopDatabase();
        }
    }
}