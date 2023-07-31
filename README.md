# DistributedServices
A sample of my thoughts on building distributed, reliable services.
Includes Tabular indexed distributed Storage, Synchronous Replication of storage, Transaction Management and log Replay, Clustering and other challenges. 
---------------------------------------------------------------------------------

The following modules come together under the covers to make this all work, each with appropriate Interfaces and DI setup modules
------------------------
- DiskPagedStorageIO
- FileMetaData
- LocalPagedStorage
- RemotePagedStorage
- InMemoryCache
- IndexedLoggedObjectStore
- LoggedKeyValueStore
- PagedStorageReplicaSet
- TransactionalPagedStorageManager
- TransactionalPageUpdate
- TransactionLog
- BinarySearchTree + BinarySearchTreeNode
- Table<T>
- PatchInstruction
- ConjunctionOperator
- QueryOperator
- SortDirection
- WhereExpressionNode
- PrincipalData
- OAuthToken
- TokenCheckMiddleware

Behind the scenes the following core fundamental services should exist
--------------------
- TableStorageService -> Code Complete
- ReplicaTargetService -> Code Complete
- QueueService -> 
- PubSubService -> 
- AuthorizationService -> 
- BlobStoreService 
- ClusterService
- EventLogService
- EventTimerService
- LocalisationService
- LockService

- RequestPartitionProxyRoutingService -> Will use YARP to route to remote replicas

The following business application services will also exist
------------------
- FormService
- ReportService
- TrackedWorkflowService
- ValidationService

The core premise here is to make these services extensible and interoperable and to provide a sensible sample of what these services might look like in an enterprise application. 

I will also provide deployment samples and config files, with dockerfiles and command line scripts, for each service for a turnkey scripted deployment.
