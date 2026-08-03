using EnergyOptimizer.Core.DTOs.DeviceDTOs;
using EnergyOptimizer.Core.Enums;
using EnergyOptimizer.Core.Features.Devices.Commands;
using FluentAssertions;

namespace EnergyOptimizer.Tests.Validators
{
    public class DeviceCommandValidatorsTests
    {
        [Fact]
        public void CreateDeviceCommandValidator_InvalidData_ReturnsValidationErrors()
        {
            // Arrange
            var validator = new CreateDeviceCommandValidator();
            var invalidDto = new CreateDeviceDto
            {
                Name = "", 
                ZoneId = 0,
                RatedPowerKW = 0.00m,
                Type = DeviceType.AirConditioner
            };
            var command = new CreateDeviceCommand(invalidDto);

            // Act
            var result = validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName.Contains("Name"));
            result.Errors.Should().Contain(e => e.PropertyName.Contains("ZoneId"));
            result.Errors.Should().Contain(e => e.PropertyName.Contains("RatedPowerKW"));
        }

        [Fact]
        public void CreateDeviceCommandValidator_ValidData_PassesValidation()
        {
            // Arrange
            var validator = new CreateDeviceCommandValidator();
            var validDto = new CreateDeviceDto
            {
                Name = "Master AC",
                ZoneId = 1,
                RatedPowerKW = 2.5m,
                Type = DeviceType.AirConditioner
            };
            var command = new CreateDeviceCommand(validDto);

            // Act
            var result = validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void UpdateDeviceCommandValidator_InvalidData_ReturnsValidationErrors()
        {
            // Arrange
            var validator = new UpdateDeviceCommandValidator();
            var invalidDto = new UpdateDeviceDto
            {
                Name = "",
                RatedPowerKW = -5.0m
            };
            var command = new UpdateDeviceCommand(0, invalidDto);

            // Act
            var result = validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "id");
            result.Errors.Should().Contain(e => e.PropertyName.Contains("Name"));
            result.Errors.Should().Contain(e => e.PropertyName.Contains("RatedPowerKW"));
        }

        [Fact]
        public void UpdateDeviceCommandValidator_ValidData_PassesValidation()
        {
            // Arrange
            var validator = new UpdateDeviceCommandValidator();
            var validDto = new UpdateDeviceDto
            {
                Name = "Kitchen Microwave",
                RatedPowerKW = 1.2m
            };
            var command = new UpdateDeviceCommand(5, validDto);

            // Act
            var result = validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }
    }
}
