using Becas.Models;

namespace Becas.Data
{
    public interface ISolicitudBecaRepository
    {
        IList<SolicitudBeca> GetAll();
        void Add(SolicitudBeca solicitud);
        void Clear();
    }
}

