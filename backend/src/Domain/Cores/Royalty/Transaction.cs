using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Domain.Commons;

namespace Domain.Cores.Royalty
{
    [Table("Transactions")]
    public class Transaction : AuditableEntity
    {
        [MaxLength(250)]
        [Required]
        public string FromUserName { get; set; } = string.Empty;
        public Guid FromUserId { get; set; }

        [MaxLength(250)]
        [Required]
        public string ToUserName { get; set; } = string.Empty;
        public Guid ToUserId { get; set; }

        public decimal Amount { get; set; }
        public TransactionType TransactionType { get; set; }

        [MaxLength(250)]
        public string? Note { get; set; }
    }

    public enum TransactionType
    {
        RoyaltyPay,
    }
}
