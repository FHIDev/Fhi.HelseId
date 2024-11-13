using Fhi.HelseId.Web.Hpr.Core;
using System.Collections.Generic;

namespace Fhi.HelseId.Web.Hpr
{
    public class HprPerson
    {
        public string HprNummer { get; set; } = "";
        public bool ErHprGodkjent { get; set; }
        public List<OId9060> HprGodkjenninger { get; set; } = new List<OId9060>();
    }
}