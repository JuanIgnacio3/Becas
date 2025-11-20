using Becas.Models;
using Becas.Utils;

namespace Becas.Data
{
    public class SessionSolicitudBecaRepository : ISolicitudBecaRepository
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string SessionKey = "SolicitudesBeca";

        public SessionSolicitudBecaRepository(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ISession Session => _httpContextAccessor.HttpContext!.Session;

        public IList<SolicitudBeca> GetAll()
        {
            var list = Session.GetObject<List<SolicitudBeca>>(SessionKey);
            return list ?? new List<SolicitudBeca>();
        }

        public void Add(SolicitudBeca solicitud)
        {
            var list = GetAll().ToList();
            list.Add(solicitud);
            Session.SetObject(SessionKey, list);
        }

        public void Clear()
        {
            Session.Remove(SessionKey);
        }
    }
}

