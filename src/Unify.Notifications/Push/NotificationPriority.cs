namespace CNCO.Unify.Notifications.Push {
    /// <summary>
    /// Push notification priority.
    /// </summary>
    public enum NotificationPriority {
        /// <summary>
        /// Unknown. Treated the same as <see cref="Default"/>.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Lowest priority possible.
        /// </summary>
        Minimum = 1,

        /// <summary>
        /// Low priority.
        /// </summary>
        Low = 2,

        /// <summary>
        /// No change, default priority.
        /// </summary>
        Default = 3,

        /// <summary>
        /// High priority.
        /// </summary>
        High = 4,

        /// <summary>
        /// Urgent notifications.
        /// </summary>
        Max = 5,
    }
}
