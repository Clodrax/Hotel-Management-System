using Hotel.DAL;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly AppDbContext _context;

        public EmployeeService(AppDbContext context)
        {
            _context = context;
        }

        public async Task CheckAndUpdateSalaryStatus()
        {
            var employees = await _context.Employers.Where(e => e.IsPaid && e.LastSalaryPaidDate.HasValue).ToListAsync();
            foreach (var employee in employees)
            {
                if (employee.LastSalaryPaidDate.Value.AddMonths(1) <= DateTime.UtcNow)
                {
                    employee.IsPaid = false;
                }
            }
            await _context.SaveChangesAsync();
        }
    }
}
