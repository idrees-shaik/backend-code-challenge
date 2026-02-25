using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CodeChallenge.Api.Controllers;

[ApiController]
[Route("api/v1/organizations/{organizationId}/messages")]
public class MessagesController : ControllerBase
{
    private readonly IMessageRepository _repository;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(IMessageRepository repository, ILogger<MessagesController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    // GET: api/v1/organizations/{organizationId}/messages
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Message>>> GetAll(Guid organizationId)
    {
        var messages = await _repository.GetAllByOrganizationAsync(organizationId);
        return Ok(messages);
    }

    // GET: api/v1/organizations/{organizationId}/messages/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Message>> GetById(Guid organizationId, Guid id)
    {
        var message = await _repository.GetByIdAsync(organizationId, id);
        if (message == null)
            return NotFound();
        return Ok(message);
    }

    // POST: api/v1/organizations/{organizationId}/messages
    [HttpPost]
    public async Task<ActionResult<Message>> Create(Guid organizationId, [FromBody] CreateMessageRequest request)
    {
        if (request == null)
            return BadRequest();
        var newMessage = new Message
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Title = request.Title,
            Content = request.Content,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var created = await _repository.CreateAsync(newMessage);
        return CreatedAtAction(nameof(GetById),new { organizationId = organizationId, id = created.Id },created);
    }

    // PUT: api/v1/organizations/{organizationId}/messages/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid organizationId, Guid id, [FromBody] UpdateMessageRequest request)
    {
        var existingMessage = await _repository.GetByIdAsync(organizationId, id);
        if (existingMessage == null)
            return NotFound();
        existing.Title = request.Title;
        existing.Content = request.Content;
        existing.UpdatedAt = DateTime.UtcNow;
        var updated = await _repository.UpdateAsync(existing);
        if (updated == null)
        return NotFound();
        return Ok(updated);
    }

    // DELETE: api/v1/organizations/{organizationId}/messages/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid organizationId, Guid id)
    {
        var deletedMessage = await _repository.DeleteAsync(organizationId, id);
        if (!deletedMessage)
            return NotFound();
        return NoContent();
    }
}

