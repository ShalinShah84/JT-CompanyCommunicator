// <copyright file="AdaptiveCardCreator.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// </copyright>

namespace Microsoft.Teams.Apps.CompanyCommunicator.Common.Services.AdaptiveCard
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Reflection;
    using System.Text.Json;
    using AdaptiveCards;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.NotificationData;

    /// <summary>
    /// Adaptive Card Creator service.
    /// </summary>
    public class AdaptiveCardCreator
    {
        /// <summary>
        /// Creates an adaptive card.
        /// </summary>
        /// <param name="notificationDataEntity">Notification data entity.</param>
        /// <returns>An adaptive card.</returns>
        public virtual AdaptiveCard CreateAdaptiveCard(NotificationDataEntity notificationDataEntity)
        {
            return this.CreateAdaptiveCard(
                notificationDataEntity.Title,
                notificationDataEntity.ImageLink,
                notificationDataEntity.Summary,
                notificationDataEntity.Author,
                notificationDataEntity.ButtonTitle,
                notificationDataEntity.ButtonLink,
                notificationDataEntity.Buttons,
                notificationDataEntity.TrackingUrl,
                notificationDataEntity.ChannelImage,
                notificationDataEntity.ChannelTitle,
                notificationDataEntity.IsAcknowledged,
                notificationDataEntity.Color,
                notificationDataEntity.IsImportant,
                notificationDataEntity.Id,
                notificationDataEntity.AcknowledgeStatus);
        }

        /// <summary>
        /// Create an adaptive card instance.
        /// </summary>
        /// <param name="title">The adaptive card's title value.</param>
        /// <param name="imageUrl">The adaptive card's image URL.</param>
        /// <param name="summary">The adaptive card's summary value.</param>
        /// <param name="author">The adaptive card's author value.</param>
        /// <param name="buttonTitle">The adaptive card's button title value.</param>
        /// <param name="buttonUrl">The adaptive card's button url value.</param>
        /// <param name="buttons">The adaptive card's collection of buttons.</param>
        /// <param name="trackingurl">The adaptive card read tracking url.</param>
        /// <param name="cardimage">Image for the card when targeting is enabled.</param>
        /// <param name="cardtitle">Title for the card when targeting is enabled.</param>
        /// <param name="isAcknowledged">Acknowledge is enabled.</param>
        /// <param name="color">color for the background.</param>
        /// <param name="isImportant">Delivery option important.</param>
        /// <param name="notificationId">Notofication id of the message.</param>
        /// <param name="acknowledgeStatus">Acknowledgement status.</param>
        /// <returns>The created adaptive card instance.</returns>
        public AdaptiveCard CreateAdaptiveCard(
            string title,
            string imageUrl,
            string summary,
            string author,
            string buttonTitle,
            string buttonUrl,
            string buttons,
            string trackingurl,
            string cardimage,
            string cardtitle,
            bool isAcknowledged,
            string color,
            bool isImportant,
            string notificationId,
            bool acknowledgeStatus)
        {
            var version = new AdaptiveSchemaVersion(1, 4);
            AdaptiveCard card = new AdaptiveCard(version);
            var mainCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 4));
            var imageFile = string.Empty;
            Color colorCode = Color.White;
            var columns = new AdaptiveColumn
            {
                Separator = true,
            };

            if (isImportant)
            {
                columns.Items.Add(new AdaptiveTextBlock
                {
                    Text = "IMPORTANT!",
                    Wrap = true,
                    Color = AdaptiveTextColor.Attention,
                    Size = AdaptiveTextSize.Small,
                    Weight = AdaptiveTextWeight.Bolder,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                    Separator = false,
                });
            }

            if (!string.IsNullOrWhiteSpace(cardimage))
            {
                columns.Items.Add(new AdaptiveImage()
                {
                    Url = new Uri(cardimage, UriKind.RelativeOrAbsolute),
                });
            }

            if (!string.IsNullOrWhiteSpace(cardtitle))
            {
                columns.Items.Add(new AdaptiveTextBlock()
                {
                    Text = cardtitle,
                    Wrap = true,
                });
            }

            columns.Items.Add(new AdaptiveTextBlock()
            {
                Text = title,
                Size = AdaptiveTextSize.ExtraLarge,
                Weight = AdaptiveTextWeight.Bolder,
                Wrap = true,
                Separator = false,
            });

            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                columns.Items.Add(new AdaptiveImage()
                {
                    Url = new Uri(imageUrl, UriKind.RelativeOrAbsolute),
                    Spacing = AdaptiveSpacing.Default,
                    Size = AdaptiveImageSize.Stretch,
                    AltText = string.Empty,
                });
            }

            if (!string.IsNullOrWhiteSpace(summary))
            {
                columns.Items.Add(new AdaptiveTextBlock()
                {
                    Text = summary,
                    Wrap = true,
                });
            }

            if (!string.IsNullOrWhiteSpace(author))
            {
                columns.Items.Add(new AdaptiveTextBlock()
                {
                    Text = author,
                    Size = AdaptiveTextSize.Small,
                    Weight = AdaptiveTextWeight.Lighter,
                    Wrap = true,
                });
            }

            if (acknowledgeStatus)
            {
                columns.Items.Add(new AdaptiveTextBlock()
                {
                    Text = "Thank for your response. You have acknowledged this message",
                    Size = AdaptiveTextSize.Small,
                    Weight = AdaptiveTextWeight.Lighter,
                    Wrap = true,
                    Color = AdaptiveTextColor.Good,
                });
            }

            if (!string.IsNullOrWhiteSpace(trackingurl))
            {
                string trul = trackingurl + "/?id=[ID]&key=[KEY]";
                columns.Items.Add(new AdaptiveImage()
                {
                    Url = new Uri(trul, UriKind.RelativeOrAbsolute),
                    Spacing = AdaptiveSpacing.Small,
                    Size = AdaptiveImageSize.Small,
                    IsVisible = false,
                    AltText = string.Empty,
                });
            }

            var columnSet = new AdaptiveColumnSet();
            columnSet.Columns.Add(columns);

            if (color.Equals("White"))
            {
                colorCode = ColorTranslator.FromHtml("#ffffff");
            }
            else if (color.Equals("Gray"))
            {
                colorCode = ColorTranslator.FromHtml("#E5E5E5");
            }
            else if (color.Equals("Standard"))
            {
                colorCode = ColorTranslator.FromHtml("#E3E8E8");
            }
            else if (color.Equals("Medium Priority"))
            {
                colorCode = ColorTranslator.FromHtml("#F3E3BE");
            }
            else if (color.Equals("High Priority"))
            {
                colorCode = ColorTranslator.FromHtml("#DBA999");
            }

            var image = new Bitmap(50, 50);

            // Set each pixel in myBitmap to colorcode.
            for (int x = 0; x < image.Height; ++x)
            {
                for (int y = 0; y < image.Width; ++y)
                {
                    image.SetPixel(x, y, colorCode);
                }
            }

            for (int x = 0; x < image.Height; ++x)
            {
                image.SetPixel(x, x, colorCode);
            }

            using (MemoryStream ms = new MemoryStream())
            {
                // Convert Image to byte[]
                image.Save(ms, ImageFormat.Png);
                byte[] imageBytes = ms.ToArray();

                // Convert byte[] to Base64 String
                imageFile = "data:image/png;base64," + Convert.ToBase64String(imageBytes);
            }

            var container = new AdaptiveContainer();
            container.Style = AdaptiveContainerStyle.Emphasis;
            container.BackgroundImage = new Uri(imageFile);
            container.Items.Add(columnSet);

            mainCard.Body.Add(container);

            if (!string.IsNullOrWhiteSpace(buttonTitle)
                && !string.IsNullOrWhiteSpace(buttonUrl)
                && string.IsNullOrWhiteSpace(buttons))
            {
                mainCard.Actions.Add(new AdaptiveOpenUrlAction()
                {
                    Title = buttonTitle,
                    Url = new Uri(buttonUrl, UriKind.RelativeOrAbsolute),
                });
            }

            if (!string.IsNullOrWhiteSpace(buttonTitle)
                && !string.IsNullOrWhiteSpace(buttonUrl)
                && string.IsNullOrWhiteSpace(buttons))
            {
                mainCard.Actions.Add(new AdaptiveOpenUrlAction()
                {
                    Title = buttonTitle,
                    Url = new Uri(buttonUrl, UriKind.RelativeOrAbsolute),
                });
            }

            if (isAcknowledged)
            {
                if (!acknowledgeStatus)
                {
                    object obj = new ReadNotification(notificationId);
                    mainCard.Actions.Add(new AdaptiveExecuteAction()
                    {
                        Title = "Acknowledge",
                        Data = obj,
                        Verb = "userAcknowledge",
                    });
                }
            }

            if (!string.IsNullOrWhiteSpace(buttons))
            {
                // enables case insensitive deserialization for card buttons
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };

                // add the buttons string to the buttons collection for the card
                mainCard.Actions.AddRange(JsonSerializer.Deserialize<List<AdaptiveOpenUrlAction>>(buttons, options));
            }

            return mainCard;
        }
    }

    /// <summary>
    /// Get notification id.
    /// </summary>
    public class ReadNotification
    {
        public string action;

        public ReadNotification(string id)
        {
            this.action = id;
        }
    }
}
