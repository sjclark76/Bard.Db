using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Docker.DotNet.Models;

namespace Bard.Db
{
    /// <summary>
    ///     Bard.Db MsSqlDatabase class used for basic management a docker instance of Microsoft SQL Server
    /// <remarks>
    ///     Based off mcr.microsoft.com/mssql/server docker image. Tag should be valid for this image see
    ///     website for full list.
    ///     https://hub.docker.com/_/microsoft-mssql-server
    /// </remarks>
    /// </summary>
    public class MsSqlDatabase : BardDbBase
    {
        private readonly string _saPassword;

        /// <summary>
        ///     MsSqlDatabase Constructor
        /// </summary>
        /// <param name="databaseName">The name of the MSSQl database instance</param>
        /// <param name="saPassword">The SA password to be used to connect to the db</param>
        /// <param name="portNumber">The port number (Optional) default value 1433</param>
        /// <param name="tagName">The docker tag name (Optional) default value 2019-latest</param>
        public MsSqlDatabase(string databaseName, string saPassword, string portNumber = "1433",
            string tagName = "2019-latest"
        ) : base(databaseName, portNumber, "mcr.microsoft.com/mssql/server", tagName)
        {
            _saPassword = saPassword;
        }

        private protected override async Task<string> CreateContainerIfRequired()
        {
            var testDb = await RetrieveContainer();

            if (testDb != null) return testDb.ID;

            Console.WriteLine($"Creating Container {ImageName}:{TagName}");

            var createParameters = new CreateContainerParameters(new Config
            {
                Image = $"{ImageName}:{TagName}",
                Cmd = new List<string> {"/opt/mssql/bin/sqlservr"},
                Env = new List<string> {"ACCEPT_EULA=Y", $"SA_PASSWORD={_saPassword}"},

                Healthcheck = new HealthConfig
                {
                    Retries = 1,
                    Test = new List<string>
                    {
                        "CMD-SHELL",
                        $"/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P '{_saPassword}' -Q 'select 1' || exit 1"
                    }
                }
            })
            {
                HostConfig = new HostConfig
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>>
                    {
                        {
                            "1433/tcp", new List<PortBinding>
                            {
                                new PortBinding
                                {
                                    HostPort = PortNumber
                                }
                            }
                        }
                    }
                },
                Name = DatabaseName
            };

            var response = await DockerClient.Containers.CreateContainerAsync(createParameters);

            return response.ID;
        }
    }
}