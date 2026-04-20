using Contracts;
using Contracts.IServices;

namespace Service;

public class PrescriptionService: IPrescriptionService
{
    private readonly IUnitOfWork unitOfWork;

    public PrescriptionService(IUnitOfWork unitOfWork )
    {
        this.unitOfWork = unitOfWork;
    }


}
