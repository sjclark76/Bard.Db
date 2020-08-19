using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Newtonsoft.Json;

namespace Bard.Db
{
    public abstract class BardDbBase
    {
        private protected readonly string DatabaseName;
        private protected readonly string PortNumber;
        private protected readonly string ImageName;
        private protected readonly string TagName;
        private protected readonly DockerClient DockerClient;
        private string _dbContainerId;

        protected BardDbBase(string databaseName, string portNumber, string imageName, string tagName)
        {
            DatabaseName = databaseName;
            PortNumber = portNumber;
            ImageName = imageName;
            TagName = tagName;
            DockerClient = new DockerClientConfiguration().CreateClient();
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
            Console.WriteLine($"Starting Container {ImageName}:{TagName} - {_dbContainerId}");

            return DockerClient.Containers.StopContainerAsync(_dbContainerId, new ContainerStopParameters());
        }
        
        public async Task<string> StartDatabaseAsync()
        {
            await PullImageIfRequired();

            _dbContainerId = await CreateContainerIfRequired();

            return await StartContainer(_dbContainerId);
        }

        protected abstract Task<string> CreateContainerIfRequired();

        private async Task PullImageIfRequired()
        {
            var dataBaseImages = await DockerClient.Images.ListImagesAsync(new ImagesListParameters
            {
                MatchName = $"{ImageName}:{TagName}"
            });

            if (dataBaseImages.Any() == false)
            {
                Console.WriteLine($"Pulling Image {ImageName}:{TagName}");

                await DockerClient.Images.CreateImageAsync(new ImagesCreateParameters
                    {
                        FromImage = ImageName,
                        Tag = TagName
                    },
                    null, new Progress<JSONMessage>(message =>
                    {
                        Console.WriteLine(message.Status);
                    }));
            }
        }
        
        private protected async Task<ContainerListResponse?> RetrieveContainer()
        {
            var fullImage = $"{ImageName}:{TagName}";
            
            var containers = await DockerClient.Containers.ListContainersAsync(new ContainersListParameters
            {
                All = true,
                Filters = new Dictionary<string, IDictionary<string, bool>>
                {
                    {"name", new Dictionary<string, bool> {{DatabaseName, true}}}
                }
            });

            var testDb = containers.SingleOrDefault();

            if (testDb != null && testDb.Image != fullImage)
            {
                testDb = null;
            }
            
            return testDb;
        }
        
        private async Task<string> StartContainer(string dbContainerId)
        {
            Console.WriteLine($"Starting Container {ImageName}:{TagName} - {dbContainerId}");

            await DockerClient.Containers.StartContainerAsync(dbContainerId, new ContainerStartParameters());

            var container = await DockerClient.Containers.InspectContainerAsync(dbContainerId);
            
            while (container.State.Health.Status == "starting")
            {
                Console.WriteLine($"Container Status {container.State.Health.Status}");

                if (container.State.Health.Status == "unhealthy")
                    throw new Exception("There is a problem with the container it is unhealthy");

                await Task.Delay(2000);

                container = await DockerClient.Containers.InspectContainerAsync(dbContainerId);
            }
            
            Console.WriteLine($"Container Started {container.State.Health.Status}");
            Console.WriteLine($"Container Started {JsonConvert.SerializeObject(container, Formatting.Indented)}");

            var hostIpAddress = container.NetworkSettings.IPAddress;

            return hostIpAddress;
        }
    }
}