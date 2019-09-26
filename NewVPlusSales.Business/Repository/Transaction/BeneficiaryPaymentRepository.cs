using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewVPlusSale.APIObjects.Common;
using NewVPlusSales.APIObjects.Settings;
using NewVPlusSales.Business.Core;
using NewVPlusSales.Business.DataManager;
using NewVPlusSales.Business.Infrastructure;
using NewVPlusSales.Business.Infrastructure.Contract;
using NewVPlusSales.BusinessObject.Settings;
using NewVPlusSales.BusinessObject.Transaction;
using NewVPlusSales.Common;
using XPLUG.WEBTOOLS;

namespace NewVPlusSales.Business.Repository.Transaction
{
   internal class BeneficiaryPaymentRepository
   {
       private readonly INewVPlusSalesRepository<Beneficiary> _beneRepositiory;
       private readonly INewVPlusSalesRepository<BeneficiaryPayment> _repositiory;
       private readonly INewVPlusSalesRepository<BeneficiaryAccount> _beneAccRepository;
       private readonly INewVPlusSalesRepository<BeneficiaryAccountTransaction> _beneAccTransRepository;
       private readonly NewVPlusSalesUoWork _uoWork;

       public BeneficiaryPaymentRepository()
       {
           _beneAccRepository= new NewVPlusSalesRepository<BeneficiaryAccount>(_uoWork);
            _repositiory= new NewVPlusSalesRepository<BeneficiaryPayment>(_uoWork);
            _beneAccTransRepository=new NewVPlusSalesRepository<BeneficiaryAccountTransaction>(_uoWork);
            _beneRepositiory=new NewVPlusSalesRepository<Beneficiary>(_uoWork);
       }

