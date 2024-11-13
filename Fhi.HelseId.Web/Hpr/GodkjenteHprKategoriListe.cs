using System.Collections.Generic;
using Fhi.HelseId.Web.Hpr.Core;

namespace Fhi.HelseId.Web.Hpr
{
    public interface IGodkjenteHprKategoriListe
    {
        IEnumerable<OId9060> Godkjenninger { get; }
    }

    /// <summary>
    /// Lag subklasse og sett opp for injection
    /// Legg til i denne listen de kategoriene som skal være godkjent
    /// Mulige verdier finnes i filen Kodekonstaner.g.cs
    /// Eks.: Verdi for Lege er:  Kodekonstanter.OId9060Lege
    /// </summary>
    public class GodkjenteHprKategoriListe : IGodkjenteHprKategoriListe
    {
        private readonly List<OId9060> godkjenninger = new();

        public void Add(OId9060 godkjent) => godkjenninger.Add(godkjent);
        public void AddRange(IEnumerable<OId9060> godkjente) => godkjenninger.AddRange(godkjente);
        public IEnumerable<OId9060> Godkjenninger => godkjenninger;
    }
}