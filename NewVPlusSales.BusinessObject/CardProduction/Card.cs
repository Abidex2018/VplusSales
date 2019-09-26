using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NewVPlusSales.BusinessObject.Settings;
using NewVPlusSales.Common;

namespace NewVPlusSales.BusinessObject.CardProduction
{
    [Table("NewVPlusSales.Card")]
    public class Card
    {

        public Card()
        {
            CardItems = new HashSet<CardItem>();
        }
        public int CardId { get; set; }

        [Column(TypeName = "varchar")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Card Title is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Card Title  must be between 3 and 200 characters")]
        public string CardTitle { get; set; }

        [CheckNumber(0,ErrorMessage = "Card Type is Required")]
        public int CardTypeId { get; set; }

        [CheckNumber(0, ErrorMessage = "Total Quantity is Required")]
        public int TotalQuantity { get; set; }

        [Column(TypeName = "varchar")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Batch key is required")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Batch key  must be 2 characters")]
        public string BatchKey { get; set; }

        [Column(TypeName = "varchar")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Start Batch  is required")]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "Start Batch must be 5 characters")]
        public string StartBatchId { get; set; }

        [Column(TypeName = "varchar")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Stop Batch  is required")]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "Stop Batch must be 5 characters")]
        public string StopBatchId { get; set; }

        [CheckNumber(0, ErrorMessage = "Number of Batch is Required")]
        public int NumberOfBatches { get; set; }

        [CheckNumber(0, ErrorMessage = "Quantity per batch is Required")]
        public int QuantityPerBatch { get; set; }

        [Column(TypeName = "varchar")]
        [Required(AllowEmptyStrings = true, ErrorMessage = "Registration Date - Time is Required")]
        [StringLength(35, MinimumLength = 10, ErrorMessage = "Registration Date - Time must be between 5 and 35 Character")]
        public string TimeStampRegisered { get; set; }

        public CardStatus Status { get; set; }

        public virtual CardType CardType { get; set; }

        public ICollection<CardItem> CardItems { get; set; }
    }
}
