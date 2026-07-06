namespace GenericCrud.Models
{
    public class Statuses
    {
   
        public int StatusesID { get; set; }

        public string? StatusName { get; set; }

        public string? LIP { get; set; }

        public string? LMAC { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string? StatusType { get; set; }

        public DateTime? DeletedAt { get; set; }

        public int? DeletedBy { get; set; }

        
    }

}
