namespace AppFrame.Interfaces;
public class TransactionLogEntry
{
    public int TransactionId {get;set;} 
    public DateTime BeganAtUTC {get;set;} = DateTime.UtcNow;
    public IDictionary<string, TransactionalPageUpdate> PageUpdateStates { get; set; } = new Dictionary<string, TransactionalPageUpdate>();
    public CommitState CommitState { get; set; } = CommitState.NotCommitted;

    // public void WriteToByteArray(byte[] data)
    // {
    //     using MemoryStream memoryStream = new(data);
    //     WriteToStream(memoryStream);
    // }
    // public byte[] AsByteArray()
    // {
    //     using MemoryStream memoryStream = new();
    //     WriteToStream(memoryStream);
    //     return memoryStream.ToArray();
    // }

    // public void WriteToStream(Stream outstream)
    // {
    //     using BinaryWriter writer = new(outstream);
    //     // Write the TransactionId
    //     writer.Write(TransactionId);

    //     // Write the number of PageUpdateStates
    //     writer.Write(PageUpdateStates.Count);

    //     writer.Write((int)CommitState);

    //     // Iterate over each PageUpdateState and write its properties
    //     foreach (KeyValuePair<string,TransactionalPageUpdate> pus in PageUpdateStates)
    //     {
    //         TransactionalPageUpdate pageUpdateState = pus.Value;
    //         writer.Write(pageUpdateState.FileId);
    //         writer.Write(pageUpdateState.PageNumber);
    //         writer.Write(pageUpdateState.NewData?.Length ?? -1);
    //         if (pageUpdateState.NewData != null)
    //             writer.Write(pageUpdateState.NewData);
    //         writer.Write(pageUpdateState.OldData?.Length ?? -1);
    //         if (pageUpdateState.OldData != null)
    //             writer.Write(pageUpdateState.OldData);
    //         writer.Write(pageUpdateState.PageOldVersion);
    //         writer.Write(pageUpdateState.PageNewVersion);
    //         writer.Write((int)pageUpdateState.CommitState);
    //     }

    //     // Check the size of the serialized data
    //     if (outstream.Length > 65535)
    //     {
    //         throw new Exception("Transaction log entry exceeds maximum page size.");
    //     }
    //     writer.Flush();
    // }

    // public static TransactionLogEntry FromStream(Stream dataStream)
    // {
    //     using BinaryReader reader = new(dataStream);
        
    //     TransactionLogEntry transactionLogEntry = new()
    //     {
    //         TransactionId = reader.ReadInt32(),
    //     };

    //     // Read the number of PageUpdateStates
    //     int count = reader.ReadInt32();

    //     // Create a list to hold the PageUpdateStates
    //     transactionLogEntry.PageUpdateStates = new Dictionary<string, TransactionalPageUpdate>();

    //     // Iterate over each PageUpdateState and read its properties
    //     for (int i = 0; i < count; i++)
    //     {
    //         int fileId = reader.ReadInt32();
    //         int pageNumber = reader.ReadInt32();
    //         int newDataLength = reader.ReadInt32();
    //         byte[] newData = newDataLength >= 0 ? reader.ReadBytes(newDataLength) : null;
    //         int oldDataLength = reader.ReadInt32();
    //         byte[] oldData = oldDataLength >= 0 ? reader.ReadBytes(oldDataLength) : null;
    //         int pageOldVersion = reader.ReadInt32();
    //         int pageNewVersion = reader.ReadInt32();
    //         CommitState commitState = (CommitState)reader.ReadInt32();

    //         TransactionalPageUpdate pageUpdateState = new(
    //             fileId,
    //             pageNumber,
    //             newData,
    //             oldData,
    //             pageOldVersion,
    //             pageNewVersion,
    //             commitState);

    //         transactionLogEntry.PageUpdateStates.Add($"{pageUpdateState.FileId}-{pageUpdateState.PageNumber}",pageUpdateState);
    //     }

    //     // Return the TransactionLogEntry
    //     return transactionLogEntry;
    // }
    // public static TransactionLogEntry FromByteArray(byte[] data)
    // {
    //     using MemoryStream memoryStream = new(data);
    //     return FromStream(memoryStream);
    // }

}
