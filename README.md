# DistributedServices
A sample of my thoughts on building distributed, reliable services to support a business in it's critical operations
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
- TableOfT
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
- BusinessRuleService

- RequestPartitionProxyRoutingService -> Will use YARP to route to remote replicas

The following business application services will also exist
------------------
- FormService (Generates HTML Forms for list and interaction with business objects)
- ReportService (Generates Static HTML Reports)
- HierarchyService (Understands the semantics of your business hierarchies (For example "Get me all of the retail customers in the north east of europe"))
- TrackedWorkflowService (Allows for long-running disconnected tasks, or ones that require approvals or pipelines)
- ValidationService (Validates that your business objects are safe according to business rules). 

You will also find sample POCOs that are relevant to most businesses and scenarios (currently in the TableServiceAPI project) 
- Person -> Complete
- Partner -> Complete
- ProductMaterial -> Complete
- StockKeepingUnit -> Complete
- PricingCalculation -> Complete
- BillOfMaterial -> Complete
- BillOfMaterialComponent -> Complete
- UnitOfMeasure -> Complete
- PurchaseRequisition 
- PurchaseOrder
- PurchaseShipNotification
- PurchaseInvoice
- PurchaseRemittance
- SalesQuotation
- SalesOrder
- SalesDispatch
- SalesReceiptConfirmation
- SalesInvoice
- SalesInvoicePayment
- Ledger
- LedgerAccount
- LedgerCostCentre
- LedgerPayment

Supporting Enums
- StructureType (Pricing Condition Structures - Fixed, Percentage, PerSKUSold, ListPrice) -> Complete
- PartnerType (Customer, Supplier, Shipping, Billing, Etc.) -> Complete

The core premise here is to make these services extensible and interoperable and to provide a sensible sample of what these services might look like in a typical line of business application. 

The overall system architecture document, in order to create a globally resilient transactional storage system with synchronous commit.
```mermaid
graph TD
  AX1[BlobServiceAPI]
  AX2[QueueServiceAPI]
  AX3[PubSubServiceAPI]
  AX4[EventLogServiceAPI]
  AX5[EventTimerServiceAPI]
  AX6[TransactionServiceAPI]
  AX7[LockServiceAPI]
  AX8[ClusterServiceAPI]
  AX9[AuthServiceAPI]
  AX10[ValidationServiceAPI]
  AX11[FormServiceAPI]
  AX12[TrackedWorkflowServiceAPI]
  AX13[ReportServiceAPI]
  A[Partition Router API IRequestProxy]
  AS[TableServiceAPI]
  AA1[ReplicationServiceAPI]
  AA2[ReplicationServiceAPI]
  AA3[ReplicationServiceAPI]
  Q[List Of IRemotePagedStorage]
  P[ILocalPagedStorage]
  A --> AS
  AX1 --> A
  AX2 --> A
  AX3 --> A
  AX4 --> A
  AX5 --> A
  AX6 --> A
  AX7 --> A
  AX8 --> A
  AX9 --> A
  AX10 --> A
  AX11 --> A
  AX12 --> A
  AX13 --> A
  AS --> ASC[TableServiceContextManager]
  ASC --> I[ITable Of T]
  I --> J[IIndexedLoggedObjectStore]
  J --> K[ILoggedKeyValueStore]
  J --> L[Dictionary Of ISearchTree]
  K --> M[ITransactionalPagedStorageManager]
  M --> N[ITransactionLog]
  M --> O[IPagedStorageReplicaSet]
  N --> O
  O --> |Local Leader Replica - United Kingdom |P
  O --> |Remote Replicas - Geodes | Q 
  Q -->|Replica 1 - Ireland| AA1
  Q -->|Replica 2 - Western Australia| AA2
  Q -->|Replica 3 - Western USA| AA3
  P --> R[DiskBackedPageStorageIO]
  P --> S[InMemoryCache]
  AA1 --> BB1[ReplicationContextManager]
  BB1 --> CC1[LocalPagedStorage]
  CC1 --> DD1[DiskBackedPageStorageIO]
  CC1 --> T[InMemoryCache]
  AA2 --> BB2[ReplicationContextManager]
  BB2 --> CC2[LocalPagedStorage]
  CC2 --> DD2[DiskBackedPageStorageIO]
  CC2 --> U[InMemoryCache]
  AA3 --> BB3[ReplicationContextManager]
  BB3 --> CC3[LocalPagedStorage]
  CC3 --> DD3[DiskBackedPageStorageIO]
  CC3 --> V[InMemoryCache]

```
I will also provide deployment samples and config files, with dockerfiles and command line scripts, for each service for a turnkey scripted deployment.
