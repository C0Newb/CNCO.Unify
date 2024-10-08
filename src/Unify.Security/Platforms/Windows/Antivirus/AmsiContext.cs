﻿using CNCO.Unify.Security.Platforms.Windows.Antivirus.Internals;
using System.ComponentModel;

namespace CNCO.Unify.Security.Platforms.Windows.Antivirus {
    public class AmsiContext {
        private readonly AmsiContextSafeHandle _context;

        private AmsiContext(AmsiContextSafeHandle context) => _context = context;

        public static AmsiContext Create(string applicationName) {
            int result = Amsi.AmsiInitialize(applicationName, out var context);
            if (result != 0)
                throw new Win32Exception(result);

            return new AmsiContext(context);
        }

        public AmsiSession CreateSession() {
            var result = Amsi.AmsiOpenSession(_context, out var session);
            session.Context = _context;
            if (result != 0)
                throw new Win32Exception(result);

            return new AmsiSession(_context, session);
        }

        public void Dispose() {
            _context.Dispose();
        }
    }
}
