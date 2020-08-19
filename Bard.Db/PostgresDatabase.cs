using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Docker.DotNet.Models;

namespace Bard.Db
{
    public class PostgresDatabase : BardDbBase
    {
        private readonly string _password;
        private readonly string _postgresUser;


        public PostgresDatabase(string databaseName, string portNumber = "5432", string imageName = "postgres",
            string tagName = "postgres", string postgresUser = "user1", string password = "Password1") : base(
            databaseName, portNumber, imageName, tagName)
        {
            _postgresUser = postgresUser;
            _password = password;
        }

        protected override async Task<string> CreateContainerIfRequired()
        {
            var testDb = await RetrieveContainer();

            if (testDb != null) return testDb.ID;

            Console.WriteLine($"Creating Container {ImageName}:{TagName}");

            var createParameters = new CreateContainerParameters(new Config
            {
                Image = $"{ImageName}:{TagName}",
                Cmd = new List<string> {"postgres"},
                Env = new List<string>
                {
                    $"POSTGRES_DB={DatabaseName}", $"POSTGRES_USER={_postgresUser}", $"POSTGRES_PASSWORD={_password}"
                },
                ExposedPorts = new Dictionary<string, EmptyStruct> {{$"{PortNumber}/tcp", new EmptyStruct()}},
                Healthcheck = new HealthConfig
                {
                    Retries = 1,
                    Test = new List<string> {"CMD-SHELL", "pg_isready -U postgres"}
                }
            })
            {
                HostConfig = new HostConfig
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>>
                    {
                        {
                            "5432/tcp", new List<PortBinding>
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