namespace CNCO.Unify.Security {
    /// <summary>
    /// An interface that can be used to create <see cref="IDataProtector"/> instances.
    /// </summary>
    public interface IDataProtectionProvider {
        /// <summary>
        /// Creates an <see cref="IDataProtector"/> given a purpose.
        /// </summary>
        /// <remarks>
        /// The <paramref name="purpose"/> parameter must be unique for the intended use case;
        /// two different <see cref="IDataProtector"/> instances created with two different <paramref name="purpose"/>
        /// values will not be able to decipher each other's payloads. The <paramref name="purpose"/> parameter value is
        /// not intended to be kept secret.
        /// </remarks>
        /// <param name="purpose">The purpose to be assigned to the newly-created <see cref="IDataProtector"/>.</param>
        /// <returns>An IDataProtector tied to the provided purpose.</returns>
        IDataProtector CreateProtector(string purpose);



        /// <summary>
        /// Creates an <see cref="IDataProtector"/> given a list of purposes.
        /// </summary>
        /// <param name="purposes">An optional list of secondary purposes which contribute to the purpose chain.
        /// If this list is provided it cannot contain null elements.</param>
        /// <returns>An <see cref="IDataProtector"/> tied to the provided purpose chain.</returns>
        /// <remarks>
        /// This is a convenience method which chains together several calls to
        /// <see cref="CreateProtector(string)"/>. See that method's
        /// documentation for more information.
        /// </remarks>
        IDataProtector CreateProtector(IEnumerable<string> purposes);



        /// <summary>
        /// Creates an <see cref="IDataProtector"/> given a list of purposes.
        /// </summary>
        /// <param name="purpose">The primary purpose used to create the <see cref="IDataProtector"/>.</param>
        /// <param name="subPurposes">The list of purposes which contribute to the purpose chain. This list must
        /// contain at least one element, and it may not contain null elements.</param>
        /// <returns>An <see cref="IDataProtector"/> tied to the provided purpose chain.</returns>
        /// <remarks>
        /// This is a convenience method which chains together several calls to
        /// <see cref="CreateProtector(string)"/>. See that method's
        /// documentation for more information.
        /// </remarks>
        IDataProtector CreateProtector(string purpose, params string[] subPurposes);
    }
}