       public BeneficiaryPaymentRegRespObj AddBeneficiaryPayment(RegBeneficiaryPaymentObj regObj)
       {
           var response = new BeneficiaryPaymentRegRespObj
           {
               Status = new APIResponseStatus
               {
                   IsSuccessful = false,
                   Message = new APIResponseMessage()
               }
           };

           try
           {
               if (regObj.Equals(null))
               {
                   response.Status.Message.FriendlyMessage = "Error Occurred! Unable to proceed with your request";
                   response.Status.Message.TechnicalMessage = "Registration Object is empty / invalid";
                   return response;
               }

               if (!EntityValidatorHelper.Validate(regObj, out var valResults))
               {
                   var errorDetail = new StringBuilder();
                   if (!valResults.IsNullOrEmpty())
                   {
                       errorDetail.AppendLine("Following error occurred:");
                       valResults.ForEachx(m => errorDetail.AppendLine(m.ErrorMessage));
                   }
                   else
                   {
                       errorDetail.AppendLine("Validation error occurred! Please check all supplied parameters and try again");
                   }
                   response.Status.Message.FriendlyMessage = errorDetail.ToString();
                   response.Status.Message.TechnicalMessage = errorDetail.ToString();
                   response.Status.IsSuccessful = false;
                   return response;
               }

               if (!HelperMethods.IsUserValid(regObj.AdminUserId, regObj.SysPathCode, HelperMethods.getRequesterRoles(), ref response.Status.Message))
               {
                   return response;
               }

               var associatedBeneficiary = GetBeneficiaryInfo(regObj.BeneficiaryId);
               if (associatedBeneficiary == null)
               {
                   response.Status.Message.FriendlyMessage = "Ooops ! Beneficiary Does not Exist!";
                   response.Status.Message.TechnicalMessage = "No Beneficiary Information Found";
                   return response;
                }

               if (associatedBeneficiary.Status != Status.Active)
               {
                   response.Status.Message.FriendlyMessage = "Sorry!This Beneficiary Cannot Perform Any Transaction Yet! Please Contact Administrator";
                   response.Status.Message.TechnicalMessage = "Beneficiary Isn't Approved yet";
                   return response;
                }
               var associatedBeneAccount = GetBeneficiaryAccountInfo(associatedBeneficiary.BeneficiaryAccountId);
               if (associatedBeneAccount == null)
               {
                   response.Status.Message.FriendlyMessage = "Ooops ! Beneficiary Account Does not Exist!";
                   response.Status.Message.TechnicalMessage = "No Beneficiary Account Information Found";
                   return response;
                }

               if (!string.IsNullOrWhiteSpace(regObj.PaymentReference))
               {
                   if (DataCheck.IsNumeric(regObj.PaymentReference))
                   {
                       response.Status.Message.FriendlyMessage = "Error Occurred! Payment reference must be Numbers Only";
                       response.Status.Message.TechnicalMessage = "Payment Reference  is invalid";
                       return response;
                    }

                   var associatedBenPayment = GetBeneficiaryPayment(new SettingSearchObj {Status = -2});
                   if (associatedBenPayment.Any())
                   {
                       if (associatedBenPayment.FindAll(payment => payment.PaymentReference == regObj.PaymentReference)
                           .Any())
                       {
                           response.Status.Message.FriendlyMessage = "Error Occurred! Payment reference is Invalid";
                           response.Status.Message.TechnicalMessage = "Duplicate Error! Payment Reference  is Invalid!";
                           return response;
                        }
                   }
               }

               //store date for Concurrency...
               var nowDateTime = DateMap.CurrentTimeStamp();
               var nowDate = nowDateTime.Substring(0, nowDateTime.IndexOf(' '));
               var nowTime = nowDateTime.Substring(nowDateTime.IndexOf('-') + 1);

               using (var db= _uoWork.BeginTransaction())
               {
                    #region Beneficiary Account Transaction Operation
                    var newBeneficiaryTransaction = new BeneficiaryAccountTransaction
                    {
                        BeneficiaryAccountId = associatedBeneAccount.BeneficiaryAccountId,
                        BeneficiaryId = regObj.BeneficiaryId,
                        Amount = regObj.AmountPaid,
                        PreviousBalance = associatedBeneAccount.AvailableBalance,
                        NewBalance = (associatedBeneAccount.AvailableBalance+regObj.AmountPaid),
                        TransactionSource = TransactionSourceType.Account_TopUp,
                        TransactionType = TransactionType.Credit,
                        Status = Status.Active,
                        RegisteredBy = regObj.AdminUserId,
                        TimeStampRegistered = nowDateTime,

                    };

                   var transactionAdded = _beneAccTransRepository.Add(newBeneficiaryTransaction);
                    _uoWork.SaveChanges();
                   if (transactionAdded.BeneficiaryAccountTransactionId < 1)
                   {
                       db.Rollback();
                       response.Status.Message.FriendlyMessage = "Error Occurred! Unable to complete your request. Please try again later";
                       response.Status.Message.TechnicalMessage = "Unable to save to database";
                       return response;
                    }

                   #endregion

                   #region Beneficiary Payment Operation

                   var newBeneficiaryPayment= new BeneficiaryPayment
                   {
                       BeneficiaryId = regObj.BeneficiaryId,
                       BeneficiaryAccountId = associatedBeneAccount.BeneficiaryAccountId,
                       PaymentDate = regObj.PaymentDate,
                       PaymentReference = regObj.PaymentReference,
                       PaySource = (PaySource)regObj.PaySource,
                       PaymentSourceName = ((PaySource)regObj.PaySource).ToString().Replace("_", " "),
                       RegisteredBy = regObj.AdminUserId,
                       TimeStampRegistered = nowDateTime,
                       Status = Status.Active,
                       AmountPaid = regObj.AmountPaid,
                       BeneficiaryAccountTransactionId = transactionAdded.BeneficiaryAccountTransactionId,
                   };

                   var paymentAdded = _repositiory.Add(newBeneficiaryPayment);
                    _uoWork.SaveChanges();
                   if (paymentAdded.BeneficiaryPaymentId < 1)
                   {
                       db.Rollback();
                       response.Status.Message.FriendlyMessage = "Error Occurred! Unable to complete your request. Please try again later";
                       response.Status.Message.TechnicalMessage = "Unable to save to database";
                       return response;
                    }
                    #endregion

                    #region Beneficiary Account Update

                   associatedBeneAccount.AvailableBalance = transactionAdded.NewBalance;
                   associatedBeneAccount.CreditLimit = 0;
                   associatedBeneAccount.LastTransactionAmount = transactionAdded.Amount;
                   associatedBeneAccount.LastTransactionType = transactionAdded.TransactionType;
                   associatedBeneAccount.Status = Status.Active;
                   associatedBeneAccount.LastTransactionId = transactionAdded.BeneficiaryAccountTransactionId;
                   associatedBeneAccount.LastTransactionTimeStamp = DateMap.CurrentTimeStamp();

                   var updateBeneAccount = _beneAccRepository.Update(associatedBeneAccount);
                    _uoWork.SaveChanges();
                   if (updateBeneAccount.BeneficiaryAccountId < 1)
                   {
                       db.Rollback();
                       response.Status.Message.FriendlyMessage = "Error Occurred! Unable to complete your request. Please try again later";
                       response.Status.Message.TechnicalMessage = "Unable to save to database";
                       return response;
                    }

                   #endregion

                   db.Commit();
                   response.Status.IsSuccessful = true;
                   response.PaymentId = paymentAdded.BeneficiaryPaymentId;
                   response.Status.Message.FriendlyMessage = "Payment Successful";
               }

               
            }
           catch (DbEntityValidationException ex)
           {
               ErrorManager.LogApplicationError(ex.StackTrace, ex.Source, ex.Message);
               response.Status.Message.FriendlyMessage = "Error Occurred! Please try again later";
               response.Status.Message.TechnicalMessage = "Error: " + ex.GetBaseException().Message;
               response.Status.IsSuccessful = false;
               return response;
           }
           catch (Exception ex)
           {
               ErrorManager.LogApplicationError(ex.StackTrace, ex.Source, ex.Message);
               response.Status.Message.FriendlyMessage = "Error Occurred! Please try again later";
               response.Status.Message.TechnicalMessage = "Error: " + ex.GetBaseException().Message;
               response.Status.IsSuccessful = false;
               return response;
           }

           return response;

        }

