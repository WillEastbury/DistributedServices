namespace AppFrame.Paging;

public class PageChange
{
    public PageChangeState pageChangeState {get;set;}
    public int pageNumber  {get;set;} = -1;
    public byte[] oldPage {get;set;} = null;
    public byte[] newPage  {get;set;} = null;

    public PageChange(int pageNumber, byte[] oldPage, byte[] newPage, PageChangeState pageChangeState)
    {
        this.pageNumber = pageNumber;
        this.oldPage = oldPage;
        this.newPage = newPage;
        this.pageChangeState = pageChangeState;
    }
}
