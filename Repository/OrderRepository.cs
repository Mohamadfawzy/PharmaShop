using Contracts;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository;

public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    private readonly RepositoryContext _db;

    public OrderRepository(RepositoryContext db) : base(db)
    {
        _db = db;
    }


}
