using System.Text;

namespace AppFrame.Implementations;
public class FileMetadata
{
    public string FileId {get;set;}
    public int filePageCount {get;set;}
    public int startOffsetInPages {get;set;}
    public int fileOverallLength {get;set;}

    public static FileMetadata Deserialize(byte[] content)
    {
        // Implement deserialization logic direct from the byte array
        var temp = new FileMetadata();
        temp.FileId = Encoding.UTF8.GetString(content, 0, 256);
        temp.filePageCount = BitConverter.ToInt32(content, 256);
        temp.startOffsetInPages = BitConverter.ToInt32(content, 260);
        temp.fileOverallLength = BitConverter.ToInt32(content, 264);
        return temp;
    }
    public static byte[] Serialize(FileMetadata fileMetadata)
    {
        // Implement serialization logic direct to byte[]
        byte[] content = new byte[268];
        Encoding.UTF8.GetBytes(fileMetadata.FileId, 0, fileMetadata.FileId.Length, content, 0);
        BitConverter.GetBytes(fileMetadata.filePageCount).CopyTo(content, 256);
        BitConverter.GetBytes(fileMetadata.startOffsetInPages).CopyTo(content, 260);
        BitConverter.GetBytes(fileMetadata.fileOverallLength).CopyTo(content, 264);
        return content;
    }
}