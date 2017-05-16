using System;

namespace CKAN.Collaborator.Business.Entities
{
    public class BaseEntity
    {
        public int ID { get; set; }
        public bool IsActive { get; set; }
        public string IPAddress { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
