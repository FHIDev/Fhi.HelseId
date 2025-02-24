using System;
using System.Collections.Generic;
using System.Linq;
using Fhi.HelseId.Common.Identity;

namespace Fhi.HelseId.Common.Configuration
{
    public interface IWhitelist
    {
        bool IsWhite(string pidPseudonym);
        string NameOf(string pidPseudonym);
    }

    public class Whitelist : List<White>, IWhitelist
    {
        public bool IsWhite(string pidPseudonym) => this.Any(x => x.PidPseudonym.Equals(pidPseudonym, StringComparison.InvariantCultureIgnoreCase));

        public string NameOf(string pidPseudonym)
        {
            var result = this.FirstOrDefault(x =>
                x.PidPseudonym.Equals(pidPseudonym, StringComparison.InvariantCultureIgnoreCase));
            return result?.Name ?? "";
        }
    }
}
