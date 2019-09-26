using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
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

namespace NewVPlusSales.Business.Repository.Settings
{
   internal class BeneficiaryRepository
   {
       private readonly INewVPlusSalesRepository<Beneficiary> _repository;
       private readonly INewVPlusSalesRepository<BeneficiaryAccount> _accountRepository;
       private readonly NewVPlusSalesUoWork _uoWork;

       public BeneficiaryRepository()
       {
           _repository = new NewVPlusSalesRepository<Beneficiary>(_uoWork);
            _accountRepository= new NewVPlusSalesRepository<BeneficiaryAccount>(_uoWork);
           _uoWork = new NewVPlusSalesUoWork();
       }

       public Beneficiary GetBeneficiary(int beneficiaryId)
       {
           try
           {
               return GetBeneficiaries().Find(m => m.BeneficiaryId == beneficiaryId) ?? new Beneficiary();

            }
            catch (Exception ex)
           {
               ErrorManager.LogApplicationError(ex.StackTrace, ex.Source, ex.Message);
                return  new Beneficiary();
            }
       }
       public List<Beneficiary> GetBeneficiaries()
       {
           try
           {
               if (!(CacheManager.GetCache("ccBeneficiaryList") is List<Beneficiary> settings ) || settings.IsNullOrEmpty())
               {
                   var myListItem = _repository.GetAll().OrderBy(m => m.BeneficiaryId).ToList();
                    if(!myListItem.Any()) return  new List<Beneficiary>();
                   settings = myListItem.ToList();
                    if (!settings.IsNullOrEmpty()) { return new List<Beneficiary>();}

                    CacheManager.SetCache("ccBeneficiary",settings,DateTime.Now.AddYears(1));
               }
               return settings;
           }
           catch (Exception ex)
           {
               ErrorManager.LogApplicationError(ex.StackTrace, ex.Source, ex.Message);
               return new List<Beneficiary>();
           }
       }

       public void Reset()
       {
           try
           {
                HelperMethods.clearCache("ccBeneficiary");
               GetBeneficiaries();

           }
           catch (Exception)
           {
               //Ignore
           }
       }


