using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using NewVPlusSale.APIObjects.Common;
using NewVPlusSales.APIObjects.Settings;
using NewVPlusSales.Business.Core;
using NewVPlusSales.Business.DataManager;
using NewVPlusSales.Business.Infrastructure;
using NewVPlusSales.Business.Infrastructure.Contract;
using NewVPlusSales.BusinessObject.Settings;
using NewVPlusSales.Common;
using XPLUG.WEBTOOLS;

namespace NewVPlusSales.Business.Repository.Settings
{
   internal class CardCommissionRepository
   {
       private readonly INewVPlusSalesRepository<CardCommission> _repository;
       private readonly NewVPlusSalesUoWork _uoWork;

       CardCommissionRepository()
       {
           _repository= new NewVPlusSalesRepository<CardCommission>(_uoWork);
           _uoWork = new NewVPlusSalesUoWork();
       }

       public CardCommission GetCardCommission(int cardCommissionId)
       {
           try
           {
               return GetCardCommissions().Find(m => m.CardCommissionId == cardCommissionId) ?? new CardCommission();
           }
           catch (Exception ex)
           {
               ErrorManager.LogApplicationError(ex.StackTrace, ex.Source, ex.Message);
                return new CardCommission();
           }
          
       }

       internal void ResetCache()
       {
           try
           {
               HelperMethods.clearCache("ccCardCommissionList");
               GetCardCommissions();
           }
           catch (Exception)
           {
               // ignored
           }
       }

        public List<CardCommission> GetCardCommissions()
       {
           try
           {
               if (!(CacheManager.GetCache("ccCardCommission") is List<CardCommission> settings))
               {
                   var myListItem = _repository.GetAll().OrderBy(m => m.CardCommissionId).ToList();
                    if(myListItem.Any()) return new List<CardCommission>();
                   settings = myListItem.ToList();
                   if (settings.IsNullOrEmpty()) { return new List<CardCommission>();}

                   CacheManager.SetCache("ccCardCommission",settings,DateTime.Now.AddYears(1));
                   

               }
               return settings;
           }
           catch (Exception ex)
           {
              ErrorManager.LogApplicationError(ex.StackTrace,ex.Source,ex.Message);
                return new List<CardCommission>();
           }
       }

       public SettingsRegResponseObj AddCardCommission(RegCardCommissionObj regObj)
       {
           var response=new SettingsRegResponseObj
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
                   response.Status.Message.FriendlyMessage = "Error occure!Unable to proceed to your Request";
                   response.Status.Message.TechnicalMessage = "Registration Object is empty/Invaild";
                   return response;
               }

               if (regObj.LowerAmount == 0 || regObj.UpperAmount == 0)
               {
                   response.Status.Message.FriendlyMessage = "Error occure!Lower/Upper Amount cannot be zero";
                   response.Status.Message.TechnicalMessage = "Registration Object is empty/Invaild";
                   return response;
                }

               if (regObj.LowerAmount > regObj.UpperAmount)
               {
                   response.Status.Message.FriendlyMessage = "Error occure!LowerAmount cannot be greater than UpperAmount";
                   response.Status.Message.TechnicalMessage = "Registration Object is empty/Invaild";
                   return response;
                }

               var associatedCardType = GetCardTypeInfo(regObj.CardTypeId);

               if (associatedCardType == null)
               {
                   response.Status.Message.FriendlyMessage = "Specified Card Type Info not available";
                   response.Status.Message.TechnicalMessage = "Specified Card Type Info not available";
                   return response;
               }

                if (regObj.LowerAmount != associatedCardType.FaceValue)
               {
                   response.Status.Message.FriendlyMessage = "Error occure!LowerAmount must be equal to face value";
                   response.Status.Message.TechnicalMessage = "Registration Object is empty/Invaild";
                   return response;
                }

                #region Lower Modulo

                if (regObj.LowerAmount % 2 == 0)
                {
                    if (regObj.LowerAmount % associatedCardType.FaceValue != 0)
                    {
                        response.Status.Message.FriendlyMessage = $"Lower Amount must be a multiple of {associatedCardType.FaceValue}";
                        response.Status.Message.TechnicalMessage = $"Lower Amount must be a multiple of {associatedCardType.FaceValue}";
                        return response;
                    }
                }
                else
                {
                    var lowerAmountCheck = regObj.LowerAmount + 1;

                    if (lowerAmountCheck % associatedCardType.FaceValue != 0)
                    {
                        response.Status.Message.FriendlyMessage = $"Lower Amount must be a multiple of {associatedCardType.FaceValue}";
                        response.Status.Message.TechnicalMessage = $"Lower Amount must be a multiple of {associatedCardType.FaceValue}";
                        return response;
                    }
                }
                #endregion

