// <copyright file="TeamData.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// </copyright>

namespace Microsoft.Teams.Apps.CompanyCommunicator.Prep.Func.Export.Model
{
    /// <summary>
    /// the model class for team data.
    /// </summary>
    public class TeamData
    {
        /// <summary>
        /// Gets or sets the team id value.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the team id value.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the delivery status value.
        /// </summary>
        public string DeliveryStatus { get; set; }

        /// <summary>
        /// Gets or sets the status reason value.
        /// </summary>
        public string StatusReason { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether user has read the message.
        /// </summary>
        public bool ReadStatus { get; set; }
    }
}
