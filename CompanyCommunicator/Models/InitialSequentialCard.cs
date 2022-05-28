// <copyright file="UserTeamsActivityHandler.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// </copyright>

using System.Collections.Generic;

namespace Microsoft.Teams.Apps.CompanyCommunicator.Bot
{
    public class InitialSequentialCard
    {
        public Action action { get; set; }
        public string trigger { get; set; }
    }

    /// <summary>
    /// Action model class.
    /// </summary>
    public class Action
    {
        public string type { get; set; }
        public string title { get; set; }
        public Data data { get; set; }
        public string verb { get; set; }
    }

    /// <summary>
    /// Data model class.
    /// </summary>
    public class Data
    {
        public string action { get; set; }
    }
}