       public BeneficiaryAccTransRespObj LoadBeneficiaryTransactionByDate(
           LoadBeneficiaryAccountTransactionByDateObj regObj)
       {
           var response = new BeneficiaryAccTransRespObj
           {
               Status = new APIResponseStatus
               {
                   IsSuccessful = false,
                   Message = new APIResponseMessage()
               }
           };

           try
           {

               if (regObj.Equals(null))
               {
                   response.Status.Message.FriendlyMessage = "Error Occurred! Unable to proceed with your request";
                   response.Status.Message.TechnicalMessage = "Registration Object is empty / invalid";
                   return response;
               }

               if (!EntityValidatorHelper.Validate(regObj, out var valResults))
               {
                   var errorDetail = new StringBuilder();
                   if (!valResults.IsNullOrEmpty())
                   {
                       errorDetail.AppendLine("Following error occurred:");
                       valResults.ForEachx(m => errorDetail.AppendLine(m.ErrorMessage));
                   }
                   else
                   {
                       errorDetail.AppendLine("Validation error occurred! Please check all supplied parameters and try again");
                   }
                   response.Status.Message.FriendlyMessage = errorDetail.ToString();
                   response.Status.Message.TechnicalMessage = errorDetail.ToString();
                   response.Status.IsSuccessful = false;
                   return response;
               }

               if (!HelperMethods.IsUserValid(regObj.AdminUserId, regObj.SysPathCode, HelperMethods.getRequesterRoles(), ref response.Status.Message))
               {
                   return response;
               }

               var thisBeneficiaryAccountTransactions = GetAccTransactionBene(regObj.BeneficiaryId);
               if (!thisBeneficiaryAccountTransactions.Any())
               {
                   response.Status.Message.FriendlyMessage = "No Beneficiary Payments  Information found!";
                   response.Status.Message.TechnicalMessage = "No Beneficiary Payments  Information found!";
                   return response;
                }

               var beneficiaryAccountTransactionByDate= new List<BeneficiaryAccountTransactionObj>();

               foreach (var m in thisBeneficiaryAccountTransactions)
               {
                   if (!string.IsNullOrWhiteSpace(regObj.BeginDate) || !string.IsNullOrWhiteSpace(regObj.EndDate))
                   {
                       var dateCreated = m.TimeStampRegistered;
                       var value = dateCreated.IndexOf(' ');
                       if (value > 0) { dateCreated = dateCreated.Substring(0, value); }
                       var realDate = DateTime.Parse(dateCreated);
                       if (realDate >= DateTime.Parse(regObj.BeginDate) && realDate <= DateTime.Parse(regObj.EndDate))
                       {
                           beneficiaryAccountTransactionByDate.Add(new BeneficiaryAccountTransactionObj
                           {
                               BeneficiaryId = m.BeneficiaryId,
                               Amount = m.Amount,
                               BeneficiaryAccountId = m.BeneficiaryAccountId,
                               NewBalance = m.NewBalance,
                               PreviousBalance = m.PreviousBalance,
                               TransactionSource = (int)m.TransactionSource,
                               TransactionSourceLabel = m.TransactionSource.ToString().Replace("_", " "),
                               TransactionType = (int)m.TransactionType,
                               TransactionTypeLabel = m.TransactionType.ToString().Replace("_", " "),
                               Status = (int)m.Status,
                               StatusLabel = m.Status.ToString().Replace("_", " "),
                               RegisteredBy = m.RegisteredBy,
                               TimeStampRegistered = m.TimeStampRegistered
                           });
                       }
                    }

                   else
                   {
                       beneficiaryAccountTransactionByDate.Add(new BeneficiaryAccountTransactionObj
                       {
                           BeneficiaryId = m.BeneficiaryId,
                           Amount = m.Amount,
                           BeneficiaryAccountId = m.BeneficiaryAccountId,
                           NewBalance = m.NewBalance,
                           PreviousBalance = m.PreviousBalance,
                           TransactionSource = (int)m.TransactionSource,
                           TransactionSourceLabel = m.TransactionSource.ToString().Replace("_", " "),
                           TransactionType = (int)m.TransactionType,
                           TransactionTypeLabel = m.TransactionType.ToString().Replace("_", " "),
                           Status = (int)m.Status,
                           StatusLabel = m.Status.ToString().Replace("_", " "),
                           RegisteredBy = m.RegisteredBy,
                           TimeStampRegistered = m.TimeStampRegistered
                       });
                    }

                  
               }
               response.Status.IsSuccessful = true;
               response.BeneficiaryAccountTransactions = beneficiaryAccountTransactionByDate;
               return response;
            }
           catch (Exception ex)
           {
              ErrorManager.LogApplicationError(ex.StackTrace,ex.Source,ex.Message);
               return new BeneficiaryAccTransRespObj();
           }
        }

