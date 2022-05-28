using System;

namespace Microsoft.Teams.Apps.CompanyCommunicator.Models
{
    /// <summary>
    /// Tracking user clicks.
    /// </summary>
    public class TrackingUserClicks
    {
        /// <summary>
        /// Gets or sets name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets clicks.
        /// </summary>
        public int Clicks { get; set; }

        /// <summary>
        /// Gets or sets datetime.
        /// </summary>
        public DateTime DateTime { get; set; }
    }
}
