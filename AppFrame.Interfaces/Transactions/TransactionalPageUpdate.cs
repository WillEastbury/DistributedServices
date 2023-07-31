namespace AppFrame.Interfaces;

public class TransactionalPageUpdate
{
    public TransactionalPageUpdate(
        string FileId, int PageNumber, byte[] NewData, byte[] OldData = null, int PageOldVersion = -1, 
        int PageNewVersion = -1, CommitState CommitState = CommitState.NotCommitted
    )
    {
        this.FileId = FileId;
        this.PageNumber = PageNumber;
        this.NewData = NewData;
        this.OldData = OldData;
        this.PageOldVersion = PageOldVersion;
        this.PageNewVersion = PageNewVersion;
        this.CommitState = CommitState;
    }  
    public TransactionalPageUpdate()
    {

    }
        
    public string FileId {set;get;}

    public int PageNumber{set;get;}
    public byte[] NewData {set;get;} = null;
    public byte[] OldData {set;get;} = null;
    public int PageOldVersion {set;get;} = -1;
    public int PageNewVersion {set;get;} = -1;
    public CommitState CommitState {set;get;} = CommitState.NotCommitted;

}


