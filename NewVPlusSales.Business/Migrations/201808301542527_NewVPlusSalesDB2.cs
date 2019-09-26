namespace NewVPlusSales.Business.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NewVPlusSalesDB2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "NewVPlusSales.Beneficiary",
                c => new
                    {
                        BeneficiaryId = c.Int(nullable: false, identity: true),
                        BeneficiaryAccountId = c.Int(nullable: false),
                        FullName = c.String(nullable: false, maxLength: 150, storeType: "varchar"),
                        BeneficiaryType = c.Int(nullable: false),
                        MobileNumber = c.String(nullable: false, maxLength: 11, storeType: "varchar"),
                        Email = c.String(nullable: false, maxLength: 1073741823, storeType: "varchar"),
                        Address = c.String(nullable: false, maxLength: 150, storeType: "varchar"),
                        TimeStampRegisered = c.String(nullable: false, maxLength: 35, storeType: "varchar"),
                        ApprovedBy = c.Int(nullable: false),
                        ApprovalComment = c.String(nullable: false, maxLength: 150, storeType: "varchar"),
                        TimeStampApproved = c.String(nullable: false, maxLength: 35, storeType: "varchar"),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.BeneficiaryId)
                .ForeignKey("NewVPlusSales.BeneficiaryAccount", t => t.BeneficiaryAccountId, cascadeDelete: true)
                .Index(t => t.BeneficiaryAccountId);
            
            CreateTable(
                "NewVPlusSales.BeneficiaryAccount",
                c => new
                    {
                        BeneficiaryAccountId = c.Int(nullable: false, identity: true),
                        AvailableBalance = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CreditLimit = c.Decimal(nullable: false, precision: 18, scale: 2),
                        LastTransactionAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        LastTransactionId = c.Long(nullable: false),
                        LastTransactionTimeStamp = c.String(nullable: false, maxLength: 35, storeType: "varchar"),
                        LastTransactionType = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.BeneficiaryAccountId);
            
            CreateTable(
                "NewVPlusSales.BeneficiaryAccountTransaction",
                c => new
                    {
                        BeneficiaryAccountTransactionId = c.Int(nullable: false, identity: true),
                        BeneficiaryAccountId = c.Int(nullable: false),
                        BeneficiaryId = c.Int(nullable: false),
                        TransactionType = c.Int(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PreviousBalance = c.Decimal(nullable: false, precision: 18, scale: 2),
                        NewBalance = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TransactionSource = c.Int(nullable: false),
                        TransactionSourceId = c.Int(nullable: false),
                        RegisteredBy = c.Int(nullable: false),
                        LastTransactionTimeStamp = c.String(nullable: false, maxLength: 35, storeType: "varchar"),
                        TimeStampRegistered = c.String(maxLength: 1073741823, fixedLength: true),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.BeneficiaryAccountTransactionId)
                .ForeignKey("NewVPlusSales.BeneficiaryAccount", t => t.BeneficiaryAccountId, cascadeDelete: true)
                .Index(t => t.BeneficiaryAccountId);
            
            CreateTable(
                "NewVPlusSales.CardRequisition",
                c => new
                    {
                        CardRequisitionId = c.Long(nullable: false, identity: true),
                        RequisitionTitle = c.String(nullable: false, maxLength: 150, storeType: "varchar"),
                        BeneficiaryId = c.Int(nullable: false),
                        TotalQuantityRequested = c.Int(nullable: false),
                        RequestedBy = c.Int(nullable: false),
                        TimeStampRequested = c.String(nullable: false, maxLength: 35, storeType: "varchar"),
                        ApprovedBy = c.Int(nullable: false),
                        ApproverComment = c.String(nullable: false, maxLength: 150, storeType: "varchar"),
                        TimeStampApproved = c.String(nullable: false, maxLength: 35, storeType: "varchar"),
                        QuantityApproved = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CardRequisitionId)
                .ForeignKey("NewVPlusSales.Beneficiary", t => t.BeneficiaryId, cascadeDelete: true)
                .Index(t => t.BeneficiaryId);
            
            CreateTable(
                "NewVPlusSales.CardRequisitionItem",
                c => new
                    {
                        CardRequisitionItemId = c.Long(nullable: false, identity: true),
                        CardRequisitionId = c.Long(nullable: false),
                        BeneficiaryId = c.Int(nullable: false),
                        CardTypeId = c.Int(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Quantity = c.Int(nullable: false),
                        CommissionRate = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CardCommissionId = c.Int(nullable: false),
                        CommissionAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CommissionQuantity = c.Int(nullable: false),
                        UnitPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        RequestedBy = c.Int(nullable: false),
                        TimeStampRequested = c.String(nullable: false, maxLength: 35, storeType: "varchar"),
                        ApprovedBy = c.Int(nullable: false),
                        ApproverComment = c.String(maxLength: 1073741823, fixedLength: true),
                        TimeStampApproved = c.String(nullable: false, maxLength: 35, storeType: "varchar"),
                        QuantityApproved = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CardRequisitionItemId)
                .ForeignKey("NewVPlusSales.Beneficiary", t => t.BeneficiaryId, cascadeDelete: true)
                .ForeignKey("NewVPlusSales.CardRequisition", t => t.CardRequisitionId, cascadeDelete: true)
                .Index(t => t.CardRequisitionId)
                .Index(t => t.BeneficiaryId);
            
            CreateTable(
                "NewVPlusSales.BeneficiaryPayment",
                c => new
                    {
                        BeneficiaryPaymentId = c.Int(nullable: false, identity: true),
                        BeneficiaryAccountTransactionId = c.Int(nullable: false),
                        BeneficiaryAccountId = c.Int(nullable: false),
                        BeneficiaryId = c.Int(nullable: false),
                        AmountPaid = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PaySource = c.Int(nullable: false),
                        PaymentSourceName = c.String(nullable: false, maxLength: 80, storeType: "varchar"),
                        PaymentReference = c.String(maxLength: 18, storeType: "varchar"),
                        PaymentDate = c.String(nullable: false, maxLength: 35, storeType: "varchar"),
                        RegisteredBy = c.Int(nullable: false),
                        TimeStampRegistered = c.String(nullable: false, maxLength: 35, storeType: "varchar"),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.BeneficiaryPaymentId)
                .ForeignKey("NewVPlusSales.BeneficiaryAccountTransaction", t => t.BeneficiaryAccountTransactionId, cascadeDelete: true)
                .Index(t => t.BeneficiaryAccountTransactionId);
            
            CreateTable(
                "NewVPlusSales.CardCommission",
                c => new
                    {
                        CardCommissionId = c.Int(nullable: false, identity: true),
                        CardTypeId = c.Int(nullable: false),
                        LowerAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        UpperAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CommissionRatee = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CardCommissionId)
                .ForeignKey("NewVPlusSales.CardType", t => t.CardTypeId, cascadeDelete: true)
                .Index(t => t.CardTypeId);
            
            CreateTable(
                "NewVPlusSales.CardType",
                c => new
                    {
                        CardTypeId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 150, storeType: "varchar"),
                        FaceValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CardTypeId);
            
            CreateTable(
                "NewVPlusSales.Card",
                c => new
                    {
                        CardId = c.Int(nullable: false, identity: true),
                        CardTitle = c.String(nullable: false, maxLength: 200, storeType: "varchar"),
                        CardTypeId = c.Int(nullable: false),
                        TotalQuantity = c.Int(nullable: false),
                        BatchKey = c.String(nullable: false, maxLength: 2, storeType: "varchar"),
                        StartBatchId = c.String(nullable: false, maxLength: 5, storeType: "varchar"),
                        StopBatchId = c.String(nullable: false, maxLength: 5, storeType: "varchar"),
                        NumberOfBatches = c.Int(nullable: false),
                        QuantityPerBatch = c.Int(nullable: false),
                        TimeStampRegisered = c.String(nullable: false, maxLength: 35, storeType: "varchar"),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CardId)
                .ForeignKey("NewVPlusSales.CardType", t => t.CardTypeId, cascadeDelete: true)
                .Index(t => t.CardTypeId);
            
            CreateTable(
                "NewVPlusSales.CardItem",
                c => new
                    {
                        CardItemId = c.Int(nullable: false, identity: true),
                        CardId = c.Int(nullable: false),
                        CardTypeId = c.Int(nullable: false),
                        DefectiveBatchNumber = c.String(nullable: false, maxLength: 500, storeType: "varchar"),
                        BatchId = c.String(nullable: false, maxLength: 2, storeType: "varchar"),
                        StartBatchNumber = c.String(nullable: false, maxLength: 5, storeType: "varchar"),
                        StopBatchNumber = c.String(nullable: false, maxLength: 5, storeType: "varchar"),
                        BatchQuantity = c.Int(nullable: false),
                        DefectiveQuantity = c.Int(nullable: false),
                        MissingQuantity = c.Int(nullable: false),
                        DeliveredQuantity = c.Int(nullable: false),
                        AvailableQuantity = c.Int(nullable: false),
                        IssuedQuantity = c.Int(nullable: false),
                        TimeStampRegisered = c.String(nullable: false, maxLength: 35, storeType: "varchar"),
                        TimeStampDelivered = c.String(nullable: false, maxLength: 35, storeType: "varchar"),
                        TimeStampLastIssued = c.String(nullable: false, maxLength: 35, storeType: "varchar"),
                        RegisteredBy = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CardItemId)
                .ForeignKey("NewVPlusSales.Card", t => t.CardId, cascadeDelete: true)
                .Index(t => t.CardId);
            
            CreateTable(
                "NewVPlusSales.CardDelivery",
                c => new
                    {
                        CardDeliveryId = c.Int(nullable: false, identity: true),
                        CardItemId = c.Int(nullable: false),
                        CardId = c.Int(nullable: false),
                        CardTypeId = c.Int(nullable: false),
                        DefectiveBatchNumber = c.String(nullable: false, maxLength: 500, storeType: "varchar"),
                        BatchId = c.String(nullable: false, maxLength: 2, storeType: "varchar"),
                        StartBatchNumber = c.String(nullable: false, maxLength: 5, storeType: "varchar"),
                        StopBatchNumber = c.String(nullable: false, maxLength: 5, storeType: "varchar"),
                        BatchQuantity = c.Int(nullable: false),
                        DefectiveQuantity = c.Int(nullable: false),
                        MissingQuantity = c.Int(nullable: false),
                        DeliveredQuantity = c.Int(nullable: false),
                        TimeStampRegisered = c.String(nullable: false, maxLength: 35, storeType: "varchar"),
                        ApproverComment = c.String(nullable: false, maxLength: 150, storeType: "varchar"),
                        TimeStampDelivered = c.String(nullable: false, maxLength: 35, storeType: "varchar"),
                        TimeStampApproved = c.String(nullable: false, maxLength: 35, storeType: "varchar"),
                        ReceivedBy = c.Int(nullable: false),
                        ApprovedBy = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CardDeliveryId)
                .ForeignKey("NewVPlusSales.CardItem", t => t.CardItemId, cascadeDelete: true)
                .Index(t => t.CardItemId);
            
            CreateTable(
                "NewVPlusSales.CardIssuance",
                c => new
                    {
                        CardIssuanceId = c.Int(nullable: false, identity: true),
                        CardRequisitionId = c.Long(nullable: false),
                        CardRequisitionItemId = c.Int(nullable: false),
                        BeneficiaryId = c.Int(nullable: false),
                        CardTypeId = c.Int(nullable: false),
                        CardItemId = c.Long(nullable: false),
                        BatchId = c.String(nullable: false, maxLength: 300, storeType: "varchar"),
                        StartBatchNumber = c.String(nullable: false, maxLength: 300, storeType: "varchar"),
                        StopBatchNumber = c.String(nullable: false, maxLength: 300, storeType: "varchar"),
                        QuantityIssued = c.Int(nullable: false),
                        IssuedBy = c.Int(nullable: false),
                        TimeStampIssued = c.String(nullable: false, maxLength: 35, storeType: "varchar"),
                        Status = c.Int(nullable: false),
                        CardItem_CardItemId = c.Int(),
                    })
                .PrimaryKey(t => t.CardIssuanceId)
                .ForeignKey("NewVPlusSales.CardItem", t => t.CardItem_CardItemId)
                .ForeignKey("NewVPlusSales.CardRequisition", t => t.CardRequisitionId, cascadeDelete: true)
                .Index(t => t.CardRequisitionId)
                .Index(t => t.CardItem_CardItemId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("NewVPlusSales.CardIssuance", "CardRequisitionId", "NewVPlusSales.CardRequisition");
            DropForeignKey("NewVPlusSales.CardIssuance", "CardItem_CardItemId", "NewVPlusSales.CardItem");
            DropForeignKey("NewVPlusSales.CardDelivery", "CardItemId", "NewVPlusSales.CardItem");
            DropForeignKey("NewVPlusSales.Card", "CardTypeId", "NewVPlusSales.CardType");
            DropForeignKey("NewVPlusSales.CardItem", "CardId", "NewVPlusSales.Card");
            DropForeignKey("NewVPlusSales.CardCommission", "CardTypeId", "NewVPlusSales.CardType");
            DropForeignKey("NewVPlusSales.BeneficiaryPayment", "BeneficiaryAccountTransactionId", "NewVPlusSales.BeneficiaryAccountTransaction");
            DropForeignKey("NewVPlusSales.CardRequisitionItem", "CardRequisitionId", "NewVPlusSales.CardRequisition");
            DropForeignKey("NewVPlusSales.CardRequisitionItem", "BeneficiaryId", "NewVPlusSales.Beneficiary");
            DropForeignKey("NewVPlusSales.CardRequisition", "BeneficiaryId", "NewVPlusSales.Beneficiary");
            DropForeignKey("NewVPlusSales.Beneficiary", "BeneficiaryAccountId", "NewVPlusSales.BeneficiaryAccount");
            DropForeignKey("NewVPlusSales.BeneficiaryAccountTransaction", "BeneficiaryAccountId", "NewVPlusSales.BeneficiaryAccount");
            DropIndex("NewVPlusSales.CardIssuance", new[] { "CardItem_CardItemId" });
            DropIndex("NewVPlusSales.CardIssuance", new[] { "CardRequisitionId" });
            DropIndex("NewVPlusSales.CardDelivery", new[] { "CardItemId" });
            DropIndex("NewVPlusSales.CardItem", new[] { "CardId" });
            DropIndex("NewVPlusSales.Card", new[] { "CardTypeId" });
            DropIndex("NewVPlusSales.CardCommission", new[] { "CardTypeId" });
            DropIndex("NewVPlusSales.BeneficiaryPayment", new[] { "BeneficiaryAccountTransactionId" });
            DropIndex("NewVPlusSales.CardRequisitionItem", new[] { "BeneficiaryId" });
            DropIndex("NewVPlusSales.CardRequisitionItem", new[] { "CardRequisitionId" });
            DropIndex("NewVPlusSales.CardRequisition", new[] { "BeneficiaryId" });
            DropIndex("NewVPlusSales.BeneficiaryAccountTransaction", new[] { "BeneficiaryAccountId" });
            DropIndex("NewVPlusSales.Beneficiary", new[] { "BeneficiaryAccountId" });
            DropTable("NewVPlusSales.CardIssuance");
            DropTable("NewVPlusSales.CardDelivery");
            DropTable("NewVPlusSales.CardItem");
            DropTable("NewVPlusSales.Card");
            DropTable("NewVPlusSales.CardType");
            DropTable("NewVPlusSales.CardCommission");
            DropTable("NewVPlusSales.BeneficiaryPayment");
            DropTable("NewVPlusSales.CardRequisitionItem");
            DropTable("NewVPlusSales.CardRequisition");
            DropTable("NewVPlusSales.BeneficiaryAccountTransaction");
            DropTable("NewVPlusSales.BeneficiaryAccount");
            DropTable("NewVPlusSales.Beneficiary");
        }
    }
}
