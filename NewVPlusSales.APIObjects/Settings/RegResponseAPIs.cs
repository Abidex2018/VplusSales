using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security;
using NewVPlusSale.APIObjects.Common;
using NewVPlusSales.APIObjects.Common;
using NewVPlusSales.Common;

namespace NewVPlusSales.APIObjects.Settings
{
    public class SettingsRegResponseObj
    {
        public int SettingId;
        public APIResponseStatus Status;
    }

    #region Card Type Response Object

    public class CardTypeRespObj
    {
        public List<CardTypeItemObj> CardTypes;
        public APIResponseStatus Status;
    }

    public class CardTypeItemObj
    {
        public int CardTypedId;
        public string Name;
        public decimal FaceValue;
        public int CardStatus;
        public string CardStatusLabel;
    }
    #endregion

    #region Card Commission Response Object

    public class CardCommissionRespObj
    {
        public List<CardCommissionItemObj> CardCommissions;
        public APIResponseStatus Status;
    }

    public class CardCommissionItemObj
    {

        public int CardCommissionId;
        public int CardTypeId;
        public string CardTypeName;
        public decimal LowerAmount;
        public decimal UpperAmount;
        public decimal CommissionRate;
        public int Status;
        public string Statuslabel;
    }

    #endregion


    #region Beneficiary Response Object

    public class BeneficiaryRespObj
    {
        public List<BeneficiaryRespItem> Beneficiaries;
        public APIResponseStatus Status;
    }

    public class BeneficiaryRespItem
    {
        public int BeneficiaryId;
        public string FullName;
        public string Address;
        public string Email;
        public string MobileNumber;
        public int BeneficiaryTypeId;
        public string BeneficiaryTypeLabel;
        public string ApprovedComment;
        public int ApprovedBy;
        public string TimeStampRegistered;
        public string TimeStampApproved;
        public int Status;
        public string StatusLabel;
        public BeneficiaryAccountObj BeneficiaryAccount;

    }

    public class BeneficiaryAccountObj
    {
        public int BeneficiaryId;
        public int BeneficiaryAccountId;
        public decimal CreditLimit;
        public decimal AvaliableBalance;
        public decimal LastTransactionAmount;
        public int LastTransactionType;
        public string LastTransactionTypeLabel;
    }
    #endregion

    #region Card Production Response Object

    public class CardRegRespObj
    {
        public long CardId;
        public APIResponseStatus Status;
    }
    public class CardRespObj
    {
        public List<CardObj> Cards;
        public APIResponseStatus Status;
    }

    public class CardObj
    {
        public int CardId;
        public int CardTypeId;
        public string CardTitle;

        public string BatchKey;
        public string StartBatchId;
        public string StopBatchId;
        public int NoOfBatches;
        public int QuantityPerBatch;
        public int TotalQuantity;

        public string TimeStampRegistered;

        public int Status;
        public string StatusLabel;
        public List<CardItemObj> CardItems;
    }

    public class CardItemObj
    {
        public long CardItemId;
        public int CardId;
        public int CardTypeId;

        public string BatchId;
        public string BatchStartNumber;
        public string BatchStopNumber;
        public string DefectiveBatchNumbers;
        public int BatchQuantity;
        public int DefectiveQuantity;

        public int MissingQuantity;
        public int DeliveredQuantity;
        public int AvailableQuantity;
        public int IssuedQuantity;

        public int RegisteredBy;
        public string TimeStampRegistered;
        public string TimeStampDelivered;
        public string TimeStampIssued;

        public int Status;
        public string StatusLabel;
    }

    #endregion

    #region Card Delivery Response Object

    public class CardDeliveryRegRespObj
    {
        public long CardDeliveryId;
        public APIResponseStatus Status;
    }

    public class CardDeliveryRespObj
    {
        public List<CardDeliveryObj> CardDeliveries;
        public APIResponseStatus Status;
    }

    public class CardDeliveryObj
    {
        public long CardDeliveryId;
        public long CardItemId;
        public int CardId;
        public string CardIdLabel { get; set; }
        public int CardTypeId;
        public string BatchId;
        public string StartBatchNumber;
        public string StopBatchNumber;
        public int QuantityDelivered;
        public int MissingQuantity;
        public int DefectiveQuantity;
        public string DeliveryDate;
        public string TimeStampRegistered;
        public int ApprovedBy;
        public string ApproverComment;
        public string TimeStampApproved;
        public int RecievedBy;
        public int Status;
        public string StatusLabel;
    }

    #endregion

    #region Beneficiary Payment Response Object

    public class BeneficiaryPaymentRegRespObj
    {
        public long PaymentId;
        public APIResponseStatus Status;
    }
    public class BeneficiaryAccountTransRegRespObj
    {
        public long TransactionId;
        public APIResponseStatus Status;
    }
    public class BeneficiaryPaymentRespObj
    {
        public List<BeneficiaryPaymentObj> BeneficiaryPayments;
        public APIResponseStatus Status;
    }

    public class BeneficiaryPaymentObj
    {
        public long BeneficiaryPaymentId;
        public long BeneficiaryAccountTransactionId;
        public int BeneficiaryAccountId;
        public int BeneficiaryId;
        public decimal AmountPaid;
        public int PaymentSource;
        public string PaymentSourceName;
        public string PaymentReference;
        public string PaymentDate;
        public int RegisteredBy;
        public string TimeStampRegistered;
        public int Status;
        public string StatusLabel;
    }
    #endregion

    #region Beneficiary Account Transaction Response Object
    
    public class BeneficiaryAccTransRespObj
    {
        public List<BeneficiaryAccountTransactionObj> BeneficiaryAccountTransactions;
        public APIResponseStatus Status;
    }

    public class BeneficiaryAccountTransactionObj
    {
        public int BeneficiaryAccountId;
        public int BeneficiaryId;
        public decimal Amount;
        public decimal PreviousBalance;
        public decimal NewBalance;
        public int RegisteredBy;
        public string TimeStampRegistered;
        public int TransactionType;
        public string TransactionTypeLabel;
        public int TransactionSource;
        public string TransactionSourceLabel;
        public int Status;
        public string StatusLabel;
    }
    #endregion


    public class SettingSearchObj:AdminObj
    {
        public int Status { get; set; }
    }
}
