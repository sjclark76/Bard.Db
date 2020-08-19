using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Newtonsoft.Json;

namespace Bard.Db
{
    public class MsSqlDatabase
    {
        private readonly string _databaseName;
        private readonly string _portNumber;
        private readonly string _image;
        private readonly string _tagName;
        private readonly string _saPassword;
      
        private readonly DockerClient _dockerClient;
        private string _dbContainerId = string.Empty;

        public MsSqlDatabase(string databaseName, string portNumber = "1433", string image="mcr.microsoft.com/mssql/server", string tagName = "2019-latest", string saPassword = "Password1")
        {
            _databaseName = databaseName;
            _portNumber = portNumber;
            _image = image;
            _tagName = tagName;
            _saPassword = saPassword;
            _dockerClient = new DockerClientConfiguration().CreateClient();
        }

        public string StartDatabase()
        {
            var result = AsyncHelper.RunSync(StartDatabaseAsync);
        
            return result;
        }

        public bool StopDatabase()
        {
            return AsyncHelper.RunSync(StopDatabaseAsync);
        }
        
        public Task<bool> StopDatabaseAsync()
        {
            Console.WriteLine($"Starting Container {_image}:{_tagName} - {_dbContainerId}");

            return _dockerClient.Containers.StopContainerAsync(_dbContainerId, new ContainerStopParameters());
        }
        
        public async Task<string> StartDatabaseAsync()
        {
            await PullImageIfRequired();

            _dbContainerId = await CreateContainerIfRequired();

           return await StartContainer(_dbContainerId);
        }

        private async Task PullImageIfRequired()
        {
            var sqlServerImages = await _dockerClient.Images.ListImagesAsync(new ImagesListParameters
            {
                MatchName = $"{_image}:{_tagName}"
            });

            if (sqlServerImages.Any() == false)
            {
                Console.WriteLine($"Pulling Image {_image}:{_tagName}");

                await _dockerClient.Images.CreateImageAsync(new ImagesCreateParameters
                    {
                        FromImage = _image,
                        Tag = _tagName
                    },
                    null, new Progress<JSONMessage>(message =>
                    {
                        Console.WriteLine(message.Status);
                    }));
            }
        }

        private async Task<string> StartContainer(string dbContainerId)
        {
            Console.WriteLine($"Starting Container {_image}:{_tagName} - {dbContainerId}");

            await _dockerClient.Containers.StartContainerAsync(dbContainerId, new ContainerStartParameters());

            var container = await _dockerClient.Containers.InspectContainerAsync(dbContainerId);
            
            while (container.State.Health.Status == "starting")
            {
                Console.WriteLine($"Container Status {container.State.Health.Status}");

                if (container.State.Health.Status == "unhealthy")
                    throw new Exception("There is a problem with the container it is unhealthy");

                await Task.Delay(2000);

                container = await _dockerClient.Containers.InspectContainerAsync(dbContainerId);
            }
            
            Console.WriteLine($"Container Started {container.State.Health.Status}");
            Console.WriteLine($"Container Started {JsonConvert.SerializeObject(container, Formatting.Indented)}");

            var hostIpAddress = container.NetworkSettings.IPAddress;

            return hostIpAddress;
        }

        private async Task<string> CreateContainerIfRequired()
        {
            var testDb = await RetrieveContainer(_databaseName, _image, _tagName);
           
            if (testDb != null) return testDb.ID;
            
            Console.WriteLine($"Creating Container {_image}:{_tagName}");

            var createParameters = new CreateContainerParameters(new Config
            {
                Image = $"{_image}:{_tagName}",
                Cmd = new List<string> {"/opt/mssql/bin/sqlservr"},
                Env = new List<string> {"ACCEPT_EULA=Y", $"SA_PASSWORD={_saPassword}"},

                Healthcheck = new HealthConfig
                {
                    Retries = 1,
                    Test = new List<string>
                    {
                        "CMD-SHELL", $"/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P '{_saPassword}' -Q 'select 1' || exit 1"
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
                                    HostPort = _portNumber
                                }
                            }
                        }
                    }
                },
                Name = _databaseName
            };
            
            var response = await _dockerClient.Containers.CreateContainerAsync(createParameters);

            return response.ID;
        }

        private async Task<ContainerListResponse?> RetrieveContainer(string databaseName, string image, string tagName)
        {
            var fullImage = $"{image}:{tagName}";
            
            var containers = await _dockerClient.Containers.ListContainersAsync(new ContainersListParameters
            {
                All = true,
                Filters = new Dictionary<string, IDictionary<string, bool>>
                {
                    {"name", new Dictionary<string, bool> {{databaseName, true}}}
                }
            });

            var testDb = containers.SingleOrDefault();

            if (testDb != null && testDb.Image != fullImage)
            {
                testDb = null;
            }
            
            return testDb;
        }
    }
}