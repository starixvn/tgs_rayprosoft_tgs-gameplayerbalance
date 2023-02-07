using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Driver;
using NSubstitute;
using Tgs.GameFundTransfer.Protos;
using Tgs.GamePlayerBalance.Core;
using Tgs.GamePlayerBalance.Repositories.Models;

namespace Tgs.GamePlayerBalance.Repositories.UnitTest;

[TestFixture]
public class PlayerFormRepositoryUnitTest
{
    [Test]
    public async Task Add()
    {
        var fakeLogger = NullLogger<PlayerFormRepository>.Instance;
        var fakeMongoDatabase = Substitute.For<IMongoDatabase>();
        var fakerSnowflake = Substitute.For<ISnowflake>();

        var gameProviderId = Guid.NewGuid();
        var gameProviderCode = "KY";
        var brandId = Guid.NewGuid();
        var brandCode = "GPI";
        var playerCode = "4ucmqplxb029";
        var username = "4ucmqplxb029";
        var walletId = Guid.NewGuid();
        var currencyCode = "CNY";

        var playerFormId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        _ = fakerSnowflake.NextId().Returns(playerFormId);

        var repository = new PlayerFormRepository(
            fakeLogger,
            fakeMongoDatabase,
            fakerSnowflake);

        _ = await repository
            .AddAsync(
                gameProviderId: gameProviderId,
                gameProviderCode: gameProviderCode,
                brandId: brandId,
                brandCode: brandCode,
                playerCode: playerCode,
                username: username,
                walletId: walletId,
                currencyCode: currencyCode)
            .ConfigureAwait(false);

        var document = new PlayerFormModel()
        {
            Id = playerFormId,
            ExpansionFormId = playerFormId,
            GameProviderId = gameProviderId,
            GameProviderCode = gameProviderCode,
            BrandId = brandId,
            BrandCode = brandCode,
            PlayerCode = playerCode,
            Username = username,
            WalletId = walletId,
            CurrencyCode = currencyCode,
            State = PlayerFormState.Queued,
            Status = $"{PlayerFormState.Queued}...",
            CreatedOn = DateTime.UtcNow,
            UpdateOn = DateTime.UtcNow,
            ExpireOn = null
        };

        _ = fakeMongoDatabase.Received(1)
              .GetCollection<PlayerFormModel>(Arg.Is("player_forms"))
              .InsertOneAsync(Arg.Is(document));
    }

    public async Task Get()
    {
        var fakeLogger = NullLogger<PlayerFormRepository>.Instance;
        var fakeMongoDatabase = Substitute.For<IMongoDatabase>();
        var fakeSnowflake = Substitute.For<ISnowflake>();

        var repository = new PlayerFormRepository(
            fakeLogger,
            fakeMongoDatabase,
            fakeSnowflake);

        var id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        _ = await repository
            .GetAsync(
                id: id)
            .ConfigureAwait(false);

        var builder = Builders<PlayerFormModel>.Filter;
        var filter = builder.Eq(x => x.Id, id);
        var options = new FindOptions<PlayerFormModel>
        {
            Skip = 0,
            Limit = 1
        };

        _ = fakeMongoDatabase.Received(1)
            .GetCollection<PlayerFormModel>(Arg.Is($"player_forms"))
            .FindAsync(
                 filter: Arg.Is(filter),
                 options: Arg.Is(options))
            .ConfigureAwait(false);
    }

