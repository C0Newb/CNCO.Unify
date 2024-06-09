namespace CNCO.Unify.Security {
    public static class UnifyRuntimeExtensions {
        /// <summary>
        /// Attach the <see cref="SecurityRuntime"/> to the <see cref="UnifyRuntime"/>.
        /// </summary>
        /// <param name="unifyRuntime">Instance to link to.</param>
        /// <returns>Runtime instance.</returns>
        public static UnifyRuntime UseSecurityRuntime(
            this UnifyRuntime unifyRuntime
        ) {
            return UseSecurityRuntime(unifyRuntime, new SecurityRuntimeConfiguration());
        }

        /// <inheritdoc cref="UseSecurityRuntime(UnifyRuntime)"/>
        /// <param name="securityRuntimeConfiguration">Runtime configuration to use.</param>
        public static UnifyRuntime UseSecurityRuntime(
            this UnifyRuntime unifyRuntime,
            SecurityRuntimeConfiguration securityRuntimeConfiguration
        ) {
            var securityRuntime = SecurityRuntime.Create(securityRuntimeConfiguration);
            unifyRuntime.AddRuntimeLink(securityRuntime);
            return unifyRuntime;
        }
    }
}
