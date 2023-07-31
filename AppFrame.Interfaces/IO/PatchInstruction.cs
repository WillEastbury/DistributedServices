namespace AppFrame.Interfaces;

public class PatchInstruction
{
    public PatchInstruction()
    {
        
    }
    public PatchInstruction(int offset, byte[] data, byte[] oldData = null)
    {
        this.Offset = offset;
        this.Data = data;
        this.OldData = oldData;
    }
    public byte[] Data {get;set;} = null;
    public int Offset {get;set;} = 0;
    public byte[] OldData {get;set;}  = null;
}

