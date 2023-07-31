using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AppFrame.Interfaces;

namespace AppFrame.Implementations
{
    public class PagedStorageReplicaSet : IPagedStorageReplicaSet
    {
        public ILocalPagedStorage LocalLeader { get; }
        public IReadOnlyList<IRemotePagedStorage> RemoteFollowers { get; }

        public PagedStorageReplicaSet(ILocalPagedStorage localLeader, IReadOnlyList<IRemotePagedStorage> remoteFollowers)
        {
            LocalLeader = localLeader;
            RemoteFollowers = remoteFollowers;
        }

        public async Task<byte[]> GetPageAsync(string fileId, int pageNumber)
        {
            // Load from the local replica
            return await LocalLeader.GetPageAsync(fileId, pageNumber);
        }

        public async Task<Stream> GetPageStreamAsync(string fileId, int pageNumber)
        {
            // Load from the local replica
            return await LocalLeader.GetPageStreamAsync(fileId, pageNumber);
        }

        public async Task SetPageStreamAsync(string fileId, int pageNumber, Stream data)
        {
            // When writing to the local replica, we also write to all remote replicas
            // If any of the remote replicas fail, then we retry the write to that replica only until we establish it is dead (on multiple timeouts) or it successfully writes
            // Local write replica failure is catastrophic, so we don't handle that case as we should simply crash and recover the log
            
            List<Task> writeTasks = new();
            Dictionary<Task, IRemotePagedStorage> failedReplicas = new();
            Random random = new();

            // Write to the local replica
            Task localWriteTask = LocalLeader.SetPageStreamAsync(fileId, pageNumber, data);
            writeTasks.Add(localWriteTask);

            // Write to the remote replicas
            foreach (var remoteFollower in RemoteFollowers)
            {
                Task remoteWriteTask = remoteFollower.SetRemotePageStreamAsync(fileId, pageNumber, data);
                writeTasks.Add(remoteWriteTask);
                failedReplicas.Add(remoteWriteTask, remoteFollower);
            }

            // Check the status of the completed tasks
            while (writeTasks.Count > 0)
            {
                var completedTask = await Task.WhenAny(writeTasks);

                if (completedTask.IsFaulted)
                {
                    IRemotePagedStorage failedReplica = failedReplicas[completedTask];

                    // Retry the failed replica with randomized exponential backoff delay
                    try
                    {
                        await RetryFailedReplicaAsync(failedReplica, fileId, pageNumber, data, writeTasks, failedReplicas, random);
                    }
                    catch (Exception iex)
                    {
                        // If failed multiple times, handle the failure according to your requirements
                        throw new Exception("One or more replicas failed to write", iex);
                    }
                }

                writeTasks.Remove(completedTask);
                failedReplicas.Remove(completedTask);

                // Optionally, you can check for a maximum number of retry attempts and handle it accordingly
            }
        }

        private static async Task RetryFailedReplicaAsync(IRemotePagedStorage failedReplica, string fileId, int pageNumber, Stream data, List<Task> writeTasks, Dictionary<Task, IRemotePagedStorage> failedReplicas, Random random)
        {
            int retryAttempts = 0;
            const int maxRetryAttempts = 3;
            int delayMilliseconds = 0;

            while (retryAttempts < maxRetryAttempts)
            {
                retryAttempts++;

                try
                {
                    await Task.Delay(delayMilliseconds); // Delay before retrying

                    Task retryTask = failedReplica.SetRemotePageStreamAsync(fileId, pageNumber, data);
                    writeTasks.Add(retryTask);
                    failedReplicas[retryTask] = failedReplica;

                    return; // Retry task added successfully
                }
                catch
                {
                    // Retry failed, calculate next delay using randomized exponential backoff
                    _ = CalculateDelayMilliseconds(retryAttempts, random);
                }

                // If failed multiple times, throw an exception
                throw new Exception($"Failed to write to the replica after {maxRetryAttempts} attempts");
            }
        }
        private static int CalculateDelayMilliseconds(int retryAttempts, Random random)
        {
            int baseDelay = 1000; // Initial base delay in milliseconds
            int maxDelay = 5000; // Maximum delay in milliseconds

            int delay = (int)Math.Pow(2, retryAttempts) * baseDelay;
            delay = random.Next(delay / 2, delay); // Randomize the delay within a range
            delay = Math.Min(delay, maxDelay);

            return delay;
        }

        public async Task SetPageAsync(string fileId, int pageNumber, byte[] data)
        {
            // When writing to the local replica, we also write to all remote replicas
            // If any of the remote replicas fail, then we retry the write to that replica only until we establish it is dead (on multiple timeouts) or it successfully writes
            // Local write replica failure is catastrophic, so we don't handle that case

            List<Task> writeTasks = new();
            Dictionary<Task, IRemotePagedStorage> failedReplicas = new();
            Random random = new();

            // Write to the local replica
            Task localWriteTask = LocalLeader.SetPageAsync(fileId, pageNumber, data);
            writeTasks.Add(localWriteTask);

            // Write to the remote replicas
            foreach (var remoteFollower in RemoteFollowers)
            {
                Task remoteWriteTask = remoteFollower.SetRemotePageAsync(fileId, pageNumber, data);
                writeTasks.Add(remoteWriteTask);
                failedReplicas.Add(remoteWriteTask, remoteFollower);
            }

            // Check the status of the completed tasks
            while (writeTasks.Count > 0)
            {
                var completedTask = await Task.WhenAny(writeTasks);

                if (completedTask.IsFaulted)
                {
                    IRemotePagedStorage failedReplica = failedReplicas[completedTask];

                    // Retry the failed replica with randomized exponential backoff delay
                    try
                    {
                        await RetryFailedReplicaAsync(failedReplica, fileId, pageNumber, data, writeTasks, failedReplicas, random);
                    }
                    catch (Exception iex)
                    {
                        // If failed multiple times, handle the failure according to your requirements
                        throw new Exception("One or more replicas failed to write", iex);
                    }
                }

                writeTasks.Remove(completedTask);
                failedReplicas.Remove(completedTask);

                // Optionally, you can check for a maximum number of retry attempts and handle it accordingly
            }
        }
        private static async Task RetryFailedReplicaAsync(IRemotePagedStorage failedReplica, string fileId, int pageNumber, byte[] data, List<Task> writeTasks, Dictionary<Task, IRemotePagedStorage> failedReplicas, Random random)
        {
            int retryAttempts = 0;
            const int maxRetryAttempts = 3;
            int delayMilliseconds = 0;

            while (retryAttempts < maxRetryAttempts)
            {
                retryAttempts++;

                try
                {
                    await Task.Delay(delayMilliseconds); // Delay before retrying

                    Task retryTask = failedReplica.SetRemotePageAsync(fileId, pageNumber, data);
                    writeTasks.Add(retryTask);
                    failedReplicas[retryTask] = failedReplica;

                    return; // Retry task added successfully
                }
                catch
                {
                    // Retry failed, calculate next delay using randomized exponential backoff
                    delayMilliseconds = CalculateDelayMilliseconds(retryAttempts, random);
                }
            }

            // If failed multiple times, throw an exception
            throw new Exception($"Failed to write to the replica after {maxRetryAttempts} attempts");
        }
    }
}