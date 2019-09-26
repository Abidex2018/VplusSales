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
using NewVPlusSales.Common;
using XPLUG.WEBTOOLS;

namespace NewVPlusSales.Business.Repository.Settings
{
    internal class CardTypeRepository
    {
        private readonly INewVPlusSalesRepository<CardType> _repository;
        private readonly NewVPlusSalesUoWork _uoWork;

        public CardTypeRepository()
        {
            _uoWork = new NewVPlusSalesUoWork();
            _repository = new NewVPlusSalesRepository<CardType>(_uoWork);
        }

        public CardType GetCardType(int cardTypeId)
        {
            try
            {
                return GetCardTypes().Find(m => m.CardTypeId == cardTypeId) ?? new CardType();
            }
            catch (Exception ex)
            {
                ErrorManager.LogApplicationError(ex.StackTrace, ex.Source, ex.Message);
                return new CardType();
            }
        }

        public List<CardType> GetCardTypes()
        {
            try
            {
                if (!(CacheManager.GetCache("ccCardTypeList") is List<CardType> settings) || settings.IsNullOrEmpty())
                {
                    var myItemList = _repository.GetAll().OrderBy(m => m.CardTypeId);
                    if(!myItemList.Any()) return new List<CardType>();
                    settings = myItemList.ToList();
                    if (!settings.Any()) { return  new List<CardType>();}
                    CacheManager.SetCache("ccCardTypeList",settings,DateTime.Now.AddYears(1));
                    
                }
                return settings;
            }
            catch (Exception ex)
            {
                ErrorManager.LogApplicationError(ex.StackTrace, ex.Source, ex.Message);
                return new List<CardType>();
            }
        }

        internal void ResetCache()
        {
            try
            {
                HelperMethods.clearCache("ccCardTypeList");
                GetCardTypes();
            }
            catch (Exception)
            {
                //Ignore
            }
        }

        public SettingsRegResponseObj AddCardType(RegCardTypeObj regObj)
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

                if (regObj.FaceValue == 0)
                {
                    response.Status.Message.FriendlyMessage = "Error Occurred! Face Value cannot be Zero";
                    response.Status.Message.TechnicalMessage = "Registration Object is empty / invalid";
                    return response;
                }

