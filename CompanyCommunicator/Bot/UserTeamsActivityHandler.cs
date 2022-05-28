// <copyright file="UserTeamsActivityHandler.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// </copyright>

namespace Microsoft.Teams.Apps.CompanyCommunicator.Bot
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AdaptiveCards;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.NotificationData;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.SentNotificationData;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Services.AdaptiveCard;
    using Microsoft.Teams.Apps.CompanyCommunicator.Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Company Communicator User Bot.
    /// Captures user data, team data.
    /// </summary>
    public class UserTeamsActivityHandler : TeamsActivityHandler
    {
        private static readonly string TeamRenamedEventType = "teamRenamed";
        private static readonly string AdaptiveCardContentType = "application/vnd.microsoft.card.adaptive";
        private readonly TeamsDataCapture teamsDataCapture;
        private readonly ISentNotificationDataRepository sentNotificationDataRepository;
        private readonly INotificationDataRepository notificationDataRepository;
        private readonly AdaptiveCardCreator adaptiveCardCreator;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserTeamsActivityHandler"/> class.
        /// </summary>
        /// <param name="teamsDataCapture">Teams data capture service.</param>
        public UserTeamsActivityHandler(TeamsDataCapture teamsDataCapture, ISentNotificationDataRepository sentNotificationDataRepository, INotificationDataRepository notificationDataRepository,AdaptiveCardCreator adaptiveCardCreator)
        {
            this.teamsDataCapture = teamsDataCapture ?? throw new ArgumentNullException(nameof(teamsDataCapture));
            this.sentNotificationDataRepository = sentNotificationDataRepository ?? throw new ArgumentNullException(nameof(sentNotificationDataRepository));
            this.notificationDataRepository = notificationDataRepository ?? throw new ArgumentNullException(nameof(notificationDataRepository));
            this.adaptiveCardCreator = adaptiveCardCreator ?? throw new ArgumentNullException(nameof(adaptiveCardCreator));
        }

        /// <summary>
        /// Invoked when a conversation update activity is received from the channel.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnConversationUpdateActivityAsync(
            ITurnContext<IConversationUpdateActivity> turnContext,
            CancellationToken cancellationToken)
        {
            // base.OnConversationUpdateActivityAsync is useful when it comes to responding to users being added to or removed from the conversation.
            // For example, a bot could respond to a user being added by greeting the user.
            // By default, base.OnConversationUpdateActivityAsync will call <see cref="OnMembersAddedAsync(IList{ChannelAccount}, ITurnContext{IConversationUpdateActivity}, CancellationToken)"/>
            // if any users have been added or <see cref="OnMembersRemovedAsync(IList{ChannelAccount}, ITurnContext{IConversationUpdateActivity}, CancellationToken)"/>
            // if any users have been removed. base.OnConversationUpdateActivityAsync checks the member ID so that it only responds to updates regarding members other than the bot itself.
            await base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);

            var activity = turnContext.Activity;

            var isTeamRenamed = this.IsTeamInformationUpdated(activity);
            if (isTeamRenamed)
            {
                await this.teamsDataCapture.OnTeamInformationUpdatedAsync(activity);
            }

            if (activity.MembersAdded != null)
            {
                await this.teamsDataCapture.OnBotAddedAsync(turnContext, activity, cancellationToken);
            }

            if (activity.MembersRemoved != null)
            {
                await this.teamsDataCapture.OnBotRemovedAsync(activity);
            }
        }

        protected override async Task<InvokeResponse> OnInvokeActivityAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Name == "adaptiveCard/action")
            {
                var data = JsonConvert.DeserializeObject<InitialSequentialCard>(turnContext.Activity.Value.ToString());
                string verb = data.action.verb;
                AdaptiveCardInvokeResponse adaptiveCardResponse;
                JObject response;

                switch (verb)
                {
                    case "userAcknowledge":

                        var notificationID = data.action.data.action;
                        var notificationEntity = await this.notificationDataRepository.GetAsync(
                        NotificationDataTableNames.SentNotificationsPartition,
                        notificationID);

                        if (notificationEntity != null)
                        {

                            List<TrackingButtonClicks> result;

                            if (notificationEntity.ButtonTrackingClicks is null)
                            {

                                result = new List<TrackingButtonClicks>();

                                var click = new TrackingButtonClicks { Name = "Acknowledged", Clicks = 1 };
                                result.Add(click);
                            }
                            else
                            {
                                result = JsonConvert.DeserializeObject<List<TrackingButtonClicks>>(notificationEntity.ButtonTrackingClicks);

                                var button = result.Find(p => p.Name == "Acknowledged");

                                if (button == null)
                                {
                                    result.Add(new TrackingButtonClicks { Name = "Acknowledged", Clicks = 1 });
                                }
                                else
                                {
                                    button.Clicks++;
                                }

                            }

                            notificationEntity.ButtonTrackingClicks = JsonConvert.SerializeObject(result);

                            // persists the change
                            await this.notificationDataRepository.CreateOrUpdateAsync(notificationEntity);

                            // save the user button clicked
                            await this.UpdateButtonClickedByUser(notificationID, turnContext.Activity.From.AadObjectId, "Acknowledged");
                        }

                        var adaptiveCard = this.CreateAdaptiveCardActivity(notificationEntity);

                        Activity updateActivity = new Activity();
                        updateActivity.Type = "message";
                        updateActivity.Id = turnContext.Activity.ReplyToId;
                        updateActivity.Attachments = new List<Attachment> { adaptiveCard };
                        await turnContext.UpdateActivityAsync(updateActivity);

                        string cardValue = JsonConvert.SerializeObject(adaptiveCard);
                        response = JObject.Parse(cardValue);
                        adaptiveCardResponse = new AdaptiveCardInvokeResponse()
                        {
                            StatusCode = 200,
                            Type = "application/vnd.microsoft.card.adaptive",
                            Value = response,
                        };

                        return CreateInvokeResponse(adaptiveCardResponse);

                }

            }

            return null;
        }

        ///// <summary>
        ///// Invoked when button is clicked within the adative card
        ///// </summary>
        ///// <param name="turnContext">The context object for this turn.</param>
        ///// <param name="invokeValue">Invoke value of the bot activity</param>
        ///// <param name="cancellationToken">A cancellation token that can be used by other objects
        ///// or threads to receive notice of cancellation.</param>
        ///// <returns>return response from the bot</returns>
        //protected override async Task<AdaptiveCardInvokeResponse> OnAdaptiveCardInvokeAsync(ITurnContext<IInvokeActivity> turnContext, AdaptiveCardInvokeValue invokeValue, CancellationToken cancellationToken)
        //{
        //    if (invokeValue.Action.Verb == "userAcknowledge")
        //    {
        //        JObject value = (JObject)invokeValue.Action.Data;
        //        ReadNotification finalData = value.ToObject<ReadNotification>();

        //        // gets the sent notification summary that needs to be updated
        //        var notificationEntity = await this.notificationDataRepository.GetAsync(
        //            NotificationDataTableNames.SentNotificationsPartition,
        //            finalData.action);

        //        if (notificationEntity != null)
        //        {

        //            List<TrackingButtonClicks> result;

        //            if (notificationEntity.ButtonTrackingClicks is null)
        //            {

        //                result = new List<TrackingButtonClicks>();

        //                var click = new TrackingButtonClicks { Name = "Acknowledged", Clicks = 1 };
        //                result.Add(click);
        //            }
        //            else
        //            {
        //                result = JsonConvert.DeserializeObject<List<TrackingButtonClicks>>(notificationEntity.ButtonTrackingClicks);

        //                var button = result.Find(p => p.Name == "Acknowledged");

        //                if (button == null)
        //                {
        //                    result.Add(new TrackingButtonClicks { Name = "Acknowledged", Clicks = 1 });
        //                }
        //                else
        //                {
        //                    button.Clicks++;
        //                }

        //            }

        //            notificationEntity.ButtonTrackingClicks = JsonConvert.SerializeObject(result);

        //            // persists the change
        //            await this.notificationDataRepository.CreateOrUpdateAsync(notificationEntity);

        //            // save the user button clicked
        //            await this.UpdateButtonClickedByUser(finalData.action, turnContext.Activity.From.AadObjectId, "Acknowledged");

        //            var adaptiveCard = this.CreateAdaptiveCardActivity(notificationEntity);
        //            Activity updateActivity = new Activity();
        //            updateActivity.Type = "message";
        //            updateActivity.Id = turnContext.Activity.ReplyToId;
        //            updateActivity.Attachments = new List<Attachment> { adaptiveCard };
        //            await turnContext.UpdateActivityAsync(updateActivity);
        //        }
        //    }

        //    var response = new AdaptiveCardInvokeResponse()
        //    {
        //        StatusCode = 200,
        //        Type = null,
        //        Value = null,
        //    };
        //    return response;
        //}

        private Attachment CreateAdaptiveCardActivity(NotificationDataEntity notificationEntity)
        {
            var adaptiveCard = this.adaptiveCardCreator.CreateAdaptiveCard(
                    notificationEntity.Title,
                    notificationEntity.ImageLink,
                    notificationEntity.Summary,
                    notificationEntity.Author,
                    notificationEntity.ButtonTitle,
                    notificationEntity.ButtonLink,
                    notificationEntity.Buttons,
                    string.Empty,
                    notificationEntity.ChannelImage,
                    notificationEntity.ChannelTitle,
                    notificationEntity.IsAcknowledged,
                    notificationEntity.Color,
                    notificationEntity.IsImportant,
                    notificationEntity.Id,
                    true);

            var attachment = new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = adaptiveCard,
            };
            return attachment;
        }

        private async Task UpdateButtonClickedByUser(string id, string key, string buttonid)
        {
            // gets the sent notification object for the message sent
            var sentnotificationEntity = await this.sentNotificationDataRepository.GetAsync(id, key);

            // if we have a instance that was sent to a user
            if (sentnotificationEntity != null)
            {

                List<TrackingUserClicks> result;

                if (sentnotificationEntity.ButtonTracking is null)
                {
                    sentnotificationEntity.AcknowledgeStatus = true;
                    sentnotificationEntity.AcknowledgedDate = DateTime.UtcNow;
                    result = new List<TrackingUserClicks>();

                    var click = new TrackingUserClicks { Name = buttonid, Clicks = 1, DateTime = DateTime.Now };
                    result.Add(click);
                }
                else
                {

                    result = JsonConvert.DeserializeObject<List<TrackingUserClicks>>(sentnotificationEntity.ButtonTracking);

                    var button = result.Find(p => p.Name == buttonid);

                    if (button == null)
                    {
                        result.Add(new TrackingUserClicks { Name = buttonid, Clicks = 1, DateTime = DateTime.Now });
                    }
                    else
                    {
                        button.Clicks++;
                        button.DateTime = DateTime.Now;
                    }
                }

                sentnotificationEntity.ButtonTracking = JsonConvert.SerializeObject(result);

                await this.sentNotificationDataRepository.CreateOrUpdateAsync(sentnotificationEntity);
            }
        }

        private bool IsTeamInformationUpdated(IConversationUpdateActivity activity)
        {
            if (activity == null)
            {
                return false;
            }

            var channelData = activity.GetChannelData<TeamsChannelData>();
            if (channelData == null)
            {
                return false;
            }

            return UserTeamsActivityHandler.TeamRenamedEventType.Equals(channelData.EventType, StringComparison.OrdinalIgnoreCase);
        }
    }
}