        public BeneficiaryPaymentRespObj LoadBeneficiaryTransactionByDate(LoadBeneficiaryPaymentByDateObj regObj)
        {
            var response = new BeneficiaryPaymentRespObj
            {
                Status = new APIResponseStatus
                {
                    IsSuccessful = false,
                    Message = new APIResponseMessage()
                }
            };

            try
            {

                if (regObj.Equals(null))
                {
                    response.Status.Message.FriendlyMessage = "Error Occurred! Unable to proceed with your request";
                    response.Status.Message.TechnicalMessage = "Registration Object is empty / invalid";
                    return response;
                }

                if (!EntityValidatorHelper.Validate(regObj, out var valResults))
                {
                    var errorDetail = new StringBuilder();
                    if (!valResults.IsNullOrEmpty())
                    {
                        errorDetail.AppendLine("Following error occurred:");
                        valResults.ForEachx(m => errorDetail.AppendLine(m.ErrorMessage));
                    }
                    else
                    {
                        errorDetail.AppendLine("Validation error occurred! Please check all supplied parameters and try again");
                    }
                    response.Status.Message.FriendlyMessage = errorDetail.ToString();
                    response.Status.Message.TechnicalMessage = errorDetail.ToString();
                    response.Status.IsSuccessful = false;
                    return response;
                }

                if (!HelperMethods.IsUserValid(regObj.AdminUserId, regObj.SysPathCode, HelperMethods.getRequesterRoles(), ref response.Status.Message))
                {
                    return response;
                }

                var thisBeneficiaryPayment = GetBeneficiaryPayment(regObj.BeneficiaryId);
                if (!thisBeneficiaryPayment.Any())
                {
                    response.Status.Message.FriendlyMessage = "No Beneficiary Payments  Information found!";
                    response.Status.Message.TechnicalMessage = "No Beneficiary Payments  Information found!";
                    return response;
                }

                var beneficiaryPaymentByDate = new List<BeneficiaryPaymentObj>();

                foreach (var m in thisBeneficiaryPayment)
                {
                    if (!string.IsNullOrWhiteSpace(regObj.BeginDate) || !string.IsNullOrWhiteSpace(regObj.EndDate))
                    {
                        var dateCreated = m.TimeStampRegistered;
                        var value = dateCreated.IndexOf(' ');
                        if (value > 0) { dateCreated = dateCreated.Substring(0, value); }
                        var realDate = DateTime.Parse(dateCreated);
                        if (realDate >= DateTime.Parse(regObj.BeginDate) && realDate <= DateTime.Parse(regObj.EndDate))
                        {
                            beneficiaryPaymentByDate.Add(new BeneficiaryPaymentObj
                            {
                                BeneficiaryId = m.BeneficiaryId,
                                BeneficiaryAccountTransactionId = m.BeneficiaryAccountTransactionId,
                                BeneficiaryAccountId = m.BeneficiaryAccountId,
                                AmountPaid = m.AmountPaid,
                                BeneficiaryPaymentId = m.BeneficiaryPaymentId,
                                PaymentDate = m.PaymentDate,
                                PaymentReference = m.PaymentReference,
                                Status = (int)m.Status,
                                StatusLabel = m.Status.ToString().Replace("_", " "),
                                PaymentSource = (int)m.PaySource,
                                PaymentSourceName = m.PaySource.ToString().Replace("_", " "),
                                RegisteredBy = m.RegisteredBy,
                                TimeStampRegistered = m.TimeStampRegistered
                            });
                        }
                    }

                    else
                    {
                        beneficiaryPaymentByDate.Add(new BeneficiaryPaymentObj
                        {
                            BeneficiaryId = m.BeneficiaryId,
                            BeneficiaryAccountTransactionId = m.BeneficiaryAccountTransactionId,
                            BeneficiaryAccountId = m.BeneficiaryAccountId,
                            AmountPaid = m.AmountPaid,
                            BeneficiaryPaymentId = m.BeneficiaryPaymentId,
                            PaymentDate = m.PaymentDate,
                            PaymentReference = m.PaymentReference,
                            Status = (int)m.Status,
                            StatusLabel = m.Status.ToString().Replace("_", " "),
                            PaymentSource = (int)m.PaySource,
                            PaymentSourceName = m.PaySource.ToString().Replace("_", " "),
                            RegisteredBy = m.RegisteredBy,
                            TimeStampRegistered = m.TimeStampRegistered
                        });
                    }


                }
                response.Status.IsSuccessful = true;
                response.BeneficiaryPayments = beneficiaryPaymentByDate;
                return response;
            }
            catch (Exception ex)
            {
                ErrorManager.LogApplicationError(ex.StackTrace, ex.Source, ex.Message);
                return new BeneficiaryPaymentRespObj();
            }
        }

