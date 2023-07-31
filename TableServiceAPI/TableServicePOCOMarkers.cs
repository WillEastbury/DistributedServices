namespace TableServiceAPI
{
    public class TableServiceKeyAttribute : Attribute
    {

    }
    
    public class TableServiceAttribute : Attribute
    {

    }
    [TableService()]
    public class BaseWithID
    {
        [TableServiceKey()]
        public string ID {get;set;}
        public string User1 {get;set;}
        public string User2 {get;set;}
        public string User3 {get;set;}
        public string User4 {get;set;}
    }
}