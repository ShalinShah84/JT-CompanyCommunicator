// <copyright file="UserData.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// </copyright>

namespace Microsoft.Teams.Apps.CompanyCommunicator.Prep.Func.Export.Model
{
    using System;

    /// <summary>
    /// the model class for user data.
    /// </summary>
    public class UserData
    {
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the user principal name.
        /// </summary>
        public string Upn { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the user type.
        /// </summary>
        public string UserType { get; set; }

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

        /// <summary>
        /// Gets or sets a value indicating whether user has read the message.
        /// </summary>
        public bool AcknowledgeStatus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating When user has read the message.
        /// </summary>
        public DateTime? AcknowledgedDate { get; set; }
    }
}