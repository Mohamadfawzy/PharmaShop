using Contracts;
using Entities.Models;

namespace Repository;

public class PrescriptionRepository : GenericRepository<Prescription>, IPrescriptionRepository
{
    private readonly RepositoryContext context;

    public PrescriptionRepository(RepositoryContext context) : base(context)
    {
        this.context = context;
    }

}