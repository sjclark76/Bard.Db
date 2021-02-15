using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Docker.DotNet.Models;

namespace Bard.Db
{
    /// <summary>
    ///     Bard.Db Postgres Database class used for basic management a docker instance of PostgreSQL
    /// <remarks>
    ///     Based off postgres docker image. Tag should be valid for this image see
    ///     website for full list.
    ///     https://hub.docker.com/_/postgres
    /// </remarks>  
    /// </summary>
    public class PostgresDatabase : BardDbBase
    {
        private readonly string _password;
        private readonly string _postgresUser;

        
        /// <summary>
        /// PostgresDatabase Constructor
        /// </summary>
        /// <param name="databaseName">The name of the PostgreSQL instance</param>
        /// <param name="postgresUser">The name of the PostgreSQL user</param>
        /// <param name="password">The PostgreSQL password</param>
        /// <param name="portNumber">The port number (Optional) default value 5432</param>
        /// <param name="tagName">The docker tag name (Optional) default value latest</param>
        public PostgresDatabase(string databaseName, 
            string postgresUser, 
            string password ,
            string portNumber = "5432",
            string tagName = "latest" 
            ) : base(
            databaseName, portNumber, "postgres", tagName)
        {
            _postgresUser = postgresUser;
            _password = password;
        }
        
        
        /// <summary>
        /// PostgresDatabase Constructor
        /// </summary>
        /// <param name="databaseName">The name of the PostgreSQL instance</param>
        /// <param name="postgresUser">The name of the PostgreSQL user</param>
        /// <param name="password">The PostgreSQL password</param>
        /// <param name="portNumber">The port number (Optional) default value 5432</param>
        /// <param name="imageName">The docker image name (Optional) default value postgres</param>
        /// <param name="tagName">The docker tag name (Optional) default value latest</param>
        public PostgresDatabase(string databaseName, 
            string postgresUser, 
            string password ,
            string imageName,
            string portNumber = "5432",
            string tagName = "latest" 
            ) : base(
            databaseName, portNumber, imageName, tagName)
        {
            _postgresUser = postgresUser;
            _password = password;
        }

        private protected override async Task<string> CreateContainerIfRequired()
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