       public BeneficiaryPaymentRegRespObj DeleteBeneficiaryPayment(DeleteBeneficiaryPaymentObj regObj)
       {
          var response = new BeneficiaryPaymentRegRespObj
          {
              Status = new APIResponseStatus
              {
                  IsSuccessful=false,
                  Message = new APIResponseMessage()
              }
          };

           try
           {
               if (regObj.Equals(null))
               {
                   response.Status.Message.FriendlyMessage = "Error Occurred! Unable to proceed with your request";
                   response.Status.Message.TechnicalMessage = "Registration Object is empty / invalid";
                   return response;
               }

               if (!EntityValidatorHelper.Validate(regObj, out var valResults))
               {
                   var errorDetail = new StringBuilder();
                   if (!valResults.IsNullOrEmpty())
                   {
                       errorDetail.AppendLine("Following error occurred:");
                       valResults.ForEachx(m => errorDetail.AppendLine(m.ErrorMessage));
                   }
                   else
                   {
                       errorDetail.AppendLine("Validation error occurred! Please check all supplied parameters and try again");
                   }
                   response.Status.Message.FriendlyMessage = errorDetail.ToString();
                   response.Status.Message.TechnicalMessage = errorDetail.ToString();
                   response.Status.IsSuccessful = false;
                   return response;
               }

               if (!HelperMethods.IsUserValid(regObj.AdminUserId, regObj.SysPathCode, new[] { "PortalAdmin", "CRMAdmin", "CRMManager" }, ref response.Status.Message))
               {
                   return response;
               }

                var thisBeneficiaryPayment = GetBeneficiaryPaymentInfo(regObj.BeneficiaryPaymentId);
               if (thisBeneficiaryPayment == null)
               {
                   response.Status.Message.FriendlyMessage = "No Beneficiary Payment Information found for the specified BeneficiaryPayment Id";
                   response.Status.Message.TechnicalMessage = "No Beneficiary Payment Information found!";
                   return response;
                }

               thisBeneficiaryPayment.Status = Status.Deleted;

               var deleteBenPayment = _repositiory.Update(thisBeneficiaryPayment);
                _uoWork.SaveChanges();
               if (deleteBenPayment.BeneficiaryPaymentId < 1)
               {
                   response.Status.Message.FriendlyMessage = "Error Occurred! Unable to complete your request. Please try again later";
                   response.Status.Message.TechnicalMessage = "Unable to save to database";
                   return response;
                }

               response.Status.IsSuccessful = true;
               response.PaymentId = deleteBenPayment.BeneficiaryPaymentId;
               response.Status.Message.FriendlyMessage = "Deleted Successfully";
           }
           catch (DbEntityValidationException ex)
           {
               ErrorManager.LogApplicationError(ex.StackTrace, ex.Source, ex.Message);
               response.Status.Message.FriendlyMessage = "Error Occurred! Please try again later";
               response.Status.Message.TechnicalMessage = "Error: " + ex.GetBaseException().Message;
               response.Status.IsSuccessful = false;
               return response;
           }
           catch (Exception ex)
           {
               ErrorManager.LogApplicationError(ex.StackTrace, ex.Source, ex.Message);
               response.Status.Message.FriendlyMessage = "Error Occurred! Please try again later";
               response.Status.Message.TechnicalMessage = "Error: " + ex.GetBaseException().Message;
               response.Status.IsSuccessful = false;
               return response;
           }
           return response;
        }
        public BeneficiaryPaymentRegRespObj DeleteBeneficiaryPayment(DeleteBeneficiaryAccountTransactionObj regObj)
        {
            var response = new BeneficiaryPaymentRegRespObj
            {
                Status = new APIResponseStatus
                {
                    IsSuccessful = false,
                    Message = new APIResponseMessage()
                }
            };

            try
            {
                if (regObj.Equals(null))
                {
                    response.Status.Message.FriendlyMessage = "Error Occurred! Unable to proceed with your request";
                    response.Status.Message.TechnicalMessage = "Registration Object is empty / invalid";
                    return response;
                }

                if (!EntityValidatorHelper.Validate(regObj, out var valResults))
                {
                    var errorDetail = new StringBuilder();
                    if (!valResults.IsNullOrEmpty())
                    {
                        errorDetail.AppendLine("Following error occurred:");
                        valResults.ForEachx(m => errorDetail.AppendLine(m.ErrorMessage));
                    }
                    else
                    {
                        errorDetail.AppendLine("Validation error occurred! Please check all supplied parameters and try again");
                    }
                    response.Status.Message.FriendlyMessage = errorDetail.ToString();
                    response.Status.Message.TechnicalMessage = errorDetail.ToString();
                    response.Status.IsSuccessful = false;
                    return response;
                }

                if (!HelperMethods.IsUserValid(regObj.AdminUserId, regObj.SysPathCode, new[] { "PortalAdmin", "CRMAdmin", "CRMManager" }, ref response.Status.Message))
                {
                    return response;
                }

                var thisBeneficiaryAccTrans = GetBeneficiaryAccTransactionInfo(regObj.BeneficiaryAccTransId);
                if (thisBeneficiaryAccTrans == null)
                {
                    response.Status.Message.FriendlyMessage = "No Beneficiary Payment Information found for the specified BeneficiaryPayment Id";
                    response.Status.Message.TechnicalMessage = "No Beneficiary Payment Information found!";
                    return response;
                }

                thisBeneficiaryAccTrans.Status = Status.Deleted;

                var deleteBenAccTrans = _beneAccTransRepository.Update(thisBeneficiaryAccTrans);
                _uoWork.SaveChanges();
                if (deleteBenAccTrans.BeneficiaryAccountTransactionId < 1)
                {
                    response.Status.Message.FriendlyMessage = "Error Occurred! Unable to complete your request. Please try again later";
                    response.Status.Message.TechnicalMessage = "Unable to save to database";
                    return response;
                }

                response.Status.IsSuccessful = true;
                response.PaymentId = deleteBenAccTrans.BeneficiaryAccountTransactionId;
                response.Status.Message.FriendlyMessage = "Deleted Successfully";
            }
            catch (DbEntityValidationException ex)
            {
                ErrorManager.LogApplicationError(ex.StackTrace, ex.Source, ex.Message);
                response.Status.Message.FriendlyMessage = "Error Occurred! Please try again later";
                response.Status.Message.TechnicalMessage = "Error: " + ex.GetBaseException().Message;
                response.Status.IsSuccessful = false;
                return response;
            }
            catch (Exception ex)
            {
                ErrorManager.LogApplicationError(ex.StackTrace, ex.Source, ex.Message);
                response.Status.Message.FriendlyMessage = "Error Occurred! Please try again later";
                response.Status.Message.TechnicalMessage = "Error: " + ex.GetBaseException().Message;
                response.Status.IsSuccessful = false;
                return response;
            }
            return response;
        }
        private List<BeneficiaryAccountTransaction> GetAccTransactionBene(int benAccTransId)
       {
           try
           {
               if (benAccTransId > 0)
               {
                   var Sql= new StringBuilder();
                   Sql.Append(
                       $"SELECT * FROM \"NewVPlusSales\".\"BeneficiaryAccountTransaction\" WHERE \"BeneficiaryAccountTransactionId\" = {benAccTransId}");
                   var agentInfo = _beneAccTransRepository.RepositoryContext().Database
                       .SqlQuery<BeneficiaryAccountTransaction>(Sql.ToString()).ToList();

                   return !agentInfo.Any() ? new List<BeneficiaryAccountTransaction>() : agentInfo;
               }
               else
               {
                   var Sql1 = new StringBuilder();
                   Sql1.Append(
                       $"SELECT * FROM \"NewVPlusSales\".\"BeneficiaryAccountTransaction\" ORDER BY \"BeneficiaryAccountTransactionId\"");
                   var agentInfo = _beneAccTransRepository.RepositoryContext().Database
                       .SqlQuery<BeneficiaryAccountTransaction>(Sql1.ToString()).ToList();

                   return !agentInfo.Any() ? new List<BeneficiaryAccountTransaction>() : agentInfo;
                }

           }
           catch (Exception ex)
           {
               ErrorManager.LogApplicationError(ex.StackTrace,ex.Source,ex.Message);
                return new List<BeneficiaryAccountTransaction>();
           }
       }

