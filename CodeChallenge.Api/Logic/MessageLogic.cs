using CodeChallenge.Api.Logic;
using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;

public class MessageLogic : IMessageLogic
{
    private readonly IMessageRepository _repository;
    private readonly ILogger<MessageLogic> _logger;

    public MessageLogic(
        IMessageRepository repository,
        ILogger<MessageLogic> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result> CreateMessageAsync(Guid organizationId, CreateMessageRequest request)
    {
        _logger.LogInformation("Creating message for OrganizationId: {OrganizationId}", organizationId);

        var errors = new Dictionary<string, string[]>();

        // Title validation
        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length < 3 || request.Title.Length > 200)
            errors.Add("Title", new[] { "Title is required and must be between 3 and 200 characters." });

        // Content validation
        if (string.IsNullOrWhiteSpace(request.Content) || request.Content.Length < 10 || request.Content.Length > 1000)
            errors.Add("Content", new[] { "Content must be between 10 and 1000 characters." });

        // Check title uniqueness
        var existing = await _repository.GetByTitleAsync(organizationId, request.Title);
        if (existing != null)
            errors.Add("Title", new[] { "Title must be unique within the organization." });

        if (errors.Any())
        {
            _logger.LogWarning("Message creation validation failed. OrgId: {OrganizationId}", organizationId);
            return new ValidationError(errors);
        }

        var message = new Message
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Title = request.Title,
            Content = request.Content,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.CreateAsync(message);

        _logger.LogInformation("Message created successfully. MessageId: {MessageId}, OrgId: {OrganizationId}",
            message.Id, organizationId);

        return new Created<Message>(message);
    }

    public async Task<Result> UpdateMessageAsync(Guid organizationId, Guid id, UpdateMessageRequest request)
    {
        _logger.LogInformation("Updating message. MessageId: {MessageId}, OrgId: {OrganizationId}", id, organizationId);

        var existing = await _repository.GetByIdAsync(organizationId, id);

        if (existing == null)
        {
            _logger.LogWarning("Update failed - Message not found. MessageId: {MessageId}, OrgId: {OrganizationId}", id, organizationId);
            return new NotFound("Message not found");
        }

        if (!existing.IsActive)
        {
            _logger.LogWarning("Update failed - Message inactive. MessageId: {MessageId}, OrgId: {OrganizationId}", id, organizationId);
            return new Conflict("Cannot update an inactive message.");
        }

        var errors = new Dictionary<string, string[]>();

        // Title validation
        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            if (request.Title.Length < 3 || request.Title.Length > 200)
                errors.Add("Title", new[] { "Title must be between 3 and 200 characters." });

            var duplicate = await _repository.GetByTitleAsync(organizationId, request.Title);
            if (duplicate != null && duplicate.Id != id)
                errors.Add("Title", new[] { "Title must be unique within the organization." });
        }

        // Content validation
        if (!string.IsNullOrWhiteSpace(request.Content))
        {
            if (request.Content.Length < 10 || request.Content.Length > 1000)
                errors.Add("Content", new[] { "Content must be between 10 and 1000 characters." });
        }

        if (errors.Any())
        {
            _logger.LogWarning("Message update validation failed. MessageId: {MessageId}, OrgId: {OrganizationId}", id, organizationId);
            return new ValidationError(errors);
        }

        // Apply updates
        existing.Title = request.Title ?? existing.Title;
        existing.Content = request.Content ?? existing.Content;
        existing.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(existing);

        _logger.LogInformation("Message updated successfully. MessageId: {MessageId}, OrgId: {OrganizationId}", id, organizationId);

        return new Updated();
    }

    public async Task<Result> DeleteMessageAsync(Guid organizationId, Guid id)
    {
        _logger.LogInformation("Deleting message. MessageId: {MessageId}, OrgId: {OrganizationId}", id, organizationId);

        var existing = await _repository.GetByIdAsync(organizationId, id);

        if (existing == null)
        {
            _logger.LogWarning("Delete failed - Message not found. MessageId: {MessageId}, OrgId: {OrganizationId}", id, organizationId);
            return new NotFound("Message not found");
        }

        if (!existing.IsActive)
        {
            _logger.LogWarning("Delete failed - Message inactive. MessageId: {MessageId}, OrgId: {OrganizationId}", id, organizationId);
            return new Conflict("Cannot delete an inactive message.");
        }

        await _repository.DeleteAsync(organizationId, id);

        _logger.LogInformation("Message deleted successfully. MessageId: {MessageId}, OrgId: {OrganizationId}", id, organizationId);

        return new Deleted();
    }

    public async Task<Message?> GetMessageAsync(Guid organizationId, Guid id)
    {
        return await _repository.GetByIdAsync(organizationId, id);
    }

    public async Task<IEnumerable<Message>> GetAllMessagesAsync(Guid organizationId)
    {
        return await _repository.GetAllByOrganizationAsync(organizationId);
    }
}