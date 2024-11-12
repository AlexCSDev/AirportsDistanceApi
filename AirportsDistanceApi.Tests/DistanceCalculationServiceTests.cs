using AirportsDistanceApi.Exceptions.DistanceCalculationService;
using AirportsDistanceApi.Exceptions.SimpleRestApiClient;
using AirportsDistanceApi.Interfaces.Services;
using AirportsDistanceApi.Models.Dto.PlacesApi;
using AirportsDistanceApi.Services;
using Microsoft.Extensions.Logging;
using Moq;
using StackExchange.Redis;

namespace AirportsDistanceApi.Tests;

public class DistanceCalculationServiceTests
{
    [Fact]
    public async Task SuccessfulExecution_ReturnsValidValue()
    {
        var redisDbMock = new Mock<IDatabase>();
        redisDbMock.Setup(m => m.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);
        redisDbMock.Setup(m => m.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(),
            It.IsAny<TimeSpan?>(), It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        var multiplexerMock = new Mock<IConnectionMultiplexer>();
        multiplexerMock.Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(() => redisDbMock.Object);

        var restClientMock = new Mock<ISimpleRestApiClientService>();
        restClientMock.Setup(m => m.GetAsync<AirportDataDto>(It.Is<string>(s => s.EndsWith("DME"))))
            .ReturnsAsync(new AirportDataDto { Location = new AirportDataDto.LocationObject { Latitude = (double)55.414566, Longitude = (double)37.899494 } });
        restClientMock.Setup(m => m.GetAsync<AirportDataDto>(It.Is<string>(s => s.EndsWith("VKO"))))
            .ReturnsAsync(new AirportDataDto { Location = new AirportDataDto.LocationObject { Latitude = (double)55.60315, Longitude = (double)37.292098 } });

        IDistanceCalculationService service = new DistanceCalculationService(
            restClientMock.Object,
            multiplexerMock.Object,
            new Mock<ILogger<DistanceCalculationService>>().Object
            );

        var retVal = await service.GetDistanceBetweenTwoAirportsAsync("DME", "VKO");

        Assert.Equal((double)27.125932088178207, retVal);
    }

    [Fact]
    public async Task InvalidAirportACodeSupplied_ThrowsInvalidAirportCodeException()
    {
        IDistanceCalculationService service = new DistanceCalculationService(
            new Mock<ISimpleRestApiClientService>().Object,
            new Mock<IConnectionMultiplexer>().Object,
            new Mock<ILogger<DistanceCalculationService>>().Object
            );

        var exception = await Assert.ThrowsAsync<InvalidAirportCodeException>(async () =>
        {
            await service.GetDistanceBetweenTwoAirportsAsync("ZZ1", "DME");
        });

        Assert.Equal("\"ZZ1\" is not a valid IATA code", exception.Message);
    }

    [Fact]
    public async Task InvalidAirportBCodeSupplied_ThrowsInvalidAirportCodeException()
    {
        IDistanceCalculationService service = new DistanceCalculationService(
            new Mock<ISimpleRestApiClientService>().Object,
            new Mock<IConnectionMultiplexer>().Object,
            new Mock<ILogger<DistanceCalculationService>>().Object
            );

        var exception = await Assert.ThrowsAsync<InvalidAirportCodeException>(async () =>
        {
            await service.GetDistanceBetweenTwoAirportsAsync("DME", "ZZ2");
        });

        Assert.Equal("\"ZZ2\" is not a valid IATA code", exception.Message);
    }

    [Fact]
    public async Task GetCacheNotAvailable_ThrowsCacheAccessException()
    {
        var redisDbMock = new Mock<IDatabase>();
        redisDbMock.Setup(m => m.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.SocketFailure, "Connection failed"));

        var multiplexerMock = new Mock<IConnectionMultiplexer>();
        multiplexerMock.Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(() => redisDbMock.Object);

        IDistanceCalculationService service = new DistanceCalculationService(
            new Mock<ISimpleRestApiClientService>().Object,
            multiplexerMock.Object,
            new Mock<ILogger<DistanceCalculationService>>().Object
            );

        var exception = await Assert.ThrowsAsync<CacheAccessException>(async () =>
        {
            await service.GetDistanceBetweenTwoAirportsAsync("DME", "VKO");
        });

        Assert.Equal("Error while accessing cache: Connection failed", exception.Message);
    }

