
using System.Runtime.CompilerServices;

using AutoMapper;

using DotNetTemplateClean.Application;
using DotNetTemplateClean.Domain;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using NUnit.Framework;

namespace DotNetTemplateClean.UnitTest;


public class MappingTests
{
    private ILoggerFactory? _loggerFactory;
    private MapperConfiguration? _configuration;
    private IMapper? _mapper;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Minimal logger factory for tests
        _loggerFactory = LoggerFactory.Create(b => b.AddDebug().SetMinimumLevel(LogLevel.Debug));

        _configuration = new MapperConfiguration(cfg =>
            cfg.AddMaps(typeof(IApplicationDbContext).Assembly),
            loggerFactory: _loggerFactory);

        _mapper = _configuration.CreateMapper();
    }

    [Test]
    public void ShouldHaveValidConfiguration() => _configuration!.AssertConfigurationIsValid();

    [Test]
    [TestCase(typeof(Entite), typeof(OrganizationUnitResponseDto))]
    [TestCase(typeof(OrganizationUnitSaveDto), typeof(Entite))]   
    public void ShouldSupportMappingFromSourceToDestination(Type source, Type destination)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));

        var instance = GetInstanceOf(source);

        var result = _mapper!.Map(instance, source, destination);

        result.Should().NotBeNull();
        result.Should().BeOfType(destination);
    }

  
    private static object GetInstanceOf(Type type)
    {
        if (type.GetConstructor(Type.EmptyTypes) != null)
            return Activator.CreateInstance(type)!;

        // Type without parameterless constructor
        return RuntimeHelpers.GetUninitializedObject(type);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown() => _loggerFactory?.Dispose();
}
