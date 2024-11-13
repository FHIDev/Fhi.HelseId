using System.Collections.Generic;
using System.Linq;
using Fhi.HelseId.Web.Hpr.Core;
using Fhi.HelseId.Web.Services;

namespace Fhi.HelseId.Web.Hpr
{
    public interface IHprService
    {
        bool SjekkGodkjenning();
        bool ErGyldigForKategorier(params OId9060[] koder);
        IEnumerable<OId9060> HentGodkjenninger();

        IHprService LeggTilGodkjenteHelsepersonellkategori(OId9060 ny);
        IHprService LeggTilGodkjenteHelsepersonellkategorier(IGodkjenteHprKategoriListe liste);
        IHprService LeggTilAlleKategorier();

        List<OId9060> GodkjenteHelsepersonellkategorier { get; }
    }

    public class HprService : IHprService
    {
        private readonly ICurrentUser _currentUser;

        public List<OId9060> GodkjenteHelsepersonellkategorier { get; }

        public HprService(ICurrentUser currentUser)
        {
            GodkjenteHelsepersonellkategorier = new List<OId9060>();
            _currentUser = currentUser;
        }

        public bool SjekkGodkjenning()
        {
            var filteredGodkjenninger = HentGodkjenninger();

            return filteredGodkjenninger.Any();
        }

        public bool ErGyldigForKategorier(params OId9060[] koder)
        {
            var filteredGodkjenninger = FilterGodkjenninger(koder);

            return filteredGodkjenninger.Any();
        }

        public IEnumerable<OId9060> HentGodkjenninger()
        {
            var filteredGodkjenninger = FilterGodkjenninger(GodkjenteHelsepersonellkategorier.ToArray());

            return filteredGodkjenninger;
        }

        private IEnumerable<OId9060> FilterGodkjenninger(OId9060[] koder)
        {
            var person = HentPerson();
            return person.HprGodkjenninger
                .Where(personGodkjenninger => koder
                    .FirstOrDefault(systemGodkjenninger => systemGodkjenninger.Value == personGodkjenninger.Value) != null);
        }

        public IHprService LeggTilGodkjenteHelsepersonellkategori(OId9060 ny)
        {
            GodkjenteHelsepersonellkategorier.Add(ny);
            return this;
        }

        public IHprService LeggTilGodkjenteHelsepersonellkategorier(IGodkjenteHprKategoriListe liste)
        {
            LeggTilGodkjenteHelsepersonellKategoriListe(liste.Godkjenninger);
            return this;
        }

        private IHprService LeggTilGodkjenteHelsepersonellKategoriListe(IEnumerable<OId9060> liste)
        {
            foreach (var godkjent in liste)
                LeggTilGodkjenteHelsepersonellkategori(godkjent);
            return this;
        }


        public IHprService LeggTilAlleKategorier()
        {
            LeggTilGodkjenteHelsepersonellKategoriListe(Kodekonstanter.KodeList);
            return this;
        }

        private HprPerson HentPerson()
        {
            var person = new HprPerson()
            {
                HprNummer = _currentUser.HprNummer ?? string.Empty,
                ErHprGodkjent = _currentUser.ErHprGodkjent,
                HprGodkjenninger = _currentUser.HprGodkjenninger
            };

            return person;
        }
    }
}