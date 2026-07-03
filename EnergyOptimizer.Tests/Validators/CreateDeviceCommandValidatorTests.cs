using EnergyOptimizer.Core.DTOs.DeviceDTOs;
using EnergyOptimizer.Core.Features.AI.Commands.DevicesCommans;
using FluentAssertions;

namespace EnergyOptimizer.Tests.Validators
{
    public class CreateDeviceCommandValidatorTests
    {
        private readonly CreateDeviceCommandValidator _validator;

        public CreateDeviceCommandValidatorTests()
        {
            _validator = new CreateDeviceCommandValidator();
        }

        [Fact]
        public void Validate_ValidCommand_ReturnsTrue()
        {
            // Arrange
            var dto = new CreateDeviceDto { Name = "Valid Device Name", ZoneId = 1, RatedPowerKW = 1.5m };
            var command = new CreateDeviceCommand(dto);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_EmptyName_ReturnsFalse()
        {
            // Arrange
            var dto = new CreateDeviceDto { Name = "", ZoneId = 1, RatedPowerKW = 1.5m };
            var command = new CreateDeviceCommand(dto);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Dto.Name" && e.ErrorMessage == "Device name is required.");
        }

        [Fact]
        public void Validate_InvalidZoneId_ReturnsFalse()
        {
            // Arrange
            var dto = new CreateDeviceDto { Name = "Valid Name", ZoneId = 0, RatedPowerKW = 1.5m };
            var command = new CreateDeviceCommand(dto);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Dto.ZoneId" && e.ErrorMessage == "A valid Zone ID is required.");
        }
    }
}