    [Fact]
    public async Task CacheContainsValue_ReturnsCachedValue()
    {
        var redisDbMock = new Mock<IDatabase>();
        redisDbMock.Setup(m => m.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue("27,125932088178207"));

        var multiplexerMock = new Mock<IConnectionMultiplexer>();
        multiplexerMock.Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(() => redisDbMock.Object);

        IDistanceCalculationService service = new DistanceCalculationService(
            new Mock<ISimpleRestApiClientService>().Object,
            multiplexerMock.Object,
            new Mock<ILogger<DistanceCalculationService>>().Object
            );

        var retVal = await service.GetDistanceBetweenTwoAirportsAsync("DME", "VKO");

        Assert.Equal((double)27.125932088178207, retVal);
    }

    [Fact]
    public async Task AirportDataDtoIsNull_ThrowsDataRetrievalException()
    {
        var redisDbMock = new Mock<IDatabase>();
        redisDbMock.Setup(m => m.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        var multiplexerMock = new Mock<IConnectionMultiplexer>();
        multiplexerMock.Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(() => redisDbMock.Object);

        var restClientMock = new Mock<ISimpleRestApiClientService>();
        restClientMock.Setup(m => m.GetAsync<AirportDataDto>(It.IsAny<string>()))
            .ReturnsAsync(() => null);

        IDistanceCalculationService service = new DistanceCalculationService(
            restClientMock.Object,
            multiplexerMock.Object,
            new Mock<ILogger<DistanceCalculationService>>().Object
            );

        var exception = await Assert.ThrowsAsync<AggregateException>(async () =>
        {
            await service.GetDistanceBetweenTwoAirportsAsync("DME", "VKO");
        });

        Assert.Equal(2, exception.InnerExceptions.Count);
        foreach(var innerEx in exception.InnerExceptions)
        {
            Assert.IsType<DataRetrievalException>(innerEx);
            Assert.StartsWith("Received empty response for airport ", innerEx.Message);
        }
    }

    [Fact]
    public async Task AirportDataDtoLocationIsNull_ThrowsDataRetrievalException()
    {
        var redisDbMock = new Mock<IDatabase>();
        redisDbMock.Setup(m => m.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        var multiplexerMock = new Mock<IConnectionMultiplexer>();
        multiplexerMock.Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(() => redisDbMock.Object);

        var restClientMock = new Mock<ISimpleRestApiClientService>();
        restClientMock.Setup(m => m.GetAsync<AirportDataDto>(It.Is<string>(s => s.EndsWith("DME"))))
            .ReturnsAsync(new AirportDataDto { Location = null! });
        restClientMock.Setup(m => m.GetAsync<AirportDataDto>(It.Is<string>(s => s.EndsWith("VKO"))))
            .ReturnsAsync(new AirportDataDto { Location = null! });

        IDistanceCalculationService service = new DistanceCalculationService(
            restClientMock.Object,
            multiplexerMock.Object,
            new Mock<ILogger<DistanceCalculationService>>().Object
            );

        var exception = await Assert.ThrowsAsync<AggregateException>(async () =>
        {
            await service.GetDistanceBetweenTwoAirportsAsync("DME", "VKO");
        });

        Assert.Equal(2, exception.InnerExceptions.Count);
        foreach (var innerEx in exception.InnerExceptions)
        {
            Assert.IsType<DataRetrievalException>(innerEx);
            Assert.StartsWith("Received location data is invalid for airport ", innerEx.Message);
        }
    }

    [Fact]
    public async Task AirportNotFound_ThrowsInvalidAirportCodeException()
    {
        var redisDbMock = new Mock<IDatabase>();
        redisDbMock.Setup(m => m.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        var multiplexerMock = new Mock<IConnectionMultiplexer>();
        multiplexerMock.Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(() => redisDbMock.Object);

        var restClientMock = new Mock<ISimpleRestApiClientService>();
        restClientMock.Setup(m => m.GetAsync<AirportDataDto>(It.IsAny<string>()))
            .ThrowsAsync(new UnsuccessfulRequestException(System.Net.HttpStatusCode.NotFound, "{}"));

        IDistanceCalculationService service = new DistanceCalculationService(
            restClientMock.Object,
            multiplexerMock.Object,
            new Mock<ILogger<DistanceCalculationService>>().Object
            );

        var exception = await Assert.ThrowsAsync<AggregateException>(async () =>
        {
            await service.GetDistanceBetweenTwoAirportsAsync("DME", "VKO");
        });

        Assert.Equal(2, exception.InnerExceptions.Count);
        foreach (var innerEx in exception.InnerExceptions)
        {
            Assert.IsType<InvalidAirportCodeException>(innerEx);
        }
    }

    [Fact]
    public async Task ExternalServiceNonSuccessfulErrorCode_ThrowsDataRetrievalException()
    {
        var redisDbMock = new Mock<IDatabase>();
        redisDbMock.Setup(m => m.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        var multiplexerMock = new Mock<IConnectionMultiplexer>();
        multiplexerMock.Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(() => redisDbMock.Object);

        var restClientMock = new Mock<ISimpleRestApiClientService>();
        restClientMock.Setup(m => m.GetAsync<AirportDataDto>(It.IsAny<string>()))
            .ThrowsAsync(new UnsuccessfulRequestException(System.Net.HttpStatusCode.BadRequest, "{}"));

        IDistanceCalculationService service = new DistanceCalculationService(
            restClientMock.Object,
            multiplexerMock.Object,
            new Mock<ILogger<DistanceCalculationService>>().Object
            );

        var exception = await Assert.ThrowsAsync<AggregateException>(async () =>
        {
            await service.GetDistanceBetweenTwoAirportsAsync("DME", "VKO");
        });

        Assert.Equal(2, exception.InnerExceptions.Count);
        foreach (var innerEx in exception.InnerExceptions)
        {
            Assert.IsType<DataRetrievalException>(innerEx);
            Assert.StartsWith("Error while retrieving data for airport ", innerEx.Message);
        }
    }

    [Fact]
    public async Task SetCacheNotAvailable_ThrowsCacheAccessException()
    {
        var redisDbMock = new Mock<IDatabase>();
        redisDbMock.Setup(m => m.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        redisDbMock.Setup(m => m.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(),
            It.IsAny<TimeSpan?>(), It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
            .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.SocketFailure, "Connection failed"));

        var multiplexerMock = new Mock<IConnectionMultiplexer>();
        multiplexerMock.Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(() => redisDbMock.Object);

        var restClientMock = new Mock<ISimpleRestApiClientService>();
        restClientMock.Setup(m => m.GetAsync<AirportDataDto>(It.Is<string>(s => s.EndsWith("DME"))))
            .ReturnsAsync(new AirportDataDto { Location = new AirportDataDto.LocationObject { Latitude = (double)55.414566, Longitude = (double)37.899494 } });
        restClientMock.Setup(m => m.GetAsync<AirportDataDto>(It.Is<string>(s => s.EndsWith("VKO"))))
            .ReturnsAsync(new AirportDataDto { Location = new AirportDataDto.LocationObject { Latitude = (double)55.60315, Longitude = (double)37.292098 } });

        IDistanceCalculationService service = new DistanceCalculationService(
            restClientMock.Object,
            multiplexerMock.Object,
            new Mock<ILogger<DistanceCalculationService>>().Object
            );

        var exception = await Assert.ThrowsAsync<CacheAccessException>(async () =>
        {
            await service.GetDistanceBetweenTwoAirportsAsync("DME", "VKO");
        });

        Assert.Equal("Error while accessing cache: Connection failed", exception.Message);
    }
}