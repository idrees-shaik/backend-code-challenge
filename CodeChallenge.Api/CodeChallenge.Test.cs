using CodeChallenge.Api.Logic;
using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;
using Moq;
using Xunit;

namespace CodeChallenge.Tests;

public class MessageLogicTests
{
    private readonly Mock<IMessageRepository> _repositoryMock;
    private readonly Mock<ILogger<MessageLogic>> _loggerMock;
    private readonly MessageLogic _logic;

    public MessageLogicTests()
    {
        _repositoryMock = new Mock<IMessageRepository>();
        _loggerMock = new Mock<ILogger<MessageLogic>>();
        _logic = new MessageLogic(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateMessage_ShouldReturnCreated_WhenValid()
    {
        var request = new CreateMessageRequest { Title = "New Message", Content = "Valid content for testing." };

        _repositoryMock.Setup(r => r.GetByTitleAsync(It.IsAny<Guid>(), request.Title))
                       .ReturnsAsync((Message?)null);

        // This must match the repository signature
        var createdMessage = new Message { Id = Guid.NewGuid(), Title = request.Title, Content = request.Content };
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Message>())).ReturnsAsync(createdMessage);

        var result = await _logic.CreateMessageAsync(Guid.NewGuid(), request);

        Assert.IsType<Created<Message>>(result);
    }

    [Fact]
    public async Task CreateMessage_ShouldReturnConflict_WhenDuplicateTitle()
    {
        var request = new CreateMessageRequest { Title = "Duplicate", Content = "Valid content" };
        _repositoryMock.Setup(r => r.GetByTitleAsync(It.IsAny<Guid>(), request.Title))
                       .ReturnsAsync(new Message { Id = Guid.NewGuid(), Title = "Duplicate" });

        var result = await _logic.CreateMessageAsync(Guid.NewGuid(), request);

        Assert.IsType<Conflict>(result);
    }

    [Fact]
    public async Task CreateMessage_ShouldReturnValidationError_WhenContentTooShort()
    {
        var request = new CreateMessageRequest { Title = "Test", Content = "Short" };

        var result = await _logic.CreateMessageAsync(Guid.NewGuid(), request);

        Assert.IsType<ValidationError>(result);
    }

    [Fact]
    public async Task UpdateMessage_ShouldReturnNotFound_WhenMessageDoesNotExist()
    {
        var request = new UpdateMessageRequest { Title = "Updated", Content = "Updated content" };
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync((Message?)null);

        var result = await _logic.UpdateMessageAsync(Guid.NewGuid(), Guid.NewGuid(), request);

        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task UpdateMessage_ShouldReturnConflict_WhenMessageInactive()
    {
        var existing = new Message { Id = Guid.NewGuid(), IsActive = false };
        var request = new UpdateMessageRequest { Title = "Updated", Content = "Updated content" };
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(existing);

        var result = await _logic.UpdateMessageAsync(Guid.NewGuid(), existing.Id, request);

        Assert.IsType<Conflict>(result);
    }

    [Fact]
    public async Task DeleteMessage_ShouldReturnNotFound_WhenMessageDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync((Message?)null);

        var result = await _logic.DeleteMessageAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.IsType<NotFound>(result);
    }
}