       public SettingsRegResponseObj AddBeneficiary(RegBeneficiaryObj regObj)
       {
           var response = new SettingsRegResponseObj
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

                if (!EntityValidatorHelper.Validate(regObj, out var valResult))
                {
                    var errorDetails = new StringBuilder();
                    if (!valResult.IsNullOrEmpty())
                    {
                        errorDetails.Append("The following error occured:");
                        valResult.ForEachx(m => errorDetails.Append(m.ErrorMessage));
                    }
                    else
                    {
                        errorDetails.Append(
                            "Validation Error Occured! Please check all supplied parameter and try again");
                    }

                    response.Status.Message.FriendlyMessage = errorDetails.ToString();
                    response.Status.Message.FriendlyMessage = errorDetails.ToString();
                    response.Status.IsSuccessful = false;
                    return response;
                }
                if (!HelperMethods.IsUserValid(regObj.AdminUserId, regObj.SysPathCode, HelperMethods.getRequesterRoles(), ref response.Status.Message))
                {
                    return response;
                }

                //check for email, fullname and mobile number if there is duplicate
                if (IsBeneficiaryDuplicate(regObj.FulllName, regObj.Email, regObj.MobileNumber, 1, ref response))
                {
                    return response;

                }
                using (var db = _uoWork.BeginTransaction())
                {
                    var BeneficiaryAccount = new BeneficiaryAccount
                    {
                        AvailableBalance = 0,
                        CreditLimit = 0,
                        LastTransactionAmount = 0,
                        LastTransactionType = TransactionType.Unknown,
                        LastTransactionTimeStamp = DateMap.CurrentTimeStamp(),
                        Status = Status.Inactive,
                        LastTransactionId = 0
                    };

                    var accoadded = _accountRepository.Add(BeneficiaryAccount);
                    _uoWork.SaveChanges();
                    if (accoadded.BeneficiaryAccountId < 1)
                    {
                        db.Rollback();
                        response.Status.Message.FriendlyMessage = "Error Occurred! Unable to complete your request. Please try again later";
                        response.Status.Message.TechnicalMessage = "Unable to save to database";
                        return response;
                    }
                    var Item = new Beneficiary
                    {
                        BeneficiaryAccountId = accoadded.BeneficiaryAccountId,
                        FullName = regObj.FulllName,
                        Address = regObj.Address,
                        Email = regObj.Email,
                        ApprovalComment = "",
                        ApprovedBy = 0,
                        BeneficiaryType = (BeneficiaryType)regObj.BeneficiaryType,
                        Status = Status.Inactive,
                        TimeStampApproved = "",
                        TimeStampRegisered = DateMap.CurrentTimeStamp(),


                    };

                    var addBeneficiary = _repository.Add(Item);
                    _uoWork.SaveChanges();
                    if (addBeneficiary.BeneficiaryId < 1)
                    {
                        db.Rollback();
                        response.Status.Message.FriendlyMessage = "Error Occurred! Unable to complete your request. Please try again later";
                        response.Status.Message.TechnicalMessage = "Unable to save to database";
                        return response;
                    }
                    db.Commit();

                    Reset();
                    response.Status.IsSuccessful = true;
                    response.SettingId = addBeneficiary.BeneficiaryId;
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

       public SettingsRegResponseObj UpdateBeneficiary(EditBeneficiaryObj regObj)
       {
           var response = new SettingsRegResponseObj
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

               if (!EntityValidatorHelper.Validate(regObj, out var valResult))
               {
                   var errorDetails = new StringBuilder();
                   if (!valResult.IsNullOrEmpty())
                   {
                       errorDetails.Append("The following error occured:");
                       valResult.ForEachx(m => errorDetails.Append(m.ErrorMessage));
                   }
                   else
                   {
                       errorDetails.Append(
                           "Validation Error Occured! Please check all supplied parameter and try again");
                   }

                   response.Status.Message.FriendlyMessage = errorDetails.ToString();
                   response.Status.Message.FriendlyMessage = errorDetails.ToString();
                   response.Status.IsSuccessful = false;
                   return response;
               }
               if (!HelperMethods.IsUserValid(regObj.AdminUserId, regObj.SysPathCode, HelperMethods.getRequesterRoles(), ref response.Status.Message))
               {
                   return response;
               }

               if (IsBeneficiaryDuplicate(regObj.FullName, regObj.Email, regObj.MobileNumber, 1, ref response))
               {
                   return response;

               }

               var thisBeneficiary = GetBeneficiaryInfo(regObj.BeneficiaryId);
               if (thisBeneficiary == null)
               {
                   response.Status.Message.FriendlyMessage = "No Beneficiary Information found for the specified Beneficiary Id";
                   response.Status.Message.TechnicalMessage = "No Beneficiary Information found!";
                   return response;
                }
               thisBeneficiary.FullName = !string.IsNullOrWhiteSpace(regObj.FullName) ? regObj.FullName : thisBeneficiary.FullName;
               thisBeneficiary.BeneficiaryType = regObj.BeneficiaryType > 0 ? (BeneficiaryType)regObj.BeneficiaryType : thisBeneficiary.BeneficiaryType;
               thisBeneficiary.Address = !string.IsNullOrWhiteSpace(regObj.Address) ? regObj.Address : thisBeneficiary.Address;
               thisBeneficiary.Email = !string.IsNullOrWhiteSpace(regObj.Email) ? regObj.Email : thisBeneficiary.Email;
               thisBeneficiary.MobileNumber = !string.IsNullOrWhiteSpace(regObj.MobileNumber) ? regObj.MobileNumber : thisBeneficiary.MobileNumber;

               var added = _repository.Add(thisBeneficiary);
                _uoWork.SaveChanges();
               if (added.BeneficiaryId < 1)
               {
                 
                   response.Status.Message.FriendlyMessage = "Error Occurred! Unable to complete your request. Please try again later";
                   response.Status.Message.TechnicalMessage = "Unable to save to database";
                   return response;
                }

               Reset();
               response.Status.IsSuccessful = true;
               response.SettingId = added.BeneficiaryId;
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

       public SettingsRegResponseObj DeleteBeneficiary(DeleteBeneficiaryObj regObj)
       {
           var response = new SettingsRegResponseObj
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

               var thisBeneficiary = GetBeneficiaryInfo(regObj.BeneficiaryId);

               if (thisBeneficiary == null)
               {
                   response.Status.Message.FriendlyMessage = "No Beneficiary Information found for the specified Beneficiary Id";
                   response.Status.Message.TechnicalMessage = "No Beneficiary Information found!";
                   return response;
                }

               thisBeneficiary.FullName = thisBeneficiary.FullName + "_Deleted_" +
                                          DateTime.Now.ToString("\"yyyy_MM_dd_hh_mm_ss\"");
               thisBeneficiary.Status = Status.Deleted;

               var updateItem = _repository.Update(thisBeneficiary);
                    _uoWork.SaveChanges();
               if (updateItem.BeneficiaryId < 1)
               {
                   response.Status.Message.FriendlyMessage = "Error Occurred! Unable to complete your request. Please try again later";
                   response.Status.Message.TechnicalMessage = "Unable to save to database";
                   return response;
                }

               Reset();
               response.Status.IsSuccessful = true;
               response.SettingId = updateItem.BeneficiaryId;
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

       public BeneficiaryRespObj LoadBeneficiary(SettingSearchObj searchObj)
       {
           var response = new BeneficiaryRespObj()
           {
               Status = new APIResponseStatus
               {
                   IsSuccessful = false,
                   Message = new APIResponseMessage()
               }
           };

           try
           {
               if (searchObj.Equals(null))
               {
                   response.Status.Message.FriendlyMessage = "Error Occurred! Unable to proceed with your request";
                   response.Status.Message.TechnicalMessage = "Registration Object is empty / invalid";
                   return response;
               }

               if (!EntityValidatorHelper.Validate(searchObj, out var valResults))
               {
                   var errorDetail = new StringBuilder();
                   if (!valResults.IsNullOrEmpty())
                   {
                       errorDetail.AppendLine("Following error occurred:");
                       valResults.ForEachx(m => errorDetail.AppendLine(m.ErrorMessage));
                   }
                   else
                   {
                       errorDetail.AppendLine(
                           "Validation error occurred! Please check all supplied parameters and try again");
                   }
                   response.Status.Message.FriendlyMessage = errorDetail.ToString();
                   response.Status.Message.TechnicalMessage = errorDetail.ToString();
                   response.Status.IsSuccessful = false;
                   return response;
               }

               if (!HelperMethods.IsUserValid(searchObj.AdminUserId, searchObj.SysPathCode,
                   HelperMethods.getRequesterRoles(), ref response.Status.Message))
               {
                   return response;
               }
               var thisBeneficiaries = GetBeneficiaries();
               if (!thisBeneficiaries.Any())
               {
                   response.Status.Message.FriendlyMessage = "No Beneficiary Information found!";
                   response.Status.Message.TechnicalMessage = "No Beneficiary  Information found!";
                   return response;
               }
               if (searchObj.Status > -1)
               {
                   thisBeneficiaries = GetBeneficiaries().FindAll(p => p.Status == (Status) searchObj.Status).ToList();
               }
               var BeneficiaryItem = new List<BeneficiaryRespItem>();

               foreach (var m in thisBeneficiaries)
               {
                   var associatedAccount = GetBeneficiaryAccountInfo(m.BeneficiaryAccountId);

                   if (associatedAccount == null)
                   {
                       response.Status.Message.FriendlyMessage = "One or More Beneficiary Account Not Found!";
                       response.Status.Message.TechnicalMessage = "One or More Beneficiary Account Not Found!!";
                       return response;
                   }

                   var AccBeneficiary = new BeneficiaryAccountObj
                   {
                       BeneficiaryAccountId = associatedAccount.BeneficiaryAccountId,
                       AvaliableBalance = associatedAccount.AvailableBalance,
                       CreditLimit = associatedAccount.CreditLimit,
                       LastTransactionType = (int) associatedAccount.LastTransactionType,
                       LastTransactionAmount = associatedAccount.LastTransactionAmount,
                       LastTransactionTypeLabel = associatedAccount.LastTransactionType.ToString().Replace("_", ""),

                   };

                   BeneficiaryItem.Add(new BeneficiaryRespItem
                   {
                       BeneficiaryId = m.BeneficiaryId,
                       FullName = m.FullName,
                       MobileNumber = m.MobileNumber,
                       Email = m.Email,
                       Address = m.Address,
                       BeneficiaryTypeId = (int) m.BeneficiaryType,
                       BeneficiaryTypeLabel = m.BeneficiaryType.ToString().Replace("_", " "),
                       Status = (int) m.Status,
                       StatusLabel = m.Status.ToString().Replace("_", ""),
                       ApprovedBy = m.ApprovedBy,
                       ApprovedComment = m.ApprovalComment,
                       TimeStampApproved = m.TimeStampApproved,
                       TimeStampRegistered = m.TimeStampRegisered,
                       BeneficiaryAccount = AccBeneficiary

                   });


               }
               response.Status.IsSuccessful = true;
               response.Beneficiaries = BeneficiaryItem;
               return response;

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

       }

       public SettingsRegResponseObj ApproveBeneficiary(ApproveBeneficairyObj regObj)
       {
           var response = new SettingsRegResponseObj
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

               if (!EntityValidatorHelper.Validate(regObj, out var valResult))
               {
                   var errorDetails = new StringBuilder();
                   if (!valResult.IsNullOrEmpty())
                   {
                       errorDetails.Append("The following error occured:");
                       valResult.ForEachx(m => errorDetails.Append(m.ErrorMessage));
                   }
                   else
                   {
                       errorDetails.Append(
                           "Validation Error Occured! Please check all supplied parameter and try again");
                   }

                   response.Status.Message.FriendlyMessage = errorDetails.ToString();
                   response.Status.Message.FriendlyMessage = errorDetails.ToString();
                   response.Status.IsSuccessful = false;
                   return response;
               }
               if (!HelperMethods.IsUserValid(regObj.AdminUserId, regObj.SysPathCode, HelperMethods.getRequesterRoles(), ref response.Status.Message))
               {
                   return response;
               }

               var thisBeneficiary = GetBeneficiaryInfo(regObj.BeneficiaryId);
               if (thisBeneficiary == null)
               {
                   response.Status.Message.FriendlyMessage = "No Beneficiary Information found!";
                   response.Status.Message.TechnicalMessage = "No Beneficiary  Information found!";
                   return response;
                }

               if (thisBeneficiary.Status == Status.Inactive)
               {
                   response.Status.Message.FriendlyMessage = "Sorry This Beneficiary Is Not Valid For Approval! Please Try Again Later";
                   response.Status.Message.TechnicalMessage = " Beneficiary Status is either already Active!";
                   return response;
                }

               thisBeneficiary.ApprovedBy = regObj.AdminUserId;
               thisBeneficiary.ApprovalComment = regObj.ApprovalComment;
               thisBeneficiary.TimeStampApproved = DateMap.CurrentTimeStamp();
               thisBeneficiary.Status = Status.Active;

               var ApprovaBeneficairy = _repository.Update(thisBeneficiary);
                _uoWork.SaveChanges();
               if (ApprovaBeneficairy.BeneficiaryId < 1)
               {
                   response.Status.Message.FriendlyMessage = "Error Occurred! Unable to complete your request. Please try again later";
                   response.Status.Message.TechnicalMessage = "Unable to save to database";
                   return response;
                }

               Reset();
               response.Status.IsSuccessful = true;
               response.SettingId = ApprovaBeneficairy.BeneficiaryId;
               response.Status.Message.FriendlyMessage = "Approved was Successful";
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

       private bool IsBeneficiaryDuplicate(string fullName,string email,string mobileNumber, int callType, ref SettingsRegResponseObj response)
        {
            try
            {
                #region FullName

                var sql1 =
                    $"Select * FROM \"NewVPlusSales\".\"Beneficiary\" WHERE \"FullName\" ~ '{fullName}')";
                var check = _repository.RepositoryContext().Database.SqlQuery<Beneficiary>(sql1).ToList();

                if (check.Count > 0)
                {
                    if (callType != 2)
                    {
                        response.Status.Message.FriendlyMessage = "Duplicate Error! Beneficiary full name already exist";
                        response.Status.Message.TechnicalMessage = "Duplicate Error! Beneficiary full name already exist";
                        return true;
                    }
                }

                #endregion

                #region Email

                var sql2 =
                    $"Select * FROM \"NewVPlusSales\".\"Beneficiary\" WHERE \"Email\" ~ '{email}')";
                var check2 = _repository.RepositoryContext().Database.SqlQuery<Beneficiary>(sql2).ToList();

                if (check2.Count > 0)
                {
                    if (callType != 2)
                    {
                        response.Status.Message.FriendlyMessage = "Duplicate Error! Beneficiary full name already exist";
                        response.Status.Message.TechnicalMessage = "Duplicate Error! Beneficiary full name already exist";
                        return true;
                    }
                }


                #endregion

                #region Mobile Number

                var sql3 =
                    $"Select * FROM \"NewVPlusSales\".\"Beneficiary\" WHERE \"MobileNumber\" ~ '{mobileNumber}')";
                var check3 = _repository.RepositoryContext().Database.SqlQuery<Beneficiary>(sql3).ToList();

                if (check3.Count > 0)
                {
                    if (callType != 2)
                    {
                        response.Status.Message.FriendlyMessage = "Duplicate Error! Beneficiary full name already exist";
                        response.Status.Message.TechnicalMessage = "Duplicate Error! Beneficiary full name already exist";
                        return true;
                    }
                }

                #endregion


                return false;
            }
            catch (Exception ex)
            {
                response.Status.Message.FriendlyMessage =
                    "Unable to complete your request due to Validation Error:Please try  again later";
                response.Status.Message.TechnicalMessage = "Duplicate check error:" + ex.Message;
                ErrorManager.LogApplicationError(ex.StackTrace, ex.Source, ex.Message);
                return true;
            }
        }

       private Beneficiary GetBeneficiaryInfo(long beneficiaryId)
       {
           try
           {
               var sql =
                   $"Select * FROM \"NewVPlusSales\".\"Beneficiary\" WHERE \"BeneficiaryId\" = '{beneficiaryId}')";

               var agentInfo = _repository.RepositoryContext().Database.SqlQuery<Beneficiary>(sql).ToList();

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

       private BeneficiaryAccount GetBeneficiaryAccountInfo(int beneficiaryAccountId)
       {
           try
           {
               var sql =
                   $"Select * FROM \"NewVPlusSales\".\"BeneficiaryAccount\" WHERE \"BeneficiaryAccountIdId\" = '{beneficiaryAccountId}')";

               var agentInfo = _repository.RepositoryContext().Database.SqlQuery<BeneficiaryAccount>(sql).ToList();

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
