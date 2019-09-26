using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NewVPlusSales.BusinessObject.Settings;
using NewVPlusSales.Common;

namespace NewVPlusSales.BusinessObject.Transaction
{
    [Table("NewVPlusSales.CardRequisition")]
    public class CardRequisition
    {

        public CardRequisition()
        {
            CardRequisitionItems = new HashSet<CardRequisitionItem>();
        }
        public long CardRequisitionId { get; set; }

        [Column(TypeName = "varchar")]
        [Required(AllowEmptyStrings = true, ErrorMessage = "Title is required")]
        [StringLength(150)]
        public string RequisitionTitle { get; set; }

        [CheckNumber(0, ErrorMessage = "Beneficiary Id is required")]
        public  int BeneficiaryId { get; set; }

        [CheckNumber(0, ErrorMessage = "Total Quantity Requested is required")]
        public int TotalQuantityRequested { get; set; }

        public int RequestedBy { get; set; }

        [Column(TypeName = "varchar")]
        [Required(AllowEmptyStrings = true, ErrorMessage = "Requested Date - Time is Required")]
        [StringLength(35, ErrorMessage = "Requested Date - Time must be between 1 and 35 Character")]
        public string TimeStampRequested { get; set; }

        public int ApprovedBy { get; set; }

        [Column(TypeName = "varchar")]
        [Required(AllowEmptyStrings = true, ErrorMessage = "Approval Comment is required")]
        [StringLength(150, ErrorMessage = "Approval Comment must be between 1 and 150 characters")]
        public string ApproverComment { get; set; }

        [Column(TypeName = "varchar")]
        [Required(AllowEmptyStrings = true, ErrorMessage = "Approved Date - Time is Required")]
        [StringLength(35, ErrorMessage = "Approved Date - Time must be between 1 and 35 Character")]
        public string TimeStampApproved { get; set; }


        [CheckNumber(0, ErrorMessage = "Quantity Approved is required")]
        public int QuantityApproved { get; set; }

        public CardRequisitionStatus Status { get; set; }

        public virtual Beneficiary Beneficiary { get; set; }

        public ICollection<CardRequisitionItem> CardRequisitionItems { get; set; }
    }
}
