using Contracts;
using Contracts.IServices;
using FluentValidation;
using Shared.Models.Dtos.Promotion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork unitOfWork;

    public OrderService(IUnitOfWork unitOfWork)
    {
        this.unitOfWork = unitOfWork;
    }


}
