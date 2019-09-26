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
using NewVPlusSales.BusinessObject.CardProduction;
using NewVPlusSales.BusinessObject.Settings;
using NewVPlusSales.Common;
using XPLUG.WEBTOOLS;

namespace NewVPlusSales.Business.Repository.CardProduction
{
    internal class CardRepository
    {
        private readonly INewVPlusSalesRepository<Card> _repository;
        private readonly INewVPlusSalesRepository<CardDelivery> _cardDeliveryRepository;
        private readonly INewVPlusSalesRepository<CardItem> _cardItemRepository;
        private readonly NewVPlusSalesUoWork _uoWork;

        public CardRepository()
        {
            _repository = new NewVPlusSalesRepository<Card>(_uoWork);
            _cardItemRepository = new NewVPlusSalesRepository<CardItem>(_uoWork);
            _cardDeliveryRepository = new NewVPlusSalesRepository<CardDelivery>(_uoWork);
            _uoWork = new NewVPlusSalesUoWork();
        }
        public Card GetCard(int cardId)
        {
            try
            {
                return GetCards().Find(m => m.CardId == cardId) ?? new Card();
            }
            catch (Exception ex)
            {
                ErrorManager.LogApplicationError(ex.StackTrace, ex.Source, ex.Message);
                return new Card();
            }
        }

        public List<Card> GetCards()
        {
            try
            {
                if (!(CacheManager.GetCache("ccCardList") is List<Card> settings) || settings.IsNullOrEmpty())
                {
                    var myItemList = _repository.GetAll().OrderBy(m => m.CardTypeId);
                    if (!myItemList.Any()) return new List<Card>();
                    settings = myItemList.ToList();
                    if (!settings.Any()) { return new List<Card>(); }
                    CacheManager.SetCache("ccCardList", settings, DateTime.Now.AddYears(1));

                }
                return settings;
            }
            catch (Exception ex)
            {
                ErrorManager.LogApplicationError(ex.StackTrace, ex.Source, ex.Message);
                return new List<Card>();
            }
        }


        public void Reset()
        {
            try
            {
                HelperMethods.clearCache("ccCardList");
                GetCards();
            }
            catch (Exception)
            {
               //Ignore
            }
            
        }

