namespace TableServiceAPI;

[TableService()]
public class Person : BaseWithID
{
    public string PersonName {get;set;}
    public int PersonAge {get;set;}
}

[TableService()]
public class Partner : BaseWithID
{
    public PartnerType PartnerType {get;set;}
    public string PartnerTree1 {get;set;}
    public string PartnerTree2 {get;set;}
    public string PartnerTree3 {get;set;}
    public string PartnerTree4 {get;set;}
    public string PartnerTree5 {get;set;}
    public string PartnerTree6 {get;set;}
    public string PartnerTree7 {get;set;}
    public string PartnerTree8 {get;set;}
    public string PartnerName {get;set;}
    public string Address1 {get;set;}
    public string Address2 {get;set;}
    public string Address3 {get;set;}
    public string Address4 {get;set;}
    public string Country {get;set;}
    public string Postcode {get;set;}
}

[TableService()]
public class ProductMaterial : BaseWithID
{
    public string ProductName {get;set;}
    public string BaseSKUID {get;set;}
    public string AttachedBOMID {get;set;}
    public bool IsStocked {get;set;}
    public bool IsFinishedProductSKU {get;set;}
    public bool IsService {get;set;}
    public bool IsSellableSKU {get;set;}
    public bool IsPurchasable {get;set;}
    public int LeadTime {get;set;}
    public decimal StandardCost {get;set;}
}

[TableService()]
public class StockKeepingUnit: BaseWithID
{
    public string ProductID {get;set;} 
    public string Name {get;set;}
    public int PiecesInSKU {get;set;}
    public string UOMID {get;set;}
}

[TableService()]
public class PricingCalculation : BaseWithID
{
    public string StockKeepingUnitID {get;set;} 
    public string PartnerTreeID {get;set;}
    public decimal Amount {get;set;}
    public StructureType StructureType {get;set;} 
    public DateTime ValidFrom {get;set;}
    public DateTime ValidTo {get;set;}
}

[TableService()]
public class BillOfMaterial : BaseWithID
{
    public string Name {get;set;} 
    public string OutputSKUID {get;set;}
    public string OutputSKUQty {get;set;}
    public List<BillOfMaterialComponent> BillOfMaterialComponents {get;set;}
}
public class BillOfMaterialComponent :BaseWithID
{
    public string InputSKUID {get;set;}
    public decimal InputAmount {get;set;}
}

[TableService()]
public class UnitOfMeasure : BaseWithID
{
    public string UOMName {get;set;}
}

public enum PartnerType
{
    CustomerShip,
    CustomerBill,
    CustomerOther,
    SupplierShip,
    SupplierBill,
    SupplierOther
}
public enum StructureType 
{
    FixedListPrice, 
    PercentageDiscount, 
    FixedAmountDiscount, 
    PerSKUItemDiscount
}
