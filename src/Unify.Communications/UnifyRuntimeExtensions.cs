namespace CNCO.Unify.Communications {
    public static class UnifyRuntimeExtensions {
        /// <summary>
        /// Attach the <see cref="CommunicationsRuntime"/> to the <see cref="UnifyRuntime"/>.
        /// </summary>
        /// <param name="unifyRuntime">Instance to link to.</param>
        /// <returns>Runtime instance.</returns>
        public static UnifyRuntime UseCommunicationsRuntime(
            this UnifyRuntime unifyRuntime
        ) {
            return UseCommunicationsRuntime(unifyRuntime, new CommunicationsRuntimeConfiguration());
        }

        /// <inheritdoc cref="UseCommunicationsRuntime(UnifyRuntime)"/>
        /// <param name="communicationsRuntimeConfiguration">Runtime configuration to use.</param>
        public static UnifyRuntime UseCommunicationsRuntime(
            this UnifyRuntime unifyRuntime,
            CommunicationsRuntimeConfiguration communicationsRuntimeConfiguration
        ) {
            var communicationsRuntime = CommunicationsRuntime.Create(communicationsRuntimeConfiguration);
            unifyRuntime.AddRuntimeLink(communicationsRuntime);
            return unifyRuntime;
        }
    }
}
