using System;
using System.Collections.Generic;
using System.Text;

namespace CKAN.Collaborator.Business.Entities
{
    public class User : BaseEntity
    {
        public string EMailAddress { get; set; }
        public int NoOfTotalLicense { get; set; }
        public int NoOfAvailableLicense { get; set; }
        public int UserRoleID { get; set; }
    }
}