                if (!EntityValidatorHelper.Validate(regObj, out var valResult))
                {
                    var errorDetails = new StringBuilder();
                    if (!valResult.IsNullOrEmpty())
                    {
                        errorDetails.Append("The following error occured:");
                        valResult.ForEachx(m=> errorDetails.Append(m.ErrorMessage));
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

                if (IsCardTypeDuplicate(regObj.Name, 1, ref response))
                {
                    return response;
                }

                var card = new CardType
                {
                    Name = regObj.Name,
                    FaceValue = regObj.FaceValue,
                    Status = (Status)regObj.Status
                };

               var added= _repository.Add(card);
                _uoWork.SaveChanges();
                if (added.CardTypeId < 1)
                {
                    response.Status.Message.FriendlyMessage = "Error Occurred! Unable to complete your request. Please try again later";
                    response.Status.Message.TechnicalMessage = "Unable to save to database";
                    return response;
                }

                ResetCache();
                response.Status.IsSuccessful = true;
                response.SettingId = added.CardTypeId;

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

        public SettingsRegResponseObj UpdateCardType(EditCardTypeObj regObj)
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

                if (regObj.FaceValue == 0)
                {
                    response.Status.Message.FriendlyMessage = "Error Occurred! Face Value cannot be Zero";
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

                if (!HelperMethods.IsUserValid(regObj.AdminUserId, regObj.SysPathCode,
                    HelperMethods.getRequesterRoles(), ref response.Status.Message))
                {
                    return response;
                }

                var thisCardType = GetCardTypeInfo(regObj.CardTypeId);
                if (thisCardType == null)
                {
                    response.Status.Message.FriendlyMessage =
                        "No CardType Information found for the specified CardType Id";
                    response.Status.Message.TechnicalMessage = "No CardType Information found!";
                    return response;
                }

                if (IsCardTypeDuplicate(regObj.Name, 1, ref response))
                {
                    return response;
                }

                thisCardType.Name = regObj.Name;
                thisCardType.FaceValue = regObj.FaceValue;
                thisCardType.Status = (Status)regObj.Status;

                var added = _repository.Update(thisCardType);
                _uoWork.SaveChanges();
                if (added.CardTypeId < 1)
                {
                    response.Status.Message.FriendlyMessage = "Error Occurred! Unable to complete your request. Please try again later";
                    response.Status.Message.TechnicalMessage = "Unable to save to database";
                    return response;
                }
                ResetCache();
                response.Status.IsSuccessful = true;
                response.SettingId = added.CardTypeId;

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

        public SettingsRegResponseObj DeleteCardType(DeleteCardtypeObj regObj)
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

                if (!HelperMethods.IsUserValid(regObj.AdminUserId, regObj.SysPathCode, new[] { "PortalAdmin", "CRMAdmin", "CRMManager" }, ref response.Status.Message))
                {
                    return response;
                }
                var thisCardType = GetCardTypeInfo(regObj.CardTypeId);
                if (thisCardType == null)
                {
                    response.Status.Message.FriendlyMessage =
                        "No CardType Information found for the specified CardType Id";
                    response.Status.Message.TechnicalMessage = "No CardType Information found!";
                    return response;
                }

                thisCardType.Name = thisCardType.Name + "_Deleted_" + DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss");
                thisCardType.Status = Status.Deleted;

                var added = _repository.Update(thisCardType);
                _uoWork.SaveChanges();
                if (added.CardTypeId < 1)
                {
                    response.Status.Message.FriendlyMessage = "Error Occurred! Unable to complete your request. Please try again later";
                    response.Status.Message.TechnicalMessage = "Unable to save to database";
                    return response;
                }
                ResetCache();
                response.Status.IsSuccessful = true;
                response.SettingId = added.CardTypeId;

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

        public CardTypeRespObj LoadCardType(SettingSearchObj searchObj)
        {
            var response = new CardTypeRespObj
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


                if (!EntityValidatorHelper.Validate(searchObj, out var valResult))
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

                if (!HelperMethods.IsUserValid(searchObj.AdminUserId, searchObj.SysPathCode,
                   HelperMethods.getRequesterRoles(), ref response.Status.Message))
                {
                    return response;
                }

                var thisCardTypes = GetCardTypes();
                if (!thisCardTypes.Any())
                {
                    response.Status.Message.FriendlyMessage = "No Card Type Information found!";
                    response.Status.Message.TechnicalMessage = "No Card Type  Information found!";
                    return response;
                }

                if (searchObj.Status > -1)
                {
                    thisCardTypes = thisCardTypes.FindAll(m => m.Status == (Status) searchObj.Status);
                }

                var cardTypeItem= new List<CardTypeItemObj>();

                thisCardTypes.ForEachx(m =>
                {
                    cardTypeItem.Add( new CardTypeItemObj
                    {
                        Name = m.Name,
                        FaceValue = m.FaceValue,
                        CardStatus = (int)m.Status,
                        CardStatusLabel = m.Status.ToString().Replace("_","")
                    });
                });

                response.Status.IsSuccessful = true;
                response.CardTypes = cardTypeItem;
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

        public bool IsCardTypeDuplicate(string name, int callType, ref SettingsRegResponseObj response)
        {
            try
            {
                var sql =
                    $"Select coalesce(Count(\"CardTypeId\"),0) FROM \"NewVPlusSales\".\"CardType\" WHERE lower(\"Name\")= lower('{name.Replace("'", "''")}')";
                var check = _repository.RepositoryContext().Database.SqlQuery<long>(sql).ToList();

                if (check.IsNullOrEmpty())
                {
                    response.Status.Message.FriendlyMessage =
                        "Unable to complete your request due to validation error. Please try again later";
                    response.Status.Message.TechnicalMessage = "Unable to chech for duplicate";
                    return true;
                }

                if (check.Count != 1)
                {
                    response.Status.Message.FriendlyMessage =
                        "Unable to complete your request due to validation error. Please try again later";
                    response.Status.Message.TechnicalMessage = "Unable to chech for duplicate";
                    return true;
                }

                if (check[0] < 1)
                {
                    return false;
                }

                if (check[0] > 0)
                {
                    if (callType != 2 || check[0] > 1)
                    {
                        response.Status.Message.FriendlyMessage = "Duplicate Error! Card Type name already exist ";
                        response.Status.Message.TechnicalMessage = "Duplicate Error! Card Type name already exist";
                        return true;
                    }
                }
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
    }
}
