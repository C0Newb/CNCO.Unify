namespace CNCO.Unify.Security {
    public class DataProtectionProvider : IDataProtectionProvider {
        public virtual IDataProtector CreateProtector(string purpose) {
            var dataProtector = Activator.CreateInstance(GetType(), purpose) as IDataProtector;
            return dataProtector ?? throw new ArgumentException($"Unable to create DataProtector.");
        }

        /// <inheritdoc cref="CreateProtector(string)"/>
        /// <param name="provider">The <see cref="IDataProtectionProvider"/> from which to generate the purpose chain.</param>
        public static IDataProtector CreateProtector(IDataProtectionProvider provider, string purpose) {
            return provider.CreateProtector(purpose);
        }


        public IDataProtector CreateProtector(IEnumerable<string> purposes) {

            if (purposes == null)
                throw new ArgumentNullException(nameof(purposes));

            IDataProtectionProvider dataProtector = this;
            bool createdProvider = false;
            foreach (var purpose in purposes) {
                if (string.IsNullOrEmpty(purpose))
                    continue;
                dataProtector = dataProtector.CreateProtector(purpose);
                createdProvider = true;
            }
            if (!createdProvider)
                throw new ArgumentException("Purposes either all null or empty, unable to create DataProtector.");

            return (IDataProtector)dataProtector ?? throw new ArgumentException($"Unable to create DataProtector.");
        }

        /// <inheritdoc cref="CreateProtector(IEnumerable{string})"/>
        /// <param name="provider">The <see cref="IDataProtectionProvider"/> from which to generate the purpose chain.</param>
        public static IDataProtector CreateProtector(IDataProtectionProvider provider, IEnumerable<string> purposes) {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));
            return provider.CreateProtector(purposes);
        }


        public IDataProtector CreateProtector(string purpose, params string[] subPurposes) {
            if (purpose == null)
                throw new ArgumentNullException(nameof(purpose));

            IDataProtector? dataProtector = CreateProtector(purpose);
            if (subPurposes != null && subPurposes.Length > 0)
                dataProtector = dataProtector?.CreateProtector(subPurposes);

            return dataProtector ?? throw new ArgumentException($"Unable to create DataProtector.");
        }

        /// <inheritdoc cref="CreateProtector(string, string[])"/>
        /// <param name="provider">The <see cref="IDataProtectionProvider"/> from which to generate the purpose chain.</param>
        public static IDataProtector CreateProtector(IDataProtectionProvider provider, string purpose, params string[] subPurposes) {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));
            return provider.CreateProtector(purpose, subPurposes);
        }
    }
}
