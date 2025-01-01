using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ParkingSystem.Data;
using ParkingSystem.Helpers;
using ParkingSystem.Services;
namespace TestForParkingSystem.Tests
{
    public abstract class TestBase : IDisposable
    {
        protected readonly AppDbContext _context;
        protected readonly IMapper _mapper;
        protected readonly Mock<ILogger<ReservationService>> _loggerMock;
        protected readonly Mock<IQRCodeService> _qrCodeServiceMock;

        protected TestBase()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);

            // Setup AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AutoMapperProfile>();
            });
            _mapper = config.CreateMapper();

            // Setup mocks
            _loggerMock = new Mock<ILogger<ReservationService>>();
            _qrCodeServiceMock = new Mock<IQRCodeService>();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
