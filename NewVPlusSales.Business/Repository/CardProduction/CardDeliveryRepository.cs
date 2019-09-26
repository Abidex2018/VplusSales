using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics.Tracing;
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
using NewVPlusSales.Common;
using XPLUG.WEBTOOLS;

namespace NewVPlusSales.Business.Repository.CardProduction
{
   internal class CardDeliveryRepository
   {
       private readonly INewVPlusSalesRepository<CardDelivery> _repository;
       private readonly INewVPlusSalesRepository<Card> _cardRepository;
       private readonly INewVPlusSalesRepository<CardItem> _cardItemRepository;
        private readonly NewVPlusSalesUoWork _uoWork;

       public CardDeliveryRepository()
       {
           _repository = new NewVPlusSalesRepository<CardDelivery>(_uoWork);
           _cardItemRepository = new NewVPlusSalesRepository<CardItem>(_uoWork);
           _cardRepository = new NewVPlusSalesRepository<Card>(_uoWork);
           _uoWork = new NewVPlusSalesUoWork();
       }

       public CardDeliveryRegRespObj AddCardDelivery(RegCardDeliveryObj regObj)
       {

           var response = new CardDeliveryRegRespObj
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

               var associatedCard = GetCardInfo(regObj.CardId);
               if (associatedCard == null)
               {
                   response.Status.Message.FriendlyMessage = "Error Occurred! No Card  Information Found";
                   response.Status.Message.TechnicalMessage = "Error Occurred! No Card  Information Found";
                   return response;
                }

               var associatedCardItem = GetCardItemInfo(regObj.CardItemId);
               if (associatedCardItem == null)
               {
                   response.Status.Message.FriendlyMessage = "Error Occurred! No Card Item  Information Found";
                   response.Status.Message.TechnicalMessage = "Error Occurred! No Card Item Information Found";
                   return response;
                }

               if (associatedCard.Status != CardStatus.Registered)
               {
                   response.Status.Message.FriendlyMessage = "Error Occurred! This Card Item Is Not Available For Delivery";
                   response.Status.Message.TechnicalMessage = "Error Occurred! This Card Item Is Not Available For Delivery";
                   return response;
                }

               //check validity of start/stop batch number

               if ((int.Parse(regObj.StopBatchNumber) - int.Parse(regObj.StartBatchNumber) + 1) !=
                   associatedCard.QuantityPerBatch)
               {
                   response.Status.Message.FriendlyMessage = "Error Occurred! Incorrect StopBatchNumber/StartBatchNumber Data";
                   response.Status.Message.TechnicalMessage = "Error Occurred! Incorrect StopBatchNumber/StartBatchNumber Data";
                   return response;
                }

               if (regObj.BatchId != associatedCardItem.BatchId)
               {
                   response.Status.Message.FriendlyMessage = "Error Occurred! Incorrect BatchId";
                   response.Status.Message.TechnicalMessage = "Error Occurred! Incorrect BatchId";
                   return response;
                }
               if (regObj.DeliveredQuantity < 1)
               {
                   response.Status.Message.FriendlyMessage = "Error Occurred! You Cannot Register An empty delivery ";
                   response.Status.Message.TechnicalMessage = "Error Occurred! Incorrect quantity Delivered Data";
                   return response;
                }

               if (regObj.DeliveredQuantity + associatedCardItem.DeliveredQuantity > associatedCardItem.BatchQuantity)
               {
                   if (associatedCardItem.BatchQuantity - (associatedCardItem.DeliveredQuantity) > 0)
                   {
                       response.Status.Message.FriendlyMessage = $"Incorrect Quantity Delivered,{associatedCardItem.BatchQuantity - (associatedCardItem.DeliveredQuantity)} is only available for delivery";
                   }
                   else if (associatedCardItem.BatchQuantity - (associatedCardItem.DeliveredQuantity) == 0)
                   {
                       response.Status.Message.FriendlyMessage = $"This Delivery is Complete";
                   }

                   response.Status.Message.TechnicalMessage = "Error Occurred! Incorrect Quantity Delivered";
                   return response;
               }

               if (DateTime.Parse(regObj.TimeStampDelivered) > DateTime.Now)
               {
                   response.Status.Message.FriendlyMessage = "Error Occurred! You Cannot Register A delivery before DeliveryDate";
                   response.Status.Message.TechnicalMessage = "Error Occurred! Incorrect Delivery Date Data";
                   return response;
               }

               using (var db= _uoWork.BeginTransaction())
               {
                    var newCardDelivery= new CardDelivery
                    {
                        CardId = regObj.CardId,
                        CardItemId = regObj.CardItemId,
                        CardTypeId = regObj.CardTypeId,
                        BatchId = associatedCardItem.BatchId,
                        Status = CardStatus.Registered,
                        ApprovedBy = 0,
                       ApproverComment = regObj.ApproverComment,
                        TimeStampApproved = "",
                        DefectiveQuantity = regObj.DefectiveQuantity,
                        TimeStampDelivered = regObj.TimeStampDelivered,
                        MissingQuantity = regObj.MissingQuantity,
                        DeliveredQuantity = regObj.DeliveredQuantity,
                        StartBatchNumber = associatedCardItem.BatchId + "" + "000",
                        StopBatchNumber = associatedCardItem.BatchId + "" + "999",
                        ReceivedBy = regObj.AdminUserId,
                        TimeStampRegisered = DateMap.CurrentTimeStamp(),
                       
                    };

                   var deliveryAdded = _repository.Add(newCardDelivery);
                    _uoWork.SaveChanges();
                   if (deliveryAdded.CardDeliveryId< 1)
                   {
                       db.Rollback();
                       response.Status.Message.FriendlyMessage = "Error Occurred! Unable to complete your request. Please try again later";
                       response.Status.Message.TechnicalMessage = "Unable to save to database";
                       return response;
                    }
                   associatedCardItem.MissingQuantity += regObj.MissingQuantity;
                   associatedCardItem.DefectiveQuantity += regObj.DefectiveQuantity;
                   associatedCardItem.DeliveredQuantity += regObj.DeliveredQuantity;
                   associatedCardItem.AvailableQuantity += regObj.DeliveredQuantity - (regObj.MissingQuantity + regObj.DefectiveQuantity);
                   associatedCardItem.TimeStampDelivered = deliveryAdded.TimeStampRegisered;
                   associatedCardItem.DefectiveBatchNumber = regObj.DefectiveBatchNumber;
                   associatedCardItem.Status = CardStatus.Registered;

                   var updateCardItem = _cardItemRepository.Update(associatedCardItem);
                    _uoWork.SaveChanges();
                   if (updateCardItem.CardItemId < 1)
                   {
                       db.Rollback();
                       response.Status.Message.FriendlyMessage = "Error Occurred! Unable to complete your request. Please try again later";
                       response.Status.Message.TechnicalMessage = "Unable to save to database";
                       return response;
                   }


                   associatedCard.Status = CardStatus.Registered;
                   var updateCard = _cardRepository.Update(associatedCard);
                    _uoWork.SaveChanges();
                   if (updateCard.CardId < 1)
                   {
                       db.Rollback();
                       response.Status.Message.FriendlyMessage = "Error Occurred! Unable to complete your request. Please try again later";
                       response.Status.Message.TechnicalMessage = "Unable to save to database";
                       return response;
                    }
                    db.Commit();

                   response.Status.IsSuccessful = true;
                   response.CardDeliveryId = deliveryAdded.CardDeliveryId;
                   response.Status.Message.FriendlyMessage = "Card Delivery Added Successfully";
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

       public CardDeliveryRegRespObj UpdateCardDelivery(EditCardDeliveryObj regObj)
       {
           var response= new CardDeliveryRegRespObj
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
               var thisCardDelivery = GetCardDeliveryInfo(regObj.CardDeliveryId);
               if (thisCardDelivery == null)
               {
                   response.Status.Message.FriendlyMessage = "Error Occurred! No Card  Information Found";
                   response.Status.Message.TechnicalMessage = "Error Occurred! No Card  Information Found";
                   return response;
                }
                var associatedCard = GetCardInfo(regObj.CardId);
                if (associatedCard == null)
                {
                    response.Status.Message.FriendlyMessage = "Error Occurred! No Card  Information Found";
                    response.Status.Message.TechnicalMessage = "Error Occurred! No Card  Information Found";
                    return response;
                }

                var associatedCardItem = GetCardItemInfo(regObj.CardItemId);
                if (associatedCardItem == null)
                {
                    response.Status.Message.FriendlyMessage = "Error Occurred! No Card Item  Information Found";
                    response.Status.Message.TechnicalMessage = "Error Occurred! No Card Item Information Found";
                    return response;
                }

                if (associatedCard.Status != CardStatus.Registered)
                {
                    response.Status.Message.FriendlyMessage = "Error Occurred! This Card Item Is Not Available For Delivery";
                    response.Status.Message.TechnicalMessage = "Error Occurred! This Card Item Is Not Available For Delivery";
                    return response;
                }

                //check validity of start/stop batch number
                
                if (regObj.DeliveredQuantity < 1)
                {
                    response.Status.Message.FriendlyMessage = "Error Occurred! You Cannot Register An empty delivery ";
                    response.Status.Message.TechnicalMessage = "Error Occurred! Incorrect quantity Delivered Data";
                    return response;
                }
                
                if (DateTime.Parse(regObj.DeliveryDate) > DateTime.Now)
                {
                    response.Status.Message.FriendlyMessage = "Error Occurred! You Cannot Register A delivery before DeliveryDate";
                    response.Status.Message.TechnicalMessage = "Error Occurred! Incorrect Delivery Date Data";
                    return response;
                }

               using (var db= _uoWork.BeginTransaction())
               {
                   thisCardDelivery.DefectiveQuantity = regObj.DefectiveQuantity > 0 ? regObj.DefectiveQuantity : thisCardDelivery.DefectiveQuantity;
                   thisCardDelivery.MissingQuantity = regObj.MissingQuantity > 0 ? regObj.MissingQuantity : thisCardDelivery.MissingQuantity;
                   thisCardDelivery.DeliveredQuantity = regObj.DeliveredQuantity > 0 ? regObj.DeliveredQuantity : thisCardDelivery.DeliveredQuantity;
                   thisCardDelivery.TimeStampDelivered = !string.IsNullOrWhiteSpace(regObj.DeliveryDate) ? regObj.DeliveryDate : thisCardDelivery.TimeStampDelivered;

                   var updateCardDelivery = _repository.Update(thisCardDelivery);
                    _uoWork.SaveChanges();
                   if (updateCardDelivery.CardDeliveryId<1)
                   {
                       db.Rollback();
                       response.Status.Message.FriendlyMessage = "Error Occurred! Unable to complete your request. Please try again later";
                       response.Status.Message.TechnicalMessage = "Unable to save to database";
                       return response;
                    }
                   associatedCardItem.MissingQuantity = regObj.MissingQuantity;
                   associatedCardItem.DefectiveQuantity = regObj.DefectiveQuantity;
                   associatedCardItem.DeliveredQuantity = regObj.DeliveredQuantity;
                   associatedCardItem.AvailableQuantity = regObj.DeliveredQuantity - (regObj.MissingQuantity + regObj.DefectiveQuantity);
                   associatedCardItem.DefectiveQuantity = regObj.DefectiveQuantity;

                   var updateCardItem = _cardItemRepository.Update(associatedCardItem);
                    _uoWork.SaveChanges();
                   if (updateCardItem.CardItemId < 1)
                   {
                       db.Rollback();
                       response.Status.Message.FriendlyMessage = "Error Occurred! Unable to complete your request. Please try again later";
                       response.Status.Message.TechnicalMessage = "Unable to save to database";
                       return response;
                    }

                   db.Commit();

                   response.Status.IsSuccessful = true;
                   response.CardDeliveryId = updateCardDelivery.CardDeliveryId;
                   response.Status.Message.FriendlyMessage = "Card delivery Update Successfully";
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

       public CardDeliveryRegRespObj ApproveCardDelivery(ApproveCardDeliveryObj regObj)
       {
           var response= new CardDeliveryRegRespObj
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

               if (!HelperMethods.IsUserValid(regObj.AdminUserId, regObj.SysPathCode, HelperMethods.getMgtExecutiveRoles(), ref response.Status.Message))
               {
                   return response;
               }
                var thisCardDelivery = GetCardDeliveryInfo(regObj.CardDeliveryId);
               if (thisCardDelivery == null)
               {
                   response.Status.Message.FriendlyMessage = "Error Occurred! No Card  Information Found";
                   response.Status.Message.TechnicalMessage = "Error Occurred! No Card  Information Found";
                   return response;
               }
               var associatedCard = GetCardInfo(thisCardDelivery.CardId);
               if (associatedCard == null)
               {
                   response.Status.Message.FriendlyMessage = "Error Occurred! No Card  Information Found";
                   response.Status.Message.TechnicalMessage = "Error Occurred! No Card  Information Found";
                   return response;
               }

               var associatedCardItem = GetCardItemInfo(thisCardDelivery.CardItemId);
               if (associatedCardItem == null)
               {
                   response.Status.Message.FriendlyMessage = "Error Occurred! No Card Item  Information Found";
                   response.Status.Message.TechnicalMessage = "Error Occurred! No Card Item Information Found";
                   return response;
               }

               if (regObj.IsApproved && regObj.IsDenied || regObj.IsApproved && regObj.IsDenied)
               {
                   response.Status.Message.FriendlyMessage = "Sorry This CardDelivery Cannot be  Approved/Defected! Please Try Again Later";
                   response.Status.Message.TechnicalMessage = " IsApproved and IsDefected is both true/false";
                   return response;
                }

               if (regObj.IsApproved)
               {
                   using (var db = _uoWork.BeginTransaction())
                   {
                       thisCardDelivery.ApprovedBy = regObj.AdminUserId;
                       thisCardDelivery.TimeStampApproved = regObj.TimeStampApproved;
                       thisCardDelivery.ApproverComment = regObj.ApproverComment;
                       thisCardDelivery.Status = CardStatus.Available;

                       var updateDelivery = _repository.Update(thisCardDelivery);
                       _uoWork.SaveChanges();
                       if (updateDelivery.CardDeliveryId < 1)
                       {
                           db.Rollback();
                           response.Status.Message.FriendlyMessage = "Error Occurred! Unable to complete your request. Please try again later";
                           response.Status.Message.TechnicalMessage = "Unable to save to database";
                           return response;
                       }

                       associatedCardItem.Status = CardStatus.Available;

                       var updateCardItem = _cardItemRepository.Update(associatedCardItem);
                       _uoWork.SaveChanges();
                       if (updateDelivery.CardItemId < 1)
                       {
                           db.Rollback();
                           response.Status.Message.FriendlyMessage = "Error Occurred! Unable to complete your request. Please try again later";
                           response.Status.Message.TechnicalMessage = "Unable to save to database";
                           return response;
                       }

                       associatedCard.Status = CardStatus.Available;

                       var updateCard = _cardRepository.Update(associatedCard);
                       _uoWork.SaveChanges();
                       if (updateDelivery.CardId < 1)
                       {
                           db.Rollback();
                           response.Status.Message.FriendlyMessage = "Error Occurred! Unable to complete your request. Please try again later";
                           response.Status.Message.TechnicalMessage = "Unable to save to database";
                           return response;
                       }
                        db.Commit();
                       response.Status.IsSuccessful = true;
                       response.CardDeliveryId = updateDelivery.CardDeliveryId;
                       response.Status.Message.FriendlyMessage = "Card Delivery Approved Successfully";
                   }

                   if (regObj.IsDenied)
                   {
                       using (var db = _uoWork.BeginTransaction())
                       {
                           thisCardDelivery.ApprovedBy = regObj.AdminUserId;
                           thisCardDelivery.TimeStampApproved = regObj.TimeStampApproved;
                           thisCardDelivery.ApproverComment = regObj.ApproverComment;
                           thisCardDelivery.Status = CardStatus.Unavalable;

                           var updateDelivery = _repository.Update(thisCardDelivery);
                           _uoWork.SaveChanges();
                           if (updateDelivery.CardDeliveryId < 1)
                           {
                               db.Rollback();
                               response.Status.Message.FriendlyMessage = "Error Occurred! Unable to complete your request. Please try again later";
                               response.Status.Message.TechnicalMessage = "Unable to save to database";
                               return response;
                           }

                           associatedCardItem.Status = CardStatus.Unavalable;
                           associatedCardItem.AvailableQuantity = 0;

                           var updateCardItem = _cardItemRepository.Update(associatedCardItem);
                           _uoWork.SaveChanges();
                           if (updateDelivery.CardItemId < 1)
                           {
                               db.Rollback();
                               response.Status.Message.FriendlyMessage = "Error Occurred! Unable to complete your request. Please try again later";
                               response.Status.Message.TechnicalMessage = "Unable to save to database";
                               return response;
                           }

                           db.Commit();
                           response.Status.IsSuccessful = true;
                           response.CardDeliveryId = updateDelivery.CardDeliveryId;
                           response.Status.Message.FriendlyMessage = "Defected Successfully";

                        }
                    }
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

       public CardDeliveryRespObj LoadCardDeliveryByDate(LoadCardDeliveryByDate searchObj)
       {
           var response= new CardDeliveryRespObj
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

               var thisCardDelivery = GetCardDelivies(searchObj.CardId);
               if (!thisCardDelivery.Any())
               {
                   response.Status.Message.FriendlyMessage = "No Card Delivery  Information found!";
                   response.Status.Message.TechnicalMessage = "No Card Delivery  Information found!";
                   return response;
                }

               var CardDeliveryByDate= new List<CardDeliveryObj>();

               foreach (var m in thisCardDelivery)
               {
                   if (!string.IsNullOrWhiteSpace(searchObj.BeginDate) && !string.IsNullOrWhiteSpace(searchObj.EndDate))
                   {
                       var dateCreated = m.TimeStampRegisered;
                       var value = dateCreated.IndexOf(' ');
                       if (value > 0)
                       {
                           dateCreated = dateCreated.Substring(0, value);
                       }
                       var realDate = DateTime.Parse(dateCreated);
                       if (realDate >= DateTime.Parse(searchObj.BeginDate) &&
                           realDate >= DateTime.Parse(searchObj.EndDate))
                       {
                           CardDeliveryByDate.Add(new CardDeliveryObj
                           {
                               CardDeliveryId = m.CardDeliveryId,
                               CardId = m.CardId,
                               CardIdLabel = new CardRepository().GetCardInfo(m.CardId).CardTitle,
                               BatchId = m.BatchId,
                               CardItemId = m.CardItemId,
                               CardTypeId = m.CardTypeId,
                               DeliveryDate = m.TimeStampDelivered,
                               StartBatchNumber = m.StartBatchNumber,
                               StopBatchNumber = m.StopBatchNumber,
                               DefectiveQuantity = m.DefectiveQuantity,
                               MissingQuantity = m.MissingQuantity,
                               QuantityDelivered = m.DeliveredQuantity,
                               RecievedBy = m.ReceivedBy,
                               ApprovedBy = m.ApprovedBy,
                               ApproverComment = m.ApproverComment,
                               TimeStampRegistered = m.TimeStampRegisered,
                               Status = (int)m.Status,
                               StatusLabel = m.Status.ToString().Replace("_", " "),
                               TimeStampApproved = m.TimeStampApproved
                           });
                       }
                   }

                   else
                   {
                       CardDeliveryByDate.Add(new CardDeliveryObj
                       {
                           CardDeliveryId = m.CardDeliveryId,
                           CardId = m.CardId,
                           CardIdLabel = new CardRepository().GetCardInfo(m.CardId).CardTitle,
                           BatchId = m.BatchId,
                           CardItemId = m.CardItemId,
                           CardTypeId = m.CardTypeId,
                           DeliveryDate = m.TimeStampDelivered,
                           StartBatchNumber = m.StartBatchNumber,
                           StopBatchNumber = m.StopBatchNumber,
                           DefectiveQuantity = m.DefectiveQuantity,
                           MissingQuantity = m.MissingQuantity,
                           QuantityDelivered = m.DeliveredQuantity,
                           RecievedBy = m.ReceivedBy,
                           ApprovedBy = m.ApprovedBy,
                           ApproverComment = m.ApproverComment,
                           TimeStampRegistered = m.TimeStampRegisered,
                           Status = (int)m.Status,
                           StatusLabel = m.Status.ToString().Replace("_", " "),
                       });
                   }
               }

               response.Status.IsSuccessful = true;
               response.CardDeliveries = CardDeliveryByDate;
               return response;
           }
            catch (Exception ex)
           {
              ErrorManager.LogApplicationError(ex.StackTrace,ex.Source,ex.Message);
               return new CardDeliveryRespObj();

           }
       }
       private Card GetCardInfo(int cardId)
       {
           try
           {
               var sql = $"SELECT * FROM \"NewVPlusSales\".\"Card\" WHERE \"CardId\" = {cardId} ";

               var agentInfo = _cardRepository.RepositoryContext().Database.SqlQuery<Card>(sql).ToList();
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

       private CardItem GetCardItemInfo(long cardItemId)
       {
           try
           {
               var sql = $"SELECT * \"NewVPlusSales\".\"CardItem\" WHERE \"CardItemId\" ={cardItemId}";
               var agentInfo = _cardItemRepository.RepositoryContext().Database.SqlQuery<CardItem>(sql).ToList();
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

       private CardDelivery GetCardDeliveryInfo(long cardDeliveryId)
       {
           try
           {
               var sql = $"SELECT * \"NewVPlusSales\".\"CardDelivery\" WHERE \"CardDeliveryId\" ={cardDeliveryId}";
               var agentInfo = _repository.RepositoryContext().Database.SqlQuery<CardDelivery>(sql).ToList();
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

       private List<CardDelivery> GetCardDelivies(int cardId)
       {
           try
           {
               if (cardId > 0)
               {
                   var Sql = new StringBuilder();
                   Sql.Append($"SELECT * FROM \"NewVPlusSales\".\"CardDelivery\" WHERE \"CardId\" = {cardId}");

                   var agentInfo = _repository.RepositoryContext().Database.SqlQuery<CardDelivery>(Sql.ToString()).ToList();

                   return !agentInfo.Any() ? new List<CardDelivery>() : agentInfo;
                }
               else
               {
                   var Sql2 = new StringBuilder();
                   Sql2.Append($"SELECT * FROM \"NewVPlusSales\".\"CardDelivery\"");

                   var agentInfo = _repository.RepositoryContext().Database.SqlQuery<CardDelivery>(Sql2.ToString()).ToList();

                   return !agentInfo.Any() ? new List<CardDelivery>() : agentInfo;
                }
           }
           catch (Exception ex)
           {
             ErrorManager.LogApplicationError(ex.StackTrace,ex.Source,ex.Message);
                return new List<CardDelivery>();
           }
       }
   }
}