    [Test]
    public async Task UpdateComplete()
    {
        var fakeLogger = NullLogger<PlayerFormRepository>.Instance;
        var fakeMongoDatabase = Substitute.For<IMongoDatabase>();
        var fakeSnowflake = Substitute.For<ISnowflake>();

        var repository = new PlayerFormRepository(
            fakeLogger,
            fakeMongoDatabase,
            fakeSnowflake);

        var id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var resultStateCode = StateCode.Success;
        var resultMessage = StateCode.Success.ToString();
        var resultFundTransactionId = Guid.NewGuid();
        var resultTransactionId = Guid.NewGuid();
        var resultExternalTransactionId = Guid.NewGuid().ToString();
        var status = "success";
        await repository
           .UpdateCompleteAsync(
               id: id,
               resultStateCode: resultStateCode,
               resultMessage: resultMessage,
               resultFundTransactionId: resultFundTransactionId,
               resultTransactionId: resultTransactionId,
               resultExternalTransactionId: resultExternalTransactionId,
               status: status)
           .ConfigureAwait(false);

        var result = new PlayerFormFundResultModel
        {
            State = resultStateCode,
            Message = resultMessage,
            FundTransactionId = resultFundTransactionId,
            TransactionId = resultTransactionId,
            ExternalTransactionId = resultExternalTransactionId
        };

        var builder = Builders<PlayerFormModel>.Filter;
        var filter = builder.Eq(x => x.Id, id);
        var update = Builders<PlayerFormModel>.Update
            .Set(s => s.State, PlayerFormState.Complete)
            .Set(s => s.Status, status)
            .Set(s => s.Result, result)
            .Set(s => s.EndOn, DateTime.UtcNow)
            .Set(s => s.UpdateOn, DateTime.UtcNow)
            .Set(s => s.ExpireOn, DateTime.UtcNow.AddDays(7));

        _ = fakeMongoDatabase.Received(1)
            .GetCollection<PlayerFormModel>(Arg.Is("player_forms"))
            .UpdateOneAsync(
                 filter: Arg.Is(filter),
                update: Arg.Is(update))
            .ConfigureAwait(false);
    }

    [Test]
    public async Task UpdateFailedAsync()
    {
        var fakeLogger = NullLogger<PlayerFormRepository>.Instance;
        var fakeMongoDatabase = Substitute.For<IMongoDatabase>();
        var fakeSnowflake = Substitute.For<ISnowflake>();

        var repository = new PlayerFormRepository(
            fakeLogger,
            fakeMongoDatabase,
            fakeSnowflake);

        var id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var status = "fail";
        await repository
           .UpdateFailedAsync(
               id: id,
               status: status)
           .ConfigureAwait(false);

        var builder = Builders<PlayerFormModel>.Filter;
        var filter = builder.Eq(x => x.Id, id);
        var update = Builders<PlayerFormModel>.Update
            .Set(s => s.State, PlayerFormState.Failed)
            .Set(s => s.Status, status)
            .Set(s => s.EndOn, DateTime.UtcNow)
            .Set(s => s.UpdateOn, DateTime.UtcNow)
            .Set(s => s.ExpireOn, DateTime.UtcNow.AddDays(7));

        _ = fakeMongoDatabase.Received(1)
            .GetCollection<PlayerFormModel>(Arg.Is("player_forms"))
            .UpdateOneAsync(
                 filter: Arg.Is(filter),
                update: Arg.Is(update))
            .ConfigureAwait(false);
    }

    [Test]
    public async Task UpdateProcessing()
    {
        var fakeLogger = NullLogger<PlayerFormRepository>.Instance;
        var fakeMongoDatabase = Substitute.For<IMongoDatabase>();
        var fakeSnowflake = Substitute.For<ISnowflake>();

        var repository = new PlayerFormRepository(
            fakeLogger,
            fakeMongoDatabase,
            fakeSnowflake);

        var id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        await repository
           .UpdateProcessingAsync(
               id: id)
           .ConfigureAwait(false);

        var builder = Builders<PlayerFormModel>.Filter;
        var filter = builder.Eq(x => x.Id, id);
        var update = Builders<PlayerFormModel>.Update
            .Set(s => s.State, PlayerFormState.Processing)
            .Set(s => s.Status, $"{PlayerFormState.Processing}...")
            .Set(s => s.StartOn, DateTime.UtcNow)
            .Set(s => s.UpdateOn, DateTime.UtcNow);

        _ = fakeMongoDatabase.Received(1)
            .GetCollection<PlayerFormModel>(Arg.Is("player_forms"))
            .UpdateOneAsync(
                 filter: Arg.Is(filter),
                update: Arg.Is(update))
            .ConfigureAwait(false);
    }
}
