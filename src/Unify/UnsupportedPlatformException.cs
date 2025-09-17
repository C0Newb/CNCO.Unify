using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNCO.Unify {
    [Serializable]
    public class UnsupportedPlatformException : Exception {
        public UnsupportedPlatformException(string tag) : base($"{tag} is unsupported on this platform!") { }

        public UnsupportedPlatformException(string tag, string message) : base(message) { }
    }
}