        public CardRegRespObj AddCard(RegCardObj regObj)
        {
            var response= new CardRegRespObj
            {
                Status = new APIResponseStatus
                {
                    IsSuccessful = false,
                    Message = new APIResponseMessage()
                }
            };

            try
            {
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

                if (!DataCheck.IsNumeric(regObj.BatchKey))
                {
                    response.Status.Message.FriendlyMessage = "Error Occurred! Batch Key Invalid";
                    response.Status.Message.TechnicalMessage = "Batch Prefix Number Must be greater than 0";
                    return response;
                }
                if (!DataCheck.IsNumeric(regObj.StartBatchId))
                {
                    response.Status.Message.FriendlyMessage = "Error Occurred! Invalid Start Batch Id";
                    response.Status.Message.TechnicalMessage = "Start Batch Id Is not numeric";
                    return response;
                }
                if (!DataCheck.IsNumeric(regObj.StopBatchId))
                {
                    response.Status.Message.FriendlyMessage = "Error Occurred! Invalid Stop Batch Id";
                    response.Status.Message.TechnicalMessage = "Stop Batch Id Is not numeric";
                    return response;
                }

                if ((int.Parse(regObj.StopBatchId) - int.Parse(regObj.StartBatchId)) + 1 != regObj.NumberOfBatches)
                {
                    response.Status.Message.FriendlyMessage = "Error Occurred! Incorrect StopBatchId/StartBatchId/NumberOfBatches";
                    response.Status.Message.TechnicalMessage = "Incorrect StopBatchId/StartBatchId/NumberOfBatches";
                    return response;
                }
                if (regObj.QuantityPerBatch < 1)
                {
                    response.Status.Message.FriendlyMessage = "Error Occurred! Quantity Per Batch Is Required";
                    response.Status.Message.TechnicalMessage = "Error Occurred! Quantity Per Batch must be greater than zero!";
                    return response;
                }
                //..................Continue here
                #region Batch Id Computation
                //var qtyPerBatchLength = regObj.QuantityPerBatch.
                #endregion

                //store date for Concurrency...
                var nowDateTime = DateMap.CurrentTimeStamp();
                var nowDate = nowDateTime.Substring(0, nowDateTime.IndexOf(' '));
                var nowTime = nowDateTime.Substring(nowDateTime.IndexOf('-') + 1);

                var cardItemList = new List<CardItem>();

                for (int i =int.Parse(regObj.StartBatchId); i < int.Parse(regObj.StopBatchId +1); i++)
                {
                    cardItemList.Add(new CardItem
                    {
                        CardTypeId = regObj.CardTypeId,
                        BatchId = i.ToString(),
                        StartBatchNumber = i.ToString() + "" + "000", //77001 000
                        StopBatchNumber = i.ToString() + "" + "999", //77001999
                        DefectiveBatchNumber = "",
                        AvailableQuantity = 0,
                        BatchQuantity = 1000,
                        DeliveredQuantity = 0,
                        MissingQuantity = 0,
                        DefectiveQuantity = 0,
                        IssuedQuantity = 0,
                        RegisteredBy = regObj.AdminUserId,
                        TimeStampRegisered = nowDateTime,
                        TimeStampDelivered = "",
                        TimeStampLastIssued = "",
                        Status = CardStatus.Registered
                    });
                }

                var card= new Card
                {

                    CardTitle = $"Card Production On {nowDate} At {nowTime}",
                    CardTypeId = regObj.CardTypeId,
                    BatchKey = regObj.BatchKey,
                    StartBatchId = regObj.BatchKey + "000",
                    StopBatchId = (Int32.Parse(regObj.BatchKey + "000") + (regObj.NumberOfBatches - 1)).ToString(),
                    NumberOfBatches = regObj.NumberOfBatches,
                    QuantityPerBatch = 1000,
                    TotalQuantity = regObj.NumberOfBatches * regObj.QuantityPerBatch,
                    Status = CardStatus.Registered,
                    TimeStampRegisered = nowDateTime,
                    CardItems = cardItemList
                };

                var added = _repository.Add(card);
                _uoWork.SaveChanges();
                if (added.CardId < 1)
                {
                    response.Status.Message.FriendlyMessage = "Error Occurred! Unable to complete your request. Please try again later";
                    response.Status.Message.TechnicalMessage = "Unable to save to database";
                    return response;
                }

                response.Status.IsSuccessful = true;
                response.CardId = added.CardId;
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

        public CardRegRespObj UpdateCard(EditCardObj regObj)
        {
            var response= new CardRegRespObj
            {
                Status = new APIResponseStatus
                {
                    IsSuccessful = false,
                    Message = new APIResponseMessage()
                }
            };

            try
            {
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

                var thisCardItem = GetCardItemInfo(regObj.CardItemId);
                if (thisCardItem == null)
                {
                    response.Status.Message.FriendlyMessage = "No Card Item Information found for the specified Card Item Id";
                    response.Status.Message.TechnicalMessage = "No Card Item Information found!";
                    return response;
                }

                var thisCardDelivery = GetCardDeliveryInfo(regObj.CardId);
                if (thisCardDelivery == null)
                {
                    response.Status.Message.FriendlyMessage = "No Card Delivery Information found for the specified Card Item Id";
                    response.Status.Message.TechnicalMessage = "No Card Delivery Information found!";
                    return response;
                }
                if (regObj.MissingQuantityFound > thisCardItem.MissingQuantity)
                {
                    response.Status.Message.FriendlyMessage = "Quantity Found Cannot be more than Missing quantity";
                    response.Status.Message.TechnicalMessage = "Quantity Found Cannot be more than Missing quantity!";
                    return response;
                }

                if (regObj.DefectiveQuantityRectified > thisCardItem.DefectiveQuantity)
                {
                    response.Status.Message.FriendlyMessage = "defective Quantity Rectified Cannot be more than defective quantity";
                    response.Status.Message.TechnicalMessage = "defective Quantity Found Cannot be more than Missing quantity!";
                    return response;
                }

                using (var db= _uoWork.BeginTransaction())
                {
                    //Update card item
                    thisCardItem.AvailableQuantity = regObj.MissingQuantityFound > 0 || regObj.DefectiveQuantityRectified > 0 ? thisCardItem.AvailableQuantity + regObj.MissingQuantityFound + regObj.DefectiveQuantityRectified : thisCardItem.AvailableQuantity;
                    thisCardItem.MissingQuantity = regObj.MissingQuantityFound > 0 ? thisCardItem.MissingQuantity - regObj.MissingQuantityFound : thisCardItem.MissingQuantity;
                    thisCardItem.DefectiveQuantity = regObj.DefectiveQuantityRectified > 0 ? thisCardItem.DefectiveQuantity - regObj.DefectiveQuantityRectified : thisCardItem.DefectiveQuantity;

                    var added = _cardItemRepository.Update(thisCardItem);
                    _uoWork.SaveChanges();
                    if (added.CardItemId < 1)
                    {
                        db.Rollback();
                        response.Status.Message.FriendlyMessage = "Error Occurred! Unable to complete your request. Please try again later";
                        response.Status.Message.TechnicalMessage = "Unable to save to database";
                        return response;
                    }

                    //Update Card Delivery
                    thisCardDelivery.MissingQuantity = regObj.MissingQuantityFound > 0 ? thisCardDelivery.MissingQuantity - regObj.MissingQuantityFound : thisCardDelivery.MissingQuantity;
                    thisCardDelivery.DefectiveQuantity = regObj.DefectiveQuantityRectified > 0 ? thisCardDelivery.DefectiveQuantity - regObj.DefectiveQuantityRectified : thisCardDelivery.DefectiveQuantity;

                    var deliveryAdded = _cardDeliveryRepository.Update(thisCardDelivery);
                    _uoWork.SaveChanges();
                    if (deliveryAdded.CardDeliveryId < 1)
                    {
                        db.Rollback();
                        response.Status.Message.FriendlyMessage = "Error Occurred! Unable to complete your request. Please try again later";
                        response.Status.Message.TechnicalMessage = "Unable to save to database";
                        return response;
                    }

                    db.Commit();

                    response.Status.IsSuccessful = true;
                    response.CardId = regObj.CardId;
                    response.Status.Message.FriendlyMessage = "Card Item Update Successfully";
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

        public SettingsRegResponseObj DeleteCard(DeleteCardObj regObj)
        {
            var response = new SettingsRegResponseObj()
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

                var thisCard = GetCardInfo(regObj.CardId);
                if (thisCard == null)
                {
                    response.Status.Message.FriendlyMessage = "No Card Production Information found for the specified Card Id";
                    response.Status.Message.TechnicalMessage = "No Card Production Information found!";
                    return response;
                }

                if (thisCard.Status != CardStatus.Registered)
                {
                    response.Status.Message.FriendlyMessage = "Sorry This Card Production Is Not Valid For Delete! Please Try Again Later";
                    response.Status.Message.TechnicalMessage = " Supply Requisition Status is either already Active/Issued/Retired!";
                    return response;
                }

                thisCard.CardTitle =
                    thisCard.CardTitle + "_Deleted_" + DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss");
                thisCard.Status = CardStatus.Defective;

                var added = _repository.Update(thisCard);
                _uoWork.SaveChanges();
                if (added.CardId < 1)
                {
                    response.Status.Message.FriendlyMessage = "Error Occurred! Unable to complete your request. Please try again later";
                    response.Status.Message.TechnicalMessage = "Unable to save to database";
                    return response;
                }


                response.Status.IsSuccessful = true;
                response.SettingId = added.CardId;

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

        public CardRespObj LoadCardByDate(LoadCardByDateObj regObj)
        {
            var response = new CardRespObj()
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


                var thiscardList = getCards();
                if (!thiscardList.Any())
                {
                    response.Status.Message.FriendlyMessage = "No Card Production Information found!";
                    response.Status.Message.TechnicalMessage = "No Card Production Information found!";
                    return response;
                }

                var CardByDate= new List<CardObj>();
                var CardItemByDate=new List<CardItemObj>();

                foreach (var m in thiscardList)
                {
                    var dateCreated = m.TimeStampRegisered;
                    var value = dateCreated.IndexOf(' ');
                    if (value > 0)
                    {
                        dateCreated = dateCreated.Substring(0, value);
                    }
                    var realDate = DateTime.Parse(dateCreated);
                    if (realDate >= DateTime.Parse(regObj.BeginDate) && realDate <= DateTime.Parse(regObj.EndDate))
                    {
                        foreach (var item in CardItemByDate)
                        {
                            CardItemByDate.Add(new CardItemObj
                            {
                                CardItemId = item.CardItemId,
                                CardId = item.CardId,
                                CardTypeId = item.CardTypeId,
                                BatchId = item.BatchId,
                                BatchStartNumber = item.BatchStartNumber,
                                BatchStopNumber = item.BatchStopNumber,
                                AvailableQuantity = item.AvailableQuantity,
                                BatchQuantity = item.BatchQuantity,
                                DefectiveBatchNumbers = item.DefectiveBatchNumbers,
                                DefectiveQuantity = item.DefectiveQuantity,
                                DeliveredQuantity = item.DeliveredQuantity,
                                IssuedQuantity = item.IssuedQuantity,
                                MissingQuantity = item.MissingQuantity,
                                RegisteredBy = item.RegisteredBy,
                                Status = (int)item.Status,
                                StatusLabel = item.Status.ToString().Replace("_", " "),
                                TimeStampDelivered = item.TimeStampDelivered,
                                TimeStampIssued = item.TimeStampIssued,
                                TimeStampRegistered = item.TimeStampRegistered
                            });

                           
                        }
                    }

                    CardByDate.Add(new CardObj
                    {
                        CardId = m.CardId,
                        CardTypeId = m.CardTypeId,
                        CardTitle = m.CardTitle,
                        BatchKey = m.BatchKey,
                        StartBatchId = m.StartBatchId,
                        StopBatchId = m.StopBatchId,
                        NoOfBatches = m.NumberOfBatches,
                        QuantityPerBatch = m.QuantityPerBatch,
                        TotalQuantity = m.TotalQuantity,
                        TimeStampRegistered = m.TimeStampRegisered,
                        Status = (int)m.Status,
                        StatusLabel = m.Status.ToString().Replace("_", " "),
                        CardItems = CardItemByDate
                    });
                }

                response.Status.IsSuccessful = true;
                response.Cards = CardByDate;
                return response;

            }
            catch (Exception ex)
            {
               ErrorManager.LogApplicationError(ex.StackTrace,ex.Source,ex.Message);
                return new CardRespObj();
            }
        }

        public CardRespObj LoadCards(SettingSearchObj searchObj)
        {

            var response= new CardRespObj
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

                if (!HelperMethods.IsUserValid(searchObj.AdminUserId, searchObj.SysPathCode, HelperMethods.getRequesterRoles(), ref response.Status.Message))
                {
                    return response;
                }

                var thisCard = GetCards(searchObj);
                if (!thisCard.Any())
                {
                    response.Status.Message.FriendlyMessage = "No Card Production Information found!";
                    response.Status.Message.TechnicalMessage = "No Card Production  Information found!";
                    return response;
                }

                var cards= new List<CardObj>();
                foreach (var m in thisCard)
                {
                    var cardItem = new List<CardItemObj>();

                    var CardItemList = getCardItems(m.CardId);
                    if (!CardItemList.Any())
                    {
                        response.Status.Message.FriendlyMessage = "No Card Item Production Information found!";
                        response.Status.Message.TechnicalMessage = "No Card Item Production  Information found!";
                        return response;
                    }

                    foreach (var item in CardItemList)
                    {
                        cardItem.Add(new CardItemObj
                        {
                            CardItemId = item.CardItemId,
                            CardId = item.CardId,
                            CardTypeId = item.CardTypeId,
                            BatchId = item.BatchId,
                            BatchStartNumber = item.StartBatchNumber,
                            BatchStopNumber = item.StopBatchNumber,
                            AvailableQuantity = item.AvailableQuantity,
                            BatchQuantity = item.BatchQuantity,
                            DefectiveBatchNumbers = item.DefectiveBatchNumber,
                            DefectiveQuantity = item.DefectiveQuantity,
                            DeliveredQuantity = item.DeliveredQuantity,
                            IssuedQuantity = item.IssuedQuantity,
                            MissingQuantity = item.MissingQuantity,
                            RegisteredBy = item.RegisteredBy,
                            Status = (int)item.Status,
                            StatusLabel = item.Status.ToString().Replace("_", " "),
                            TimeStampDelivered = item.TimeStampDelivered,
                            TimeStampIssued = item.TimeStampLastIssued,
                            TimeStampRegistered = item.TimeStampRegisered
                        });
                    }

                    cards.Add(new CardObj()
                    {
                        CardId = m.CardId,
                        CardTypeId = m.CardTypeId,
                        CardTitle = m.CardTitle,
                        BatchKey = m.BatchKey,
                        StartBatchId = m.StartBatchId,
                        StopBatchId = m.StopBatchId,
                        NoOfBatches = m.NumberOfBatches,
                        QuantityPerBatch = m.QuantityPerBatch,
                        TotalQuantity = m.TotalQuantity,
                        TimeStampRegistered = m.TimeStampRegisered,
                        Status = (int)m.Status,
                        StatusLabel = m.Status.ToString().Replace("_", " "),
                        CardItems = cardItem
                    });
                }

                response.Status.IsSuccessful = true;
                response.Cards = cards;
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

        public Card GetCardInfo(long cardId)
        {
            try
            {
                var sql =
                    $"Select * FROM \"NewVPlusSales\".\"Card\" WHERE \"CardId\" = '{cardId}')";

                var agentInfo = _repository.RepositoryContext().Database.SqlQuery<Card>(sql).ToList();

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
        private CardItem GetCardItemInfo(long cardItemId)
        {
            try
            {
                var sql =
                    $"Select * FROM \"NewVPlusSales\".\"CardItem\" WHERE \"CardId\" = '{cardItemId}')";

                var agentInfo = _cardItemRepository.RepositoryContext().Database.SqlQuery<CardItem>(sql).ToList();

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
        private CardDelivery GetCardDeliveryInfo(long cardDeliveryId)
        {
            try
            {
                var sql =
                    $"Select * FROM \"NewVPlusSales\".\"CardDelivery\" WHERE \"CardDeliveryId\" = '{cardDeliveryId}')";

                var agentInfo = _cardDeliveryRepository.RepositoryContext().Database.SqlQuery<CardDelivery>(sql).ToList();

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

        private List<Card> GetCards(SettingSearchObj searchObj)
        {
            try
            {
                var sql = new StringBuilder();
                if (searchObj.Status == -2)
                {
                    sql.Append(
                        $"SELECT * FROM \"NewVPlusSale\".\"Card\" WHERE \"Status\" != {(int)CardStatus.Defective} AND \"Status\" !={(int)CardStatus.Deleted} ");
                }
                else
                {
                    sql.Append(
                        $"SELECT * FROM \"NewVPlusSale\".\"Card\" WHERE \"Status\" != {searchObj.Status} ");
                }

                var agentInfo = _repository.RepositoryContext().Database.SqlQuery<Card>(sql.ToString()).ToList();

                return !agentInfo.Any() ? new List<Card>() : agentInfo;
            }
            catch (Exception ex)
            {

                ErrorManager.LogApplicationError(ex.StackTrace, ex.Source, ex.Message);
                return new List<Card>();
            }
        }
        public List<Card> getCards()
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(
                    $"SELECT * FROM \"NewVPlusSale\".\"Card\" WHERE \"Status\" != {(int)CardStatus.Defective} AND \"Status\" !={(int)CardStatus.Deleted} ");

                var agentInfo = _repository.RepositoryContext().Database.SqlQuery<Card>(sql.ToString()).ToList();

                return !agentInfo.Any() ? new List<Card>() : agentInfo;
            }
            catch (Exception ex)
            {

                ErrorManager.LogApplicationError(ex.StackTrace, ex.Source, ex.Message);
                return new List<Card>();
            }
        }

        private List<CardItem> getCardItems(int CardItemId)
        {
            try
            {
                var sql = new StringBuilder();
                sql.Append(
                    $"SELECT * FROM \"NewVPlusSale\".\"CardItem\" WHERE \"CardItemId\" = {CardItemId} ");

                var agentInfo = _cardItemRepository.RepositoryContext().Database.SqlQuery<CardItem>(sql.ToString()).ToList();

                return !agentInfo.Any() ? null : agentInfo;
            }
            catch (Exception ex)
            {

                ErrorManager.LogApplicationError(ex.StackTrace, ex.Source, ex.Message);
                return new List<CardItem>();
            }
        }
    }
}