                #region Upper Modulo
                if (regObj.UpperAmount % 2 == 0)
                {
                    if (regObj.UpperAmount % associatedCardType.FaceValue != 0)
                    {
                        response.Status.Message.FriendlyMessage = $"Upper Amount must be a multiple of {associatedCardType.FaceValue}";
                        response.Status.Message.TechnicalMessage = $"Upper Amount must be a multiple of {associatedCardType.FaceValue}";
                        return response;
                    }
                }
                else
                {
                    var upperAmountCheck = regObj.UpperAmount + 1;

                    if (upperAmountCheck % associatedCardType.FaceValue != 0)
                    {
                        response.Status.Message.FriendlyMessage = $"Upper Amount must be a multiple of {associatedCardType.FaceValue}";
                        response.Status.Message.TechnicalMessage = $"Upper Amount must be a multiple of {associatedCardType.FaceValue}";
                        return response;
                    }
                }
                #endregion


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

               if (IsCardCommissionDuplicate(regObj.LowerAmount, regObj.UpperAmount, regObj.CommissionRatee,
                   regObj.CardTypeId, 1, ref response))
               {
                   return response;
               }

               var cardCommission = new CardCommission
               {
                   CommissionRatee = regObj.CommissionRatee,
                   CardTypeId = regObj.CardTypeId,
                   LowerAmount = regObj.LowerAmount,
                   UpperAmount = regObj.UpperAmount,
                   Status = (Status)regObj.Status

               };

               var added = _repository.Add(cardCommission);
                _uoWork.SaveChanges();
               if (added.CardCommissionId < 1)
               {
                   response.Status.Message.FriendlyMessage = "Error Occurred! Unable to complete your request. Please try again later";
                   response.Status.Message.TechnicalMessage = "Unable to save to database";
                   return response;
                }

               ResetCache();
               response.Status.IsSuccessful = true;
               response.SettingId = added.CardCommissionId;
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

       public SettingsRegResponseObj UpdateCardCommission(EditCardCommissionObj regObj)
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
                   response.Status.Message.FriendlyMessage = "Error occure!Unable to proceed to your Request";
                   response.Status.Message.TechnicalMessage = "Registration Object is empty/Invaild";
                   return response;
               }

               if (regObj.LowerAmount == 0 || regObj.UpperAmount == 0)
               {
                   response.Status.Message.FriendlyMessage = "Error occure!Lower/Upper Amount cannot be zero";
                   response.Status.Message.TechnicalMessage = "Registration Object is empty/Invaild";
                   return response;
               }

               if (regObj.LowerAmount > regObj.UpperAmount)
               {
                   response.Status.Message.FriendlyMessage =
                       "Error occure!LowerAmount cannot be greater than UpperAmount";
                   response.Status.Message.TechnicalMessage = "Registration Object is empty/Invaild";
                   return response;
               }

               var associatedCardType = GetCardTypeInfo(regObj.CardTypeId);

               if (associatedCardType == null)
               {
                   response.Status.Message.FriendlyMessage = "Specified Card Type Info not available";
                   response.Status.Message.TechnicalMessage = "Specified Card Type Info not available";
                   return response;
               }

               if (regObj.LowerAmount != associatedCardType.FaceValue)
               {
                   response.Status.Message.FriendlyMessage = "Error occure!LowerAmount must be equal to face value";
                   response.Status.Message.TechnicalMessage = "Registration Object is empty/Invaild";
                   return response;
               }

               #region Lower Modulo

               if (regObj.LowerAmount % 2 == 0)
               {
                   if (regObj.LowerAmount % associatedCardType.FaceValue != 0)
                   {
                       response.Status.Message.FriendlyMessage =
                           $"Lower Amount must be a multiple of {associatedCardType.FaceValue}";
                       response.Status.Message.TechnicalMessage =
                           $"Lower Amount must be a multiple of {associatedCardType.FaceValue}";
                       return response;
                   }
               }
               else
               {
                   var lowerAmountCheck = regObj.LowerAmount + 1;

                   if (lowerAmountCheck % associatedCardType.FaceValue != 0)
                   {
                       response.Status.Message.FriendlyMessage =
                           $"Lower Amount must be a multiple of {associatedCardType.FaceValue}";
                       response.Status.Message.TechnicalMessage =
                           $"Lower Amount must be a multiple of {associatedCardType.FaceValue}";
                       return response;
                   }
               }

               #endregion

               #region Upper Modulo

