using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNCO.Unify {
    /// <summary>
    /// Allows Unify libraries to link into the Unify Runtime without Unify depending on those classes.
    /// </summary>
    public class RuntimeLink {
        public readonly IRuntime Instance;
        public RuntimeLink(IRuntime instance) => Instance = instance;
    }
}
