using MongoDB.Driver;
using NSubstitute;
using Tgs.GamePlayerBalance.Core;
using Tgs.GamePlayerBalance.Repositories.Models;

namespace Tgs.GamePlayerBalance.Repositories.UnitTest;

[TestFixture]
public class BrandFormRepositoryUnitTest
{
    [Test]
    public async Task Add()
    {
        var fakerMongoDatabase = Substitute.For<IMongoDatabase>();
        var fakerSnowflake = Substitute.For<ISnowflakeWork>();

        var gameProviderId = Guid.NewGuid();
        var gameProviderCode = "KY";
        var brandId = Guid.NewGuid();
        var brandCode = "GPI";

        var brandFormId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        _ = fakerSnowflake.GetId().Returns(brandFormId);

        var repository = new BrandFormRepository(
            fakerMongoDatabase,
            fakerSnowflake);

        _ = await repository
            .AddAsync(
                gameProviderId: gameProviderId,
                gameProviderCode: gameProviderCode,
                brandId: brandId,
                brandCode: brandCode)
            .ConfigureAwait(false);

        var document = new BrandFormModel()
        {
            Id = brandFormId,
            BrandId = brandId,
            BrandCode = brandCode,
            GameProviderId = gameProviderId,
            GameProviderCode = gameProviderCode,
            State = BrandFormState.Queued,
            Status = $"{BrandFormState.Queued}...",
            CreatedOn = DateTime.UtcNow,
            UpdateOn = DateTime.UtcNow,
            ExpireOn = null
        };

        _ = fakerMongoDatabase.Received(1)
              .GetCollection<BrandFormModel>(Arg.Is("brand_forms"))
              .InsertOneAsync(Arg.Is(document));
    }

    [Test]
    public async Task UpdateComplete()
    {
        var fakerMongoDatabase = Substitute.For<IMongoDatabase>();
        var fakerSnowflake = Substitute.For<ISnowflakeWork>();

        var repository = new BrandFormRepository(
            fakerMongoDatabase,
            fakerSnowflake);

        var id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var status = "success";
        await repository
           .UpdateCompleteAsync(
               id: id,
               status: status)
           .ConfigureAwait(false);

        var builder = Builders<BrandFormModel>.Filter;
        var filter = builder.Eq(x => x.Id, id);
        var update = Builders<BrandFormModel>.Update
            .Set(s => s.State, BrandFormState.Complete)
            .Set(s => s.Status, status)
            .Set(s => s.EndOn, DateTime.UtcNow)
            .Set(s => s.UpdateOn, DateTime.UtcNow)
            .Set(s => s.ExpireOn, DateTime.UtcNow.AddDays(7));

        _ = fakerMongoDatabase.Received(1)
            .GetCollection<BrandFormModel>(Arg.Is("brand_forms"))
            .UpdateOneAsync(
                 filter: Arg.Is(filter),
                update: Arg.Is(update))
            .ConfigureAwait(false);
    }

    [Test]
    public async Task UpdateFailedAsync()
    {
        var fakerMongoDatabase = Substitute.For<IMongoDatabase>();
        var fakerSnowflake = Substitute.For<ISnowflakeWork>();

        var repository = new BrandFormRepository(
            fakerMongoDatabase,
            fakerSnowflake);

        var id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var status = "fail";
        await repository
           .UpdateCompleteAsync(
               id: id,
               status: status)
           .ConfigureAwait(false);

        var builder = Builders<BrandFormModel>.Filter;
        var filter = builder.Eq(x => x.Id, id);
        var update = Builders<BrandFormModel>.Update
            .Set(s => s.State, BrandFormState.Failed)
            .Set(s => s.Status, status)
            .Set(s => s.EndOn, DateTime.UtcNow)
            .Set(s => s.UpdateOn, DateTime.UtcNow)
            .Set(s => s.ExpireOn, DateTime.UtcNow.AddDays(7));

        _ = fakerMongoDatabase.Received(1)
            .GetCollection<BrandFormModel>(Arg.Is("brand_forms"))
            .UpdateOneAsync(
                 filter: Arg.Is(filter),
                update: Arg.Is(update))
            .ConfigureAwait(false);
    }

    [Test]
    public async Task UpdateProcessing()
    {
        var fakerMongoDatabase = Substitute.For<IMongoDatabase>();
        var fakerSnowflake = Substitute.For<ISnowflakeWork>();

        var id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var repository = new BrandFormRepository(
            fakerMongoDatabase,
            fakerSnowflake);

        await repository
           .UpdateProcessingAsync(
               id: id)
           .ConfigureAwait(false);

        var builder = Builders<BrandFormModel>.Filter;
        var filter = builder.Eq(x => x.Id, id);
        var update = Builders<BrandFormModel>.Update
            .Set(s => s.State, BrandFormState.Processing)
            .Set(s => s.Status, $"{BrandFormState.Processing}...")
            .Set(s => s.StartOn, DateTime.UtcNow)
            .Set(s => s.UpdateOn, DateTime.UtcNow);

        _ = fakerMongoDatabase.Received(1)
            .GetCollection<BrandFormModel>(Arg.Is("brand_forms"))
            .UpdateOneAsync(
                 filter: Arg.Is(filter),
                update: Arg.Is(update))
            .ConfigureAwait(false);
    }
}
