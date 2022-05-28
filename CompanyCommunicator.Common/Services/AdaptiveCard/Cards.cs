using Microsoft.Bot.Schema;
using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.NotificationData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Teams.Apps.CompanyCommunicator.Common.Services.AdaptiveCard
{
    public class Cards
    {
        /// <summary>
        /// Creates an Video card.
        /// </summary>
        /// <param name="notificationDataEntity">Notification data entity.</param>
        /// <returns>An Video card.</returns>
        public virtual VideoCard GetVideoCard(NotificationDataEntity notificationDataEntity)
        {
            return this.GetVideoCard(
                notificationDataEntity.Title,
                notificationDataEntity.Summary);
        }

        /// <summary>
        /// Create an Video card instance.
        /// </summary>
        /// <param name="title">The adaptive card's title value.</param>
        /// <param name="imageUrl">The adaptive card's image URL.</param>
        /// <param name="summary">The adaptive card's summary value.</param>
        /// <returns>The created video card instance.</returns>
        public VideoCard GetVideoCard(
            string title,
            string summary)
        {
            var videoCard = new VideoCard
            {
                Title = title,
                Text = summary,
                Image = new ThumbnailUrl
                {
                    Url = "https://upload.wikimedia.org/wikipedia/commons/thumb/c/c5/Big_buck_bunny_poster_big.jpg/220px-Big_buck_bunny_poster_big.jpg",
                },
                Media = new List<MediaUrl>
                {
                    new MediaUrl()
                    {
                        Url = "http://download.blender.org/peach/bigbuckbunny_movies/BigBuckBunny_320x180.mp4",
                    },
                },
                Buttons = new List<CardAction>
                {
                    new CardAction()
                    {
                        Title = "Learn More",
                        Type = ActionTypes.OpenUrl,
                        Value = "https://peach.blender.org/",
                    },
                },
            };

            return videoCard;
        }
    }
}