               if (regObj.UpperAmount % 2 == 0)
               {
                   if (regObj.UpperAmount % associatedCardType.FaceValue != 0)
                   {
                       response.Status.Message.FriendlyMessage =
                           $"Upper Amount must be a multiple of {associatedCardType.FaceValue}";
                       response.Status.Message.TechnicalMessage =
                           $"Upper Amount must be a multiple of {associatedCardType.FaceValue}";
                       return response;
                   }
               }
               else
               {
                   var upperAmountCheck = regObj.UpperAmount + 1;

                   if (upperAmountCheck % associatedCardType.FaceValue != 0)
                   {
                       response.Status.Message.FriendlyMessage =
                           $"Upper Amount must be a multiple of {associatedCardType.FaceValue}";
                       response.Status.Message.TechnicalMessage =
                           $"Upper Amount must be a multiple of {associatedCardType.FaceValue}";
                       return response;
                   }
               }

               #endregion

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
                       errorDetail.AppendLine(
                           "Validation error occurred! Please check all supplied parameters and try again");
                   }
                   response.Status.Message.FriendlyMessage = errorDetail.ToString();
                   response.Status.Message.TechnicalMessage = errorDetail.ToString();
                   response.Status.IsSuccessful = false;
                   return response;
               }

               if (!HelperMethods.IsUserValid(regObj.AdminUserId, regObj.SysPathCode, HelperMethods.getRequesterRoles(),
                   ref response.Status.Message))
               {
                   return response;
               }
               var thisCardCommission = GetCardCommissionInfo(regObj.CardCommissionId);
               if (thisCardCommission == null)
               {
                   response.Status.Message.FriendlyMessage =
                       "No CardCommission Information found for the specified CardCommission Id";
                   response.Status.Message.TechnicalMessage = "No CardCommission Information found!";
                   return response;
               }
               if (IsCardCommissionDuplicate(regObj.LowerAmount, regObj.UpperAmount, regObj.CommissionRatee,
                   regObj.CardTypeId, 1, ref response))
               {
                   return response;
               }

               thisCardCommission.CardTypeId = regObj.CardTypeId;
               thisCardCommission.LowerAmount = regObj.LowerAmount;
               thisCardCommission.UpperAmount = regObj.UpperAmount;
               thisCardCommission.Status = (Status) regObj.Status;
               thisCardCommission.CommissionRatee = regObj.CommissionRatee;

               var added = _repository.Update(thisCardCommission);
               _uoWork.SaveChanges();
               if (added.CardCommissionId < 1)
               {
                   response.Status.Message.FriendlyMessage =
                       "Error Occurred! Unable to complete your request. Please try again later";
                   response.Status.Message.TechnicalMessage = "Unable to save to database";
                   return response;
               }

               ResetCache();
               response.Status.IsSuccessful = true;
               response.SettingId = added.CardCommissionId;


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

       public SettingsRegResponseObj DeleteCardCommission(DeleteCardCommission regObj)
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

               var thisCardCommission = GetCardCommissionInfo(regObj.CardCommissionId);
               if (thisCardCommission == null)
               {
                   response.Status.Message.FriendlyMessage = "No CardCommission Information found for the specified CardCommission Id";
                   response.Status.Message.TechnicalMessage = "No CardCommission Information found!";
                   return response;
                }
               
               thisCardCommission.Status = Status.Deleted;

               var added = _repository.Update(thisCardCommission);
                _uoWork.SaveChanges();
               if (added.CardCommissionId < 1)
               {
                   response.Status.Message.FriendlyMessage = "Error Occurred! Unable to complete your request. Please try again later";
                   response.Status.Message.TechnicalMessage = "Unable to save to database";
                   return response;
                }

               ResetCache();
               response.Status.IsSuccessful = true;
               response.SettingId = added.CardCommissionId;
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

       public CardCommissionRespObj LoadCardcommission(SettingSearchObj searchObj)
       {
           var response = new CardCommissionRespObj
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
                       errorDetail.AppendLine("Validation error occurred! Please check all supplied parameters and try again");
                   }
                   response.Status.Message.FriendlyMessage = errorDetail.ToString();
                   response.Status.Message.TechnicalMessage = errorDetail.ToString();
                   response.Status.IsSuccessful = false;
                   return response;
               }

               if (!HelperMethods.IsUserValid(searchObj.AdminUserId, searchObj.SysPathCode, new[] { "PortalAdmin", "CRMAdmin", "CRMManager" }, ref response.Status.Message))
               {
                   return response;
               }

               var thisCardCommission = GetCardCommissions();
               if (!thisCardCommission.Any())
               {
                   response.Status.Message.FriendlyMessage = "No Card Type Information found!";
                   response.Status.Message.TechnicalMessage = "No Card Type  Information found!";
                   return response;
                }

               if (searchObj.Status > 1)
               {
                   thisCardCommission = thisCardCommission.FindAll(m => m.Status == (Status) searchObj.Status).ToList();
               }
               var thisCardCommissionItem = new List<CardCommissionItemObj>();
               
                thisCardCommission.ForEachx(m =>
                {
                    thisCardCommissionItem.Add(new CardCommissionItemObj
                    {
                        CardCommissionId = m.CardCommissionId,
                        CommissionRate = m.CommissionRatee,
                        CardTypeId = m.CardTypeId,
                        CardTypeName = new CardCommissionRepository().GetCardTypeInfo(m.CardTypeId).Name,
                        LowerAmount = m.LowerAmount,
                        UpperAmount=m.UpperAmount,
                        Status = (int)m.Status,
                        Statuslabel = m.Status.ToString().Replace("_","")
                  
                        
                    });
                });

               response.Status.IsSuccessful = true;
               response.CardCommissions = thisCardCommissionItem;
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
           

       
       public CardType GetCardTypeInfo(int cardTypeId)
       {
           try
           {
               var sql1 =
                   $"Select * FROM \"NewVPlusSales\".\"CardType\" WHERE \"CardTypeId\"= {cardTypeId}";

               var agentInfos = _repository.RepositoryContext().Database.SqlQuery<CardType>(sql1).ToList();

               if (agentInfos.Any() || agentInfos.Count != 1)
               {
                   return null;
               }

               return agentInfos[0];
           }
           catch (Exception ex)
           {
               ErrorManager.LogApplicationError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
       }

       public bool IsCardCommissionDuplicate(decimal lowerAmount, decimal upperAmount, decimal commissionRate,
           int cardTypeId, int callType, ref SettingsRegResponseObj response)
       {
           try
           {
               var sql =
                   $"Select * FROM \"NewVPlusSales\".\"CardCommission\" WHERE \"CardTypeId\" = '{cardTypeId}' AND  \"Status\" != {-100})";
               var check = _repository.RepositoryContext().Database.SqlQuery<CardCommission>(sql).ToList();

               if (check.Count == 0)
               {
                   return false;
               }

               if (check.FindAll(m => m.LowerAmount <= lowerAmount && m.UpperAmount >= lowerAmount).Count > 0)
               {
                   if (callType != 2)
                   {
                       response.Status.Message.FriendlyMessage = "Duplicate Error Check Lower Amount! The selected range is already covered Range already exist";
                       response.Status.Message.TechnicalMessage = "Duplicate Error Check Lower Amount!The selected range is already covered Range already exist";
                       return true;
                   }
               }

               if (check.FindAll(amt => amt.LowerAmount <= upperAmount && amt.UpperAmount >= upperAmount).Count > 0)
               {
                   if (callType != 2)
                   {
                       response.Status.Message.FriendlyMessage = "Duplicate Error Check Upper Amount!The selected range is already covered";
                       response.Status.Message.TechnicalMessage = "Duplicate Error Check Upper Amount!The selected range is already covered";
                       return true;
                   }

               }

               if (check.FindAll(rate => rate.CommissionRatee == commissionRate).Count > 0)
               {
                   if (callType != 2)
                   {
                       response.Status.Message.FriendlyMessage = "Duplicate Error! Commission Rate already exist";
                       response.Status.Message.TechnicalMessage = "Duplicate Error! Commission Rate already exist";
                       return true;
                   }

               }
               return false;
            }
           catch (Exception ex)
           {
               response.Status.Message.FriendlyMessage =
                   "Unable to complete your request due to validation error. Please try again later";
               response.Status.Message.TechnicalMessage = "Duplicate Check Error: " + ex.Message;
               ErrorManager.LogApplicationError(ex.StackTrace, ex.Source, ex.Message);
               return true;
            }
           
        }

       private CardCommission GetCardCommissionInfo(int cardCommissionId)
       {
           try
           {

               var sql1 = $"SELECT *  FROM  \"NewVPlusSales\".\"CardCommission\" WHERE  \"CardCommissionId\" = {cardCommissionId};";

               var agentInfos = _repository.RepositoryContext().Database.SqlQuery<CardCommission>(sql1).ToList();
               if (!agentInfos.Any() || agentInfos.Count != 1)
               {
                   return null;
               }
               return agentInfos[0];

           }
           catch (Exception ex)
           {
               ErrorManager.LogApplicationError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
       }
    }

}
