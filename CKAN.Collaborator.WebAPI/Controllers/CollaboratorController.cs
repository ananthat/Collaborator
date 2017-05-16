using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CKAN.Collaborator.Business.Entities;
using CKAN.Collaborator.DAL;
using System.Data.Entity.Core.Objects;
using System.Net.Mail;
using System.Configuration;
using CKAN.Collaborator.Utilities;

namespace CKAN.Collaborator.WebAPI.Controllers
{
    [RoutePrefix("api/Collaborator")]
    public class CollaboratorController : ApiController
    {
        GenericUnitOfWork _unitOfCollaborator;

        [Route("Test")]
        [HttpGet]
        public HttpResponseMessage Get()
        {
            return Request.CreateResponse(HttpStatusCode.OK, "ananth sindhu aaradhana");
        }

        [Route("Add")]
        [HttpPost]
        public HttpResponseMessage PostCollaborator(Collaborators objCollaborators)
        {
            //TblCollaborator objTblCollaborator = ConvertToCollaboratorDataModel(objCollaborators);
            //_unitOfCollaborator.GetRepoInstance<TblCollaborator>().Insert(objTblCollaborator);
            //_unitOfCollaborator.SaveChanges();
            HttpResponseMessage objHTTPResponseMessage;
            _unitOfCollaborator = new GenericUnitOfWork();

            RandomStringGenerator objRandomStringGenerator = new RandomStringGenerator();
            objCollaborators.Token = objRandomStringGenerator.Generate(64);

            ObjectResult<string> objResult = _unitOfCollaborator.GetRepoInstance<TblCollaborator>().SPSave(objCollaborators.VaultID, objCollaborators.CollabeMail, objCollaborators.UserID, objCollaborators.CollabeID, objCollaborators.Token, objCollaborators.IPAddress);
            objHTTPResponseMessage = Request.CreateResponse(objResult.FirstOrDefault());
            if (objHTTPResponseMessage.StatusCode == HttpStatusCode.OK)
                SendMail(objCollaborators);
            return objHTTPResponseMessage;
        }

        [Route("Delete")]
        [HttpPost]
        public HttpResponseMessage DeleteCollaborator(Collaborators objCollaborators)
        {
            //_unitOfCollaborator.GetRepoInstance<TblCollaborator>().Delete(objTblCollaborator);
            HttpResponseMessage objHTTPResponseMessage;
            _unitOfCollaborator = new GenericUnitOfWork();
            ObjectResult<string> objResult = _unitOfCollaborator.GetRepoInstance<TblCollaborator>().SPDelete(objCollaborators.CollabeID, objCollaborators.UserID, objCollaborators.IPAddress);
            objHTTPResponseMessage = Request.CreateResponse(objResult.FirstOrDefault());
            return objHTTPResponseMessage;
        }

        [Route("Verify")]
        [HttpPost]
        public HttpResponseMessage VerifyCollaborator(string Token, string Email, string VaultID)
        {
            HttpResponseMessage objHTTPResponseMessage = new HttpResponseMessage();
            _unitOfCollaborator = new GenericUnitOfWork();
            ObjectResult<string> objResult = _unitOfCollaborator.GetRepoInstance<TblCollaborator>().SPVerify(Convert.ToInt32(VaultID), Email, Token);
            return objHTTPResponseMessage;
        }

        private TblCollaborator ConvertToCollaboratorDataModel(Collaborators objCollaborators)
        {
            TblCollaborator objTblCollaborators = new TblCollaborator();
            objTblCollaborators.CollabeEmail = objCollaborators.CollabeMail;
            objTblCollaborators.VaultID = objCollaborators.VaultID;
            objTblCollaborators.UserID = objCollaborators.UserID;
            objTblCollaborators.CollabID = 1;
            objTblCollaborators.Token = string.Empty;
            return objTblCollaborators;
        }

        private bool IsUserExist(int UserID)
        {
            bool IsUserExist = false;
            using (CKANDBEntities context = new CKANDBEntities())
            {
                if ((from x in context.TblUsers where x.ID == UserID select x.ID).Count() > 0)
                    IsUserExist = true;
            }
            return IsUserExist;
        }

        private bool IsDuplicateCollaborator(Collaborators objCollaborator)
        {
            bool IsDuplicateCollaborator = false;
            using (CKANDBEntities context = new CKANDBEntities())
            {
                if ((from x in context.TblCollaborators
                     where
                        x.VaultID == objCollaborator.VaultID &&
                        x.CollabeEmail == objCollaborator.CollabeMail &&
                        x.UserID == objCollaborator.UserID
                     select x.ID).Count() > 0)
                    IsDuplicateCollaborator = true;
            }
            return IsDuplicateCollaborator;
        }

        private bool SendMail(Collaborators objCollaborators)
        {
            bool IsMailSent = false;
            try
            {
                MailMessage mail = new MailMessage();

                string SMPTPServer = ConfigurationManager.AppSettings["SMTPServer"] != null ?
                                        ConfigurationManager.AppSettings["SMTPServer"].ToString() : string.Empty;
                string SenderAddress = ConfigurationManager.AppSettings["SenderAddress"] != null ?
                                            ConfigurationManager.AppSettings["SenderAddress"].ToString() : string.Empty;
                string RecipientAddress = ConfigurationManager.AppSettings["RecipientAddress"] != null ?
                                            ConfigurationManager.AppSettings["RecipientAddress"].ToString() : string.Empty;
                int PortNumber = ConfigurationManager.AppSettings["PortNumber"] != null ?
                                    Convert.ToInt32(ConfigurationManager.AppSettings["PortNumber"].ToString()) : 0;
                string UserName = ConfigurationManager.AppSettings["UserName"] != null ?
                                    ConfigurationManager.AppSettings["UserName"].ToString() : string.Empty;
                string Password = ConfigurationManager.AppSettings["Password"] != null ?
                                    ConfigurationManager.AppSettings["Password"].ToString() : string.Empty;
                string VerifyURL = ConfigurationManager.AppSettings["VerifyURL"] != null ?
                                    ConfigurationManager.AppSettings["VerifyURL"].ToString() : string.Empty;

                SmtpClient SmtpServer = new SmtpClient(SMPTPServer);

                mail.From = new MailAddress(SenderAddress);
                mail.To.Add(RecipientAddress);
                mail.Subject = "Test Mail for Cloud Kickers";
                mail.Body = "Test Mail for Cloud Kickers";
                //Attachment attachment = new Attachment(filename);
                //mail.Attachments.Add(attachment);

                RandomStringGenerator objRandomStringGenerator = new RandomStringGenerator();
                string strVerifyToken = objRandomStringGenerator.Generate(64);
                VerifyURL = VerifyURL.Replace("@Token", strVerifyToken);
                VerifyURL = VerifyURL.Replace("@Email", objCollaborators.CollabeMail);
                VerifyURL = VerifyURL.Replace("@VaultID", objCollaborators.VaultID.ToString());
                VerifyURL = VerifyURL.Replace("#", "&");

                mail.Body = VerifyURL;

                SmtpServer.Port = PortNumber;
                SmtpServer.Credentials = new System.Net.NetworkCredential(UserName, Password);
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);
                IsMailSent = true;

            }
            catch (Exception ex)
            {
            }
            return IsMailSent;
        }
    }
}
