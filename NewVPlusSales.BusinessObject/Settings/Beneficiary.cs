using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NewVPlusSales.BusinessObject.Transaction;
using NewVPlusSales.Common;

namespace NewVPlusSales.BusinessObject.Settings
{
    [Table("NewVPlusSales.Beneficiary")]
   public class Beneficiary
    {
        public Beneficiary()
        {
            CardRequisitions=new HashSet<CardRequisition>();
        }

        public int BeneficiaryId { get; set; }

        public int BeneficiaryAccountId { get; set; }

        [Column(TypeName = "varchar")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Full Name is Required")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Full must be between 3 and 150 Character")]
        public string FullName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Beneficiary Type is Required")]
        public BeneficiaryType BeneficiaryType { get; set; }

        [Column(TypeName = "varchar")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Beneficiary Mobile Number is Required")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Invaild Beneficiary Mobile Number")]
        public string MobileNumber { get; set; }

        [Column(TypeName = "varchar")]
        [Required(AllowEmptyStrings = true, ErrorMessage = "Beneficiary Email is Required")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}", ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Column(TypeName = "varchar")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Beneficiary Address is Required")]
        [StringLength(150, MinimumLength = 5, ErrorMessage = "Beneficiary Address must be between 5 and 150 Character")]
        public string Address { get; set; }

        [Column(TypeName = "varchar")]
        [Required(AllowEmptyStrings = true, ErrorMessage = "Registered Date - Time is Required")]
        [StringLength(35,MinimumLength = 10, ErrorMessage = "Registered Date - Time must be between 5 and 35 Character")]
        public string TimeStampRegisered { get; set; }

        public int ApprovedBy { get; set; }

        [Column(TypeName = "varchar")]
        [Required(AllowEmptyStrings = true, ErrorMessage = "Approval Comment is Required")]
        [StringLength(150, ErrorMessage = "Beneficiary Address must be between 1 and 150 Character")]
        public string ApprovalComment { get; set; }

        [Column(TypeName = "varchar")]
        [Required(AllowEmptyStrings = true, ErrorMessage = "Approval Date - Time is Required")]
        [StringLength(35, ErrorMessage = "Approval Date - Time must be between 1 and 35 Character")]
        public string TimeStampApproved { get; set; }

        public  virtual  BeneficiaryAccount BeneficiaryAccount { get; set; }
        public ICollection<CardRequisition> CardRequisitions { get; set; }
        public Status Status { get; set; }
    }
}