       private List<BeneficiaryPayment> GetBeneficiaryPayment(SettingSearchObj searchObj)
       {
           try
           {
               if (searchObj.Status == -2)
               {
                    var sql= new StringBuilder();
                   sql.Append($"SELECT * FROM \"NewVPlusSales\".\"BeneficiaryPayment\"");

                   var agentInfo = _repositiory.RepositoryContext().Database
                       .SqlQuery<BeneficiaryPayment>(sql.ToString()).ToList();
                   return !agentInfo.Any() ? new List<BeneficiaryPayment>() : agentInfo;
               }

               else
               {
                   var sql2 = new StringBuilder();
                   sql2.Append($"SELECT * FROM \"NewVPlusSales\".\"BeneficiaryPayment\" WHERE \"Status\" = {searchObj.Status}");

                   var agentInfo = _repositiory.RepositoryContext().Database
                       .SqlQuery<BeneficiaryPayment>(sql2.ToString()).ToList();
                   return !agentInfo.Any() ? new List<BeneficiaryPayment>() : agentInfo;
                }
            }
           catch (Exception ex)
           {
               ErrorManager.LogApplicationError(ex.StackTrace,ex.Source,ex.Message);
                return  new List<BeneficiaryPayment>();
           }
       }

       private List<BeneficiaryPayment> GetBeneficiaryPayment(int beneficiaryId)
       {
           try
           {
               if (beneficiaryId > 0)
               {
                   var sql = new StringBuilder();
                   sql.Append($"SELECT * FROM \"NewVPlusSales\".\"BeneficiaryPayment\" WHERE \"BeneficiaryId\" = {beneficiaryId}");

                   var agentInfo = _repositiory.RepositoryContext().Database
                       .SqlQuery<BeneficiaryPayment>(sql.ToString()).ToList();
                   return !agentInfo.Any() ? new List<BeneficiaryPayment>() : agentInfo;
                }

               else
               {
                   var sql2 = new StringBuilder();
                   sql2.Append($"SELECT * FROM \"NewVPlusSales\".\"BeneficiaryPayment\"");

                   var agentInfo = _repositiory.RepositoryContext().Database
                       .SqlQuery<BeneficiaryPayment>(sql2.ToString()).ToList();
                   return !agentInfo.Any() ? new List<BeneficiaryPayment>() : agentInfo;
                }
           }
           catch (Exception ex)
           {
               ErrorManager.LogApplicationError(ex.StackTrace, ex.Source, ex.Message);
               return new List<BeneficiaryPayment>();
           }
       }

