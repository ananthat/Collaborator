using System;
using System.Collections.Generic;
using System.Text;

namespace CKAN.Collaborator.Business.Entities
{
    public class Collaborators : BaseEntity
    {
        public int UserID { get; set; }
        public int VaultID { get; set; }
        public string CollabeMail { get; set; }
        public int CollabeID { get; set; }
        public string Token { get; set; }
    }
}