       private Beneficiary GetBeneficiaryInfo(long beneficiaryId)
       {
           try
           {
               var sql = new StringBuilder();
               sql.Append($"SELECT * FROM \"NewVPlusSales\".\"Beneficiary\" WHERE \"BeneficiaryId\" = {beneficiaryId}");

               var agentInfo = _repositiory.RepositoryContext().Database
                   .SqlQuery<Beneficiary>(sql.ToString()).ToList();
               if (!agentInfo.Any() || agentInfo.Count != 1)
               {
                   return null;
               }
               return agentInfo[0];
           }
           catch (Exception ex)
           {
              ErrorManager.LogApplicationError(ex.StackTrace,ex.Source,ex.Message);
               return null;
           }
       }
       private BeneficiaryAccountTransaction GetBeneficiaryAccTransactionInfo(long beneficiaryAccTransId)
       {
           try
           {
               var sql = new StringBuilder();
               sql.Append($"SELECT * FROM \"NewVPlusSales\".\"BeneficiaryAccountTransaction\" WHERE \"BeneficiaryAccountTransactionId\" = {beneficiaryAccTransId}");

               var agentInfo = _beneAccTransRepository.RepositoryContext().Database
                   .SqlQuery<BeneficiaryAccountTransaction>(sql.ToString()).ToList();
               if (!agentInfo.Any() || agentInfo.Count != 1)
               {
                   return null;
               }
               return agentInfo[0];
           }
           catch (Exception ex)
           {
               ErrorManager.LogApplicationError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
       }
       private BeneficiaryAccount GetBeneficiaryAccountInfo(long beneficiaryAccInfo)
       {
           try
           {
               var sql = new StringBuilder();
               sql.Append($"SELECT * FROM \"NewVPlusSales\".\"BeneficiaryAccount\" WHERE \"BeneficiaryAccountId\" = {beneficiaryAccInfo}");

               var agentInfo = _beneAccRepository.RepositoryContext().Database
                   .SqlQuery<BeneficiaryAccount>(sql.ToString()).ToList();
               if (!agentInfo.Any() || agentInfo.Count != 1)
               {
                   return null;
               }
               return agentInfo[0];
           }
           catch (Exception ex)
           {
               ErrorManager.LogApplicationError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
       }
       private BeneficiaryPayment GetBeneficiaryPaymentInfo(long BeneficiaryPayment)
       {
           try
           {
               var sql = new StringBuilder();
               sql.Append($"SELECT * FROM \"NewVPlusSales\".\"BeneficiaryPayment\" WHERE \"BeneficiaryPaymentId\" = {BeneficiaryPayment}");

               var agentInfo = _repositiory.RepositoryContext().Database
                   .SqlQuery<BeneficiaryPayment>(sql.ToString()).ToList();
               if (!agentInfo.Any() || agentInfo.Count != 1)
               {
                   return null;
               }
               return agentInfo[0];
           }
           catch (Exception ex)
           {
               ErrorManager.LogApplicationError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
       }
    }
}
